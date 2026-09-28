using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.Operator001
    /// VirtualFileSystemFactory.CreateOperator 操作器事件机制验证:
    /// OpenWrite/CloseStream/FileExists/DeleteFile 事件触发, 以及 EnsureDirectoryExistsAsync 两次结果 (首次创建 true / 再次已存在 false)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 需要临时目录时自建 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除; 固定内容断言, 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class VirtualFileSystemOperatorTest : UnitTestBase
    {
        /// <summary>写入/删除/存在性检查的事件文件名</summary>
        private const string EventFileName = "event.txt";
        /// <summary>EnsureDirectoryExists 的目标目录名</summary>
        private const string EnsureDirName = "ensure_dir";
        /// <summary>写入的固定文件内容 (与源控制台测试一致)</summary>
        private const string FixedContent = "事件测试";

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
        /// 创建带事件监听的测试场景: 工厂 + LocalFileSystem 提供者 + 操作器 + 两个事件记录列表 (及 Invoked 事件参数, 用于校验结果)
        /// </summary>
        private static (IVirtualFileSystemOperator op, LocalFileSystem fs, List<VirtualFileSystemOperation> invokingOps, List<VirtualFileSystemOperation> invokedOps, List<VirtualFileSystemOperationEventArgs> invokedArgs) CreateScenario()
        {
            ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem factory = new();
            LocalFileSystem fs = new();
            factory.RegisterProvider(fs);
            var op = factory.CreateOperator();
            var invokingOps = new List<VirtualFileSystemOperation>();
            var invokedOps = new List<VirtualFileSystemOperation>();
            var invokedArgs = new List<VirtualFileSystemOperationEventArgs>();
            op.OperationInvoking += (s, e) => invokingOps.Add(e.Operation);
            op.OperationInvoked += (s, e) =>
            {
                invokedOps.Add(e.Operation);
                invokedArgs.Add(e);
            };
            return (op, fs, invokingOps, invokedOps, invokedArgs);
        }

        /// <summary>
        /// 获取根目录条目与 event.txt 文件条目
        /// </summary>
        private static async Task<(IVirtualDirectory root, IVirtualFile file)> GetRootAndFileAsync(IVirtualFileSystemOperator op, LocalFileSystem fs, string[] segments)
        {
            var rootResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(segments));
            Assert.IsTrue(rootResult.IsSuccess, "获取根目录条目应成功");
            rootResult.DataImpossibleNull("根目录条目");
            var fileResult = await op.GetFileAsync(new VirtualFilePath(rootResult.Data, [EventFileName]));
            Assert.IsTrue(fileResult.IsSuccess, "获取 event.txt 文件条目应成功");
            fileResult.DataImpossibleNull("event.txt 文件条目");
            return (rootResult.Data, fileResult.Data);
        }

        /// <summary>
        /// 校验所有 Invoked 事件均携带成功结果 (对应源测试的"事件结果携带成功"检查点)
        /// </summary>
        private static void AssertAllInvokedSucceeded(List<VirtualFileSystemOperationEventArgs> invokedArgs)
        {
            foreach (var e in invokedArgs)
            {
                Assert.IsNotNull(e.Result, $"Invoked 事件 {e.Operation} 应携带结果");
                Assert.IsTrue(e.Result.IsSuccess, $"Invoked 事件 {e.Operation} 结果应为成功");
            }
        }

        [TestMethod]
        public async Task OpenWrite_写入并释放流_事件序列含OpenWrite与CloseStream()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                var (op, fs, invokingOps, invokedOps, invokedArgs) = CreateScenario();
                var (root, file) = await GetRootAndFileAsync(op, fs, segments);

                Log("--- 打开写入流 (触发 OpenWrite + CloseStream) ---");
                var writeResult = await op.OpenWriteAsync(file);
                Assert.IsTrue(writeResult.IsSuccess, "打开写入流应成功");
                Assert.IsNotNull(writeResult.Data, "写入流不应为 null");
                using (StreamWriter writer = new(writeResult.Data))
                {
                    await writer.WriteAsync(FixedContent);
                    await writer.FlushAsync();
                }
                Log($"已写入固定内容并释放流: {FixedContent}");

                Log("Invoking 事件序列: " + string.Join(", ", invokingOps));
                Log("Invoked 事件序列: " + string.Join(", ", invokedOps));

                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.OpenWrite), "Invoking 应包含 OpenWrite");
                Assert.IsTrue(invokedOps.Contains(VirtualFileSystemOperation.OpenWrite), "Invoked 应包含 OpenWrite");
                Assert.IsTrue(invokedOps.Contains(VirtualFileSystemOperation.CloseStream), "Invoked 应包含 CloseStream (流释放时触发)");
                AssertAllInvokedSucceeded(invokedArgs);

                CollectionAssert.AreEqual(
                    new[] { VirtualFileSystemOperation.GetFile, VirtualFileSystemOperation.OpenWrite },
                    invokingOps, "Invoking 精确序列应为 [GetFile, OpenWrite]");
                CollectionAssert.AreEqual(
                    new[] { VirtualFileSystemOperation.GetFile, VirtualFileSystemOperation.OpenWrite, VirtualFileSystemOperation.CloseStream },
                    invokedOps, "Invoked 精确序列应为 [GetFile, OpenWrite, CloseStream]");
                Log("事件序列断言通过");
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
        public async Task FileExists_DeleteFile_事件序列含FileExists与DeleteFile()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                var (op, fs, invokingOps, invokedOps, invokedArgs) = CreateScenario();
                var (_, file) = await GetRootAndFileAsync(op, fs, segments);

                // 先写入文件, 保证删除目标真实存在
                var writeResult = await op.OpenWriteAsync(file);
                Assert.IsTrue(writeResult.IsSuccess, "打开写入流应成功");
                Assert.IsNotNull(writeResult.Data, "写入流不应为 null");
                using (StreamWriter writer = new(writeResult.Data))
                {
                    await writer.WriteAsync(FixedContent);
                    await writer.FlushAsync();
                }

                Log("--- 文件存在检查 (触发 FileExists) ---");
                var existsResult = await op.FileExistsAsync(file);
                Assert.IsTrue(existsResult.IsSuccess, "文件存在检查应成功");
                Assert.IsTrue(existsResult.Data, "写入后文件应存在");
                Log($"文件存在: {existsResult.Data}, 通过");

                Log("--- 删除文件 (触发 DeleteFile) ---");
                var deleteResult = await op.DeleteFileAsync(file);
                Assert.IsTrue(deleteResult.IsSuccess, "删除文件应成功");
                Log($"删除结果成功: {deleteResult.IsSuccess}, 通过");

                Log("--- 删除后再查 (应不存在) ---");
                var afterDelete = await op.FileExistsAsync(file);
                Assert.IsTrue(afterDelete.IsSuccess, "删除后的存在性检查应成功");
                Assert.IsFalse(afterDelete.Data, "删除后文件不应存在");
                Log($"删除后文件存在: {afterDelete.Data}, 通过");

                Log("Invoking 事件序列: " + string.Join(", ", invokingOps));
                Log("Invoked 事件序列: " + string.Join(", ", invokedOps));

                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.FileExists), "Invoking 应包含 FileExists");
                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.DeleteFile), "Invoking 应包含 DeleteFile");
                Assert.IsTrue(invokedOps.Contains(VirtualFileSystemOperation.FileExists), "Invoked 应包含 FileExists");
                Assert.IsTrue(invokedOps.Contains(VirtualFileSystemOperation.DeleteFile), "Invoked 应包含 DeleteFile");
                AssertAllInvokedSucceeded(invokedArgs);
                Log("事件序列断言通过");
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
        public async Task EnsureDirectoryExists_首次创建_再次已存在_两次结果()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                var (op, fs, invokingOps, invokedOps, invokedArgs) = CreateScenario();
                var (root, _) = await GetRootAndFileAsync(op, fs, segments);

                var ensureDirResult = await op.GetDirectoryAsync(new VirtualFilePath(root, [EnsureDirName]));
                Assert.IsTrue(ensureDirResult.IsSuccess, "获取 ensure_dir 目录条目应成功");
                ensureDirResult.DataImpossibleNull("ensure_dir 目录条目");
                var ensureDir = ensureDirResult.Data;

                Log("--- 首次确保 (应创建, true) ---");
                var ensureFirst = await op.EnsureDirectoryExistsAsync(ensureDir);
                Assert.IsTrue(ensureFirst.IsSuccess, "首次确保应成功");
                Assert.IsTrue(ensureFirst.Data, "首次确保应创建目录 (返回 true)");
                Log($"首次确保: 成功={ensureFirst.IsSuccess}, 已创建={ensureFirst.Data}, 通过");

                Log("--- 目录已创建检查 ---");
                var dirExists = await op.DirectoryExistsAsync(ensureDir);
                Assert.IsTrue(dirExists.IsSuccess, "目录存在检查应成功");
                Assert.IsTrue(dirExists.Data, "确保后目录应真实存在");
                Log($"目录已创建: {dirExists.Data}, 通过");

                Log("--- 再次确保 (已存在, false) ---");
                var ensureSecond = await op.EnsureDirectoryExistsAsync(ensureDir);
                Assert.IsTrue(ensureSecond.IsSuccess, "再次确保应成功");
                Assert.IsFalse(ensureSecond.Data, "再次确保时目录已存在 (返回 false)");
                Log($"再次确保: 成功={ensureSecond.IsSuccess}, 已存在={!ensureSecond.Data}, 通过");

                Log("Invoking 事件序列: " + string.Join(", ", invokingOps));
                Log("Invoked 事件序列: " + string.Join(", ", invokedOps));

                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.DirectoryExists), "Invoking 应包含 DirectoryExists");
                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.CreateDirectory), "Invoking 应包含 CreateDirectory (仅首次创建时触发)");
                Assert.IsTrue(invokingOps.Contains(VirtualFileSystemOperation.GetDirectory), "Invoking 应包含 GetDirectory");
                Assert.AreEqual(1, invokingOps.Count(op => op == VirtualFileSystemOperation.CreateDirectory),
                    "CreateDirectory 应只出现一次 (仅首次创建)");
                AssertAllInvokedSucceeded(invokedArgs);
                Log("事件序列宽松断言通过 (包含 GetDirectory/DirectoryExists/CreateDirectory, CreateDirectory 仅一次)");
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
