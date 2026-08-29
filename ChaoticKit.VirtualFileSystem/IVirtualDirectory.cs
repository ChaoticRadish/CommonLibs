namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 虚拟目录条目 (目录名 + 路径信息 + 描述)
    /// </summary>
    public interface IVirtualDirectory : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 目录名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 路径信息: 路径段数组, 与具体实现无关 (如 ["a", "b"] 表示 a/b 目录)
        /// </summary>
        string[] Paths { get; }

        /// <summary>
        /// 完整路径 (主要用于显示/调试, 实际操作不使用)
        /// </summary>
        string FullPath { get; }
    }
}
