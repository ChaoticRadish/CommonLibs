using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VirtualFileSystemFactory = ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class Transfer001() : TestBase("测试工厂注册与兜底传输串联")
    {
        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            string testDir = GetTestDir();
            var factory = new VirtualFileSystemFactory();
            var fs = new LocalFileSystem();
            factory.RegisterProvider(fs);

            WriteEmptyLine();
            WriteLine("== 实现注册与查询 ==");
            WritePair("GetProvider(Local) 非空", factory.GetProvider(FileSystemTypeConstants.Local) != null);
            WritePair("GetProvider(FTP) 为空", factory.GetProvider(FileSystemTypeConstants.Ftp) == null);
            WritePair("Providers 数量", factory.Providers.Count);
            WritePair("注销 Local", factory.UnregisterProvider(FileSystemTypeConstants.Local));
            WritePair("注销后 GetProvider 为空", factory.GetProvider(FileSystemTypeConstants.Local) == null);
            factory.RegisterProvider(fs);
            WritePair("重新注册 Local", factory.GetProvider(FileSystemTypeConstants.Local) != null);

            WriteEmptyLine();
            WriteLine("== 传输方案注册与解析 ==");
            var fallback = factory.GetTransfer(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local);
            WritePair("未注册时返回兜底 (非空)", fallback != null);
            WritePair("兜底类型", fallback!.GetType().Name);

            var fake = new FakeTransfer();
            factory.RegisterTransfer(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local, fake);
            var resolved = factory.GetTransfer(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local);
            WritePair("注册后返回优化实例", ReferenceEquals(resolved, fake));
            factory.UnregisterTransfer(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local);
            var fallback2 = factory.GetTransfer(FileSystemTypeConstants.Local, FileSystemTypeConstants.Local);
            WritePair("注销后返回兜底", ReferenceEquals(fallback2, fallback));

            WriteEmptyLine();
            WriteLine("== 操作器 + 兜底拷贝 (跨目录串联) ==");
            var op = factory.CreateOperator();
            var srcDir = (await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(Seg("src")))).Data;
            var dstDir = (await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(Seg("dst")))).Data;
            await fs.CreateDirectoryAsync(srcDir!);
            await fs.CreateDirectoryAsync(dstDir!);

            var srcFile = (await fs.GetFileAsync(VirtualFilePath.FromRoot(Seg("src", "data.txt")))).Data;
            var dstFile = (await fs.GetFileAsync(VirtualFilePath.FromRoot(Seg("dst", "data.txt")))).Data;
            var writeResult = await fs.OpenWriteAsync(srcFile!);
            if (writeResult.IsSuccess && writeResult.Data != null)
            {
                using (var writer = new StreamWriter(writeResult.Data))
                {
                    await writer.WriteAsync("拷贝测试内容");
                    await writer.FlushAsync();
                }
            }

            WritePair("操作器拷贝文件", await op.CopyFileAsync(srcFile!, dstFile!));
            WritePair("目标文件存在", (await fs.FileExistsAsync(dstFile!)).Data);
            var readResult = await fs.OpenReadAsync(dstFile!);
            if (readResult.IsSuccess && readResult.Data != null)
            {
                using var reader = new StreamReader(readResult.Data);
                WritePair("目标内容", await reader.ReadToEndAsync());
            }

            WriteEmptyLine();
            WriteLine("== 操作器 + 兜底移动 ==");
            var moveSrc = (await fs.GetFileAsync(VirtualFilePath.FromRoot(Seg("src", "move.txt")))).Data;
            var moveDst = (await fs.GetFileAsync(VirtualFilePath.FromRoot(Seg("dst", "move.txt")))).Data;
            var writeMove = await fs.OpenWriteAsync(moveSrc!);
            if (writeMove.IsSuccess && writeMove.Data != null)
            {
                using var w = new StreamWriter(writeMove.Data);
                await w.WriteAsync("移动测试内容");
                await w.FlushAsync();
            }
            WritePair("操作器移动文件", await op.MoveFileAsync(moveSrc!, moveDst!));
            WritePair("源文件已删除", !(await fs.FileExistsAsync(moveSrc!)).Data);
            WritePair("目标文件存在", (await fs.FileExistsAsync(moveDst!)).Data);

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            var root = (await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(Seg()))).Data;
            await fs.DeleteDirectoryAsync(root!, true);
            WriteLine("清理完成");
        }

        /// <summary>
        /// 测试目录路径段 + 追加段
        /// </summary>
        /// <param name="tail"></param>
        private string[] Seg(params string[] tail)
        {
            return [.. TestPathHelper.ToSegments(GetTestDir()), .. tail];
        }

        private class FakeTransfer : IVirtualFileSystemTransfer
        {
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
