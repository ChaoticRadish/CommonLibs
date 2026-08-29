namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 操作器: 同时提供实现操作与多目录操作, 并统一触发操作事件 (不按操作类型拆分事件)
    /// </summary>
    public interface IVirtualFileSystemOperator : IVirtualFileSystemProvider, IVirtualFileSystemTransfer
    {
        /// <summary>
        /// 操作执行前触发 (所有操作共用, 用 <see cref="VirtualFileSystemOperation"/> 区分)
        /// </summary>
        event EventHandler<VirtualFileSystemOperationEventArgs>? OperationInvoking;

        /// <summary>
        /// 操作执行后触发 (所有操作共用, 用 <see cref="VirtualFileSystemOperation"/> 区分)
        /// </summary>
        event EventHandler<VirtualFileSystemOperationEventArgs>? OperationInvoked;
    }
}
