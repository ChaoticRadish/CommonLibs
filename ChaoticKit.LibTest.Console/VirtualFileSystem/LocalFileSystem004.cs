using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class LocalFileSystem004() : TestBase("测试 LocalFileSystem 清理目录 (ClearDirectoryAsync)")
    {
        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            string testDir = GetTestDir();
            var fs = new LocalFileSystem();
            var baseDir = LocalDirectory.FromPath(testDir, "来自测试配置");

            WriteLine("== 准备目录结构与文件 (base/a/b + f1/f2/f3) ==");
            var dirA = (await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]))).Data;
            var dirB = (await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b"]))).Data;
            await fs.CreateDirectoryAsync(dirA!);
            await fs.CreateDirectoryAsync(dirB!);
            var f1 = (await fs.GetFileAsync(new VirtualFilePath(baseDir, ["f1.txt"]))).Data;
            var f2 = (await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "f2.txt"]))).Data;
            var f3 = (await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "b", "f3.txt"]))).Data;
            await WriteFileAsync(fs, f1!, "1");
            await WriteFileAsync(fs, f2!, "2");
            await WriteFileAsync(fs, f3!, "3");
            WritePair("准备完成", true);

            WriteEmptyLine();
            WriteLine("== 清理: Files | Recursive (保留目录结构, 删除所有文件) ==");
            WritePair("清理结果", await fs.ClearDirectoryAsync(baseDir, VirtualFileSystemClearOption.Files | VirtualFileSystemClearOption.Recursive));
            WritePair("f1 已删除", !(await fs.FileExistsAsync(f1!)).Data);
            WritePair("f2 已删除", !(await fs.FileExistsAsync(f2!)).Data);
            WritePair("f3 已删除", !(await fs.FileExistsAsync(f3!)).Data);
            WritePair("目录 a 保留", (await fs.DirectoryExistsAsync(dirA!)).Data);
            WritePair("目录 a/b 保留", (await fs.DirectoryExistsAsync(dirB!)).Data);

            WriteEmptyLine();
            WriteLine("== 清理: Directories (删除子目录, 不含子级) ==");
            WritePair("清理结果", await fs.ClearDirectoryAsync(baseDir, VirtualFileSystemClearOption.Directories));
            WritePair("目录 a 已删除", !(await fs.DirectoryExistsAsync(dirA!)).Data);

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            await fs.DeleteDirectoryAsync(baseDir, true);
            WriteLine("清理完成");
        }

        private static async Task WriteFileAsync(LocalFileSystem fs, IVirtualFile file, string content)
        {
            var result = await fs.OpenWriteAsync(file);
            if (result.IsSuccess && result.Data != null)
            {
                using var writer = new StreamWriter(result.Data);
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }
    }
}
