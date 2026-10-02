namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 虚拟文件系统描述契约: 标识文件系统类型与来源 (仅类型标识, 不含具体位置)
    /// </summary>
    public interface IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 文件系统类型, 如 "Local" / "FTP" / "WindowsShare" 等
        /// </summary>
        string FileSystemType { get; }

        /// <summary>
        /// 来源描述, 侧重于"来源"而非实现细节, 主要用于调试
        /// <para>如实现类全名 "ChaoticKit.VirtualFileSystem.Default.LocalFileSystem"; 若条目是由配置信息提取转换而来, 则写 "来自 XXX 配置信息"</para>
        /// </summary>
        string Source { get; }
    }
}
