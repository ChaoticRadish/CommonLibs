using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.LocalFileSystem003
    /// LocalFileSystem (ChaoticKit.VirtualFileSystem.Default) 失败场景 (返回失败结果而非抛异常) 与 VirtualFilePath 的 '.'/'..' 路径归一化验证:
    /// 读取不存在的文件返回失败且携带异常; 含分隔符的非法路径段返回失败而非抛异常; '.' 段忽略与 '..' 段回溯归一化
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 仅"读取不存在文件"场景自建临时目录 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除;
    /// '.'/'..' 路径归一化为纯逻辑断言, 不触碰磁盘; 固定数据断言, 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class LocalFileSystemFailureTest : UnitTestBase
    {
        /// <summary>不存在的测试文件名 (位于自建临时目录下)</summary>
        private const string MissingFileName = "not_exists.txt";

        /// <summary>
        /// 在系统临时目录下自建唯一测试目录 (不依赖运行环境绝对路径), 并转换为盘符 + 路径段数组
        /// </summary>
        /// <param name="segments">目录路径段 (["盘符", ...], 供 VirtualFilePath.FromRoot 使用)</param>
        /// <returns>测试目录的本地绝对路径</returns>
        private static string CreateTestDir(out string[] segments)
        {
            string testDir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);
            segments = ToSegments(testDir);
            return testDir;
        }

        /// <summary>
        /// 将绝对路径转换为 ["盘符", ...路径段] 数组 (复刻控制台工程 TestPathHelper.ToSegments 的逻辑)
        /// </summary>
        /// <param name="absolutePath"></param>
        private static string[] ToSegments(string absolutePath)
        {
            string root = Path.GetPathRoot(absolutePath) ?? throw new InvalidOperationException($"无法取得路径根: {absolutePath}");
            string relative = Path.GetRelativePath(root, absolutePath);
            string[] parts = relative.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
            return [root.TrimEnd('\\', '/'), .. parts];
        }

        [TestMethod]
        public async Task OpenReadAsync_文件不存在_返回失败结果并携带异常()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();

                Log("--- 读取不存在的文件 ---");
                var missingFileResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, MissingFileName]));
                Assert.IsTrue(missingFileResult.IsSuccess, "获取文件条目本身应成功 (条目创建不触碰磁盘)");
                missingFileResult.DataImpossibleNull("不存在的文件条目");
                IVirtualFile missingFile = missingFileResult.Data;

                var readResult = await fs.OpenReadAsync(missingFile);
                Assert.IsTrue(readResult.IsFailure, "读取不存在的文件应返回失败结果");
                Assert.IsTrue(readResult.HasException, "失败结果应携带异常");
                Assert.IsInstanceOfType(readResult.Exception, typeof(FileNotFoundException), "异常应为 FileNotFoundException");
                Log($"结果是失败: {readResult.IsFailure}, 携带异常: {readResult.HasException}, 异常类型: {readResult.Exception?.GetType().Name}, 通过");
            }
            finally
            {
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, recursive: true);
                }
            }
        }

        [TestMethod]
        public async Task GetFileAsync_非法路径段含分隔符_返回失败而非抛异常()
        {
            Log("--- 非法路径段 (含分隔符) ---");
            LocalFileSystem fs = new();

            // 目标代码不向外抛异常: NormalizeSegments 抛出的 ArgumentException 被 RunAsync 包装进失败结果
            var badResult = await fs.GetFileAsync(new VirtualFilePath(null, ["a/b.txt"]));

            Assert.IsTrue(badResult.IsFailure, "含分隔符的非法路径段应返回失败结果");
            Assert.IsTrue(badResult.HasException, "失败结果应携带异常");
            Assert.IsInstanceOfType(badResult.Exception, typeof(ArgumentException), "异常应为 ArgumentException");
            Log($"非法段返回失败: {badResult.IsFailure}, 携带异常: {badResult.HasException}, 异常类型: {badResult.Exception?.GetType().Name}, 通过");
        }

        [DataTestMethod]
        [DataRow(new string[] { "a", ".", "b" }, "a/b")]
        [DataRow(new string[] { "a", "..", "b" }, "b")]
        [DataRow(new string[] { "..", "a", "b" }, "a/b")]
        [DataRow(new string[] { "a", "b", "..", "c" }, "a/c")]
        [DataRow(new string[] { ".", "a", ".", "b" }, "a/b")]
        public async Task GetDirectoryAsync_点与点点段归一化(string[] segments, string expectedDisplay)
        {
            LocalFileSystem fs = new();

            var result = await fs.GetDirectoryAsync(new VirtualFilePath(null, segments));
            Assert.IsTrue(result.IsSuccess, "纯逻辑路径段归一化应返回成功");
            result.DataImpossibleNull("归一化后的目录条目");

            string actualDisplay = string.Join("/", result.Data.Paths);
            Assert.AreEqual(expectedDisplay, actualDisplay, $"路径段 [{string.Join(", ", segments)}] 归一化后应展示为 {expectedDisplay}");
            CollectionAssert.AreEqual(expectedDisplay.Split('/'), result.Data.Paths, "归一化后的路径段数组应一致");
            Log($"路径段 [{string.Join(", ", segments)}] 归一化后: {actualDisplay}, 通过");
        }
    }
}
