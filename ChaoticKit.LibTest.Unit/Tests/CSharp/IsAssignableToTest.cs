namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Reflection002
    /// Type.IsAssignableTo 与 Type.GetInterfaces().Contains 判断"类型是否实现某接口"的差异验证
    /// </summary>
    /// <remarks>
    /// 差异要点: IsAssignableTo 含"类型自身等于目标"的判定 (identity), 对接口类型 typeof(ITest) 本身返回 true;
    /// 而 GetInterfaces() 不含接口自身, 对 typeof(ITest) 返回 false。
    /// 两者对继承链均返回 true: ClassB : ClassA : ITest 时 GetInterfaces() 返回全部继承接口, 故仍含 ITest。
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class IsAssignableToTest : UnitTestBase
    {
        private interface ITest { }

        private class ClassA : ITest { }

        private sealed class ClassB : ClassA { }

        private sealed class ClassC : ClassA, ITest { }

        [DataTestMethod]
        [DataRow(typeof(ITest), true)]
        [DataRow(typeof(ClassA), true)]
        [DataRow(typeof(ClassB), true)]
        [DataRow(typeof(ClassC), true)]
        public void IsAssignableTo_判断是否实现接口(Type type, bool expected)
        {
            Log($"--- 测试: {type.Name}.IsAssignableTo(typeof(ITest)) ---");
            bool actual = type.IsAssignableTo(typeof(ITest));

            Assert.AreEqual(expected, actual, $"{type.Name}.IsAssignableTo(typeof(ITest)) 结果应为 {expected}");
            Log($"{type.Name}.IsAssignableTo(typeof(ITest)) = {actual}, 通过");
        }

        [DataTestMethod]
        [DataRow(typeof(ITest), false)]
        [DataRow(typeof(ClassA), true)]
        [DataRow(typeof(ClassB), true)]
        [DataRow(typeof(ClassC), true)]
        public void GetInterfaces_Contains_判断是否实现接口(Type type, bool expected)
        {
            Log($"--- 测试: {type.Name}.GetInterfaces().Contains(typeof(ITest)) ---");
            bool actual = type.GetInterfaces().Contains(typeof(ITest));

            Assert.AreEqual(expected, actual, $"{type.Name}.GetInterfaces().Contains(typeof(ITest)) 结果应为 {expected}");
            Log($"{type.Name}.GetInterfaces().Contains(typeof(ITest)) = {actual}, 通过");
        }

        [TestMethod]
        public void IsAssignableTo_对接口自身为True_而GetInterfaces_不含自身()
        {
            Log("--- 测试: 接口自身判断的差异 ---");
            bool viaAssignableTo = typeof(ITest).IsAssignableTo(typeof(ITest));
            bool viaGetInterfaces = typeof(ITest).GetInterfaces().Contains(typeof(ITest));
            Log($"typeof(ITest).IsAssignableTo(typeof(ITest)) = {viaAssignableTo}");
            Log($"typeof(ITest).GetInterfaces().Contains(typeof(ITest)) = {viaGetInterfaces}");

            Assert.IsTrue(viaAssignableTo, "IsAssignableTo 对接口自身应返回 true (identity 判定)");
            Assert.IsFalse(viaGetInterfaces, "GetInterfaces() 不含接口自身, 应返回 false");
            Log("差异点验证通过: 接口自身仅 IsAssignableTo 判定为已实现");
        }
    }
}
