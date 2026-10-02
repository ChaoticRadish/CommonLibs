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

        /// <summary>
        /// 获取描述该目录下文件的条目: 最后一段为文件名, 前面的段为子目录路径 (仅传文件名即直接子级), 不检查存在性
        /// </summary>
        /// <param name="segments">路径段, 不允许为空</param>
        IVirtualFile GetFile(params string[] segments);

        /// <summary>
        /// 获取描述该目录下子目录的条目: 路径段为子目录路径 (仅传目录名即直接子级), 不检查存在性
        /// </summary>
        /// <param name="segments">路径段, 不允许为空</param>
        IVirtualDirectory GetDirectory(params string[] segments);
    }
}
