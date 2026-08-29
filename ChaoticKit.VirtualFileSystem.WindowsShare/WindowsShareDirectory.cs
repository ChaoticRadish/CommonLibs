namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// Windows 共享目录目录条目 (实现 <see cref="IWindowsShareConnectionInfo"/> 承载连接信息)
    /// </summary>
    public class WindowsShareDirectory : IVirtualDirectory, IWindowsShareConnectionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">目录名</param>
        /// <param name="paths">路径段数组 (相对默认源头)</param>
        /// <param name="source">来源描述</param>
        /// <param name="deviceHost">所在设备的 IP 或设备名</param>
        /// <param name="shareRootPath">共享根路径 (UNC 或本地路径)</param>
        /// <param name="credential">凭据 (可为 <see langword="null"/>)</param>
        public WindowsShareDirectory(string name, string[] paths, string source, string deviceHost, string shareRootPath, WindowsShareCredential? credential)
        {
            Name = name;
            Paths = paths;
            Source = source;
            DeviceHost = deviceHost;
            ShareRootPath = shareRootPath;
            Credential = credential;
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.WindowsShare;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string[] Paths { get; }

        /// <inheritdoc/>
        public string DeviceHost { get; }

        /// <inheritdoc/>
        public string ShareRootPath { get; }

        /// <inheritdoc/>
        public WindowsShareCredential? Credential { get; }

        /// <inheritdoc/>
        public string FullPath
        {
            get
            {
                string root = ShareRootPath.TrimEnd('\\', '/');
                if (Paths.Length == 0)
                {
                    return root;
                }
                return root + "\\" + string.Join('\\', Paths);
            }
        }
    }
}
