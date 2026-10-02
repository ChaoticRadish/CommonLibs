using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using VirtualFileSystemFactory = ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem;

namespace ChaoticKit.LibTest.Unit.VirtualFileSystem
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.VirtualFileSystem.Transfer001
    /// VirtualFileSystemFactory (ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem) 的实现注册/注销与传输方案注册/兜底解析,
    /// 以及操作器 (CreateOperator) 经兜底传输 (FallbackTransfer) 串联的跨目录 CopyFileAsync / MoveFileAsync 验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 拷贝/移动用例自建临时目录 (Path.GetTempPath()\ChaoticKitTest\Guid) 并在 finally 中删除;
    /// 注册/注销与传输方案解析为纯内存断言 (不触碰磁盘); 固定内容断言, 无随机/计时/网络依赖。
    /// </remarks>
    [TestClass]
    public sealed class VirtualFileSystemTransferTest : UnitTestBase
    {
        /// <summary>拷贝测试写入的固定内容 (与源控制台测试一致)</summary>
        private const string CopyContent = "拷贝测试内容";
        /// <summary>移动测试写入的固定内容 (与源控制台测试一致)</summary>
        private const string MoveContent = "移动测试内容";

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
        /// 通过 LocalFileSystem API 向文件写入固定内容
        /// </summary>
        private static async Task WriteContentAsync(LocalFileSystem fs, IVirtualFile file, string content)
        {
            var writeResult = await fs.OpenWriteAsync(file);
            Assert.IsTrue(writeResult.IsSuccess, "打开写入流应成功");
            Assert.IsNotNull(writeResult.Data, "写入流不应为 null");
            using (StreamWriter writer = new(writeResult.Data!))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// 通过 LocalFileSystem API 读取文件全部内容
        /// </summary>
        private static async Task<string> ReadAllAsync(LocalFileSystem fs, IVirtualFile file)
        {
            var readResult = await fs.OpenReadAsync(file);
            Assert.IsTrue(readResult.IsSuccess, "打开读取流应成功");
            Assert.IsNotNull(readResult.Data, "读取流不应为 null");
            using StreamReader reader = new(readResult.Data!);
            return await reader.ReadToEndAsync();
        }

        [TestMethod]
        public void RegisterProvider_注册注销再注册_GetProvider与Providers数量()
        {
            Log("--- 实现注册与查询 (纯内存, 不触碰磁盘) ---");
            var factory = new VirtualFileSystemFactory();
            var fs = new LocalFileSystem();

            factory.RegisterProvider(fs);
            Assert.IsNotNull(factory.GetProvider(FileSystemTypeConstants.Local), "注册后 GetProvider(Local) 不应为空");
            Assert.IsNull(factory.GetProvider(FileSystemTypeConstants.Ftp), "未注册的 FTP 类型应返回 null");
            Assert.AreEqual(1, factory.Providers.Count, "注册 1 个实现后 Providers 数量应为 1");
            Log($"注册后: GetProvider(Local) 非空, GetProvider(FTP) 为空, Providers.Count = {factory.Providers.Count}, 通过");

            Log("--- 注销与重新注册 ---");
            bool unregistered = factory.UnregisterProvider(FileSystemTypeConstants.Local);
            Assert.IsTrue(unregistered, "注销已注册的 Local 应返回 true");
            Assert.IsNull(factory.GetProvider(FileSystemTypeConstants.Local), "注销后 GetProvider(Local) 应为空");
            Log($"注销 Local: {unregistered}, 注销后 GetProvider(Local) 为空, 通过");

            factory.RegisterProvider(fs);
            Assert.IsNotNull(factory.GetProvider(FileSystemTypeConstants.Local), "重新注册后 GetProvider(Local) 不应为空");
            Assert.AreEqual(1, factory.Providers.Count, "重新注册后 Providers 数量应为 1");
            Log("重新注册 Local 后 GetProvider(Local) 非空, 通过");
        }

        [TestMethod]
        public void GetTransfer_未注册返回兜底_注册返回优化实例_注销回兜底()
        {
            Log("--- 传输方案注册与解析 (纯内存, 不触碰磁盘) ---");
            var factory = new VirtualFileSystemFactory();
            var localPair = new TransferSourceTargetTypePair(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local);

            var fallback = factory.GetTransfer(localPair);
            Assert.IsNotNull(fallback, "未注册传输方案时应返回兜底实现 (非 null)");
            Assert.AreEqual("FallbackTransfer", fallback!.GetType().Name, "兜底实现类型应为 FallbackTransfer");
            Log($"未注册时返回兜底: 类型 {fallback.GetType().Name}, 通过");

            var fake = new FakeTransfer();
            factory.RegisterTransfer(localPair, fake);
            var resolved = factory.GetTransfer(localPair);
            Assert.AreSame(fake, resolved, "注册后应返回注册的优化传输实例");
            Log("注册后 GetTransfer 返回优化实例 (ReferenceEquals 一致), 通过");

            factory.UnregisterTransfer(localPair);
            var fallback2 = factory.GetTransfer(localPair);
            Assert.AreSame(fallback, fallback2, "注销后应重新返回兜底实例 (与初次兜底为同一实例)");
            Log("注销后 GetTransfer 返回兜底实例 (与初次兜底同一实例), 通过");
        }

        [TestMethod]
        public async Task OperatorCopyFile_兜底拷贝_目标文件存在且内容一致()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                var factory = new VirtualFileSystemFactory();
                var fs = new LocalFileSystem();
                factory.RegisterProvider(fs);
                var op = factory.CreateOperator();

                Log("--- 准备 src/dst 目录与源文件 ---");
                var srcDirResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, "src"]));
                Assert.IsTrue(srcDirResult.IsSuccess, "获取 src 目录条目应成功");
                srcDirResult.DataImpossibleNull("src 目录条目");
                IVirtualDirectory srcDir = srcDirResult.Data;

                var dstDirResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, "dst"]));
                Assert.IsTrue(dstDirResult.IsSuccess, "获取 dst 目录条目应成功");
                dstDirResult.DataImpossibleNull("dst 目录条目");
                IVirtualDirectory dstDir = dstDirResult.Data;

                Assert.IsTrue((await fs.CreateDirectoryAsync(srcDir)).IsSuccess, "创建 src 目录应成功");
                Assert.IsTrue((await fs.CreateDirectoryAsync(dstDir)).IsSuccess, "创建 dst 目录应成功");

                var srcFileResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, "src", "data.txt"]));
                Assert.IsTrue(srcFileResult.IsSuccess, "获取源文件条目应成功");
                srcFileResult.DataImpossibleNull("源文件条目");
                IVirtualFile srcFile = srcFileResult.Data;

                var dstFileResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, "dst", "data.txt"]));
                Assert.IsTrue(dstFileResult.IsSuccess, "获取目标文件条目应成功");
                dstFileResult.DataImpossibleNull("目标文件条目");
                IVirtualFile dstFile = dstFileResult.Data;

                await WriteContentAsync(fs, srcFile, CopyContent);
                Log($"已写入源文件: \"{CopyContent}\"");

                Log("--- 操作器兜底拷贝 ---");
                var copyResult = await op.CopyFileAsync(srcFile, dstFile);
                Assert.IsTrue(copyResult.IsSuccess, "操作器拷贝文件应成功");
                Log($"操作器拷贝结果: {copyResult}, 通过");

                var srcExists = await fs.FileExistsAsync(srcFile);
                Assert.IsTrue(srcExists.IsSuccess && srcExists.Data, "拷贝后源文件仍应存在 (拷贝不删除源)");
                var dstExists = await fs.FileExistsAsync(dstFile);
                Assert.IsTrue(dstExists.IsSuccess && dstExists.Data, "拷贝后目标文件应存在");
                Log($"目标文件存在: {dstExists.Data}, 通过");

                string copied = await ReadAllAsync(fs, dstFile);
                Assert.AreEqual(CopyContent, copied, "目标文件内容应与源文件写入内容一致");
                Log($"目标内容: \"{copied}\", 与源一致: {copied == CopyContent}, 通过");
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
        public async Task OperatorMoveFile_兜底移动_源删除且目标存在内容一致()
        {
            string testDir = CreateTestDir(out string[] segments);
            try
            {
                Log($"测试根目录: {testDir}");
                var factory = new VirtualFileSystemFactory();
                var fs = new LocalFileSystem();
                factory.RegisterProvider(fs);
                var op = factory.CreateOperator();

                Log("--- 准备 src/dst 目录与源文件 ---");
                var srcDirResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, "src"]));
                Assert.IsTrue(srcDirResult.IsSuccess, "获取 src 目录条目应成功");
                srcDirResult.DataImpossibleNull("src 目录条目");
                IVirtualDirectory srcDir = srcDirResult.Data;

                var dstDirResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. segments, "dst"]));
                Assert.IsTrue(dstDirResult.IsSuccess, "获取 dst 目录条目应成功");
                dstDirResult.DataImpossibleNull("dst 目录条目");
                IVirtualDirectory dstDir = dstDirResult.Data;

                Assert.IsTrue((await fs.CreateDirectoryAsync(srcDir)).IsSuccess, "创建 src 目录应成功");
                Assert.IsTrue((await fs.CreateDirectoryAsync(dstDir)).IsSuccess, "创建 dst 目录应成功");

                var moveSrcResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, "src", "move.txt"]));
                Assert.IsTrue(moveSrcResult.IsSuccess, "获取移动源文件条目应成功");
                moveSrcResult.DataImpossibleNull("移动源文件条目");
                IVirtualFile moveSrc = moveSrcResult.Data;

                var moveDstResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. segments, "dst", "move.txt"]));
                Assert.IsTrue(moveDstResult.IsSuccess, "获取移动目标文件条目应成功");
                moveDstResult.DataImpossibleNull("移动目标文件条目");
                IVirtualFile moveDst = moveDstResult.Data;

                await WriteContentAsync(fs, moveSrc, MoveContent);
                Log($"已写入移动源文件: \"{MoveContent}\"");

                Log("--- 操作器兜底移动 ---");
                var moveResult = await op.MoveFileAsync(moveSrc, moveDst);
                Assert.IsTrue(moveResult.IsSuccess, "操作器移动文件应成功");
                Log($"操作器移动结果: {moveResult}, 通过");

                var moveSrcExists = await fs.FileExistsAsync(moveSrc);
                Assert.IsFalse(moveSrcExists.Data, "移动后源文件应已被删除");
                Log($"源文件已删除: {!moveSrcExists.Data}, 通过");

                var moveDstExists = await fs.FileExistsAsync(moveDst);
                Assert.IsTrue(moveDstExists.Data, "移动后目标文件应存在");
                Log($"目标文件存在: {moveDstExists.Data}, 通过");

                string moved = await ReadAllAsync(fs, moveDst);
                Assert.AreEqual(MoveContent, moved, "移动后目标文件内容应与写入内容一致");
                Log($"目标内容: \"{moved}\", 与写入一致: {moved == MoveContent}, 通过");
            }
            finally
            {
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, recursive: true);
                }
            }
        }

        /// <summary>
        /// 测试用假传输实现: 所有操作直接返回成功并累计调用次数 (仅用于验证传输注册/解析, 不参与拷贝/移动)
        /// </summary>
        private sealed class FakeTransfer : IVirtualFileSystemTransfer
        {
            /// <summary>四个操作方法的累计调用次数</summary>
            public int CallCount { get; private set; }

            public ValueTask<IOperationResultEx> MoveFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return ValueTask.FromResult<IOperationResultEx>(OperationResultEx.Success);
            }

            public ValueTask<IOperationResultEx> MoveDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return ValueTask.FromResult<IOperationResultEx>(OperationResultEx.Success);
            }

            public ValueTask<IOperationResultEx> CopyFileAsync(IVirtualFile source, IVirtualFile target, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return ValueTask.FromResult<IOperationResultEx>(OperationResultEx.Success);
            }

            public ValueTask<IOperationResultEx> CopyDirectoryAsync(IVirtualDirectory source, IVirtualDirectory target, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return ValueTask.FromResult<IOperationResultEx>(OperationResultEx.Success);
            }
        }
    }
}
