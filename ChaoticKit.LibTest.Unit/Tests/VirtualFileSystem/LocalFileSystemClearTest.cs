using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.LocalFileSystem004
    /// LocalFileSystem.ClearDirectoryAsync 各清理选项的验证:
    /// 准备 base/a/b + f1/f2/f3 目录结构, Files|Recursive 删除全部文件保留目录结构, Directories 删除子目录(不含子级)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 需要临时目录时自建 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除; 固定内容断言, 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class LocalFileSystemClearTest : UnitTestBase
    {
        /// <summary>写入文件的固定内容 (内容不参与清理断言)</summary>
        private const string FileContent = "content";

        /// <summary>
        /// 在系统临时目录下自建唯一测试目录 (不依赖运行环境绝对路径)
        /// </summary>
        private static string CreateTestDir()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);
            return testDir;
        }

        /// <summary>
        /// 通过 LocalFileSystem API 准备 base/a/b 目录结构与 f1/f2/f3 文件 (f1 在根、f2 在 a、f3 在 a/b)
        /// </summary>
        /// <returns>目录 a、目录 a/b 与文件 f1/f2/f3 的条目</returns>
        private static async Task<(IVirtualDirectory dirA, IVirtualDirectory dirB, IVirtualFile f1, IVirtualFile f2, IVirtualFile f3)> PrepareStructureAsync(LocalFileSystem fs, string testDir)
        {
            var baseDir = LocalDirectory.FromPath(testDir, "来自测试配置");

            var dirAResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]));
            Assert.IsTrue(dirAResult.IsSuccess, "获取目录 a 条目应成功");
            dirAResult.DataImpossibleNull("目录 a 条目");
            var dirBResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b"]));
            Assert.IsTrue(dirBResult.IsSuccess, "获取目录 a/b 条目应成功");
            dirBResult.DataImpossibleNull("目录 a/b 条目");

            var createAResult = await fs.CreateDirectoryAsync(dirAResult.Data);
            Assert.IsTrue(createAResult.IsSuccess, "创建目录 a 应成功");
            var createBResult = await fs.CreateDirectoryAsync(dirBResult.Data);
            Assert.IsTrue(createBResult.IsSuccess, "创建目录 a/b 应成功");

            IVirtualFile f1 = await CreateFileAsync(fs, baseDir, "f1.txt");
            IVirtualFile f2 = await CreateFileAsync(fs, baseDir, "a", "f2.txt");
            IVirtualFile f3 = await CreateFileAsync(fs, baseDir, "a", "b", "f3.txt");

            Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "a", "b")), "磁盘上 a/b 目录应真实存在");
            Assert.IsTrue(File.Exists(Path.Combine(testDir, "f1.txt")), "磁盘上 f1.txt 应真实存在");
            Assert.IsTrue(File.Exists(Path.Combine(testDir, "a", "f2.txt")), "磁盘上 a/f2.txt 应真实存在");
            Assert.IsTrue(File.Exists(Path.Combine(testDir, "a", "b", "f3.txt")), "磁盘上 a/b/f3.txt 应真实存在");

            return (dirAResult.Data, dirBResult.Data, f1, f2, f3);
        }

        /// <summary>
        /// 通过 LocalFileSystem API 创建并写入固定内容的文件, 返回文件条目
        /// </summary>
        private static async Task<IVirtualFile> CreateFileAsync(LocalFileSystem fs, IVirtualDirectory baseDir, params string[] segments)
        {
            var fileResult = await fs.GetFileAsync(new VirtualFilePath(baseDir, segments));
            Assert.IsTrue(fileResult.IsSuccess, $"获取文件条目 {string.Join("/", segments)} 应成功");
            fileResult.DataImpossibleNull("文件条目");

            var writeResult = await fs.OpenWriteAsync(fileResult.Data);
            Assert.IsTrue(writeResult.IsSuccess, $"打开写入流 {string.Join("/", segments)} 应成功");
            Assert.IsNotNull(writeResult.Data, "写入流不应为 null");
            using (StreamWriter writer = new(writeResult.Data))
            {
                await writer.WriteAsync(FileContent);
                await writer.FlushAsync();
            }
            return fileResult.Data;
        }

        [TestMethod]
        public async Task Clear_FilesRecursive_删除全部文件_保留目录结构()
        {
            string testDir = CreateTestDir();
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();
                var (dirA, dirB, f1, f2, f3) = await PrepareStructureAsync(fs, testDir);
                var baseDir = LocalDirectory.FromPath(testDir, "来自测试配置");

                Log("--- 清理: Files | Recursive (保留目录结构, 删除所有文件) ---");
                var clearResult = await fs.ClearDirectoryAsync(baseDir, VirtualFileSystemClearOption.Files | VirtualFileSystemClearOption.Recursive);
                Assert.IsTrue(clearResult.IsSuccess, "Files|Recursive 清理应成功");
                Log($"清理结果: {clearResult}");

                Assert.IsFalse((await fs.FileExistsAsync(f1)).Data, "f1.txt 应已删除");
                Assert.IsFalse((await fs.FileExistsAsync(f2)).Data, "a/f2.txt 应已删除");
                Assert.IsFalse((await fs.FileExistsAsync(f3)).Data, "a/b/f3.txt 应已删除");
                Log("f1/f2/f3 均已删除, 通过");

                Assert.IsTrue((await fs.DirectoryExistsAsync(dirA)).Data, "目录 a 应保留");
                Assert.IsTrue((await fs.DirectoryExistsAsync(dirB)).Data, "目录 a/b 应保留");
                Log("目录 a 与 a/b 均保留, 通过");

                Assert.IsFalse(File.Exists(Path.Combine(testDir, "f1.txt")), "磁盘上 f1.txt 应已被删除");
                Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "a")), "磁盘上目录 a 应保留");
                Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "a", "b")), "磁盘上目录 a/b 应保留");
                Log("磁盘核对: 文件全删、目录全留, 通过");
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
        public async Task Clear_Directories_删除子目录_不含子级()
        {
            string testDir = CreateTestDir();
            try
            {
                Log($"测试根目录: {testDir}");
                LocalFileSystem fs = new();
                var (dirA, dirB, f1, f2, f3) = await PrepareStructureAsync(fs, testDir);
                var baseDir = LocalDirectory.FromPath(testDir, "来自测试配置");

                Log("--- 清理: Directories (删除子目录, 不含子级) ---");
                var clearResult = await fs.ClearDirectoryAsync(baseDir, VirtualFileSystemClearOption.Directories);
                Assert.IsTrue(clearResult.IsSuccess, "Directories 清理应成功");
                Log($"清理结果: {clearResult}");

                Assert.IsFalse((await fs.DirectoryExistsAsync(dirA)).Data, "目录 a 应已删除");
                Assert.IsFalse((await fs.DirectoryExistsAsync(dirB)).Data, "目录 a/b 应已删除");
                Log("目录 a 与 a/b 均已删除, 通过");

                // Directories 选项未含 Files: 根下文件应保留
                Assert.IsTrue((await fs.FileExistsAsync(f1)).Data, "f1.txt 应保留 (未指定 Files)");
                Assert.IsFalse((await fs.FileExistsAsync(f2)).Data, "a/f2.txt 应随目录 a 删除");
                Assert.IsFalse((await fs.FileExistsAsync(f3)).Data, "a/b/f3.txt 应随目录 a 删除");
                Log("f1 保留, f2/f3 随目录删除, 通过");

                Assert.IsFalse(Directory.Exists(Path.Combine(testDir, "a")), "磁盘上目录 a 应已被删除");
                Assert.IsTrue(File.Exists(Path.Combine(testDir, "f1.txt")), "磁盘上 f1.txt 应保留");
                Log("磁盘核对: 子目录删除、根文件保留, 通过");
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
