namespace ChaoticKit.VirtualFileSystem
{
    /// <summary>
    /// 虚拟路径工具: 路径段的校验与规范化
    /// </summary>
    public static class VirtualPathHelper
    {
        /// <summary>
        /// 校验单个路径段是否合法
        /// <para>合法条件: 非 <see langword="null"/> 非空, 不含 '/' 与 '\\', 不是 "." 或 ".."</para>
        /// </summary>
        /// <param name="segment"></param>
        public static bool IsValidSegment(string? segment)
        {
            if (string.IsNullOrEmpty(segment)) return false;
            if (segment!.Contains('/') || segment.Contains('\\')) return false;
            if (segment == "." || segment == "..") return false;
            return true;
        }

        /// <summary>
        /// 规范化路径段: 去除空段与 <see langword="null"/> 段; 段内不支持 '/' 与 '\\' (遇到则抛 <see cref="ArgumentException"/>);
        /// "." 段忽略; ".." 段表示上级目录 (弹出上一段, 无段可弹时忽略); 返回新数组
        /// </summary>
        /// <param name="segments"></param>
        /// <exception cref="ArgumentException">路径段包含分隔符时抛出</exception>
        public static string[] NormalizeSegments(IEnumerable<string?> segments)
        {
            List<string> output = [];
            foreach (var segment in segments)
            {
                if (string.IsNullOrEmpty(segment)) continue;
                if (segment!.Contains('/') || segment.Contains('\\'))
                {
                    throw new ArgumentException($"路径段不能包含分隔符: {segment}");
                }
                if (segment == ".") continue;
                if (segment == "..")
                {
                    if (output.Count > 0)
                    {
                        output.RemoveAt(output.Count - 1);
                    }
                    continue;
                }
                output.Add(segment);
            }
            return [.. output];
        }

        /// <summary>
        /// 将路径段拼接为以 '/' 分隔的展示路径 (不做合法性校验, 仅拼接)
        /// </summary>
        /// <param name="segments"></param>
        public static string ToDisplayPath(IEnumerable<string> segments)
        {
            return string.Join('/', segments);
        }
    }
}
