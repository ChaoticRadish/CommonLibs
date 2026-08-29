namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// Windows 共享目录连接信息接口
    /// </summary>
    public interface IWindowsShareConnectionInfo : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 所在设备的 IP 或设备名 (如 "192.168.1.100" 或 "NAS-01")
        /// </summary>
        string DeviceHost { get; }

        /// <summary>
        /// 共享根路径 (UNC 或本地路径)
        /// </summary>
        string ShareRootPath { get; }

        /// <summary>
        /// 凭据 (可为 <see langword="null"/>)
        /// </summary>
        WindowsShareCredential? Credential { get; }
    }
}
