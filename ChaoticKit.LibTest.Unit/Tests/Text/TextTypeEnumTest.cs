using ChaoticKit;
using ChaoticKit.Enums;
using ChaoticKit.Extensions.EnumType;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.TextTypeEnum001 (确定性部分)
    /// 验证 TextTypeEnum 各枚举值的默认 MIME 类型值 (DefaultContentTypeValue)
    /// 以及 EnumHelper.ForEach 对枚举类型的遍历行为
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class TextTypeEnumTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow(TextTypeEnum.Plain, "text/plain")]
        [DataRow(TextTypeEnum.Json, "application/json")]
        [DataRow(TextTypeEnum.Xml, "application/xml")]
        [DataRow(TextTypeEnum.Html, "text/html")]
        [DataRow(TextTypeEnum.Csv, "text/csv")]
        [DataRow(TextTypeEnum.Markdown, "text/markdown")]
        public void DefaultContentTypeValue_各枚举值返回默认MIME类型(TextTypeEnum textType, string expected)
        {
            string actual = textType.DefaultContentTypeValue();
            Assert.AreEqual(expected, actual, $"{textType} 的默认 MIME 类型不匹配");
            Log($"{textType} => {actual}, 通过");
        }

        [TestMethod]
        public void DefaultContentTypeValue_未映射枚举值抛InvalidDataException()
        {
            Log("--- 测试: 未映射枚举值抛 InvalidDataException ---");
            TextTypeEnum unmapped = (TextTypeEnum)999;
            Assert.ThrowsException<InvalidDataException>(
                () => { unmapped.DefaultContentTypeValue(); },
                "未映射默认 MIME 类型的枚举值应抛 InvalidDataException");
            Log($"{(int)unmapped} 抛 InvalidDataException, 通过");
        }

        [TestMethod]
        public void EnumHelper_ForEach_遍历全部枚举值并计算默认值()
        {
            Log("--- 测试: EnumHelper.ForEach 遍历 TextTypeEnum ---");
            var pairs = new List<(TextTypeEnum type, string value)>();
            EnumHelper.ForEach<TextTypeEnum>(t => pairs.Add((t, t.DefaultContentTypeValue())));

            Assert.AreEqual(6, pairs.Count, "应遍历出 6 个枚举值");

            var expectedTypes = new[]
            {
                TextTypeEnum.Plain, TextTypeEnum.Json, TextTypeEnum.Xml,
                TextTypeEnum.Html, TextTypeEnum.Csv, TextTypeEnum.Markdown,
            };
            CollectionAssert.AreEqual(expectedTypes, pairs.Select(p => p.type).ToArray(), "遍历到的枚举值集合/顺序应与声明一致");

            foreach (var pair in pairs)
            {
                string expectedValue = pair.type.DefaultContentTypeValue();
                Assert.AreEqual(expectedValue, pair.value, $"{pair.type} 默认 MIME 类型不匹配");
                Log($"{pair.type} => {pair.value}");
            }
            Log("ForEach 遍历 6 个枚举值全部计算成功, 通过");
        }

        [TestMethod]
        public void EnumHelper_ForEach_空Action不抛异常()
        {
            Log("--- 测试: EnumHelper.ForEach 传入 null Action 不抛异常 ---");
            EnumHelper.ForEach<TextTypeEnum>(null!);
            Log("null Action 遍历无异常, 通过");
        }
    }
}
