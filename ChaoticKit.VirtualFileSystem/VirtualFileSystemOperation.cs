namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 操作类型枚举: 区分操作器事件中的具体操作
    /// </summary>
    public enum VirtualFileSystemOperation
    {
        /// <summary>
        /// 打开读取流
        /// </summary>
        OpenRead,

        /// <summary>
        /// 打开写入流
        /// </summary>
        OpenWrite,

        /// <summary>
        /// 关闭流 (流被释放时触发)
        /// </summary>
        CloseStream,

        /// <summary>
        /// 按路径获取文件条目
        /// </summary>
        GetFile,

        /// <summary>
        /// 按路径获取目录条目
        /// </summary>
        GetDirectory,

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        FileExists,

        /// <summary>
        /// 删除文件
        /// </summary>
        DeleteFile,

        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        DirectoryExists,

        /// <summary>
        /// 创建目录
        /// </summary>
        CreateDirectory,

        /// <summary>
        /// 删除目录
        /// </summary>
        DeleteDirectory,

        /// <summary>
        /// 枚举文件
        /// </summary>
        ListFiles,

        /// <summary>
        /// 枚举目录
        /// </summary>
        ListDirectories,

        /// <summary>
        /// 移动文件
        /// </summary>
        MoveFile,

        /// <summary>
        /// 移动目录
        /// </summary>
        MoveDirectory,

        /// <summary>
        /// 拷贝文件
        /// </summary>
        CopyFile,

        /// <summary>
        /// 拷贝目录
        /// </summary>
        CopyDirectory,
    }
}
