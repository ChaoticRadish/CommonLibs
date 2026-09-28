using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.Generics
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Generics.EnumBoxUp001
    /// 枚举值作为泛型参数时的装箱行为验证, 以及 GEnumTest 静态构造函数对泛型类型的枚举约束验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// GEnumTest&lt;T&gt; 静态构造用 ChaoticKit.Extensions.TypeExtensions.IsEnum(true) 校验泛型类型:
    /// 允许枚举与可空枚举 (Nullable&lt;T&gt; 底层为枚举); 非枚举类型实例化会触发静态构造异常,
    /// 被 CLR 包装为外层 TypeInitializationException, 内部异常为 InvalidOperationException。
    /// </remarks>
    [TestClass]
    public sealed class EnumBoxUpTest : UnitTestBase
    {
        private class Test001
        {
            public int x;

            public override string ToString()
            {
                return $"Test001 => x: {x}";
            }
        }

        private enum Test002
        {
            AAA,
            BBB,
            CCC,
        }

        private class GTest<T>
        {
            public T? Value { get; set; }

            public override string ToString()
            {
                return Value == null ? "<null>" : Value.ToString() ?? "Empty";
            }
        }

        private class GEnumTest<T> : GTest<T?>
        {
            static GEnumTest()
            {
                Type type = typeof(T);
                if (!type.IsEnum(true))
                {
                    throw new InvalidOperationException($"泛型类型 {nameof(T)} 必须是枚举类型");
                }
            }
        }

        private class GEnumTest2 : GEnumTest<Test002?>
        {
        }

        [TestMethod]
        public void GTest_引用类型装箱与ToString()
        {
            Log("--- 测试: GTest<Test001> 引用类型值装箱 ---");
            var t1 = new GTest<Test001>
            {
                Value = new(),
            };
            var t2 = new GTest<Test001>
            {
                Value = null,
            };

            Assert.IsNotNull(t1.Value, "赋了实例的 Value 不应为 null");
            Assert.AreEqual("Test001 => x: 0", t1.ToString(), "装箱引用类型 ToString 应输出实例描述");
            Assert.IsNull(t2.Value, "赋 null 的 Value 应为 null");
            Assert.AreEqual("<null>", t2.ToString(), "null 值 ToString 应输出 <null>");
            Log($"t1.ToString() = {t1}, t2.ToString() = {t2}, 通过");
        }

        [TestMethod]
        public void GEnumTest_枚举类型装箱与ToString()
        {
            Log("--- 测试: GEnumTest<Test002> 枚举类型装箱 ---");
            var t3 = new GEnumTest<Test002>
            {
                Value = Test002.AAA,
            };

            Assert.IsTrue(t3.Value == Test002.AAA, "枚举值应装箱保存为 AAA");
            Assert.AreEqual("AAA", t3.ToString(), "枚举装箱后 ToString 应输出成员名 AAA");
            Log($"t3.Value = {t3.Value}, t3.ToString() = {t3}, 通过");
        }

        [TestMethod]
        public void GEnumTest_可空枚举装箱与ToString()
        {
            Log("--- 测试: GEnumTest<Test002?> 可空枚举装箱 ---");
            var t4 = new GEnumTest<Test002?>
            {
                Value = null,
            };

            Assert.IsFalse(t4.Value.HasValue, "可空枚举赋 null 应无值");
            Assert.AreEqual("<null>", t4.ToString(), "无值可空枚举 ToString 应输出 <null>");
            Log($"t4.Value.HasValue = {t4.Value.HasValue}, t4.ToString() = {t4}, 通过");
        }

        [TestMethod]
        public void GEnumTest2_继承可空枚举装箱与ToString()
        {
            Log("--- 测试: GEnumTest2 (继承 GEnumTest<Test002?>) 装箱 ---");
            var t5 = new GEnumTest2
            {
                Value = null,
            };
            var t6 = new GEnumTest2
            {
                Value = Test002.BBB,
            };

            Assert.IsFalse(t5.Value.HasValue, "t5 赋 null 应无值");
            Assert.AreEqual("<null>", t5.ToString(), "t5 无值 ToString 应输出 <null>");
            Assert.IsTrue(t6.Value == Test002.BBB, "t6 枚举值应装箱保存为 BBB");
            Assert.AreEqual("BBB", t6.ToString(), "t6 枚举装箱后 ToString 应输出成员名 BBB");
            Log($"t5.ToString() = {t5}, t6.ToString() = {t6}, 通过");
        }

        [TestMethod]
        public void GEnumTest_非枚举类型触发静态构造异常()
        {
            Log("--- 测试: GEnumTest<Test001> 非枚举类型应触发静态构造异常 ---");
            // GEnumTest<Test001> 静态构造校验 typeof(Test001).IsEnum(true) = false,
            // 抛出 InvalidOperationException, 首次实例化时被 CLR 包装为 TypeInitializationException
            TypeInitializationException ex = Assert.ThrowsException<TypeInitializationException>(
                () => new GEnumTest<Test001>(),
                "非枚举类型实例化 GEnumTest 应抛 TypeInitializationException");

            Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException),
                "内部异常应为 InvalidOperationException, 实际: " + ex.InnerException?.GetType().Name);
            StringAssert.Contains(ex.InnerException?.Message ?? string.Empty, "必须是枚举类型",
                "内部异常消息应说明泛型类型必须是枚举类型");
            Log($"外层异常 = {ex.GetType().Name}, 内部异常 = {ex.InnerException?.GetType().Name} ({ex.InnerException?.Message}), 通过");
        }
    }
}
