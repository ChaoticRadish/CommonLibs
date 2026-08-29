using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem.Default;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// Windows 共享目录文件系统实现
    /// <para>实现无状态, 连接信息全部由条目 (<see cref="WindowsShareDirectory"/> / <see cref="WindowsShareFile"/> 实现的 <see cref="IWindowsShareConnectionInfo"/>) 承载;
    /// 实现内部区分运行时系统: Windows 上有凭据时通过 P/Invoke 挂载凭据连接, 无凭据时直接以系统当前凭据访问 UNC 路径; 非 Windows 系统返回失败结果</para>
    /// </summary>
    public class WindowsShareFileSystem : VirtualFileSystemProviderBase
    {
        /// <summary>
        /// CONNECT_TEMPORARY: 临时连接, 不持久化
        /// </summary>
        private const int CONNECT_TEMPORARY = 0x4;

        /// <summary>
        /// 
        /// </summary>
        public WindowsShareFileSystem()
            : base(FileSystemTypeConstants.WindowsShare, typeof(WindowsShareFileSystem).FullName ?? nameof(WindowsShareFileSystem))
        {
        }

        /// <summary>
        /// 从条目获取共享目录连接信息
        /// </summary>
        /// <param name="descriptor">文件或目录条目</param>
        private static IWindowsShareConnectionInfo GetConnectionInfo(IVirtualFileSystemDescriptor descriptor)
        {
            if (descriptor is IWindowsShareConnectionInfo info)
            {
                return info;
            }
            throw new ArgumentException($"条目未提供 Windows 共享目录连接信息: {descriptor.Source}");
        }

        /// <summary>
        /// 将目录条目 (及可选文件名) 解析为本地实际路径
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        private string ResolveFullPath(IVirtualDirectory directory, string? fileName = null)
        {
            var info = GetConnectionInfo(directory);
            List<string> segments = [.. directory.Paths];
            if (fileName != null)
            {
                segments.Add(fileName);
            }
            segments = [.. VirtualPathHelper.NormalizeSegments(segments)];
            return Path.Combine([info.ShareRootPath, .. segments]);
        }

        /// <summary>
        /// 尝试建立凭据连接 (条目提供凭据时), 返回用于断开的 <see cref="IDisposable"/>; 无凭据时返回 <see langword="null"/>
        /// <para>非 Windows 系统抛出 <see cref="PlatformNotSupportedException"/>, 由基类包装为失败结果</para>
        /// </summary>
        /// <param name="descriptor">文件或目录条目</param>
        private IDisposable? TryConnect(IVirtualFileSystemDescriptor descriptor)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Windows 共享目录访问仅支持 Windows 系统");
            }
            var info = GetConnectionInfo(descriptor);
            if (info.Credential == null)
            {
                return null;
            }

            NETRESOURCE netResource = new()
            {
                dwType = 1, // RESOURCETYPE_DISK
                lpRemoteName = info.ShareRootPath,
            };
            string user = string.IsNullOrEmpty(info.Credential.Domain)
                ? info.Credential.UserName
                : info.Credential.Domain + "\\" + info.Credential.UserName;

            int result = WNetUseConnection(
                IntPtr.Zero,
                ref netResource,
                info.Credential.Password,
                user,
                CONNECT_TEMPORARY,
                null,
                ref _bufferSize,
                ref _result);

            if (result != 0)
            {
                throw new Win32Exception(result, $"建立共享目录连接失败: {info.ShareRootPath}");
            }

            return new DisconnectAction(() =>
            {
                WNetCancelConnection2(info.ShareRootPath, 0, true);
            });
        }

        private int _bufferSize;
        private int _result;

        /// <summary>
        /// 由根路径取得根目录名
        /// </summary>
        /// <param name="shareRootPath"></param>
        private static string GetRootName(string shareRootPath)
        {
            string trimmed = shareRootPath.TrimEnd('\\', '/');
            int index = trimmed.LastIndexOfAny(['\\', '/']);
            string name = index >= 0 ? trimmed[(index + 1)..] : trimmed;
            return string.IsNullOrEmpty(name) ? shareRootPath : name;
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                return ValueTask.FromResult<IOperationResultEx<IVirtualFile>>(OperationResultEx<IVirtualFile>.Failure("未提供相对源, 无法确定 Windows 共享目录连接信息"));
            }
            return RunAsync<IVirtualFile>(() =>
            {
                var info = GetConnectionInfo(path.RelativeSource);
                string[] segments = CombineSegments(path.RelativeSource, path.PathSegments);
                if (segments.Length == 0)
                {
                    throw new ArgumentException("文件路径段不能为空");
                }
                string name = segments[^1];
                string[] dirSegments = segments[..^1];
                WindowsShareDirectory directory = CreateDirectoryEntry(dirSegments, info);
                return Task.FromResult<IVirtualFile>(new WindowsShareFile(name, directory, Source));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                return ValueTask.FromResult<IOperationResultEx<IVirtualDirectory>>(OperationResultEx<IVirtualDirectory>.Failure("未提供相对源, 无法确定 Windows 共享目录连接信息"));
            }
            return RunAsync<IVirtualDirectory>(() =>
            {
                var info = GetConnectionInfo(path.RelativeSource);
                string[] segments = CombineSegments(path.RelativeSource, path.PathSegments);
                return Task.FromResult<IVirtualDirectory>(CreateDirectoryEntry(segments, info));
            });
        }

        /// <summary>
        /// 由路径段创建目录条目 (根目录段为空时返回根条目)
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="info"></param>
        private WindowsShareDirectory CreateDirectoryEntry(string[] segments, IWindowsShareConnectionInfo info)
        {
            if (segments.Length == 0)
            {
                return new WindowsShareDirectory(GetRootName(info.ShareRootPath), [], Source, info.DeviceHost, info.ShareRootPath, info.Credential);
            }
            return new WindowsShareDirectory(segments[^1], segments, Source, info.DeviceHost, info.ShareRootPath, info.Credential);
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(() =>
            {
                using var connection = TryConnect(file);
                string full = ResolveFullPath(file.Directory, file.Name);
                return Task.FromResult<Stream>(new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(() =>
            {
                using var connection = TryConnect(file);
                string full = ResolveFullPath(file.Directory, file.Name);
                FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
                return Task.FromResult<Stream>(new FileStream(full, mode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<bool>(() =>
            {
                using var connection = TryConnect(file);
                return Task.FromResult(File.Exists(ResolveFullPath(file.Directory, file.Name)));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync(() =>
            {
                using var connection = TryConnect(file);
                string full = ResolveFullPath(file.Directory, file.Name);
                if (File.Exists(full))
                {
                    File.Delete(full);
                }
                return Task.CompletedTask;
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<bool>> DirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<bool>(() =>
            {
                using var connection = TryConnect(directory);
                return Task.FromResult(Directory.Exists(ResolveFullPath(directory)));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync(() =>
            {
                using var connection = TryConnect(directory);
                string full = ResolveFullPath(directory);
                if (!Directory.Exists(full))
                {
                    Directory.CreateDirectory(full);
                }
                return Task.CompletedTask;
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> DeleteDirectoryAsync(IVirtualDirectory directory, bool recursive = false, CancellationToken cancellationToken = default)
        {
            return RunAsync(() =>
            {
                using var connection = TryConnect(directory);
                string full = ResolveFullPath(directory);
                if (Directory.Exists(full))
                {
                    Directory.Delete(full, recursive);
                }
                return Task.CompletedTask;
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualFile[]>> ListFilesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualFile[]>(() =>
            {
                using var connection = TryConnect(directory);
                string full = ResolveFullPath(directory);
                string[] files = Directory.GetFiles(full);
                var shareDirectory = (WindowsShareDirectory)directory;
                return Task.FromResult(files.Select(f => (IVirtualFile)new WindowsShareFile(Path.GetFileName(f), shareDirectory, Source)).ToArray());
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualDirectory[]>(() =>
            {
                using var connection = TryConnect(directory);
                string full = ResolveFullPath(directory);
                string[] dirs = Directory.GetDirectories(full);
                var shareDirectory = (WindowsShareDirectory)directory;
                return Task.FromResult(dirs.Select(d =>
                {
                    string name = Path.GetFileName(d);
                    string[] paths = [.. directory.Paths, name];
                    return (IVirtualDirectory)new WindowsShareDirectory(name, paths, Source, shareDirectory.DeviceHost, shareDirectory.ShareRootPath, shareDirectory.Credential);
                }).ToArray());
            });
        }

        #region P/Invoke

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string? lpLocalName;
            public string? lpRemoteName;
            public string? lpComment;
            public string? lpProvider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            ref NETRESOURCE lpNetResource,
            string? lpPassword,
            string? lpUserID,
            int dwFlags,
            StringBuilder? lpAccessName,
            ref int lpBufferSize,
            ref int lpResult);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        #endregion
    }
}
