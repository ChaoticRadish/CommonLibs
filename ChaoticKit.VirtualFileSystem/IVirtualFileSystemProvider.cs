using ChaoticKit.Data.Struct;

namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 单个虚拟文件系统实现的操作接口
    /// <para>方法接收条目 (<see cref="IVirtualFile"/> / <see cref="IVirtualDirectory"/>), 不接收裸路径; 仅 <see cref="GetFileAsync"/> / <see cref="GetDirectoryAsync"/> 接收 <see cref="VirtualFilePath"/></para>
    /// </summary>
    public interface IVirtualFileSystemProvider : IVirtualFileSystemDescriptor
    {
        /// <summary>
        /// 按路径关系获取文件条目 (不检查文件是否存在)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<IVirtualFile>> GetFileAsync(VirtualFilePath path, CancellationToken cancellationToken = default);

        /// <summary>
        /// 按路径关系获取目录条目 (不检查目录是否存在)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<IVirtualDirectory>> GetDirectoryAsync(VirtualFilePath path, CancellationToken cancellationToken = default);

        /// <summary>
        /// 打开文件读取流
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<Stream>> OpenReadAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// 打开文件写入流
        /// </summary>
        /// <param name="file"></param>
        /// <param name="overwrite">为 <see langword="true"/> 时覆盖已存在文件, 否则新建</param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<bool>> FileExistsAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> DeleteFileAsync(IVirtualFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// 目录是否存在
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<bool>> DirectoryExistsAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建目录 (含父目录), 传入期望的目录条目
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> CreateDirectoryAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="recursive">是否递归删除子内容</param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> DeleteDirectoryAsync(IVirtualDirectory directory, bool recursive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 枚举指定目录下的文件条目 (不含子目录)
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<IVirtualFile[]>> ListFilesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 枚举指定目录下的子目录条目
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<IVirtualDirectory[]>> ListDirectoriesAsync(IVirtualDirectory directory, CancellationToken cancellationToken = default);
    }
}
