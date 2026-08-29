namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 虚拟文件条目 (文件名 + 所属目录 + 描述)
    /// </summary>
    public interface IVirtualFile : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 文件名 (不含路径)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 所属目录条目
        /// </summary>
        IVirtualDirectory Directory { get; }

        /// <summary>
        /// 完整路径 (主要用于显示/调试, 实际操作不使用)
        /// </summary>
        string FullPath { get; }
    }
}
