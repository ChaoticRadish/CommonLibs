using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Streams;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Stream
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Stream.File001 (确定性核心部分)
    /// OffsetWrapperStream 偏移包装流与 FileSegment 片段读取的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 原控制台测试包含大量顺序编辑演示(写入随机文本+多次 Seek 改写), 迁移为固定内容的核心行为断言。
    /// </remarks>
    [TestClass]
    public sealed class OffsetWrapperStreamTest : UnitTestBase
    {
        /// <summary>固定测试内容 (20 字符, 每个字符即一个字节的 ASCII)</summary>
        private const string Content = "0123456789ABCDEFGHIJ";

        private static string CreateTestFile()
        {
            string file = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N") + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, Content, Encoding.ASCII);
            return file;
        }

        [TestMethod]
        public void 构造属性_起点终点与限制长度()
        {
            Log("--- 测试: OffsetWrapperStream 构造属性 ---");
            string file = CreateTestFile();
            try
            {
                using FileStream fs = File.Open(file, FileMode.Open);
                using OffsetWrapperStream ows = new(fs, 5, 5);

                Assert.AreEqual(5, ows.WrapperStart, "WrapperStart 应为 5");
                Assert.AreEqual(10, ows.WrapperEnd, "WrapperEnd 应为 5+5=10");
                Assert.AreEqual(5, ows.Length, "Length 应为限制长度 5");
                Assert.AreEqual(0, ows.Position, "实例化后 Position 应为 0 (源流未 Seek 时按包装偏移折算)");
                Log($"WrapperStart={ows.WrapperStart}, WrapperEnd={ows.WrapperEnd}, Length={ows.Length}, Position={ows.Position}, 通过");
            }
            finally { File.Delete(file); }
        }

        [TestMethod]
        public void 读取_包装段内容与源对应子串一致()
        {
            Log("--- 测试: 读取包装段 ---");
            string file = CreateTestFile();
            try
            {
                using FileStream fs = File.Open(file, FileMode.Open);
                using OffsetWrapperStream ows = new(fs, 5, 5);
                ows.Seek(0, SeekOrigin.Begin);

                using StreamReader sr = new(ows, Encoding.ASCII);
                string read = sr.ReadToEnd();
                Assert.AreEqual("56789", read, "读取的包装段应为源 5..9 位置的子串");
                Log($"读取到: [{read}], 通过");
            }
            finally { File.Delete(file); }
        }

        [TestMethod]
        public void 读取_无长度限制时读至源末尾()
        {
            Log("--- 测试: 无长度限制 (limitLength=null) ---");
            string file = CreateTestFile();
            try
            {
                using FileStream fs = File.Open(file, FileMode.Open);
                using OffsetWrapperStream ows = new(fs, 5, null);
                ows.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(Content.Length - 5, ows.Length, "无限制时 Length 应为源长-偏移");
                using StreamReader sr = new(ows, Encoding.ASCII);
                string read = sr.ReadToEnd();
                Assert.AreEqual("56789ABCDEFGHIJ", read, "应读取偏移之后全部内容");
                Log($"Length={ows.Length}, 读取到: [{read}], 通过");
            }
            finally { File.Delete(file); }
        }

        [TestMethod]
        public void Seek_调整位置后读取对应片段()
        {
            Log("--- 测试: Seek 调整包装内位置 ---");
            string file = CreateTestFile();
            try
            {
                using FileStream fs = File.Open(file, FileMode.Open);
                using OffsetWrapperStream ows = new(fs, 0, 10);

                ows.Seek(3, SeekOrigin.Begin);
                Assert.AreEqual(3, ows.Position, "Seek(3) 后 Position 应为 3");
                byte[] buffer = new byte[4];
                int n = ows.Read(buffer, 0, 4);
                Assert.AreEqual(4, n, "应读取 4 字节");
                Assert.AreEqual("3456", Encoding.ASCII.GetString(buffer), "读取内容应为源 3..6 位置");
                Log($"Seek(3) 后读取: [{Encoding.ASCII.GetString(buffer)}], 通过");
            }
            finally { File.Delete(file); }
        }

        [TestMethod]
        public void FileSegment_片段读取()
        {
            Log("--- 测试: FileSegment.OpenStream 片段读取 ---");
            string file = CreateTestFile();
            try
            {
                using var stream = new FileSegment
                {
                    FullName = file,
                    Start = 10,
                    Length = 10,
                }.OpenStream();

                using StreamReader sr = new(stream, Encoding.ASCII);
                string read = sr.ReadToEnd();
                Assert.AreEqual("ABCDEFGHIJ", read, "FileSegment 应读取源 10..19 位置的片段");
                Log($"FileSegment 读取到: [{read}], 通过");
            }
            finally { File.Delete(file); }
        }
    }
}
