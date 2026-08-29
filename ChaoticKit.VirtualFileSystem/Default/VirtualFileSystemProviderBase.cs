using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 实现抽象基类: 提供描述属性, 异常包装与路径段合并的公共逻辑
    /// </summary>
    public abstract class VirtualFileSystemProviderBase : IVirtualFileSystemProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystemType">文件系统类型</param>
        /// <param name="source">来源描述</param>
        protected VirtualFileSystemProviderBase(string fileSystemType, string source)
        {
            FileSystemType = fileSystemType;
            Source = source;
        }

        /// <inheritdoc/>
        public string FileSystemType { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <summary>
        /// 合并路径段: 相对源条目的 <see cref="IVirtualDirectory.Paths"/> + 追加路径段, 并规范化
        /// </summary>
        /// <param name="relativeSource"></param>
        /// <param name="segments"></param>
        protected static string[] CombineSegments(IVirtualDirectory? relativeSource, IEnumerable<string?> segments)
        {
            List<string?> list = [];
            if (relativeSource != null)
            {
                list.AddRange(relativeSource.Paths);
            }
            list.AddRange(segments);
            return VirtualPathHelper.NormalizeSegments(list);
        }

        /// <summary>
        /// 异步执行并包装异常为失败结果 (不向外抛异常)
        /// </summary>
        /// <param name="body"></param>
        protected static async ValueTask<IOperationResultEx> RunAsync(Func<Task> body)
        {
            try
            {
                await body();
                return OperationResultEx.Success;
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }

        /// <summary>
        /// 异步执行并包装异常为失败结果 (不向外抛异常), 带数据版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        protected static async ValueTask<IOperationResultEx<T>> RunAsync<T>(Func<Task<T>> body)
        {
            try
            {
                T data = await body();
                return OperationResultEx<T>.Success(data);
            }
            catch (Exception ex)
            {
                return OperationResultEx<T>.Failure(ex);
            }
        }

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<bool>> DirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx> DeleteDirectoryAsync(IVirtualDirectory directory, bool recursive = false, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<IVirtualFile[]>> ListFilesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);
    }
}
