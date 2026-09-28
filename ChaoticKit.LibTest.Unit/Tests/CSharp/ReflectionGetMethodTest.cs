using System.Reflection;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Reflection001 (反射测试, 反射取得方法信息)
    /// 覆盖 Type.GetMethod 的几种重载解析行为:
    /// - 无重载方法可直接按名称获取并调用
    /// - 有重载方法时, 无参 GetMethod(name) 抛 AmbiguousMatchException
    /// - 用参数类型数组 GetMethod(name, types) 可精确定位重载
    /// - 空类型数组 GetMethod(name, []) 定位无参重载
    /// 原控制台测试的 ClassTest 依赖 TestBase 打印, 迁移后改为写入 Output 列表供断言。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// GetMethod 重载解析: 名称唯一时直接获取; 名称存在重载时须显式传参数类型数组, 否则抛 AmbiguousMatchException。
    /// </remarks>
    [TestClass]
    public sealed class ReflectionGetMethodTest : UnitTestBase
    {
        /// <summary>
        /// 被测类: 含一个无重载方法 Method1, 以及一对重载 Method2() / Method2(string, string)
        /// </summary>
        public sealed class ClassTest
        {
            /// <summary>
            /// 方法被调用时写入的输出记录 (替代原控制台测试的 TestBase 打印)
            /// </summary>
            public List<string> Output { get; } = new();

            public void Method1()
            {
                Output.Add("Method1()");
            }

            public void Method2()
            {
                Output.Add("Method2()");
            }

            public void Method2(string x, string y)
            {
                Output.Add($"Method2({x}, {y})");
            }
        }

        [TestMethod]
        public void GetMethod_无重载方法_按名称获取并调用()
        {
            Log("--- 测试: GetMethod(nameof(Method1)) 无重载, 应直接获取 ---");

            MethodInfo? method = typeof(ClassTest).GetMethod(nameof(ClassTest.Method1));

            Assert.IsNotNull(method, "Method1 无重载, GetMethod 应按名称获取到");
            Assert.AreEqual("Method1", method.Name, "获取到的方法名应为 Method1");
            Assert.AreEqual(typeof(ClassTest), method.DeclaringType, "DeclaringType 应为 ClassTest");
            Assert.AreEqual(MemberTypes.Method, method.MemberType, "MemberType 应为 Method");
            Log($"method.Name={method.Name}, DeclaringType={method.DeclaringType}, MemberType={method.MemberType}");

            var target = new ClassTest();
            method.Invoke(target, null);

            Assert.AreEqual(1, target.Output.Count, "调用 Method1 应产生一条输出");
            Assert.AreEqual("Method1()", target.Output[0], "Method1 的输出内容不匹配");
            Log($"调用 Method1() 成功, 输出={target.Output[0]}, 通过");
        }

        [TestMethod]
        public void GetMethod_无参重载_抛AmbiguousMatchException()
        {
            Log("--- 测试: GetMethod(nameof(Method2)) 有重载, 应抛 AmbiguousMatchException ---");

            Assert.ThrowsException<AmbiguousMatchException>(
                () => typeof(ClassTest).GetMethod(nameof(ClassTest.Method2)),
                "Method2 存在多个重载, 无参 GetMethod 应抛 AmbiguousMatchException");
            Log("无参 GetMethod 遇到重载抛 AmbiguousMatchException, 通过");
        }

        [TestMethod]
        public void GetMethod_指定参数类型_获取重载并调用()
        {
            Log("--- 测试: GetMethod(Method2, [string, string]) 应定位 Method2(string, string) 重载 ---");

            MethodInfo? method = typeof(ClassTest).GetMethod(nameof(ClassTest.Method2), new[] { typeof(string), typeof(string) });

            Assert.IsNotNull(method, "用参数类型 [string, string] 应获取到 Method2(string, string) 重载");
            Assert.AreEqual("Method2", method.Name, "获取到的方法名应为 Method2");
            Assert.AreEqual(typeof(ClassTest), method.DeclaringType, "DeclaringType 应为 ClassTest");
            Assert.AreEqual(MemberTypes.Method, method.MemberType, "MemberType 应为 Method");
            Log($"method.Name={method.Name}, DeclaringType={method.DeclaringType}, MemberType={method.MemberType}");

            var target = new ClassTest();
            method.Invoke(target, new object[] { "aaa", "bbb" });

            Assert.AreEqual(1, target.Output.Count, "调用 Method2(string, string) 应产生一条输出");
            Assert.AreEqual("Method2(aaa, bbb)", target.Output[0], "Method2(string, string) 的输出内容不匹配");
            Log($"调用 Method2(\"aaa\", \"bbb\") 成功, 输出={target.Output[0]}, 通过");
        }

        [TestMethod]
        public void GetMethod_空类型数组_获取无参重载并调用()
        {
            Log("--- 测试: GetMethod(Method2, []) 空类型数组应定位 Method2() 无参重载 ---");

            MethodInfo? method = typeof(ClassTest).GetMethod(nameof(ClassTest.Method2), Array.Empty<Type>());

            Assert.IsNotNull(method, "空类型数组应获取到 Method2() 无参重载");
            Assert.AreEqual("Method2", method.Name, "获取到的方法名应为 Method2");
            Assert.AreEqual(typeof(ClassTest), method.DeclaringType, "DeclaringType 应为 ClassTest");
            Assert.AreEqual(MemberTypes.Method, method.MemberType, "MemberType 应为 Method");
            Log($"method.Name={method.Name}, DeclaringType={method.DeclaringType}, MemberType={method.MemberType}");

            var target = new ClassTest();
            method.Invoke(target, null);

            Assert.AreEqual(1, target.Output.Count, "调用 Method2() 应产生一条输出");
            Assert.AreEqual("Method2()", target.Output[0], "Method2() 的输出内容不匹配");
            Log($"调用 Method2() 成功, 输出={target.Output[0]}, 通过");
        }
    }
}
