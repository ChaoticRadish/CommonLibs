using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.LocalFileSystem002
    /// LocalFileSystem 目录操作 (创建/存在性/递归删除) 与枚举 (文件/子目录), 以及 LocalDirectory 便捷方法 (GetFile/GetDirectory) 的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 需要临时目录: 通过 Path.GetTempPath() + Guid 自建唯一目录 (LocalDirectory.FromPath 解析为路径段), finally 中递归删除;
    /// 全部断言使用固定数据, 无随机/计时。源测试通过 VirtualFilePath(baseDir, 段数组) 内联构造路径 (等价替代 TestPathHelper.ToSegments 思路)。
    /// </remarks>
    [TestClass]
    public sealed class LocalFileSystemDirectoryTest : UnitTestBase
    {
        /// <summary>目录条目的来源描述 (模拟从配置文件读取的路径配置)</summary>
        private const string SourceDesc = "来自测试配置";

        /// <summary>
        /// 生成唯一临时测试目录路径 (仅构造路径字符串, 不落盘)
        /// </summary>
        private static string NewTestDir()
        {
            return Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
        }

        [TestMethod]
        public async Task CreateDirectories_多级目录_存在性()
        {
            string testDir = NewTestDir();
            var fs = new LocalFileSystem();
            var baseDir = LocalDirectory.FromPath(testDir, SourceDesc);
            Log($"临时目录: {baseDir.FullPath}");
            try
            {
                Log("== 创建多级目录 a/b/c ==");
                var dirCResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b", "c"]));
                dirCResult.DataImpossibleNull();
                IVirtualDirectory dirC = dirCResult.Data!;
                IOperationResultEx createC = await fs.CreateDirectoryAsync(dirC);
                Assert.IsTrue(createC.IsSuccess, "创建 a/b/c 应成功");
                Log($"创建 a/b/c: IsSuccess={createC.IsSuccess}");

                var dirBResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b"]));
                dirBResult.DataImpossibleNull();
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirBResult.Data!)).IsSuccess, "创建 a/b 应成功");
                Log("创建 a/b: IsSuccess=True");

                var dirAResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]));
                dirAResult.DataImpossibleNull();
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirAResult.Data!)).IsSuccess, "创建 a 应成功");
                Log("创建 a: IsSuccess=True");

                Log("== 目录存在性 ==");
                var existsResult = await fs.DirectoryExistsAsync(dirC);
                Assert.IsTrue(existsResult.Data == true, "a/b/c 应存在");
                Log($"a/b/c 存在: {existsResult.Data}");

                Assert.IsTrue(Directory.Exists(dirC.FullPath), "磁盘上 a/b/c 目录应真实存在");
                Log("磁盘校验: 真实目录存在, 通过");
            }
            finally
            {
                await fs.DeleteDirectoryAsync(baseDir, true);
                Log("清理完成");
            }
        }

        [TestMethod]
        public async Task ListFiles_枚举目录a下文件()
        {
            string testDir = NewTestDir();
            var fs = new LocalFileSystem();
            var baseDir = LocalDirectory.FromPath(testDir, SourceDesc);
            try
            {
                var dirAResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]));
                dirAResult.DataImpossibleNull();
                IVirtualDirectory dirA = dirAResult.Data!;
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirA)).IsSuccess, "创建目录 a 应成功");

                Log("== 写入文件 a/f1.txt, a/f2.txt ==");
                var f1Result = await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "f1.txt"]));
                f1Result.DataImpossibleNull();
                var f2Result = await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "f2.txt"]));
                f2Result.DataImpossibleNull();
                await WriteFileAsync(fs, f1Result.Data!, "内容1");
                await WriteFileAsync(fs, f2Result.Data!, "内容2");

                Assert.AreEqual("内容1", File.ReadAllText(Path.Combine(dirA.FullPath, "f1.txt")), "f1.txt 内容应写回一致");
                Assert.AreEqual("内容2", File.ReadAllText(Path.Combine(dirA.FullPath, "f2.txt")), "f2.txt 内容应写回一致");
                Log("文件内容写回校验通过");

                Log("== 枚举目录 a 下的文件 ==");
                var filesResult = await fs.ListFilesAsync(dirA);
                Assert.IsTrue(filesResult.IsSuccess, "枚举文件应成功");
                Log($"枚举结果: IsSuccess={filesResult.IsSuccess}");
                IVirtualFile[] files = filesResult.Data ?? [];
                var names = files.Select(f => f.Name).ToList();
                CollectionAssert.AreEquivalent(new[] { "f1.txt", "f2.txt" }, names, "枚举结果应包含 f1.txt 与 f2.txt");
                foreach (var f in files)
                {
                    Assert.AreEqual(FileSystemTypeConstants.Local, f.FileSystemType, "枚举条目的 FileSystemType 应为 Local");
                    Assert.IsFalse(string.IsNullOrEmpty(f.Source), "枚举条目的 Source 不应为空");
                    Assert.IsTrue(f.FullPath.EndsWith(Path.DirectorySeparatorChar + f.Name), $"FullPath 应以文件名结尾: {f.FullPath}");
                    Log($"  - Name={f.Name}, FullPath={f.FullPath}, FileSystemType={f.FileSystemType}, Source={f.Source}");
                }
            }
            finally
            {
                await fs.DeleteDirectoryAsync(baseDir, true);
                Log("清理完成");
            }
        }

        [TestMethod]
        public async Task ListDirectories_枚举目录a下子目录()
        {
            string testDir = NewTestDir();
            var fs = new LocalFileSystem();
            var baseDir = LocalDirectory.FromPath(testDir, SourceDesc);
            try
            {
                var dirAResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]));
                dirAResult.DataImpossibleNull();
                IVirtualDirectory dirA = dirAResult.Data!;
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirA)).IsSuccess, "创建目录 a 应成功");

                var dirCResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b", "c"]));
                dirCResult.DataImpossibleNull();
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirCResult.Data!)).IsSuccess, "创建 a/b/c 应成功");

                Log("== 枚举目录 a 下的子目录 ==");
                var dirsResult = await fs.ListDirectoriesAsync(dirA);
                Assert.IsTrue(dirsResult.IsSuccess, "枚举子目录应成功");
                Log($"枚举结果: IsSuccess={dirsResult.IsSuccess}");
                IVirtualDirectory[] dirs = dirsResult.Data ?? [];
                Assert.AreEqual(1, dirs.Length, "目录 a 下应只有 1 个子目录");
                IVirtualDirectory dirB = dirs[0];
                Assert.AreEqual("b", dirB.Name, "子目录名应为 b");
                Assert.AreEqual("b", dirB.Paths[^1], "子目录 Paths 末段应为 b");
                Assert.AreEqual(FileSystemTypeConstants.Local, dirB.FileSystemType, "子目录 FileSystemType 应为 Local");
                Assert.IsTrue(dirB.FullPath.EndsWith(Path.DirectorySeparatorChar + "b"), $"子目录 FullPath 应以 b 结尾: {dirB.FullPath}");
                Log($"  - Name={dirB.Name}, FullPath={dirB.FullPath}, Paths=[{string.Join("/", dirB.Paths)}], FileSystemType={dirB.FileSystemType}");
            }
            finally
            {
                await fs.DeleteDirectoryAsync(baseDir, true);
                Log("清理完成");
            }
        }

        [TestMethod]
        public async Task DeleteDirectory_递归删除_删除后不存在()
        {
            string testDir = NewTestDir();
            var fs = new LocalFileSystem();
            var baseDir = LocalDirectory.FromPath(testDir, SourceDesc);
            try
            {
                var dirCResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b", "c"]));
                dirCResult.DataImpossibleNull();
                IVirtualDirectory dirC = dirCResult.Data!;
                Assert.IsTrue((await fs.CreateDirectoryAsync(dirC)).IsSuccess, "创建 a/b/c 应成功");

                var dirAResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]));
                dirAResult.DataImpossibleNull();
                IVirtualDirectory dirA = dirAResult.Data!;

                Log("== 递归删除目录 a ==");
                IOperationResultEx deleteResult = await fs.DeleteDirectoryAsync(dirA, true);
                Assert.IsTrue(deleteResult.IsSuccess, "递归删除 a 应成功");
                Log($"删除 a (递归): IsSuccess={deleteResult.IsSuccess}");

                var existsAfter = await fs.DirectoryExistsAsync(dirA);
                Assert.IsTrue(existsAfter.Data == false, "删除后 a 应不存在");
                Log($"删除后 a 存在: {existsAfter.Data}");

                Assert.IsFalse(Directory.Exists(dirA.FullPath), "磁盘上 a 目录应已被删除");
                Assert.IsTrue(Directory.Exists(baseDir.FullPath), "父目录 baseDir 应保留");
                Log("磁盘校验: a 已删除且父目录保留, 通过");
            }
            finally
            {
                await fs.DeleteDirectoryAsync(baseDir, true);
                Log("清理完成");
            }
        }

        [TestMethod]
        public void LocalDirectory_便捷方法_GetFile与GetDirectory()
        {
            var baseDir = LocalDirectory.FromPath(NewTestDir(), SourceDesc);

            Log("== 目录条目便捷方法 GetFile / GetDirectory ==");
            var directFile = baseDir.GetFile("direct.txt");
            Assert.AreEqual("direct.txt", directFile.Name, "直接文件 Name 应一致");
            Assert.AreSame(baseDir, directFile.Directory, "单段 GetFile 的所属目录应为当前目录条目");
            Assert.AreEqual(Path.Combine(baseDir.FullPath, "direct.txt"), directFile.FullPath, "直接文件 FullPath 应一致");
            Log($"直接文件 FullPath: {directFile.FullPath}");

            var deepFile = baseDir.GetFile("x", "y", "deep.txt");
            Assert.AreEqual("deep.txt", deepFile.Name, "深层文件 Name 应一致");
            Assert.AreEqual("y", deepFile.Directory.Name, "深层文件所属目录名应为 y");
            Assert.AreEqual(Path.Combine(baseDir.FullPath, "x", "y", "deep.txt"), deepFile.FullPath, "深层文件 FullPath 应一致");
            Log($"深层文件 FullPath: {deepFile.FullPath}");
            Log($"深层文件所属目录: {deepFile.Directory.Name}");

            var deepDir = baseDir.GetDirectory("x", "y");
            Assert.AreEqual("y", deepDir.Name, "深层目录 Name 应一致");
            CollectionAssert.AreEqual(new[] { "x", "y" }, deepDir.Paths[^2..], "深层目录 Paths 尾部应为 x/y");
            Assert.AreEqual(Path.Combine(baseDir.FullPath, "x", "y"), deepDir.FullPath, "深层目录 FullPath 应一致");
            Log($"深层目录 FullPath: {deepDir.FullPath}");

            Assert.AreEqual(SourceDesc, directFile.Source, "便捷方法创建的条目应继承来源描述");
            Log("来源描述继承校验通过");
        }

        [TestMethod]
        public void LocalDirectory_GetFile_空路径段抛ArgumentException()
        {
            var baseDir = LocalDirectory.FromPath(NewTestDir(), SourceDesc);

            Assert.ThrowsException<ArgumentException>(() => baseDir.GetFile(), "GetFile() 无路径段应抛 ArgumentException");
            Log("GetFile() 无路径段抛 ArgumentException, 通过");
            Assert.ThrowsException<ArgumentException>(() => baseDir.GetFile(""), "GetFile(\"\") 空路径段应抛 ArgumentException");
            Log("GetFile(\"\") 空路径段抛 ArgumentException, 通过");
            Assert.ThrowsException<ArgumentException>(() => baseDir.GetFile("a", ""), "GetFile(\"a\", \"\") 中间空路径段应抛 ArgumentException");
            Log("GetFile(\"a\", \"\") 中间空路径段抛 ArgumentException, 通过");
        }

        [TestMethod]
        public void LocalDirectory_GetDirectory_空路径段抛ArgumentException()
        {
            var baseDir = LocalDirectory.FromPath(NewTestDir(), SourceDesc);

            Assert.ThrowsException<ArgumentException>(() => baseDir.GetDirectory(), "GetDirectory() 无路径段应抛 ArgumentException");
            Log("GetDirectory() 无路径段抛 ArgumentException, 通过");
            Assert.ThrowsException<ArgumentException>(() => baseDir.GetDirectory(""), "GetDirectory(\"\") 空路径段应抛 ArgumentException");
            Log("GetDirectory(\"\") 空路径段抛 ArgumentException, 通过");
            Assert.ThrowsException<ArgumentException>(() => baseDir.GetDirectory("x", ""), "GetDirectory(\"x\", \"\") 中间空路径段应抛 ArgumentException");
            Log("GetDirectory(\"x\", \"\") 中间空路径段抛 ArgumentException, 通过");
        }

        /// <summary>
        /// 通过 OpenWriteAsync 向文件写入内容
        /// </summary>
        private static async Task WriteFileAsync(LocalFileSystem fs, IVirtualFile file, string content)
        {
            var result = await fs.OpenWriteAsync(file);
            Assert.IsTrue(result.IsSuccess && result.Data != null, "打开文件写入流应成功");
            using var writer = new StreamWriter(result.Data!);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
        }
    }
}
