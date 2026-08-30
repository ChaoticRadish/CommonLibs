namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 清理目录选项 (Flag 风格): 决定清理的文件/目录范围与是否包含子级
    /// </summary>
    [Flags]
    public enum VirtualFileSystemClearOption
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 清理文件 (删除指定范围内的文件)
        /// </summary>
        Files = 1 << 0,

        /// <summary>
        /// 清理目录 (删除指定范围内的目录本身)
        /// </summary>
        Directories = 1 << 1,

        /// <summary>
        /// 包含子级 (递归进入子目录执行清理)
        /// </summary>
        Recursive = 1 << 2,

        /// <summary>
        /// 清理全部内容 (删除文件与目录, 不包含子级)
        /// </summary>
        All = Files | Directories,

        /// <summary>
        /// 递归清理全部内容 (删除文件与目录, 包含子级)
        /// </summary>
        AllRecursive = Files | Directories | Recursive,
    }
}
