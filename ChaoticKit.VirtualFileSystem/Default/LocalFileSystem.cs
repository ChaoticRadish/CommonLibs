using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 本机文件系统实现: 直接访问操作系统文件系统, 跨平台无环境依赖
    /// <para>无状态无配置; 路径 string[] 的生效逻辑由本实现决定: 默认从程序所在磁盘的根目录开始解析, 路径段首段为盘符 (如 "C:") 时从该盘根目录开始 (支持 Windows 下访问不同盘符)</para>
    /// </summary>
    public class LocalFileSystem : VirtualFileSystemProviderBase
    {
        /// <summary>
        /// 程序所在磁盘的根目录 (如 "D:\")
        /// </summary>
        private static readonly string DefaultRoot = Path.GetPathRoot(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException("无法确定程序所在磁盘的根目录");

        /// <summary>
        /// 
        /// </summary>
        public LocalFileSystem()
            : base(FileSystemTypeConstants.Local, typeof(LocalFileSystem).FullName ?? nameof(LocalFileSystem))
        {
        }

        /// <summary>
        /// 判断路径段是否为盘符 (如 "C:")
        /// </summary>
        /// <param name="segment"></param>
        private static bool IsDriveSegment(string segment)
        {
            return segment.Length == 2 && segment[1] == ':';
        }

        /// <summary>
        /// 将目录条目 (及可选文件名) 解析为本地实际路径
        /// <para>首段为盘符时从该盘根目录解析, 否则从程序所在磁盘的根目录解析</para>
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        private string ResolveFullPath(IVirtualDirectory directory, string? fileName = null)
        {
            List<string> segments = [.. directory.Paths];
            if (fileName != null)
            {
                segments.Add(fileName);
            }
            segments = [.. VirtualPathHelper.NormalizeSegments(segments)];

            if (segments.Count > 0 && IsDriveSegment(segments[0]))
            {
                string driveRoot = segments[0] + Path.DirectorySeparatorChar;
                return Path.Combine([driveRoot, .. segments[1..]]);
            }
            return Path.Combine([DefaultRoot, .. segments]);
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualFile>(() =>
            {
                string[] segments = CombineSegments(path.RelativeSource, path.PathSegments);
                if (segments.Length == 0)
                {
                    throw new ArgumentException("文件路径段不能为空");
                }
                string name = segments[^1];
                string[] dirSegments = segments[..^1];
                IVirtualDirectory directory = CreateDirectoryEntry(dirSegments);
                return Task.FromResult<IVirtualFile>(new LocalFile(name, directory, Source));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualDirectory>(() =>
            {
                string[] segments = CombineSegments(path.RelativeSource, path.PathSegments);
                return Task.FromResult(CreateDirectoryEntry(segments));
            });
        }

        /// <summary>
        /// 由路径段创建目录条目 (路径段为空时返回根条目)
        /// </summary>
        /// <param name="segments"></param>
        private IVirtualDirectory CreateDirectoryEntry(string[] segments)
        {
            if (segments.Length == 0)
            {
                return new LocalDirectory(GetRootName(), [], Source);
            }
            return new LocalDirectory(segments[^1], segments, Source);
        }

        /// <summary>
        /// 根目录名 (程序所在磁盘的盘符, 如 "D:")
        /// </summary>
        private string GetRootName()
        {
            return DefaultRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(() =>
            {
                string full = ResolveFullPath(file.Directory, file.Name);
                return Task.FromResult<Stream>(new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(() =>
            {
                string full = ResolveFullPath(file.Directory, file.Name);
                FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
                return Task.FromResult<Stream>(new FileStream(full, mode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<bool>(() => Task.FromResult(File.Exists(ResolveFullPath(file.Directory, file.Name))));
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync(() =>
            {
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
            return RunAsync<bool>(() => Task.FromResult(Directory.Exists(ResolveFullPath(directory))));
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync(() =>
            {
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
                string full = ResolveFullPath(directory);
                string[] files = Directory.GetFiles(full);
                return Task.FromResult(files.Select(f => (IVirtualFile)new LocalFile(Path.GetFileName(f), directory, Source)).ToArray());
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualDirectory[]>(() =>
            {
                string full = ResolveFullPath(directory);
                string[] dirs = Directory.GetDirectories(full);
                return Task.FromResult(dirs.Select(d =>
                {
                    string name = Path.GetFileName(d);
                    string[] paths = [.. directory.Paths, name];
                    return (IVirtualDirectory)new LocalDirectory(name, paths, Source);
                }).ToArray());
            });
        }
    }

    /// <summary>
    /// 本机文件系统文件条目
    /// </summary>
    public class LocalFile : IVirtualFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="directory">所属目录条目</param>
        /// <param name="source">来源描述</param>
        public LocalFile(string name, IVirtualDirectory directory, string source)
        {
            Name = name;
            Directory = directory;
            Source = source;
        }

        /// <summary>
        /// 由路径字符串创建文件条目: 相对路径按当前程序路径 (<see cref="AppContext.BaseDirectory"/>) 为基准解析为绝对路径, 再拆分为路径段
        /// </summary>
        /// <param name="path">文件路径 (相对或绝对)</param>
        /// <param name="source">来源描述</param>
        public static LocalFile FromPath(string path, string source)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("路径不能为空", nameof(path));
            }
            string full = Path.GetFullPath(path, AppContext.BaseDirectory);
            string name = Path.GetFileName(full);
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"路径未包含文件名: {path}");
            }
            string dirPath = Path.GetDirectoryName(full) ?? throw new InvalidOperationException($"无法取得文件所在目录: {full}");
            return new LocalFile(name, LocalDirectory.FromPath(dirPath, source), source);
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.Local;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IVirtualDirectory Directory { get; }

        /// <inheritdoc/>
        public string FullPath
        {
            get
            {
                string dir = Directory.FullPath;
                return string.IsNullOrEmpty(dir) ? Name : Path.Combine(dir, Name);
            }
        }
    }

    /// <summary>
    /// 本机文件系统目录条目
    /// </summary>
    public class LocalDirectory : IVirtualDirectory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">目录名</param>
        /// <param name="paths">路径段数组</param>
        /// <param name="source">来源描述</param>
        public LocalDirectory(string name, string[] paths, string source)
        {
            Name = name;
            Paths = paths;
            Source = source;
        }

        /// <summary>
        /// 由路径字符串创建目录条目: 相对路径按当前程序路径 (<see cref="AppContext.BaseDirectory"/>) 为基准解析为绝对路径, 再拆分为路径段
        /// <para>支持盘符路径 (如 "D:\data"); 不支持 UNC 路径</para>
        /// </summary>
        /// <param name="path">目录路径 (相对或绝对)</param>
        /// <param name="source">来源描述</param>
        public static LocalDirectory FromPath(string path, string source)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("路径不能为空", nameof(path));
            }
            string full = Path.GetFullPath(path, AppContext.BaseDirectory);
            string root = Path.GetPathRoot(full) ?? throw new InvalidOperationException($"无法取得路径根: {full}");
            if (root.StartsWith(@"\\", StringComparison.Ordinal))
            {
                throw new NotSupportedException("LocalDirectory 暂不支持 UNC 路径: " + full);
            }
            string relative = Path.GetRelativePath(root, full);
            string[] parts = relative.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
            string drive = root.TrimEnd('\\', '/');
            string[] paths = [drive, .. parts];
            string name = paths.Length > 0 ? paths[^1] : drive;
            return new LocalDirectory(name, paths, source);
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.Local;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string[] Paths { get; }

        /// <inheritdoc/>
        public string FullPath
        {
            get
            {
                if (Paths.Length == 0)
                {
                    return string.Empty;
                }
                // 首段为盘符 (如 "D:") 时从该盘根开始, 拼接使用环境分隔符, 方便拷贝使用
                return Path.Combine([Paths[0] + Path.DirectorySeparatorChar, .. Paths[1..]]);
            }
        }
    }
}
