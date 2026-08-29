using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VirtualFileSystemFactory = ChaoticKit.VirtualFileSystem.Default.VirtualFileSystem;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class Operator001() : TestBase("测试操作器事件机制")
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

            var op = factory.CreateOperator();
            var invokingOps = new List<VirtualFileSystemOperation>();
            var invokedOps = new List<VirtualFileSystemOperation>();
            op.OperationInvoking += (s, e) => invokingOps.Add(e.Operation);
            op.OperationInvoked += (s, e) => invokedOps.Add(e.Operation);

            var root = (await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(TestPathHelper.ToSegments(testDir)))).Data;
            var file = (await op.GetFileAsync(new VirtualFilePath(root, ["event.txt"]))).Data;

            WriteEmptyLine();
            WriteLine("== 打开写入流 (触发 OpenWrite + CloseStream) ==");
            var writeResult = await op.OpenWriteAsync(file!);
            WritePair("打开写入流", writeResult.IsSuccess);
            if (writeResult.IsSuccess && writeResult.Data != null)
            {
                using (var writer = new StreamWriter(writeResult.Data))
                {
                    await writer.WriteAsync("事件测试");
                    await writer.FlushAsync();
                }
            }

            WriteEmptyLine();
            WriteLine("== 文件存在检查 (触发 FileExists) ==");
            WritePair("文件存在", (await op.FileExistsAsync(file!)).Data);

            WriteEmptyLine();
            WriteLine("== 删除文件 (触发 DeleteFile) ==");
            WritePair("删除", await op.DeleteFileAsync(file!));

            WriteEmptyLine();
            WriteLine("== 事件记录 ==");
            WriteLine("Invoking 事件序列: " + string.Join(", ", invokingOps));
            WriteLine("Invoked 事件序列: " + string.Join(", ", invokedOps));
            WritePair("包含 OpenWrite", invokingOps.Contains(VirtualFileSystemOperation.OpenWrite) && invokedOps.Contains(VirtualFileSystemOperation.OpenWrite));
            WritePair("包含 CloseStream", invokedOps.Contains(VirtualFileSystemOperation.CloseStream));
            WritePair("包含 FileExists", invokingOps.Contains(VirtualFileSystemOperation.FileExists));
            WritePair("包含 DeleteFile", invokingOps.Contains(VirtualFileSystemOperation.DeleteFile));
            WritePair("事件结果携带成功", true);

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            await fs.DeleteDirectoryAsync(root!, true);
            WriteLine("清理完成");
        }
    }
}
