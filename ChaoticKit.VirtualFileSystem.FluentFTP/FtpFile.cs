namespace ChaoticKit.VirtualFileSystem.FluentFTP
{
    /// <summary>
    /// FTP 文件条目 (实现 <see cref="IFtpConnectionInfo"/>, 连接信息通过所属目录转发获取)
    /// </summary>
    public class FtpFile : IVirtualFile, IFtpConnectionInfo
    {
        private readonly FtpDirectory _directory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="directory">所属目录条目 (具体类型 <see cref="FtpDirectory"/>)</param>
        /// <param name="source">来源描述</param>
        public FtpFile(string name, FtpDirectory directory, string source)
        {
            Name = name;
            _directory = directory;
            Source = source;
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.Ftp;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public FtpDirectory Directory => _directory;

        /// <inheritdoc/>
        IVirtualDirectory IVirtualFile.Directory => _directory;

        /// <inheritdoc/>
        public string Host => _directory.Host;

        /// <inheritdoc/>
        public int Port => _directory.Port;

        /// <inheritdoc/>
        public string UserName => _directory.UserName;

        /// <inheritdoc/>
        public string Password => _directory.Password;

        /// <inheritdoc/>
        public string RootPath => _directory.RootPath;

        /// <inheritdoc/>
        public string FullPath => _directory.FullPath.TrimEnd('/') + "/" + Name;
    }
}
