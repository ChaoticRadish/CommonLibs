using ChaoticKit.Data.Structure.Value;
using ChaoticKit.IO;

namespace ChaoticKit.LibTest.Unit.IO
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.IO.TempFile001
    /// TempFileHelper 临时文件创建/写入/回读全文, 以及 FileSegment 文件段读取的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。需要临时目录时自建并清理, 不依赖运行环境绝对路径; 已移除源测试中的 Thread.Sleep。
    /// </remarks>
    [TestClass]
    public sealed class TempFileHelperTest : UnitTestBase
    {
        /// <summary>写入的第一行固定数据 (与源控制台测试一致)</summary>
        private const string Line1 = "asdasdasdasdasdqweqawsdazfase";
        /// <summary>写入的第二行固定数据 (与源控制台测试一致)</summary>
        private const string Line2 = "123123213";

        [TestMethod]
        public void NewTempFile_默认临时目录_写入后回读全文一致()
        {
            Log("--- 测试: NewTempFile 默认临时目录, 写入后回读全文一致 ---");

            string? prevDir = TempFileHelper.CustomTempFileDir;
            TempFileHelper.CustomTempFileDir = null;
            string? tempFilePath = null;
            try
            {
                using (TempFileHelper.TempFile tempFile = TempFileHelper.NewTempFile())
                {
                    tempFilePath = tempFile.Path;
                    Log($"取得临时文件: {tempFile.Path}");

                    Assert.AreEqual(Path.GetTempPath(), tempFile.TempFileDir, "默认情况下临时文件应创建在系统临时目录");
                    Assert.IsTrue(File.Exists(tempFile.Path), "临时文件应已创建");
                    Assert.AreEqual(0L, new FileInfo(tempFile.Path).Length, "新创建的临时文件应为空");

                    string fullText = WriteTwoLinesAndReadBack(tempFile);

                    string expected = Line1 + Environment.NewLine + Line2 + Environment.NewLine;
                    Assert.AreEqual(expected, fullText, "写入后回读的全文应与两次 WriteLine 的内容一致");
                    Log($"临时文件内回读全文: {fullText.Replace(Environment.NewLine, "\\r\\n")}, 通过");
                }
            }
            finally
            {
                TempFileHelper.CustomTempFileDir = prevDir;
                // TempFile.Dispose 会异步删除临时文件, 这里尽力兜底清理
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* 可能已被 TempFile.Dispose 异步删除 */ }
                }
            }
        }

        [TestMethod]
        public void NewTempFile_自定义临时目录_写入回读与FileSegment读段()
        {
            Log("--- 测试: NewTempFile 自定义临时目录 + FileSegment 读段 ---");

            string? prevDir = TempFileHelper.CustomTempFileDir;
            string tempDir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string? tempFilePath = null;
            TempFileHelper.CustomTempFileDir = tempDir;
            try
            {
                using (TempFileHelper.TempFile tempFile = TempFileHelper.NewTempFile())
                {
                    tempFilePath = tempFile.Path;
                    Log($"取得临时文件: {tempFile.Path}");

                    Assert.AreEqual(tempDir, tempFile.TempFileDir, "临时文件应创建在自定义目录");
                    Assert.AreEqual(tempDir, Path.GetDirectoryName(tempFile.Path), "临时文件所在目录应为自定义目录");
                    Assert.IsTrue(File.Exists(tempFile.Path), "临时文件应已创建");
                    Assert.AreEqual(0L, new FileInfo(tempFile.Path).Length, "新创建的临时文件应为空");

                    string fullText = WriteTwoLinesAndReadBack(tempFile);
                    Log($"临时文件内回读全文: {fullText.Replace(Environment.NewLine, "\\r\\n")}");

                    // FileSegment: 以整个文件为一段, 读出内容应与写入的全文一致
                    FileSegment segment = new(tempFile.Path);
                    Assert.AreEqual(0u, segment.Start, "FileSegment 起点应为 0");
                    Assert.AreEqual((uint)new FileInfo(tempFile.Path).Length, segment.Length, "FileSegment 长度应等于文件长度");
                    Assert.AreEqual(Path.GetFullPath(tempFile.Path), segment.FullName, "FileSegment 全名应与临时文件完整路径一致");
                    Log($"FileSegment 信息: {segment}");

                    using (System.IO.Stream segStream = segment.OpenStream())
                    using (StreamReader segReader = new(segStream))
                    {
                        string segmentText = segReader.ReadToEnd();
                        Assert.AreEqual(fullText, segmentText, "FileSegment 读出的段内容应等于写入的全文");
                        Log($"FileSegment 读出内容: {segmentText.Replace(Environment.NewLine, "\\r\\n")}, 通过");
                    }
                }
            }
            finally
            {
                TempFileHelper.CustomTempFileDir = prevDir;
                // 自建临时目录在 finally 中清理 (TempFile.Dispose 会异步删除临时文件, 这里尽力兜底)
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* 可能已被 TempFile.Dispose 异步删除 */ }
                }
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, recursive: true); } catch { /* 尽力清理 */ }
                }
            }
        }

        /// <summary>
        /// 向临时文件写入两行固定数据 (与源控制台测试相同的写入序列, 无 Thread.Sleep),
        /// 显式关闭写入器后重新打开流回读全文
        /// </summary>
        private string WriteTwoLinesAndReadBack(TempFileHelper.TempFile tempFile)
        {
            using (FileStream fs1 = tempFile.OpenStream())
            {
                StreamWriter sw = new(fs1);
                Log($"写入第一行: {Line1}");
                sw.WriteLine(Line1);
                sw.Flush();
                Log($"Flush 后文件流位置: {fs1.Position}, 长度: {fs1.Length}");

                Log($"写入第二行: {Line2}");
                sw.WriteLine(Line2);
                Log($"关闭流写入器前文件流位置: {fs1.Position}, 长度: {fs1.Length}");
                sw.Close();
                Log("关闭流写入器");
            }

            using (FileStream fs2 = tempFile.OpenStream())
            {
                using StreamReader sr = new(fs2);
                string fullText = sr.ReadToEnd();
                Log($"回读流位置: {fs2.Position}, 长度: {fs2.Length}");
                return fullText;
            }
        }
    }
}
