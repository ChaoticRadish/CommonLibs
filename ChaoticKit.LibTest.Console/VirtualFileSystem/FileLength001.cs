using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualFileSystemFactory = ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    /// <summary>
    /// 测试文件长度获取: 实现直接获取与操作器读流计算两条路径
    /// </summary>
    internal class FileLength001() : TestBase("测试文件长度获取 (直接获取 / 读流计算)")
    {
        /// <summary>
        /// 声明不支持直接获取长度的本机实现: 用于验证读流计算的兜底路径
        /// </summary>
        private sealed class NoDirectLengthLocalFileSystem : LocalFileSystem
        {
            public override bool SupportGetFileLength(IVirtualFile file) => false;
        }

        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            const int fileSize = 1024 * 3 + 7;  // 非整块大小, 覆盖分块读取的累加逻辑
            string testDir = GetTestDir();

            var factory = new VirtualFileSystemFactory();
            var fs = new LocalFileSystem();
            factory.RegisterProvider(fs);
            var op = factory.CreateOperator();

            var root = LocalDirectory.FromPath(testDir, "来自测试配置");
            var file = root.GetFile("length.bin");
            var missingFile = root.GetFile("missing.bin");

            WriteLine("== 准备测试文件 ==");
            byte[] payload = new byte[fileSize];
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i % 251);
            }
            await WriteBytesAsync(fs, file, payload);
            WritePair("预期长度", fileSize);
            WritePair("文件存在", (await fs.FileExistsAsync(file)).Data);

            WriteEmptyLine();
            WriteLine("== 实现能力声明 ==");
            WritePair("LocalFileSystem.SupportGetFileLength", fs.SupportGetFileLength(file));
            WritePair("操作器.SupportGetFileLength", op.SupportGetFileLength(file));

            WriteEmptyLine();
            WriteLine("== 直接获取 (本机实现) ==");
            var directResult = await fs.GetFileLengthAsync(file);
            WritePair("成功", directResult.IsSuccess);
            WritePair("实际长度", directResult.Data);
            WritePair("长度一致", directResult.Data == fileSize);

            WriteEmptyLine();
            WriteLine("== 操作器获取 (实现支持直接获取) ==");
            var opInvoking = new List<VirtualFileSystemOperation>();
            var opInvoked = new List<VirtualFileSystemOperation>();
            op.OperationInvoking += (_, e) => opInvoking.Add(e.Operation);
            op.OperationInvoked += (_, e) => opInvoked.Add(e.Operation);
            var opResult = await op.GetFileLengthAsync(file);
            WritePair("成功", opResult.IsSuccess);
            WritePair("实际长度", opResult.Data);
            WritePair("长度一致", opResult.Data == fileSize);
            WritePair("触发 GetFileLength 事件", opInvoking.Contains(VirtualFileSystemOperation.GetFileLength) && opInvoked.Contains(VirtualFileSystemOperation.GetFileLength));
            WritePair("未触发 OpenRead (直接获取不读内容)", !opInvoking.Contains(VirtualFileSystemOperation.OpenRead));

            WriteEmptyLine();
            WriteLine("== 操作器获取 (实现不支持直接获取 → 读流计算) ==");
            var noDirectFactory = new VirtualFileSystemFactory();
            var fsNoDirect = new NoDirectLengthLocalFileSystem();
            noDirectFactory.RegisterProvider(fsNoDirect);
            var opNoDirect = noDirectFactory.CreateOperator();
            var noDirectInvoking = new List<VirtualFileSystemOperation>();
            opNoDirect.OperationInvoking += (_, e) => noDirectInvoking.Add(e.Operation);
            WritePair("noDirect.SupportGetFileLength", fsNoDirect.SupportGetFileLength(file));
            var fallbackResult = await opNoDirect.GetFileLengthAsync(file);
            WritePair("成功", fallbackResult.IsSuccess);
            WritePair("实际长度", fallbackResult.Data);
            WritePair("长度一致", fallbackResult.Data == fileSize);
            WritePair("触发 OpenRead (读流计算)", noDirectInvoking.Contains(VirtualFileSystemOperation.OpenRead));

            WriteEmptyLine();
            WriteLine("== 实现基类的兜底实现 (直接调用不支持直接获取的实现) ==");
            var baseFallbackResult = await fsNoDirect.GetFileLengthAsync(file);
            WritePair("成功", baseFallbackResult.IsSuccess);
            WritePair("实际长度", baseFallbackResult.Data);
            WritePair("长度一致", baseFallbackResult.Data == fileSize);

            WriteEmptyLine();
            WriteLine("== 文件不存在 ==");
            var missingResult = await op.GetFileLengthAsync(missingFile);
            WritePair("失败 (应发生)", missingResult.IsFailure);
            WritePair("失败原因", (object?)missingResult.FailureReason);

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            await fs.DeleteDirectoryAsync(root, true);
            WritePair("目录已清理", !(await fs.DirectoryExistsAsync(root)).Data);
        }

        private static async Task WriteBytesAsync(LocalFileSystem fs, IVirtualFile file, byte[] payload)
        {
            var openResult = await fs.OpenWriteAsync(file);
            openResult.DataImpossibleNull("文件写入流");
            using var stream = openResult.Data;
            await stream.WriteAsync(payload);
        }
    }
}
