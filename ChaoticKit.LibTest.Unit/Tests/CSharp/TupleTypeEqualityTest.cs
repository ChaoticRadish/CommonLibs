namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Equal001 (验证类型比较)
    /// 元组类型 (typeof) 相等比较: 元组元素名是否参与运行时类型同一性
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class TupleTypeEqualityTest : UnitTestBase
    {
        [TestMethod]
        public void 命名元组类型_等于_未命名元组类型()
        {
            Log("--- 测试: typeof((string str, int v)) == typeof((string, int)) ---");
            bool equal = typeof((string str, int v)) == typeof((string, int));

            Assert.IsTrue(equal, "元组元素名不参与运行时类型同一性, 命名元组与未命名元组应相等");
            Assert.AreEqual(typeof(ValueTuple<string, int>), typeof((string str, int v)),
                "命名元组的运行时类型应为 ValueTuple<string,int>");
            Log($"typeof((string str, int v)) == typeof((string, int)) => {equal}, 通过");
        }

        [TestMethod]
        public void 命名元组类型_等于_不同元素名命名元组类型()
        {
            Log("--- 测试: typeof((string str, int v)) == typeof((string rts, int myInt)) ---");
            bool equal = typeof((string str, int v)) == typeof((string rts, int myInt));

            Assert.IsTrue(equal, "元素名不同不影响类型同一性, 两个命名元组应相等");
            Assert.AreEqual(typeof(ValueTuple<string, int>), typeof((string rts, int myInt)),
                "不同元素名命名元组的运行时类型也应均为 ValueTuple<string,int>");
            Log($"typeof((string str, int v)) == typeof((string rts, int myInt)) => {equal}, 通过");
        }
    }
}
