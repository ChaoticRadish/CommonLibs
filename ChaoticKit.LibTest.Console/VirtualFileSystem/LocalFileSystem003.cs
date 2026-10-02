using ChaoticKit.Data.Struct;
using ChaoticKit.VirtualFileSystem;
using ChaoticKit.VirtualFileSystem.Default;
using System;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.VirtualFileSystem
{
    internal class LocalFileSystem003() : TestBase("测试 LocalFileSystem 失败场景 (返回失败结果而非抛异常)")
    {
        protected override void RunImpl()
        {
        }

        protected override async Task RunImplAsync()
        {
            string testDir = GetTestDir();
            var fs = new LocalFileSystem();
            var root = (await fs.GetDirectoryAsync(VirtualFilePath.FromRoot(TestPathHelper.ToSegments(testDir)))).Data;

            WriteEmptyLine();
            WriteLine("== 读取不存在的文件 ==");
            var missingFile = (await fs.GetFileAsync(VirtualFilePath.FromRoot([.. TestPathHelper.ToSegments(testDir), "not_exists.txt"]))).Data;
            var readResult = await fs.OpenReadAsync(missingFile!);
            WritePair("结果是失败", readResult.IsFailure);
            WritePair("携带异常", readResult.HasException);

            WriteEmptyLine();
            WriteLine("== 非法路径段 (含分隔符) ==");
            try
            {
                var badResult = await fs.GetFileAsync(new VirtualFilePath(null, ["a/b.txt"]));
                WritePair("非法段返回失败", badResult.IsFailure);
            }
            catch (Exception ex)
            {
                WritePair("抛出异常(不应发生)", ex.Message);
            }

            WriteEmptyLine();
            WriteLine("== 路径段 '.' 忽略与 '..' 回溯 ==");
            var dotResult = await fs.GetDirectoryAsync(new VirtualFilePath(null, ["a", ".", "b"]));
            WritePair("'.' 忽略后路径段", string.Join("/", dotResult.Data?.Paths ?? []));
            WritePair("结果是 a/b", string.Join("/", dotResult.Data?.Paths ?? []) == "a/b");

            var dotDotResult = await fs.GetDirectoryAsync(new VirtualFilePath(null, ["a", "..", "b"]));
            WritePair("'..' 回溯后路径段", string.Join("/", dotDotResult.Data?.Paths ?? []));
            WritePair("结果是 b", string.Join("/", dotDotResult.Data?.Paths ?? []) == "b");

            WriteEmptyLine();
            WriteLine("== 清理 ==");
            await fs.DeleteDirectoryAsync(root!, true);
            WriteLine("清理完成");
        }
    }
}
