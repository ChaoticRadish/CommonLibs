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
        /// <para>需要保留已有内容时使用 <see cref="OpenUpdateAsync"/></para>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="overwrite">为 <see langword="true"/> 时覆盖已存在文件, 否则新建</param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<Stream>> OpenWriteAsync(IVirtualFile file, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 以更新方式打开文件写入流: 不清空已有内容, 文件不存在时创建
        /// <para>与 <see cref="OpenWriteAsync"/> 的区别: 后者覆盖模式下会清空已有内容, 本方法保留已有内容, 适合在现有数据上定位写入 (随机写入)</para>
        /// <para>实现应尽量返回支持定位 (<see cref="Stream.CanSeek"/>) 的流, 不支持的实现由调用方负责检查并处理</para>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx<Stream>> OpenUpdateAsync(IVirtualFile file, CancellationToken cancellationToken = default);

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

        /// <summary>
        /// 清理指定目录: 按 <paramref name="option"/> 决定清理文件/目录及是否包含子级
        /// <para>示例: <see cref="VirtualFileSystemClearOption.Recursive"/> + <see cref="VirtualFileSystemClearOption.Files"/> = 保留目录结构, 删除所有文件</para>
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="option"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IOperationResultEx> ClearDirectoryAsync(IVirtualDirectory directory, VirtualFileSystemClearOption option, CancellationToken cancellationToken = default);

        /// <summary>
        /// 判断两个目录条目是否指向同一目录
        /// <para>仅用于比较本实现的条目: 条目不属于本实现时一律返回 <see langword="false"/>; 跨文件系统比较由 <see cref="IVirtualFileSystemEntryComparer"/> 负责</para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        bool DirectoryEquals(IVirtualDirectory left, IVirtualDirectory right);

        /// <summary>
        /// 判断两个文件条目是否指向同一文件
        /// <para>仅用于比较本实现的条目: 条目不属于本实现时一律返回 <see langword="false"/></para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        bool FileEquals(IVirtualFile left, IVirtualFile right);

        /// <summary>
        /// 取得目录条目的哈希码 (需与 <see cref="DirectoryEquals"/> 的判定结果保持一致)
        /// </summary>
        /// <param name="directory"></param>
        int GetDirectoryHashCode(IVirtualDirectory directory);

        /// <summary>
        /// 取得文件条目的哈希码 (需与 <see cref="FileEquals"/> 的判定结果保持一致)
        /// </summary>
        /// <param name="file"></param>
        int GetFileHashCode(IVirtualFile file);
    }
}
