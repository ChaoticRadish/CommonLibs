namespace ChaoticKit.VirtualFileSystem.FluentFTP
{
    /// <summary>
    /// FTP 目录条目 (实现 <see cref="IFtpConnectionInfo"/> 承载全部连接信息)
    /// </summary>
    public class FtpDirectory : IVirtualDirectory, IFtpConnectionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">目录名</param>
        /// <param name="paths">路径段数组 (相对默认源头)</param>
        /// <param name="source">来源描述</param>
        /// <param name="host">服务器主机</param>
        /// <param name="port">服务器端口号</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="rootPath">FTP 根路径 (如 "/" 或 "/data")</param>
        public FtpDirectory(string name, string[] paths, string source, string host, int port, string userName, string password, string rootPath)
        {
            Name = name;
            Paths = paths;
            Source = source;
            Host = host;
            Port = port;
            UserName = userName;
            Password = password;
            RootPath = rootPath;
        }

        /// <inheritdoc/>
        public string FileSystemType => FileSystemTypeConstants.Ftp;

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string[] Paths { get; }

        /// <inheritdoc/>
        public string Host { get; }

        /// <inheritdoc/>
        public int Port { get; }

        /// <inheritdoc/>
        public string UserName { get; }

        /// <inheritdoc/>
        public string Password { get; }

        /// <inheritdoc/>
        public string RootPath { get; }

        /// <inheritdoc/>
        public string FullPath
        {
            get
            {
                List<string> segments = [];
                if (!string.IsNullOrEmpty(RootPath))
                {
                    segments.AddRange(RootPath.Trim('/').Split('/'));
                }
                segments.AddRange(Paths);
                return "/" + string.Join('/', segments);
            }
        }
    }
}
