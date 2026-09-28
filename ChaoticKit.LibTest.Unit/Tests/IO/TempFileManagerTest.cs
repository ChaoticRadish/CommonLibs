using ChaoticKit.Extensions;
using ChaoticKit.IO;
using ChaoticKit.Streams;
using System.Text;

namespace ChaoticKit.LibTest.Unit.IO
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.IO.TempFile002
    /// TempFileManager + TempFileAllocatorBaseSize + OffsetWrapperStream 的协同验证:
    /// 片段按 Offset/Length 写入与回读校验、容量限制 (MustMaxFileSizeLimit / NewTempFileSizeLimit)、Dispose 释放
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 原控制台测试只有写入流程无断言, 本测试补充: 分段回读校验 / 越界写入截断 / 容量上限 / 换文件规则 / 释放行为。
    /// </remarks>
    [TestClass]
    public sealed class TempFileManagerTest : UnitTestBase
    {
        /// <summary>硬性最大文件尺寸限制 (与原测试一致)</summary>
        private const long MustMaxFileSizeLimit = 1024;
        /// <summary>单个临时文件内可分配的总长度限制 (与原测试一致)</summary>
        private const long NewTempFileSizeLimit = 100;

        #region 辅助方法

        /// <summary>在系统临时目录下自建唯一的测试目录</summary>
        private static string CreateTempDir()
        {
            return Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
        }

        /// <summary>删除测试目录 (含递归子项)</summary>
        private static void DeleteTempDir(string dir)
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        /// <summary>创建指定目录下的管理器与分配器 (固定容量限制)</summary>
        private static (TempFileManager Manager, TempFileAllocatorBaseSize Allocator) CreateFixture(string dir)
        {
            TempFileManager manager = new(dir);
            TempFileAllocatorBaseSize allocator = new(manager)
            {
                MustMaxFileSizeLimit = MustMaxFileSizeLimit,
                NewTempFileSizeLimit = NewTempFileSizeLimit,
            };
            return (manager, allocator);
        }

        /// <summary>把 payload 写入片段 (由 OffsetWrapperStream 按片段的 Offset/Length 定位, 超长部分会被截断)</summary>
        private static void WriteToSegment(TempFileSegment segment, byte[] payload)
        {
            using var writeStream = segment.TempFile.OpenWrite();
            using OffsetWrapperStream ows = new(writeStream, segment.Offset, segment.Length);
            ows.Seek(0, SeekOrigin.Begin);
            ows.Write(payload, 0, payload.Length);
            writeStream.Flush();
        }

        /// <summary>按片段的 Offset/Length 回读全部字节</summary>
        private static byte[] ReadFromSegment(TempFileSegment segment)
        {
            using var readStream = segment.TempFile.OpenRead();
            using OffsetWrapperStream ows = new(readStream, segment.Offset, segment.Length);
            byte[] buffer = new byte[(int)segment.Length];
            int total = 0;
            while (total < buffer.Length)
            {
                int n = ows.Read(buffer, total, buffer.Length - total);
                if (n <= 0) break;
                total += n;
            }
            return buffer.AsSpan(0, total).ToArray();
        }

        /// <summary>构建期望字节: 文本前缀 + 零填充到指定长度 (文本比长度长时截断)</summary>
        private static byte[] BuildExpected(string text, int length)
        {
            byte[] expected = new byte[length];
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            Array.Copy(textBytes, 0, expected, 0, Math.Min(textBytes.Length, length));
            return expected;
        }

        /// <summary>逐字节断言并输出差异定位信息</summary>
        private static void AssertBytesEqual(byte[] expected, byte[] actual, string context)
        {
            Assert.AreEqual(expected.Length, actual.Length, $"{context}: 读取长度不匹配, 期望 {expected.Length} 实际 {actual.Length}");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], $"{context}: 第 {i} 字节不匹配, 期望 0x{expected[i]:X2} 实际 0x{actual[i]:X2}");
            }
        }

        #endregion

        [TestMethod]
        public void 写入_按Offset与Length回读校验()
        {
            Log("--- 测试: 分段写入后按 Offset/Length 回读校验 ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    // 同文件内连续分配 3 个 30 长度片段 (0+30+30=90 ≤ 100, 不换文件)
                    TempFileSegment s1 = allocator.Allocate(30);
                    TempFileSegment s2 = allocator.Allocate(30);
                    TempFileSegment s3 = allocator.Allocate(30);
                    // 90+30 > 100, 应换新文件, 偏移从 0 重新开始
                    TempFileSegment s4 = allocator.Allocate(30);

                    Assert.AreEqual(0L, s1.Offset, "片段1 偏移应为 0");
                    Assert.AreEqual(30L, s1.Length, "片段1 长度应为 30");
                    Assert.AreEqual(30L, s2.Offset, "片段2 偏移应为 30");
                    Assert.AreEqual(60L, s3.Offset, "片段3 偏移应为 60");
                    Assert.AreEqual(0L, s4.Offset, "片段4 应在新文件内偏移 0");
                    Assert.AreNotEqual(s1.TempFile.FileDescription, s4.TempFile.FileDescription,
                        "片段4 应位于不同的临时文件");

                    // 满长度写入 (文本 + 零填充), 保证每个片段的窗口都被文件数据覆盖
                    WriteToSegment(s1, BuildExpected("11223344", 30));
                    WriteToSegment(s2, BuildExpected("55667788", 30));
                    WriteToSegment(s3, BuildExpected("44332211", 30));
                    WriteToSegment(s4, BuildExpected("7766554411", 30));

                    Log($"片段1: offset={s1.Offset} length={s1.Length} file={Path.GetFileName(s1.TempFile.FileDescription)}");
                    Log($"片段2: offset={s2.Offset} length={s2.Length} file={Path.GetFileName(s2.TempFile.FileDescription)}");
                    Log($"片段3: offset={s3.Offset} length={s3.Length} file={Path.GetFileName(s3.TempFile.FileDescription)}");
                    Log($"片段4: offset={s4.Offset} length={s4.Length} file={Path.GetFileName(s4.TempFile.FileDescription)}");

                    // 逐段回读: 文本落在片段起点, 其后为写入的零填充
                    AssertBytesEqual(BuildExpected("11223344", 30), ReadFromSegment(s1), "片段1");
                    AssertBytesEqual(BuildExpected("55667788", 30), ReadFromSegment(s2), "片段2");
                    AssertBytesEqual(BuildExpected("44332211", 30), ReadFromSegment(s3), "片段3");
                    AssertBytesEqual(BuildExpected("7766554411", 30), ReadFromSegment(s4), "片段4");

                    Log("4 个片段按 Offset/Length 回读全部一致, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [TestMethod]
        public void 越界写入_被截断不溢出()
        {
            Log("--- 测试: 写入超过片段 Length 的文本应被 OffsetWrapperStream 截断, 不得溢出到相邻片段 ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    TempFileSegment s5 = allocator.Allocate(5);
                    TempFileSegment s6 = allocator.Allocate(3);
                    Assert.AreEqual(0L, s5.Offset, "片段5 偏移应为 0");
                    Assert.AreEqual(5L, s6.Offset, "片段6 应紧接片段5 (偏移 5)");

                    // 8 字节文本写入 5/3 长度片段, 期望被截断
                    byte[] text8 = Encoding.ASCII.GetBytes("44332211");
                    WriteToSegment(s5, text8);
                    WriteToSegment(s6, text8);

                    // 回读: 片段5 只应有 5 字节 "44332", 片段6 只应有 3 字节 "443"
                    AssertBytesEqual(BuildExpected("44332211", 5), ReadFromSegment(s5), "片段5");
                    AssertBytesEqual(BuildExpected("44332211", 3), ReadFromSegment(s6), "片段6");

                    // 物理文件长度 = 5 + 3 = 8, 若片段5 的写入未被截断, 文件长度会变成 11
                    string filePath = s6.TempFile.FileDescription;
                    Assert.AreEqual(8L, new FileInfo(filePath).Length,
                        "文件长度应为 5+3=8, 说明越界写入被截断且未污染相邻片段");
                    Log($"截断校验: 片段5=\"44332\" 片段6=\"443\" 文件长度=8, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [DataTestMethod]
        [DataRow(1025L)]
        [DataRow(2000L)]
        [DataRow(999999L)]
        public void 容量限制_超过MustMaxFileSizeLimit抛出(long size)
        {
            Log($"--- 测试: Allocate({size}) 超过 MustMaxFileSizeLimit={MustMaxFileSizeLimit} 应抛出 ArgumentException ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    ArgumentException ex = Assert.ThrowsException<ArgumentException>(
                        () => allocator.Allocate(size),
                        $"申请 {size} 应因超过最大尺寸限制而抛出");
                    StringAssert.Contains(ex.Message, "已超过允许的最大尺寸",
                        $"异常消息应包含容量限制说明, 实际: {ex.Message}");
                    Log($"Allocate({size}) 抛出 ArgumentException: {ex.Message}, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [TestMethod]
        public void 容量限制_等于上限允许()
        {
            Log($"--- 测试: Allocate({MustMaxFileSizeLimit}) 恰好等于上限应允许 ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    TempFileSegment segment = allocator.Allocate(MustMaxFileSizeLimit);
                    Assert.AreEqual(MustMaxFileSizeLimit, segment.Length, "片段长度应等于申请值 1024");
                    Assert.AreEqual(0L, segment.Offset, "新文件内片段偏移应为 0");
                    Log($"Allocate(1024) 成功: offset={segment.Offset} length={segment.Length}, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [DataTestMethod]
        [DataRow("30,30,30,30")]
        [DataRow("60,60")]
        [DataRow("100,100")]
        [DataRow("3,3,3,3,3,3,3")]
        public void 按NewTempFileSizeLimit换文件(string sizesCsv)
        {
            Log($"--- 测试: 按 NewTempFileSizeLimit={NewTempFileSizeLimit} 换文件规则, sizes=[{sizesCsv}] ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    int[] sizes = sizesCsv.Split(',').Select(int.Parse).ToArray();
                    string? prevFile = null;
                    long prevOffset = 0;
                    long prevLength = 0;

                    for (int i = 0; i < sizes.Length; i++)
                    {
                        TempFileSegment segment = allocator.Allocate(sizes[i]);
                        Assert.AreEqual((long)sizes[i], segment.Length, $"片段 {i} 长度应等于申请值");

                        if (prevFile == null)
                        {
                            Assert.AreEqual(0L, segment.Offset, $"首个片段 {i} 偏移应为 0");
                        }
                        else if (segment.TempFile.FileDescription == prevFile)
                        {
                            Assert.AreEqual(prevOffset + prevLength, segment.Offset,
                                $"同文件内片段 {i} 应紧接前一片段之后");
                        }
                        else
                        {
                            Assert.AreEqual(0L, segment.Offset, $"换新文件后片段 {i} 偏移应重置为 0");
                        }

                        prevFile = segment.TempFile.FileDescription;
                        prevOffset = segment.Offset;
                        prevLength = segment.Length;
                        Log($"片段 #{i}: size={sizes[i]} offset={segment.Offset} length={segment.Length} file={Path.GetFileName(prevFile)}");
                    }
                    Log("换文件规则校验通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [TestMethod]
        public void Dispose释放_片段与文件生命周期()
        {
            Log("--- 测试: 片段 Dispose 释放行为与文件生命周期 ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    // 60+60 > 100, 两个片段各占一个临时文件, fileB 是当前文件
                    TempFileSegment segA = allocator.Allocate(60);
                    TempFileSegment segB = allocator.Allocate(60);
                    string pathA = segA.TempFile.FileDescription;
                    string pathB = segB.TempFile.FileDescription;
                    Assert.AreNotEqual(pathA, pathB, "两个 60 长度片段应位于不同临时文件");
                    Assert.IsTrue(File.Exists(pathA), "文件A 应存在");
                    Assert.IsTrue(File.Exists(pathB), "文件B 应存在");
                    Log($"文件A: {pathA}");
                    Log($"文件B: {pathB}");

                    // 释放非当前文件的最后一个片段 → 对应临时文件被删除
                    segA.Dispose();
                    Assert.IsFalse(File.Exists(pathA), "文件A 的片段全部释放后, 文件A 应被删除");
                    Log("segA.Dispose() 后文件A 已删除, 通过");

                    // 释放当前文件的片段 → 分配器保留当前文件, 文件B 不应被删除
                    segB.Dispose();
                    Assert.IsTrue(File.Exists(pathB), "当前文件的片段释放后, 文件B 应保留 (分配器持有当前文件)");
                    Log("segB.Dispose() 后文件B 仍保留 (当前文件), 通过");

                    // 分配器释放 → 其管理的全部临时文件被释放
                    allocator.Dispose();
                    Assert.IsFalse(File.Exists(pathB), "分配器释放后文件B 应被删除");
                    Log("allocator.Dispose() 后文件B 已删除, 通过");

                    // 管理器释放 → 目录内不应残留任何临时文件
                    manager.Dispose();
                    Assert.AreEqual(0, Directory.GetFiles(dir).Length, "全部释放后临时目录应被清空");
                    Log("manager.Dispose() 后临时目录已清空, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }

        [TestMethod]
        public void 原始流程冒烟_不抛异常()
        {
            Log("--- 测试: 复刻原控制台测试的完整写入流程 (16×30 + 7×3 + 20×5), 校验片段元数据不变式 ---");
            string dir = CreateTempDir();
            try
            {
                var (manager, allocator) = CreateFixture(dir);
                using (manager)
                using (allocator)
                {
                    // 与原 TempFile002 相同的写入序列: (尺寸, 文本, 是否立即释放片段)
                    var sequence = new List<(int Size, string Text, bool Dispose)>();
                    string[] texts30 =
                    [
                        "11223344", "11223344", "55667788", "44332211", "7766554411",
                        "55667788", "44332211", "55667788", "44332211", "55667788",
                        "44332211", "55667788", "44332211", "55667788", "44332211", "55667788",
                    ];
                    bool[] disposes30 =
                    [
                        true, false, true, false, false, true, false,
                        true, false, true, false, true, false, true, false, true,
                    ];
                    for (int i = 0; i < 16; i++) sequence.Add((30, texts30[i], disposes30[i]));
                    for (int i = 0; i < 7; i++) sequence.Add((3, "44332211", false));
                    foreach (int _ in 100.ForUntil(5)) sequence.Add((5, "44332211", false));

                    for (int index = 0; index < sequence.Count; index++)
                    {
                        (int size, string text, bool dispose) = sequence[index];
                        TempFileSegment segment = allocator.Allocate(size);

                        Assert.AreEqual((long)size, segment.Length, $"片段 {index} 长度应等于申请值 {size}");
                        Assert.IsTrue(segment.Offset >= 0 && segment.Offset + segment.Length <= NewTempFileSizeLimit,
                            $"片段 {index} 应完全落在单文件限制 {NewTempFileSizeLimit} 内, offset={segment.Offset} length={segment.Length}");

                        WriteToSegment(segment, Encoding.ASCII.GetBytes(text));
                        if (dispose) segment.Dispose();

                        if (index % 5 == 0 || index == sequence.Count - 1)
                        {
                            Log($"片段 #{index}: size={size} offset={segment.Offset} length={segment.Length} dispose={dispose}");
                        }
                    }

                    // 流程结束但尚未释放 → 临时目录中应仍有文件
                    Assert.IsTrue(Directory.GetFiles(dir).Length > 0, "释放前临时目录应仍有临时文件");
                    Log($"写入流程完成, 共 {sequence.Count} 个片段, 释放前目录文件数={Directory.GetFiles(dir).Length}");

                    // 显式释放分配器与管理器 → 目录清空
                    allocator.Dispose();
                    manager.Dispose();
                    Assert.AreEqual(0, Directory.GetFiles(dir).Length, "全部释放后临时目录应被清空");
                    Log("分配器与管理器释放后临时目录已清空, 通过");
                }
            }
            finally
            {
                DeleteTempDir(dir);
            }
        }
    }
}
