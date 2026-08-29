using System;
using System.IO;
using System.Linq;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    /// <summary>
    /// 测试路径辅助: 将本地绝对路径转换为盘符 + 路径段数组 (供 VirtualFilePath 使用)
    /// </summary>
    internal static class TestPathHelper
    {
        /// <summary>
        /// 将绝对路径转换为 ["盘符", ...路径段] 数组 (如 "D:\a\b" -> ["D:", "a", "b"])
        /// </summary>
        /// <param name="absolutePath"></param>
        public static string[] ToSegments(string absolutePath)
        {
            string root = Path.GetPathRoot(absolutePath) ?? throw new InvalidOperationException($"无法取得路径根: {absolutePath}");
            string relative = Path.GetRelativePath(root, absolutePath);
            string[] parts = relative.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
            return [root.TrimEnd('\\', '/'), .. parts];
        }
    }
}
