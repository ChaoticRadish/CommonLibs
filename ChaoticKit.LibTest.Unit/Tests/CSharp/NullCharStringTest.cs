using ChaoticKit.Extensions;
using System.Linq;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.String001
    /// 含 \0 (0x00) 字符的字符串在 ChaoticKit.Extensions 扩展下的行为验证:
    /// - Length: 按字符计数, \0 也是 1 个字符
    /// - FullInfoString: string 属内置类型 (Type.GetTypeCode != Object), 走 ToString 分支,
    ///   原样返回字符串本身 (不带引号、不转义, 含 \0 时输出即裸 \0 字符)
    /// - ToHexString: 逐字节转 2 位大写十六进制, 多个字节以空格分隔
    /// 期望值均取自源控制台测试 (WritePair 直接打印值), 全部确定性数据
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class NullCharStringTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("\0", 1, "\0", "00")]
        [DataRow("a\0b", 3, "a\0b", "61 00 62")]
        [DataRow("\0\0", 2, "\0\0", "00 00")]
        [DataRow("", 0, "", "")]
        public void NullChar字符串_长度与扩展输出(string input, int expectedLength, string expectedFullInfo, string expectedHex)
        {
            Log($"--- 测试: 输入字符串字符数={input.Length} ---");

            Assert.AreEqual(expectedLength, input.Length, "含 \\0 字符的字符串 Length 应按字符数计数");

            string fullInfo = input.FullInfoString();
            Assert.AreEqual(expectedFullInfo, fullInfo, "FullInfoString 应原样返回字符串本身");
            Log($"FullInfoString = <{fullInfo}> (长度 {fullInfo.Length})");

            string hex = input.Select(i => (byte)i).ToHexString();
            Assert.AreEqual(expectedHex, hex, "逐字节转十六进制输出不匹配");
            Log($"ToHexString    = {hex}, 通过");
        }

        [TestMethod]
        public void 源场景_单Null字符字符串_扩展输出()
        {
            Log("--- 源测试场景: str = ((char)0).ToString() ---");
            string str = ((char)0).ToString();

            Log($"Length         = {str.Length}");
            Assert.AreEqual(1, str.Length, "单个 \\0 字符的字符串 Length 应为 1");

            string fullInfo = str.FullInfoString();
            Log($"FullInfoString = <{fullInfo}> (含 \\0 原样输出, 长度 {fullInfo.Length})");
            Assert.AreEqual("\0", fullInfo, "FullInfoString 对 string 应原样返回 (string 属内置类型走 ToString 分支)");

            string hex = str.Select(i => (byte)i).ToHexString();
            Log($"ToHexString    = {hex}");
            Assert.AreEqual("00", hex, "单个 0x00 字节应转为 2 位大写十六进制 \"00\"");
        }
    }
}
