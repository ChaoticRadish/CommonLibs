namespace ChaoticKit.VirtualFileSystem.WindowsShare
{
    /// <summary>
    /// 断开共享目录连接的释放器
    /// </summary>
    internal sealed class DisconnectAction : IDisposable
    {
        private Action? _action;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public DisconnectAction(Action action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
            GC.SuppressFinalize(this);
        }
    }
}
