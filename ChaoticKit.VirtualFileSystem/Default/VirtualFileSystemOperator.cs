using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 操作器默认实现: 持有工厂引用, 实现操作按条目类型解析实现后委托, 传输操作按 (源类型, 目标类型) 解析传输实现,
    /// 统一触发 <see cref="OperationInvoking"/> / <see cref="OperationInvoked"/> 事件; 返回的流为包装流, 释放时触发 <see cref="VirtualFileSystemOperation.CloseStream"/>
    /// </summary>
    public class VirtualFileSystemOperator : IVirtualFileSystemOperator
    {
        private readonly IVirtualFileSystem _factory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public VirtualFileSystemOperator(IVirtualFileSystem factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public event EventHandler<VirtualFileSystemOperationEventArgs>? OperationInvoking;

        /// <inheritdoc/>
        public event EventHandler<VirtualFileSystemOperationEventArgs>? OperationInvoked;

        /// <inheritdoc/>
        public string FileSystemType => "Operator";

        /// <inheritdoc/>
        public string Source => typeof(VirtualFileSystemOperator).FullName ?? nameof(VirtualFileSystemOperator);

        #region 事件辅助

        private void RaiseInvoking(VirtualFileSystemOperation operation, IVirtualFile? file, IVirtualDirectory? directory)
        {
            OperationInvoking?.Invoke(this, new VirtualFileSystemOperationEventArgs
            {
                Operation = operation,
                File = file,
                Directory = directory,
            });
        }

        private void RaiseInvoked(VirtualFileSystemOperation operation, IVirtualFile? file, IVirtualDirectory? directory, IOperationResultEx result)
        {
            OperationInvoked?.Invoke(this, new VirtualFileSystemOperationEventArgs
            {
                Operation = operation,
                File = file,
                Directory = directory,
                Result = result,
            });
        }

        #endregion

        #region 实现委托辅助

        private IVirtualFileSystemProvider? Resolve(string fileSystemType)
        {
            return _factory.GetProvider(fileSystemType);
        }

        private async ValueTask<IOperationResultEx> RunProviderAsync(VirtualFileSystemOperation operation, IVirtualFile? file, IVirtualDirectory? directory, string fileSystemType, Func<IVirtualFileSystemProvider, ValueTask<IOperationResultEx>> action)
        {
            RaiseInvoking(operation, file, directory);
            var provider = Resolve(fileSystemType);
            IOperationResultEx result;
            if (provider == null)
            {
                result = OperationResultEx.Failure($"未注册实现: {fileSystemType}");
            }
            else
            {
                result = await action(provider);
            }
            RaiseInvoked(operation, file, directory, result);
            return result;
        }

        private async ValueTask<IOperationResultEx<T>> RunProviderAsync<T>(VirtualFileSystemOperation operation, IVirtualFile? file, IVirtualDirectory? directory, string fileSystemType, Func<IVirtualFileSystemProvider, ValueTask<IOperationResultEx<T>>> action)
        {
            RaiseInvoking(operation, file, directory);
            var provider = Resolve(fileSystemType);
            IOperationResultEx<T> result;
            if (provider == null)
            {
                result = OperationResultEx<T>.Failure($"未注册实现: {fileSystemType}");
            }
            else
            {
                result = await action(provider);
            }
            RaiseInvoked(operation, file, directory, result);
            return result;
        }

        private async ValueTask<IOperationResultEx> RunTransferAsync(VirtualFileSystemOperation operation, IVirtualFile? file, IVirtualDirectory? directory, Func<IVirtualFileSystemTransfer, ValueTask<IOperationResultEx>> action, string sourceType, string targetType)
        {
            RaiseInvoking(operation, file, directory);
            var transfer = _factory.GetTransfer(sourceType, targetType);
            var result = await action(transfer);
            RaiseInvoked(operation, file, directory, result);
            return result;
        }

        #endregion

        #region IVirtualFileSystemProvider

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                RaiseInvoking(VirtualFileSystemOperation.GetFile, null, null);
                var failure = OperationResultEx<IVirtualFile>.Failure("未提供相对源, 无法确定实现类型");
                RaiseInvoked(VirtualFileSystemOperation.GetFile, null, null, failure);
                return ValueTask.FromResult<IOperationResultEx<IVirtualFile>>(failure);
            }
            return RunProviderAsync(VirtualFileSystemOperation.GetFile, null, path.RelativeSource, path.RelativeSource.FileSystemType,
                provider => provider.GetFileAsync(path, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default)
        {
            if (path.RelativeSource == null)
            {
                RaiseInvoking(VirtualFileSystemOperation.GetDirectory, null, null);
                var failure = OperationResultEx<IVirtualDirectory>.Failure("未提供相对源, 无法确定实现类型");
                RaiseInvoked(VirtualFileSystemOperation.GetDirectory, null, null, failure);
                return ValueTask.FromResult<IOperationResultEx<IVirtualDirectory>>(failure);
            }
            return RunProviderAsync(VirtualFileSystemOperation.GetDirectory, null, path.RelativeSource, path.RelativeSource.FileSystemType,
                provider => provider.GetDirectoryAsync(path, cancellationToken));
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            RaiseInvoking(VirtualFileSystemOperation.OpenRead, file, null);
            var provider = Resolve(file.FileSystemType);
            IOperationResultEx<Stream> result;
            if (provider == null)
            {
                result = OperationResultEx<Stream>.Failure($"未注册实现: {file.FileSystemType}");
            }
            else
            {
                result = await provider.OpenReadAsync(file, cancellationToken);
                if (result.IsSuccess && result.Data != null)
                {
                    var inner = result.Data;
                    result = OperationResultEx<Stream>.Success(new EventStream(inner, () =>
                    {
                        RaiseInvoked(VirtualFileSystemOperation.CloseStream, file, null, OperationResultEx.Success);
                    }));
                }
            }
            RaiseInvoked(VirtualFileSystemOperation.OpenRead, file, null, result);
            return result;
        }

        /// <inheritdoc/>
        public async ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            RaiseInvoking(VirtualFileSystemOperation.OpenWrite, file, null);
            var provider = Resolve(file.FileSystemType);
            IOperationResultEx<Stream> result;
            if (provider == null)
            {
                result = OperationResultEx<Stream>.Failure($"未注册实现: {file.FileSystemType}");
            }
            else
            {
                result = await provider.OpenWriteAsync(file, overwrite, cancellationToken);
                if (result.IsSuccess && result.Data != null)
                {
                    var inner = result.Data;
                    result = OperationResultEx<Stream>.Success(new EventStream(inner, () =>
                    {
                        RaiseInvoked(VirtualFileSystemOperation.CloseStream, file, null, OperationResultEx.Success);
                    }));
                }
            }
            RaiseInvoked(VirtualFileSystemOperation.OpenWrite, file, null, result);
            return result;
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.FileExists, file, null, file.FileSystemType,
                provider => provider.FileExistsAsync(file, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.DeleteFile, file, null, file.FileSystemType,
                provider => provider.DeleteFileAsync(file, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<bool>> DirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.DirectoryExists, null, directory, directory.FileSystemType,
                provider => provider.DirectoryExistsAsync(directory, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.CreateDirectory, null, directory, directory.FileSystemType,
                provider => provider.CreateDirectoryAsync(directory, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> DeleteDirectoryAsync(IVirtualDirectory directory, bool recursive = false, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.DeleteDirectory, null, directory, directory.FileSystemType,
                provider => provider.DeleteDirectoryAsync(directory, recursive, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<IVirtualFile[]>> ListFilesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.ListFiles, null, directory, directory.FileSystemType,
                provider => provider.ListFilesAsync(directory, cancellationToken));
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default)
        {
            return RunProviderAsync(VirtualFileSystemOperation.ListDirectories, null, directory, directory.FileSystemType,
                provider => provider.ListDirectoriesAsync(directory, cancellationToken));
        }

        #endregion

        #region IVirtualFileSystemTransfer

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> MoveFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
        {
            return RunTransferAsync(VirtualFileSystemOperation.MoveFile, source, null,
                transfer => transfer.MoveFileAsync(source, target, cancellationToken), source.FileSystemType, target.FileSystemType);
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> MoveDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
        {
            return RunTransferAsync(VirtualFileSystemOperation.MoveDirectory, null, source,
                transfer => transfer.MoveDirectoryAsync(source, target, cancellationToken), source.FileSystemType, target.FileSystemType);
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> CopyFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
        {
            return RunTransferAsync(VirtualFileSystemOperation.CopyFile, source, null,
                transfer => transfer.CopyFileAsync(source, target, cancellationToken), source.FileSystemType, target.FileSystemType);
        }

        /// <inheritdoc/>
        public ValueTask<IOperationResultEx> CopyDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
        {
            return RunTransferAsync(VirtualFileSystemOperation.CopyDirectory, null, source,
                transfer => transfer.CopyDirectoryAsync(source, target, cancellationToken), source.FileSystemType, target.FileSystemType);
        }

        #endregion
    }
}
