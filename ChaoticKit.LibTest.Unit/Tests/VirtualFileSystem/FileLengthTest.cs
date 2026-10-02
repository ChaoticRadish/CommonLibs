using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.FileLength001
    /// 文件长度获取的两条路径验证:
    /// ① 实现声明支持直接获取 (LocalFileSystem 用 FileInfo.Length) —— 不读取文件内容;
    /// ② 实现声明不支持时, 由操作器或实现基类以读取内容的方式计算 (OpenRead + 分块累加)
    /// 另验证文件不存在时返回失败结果 (不向外抛异常)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 需要临时目录时自建 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除;
    /// 写入内容为固定生成的 3079 字节 (i % 251, 非整块大小以覆盖分块累加), 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class FileLengthTest : UnitTestBase
    {
        /// <summary>测试文件大小: 非整块大小, 覆盖分块读取的累加逻辑 (与源控制台测试一致)</summary>
        private const int FileSize = 1024 * 3 + 7;
        /// <summary>测试文件名</summary>
        private const string FileName = "length.bin";
        /// <summary>用于验证"文件不存在"路径的文件名</summary>
        private const string MissingFileName = "missing.bin";

        /// <summary>
        /// 声明不支持直接获取长度的本机实现: 用于验证读流计算的兜底路径
        /// </summary>
        private sealed class NoDirectLengthLocalFileSystem : LocalFileSystem
        {
            public override bool SupportGetFileLength(IVirtualFile file) => false;
        }

        /// <summary>测试场景: 已写入固定内容的临时目录, 以及两套 (支持/不支持直接获取长度) 实现与操作器</summary>
        private sealed class Scenario
        {
            public required string TestDir { get; init; }
            public required LocalFileSystem Fs { get; init; }
            public required IVirtualFileSystemOperator Op { get; init; }
            public required NoDirectLengthLocalFileSystem FsNoDirect { get; init; }
            public required IVirtualFileSystemOperator OpNoDirect { get; init; }
            public required LocalDirectory Root { get; init; }
            public required IVirtualFile TargetFile { get; init; }
            public required IVirtualFile MissingFile { get; init; }
        }

        /// <summary>固定内容: 第 i 个字节为 i % 251</summary>
        private static byte[] CreatePayload()
        {
            byte[] payload = new byte[FileSize];
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i % 251);
            }
            return payload;
        }

        private static async Task WriteBytesAsync(LocalFileSystem fs, IVirtualFile file, byte[] payload)
        {
            var openResult = await fs.OpenWriteAsync(file);
            openResult.DataImpossibleNull("文件写入流");
            using var stream = openResult.Data;
            await stream.WriteAsync(payload);
        }

        /// <summary>
        /// 创建场景: 临时目录中写入固定内容文件, 并分别注册 LocalFileSystem 与"不支持直接获取长度"的实现
        /// </summary>
        private static async Task<Scenario> CreateScenarioAsync()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            // 场景一: 默认实现 (支持直接获取长度)
            var factory = new ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem();
            var fs = new LocalFileSystem();
            factory.RegisterProvider(fs);
            var op = factory.CreateOperator();

            var root = LocalDirectory.FromPath(testDir, "来自测试配置");
            var targetFile = root.GetFile(FileName);
            var missingFile = root.GetFile(MissingFileName);
            await WriteBytesAsync(fs, targetFile, CreatePayload());

            // 场景二: 声明不支持直接获取长度的实现 (走读流计算兜底)
            var noDirectFactory = new ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem();
            var fsNoDirect = new NoDirectLengthLocalFileSystem();
            noDirectFactory.RegisterProvider(fsNoDirect);
            var opNoDirect = noDirectFactory.CreateOperator();

            return new Scenario
            {
                TestDir = testDir,
                Fs = fs,
                Op = op,
                FsNoDirect = fsNoDirect,
                OpNoDirect = opNoDirect,
                Root = root,
                TargetFile = targetFile,
                MissingFile = missingFile,
            };
        }

        private static void Cleanup(Scenario scenario)
        {
            if (Directory.Exists(scenario.TestDir))
            {
                Directory.Delete(scenario.TestDir, recursive: true);
            }
        }

        [TestMethod]
        public async Task SupportGetFileLength_能力声明_实现与操作器一致()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log($"测试文件: {s.TargetFile.Directory.FullPath}\\{s.TargetFile.Name}, 预期长度: {FileSize}");

                Assert.IsTrue(s.Fs.SupportGetFileLength(s.TargetFile), "LocalFileSystem 应声明支持直接获取长度");
                Assert.IsTrue(s.Op.SupportGetFileLength(s.TargetFile), "操作器应转发实现的声明 (不支持者以外为 true)");
                Assert.IsFalse(s.FsNoDirect.SupportGetFileLength(s.TargetFile), "自定义实现应声明不支持直接获取长度");
                Assert.IsFalse(s.OpNoDirect.SupportGetFileLength(s.TargetFile), "操作器应转发实现的声明 (不支持时为 false)");

                Log("能力声明: LocalFileSystem=true, 操作器=true, 不支持实现=false, 操作器=false, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }

        [TestMethod]
        public async Task GetFileLengthAsync_实现直接获取_长度一致且不读取内容()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log("--- 直接调用实现 (LocalFileSystem) ---");
                var directResult = await s.Fs.GetFileLengthAsync(s.TargetFile);
                Assert.IsTrue(directResult.IsSuccess, "实现直接获取长度应成功");
                Assert.AreEqual((long)FileSize, directResult.Data, "实现直接获取的长度应与写入长度一致");
                Log($"实现直接获取: 长度={directResult.Data}, 通过");

                Log("--- 经操作器获取 (实现支持直接获取) ---");
                var opInvoking = new List<VirtualFileSystemOperation>();
                var opInvoked = new List<VirtualFileSystemOperation>();
                s.Op.OperationInvoking += (_, e) => opInvoking.Add(e.Operation);
                s.Op.OperationInvoked += (_, e) => opInvoked.Add(e.Operation);

                var opResult = await s.Op.GetFileLengthAsync(s.TargetFile);
                Assert.IsTrue(opResult.IsSuccess, "操作器获取长度应成功");
                Assert.AreEqual((long)FileSize, opResult.Data, "操作器获取的长度应与写入长度一致");

                Assert.IsTrue(opInvoking.Contains(VirtualFileSystemOperation.GetFileLength), "应触发 GetFileLength 的 Invoking 事件");
                Assert.IsTrue(opInvoked.Contains(VirtualFileSystemOperation.GetFileLength), "应触发 GetFileLength 的 Invoked 事件");
                Assert.IsFalse(opInvoking.Contains(VirtualFileSystemOperation.OpenRead), "实现支持直接获取时不应打开只读流");
                Log($"操作器直接获取: 长度={opResult.Data}, 未触发 OpenRead, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }

        [TestMethod]
        public async Task GetFileLengthAsync_实现不支持_读流计算兜底()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log("--- 经操作器获取 (实现声明不支持直接获取 → 读流计算) ---");
                var invoking = new List<VirtualFileSystemOperation>();
                s.OpNoDirect.OperationInvoking += (_, e) => invoking.Add(e.Operation);

                var fallbackResult = await s.OpNoDirect.GetFileLengthAsync(s.TargetFile);
                Assert.IsTrue(fallbackResult.IsSuccess, "读流计算长度应成功");
                Assert.AreEqual((long)FileSize, fallbackResult.Data, "读流计算的长度应与写入长度一致 (含非整块尾段)");

                Assert.IsTrue(invoking.Contains(VirtualFileSystemOperation.GetFileLength), "应触发 GetFileLength 事件");
                Assert.IsTrue(invoking.Contains(VirtualFileSystemOperation.OpenRead), "实现不支持直接获取时应打开只读流计算长度");
                Log($"读流计算: 长度={fallbackResult.Data}, 触发了 OpenRead, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }

        [TestMethod]
        public async Task GetFileLengthAsync_基类兜底_直接调用不支持直接获取的实现()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log("--- 直接调用实现 (走 VirtualFileSystemProviderBase 的兜底实现) ---");
                var baseFallbackResult = await s.FsNoDirect.GetFileLengthAsync(s.TargetFile);
                Assert.IsTrue(baseFallbackResult.IsSuccess, "实现基类的读流兜底计算应成功");
                Assert.AreEqual((long)FileSize, baseFallbackResult.Data, "实现基类兜底计算的长度应与写入长度一致");
                Log($"实现基类兜底: 长度={baseFallbackResult.Data}, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }

        [TestMethod]
        public async Task GetFileLengthAsync_文件不存在_返回失败结果()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log("--- 获取不存在文件的长度 ---");
                var missingResult = await s.Op.GetFileLengthAsync(s.MissingFile);

                Assert.IsTrue(missingResult.IsFailure, "不存在的文件应返回失败结果 (不向外抛异常)");
                Assert.IsFalse(string.IsNullOrEmpty(missingResult.FailureReason), "失败结果应带有失败原因");
                Log($"失败 (预期): 原因={missingResult.FailureReason}, 含异常={missingResult.HasException}, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }

        [TestMethod]
        public async Task DeleteDirectoryAsync_清理后目录不存在()
        {
            Scenario s = await CreateScenarioAsync();
            try
            {
                Log("--- 递归删除测试目录 ---");
                var deleteResult = await s.Fs.DeleteDirectoryAsync(s.Root, recursive: true);
                Assert.IsTrue(deleteResult.IsSuccess, "递归删除测试目录应成功");

                var existsResult = await s.Fs.DirectoryExistsAsync(s.Root);
                Assert.IsFalse(existsResult.Data, "删除后目录不应存在");
                Assert.IsFalse(Directory.Exists(s.TestDir), "磁盘上测试目录应已被删除");
                Log("目录已清理, 通过");
            }
            finally
            {
                Cleanup(s);
            }
        }
    }
}
