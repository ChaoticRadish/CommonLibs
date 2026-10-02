using ChaoticKit.Data.Struct;

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

        /// <summary>
        /// 确保指定目录存在: 不存在时创建 (封装常用操作, 简化调用; 内部通过 <see cref="IVirtualFileSystemProvider.DirectoryExistsAsync"/> 与 <see cref="IVirtualFileSystemProvider.CreateDirectoryAsync"/> 实现)
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>成功时附带 <see langword="bool"/> 数据: <see langword="true"/> 表示执行了创建, <see langword="false"/> 表示目录原本已存在</returns>
        ValueTask<IOperationResultEx<bool>> EnsureDirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);
    }
}
