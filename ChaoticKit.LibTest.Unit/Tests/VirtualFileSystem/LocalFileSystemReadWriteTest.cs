using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.LocalFileSystem001
    /// LocalFileSystem (ChaoticKit.VirtualFileSystem.Default) 读写删查基本流程的验证:
    /// 获取根/子目录条目、创建目录、获取文件条目、写入固定内容、存在性检查、回读内容一致、删除后不存在
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 需要临时目录时自建 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除; 固定内容断言, 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class LocalFileSystemReadWriteTest : UnitTestBase
    {
        /// <summary>写入的固定文件内容 (与源控制台测试一致)</summary>
        private const string FixedContent = "Hello VirtualFileSystem!";
        /// <summary>创建的子目录名</summary>
        private const string DirName = "dirA";
        /// <summary>写入/读取的文件名</summary>
        private const string FileName = "hello.txt";

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

        /// <summary>
        /// 通过 LocalFileSystem API 创建 dirA 目录并获取 hello.txt 文件条目
        /// </summary>
        /// <returns>目录条目与文件条目</returns>
        private static async Task<(IVirtualDirectory dirA, IVirtualFile file)> PrepareDirAndFileAsync(LocalFileSystem fs, string[] segments)
        {
            var dirAResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, DirName]));
            Assert.IsTrue(dirAResult.IsSuccess, "获取 dirA 目录条目应成功");
            dirAResult.DataImpossibleNull("dirA 目录条目");
            Assert.AreEqual(DirName, dirAResult.Data.Name, "目录条目名应为 dirA");

            var createDirResult = await fs.CreateDirectoryAsync(dirAResult.Data);
            Assert.IsTrue(createDirResult.IsSuccess, "创建 dirA 目录应成功");

            var fileResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, DirName, FileName]));
            Assert.IsTrue(fileResult.IsSuccess, "获取 hello.txt 文件条目应成功");
            fileResult.DataImpossibleNull("hello.txt 文件条目");
            Assert.AreEqual(FileName, fileResult.Data.Name, "文件条目名应为 hello.txt");
            Assert.AreEqual(DirName, fileResult.Data.Directory.Name, "文件所属目录名应为 dirA");

            return (dirAResult.Data, fileResult.Data);
        }

        /// <summary>
        /// 通过 LocalFileSystem API 向文件写入固定内容
        /// </summary>
        private static async Task WriteFixedContentAsync(LocalFileSystem fs, IVirtualFile file)
        {
            var writeResult = await fs.OpenWriteAsync(file);
            Assert.IsTrue(writeResult.IsSuccess, "打开写入流应成功");
            Assert.IsNotNull(writeResult.Data, "写入流不应为 null");
            using (StreamWriter writer = new(writeResult.Data!))
            {
                await writer.WriteAsync(FixedContent);
                await writer.FlushAsync();
            }
        }

        [TestMethod]
        public async Task GetRootEntry_根目录条目与创建子目录()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();

                Log("--- 获取根目录条目 ---");
                var rootResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(segments));
                Assert.IsTrue(rootResult.IsSuccess, "获取根目录条目应成功");
                rootResult.DataImpossibleNull("根目录条目");
                Assert.AreEqual(Path.GetFileName(testDir), rootResult.Data.Name, "根目录条目名应为测试目录的末段名");
                Assert.AreEqual(segments.Length, rootResult.Data.Paths.Length, "根目录条目的路径段数量应与输入段数量一致");
                Log($"根目录名: {rootResult.Data.Name}, 路径段数量: {rootResult.Data.Paths.Length}, 通过");

                Log("--- 创建目录 dirA ---");
                var dirAResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, DirName]));
                Assert.IsTrue(dirAResult.IsSuccess, "获取 dirA 目录条目应成功");
                dirAResult.DataImpossibleNull("dirA 目录条目");
                var createDirResult = await fs.CreateDirectoryAsync(dirAResult.Data);
                Assert.IsTrue(createDirResult.IsSuccess, "创建 dirA 目录应成功");
                Assert.IsTrue(Directory.Exists(Path.Combine(testDir, DirName)), "磁盘上 dirA 目录应真实存在");
                Log($"dirA 已创建: {Path.Combine(testDir, DirName)}, 通过");
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
        public async Task OpenWrite_写入固定内容_文件存在性为真()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();

                var (_, file) = await PrepareDirAndFileAsync(fs, segments);

                Log("--- 写入固定内容 ---");
                await WriteFixedContentAsync(fs, file);
                Log("写入完成");

                Log("--- 存在性检查 ---");
                var existsResult = await fs.FileExistsAsync(file);
                Assert.IsTrue(existsResult.IsSuccess, "存在性检查应成功");
                Assert.IsTrue(existsResult.Data, "写入后文件应存在");
                Log($"文件存在: {existsResult.Data}, 通过");
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
        public async Task OpenRead_回读内容与写入一致()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();

                var (_, file) = await PrepareDirAndFileAsync(fs, segments);
                await WriteFixedContentAsync(fs, file);

                Log("--- 读取比对 ---");
                var readResult = await fs.OpenReadAsync(file);
                Assert.IsTrue(readResult.IsSuccess, "打开读取流应成功");
                Assert.IsNotNull(readResult.Data, "读取流不应为 null");
                string content;
                using (StreamReader reader = new(readResult.Data!))
                {
                    content = await reader.ReadToEndAsync();
                }
                Assert.AreEqual(FixedContent, content, "回读内容应与写入的固定内容完全一致");
                Log($"读取内容: {content}, 内容一致: {content == FixedContent}, 通过");
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
        public async Task DeleteFile_删除后不存在_目录递归删除()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();

                var (_, file) = await PrepareDirAndFileAsync(fs, segments);
                await WriteFixedContentAsync(fs, file);

                var beforeResult = await fs.FileExistsAsync(file);
                Assert.IsTrue(beforeResult.Data, "删除前文件应存在");

                Log("--- 删除文件 ---");
                var deleteResult = await fs.DeleteFileAsync(file);
                Assert.IsTrue(deleteResult.IsSuccess, "删除文件应成功");
                var existsAfter = await fs.FileExistsAsync(file);
                Assert.IsFalse(existsAfter.Data, "删除后文件不应存在");
                Log($"删除结果: {deleteResult}, 删除后文件存在: {existsAfter.Data}, 通过");

                Log("--- 递归删除根目录 ---");
                var rootResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(segments));
                rootResult.DataImpossibleNull("根目录条目");
                var deleteDirResult = await fs.DeleteDirectoryAsync(rootResult.Data, recursive: true);
                Assert.IsTrue(deleteDirResult.IsSuccess, "递归删除根目录应成功");
                Assert.IsFalse(Directory.Exists(testDir), "磁盘上测试目录应已被删除");
                Log("目录递归删除完成, 通过");
            }
            finally
            {
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, recursive: true);
                }
            }
        }
    }
}
