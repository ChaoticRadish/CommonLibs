using ChaoticKit.Data.Struct;
using ChaoticKit.Interfaces.IO;
using ChaoticKit.IO;
using ChaoticKit.VirtualFileSystem.Default;
using FluentFTP;

namespace ChaoticKit.VirtualFileSystem.FluentFTP
{
    /// <summary>
    /// FTP 文件系统实现: 基于 FluentFTP 实现, 每次操作创建并释放连接
    /// <para>实现无状态, 连接信息全部由条目 (<see cref="FtpDirectory"/> / <see cref="FtpFile"/> 实现的 <see cref="IFtpConnectionInfo"/>) 承载, 操作时从条目获取连接信息创建客户端</para>
    /// <para>读写使用临时文件承载数据, 避免大文件占用内存; 未提供临时文件管理器时随实例创建 <see cref="TempFileManagerOfHelper"/>, 释放实现时一并释放</para>
    /// </summary>
    public class FtpFileSystem : VirtualFileSystemProviderBase, IDisposable
    {
        private readonly ITempFileManager _tempFileManager;
        private readonly bool _disposeTempFileManager;
        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempFileManager">临时文件管理器, 为 <see langword="null"/> 时随实例创建 <see cref="TempFileManagerOfHelper"/></param>
        public FtpFileSystem(ITempFileManager? tempFileManager = null)
            : base(FileSystemTypeConstants.Ftp, typeof(FtpFileSystem).FullName ?? nameof(FtpFileSystem))
        {
            _disposeTempFileManager = tempFileManager == null;
            _tempFileManager = tempFileManager ?? new TempFileManagerOfHelper();
        }

        /// <summary>
        /// 释放随本实例创建的临时文件管理器 (外部传入的管理器不负责释放)
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_disposeTempFileManager)
                {
                    _tempFileManager.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 从条目获取 FTP 连接信息
        /// </summary>
        /// <param name="descriptor">文件或目录条目</param>
        private static IFtpConnectionInfo GetConnectionInfo(IVirtualFileSystemDescriptor descriptor)
        {
            if (descriptor is IFtpConnectionInfo info)
            {
                return info;
            }
            throw new ArgumentException($"条目未提供 FTP 连接信息: {descriptor.Source}");
        }

        /// <summary>
        /// 创建 FTP 客户端 (连接信息全部来自条目)
        /// </summary>
        /// <param name="descriptor">文件或目录条目</param>
        private AsyncFtpClient CreateClient(IVirtualFileSystemDescriptor descriptor)
        {
            var info = GetConnectionInfo(descriptor);
            return new AsyncFtpClient(info.Host, info.UserName, info.Password, info.Port);
        }

        /// <summary>
        /// 将目录条目 (及可选文件名) 解析为 FTP 远程路径
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        private string ResolveRemotePath(IVirtualDirectory directory, string? fileName = null)
        {
            var info = GetConnectionInfo(directory);
            List<string> segments = [.. directory.Paths];
            if (fileName != null)
            {
                segments.Add(fileName);
            }
            segments = [.. VirtualPathHelper.NormalizeSegments(segments)];

            string remote = "/";
            if (!string.IsNullOrEmpty(info.RootPath))
            {
                remote += info.RootPath.Trim('/');
            }
            if (segments.Count > 0)
            {
                remote += "/" + string.Join('/', segments);
            }
            return remote;
        }

        /// <summary>
        /// 由根路径取得根目录名
        /// </summary>
        /// <param name="rootPath"></param>
        private static string GetRootName(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath) || rootPath.Trim('/').Length == 0)
            {
                return "/";
            }
            return rootPath.Trim('/').Split('/')[^1];
        }

        /// <summary>
        /// 建立连接并在结束时断开
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="action"></param>
        private static async Task ConnectAndUseAsync(AsyncFtpClient client, CancellationToken cancellationToken, Func<AsyncFtpClient, Task> action)
        {
            await client.Connect(cancellationToken);
            try
            {
                await action(client);
            }
            finally
            {
                await client.Disconnect();
            }
        }

        /// <summary>
        /// 建立连接并在结束时断开 (带返回值)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="action"></param>
        private static async Task<T> ConnectAndUseAsync<T>(AsyncFtpClient client, CancellationToken cancellationToken, Func<AsyncFtpClient, Task<T>> action)
        {
            await client.Connect(cancellationToken);
            try
            {
                return await action(client);
            }
            finally
            {
                await client.Disconnect();
            }
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                return ValueTask.FromResult<IOperationResultEx<IVirtualFile>>(OperationResultEx<IVirtualFile>.Failure("未提供相对源, 无法确定 FTP 连接信息"));
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
                FtpDirectory directory = CreateDirectoryEntry(dirSegments, info);
                return Task.FromResult<IVirtualFile>(new FtpFile(name, directory, Source));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                return ValueTask.FromResult<IOperationResultEx<IVirtualDirectory>>(OperationResultEx<IVirtualDirectory>.Failure("未提供相对源, 无法确定 FTP 连接信息"));
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
        private FtpDirectory CreateDirectoryEntry(string[] segments, IFtpConnectionInfo info)
        {
            if (segments.Length == 0)
            {
                return new FtpDirectory(GetRootName(info.RootPath), [], Source, info.Host, info.Port, info.UserName, info.Password, info.RootPath);
            }
            return new FtpDirectory(segments[^1], segments, Source, info.Host, info.Port, info.UserName, info.Password, info.RootPath);
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(async () =>
            {
                string remote = ResolveRemotePath(file.Directory, file.Name);
                var tempFile = _tempFileManager.NewOne();
                try
                {
                    // 下载到临时文件, 下载完成后即可关闭连接
                    using var client = CreateClient(file);
                    await ConnectAndUseAsync(client, cancellationToken, async c =>
                    {
                        using var writeStream = tempFile.OpenWrite();
                        bool success = await c.DownloadStream(writeStream, remote, 0, null, cancellationToken);
                        if (!success)
                        {
                            throw new IOException($"FTP 下载失败: {remote}");
                        }
                    });
                    return new TempFileReadStream(tempFile);
                }
                catch
                {
                    tempFile.Dispose();
                    throw;
                }
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            return RunAsync<Stream>(async () =>
            {
                string remote = ResolveRemotePath(file.Directory, file.Name);
                FtpRemoteExists existsMode = overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip;
                var tempFile = _tempFileManager.NewOne();
                try
                {
                    // 若远程文件存在, 先下载到临时文件 (供读取原内容); 随后通过提交操作上传
                    using var client = CreateClient(file);
                    await ConnectAndUseAsync(client, cancellationToken, async c =>
                    {
                        if (await c.FileExists(remote, cancellationToken))
                        {
                            using var writeStream = tempFile.OpenWrite();
                            bool success = await c.DownloadStream(writeStream, remote, 0, null, cancellationToken);
                            if (!success)
                            {
                                throw new IOException($"FTP 下载失败: {remote}");
                            }
                        }
                    });

                    return new TempFileWriteStream(tempFile, async () =>
                    {
                        using var uploadClient = CreateClient(file);
                        await ConnectAndUseAsync(uploadClient, CancellationToken.None, async c =>
                        {
                            using var readStream = tempFile.OpenRead();
                            await c.UploadStream(readStream, remote, existsMode, true, null, CancellationToken.None);
                        });
                    });
                }
                catch
                {
                    tempFile.Dispose();
                    throw;
                }
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync<bool>(async () =>
            {
                string remote = ResolveRemotePath(file.Directory, file.Name);
                using var client = CreateClient(file);
                return await ConnectAndUseAsync(client, cancellationToken, c => c.FileExists(remote, cancellationToken));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunAsync(async () =>
            {
                string remote = ResolveRemotePath(file.Directory, file.Name);
                using var client = CreateClient(file);
                await ConnectAndUseAsync(client, cancellationToken, async c =>
                {
                    if (await c.FileExists(remote, cancellationToken))
                    {
                        await c.DeleteFile(remote, cancellationToken);
                    }
                });
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<bool>> DirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<bool>(async () =>
            {
                string remote = ResolveRemotePath(directory);
                using var client = CreateClient(directory);
                return await ConnectAndUseAsync(client, cancellationToken, c => c.DirectoryExists(remote, cancellationToken));
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync(async () =>
            {
                string remote = ResolveRemotePath(directory);
                using var client = CreateClient(directory);
                await ConnectAndUseAsync(client, cancellationToken, async c =>
                {
                    if (!await c.DirectoryExists(remote, cancellationToken))
                    {
                        await c.CreateDirectory(remote, cancellationToken);
                    }
                });
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx> DeleteDirectoryAsync(IVirtualDirectory directory, bool recursive = false, CancellationToken cancellationToken = default)
        {
            return RunAsync(async () =>
            {
                string remote = ResolveRemotePath(directory);
                using var client = CreateClient(directory);
                await ConnectAndUseAsync(client, cancellationToken, async c =>
                {
                    if (await c.DirectoryExists(remote, cancellationToken))
                    {
                        // FluentFTP 的 DeleteDirectory 会递归删除整个目录树
                        await c.DeleteDirectory(remote, cancellationToken);
                    }
                });
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualFile[]>> ListFilesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualFile[]>(async () =>
            {
                string remote = ResolveRemotePath(directory);
                using var client = CreateClient(directory);
                var items = await ConnectAndUseAsync(client, cancellationToken, c => c.GetListing(remote, cancellationToken));
                var ftpDirectory = (FtpDirectory)directory;
                return items
                    .Where(i => i.Type == FtpObjectType.File)
                    .Select(i => (IVirtualFile)new FtpFile(i.Name, ftpDirectory, Source))
                    .ToArray();
            });
        }

        /// <inheritdoc/>
        public override ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunAsync<IVirtualDirectory[]>(async () =>
            {
                string remote = ResolveRemotePath(directory);
                using var client = CreateClient(directory);
                var items = await ConnectAndUseAsync(client, cancellationToken, c => c.GetListing(remote, cancellationToken));
                var ftpDirectory = (FtpDirectory)directory;
                return items
                    .Where(i => i.Type == FtpObjectType.Directory)
                    .Select(i =>
                    {
                        string[] paths = [.. directory.Paths, i.Name];
                        return (IVirtualDirectory)new FtpDirectory(i.Name, paths, Source, ftpDirectory.Host, ftpDirectory.Port, ftpDirectory.UserName, ftpDirectory.Password, ftpDirectory.RootPath);
                    })
                    .ToArray();
            });
        }
    }
}
