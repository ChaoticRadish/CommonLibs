using System.Diagnostics.CodeAnalysis;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.RefParameter001
    /// 验证 ref 形参 + [NotNull] 的用法: 方法内对可能为 null 的引用参数做 ??= 初始化, 再向字典添加固定条目
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class RefParameterTest : UnitTestBase
    {
        [TestMethod]
        public void 传入null_方法内自动初始化字典并添加条目()
        {
            Log("--- 测试: ref 传入 null ---");
            Dictionary<string, string>? dic = null;

            myMethod(ref dic);

            Assert.IsNotNull(dic, "ref 传入 null 后, 方法内应通过 ??= [] 自动初始化为空字典");
            Assert.AreEqual(2, dic.Count, "方法内应添加 2 个条目");
            Assert.AreEqual("good", dic["Test001"], "Test001 的值应为 good");
            Assert.AreEqual("bad", dic["Test002"], "Test002 的值应为 bad");
            Log($"传入 null → 调用后 Count={dic.Count}, 内容=[{string.Join(", ", dic.Select(p => $"{p.Key}={p.Value}"))}], 通过");
        }

        [TestMethod]
        public void 传入非null_保留原有条目并追加新条目()
        {
            Log("--- 测试: ref 传入非 null ---");
            Dictionary<string, string>? dic = new()
            {
                { "测试1", "值1" },
                { "测试2", "值2" },
            };

            myMethod(ref dic);

            Assert.IsNotNull(dic, "传入非 null 时, 方法内不应重新初始化");
            Assert.AreEqual(4, dic.Count, "原有 2 个条目 + 新增 2 个条目 = 4 个条目");
            Assert.AreEqual("值1", dic["测试1"], "原有条目 测试1 应保留");
            Assert.AreEqual("值2", dic["测试2"], "原有条目 测试2 应保留");
            Assert.AreEqual("good", dic["Test001"], "新增条目 Test001 的值应为 good");
            Assert.AreEqual("bad", dic["Test002"], "新增条目 Test002 的值应为 bad");
            Log($"传入非 null → 调用后 Count={dic.Count}, 内容=[{string.Join(", ", dic.Select(p => $"{p.Key}={p.Value}"))}], 通过");
        }

        /// <summary>
        /// 复刻源测试的被测方法: [NotNull] 修饰 ref 形参, 方法内 ??= 初始化空字典, 再添加固定条目
        /// </summary>
        private void myMethod([NotNull] ref Dictionary<string, string>? dic)
        {
            dic ??= [];
            dic.Add("Test001", "good");
            dic.Add("Test002", "bad");
        }
    }
}
