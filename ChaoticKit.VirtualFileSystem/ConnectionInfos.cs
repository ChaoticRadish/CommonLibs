namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// FTP 连接信息接口: 承载 FTP 服务器连接所需的全部信息
    /// </summary>
    public interface IFtpConnectionInfo : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 服务器主机
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 服务器端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 密码
        /// </summary>
        string Password { get; }

        /// <summary>
        /// FTP 根路径 (如 "/" 或 "/data"), 默认登录后根目录
        /// </summary>
        string RootPath { get; }
    }

    /// <summary>
    /// HTTP 连接信息接口 (预留 HTTP 文件系统): 承载基础地址
    /// </summary>
    public interface IHttpConnectionInfo : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 基础地址
        /// </summary>
        Uri BaseUri { get; }
    }
}
