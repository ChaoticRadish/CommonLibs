using ChaoticKit.Web;

namespace ChaoticKit.LibTest.Unit.Text
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Text.Url001
    /// UrlHelper.SplitPath / PathEquals 访问路径切分的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class UrlHelperSplitPathTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("/", true, "")]
        [DataRow("/user", true, "user")]
        [DataRow("/user/my", true, "user|my")]
        [DataRow("/user/my/test", true, "user|my|test")]
        [DataRow("/user%2Fdir", true, "user/dir")]
        [DataRow("/user%20name", true, "user name")]
        [DataRow("/path%2Bwith%2Bplus", true, "path+with+plus")]
        [DataRow("", true, "")]
        [DataRow(null, true, "")]
        [DataRow("invalid", false, "<null>")]
        [DataRow("//", true, "|")]
        [DataRow("/user//test", true, "user||test")]
        [DataRow("/user/", true, "user|")]
        public void SplitPath(string? input, bool expectedResult, string expectedJoined)
        {
            bool result = UrlHelper.SplitPath(input, out string[]? output);
            Assert.AreEqual(expectedResult, result, $"输入 {(input ?? "<null>")} 的返回值不匹配");

            if (expectedResult)
            {
                string actual = string.Join("|", output ?? []);
                Assert.AreEqual(expectedJoined, actual, $"输入 {(input ?? "<null>")} 的输出不匹配");
            }
            else
            {
                Assert.IsNull(output, "返回 false 时输出应为 null");
            }

            Log($"SplitPath 输入={(input ?? "<null>")}, 结果={result}, 输出=[{string.Join(", ", output ?? Array.Empty<string>())}]");
        }

        [TestMethod]
        public void PathEquals()
        {
            Log("--- 测试: PathEquals 比较 ---");
            Assert.IsTrue(UrlHelper.PathEquals(["user", "my"], ["user", "my"]), "相同数组应相等");
            Assert.IsFalse(UrlHelper.PathEquals(["user", "my"], ["user", "my", "x"]), "长度不同应不相等");
            Assert.IsFalse(UrlHelper.PathEquals(["user", "my"], ["user", "yo"]), "内容不同应不相等");
            Assert.IsTrue(UrlHelper.PathEquals(null, null), "两个 null 应相等");
            Assert.IsFalse(UrlHelper.PathEquals(null, ["user"]), "null 与非 null 应不相等");
            Log("PathEquals 全部通过");
        }
    }
}
