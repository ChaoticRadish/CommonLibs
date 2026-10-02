namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 文件系统类型常量, 各实现统一复用, 避免魔法字符串散落
    /// </summary>
    public static class FileSystemTypeConstants
    {
        /// <summary>
        /// 本机文件系统
        /// </summary>
        public const string Local = "Local";

        /// <summary>
        /// FTP
        /// </summary>
        public const string Ftp = "FTP";

        /// <summary>
        /// Windows 共享目录
        /// </summary>
        public const string WindowsShare = "WindowsShare";
    }
}
