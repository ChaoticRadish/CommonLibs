namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 条目相等比较服务: 判断目录 / 文件条目是否指向同一位置
    /// <para>跨文件系统 (<see cref="IVirtualFileSystemDescriptor.FileSystemType"/> 不同) 必定不相等; 同一文件系统的条目交由对应实现 (<see cref="IVirtualFileSystemProvider"/>) 比较</para>
    /// <para>比较规则由各实现决定 (如本机 Windows 与 Windows 共享不区分大小写), 需要放入字典 / 集合时使用 <see cref="DirectoryComparer"/> 与 <see cref="FileComparer"/></para>
    /// </summary>
    public interface IVirtualFileSystemEntryComparer
    {
        /// <summary>
        /// 判断两个目录条目是否指向同一目录
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>两者均为 <see langword="null"/> 或指向同一目录时为 <see langword="true"/></returns>
        bool DirectoryEquals(IVirtualDirectory? left, IVirtualDirectory? right);

        /// <summary>
        /// 判断两个文件条目是否指向同一文件
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>两者均为 <see langword="null"/> 或指向同一文件时为 <see langword="true"/></returns>
        bool FileEquals(IVirtualFile? left, IVirtualFile? right);

        /// <summary>
        /// 目录条目相等比较器 (可用于字典 / 集合等需要 <see cref="IEqualityComparer{T}"/> 的场景)
        /// </summary>
        IEqualityComparer<IVirtualDirectory> DirectoryComparer { get; }

        /// <summary>
        /// 文件条目相等比较器 (可用于字典 / 集合等需要 <see cref="IEqualityComparer{T}"/> 的场景)
        /// </summary>
        IEqualityComparer<IVirtualFile> FileComparer { get; }
    }
}
