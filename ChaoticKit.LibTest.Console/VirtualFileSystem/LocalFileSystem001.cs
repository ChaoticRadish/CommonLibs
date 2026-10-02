using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class LocalFileSystem001() : TestBase("测试 LocalFileSystem 基本读写删查")
    {
        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            string testDir = GetTestDir();
            WriteLine("测试根目录: " + testDir);
            var fs = new LocalFileSystem();

            WriteEmptyLine();
            WriteLine("== 获取根目录条目 ==");
            var rootResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(TestPathHelper.ToSegments(testDir)));
            WritePair("结果", rootResult);
            rootResult.DataImpossibleNull();
            WritePair("根目录名", rootResult.Data.Name);
            WritePair("根路径段数量", rootResult.Data.Paths.Length);

            WriteEmptyLine();
            WriteLine("== 创建目录 dirA ==");
            var dirAResult = await fs.GetDirectoryAsync(VirtualFilePath.FromRoot([.. TestPathHelper.ToSegments(testDir), "dirA"]));
            dirAResult.DataImpossibleNull();
            var createDirResult = await fs.CreateDirectoryAsync(dirAResult.Data);
            WritePair("创建结果", createDirResult);

            WriteEmptyLine();
            WriteLine("== 获取文件条目并写入 ==");
            var fileResult = await fs.GetFileAsync(VirtualFilePath.FromRoot([.. TestPathHelper.ToSegments(testDir), "dirA", "hello.txt"]));
            fileResult.DataImpossibleNull();
            WritePair("文件条目名", fileResult.Data.Name);
            WritePair("所属目录名", fileResult.Data.Directory.Name);
            var writeResult = await fs.OpenWriteAsync(fileResult.Data);
            WritePair("打开写入流", writeResult.IsSuccess);
            if (writeResult.IsSuccess && writeResult.Data != null)
            {
                using (var writer = new StreamWriter(writeResult.Data))
                {
                    await writer.WriteAsync("Hello VirtualFileSystem!");
                    await writer.FlushAsync();
                }
            }

            WriteEmptyLine();
            WriteLine("== 存在性检查 ==");
            var existsResult = await fs.FileExistsAsync(fileResult.Data);
            WritePair("文件存在", existsResult.Data);

            WriteEmptyLine();
            WriteLine("== 读取比对 ==");
            var readResult = await fs.OpenReadAsync(fileResult.Data);
            WritePair("打开读取流", readResult.IsSuccess);
            if (readResult.IsSuccess && readResult.Data != null)
            {
                using var reader = new StreamReader(readResult.Data);
                string content = await reader.ReadToEndAsync();
                WritePair("读取内容", content);
                WritePair("内容一致", content == "Hello VirtualFileSystem!");
            }

            WriteEmptyLine();
            WriteLine("== 删除文件 ==");
            var deleteResult = await fs.DeleteFileAsync(fileResult.Data);
            WritePair("删除结果", deleteResult);
            var existsAfter = await fs.FileExistsAsync(fileResult.Data);
            WritePair("删除后文件存在", existsAfter.Data);

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            await fs.DeleteDirectoryAsync(rootResult.Data, true);
            WriteLine("清理完成");
        }
    }
}
