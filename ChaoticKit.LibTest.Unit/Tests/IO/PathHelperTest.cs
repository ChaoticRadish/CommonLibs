using ChaoticKit.IO;

namespace ChaoticKit.LibTest.Unit.IO
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.IO.Path001
    /// PathHelper 绝对/相对路径转换与路径类型判断的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。路径预期值基于 Windows。
    /// </remarks>
    [TestClass]
    public sealed class PathHelperTest : UnitTestBase
    {
        private const string AbsPath = "C:\\A\\B\\C.mf";

        [DataTestMethod]
        [DataRow("..\\..\\asd.psd", "C:\\asd.psd")]
        [DataRow("..\\..\\..\\..\\asd.psd", "C:\\asd.psd")]
        [DataRow(".\\asd.psd", "C:\\A\\B\\asd.psd")]
        [DataRow("asd.psd", "C:\\A\\B\\asd.psd")]
        [DataRow(".\\Q\\asd.psd", "C:\\A\\B\\Q\\asd.psd")]
        [DataRow("Q\\asd.psd", "C:\\A\\B\\Q\\asd.psd")]
        public void GetAbsolutePath(string relatively, string expected)
        {
            string result = PathHelper.GetAbsolutePath(AbsPath, relatively);
            Assert.AreEqual(expected, result, $"绝对路径 {AbsPath} + 相对路径 {relatively} 的结果不匹配");
            Log($"GetAbsolutePath({AbsPath}, {relatively}) = {result}, 通过");
        }

        [TestMethod]
        public void GetRelativelyPath_往返还原()
        {
            Log("--- 测试: GetRelativelyPath 往返还原 ---");
            string abs = "C:\\A\\B\\C.mf";
            string relatively = "Q\\asd.psd";

            string result = PathHelper.GetAbsolutePath(abs, relatively);
            // 还原出的相对路径再转绝对路径, 应得到相同结果
            string back = PathHelper.GetAbsolutePath(abs, PathHelper.GetRelativelyPath(abs, result));
            Assert.AreEqual(result, back, "相对路径还原后应一致");
            Log($"往返一致: {result}, 通过");
        }

        [DataTestMethod]
        [DataRow("C:\\A\\B\\C.mf", true)]
        [DataRow("C:\\", true)]
        [DataRow("C:", true)]
        [DataRow("/usr/bin", true)]
        [DataRow("asd.psd", false)]
        [DataRow(".\\asd.psd", false)]
        public void IsAbsolutePathSimpleCheck(string path, bool expected)
        {
            bool result = PathHelper.IsAbsolutePathSimpleCheck(path);
            Assert.AreEqual(expected, result, $"IsAbsolutePathSimpleCheck({path}) 结果不匹配");
            Log($"IsAbsolutePathSimpleCheck({path}) = {result}, 通过");
        }
    }
}
