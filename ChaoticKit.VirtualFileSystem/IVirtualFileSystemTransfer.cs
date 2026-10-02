using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 多目录操作接口: 跨路径/跨实现的文件与目录操作 (移动, 拷贝等), 方法接收条目
    /// </summary>
    public interface IVirtualFileSystemTransfer
    {
        /// <summary>
        /// 移动文件 (源条目 → 目标条目)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> MoveFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default);

        /// <summary>
        /// 移动目录 (源条目 → 目标条目)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> MoveDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default);

        /// <summary>
        /// 拷贝文件 (源条目 → 目标条目)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> CopyFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default);

        /// <summary>
        /// 拷贝目录 (源条目 → 目标条目)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> CopyDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// 在 <see cref="IVirtualFileSystemTransfer"/> 基础上, 包含支持的类型信息
    /// </summary>
    public interface IVirtualFileSystemTransferIncludeTypeInfo : IVirtualFileSystemTransfer
    {
        /// <summary>
        /// 支持的类型信息对
        /// </summary>
        IReadOnlyList<TransferSourceTargetTypePair> SupportedTypePairs { get; }
    }

    /// <summary>
    /// 单个信息对: <see cref="IVirtualFileSystemTransfer"/> 的源类型与目标类型的类型信息
    /// </summary>
    /// <param name="SourceType">源类型</param>
    /// <param name="TargetType">目标类型</param>
    public record struct TransferSourceTargetTypePair(string SourceType, string TargetType)
    {

    }
}
