using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.IO
{
    internal class TempFile002() : TestBase("测试临时文件管理器与分配器")
    {
        protected override void RunImpl()
        {
            var logger = GetLevelLogger("测试流程");

            using TempFileManager manager = new(GetTestDir())
            {
                Logger = this.GetLevelLogger("管理器"),
            };
            using TempFileAllocatorBaseSize allocator = new(manager)
            {
                MustMaxFileSizeLimit = 1024,
                NewTempFileSizeLimit = 100,
                Logger = this.GetLevelLogger("分配器"),
            };
            testWrite(allocator.Allocate(30), "11223344", true);
            testWrite(allocator.Allocate(30), "11223344", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(30), "7766554411", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(30), "55667788", true);
            testWrite(allocator.Allocate(30), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);
            testWrite(allocator.Allocate(3), "44332211", false);

            foreach (int index in 100.ForUntil(5))
            {
                testWrite(allocator.Allocate(5), "44332211", false);
            }
            //testWrite(allocator.Allocate(2000), "7766554411", false);

            logger.Info("结束");
        }

        private void testWrite(TempFileSegment segment, string text, bool dispose)
        {
            using FileStream fs = File.Open(segment.Path, FileMode.Open);
            using OffsetWrapperStream ows = new OffsetWrapperStream(fs, segment.Offset, segment.Length);
            ows.Seek(0, SeekOrigin.Begin);
            ows.Write(Encoding.ASCII.GetBytes(text));

            if (dispose) segment.Dispose();
        }

    }
}
