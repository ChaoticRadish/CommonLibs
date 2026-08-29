using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 兜底传输实现: 将不同实现之间的文件操作串联起来
    /// <para>拷贝 = 源实现 <see cref="IVirtualFileSystemProvider.OpenReadAsync"/> 获取全部数据 → 目标实现 <see cref="IVirtualFileSystemProvider.OpenWriteAsync"/> 流式写入; 移动 = 拷贝 + 删除源</para>
    /// </summary>
    public class FallbackTransfer : IVirtualFileSystemTransfer
    {
        private readonly Func<string, IVirtualFileSystemProvider?> _providerResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerResolver">按文件系统类型解析实现实例的函数</param>
        public FallbackTransfer(Func<string, IVirtualFileSystemProvider?> providerResolver)
        {
            _providerResolver = providerResolver;
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx> MoveFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
        {
            var copyResult = await CopyFileAsync(source, target, cancellationToken);
            if (copyResult.IsFailure)
            {
                return copyResult;
            }
            var provider = Resolve(source.FileSystemType);
            if (provider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {source.FileSystemType}");
            }
            return await provider.DeleteFileAsync(source, cancellationToken);
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx> MoveDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
        {
            var copyResult = await CopyDirectoryAsync(source, target, cancellationToken);
            if (copyResult.IsFailure)
            {
                return copyResult;
            }
            var provider = Resolve(source.FileSystemType);
            if (provider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {source.FileSystemType}");
            }
            return await provider.DeleteDirectoryAsync(source, true, cancellationToken);
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx> CopyFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
        {
            var sourceProvider = Resolve(source.FileSystemType);
            var targetProvider = Resolve(target.FileSystemType);
            if (sourceProvider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {source.FileSystemType}");
            }
            if (targetProvider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {target.FileSystemType}");
            }

            var readResult = await sourceProvider.OpenReadAsync(source, cancellationToken);
            if (readResult.IsFailure || readResult.Data == null)
            {
                return OperationResultEx.Failure(readResult, "打开源文件读取流失败");
            }
            using var readStream = readResult.Data;

            var writeResult = await targetProvider.OpenWriteAsync(target, true, cancellationToken);
            if (writeResult.IsFailure || writeResult.Data == null)
            {
                return OperationResultEx.Failure(writeResult, "打开目标文件写入流失败");
            }
            using var writeStream = writeResult.Data;

            try
            {
                await readStream.CopyToAsync(writeStream, cancellationToken);
                return OperationResultEx.Success;
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx> CopyDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
        {
            var sourceProvider = Resolve(source.FileSystemType);
            var targetProvider = Resolve(target.FileSystemType);
            if (sourceProvider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {source.FileSystemType}");
            }
            if (targetProvider == null)
            {
                return OperationResultEx.Failure($"未注册实现: {target.FileSystemType}");
            }

            var createResult = await targetProvider.CreateDirectoryAsync(target, cancellationToken);
            if (createResult.IsFailure)
            {
                return createResult;
            }

            var filesResult = await sourceProvider.ListFilesAsync(source, cancellationToken);
            if (filesResult.IsFailure || filesResult.Data == null)
            {
                return OperationResultEx.Failure(filesResult, "枚举源目录文件失败");
            }
            foreach (var file in filesResult.Data)
            {
                var targetFileResult = await targetProvider.GetFileAsync(new VirtualFilePath(target, [file.Name]), cancellationToken);
                if (targetFileResult.IsFailure || targetFileResult.Data == null)
                {
                    return OperationResultEx.Failure(targetFileResult, $"获取目标文件条目失败: {file.Name}");
                }
                var copyResult = await CopyFileAsync(file, targetFileResult.Data, cancellationToken);
                if (copyResult.IsFailure)
                {
                    return copyResult;
                }
            }

            var dirsResult = await sourceProvider.ListDirectoriesAsync(source, cancellationToken);
            if (dirsResult.IsFailure || dirsResult.Data == null)
            {
                return OperationResultEx.Failure(dirsResult, "枚举源目录子目录失败");
            }
            foreach (var dir in dirsResult.Data)
            {
                var targetDirResult = await targetProvider.GetDirectoryAsync(new VirtualFilePath(target, [dir.Name]), cancellationToken);
                if (targetDirResult.IsFailure || targetDirResult.Data == null)
                {
                    return OperationResultEx.Failure(targetDirResult, $"获取目标目录条目失败: {dir.Name}");
                }
                var copyResult = await CopyDirectoryAsync(dir, targetDirResult.Data, cancellationToken);
                if (copyResult.IsFailure)
                {
                    return copyResult;
                }
            }

            return OperationResultEx.Success;
        }

        /// <summary>
        /// 解析实现实例
        /// </summary>
        /// <param name="fileSystemType"></param>
        private IVirtualFileSystemProvider? Resolve(string fileSystemType)
        {
            return _providerResolver(fileSystemType);
        }
    }
}
