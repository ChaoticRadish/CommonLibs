using System.Text.RegularExpressions;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.Regex001
    /// 验证"匹配标识符(变量名/方法名, 允许中文) + 可选参数列表"的自定义正则:
    /// 对每组固定输入断言整体匹配结果, 以及 identifier / args 两个捕获组是否成功及其取值。
    /// </summary>
    /// <remarks>
    /// 正则规则 (与源测试注释一致):
    /// 1. 标识符 [\p{L}][\p{L}0-9]*: 支持中文, 不考虑与关键字冲突、不考虑 @ 开头
    /// 2. 参数列表 (...) 内只支持数字 (如 123, 1.23)、简单字符串 (如 "hello")、字符 (如 'c')
    /// 核心检查: match.Groups["args"].Success 为真 => 方法调用; 为假 => 变量名/方法名
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class RegexPatternTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("", false, null, null)]
        [DataRow("()", false, null, null)]
        [DataRow("myVariable", true, "myVariable", null)]
        [DataRow("中文方法", true, "中文方法", null)]
        [DataRow("MyMethod()", true, "MyMethod", "()")]
        [DataRow("Calculate(123)", true, "Calculate", "(123)")]
        [DataRow("Print(\"hello world\")", true, "Print", "(\"hello world\")")]
        [DataRow("Add(1, 2.5)", true, "Add", "(1, 2.5)")]
        [DataRow("Log(\"start\", 1, 'c')", true, "Log", "(\"start\", 1, 'c')")]
        public void Match_标识符与参数_符合预期(string input, bool expectedSuccess, string? expectedIdentifier, string? expectedArgs)
        {
            Log($"--- 测试输入: [{input}] ---");

            string pattern = @"(?<identifier>[\p{L}][\p{L}0-9]*)(?<args>\((\s*(?:\d+(?:\.\d+)?|""[^""]*""|'[^']*')\s*(?:,\s*(?:\d+(?:\.\d+)?|""[^""]*""|'[^']*')\s*)*)?\))?";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);

            // 断言整体匹配结果
            Assert.AreEqual(expectedSuccess, match.Success, $"'{input}' 的整体匹配结果与预期不符");

            if (!expectedSuccess)
            {
                Log($"[不匹配] '{input}', 通过");
                return;
            }

            // 断言 identifier 捕获组取值
            Assert.IsNotNull(expectedIdentifier, "预期成功时必须有期望的标识符取值");
            Assert.AreEqual(expectedIdentifier, match.Groups["identifier"].Value, $"'{input}' 的标识符捕获组与预期不符");

            // 断言 args 捕获组: expectedArgs 非空 => 必须成功且取值相等; 为空 => 必须未成功 (仅变量名/方法名)
            bool expectedHasArgs = expectedArgs != null;
            Assert.AreEqual(expectedHasArgs, match.Groups["args"].Success, $"'{input}' 的 args 捕获组成功标志与预期不符");

            if (expectedHasArgs)
            {
                Assert.AreEqual(expectedArgs, match.Groups["args"].Value, $"'{input}' 的参数列表捕获组取值与预期不符");
                Log($"[方法调用] '{input}' -> 标识符: {match.Groups["identifier"].Value} 参数: {match.Groups["args"].Value}, 通过");
            }
            else
            {
                Log($"[变量名/方法名] '{input}' -> 标识符: {match.Groups["identifier"].Value}, 通过");
            }
        }
    }
}
