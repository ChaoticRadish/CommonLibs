namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// Windows 共享目录文件条目 (实现 <see cref="IWindowsShareConnectionInfo"/>, 连接信息通过所属目录转发获取)
    /// </summary>
    public class WindowsShareFile : IVirtualFile, IWindowsShareConnectionInfo
    {
        private readonly WindowsShareDirectory _directory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="directory">所属目录条目 (具体类型 <see cref="WindowsShareDirectory"/>)</param>
        /// <param name="source">来源描述</param>
        public WindowsShareFile(string name, WindowsShareDirectory directory, string source)
        {
            Name = name;
            _directory = directory;
            Source = source;
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.WindowsShare;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public WindowsShareDirectory Directory => _directory;

        /// <inheritdoc/>
        IVirtualDirectory IVirtualFile.Directory => _directory;

        /// <inheritdoc/>
        public string DeviceHost => _directory.DeviceHost;

        /// <inheritdoc/>
        public string ShareRootPath => _directory.ShareRootPath;

        /// <inheritdoc/>
        public WindowsShareCredential? Credential => _directory.Credential;

        /// <inheritdoc/>
        public string FullPath => _directory.FullPath.TrimEnd('\\', '/') + "\\" + Name;
    }
}
