namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// Windows 共享目录访问凭据
    /// </summary>
    /// <param name="UserName">用户名</param>
    /// <param name="Password">密码</param>
    /// <param name="Domain">域 (可为 <see langword="null"/>)</param>
    public record WindowsShareCredential(string UserName, string Password, string? Domain = null);
}
