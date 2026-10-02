using ChaoticKit.Web;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.Url002
    /// UrlHelper.SplitQuery / QueryEquals 查询参数字符串切分的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class UrlHelperSplitQueryTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("arg0=1&arg1=test&arg2=yes", "arg0=1|arg1=test|arg2=yes")]
        [DataRow("name=John%20Doe&city=New%20York", "name=John Doe|city=New York")]
        [DataRow("color=red&color=blue&color=green", "color=red|color=blue|color=green")]
        [DataRow("key1=&key2&key3=value", "key1=|key2=|key3=value")]
        [DataRow("search=hello%2Bworld%3F&price=100%24", "search=hello+world?|price=100$")]
        [DataRow("", "")]
        [DataRow(null, "")]
        [DataRow("q=search%20term&page=1&sort=name&order=asc&filter=category%3Abooks", "q=search term|page=1|sort=name|order=asc|filter=category:books")]
        [DataRow("flag&debug&verbose", "flag=|debug=|verbose=")]
        [DataRow("a=1&b&c=hello%20world&a=2&d=test", "a=1|b=|c=hello world|a=2|d=test")]
        [DataRow("key=value=extra", "key=value=extra")]
        [DataRow("key==value", "key==value")]
        [DataRow("key=value==", "key=value==")]
        [DataRow("=value", "=value")]
        [DataRow("key1=val=ue1&key2=val=ue=2", "key1=val=ue1|key2=val=ue=2")]
        [DataRow("a==b&c=d=e=f", "a==b|c=d=e=f")]
        [DataRow("&", "=|=")]
        [DataRow("&&&", "=|=|=|=")]
        [DataRow("&test1=1&", "=|test1=1|=")]
        [DataRow("test1=1&&test2=b", "test1=1|=|test2=b")]
        [DataRow("=", "=")]
        [DataRow("&test1=1&test2=b", "=|test1=1|test2=b")]
        [DataRow("test1=1&test2=b&", "test1=1|test2=b|=")]
        [DataRow("&&test1=1&&test2=b&&", "=|=|test1=1|=|test2=b|=|=")]
        [DataRow(" = & = ", " = | = ")]
        public void SplitQuery(string? input, string expectedJoined)
        {
            bool result = UrlHelper.SplitQuery(input, out List<KeyValuePair<string, string>>? output);
            Assert.IsTrue(result, $"输入 {(input ?? "<null>")} 应返回 true");
            Assert.IsNotNull(output, "输出不应为 null");

            string actual = string.Join("|", output!.Select(kv => $"{kv.Key}={kv.Value}"));
            Assert.AreEqual(expectedJoined, actual, $"输入 {(input ?? "<null>")} 的输出不匹配");

            Log($"SplitQuery 输入={(input ?? "<null>")}, 输出=[{string.Join(", ", output.Select(kv => $"{kv.Key}={kv.Value}"))}]");
        }

        [TestMethod]
        public void QueryEquals()
        {
            Log("--- 测试: QueryEquals 比较 ---");
            var q1 = new List<KeyValuePair<string, string>> { new("a", "1"), new("b", "2") };
            var q2 = new List<KeyValuePair<string, string>> { new("a", "1"), new("b", "2") };
            var q3 = new List<KeyValuePair<string, string>> { new("a", "1") };

            Assert.IsTrue(UrlHelper.QueryEquals(q1, q2), "相同参数应相等");
            Assert.IsFalse(UrlHelper.QueryEquals(q1, q3), "长度不同应不相等");
            Assert.IsTrue(UrlHelper.QueryEquals(null, null), "两个 null 应相等");
            Assert.IsFalse(UrlHelper.QueryEquals(q1, null), "null 与非 null 应不相等");
            Log("QueryEquals 全部通过");
        }
    }
}
