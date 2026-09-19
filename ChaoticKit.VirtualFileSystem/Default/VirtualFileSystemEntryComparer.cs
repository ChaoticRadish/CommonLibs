namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 条目相等比较服务默认实现: 通过文件系统工厂 (<see cref="IVirtualFileSystem"/>) 解析条目所属实现完成比较
    /// <para>跨文件系统必定不相等; 同一文件系统交由对应实现比较; 文件系统未注册实现时视为不相等 (哈希码为 0)</para>
    /// </summary>
    public class VirtualFileSystemEntryComparer : IVirtualFileSystemEntryComparer
    {
        private readonly IVirtualFileSystem fileSystem;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem">文件系统工厂: 按条目的 <see cref="IVirtualFileSystemDescriptor.FileSystemType"/> 解析实现</param>
        public VirtualFileSystemEntryComparer(IVirtualFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            DirectoryComparer = new DirectoryEntryComparer(this);
            FileComparer = new FileEntryComparer(this);
        }

        /// <inheritdoc/>
        public IEqualityComparer<IVirtualDirectory> DirectoryComparer { get; }

        /// <inheritdoc/>
        public IEqualityComparer<IVirtualFile> FileComparer { get; }

        /// <inheritdoc/>
        public bool DirectoryEquals(IVirtualDirectory? left, IVirtualDirectory? right)
        {
            return EntryEquals(left, right, static (provider, leftOne, rightOne) => provider.DirectoryEquals(leftOne, rightOne));
        }

        /// <inheritdoc/>
        public bool FileEquals(IVirtualFile? left, IVirtualFile? right)
        {
            return EntryEquals(left, right, static (provider, leftOne, rightOne) => provider.FileEquals(leftOne, rightOne));
        }

        /// <summary>
        /// 条目相等比较: 跨文件系统必定不相等, 同一文件系统交由对应实现比较, 未注册实现时视为不相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="compare">取得实现后的比较方法</param>
        private bool EntryEquals<T>(T? left, T? right, Func<IVirtualFileSystemProvider, T, T, bool> compare)
            where T : class, IVirtualFileSystemDescriptor
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            if (!string.Equals(left.FileSystemType, right.FileSystemType, StringComparison.Ordinal)) return false;

            var provider = fileSystem.GetProvider(left.FileSystemType);
            return provider != null && compare(provider, left, right);
        }

        /// <summary>
        /// 取得条目哈希码: 由文件系统类型与对应实现计算的哈希码组合而成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <param name="getHashCode">取得实现后的哈希码计算方法</param>
        private int GetEntryHashCode<T>(T entry, Func<IVirtualFileSystemProvider, T, int> getHashCode)
            where T : class, IVirtualFileSystemDescriptor
        {
            var provider = fileSystem.GetProvider(entry.FileSystemType);
            return provider == null ? 0 : HashCode.Combine(entry.FileSystemType, getHashCode(provider, entry));
        }

        /// <summary>
        /// 目录条目的相等比较器
        /// </summary>
        /// <param name="owner">所属比较服务</param>
        private sealed class DirectoryEntryComparer(VirtualFileSystemEntryComparer owner) : IEqualityComparer<IVirtualDirectory>
        {
            /// <inheritdoc/>
            public bool Equals(IVirtualDirectory? x, IVirtualDirectory? y)
            {
                return owner.DirectoryEquals(x, y);
            }

            /// <inheritdoc/>
            public int GetHashCode(IVirtualDirectory obj)
            {
                ArgumentNullException.ThrowIfNull(obj);
                return owner.GetEntryHashCode(obj, static (provider, entry) => provider.GetDirectoryHashCode(entry));
            }
        }

        /// <summary>
        /// 文件条目的相等比较器
        /// </summary>
        /// <param name="owner">所属比较服务</param>
        private sealed class FileEntryComparer(VirtualFileSystemEntryComparer owner) : IEqualityComparer<IVirtualFile>
        {
            /// <inheritdoc/>
            public bool Equals(IVirtualFile? x, IVirtualFile? y)
            {
                return owner.FileEquals(x, y);
            }

            /// <inheritdoc/>
            public int GetHashCode(IVirtualFile obj)
            {
                ArgumentNullException.ThrowIfNull(obj);
                return owner.GetEntryHashCode(obj, static (provider, entry) => provider.GetFileHashCode(entry));
            }
        }
    }
}
