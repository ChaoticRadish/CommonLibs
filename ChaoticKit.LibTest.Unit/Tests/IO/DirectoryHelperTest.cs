using ChaoticKit.Extensions;
using ChaoticKit.IO;

namespace ChaoticKit.LibTest.Unit.IO
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.IO.Directory001
    /// DirectoryHelper.TraversalFiles 目录遍历与 MatchSuffix 后缀过滤的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 遍历顺序不参与断言, 一律按相对路径排序后比对, 保证确定性。
    /// </remarks>
    [TestClass]
    public sealed class DirectoryHelperTest : UnitTestBase
    {
        /// <summary>
        /// 在系统临时目录下构建固定结构的测试目录树, 测试结束后递归删除。
        /// 结构:
        ///   root/a.cs, root/b.txt, root/c.log
        ///   root/sub1/d.cs, root/sub1/e.TXT
        ///   root/sub1/nested/f.cs
        ///   root/sub2/g.png
        /// </summary>
        private sealed class TempTree : IDisposable
        {
            private readonly List<string> _all = new();
            private readonly List<string> _rootOnly = new();
            private readonly List<string> _csOnly = new();
            private readonly List<string> _csOrTxt = new();

            /// <summary>目录树根路径</summary>
            public string Root { get; }

            /// <summary>全部文件 (含子文件夹), 相对路径, 已按 Ordinal 排序</summary>
            public string[] All { get; private set; } = Array.Empty<string>();

            /// <summary>仅根目录文件, 相对路径, 已按 Ordinal 排序</summary>
            public string[] RootOnly { get; private set; } = Array.Empty<string>();

            /// <summary>所有 .cs 文件, 相对路径, 已按 Ordinal 排序</summary>
            public string[] CsOnly { get; private set; } = Array.Empty<string>();

            /// <summary>所有 .cs 或 .txt 文件, 相对路径, 已按 Ordinal 排序</summary>
            public string[] CsOrTxt { get; private set; } = Array.Empty<string>();

            public TempTree()
            {
                Root = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Root);

                CreateFile("a.cs");
                CreateFile("b.txt");
                CreateFile("c.log");
                CreateFile(Path.Combine("sub1", "d.cs"));
                CreateFile(Path.Combine("sub1", "e.TXT"));
                CreateFile(Path.Combine("sub1", "nested", "f.cs"));
                CreateFile(Path.Combine("sub2", "g.png"));

                All = _all.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                RootOnly = _rootOnly.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                CsOnly = _csOnly.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                CsOrTxt = _csOrTxt.OrderBy(x => x, StringComparer.Ordinal).ToArray();
            }

            private void CreateFile(string relative)
            {
                string full = Path.Combine(Root, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                File.WriteAllText(full, "test");

                _all.Add(relative);
                if (!relative.Contains(Path.DirectorySeparatorChar))
                {
                    _rootOnly.Add(relative);
                }

                string ext = Path.GetExtension(relative);
                if (ext.Equals(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    _csOnly.Add(relative);
                    _csOrTxt.Add(relative);
                }
                else if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    _csOrTxt.Add(relative);
                }
            }

            public void Dispose()
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, recursive: true);
                }
            }
        }

        [TestMethod]
        public void TraversalFiles_遍历所有子文件夹_返回全部文件()
        {
            TempTree tree = new();
            try
            {
                Log("--- 测试: TraversalFiles(root, true) 返回全部文件 ---");
                string[] actual = DirectoryHelper.TraversalFiles(tree.Root, true)
                    .Select(f => Path.GetRelativePath(tree.Root, f.FullName))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                CollectionAssert.AreEqual(tree.All, actual, "遍历所有子文件夹应返回目录树中的全部文件");
                Log($"共 {actual.Length} 个文件: {string.Join(", ", actual)}, 通过");
            }
            finally
            {
                tree.Dispose();
            }
        }

        [TestMethod]
        public void TraversalFiles_仅遍历根文件夹_只返回根目录文件()
        {
            TempTree tree = new();
            try
            {
                Log("--- 测试: TraversalFiles(root, false) 仅返回根目录文件 ---");
                string[] actual = DirectoryHelper.TraversalFiles(tree.Root, false)
                    .Select(f => Path.GetRelativePath(tree.Root, f.FullName))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                CollectionAssert.AreEqual(tree.RootOnly, actual, "仅遍历根文件夹应只返回根目录下的文件");
                Log($"共 {actual.Length} 个文件: {string.Join(", ", actual)}, 通过");
            }
            finally
            {
                tree.Dispose();
            }
        }

        [TestMethod]
        public void TraversalFiles_目录不存在_返回空()
        {
            Log("--- 测试: TraversalFiles 对不存在的目录返回空 ---");
            string missing = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            try
            {
                Assert.AreEqual(0, DirectoryHelper.TraversalFiles(missing, true).Count(), "不存在的目录遍历所有子文件夹应返回空");
                Assert.AreEqual(0, DirectoryHelper.TraversalFiles(missing, false).Count(), "不存在的目录仅遍历根文件夹应返回空");
                Log("TraversalFiles 对不存在的目录返回空, 通过");
            }
            finally
            {
                if (Directory.Exists(missing))
                {
                    Directory.Delete(missing, recursive: true);
                }
            }
        }

        [DataTestMethod]
        [DataRow("cs")]
        [DataRow(".cs")]
        [DataRow("CS")]
        public void MatchSuffix_单后缀忽略大小写与点前缀(string suffix)
        {
            TempTree tree = new();
            try
            {
                Log($"--- 测试: MatchSuffix(\"{suffix}\") 过滤 .cs 文件 ---");
                string[] actual = DirectoryHelper.TraversalFiles(tree.Root, true)
                    .MatchSuffix(suffix)
                    .Select(f => Path.GetRelativePath(tree.Root, f.FullName))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                CollectionAssert.AreEqual(tree.CsOnly, actual, $"MatchSuffix(\"{suffix}\") 应只返回 .cs 文件");
                Log($"MatchSuffix(\"{suffix}\") 命中 {actual.Length} 个文件: {string.Join(", ", actual)}, 通过");
            }
            finally
            {
                tree.Dispose();
            }
        }

        [TestMethod]
        public void MatchSuffix_多后缀忽略大小写()
        {
            TempTree tree = new();
            try
            {
                Log("--- 测试: MatchSuffix(\"cs\", \"TXt\") 过滤 .cs 与 .txt 文件 (忽略大小写) ---");
                string[] actual = DirectoryHelper.TraversalFiles(tree.Root, true)
                    .MatchSuffix("cs", "TXt")
                    .Select(f => Path.GetRelativePath(tree.Root, f.FullName))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                CollectionAssert.AreEqual(tree.CsOrTxt, actual, "MatchSuffix(\"cs\", \"TXt\") 应返回 .cs 与 .txt 文件 (e.TXT 也应命中)");
                Log($"MatchSuffix(\"cs\", \"TXt\") 命中 {actual.Length} 个文件: {string.Join(", ", actual)}, 通过");
            }
            finally
            {
                tree.Dispose();
            }
        }

        [TestMethod]
        public void MatchSuffix_空匹配项_返回空()
        {
            TempTree tree = new();
            try
            {
                Log("--- 测试: MatchSuffix() 空匹配项返回空 ---");
                string[] actual = DirectoryHelper.TraversalFiles(tree.Root, true)
                    .MatchSuffix()
                    .Select(f => Path.GetRelativePath(tree.Root, f.FullName))
                    .ToArray();

                Assert.AreEqual(0, actual.Length, "空匹配项应返回空集合");
                Log("MatchSuffix() 空匹配项返回空, 通过");
            }
            finally
            {
                tree.Dispose();
            }
        }
    }
}
