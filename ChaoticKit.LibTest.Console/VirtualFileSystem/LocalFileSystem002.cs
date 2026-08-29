using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class LocalFileSystem002() : TestBase("测试 LocalFileSystem 目录操作与枚举")
    {
        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            string testDir = GetTestDir();
            var fs = new LocalFileSystem();

            // 模拟从配置文件读取路径配置: 由路径字符串创建目录条目作为相对源
            var baseDir = LocalDirectory.FromPath(testDir, "来自测试配置");

            WriteLine("== 创建多级目录 a/b/c ==");
            var dirCResult = await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b", "c"]));
            dirCResult.DataImpossibleNull();
            var dirC = dirCResult.Data;
            WritePair("创建 a/b/c", await fs.CreateDirectoryAsync(dirC!));
            var dirB = (await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a", "b"]))).Data;
            WritePair("创建 a/b", await fs.CreateDirectoryAsync(dirB!));
            var dirA = (await fs.GetDirectoryAsync(new VirtualFilePath(baseDir, ["a"]))).Data;
            WritePair("创建 a", await fs.CreateDirectoryAsync(dirA!));

            WriteLine("== 写入文件 a/f1.txt, a/f2.txt ==");
            var f1 = (await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "f1.txt"]))).Data;
            var f2 = (await fs.GetFileAsync(new VirtualFilePath(baseDir, ["a", "f2.txt"]))).Data;
            await WriteFileAsync(fs, f1!, "内容1");
            await WriteFileAsync(fs, f2!, "内容2");

            WriteEmptyLine();
            WriteLine("== 枚举目录 a 下的文件 ==");
            var filesResult = await fs.ListFilesAsync(dirA!);
            WritePair("枚举结果", filesResult.IsSuccess);
            foreach (var f in filesResult.Data ?? [])
            {
                WriteLine($"  - Name={f.Name}, FullPath={f.FullPath}, FileSystemType={f.FileSystemType}, Source={f.Source}");
            }

            WriteEmptyLine();
            WriteLine("== 枚举目录 a 下的子目录 ==");
            var dirsResult = await fs.ListDirectoriesAsync(dirA!);
            WritePair("枚举结果", dirsResult.IsSuccess);
            foreach (var d in dirsResult.Data ?? [])
            {
                WriteLine($"  - Name={d.Name}, FullPath={d.FullPath}, Paths=[{string.Join("/", d.Paths)}], FileSystemType={d.FileSystemType}");
            }
            WritePair("枚举到 b 目录", (dirsResult.Data ?? []).Any(d => d.Name == "b"));

            WriteEmptyLine();
            WriteLine("== 目录存在性 ==");
            var existsResult = await fs.DirectoryExistsAsync(dirC);
            WritePair("a/b/c 存在", existsResult.Data);

            WriteEmptyLine();
            WriteLine("== 递归删除目录 a ==");
            WritePair("删除 a (递归)", await fs.DeleteDirectoryAsync(dirA!, true));
            WritePair("删除后 a 存在", (await fs.DirectoryExistsAsync(dirA!)).Data);

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
                using var writer = new System.IO.StreamWriter(result.Data);
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }
    }
}
