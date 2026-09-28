using System.Text.RegularExpressions;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.Unescape001
    /// 验证 System.Text.RegularExpressions.Regex.Unescape 对 \n / \uXXXX / \xXX 等转义序列的反转义结果
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 语义说明 (net8.0): \u 后取 4 位十六进制数字; \x 后只取 2 位十六进制数字,
    /// 多余的字符按普通文本保留, 如 \x6767 反转义为 'g' + "67"; \x88 反转义为 U+0088。
    /// </remarks>
    [TestClass]
    public sealed class RegexUnescapeTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("\\n", "\n")]
        [DataRow("\\u1234", "\u1234")]
        [DataRow("\\uA001", "\uA001")]
        [DataRow("\\u005C", "\u005C")]
        [DataRow("\\u0080", "\u0080")]
        [DataRow("\\u0061", "a")]
        [DataRow("\\u0063", "c")]
        [DataRow("\\u0065", "e")]
        [DataRow("\\x88", "\u0088")]
        [DataRow("\\x67", "g")]
        [DataRow("\\x6767", "g67")]
        public void Unescape_反转义结果(string pattern, string expected)
        {
            string actual = Regex.Unescape(pattern);

            Assert.AreEqual(expected, actual, $"Regex.Unescape(\"{pattern}\") 结果不匹配");

            string codepoints = string.Join(",", actual.Select(c => ((int)c).ToString("X4")));
            Log($"Regex.Unescape(\"{pattern}\") => 长度 {actual.Length}, 码点 [{codepoints}], 通过");
        }
    }
}
