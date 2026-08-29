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
}
