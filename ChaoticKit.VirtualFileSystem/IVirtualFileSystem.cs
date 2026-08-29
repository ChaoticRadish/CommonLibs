namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 虚拟文件系统工厂: 注册/注销实现与传输方案, 创建操作器
    /// <para>工厂不直接提供操作, 所有操作行为在操作器中</para>
    /// </summary>
    public interface IVirtualFileSystem
    {
        #region 实现注册与管理
        /// <summary>
        /// 注册实现 (按 <see cref="IVirtualFileSystemDescriptor.FileSystemType"/> 去重, 后注册覆盖先注册)
        /// </summary>
        /// <param name="provider"></param>
        void RegisterProvider(IVirtualFileSystemProvider provider);

        /// <summary>
        /// 注销实现
        /// </summary>
        /// <param name="fileSystemType"></param>
        /// <returns>是否注销成功</returns>
        bool UnregisterProvider(string fileSystemType);

        /// <summary>
        /// 按类型获取实现, 未注册返回 <see langword="null"/>
        /// </summary>
        /// <param name="fileSystemType"></param>
        IVirtualFileSystemProvider? GetProvider(string fileSystemType);

        /// <summary>
        /// 已注册的全部实现
        /// </summary>
        IReadOnlyList<IVirtualFileSystemProvider> Providers { get; }
        #endregion

        #region 传输方案注册与解析
        /// <summary>
        /// 注册 (源类型, 目标类型) 对应的优化传输实现
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="transfer"></param>
        void RegisterTransfer(string sourceType, string targetType, IVirtualFileSystemTransfer transfer);

        /// <summary>
        /// 注销传输方案
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns>是否注销成功</returns>
        bool UnregisterTransfer(string sourceType, string targetType);

        /// <summary>
        /// 解析 (源类型, 目标类型) 的传输实现, 未注册时返回兜底实现 (不为 <see langword="null"/>)
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        IVirtualFileSystemTransfer GetTransfer(string sourceType, string targetType);
        #endregion

        #region 操作器
        /// <summary>
        /// 创建绑定当前工厂 (实现与传输方案) 的操作器
        /// </summary>
        IVirtualFileSystemOperator CreateOperator();
        #endregion
    }
}
