using ChaoticKit.Extensions;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Stream
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Stream.Char001
    /// 验证 Stream.AsCharEnumerable() (ChaoticKit.Extensions) 将流按 UTF-8 读取为字符集合:
    /// 字符数正确、与原文往返一致, 以及流位置对重复枚举的影响 (纯内存 MemoryStream + 固定文本)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class CharEnumerableTest : UnitTestBase
    {
        private const string SourceText = "你好，世界！";
        private const int ExpectedCharCount = 6;

        [TestMethod]
        public void AsCharEnumerable_字符数与Seek后往返字符串()
        {
            Log("--- 测试: Count() 字符数 + Seek(0) 后往返字符串 ---");
            byte[] byteArray = Encoding.UTF8.GetBytes(SourceText);
            using MemoryStream memoryStream = new(byteArray);

            int count = memoryStream.AsCharEnumerable().Count();
            Log($"Count() = {count}");
            Assert.AreEqual(ExpectedCharCount, count, $"字符数应等于 {ExpectedCharCount}");

            memoryStream.Seek(0, SeekOrigin.Begin);
            string roundTrip = new(memoryStream.AsCharEnumerable().ToArray());
            Log($"往返字符串 = \"{roundTrip}\"");
            Assert.AreEqual(SourceText, roundTrip, "Seek(0) 后往返字符串应与原文一致");
        }

        [TestMethod]
        public void AsCharEnumerable_枚举消耗后不Seek的往返()
        {
            Log("--- 测试: 先 Count() 消耗流, 不 Seek 再次枚举 (位置相关行为) ---");
            byte[] byteArray = Encoding.UTF8.GetBytes(SourceText);
            using MemoryStream memoryStream = new(byteArray);

            int count = memoryStream.AsCharEnumerable().Count();
            Log($"Count() = {count}");
            Assert.AreEqual(ExpectedCharCount, count, $"字符数应等于 {ExpectedCharCount}");

            // 库行为快照: 枚举消耗流后当前位置在末尾, 不 Seek 的第二次枚举读到空字符串
            // (见 StringHelper.AsEnumerable 备注: 枚举器重复使用会再次读取流, 需修改流的当前位置)
            string roundTrip = new(memoryStream.AsCharEnumerable().ToArray());
            Log($"不 Seek 的往返字符串 = \"{roundTrip}\"");
            Assert.AreEqual(string.Empty, roundTrip, "不 Seek 时第二次枚举从流当前位置(末尾)读取, 应得到空字符串 (行为快照, 待人工确认是否符合预期)");
        }
    }
}
