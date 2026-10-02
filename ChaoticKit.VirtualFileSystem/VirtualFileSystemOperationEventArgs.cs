using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 操作事件参数: 所有操作共用同一事件, 用 <see cref="VirtualFileSystemOperation"/> 区分
    /// </summary>
    public class VirtualFileSystemOperationEventArgs : EventArgs
    {
        /// <summary>
        /// 具体操作类型
        /// </summary>
        public VirtualFileSystemOperation Operation { get; init; }

        /// <summary>
        /// 涉及的文件条目 (按操作类型可能为 <see langword="null"/>)
        /// </summary>
        public IVirtualFile? File { get; init; }

        /// <summary>
        /// 涉及的目录条目 (按操作类型可能为 <see langword="null"/>)
        /// </summary>
        public IVirtualDirectory? Directory { get; init; }

        /// <summary>
        /// 操作结果 (操作执行后触发时携带)
        /// </summary>
        public IOperationResultEx? Result { get; init; }
    }
}
