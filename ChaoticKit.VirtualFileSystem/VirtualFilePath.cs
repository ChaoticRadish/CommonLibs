namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 路径关系: 可选的相对源目录 + 查找路径段
    /// </summary>
    /// <param name="RelativeSource">可选的相对源目录; 为 <see langword="null"/> 时表示从接收方 (实现) 的默认源头开始, 通常即根目录</param>
    /// <param name="PathSegments">访问的查找路径段数组 (如 ["a", "b", "c.png"])</param>
    public readonly record struct VirtualFilePath(IVirtualDirectory? RelativeSource, string[] PathSegments)
    {
        /// <summary>
        /// 从接收方默认源头 (通常为根目录) 开始的路径
        /// </summary>
        /// <param name="segments">查找路径段</param>
        public static VirtualFilePath FromRoot(params string[] segments) => new(null, segments);
    }
}
