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
        public abstract ValueTask<IOperationResultEx<Stream>> OpenUpdateAsync(IVirtualFile file, CancellationToken cancellationToken = default);

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

        /// <inheritdoc/>
        public abstract ValueTask<IOperationResultEx> ClearDirectoryAsync(IVirtualDirectory directory, VirtualFileSystemClearOption option, CancellationToken cancellationToken = default);

        #region 条目比较

        /// <summary>
        /// 定位键比较器: 决定本实现的条目比较规则 (是否区分大小写等), 默认序数比较
        /// </summary>
        protected virtual StringComparer EntryKeyComparer => StringComparer.Ordinal;

        /// <summary>
        /// 取得条目的定位键: 定位键相同即表示指向同一位置 (仅限本实现的条目)
        /// <para>默认由路径段 (文件附加文件名) 拼接; 条目携带额外定位信息 (如服务器, 共享根) 的实现应重写</para>
        /// </summary>
        /// <param name="entry"></param>
        /// <exception cref="NotSupportedException">传入的条目类型不受支持</exception>
        protected virtual string GetEntryKey(IVirtualFileSystemDescriptor entry)
        {
            string[] segments = entry switch
            {
                IVirtualDirectory directory => directory.Paths,
                IVirtualFile file => [.. file.Directory.Paths, file.Name],
                _ => throw new NotSupportedException($"不支持的条目类型: {entry.GetType().FullName}"),
            };
            return string.Join('/', segments);
        }

        /// <inheritdoc/>
        public virtual bool DirectoryEquals(IVirtualDirectory left, IVirtualDirectory right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);
            return IsOwnEntry(left) && IsOwnEntry(right) && EntryKeyComparer.Equals(GetEntryKey(left), GetEntryKey(right));
        }

        /// <inheritdoc/>
        public virtual bool FileEquals(IVirtualFile left, IVirtualFile right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);
            return IsOwnEntry(left) && IsOwnEntry(right) && EntryKeyComparer.Equals(GetEntryKey(left), GetEntryKey(right));
        }

        /// <inheritdoc/>
        public virtual int GetDirectoryHashCode(IVirtualDirectory directory)
        {
            ArgumentNullException.ThrowIfNull(directory);
            return EntryKeyComparer.GetHashCode(GetEntryKey(directory));
        }

        /// <inheritdoc/>
        public virtual int GetFileHashCode(IVirtualFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            return EntryKeyComparer.GetHashCode(GetEntryKey(file));
        }

        /// <summary>
        /// 判断条目是否属于本实现 (条目的 <see cref="IVirtualFileSystemDescriptor.FileSystemType"/> 与本实现一致)
        /// </summary>
        /// <param name="entry"></param>
        protected bool IsOwnEntry(IVirtualFileSystemDescriptor entry)
        {
            return string.Equals(entry.FileSystemType, FileSystemType, StringComparison.Ordinal);
        }

        #endregion
    }
}
