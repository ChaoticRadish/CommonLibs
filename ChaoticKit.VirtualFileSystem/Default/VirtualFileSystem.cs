namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 虚拟文件系统工厂默认实现: 实现字典 + 传输注册表 (未注册返回兜底) + 创建操作器
    /// </summary>
    public class VirtualFileSystem : IVirtualFileSystem
    {
        private readonly Dictionary<string, IVirtualFileSystemProvider> _providers = [];
        private readonly Dictionary<(string sourceType, string targetType), IVirtualFileSystemTransfer> _transfers = [];
        private readonly FallbackTransfer _fallback;

        /// <summary>
        /// 
        /// </summary>
        public VirtualFileSystem()
        {
            _fallback = new FallbackTransfer(GetProvider);
        }

        /// <inheritdoc/>
        public void RegisterProvider(IVirtualFileSystemProvider provider)
        {
            _providers[provider.FileSystemType] = provider;
        }

        /// <inheritdoc/>
        public bool UnregisterProvider(string fileSystemType)
        {
            return _providers.Remove(fileSystemType);
        }

        /// <inheritdoc/>
        public IVirtualFileSystemProvider? GetProvider(string fileSystemType)
        {
            return _providers.TryGetValue(fileSystemType, out var provider) ? provider : null;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IVirtualFileSystemProvider> Providers => _providers.Values.ToList();

        /// <inheritdoc/>
        public void RegisterTransfer(string sourceType, string targetType, IVirtualFileSystemTransfer transfer)
        {
            _transfers[(sourceType, targetType)] = transfer;
        }

        /// <inheritdoc/>
        public bool UnregisterTransfer(string sourceType, string targetType)
        {
            return _transfers.Remove((sourceType, targetType));
        }

        /// <inheritdoc/>
        public IVirtualFileSystemTransfer GetTransfer(string sourceType, string targetType)
        {
            return _transfers.TryGetValue((sourceType, targetType), out var transfer) ? transfer : _fallback;
        }

        /// <inheritdoc/>
        public IVirtualFileSystemOperator CreateOperator()
        {
            return new VirtualFileSystemOperator(this);
        }
    }
}
