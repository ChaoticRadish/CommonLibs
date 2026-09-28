using ChaoticKit.String;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.Escape001 (确定性部分, 去掉随机演示)
    /// EscapeHelper 转义/反转义往返一致性的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class EscapeHelperTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("asdqwaiofujewqg12564321asasdasdqwrf")]
        [DataRow("12312364asd56")]
        [DataRow("")]
        [DataRow("abcdef")]
        [DataRow("a")]
        [DataRow("123")]
        public void AddEscape_RemoveEscape_往返一致(string input)
        {
            string escaped = EscapeHelper.AddEscape(input, 'a', '1', '2', '3', '2', '1');
            string restored = EscapeHelper.RemoveEscape(escaped, 'a');

            Assert.AreEqual(input, restored, "AddEscape 后再 RemoveEscape 应还原原始字符串");
            Log($"输入=[{input}], 转义后=[{escaped}], 还原后=[{restored}], 通过");
        }

        [TestMethod]
        public void AddEscape_需转义字符前插入转义字符()
        {
            Log("--- 测试: AddEscape 插入转义字符 ---");
            // 转义字符 'a', 需转义字符 '1','2','3' (含重复与转义字符自身)
            string escaped = EscapeHelper.AddEscape("a1b2c3", 'a', '1', '2', '3', '2', '1');
            Assert.AreEqual("aa a1 b a2 c a3".Replace(" ", ""), escaped, "每个需转义字符前都应插入转义字符 'a'");
            Log($"AddEscape(\"a1b2c3\", 'a', '1','2','3') = {escaped}, 通过");
        }

        [TestMethod]
        public void Ergodic_遍历识别被转义字符()
        {
            Log("--- 测试: Ergodic 遍历 ---");
            var result = new List<(char c, bool beEscape)>();
            EscapeHelper.Ergodic("a1a2a3", 'a', (c, beEscape) => result.Add((c, beEscape)));

            Assert.AreEqual(3, result.Count, "应遍历出 3 个被转义字符");
            Assert.IsTrue(result.All(r => r.beEscape), "所有字符都应是转义后的字符");
            CollectionAssert.AreEqual(new[] { '1', '2', '3' }, result.Select(r => r.c).ToArray(), "被转义字符序列不匹配");
            Log($"Ergodic(\"a1a2a3\", 'a') 遍历出: {string.Join(",", result.Select(r => r.c))}, 全部 beEscape=true, 通过");
        }

        [TestMethod]
        public void Ergodic_末尾转义字符抛异常()
        {
            Log("--- 测试: Ergodic 末尾转义字符抛异常 ---");
            Assert.ThrowsException<ArgumentException>(
                () => EscapeHelper.Ergodic("abc a".Replace(" ", ""), 'a', (c, b) => { }),
                "转义字符后无其他字符应抛 ArgumentException");
            Log("末尾孤立转义字符抛 ArgumentException, 通过");
        }
    }
}
