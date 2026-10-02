using ChaoticKit.Data.Struct;
using System.Reflection;

namespace ChaoticKit.LibTest.Unit.DataWrapper
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataWrapper.NeedInitObject001
    /// NeedInitObject&lt;T&gt; 包装/初始化状态 + SetValueToProperty (ChaoticKit.Data.Struct) 的行为验证
    /// </summary>
    /// <remarks>
    /// - 未赋值时读取 Value 抛 InvalidOperationException ("未赋值! ")
    /// - 已赋值后再次赋值抛 InvalidOperationException ("已具有初始值, 无法再次赋值! ")
    /// - 值类型 T 时 HasValue 恒为 true: 无约束 T 的 T? 即 T (非 Nullable&lt;T&gt;), 默认值 0 != null 恒成立
    /// - SetValueToProperty 走反射取属性, 但测的是库方法对 INeedInitObject 包装器的赋值逻辑
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class NeedInitObjectTest : UnitTestBase
    {
        /// <summary>
        /// 被测样例类: 属性为 NeedInitObject&lt;string&gt;, 属性初始化器自动创建未赋值包装器 (对应源测试的 TestClass)
        /// </summary>
        private class TestClass
        {
            public NeedInitObject<string> TestA { get; } = new();

            public override string ToString()
            {
                return $"TestClass {TestA}";
            }
        }

        [TestMethod]
        public void StringWrapper_未赋值_初始状态()
        {
            Log("--- 测试: NeedInitObject<string> 未赋值初始状态 (对应 test1) ---");
            var wrapper = new NeedInitObject<string>();

            Assert.IsFalse(wrapper.Inited, "未赋值时 Inited 应为 false");
            Assert.IsFalse(wrapper.HasValue, "未赋值时 HasValue 应为 false");
            Assert.AreEqual("NeedInitObject<String>_WaitingValue_[null]", wrapper.ToString(), "ToString 状态字符串不匹配");

            Log($"Inited={wrapper.Inited}, HasValue={wrapper.HasValue}, ToString={wrapper}, 通过");
        }

        [TestMethod]
        public void StringWrapper_赋值后_状态与值()
        {
            Log("--- 测试: NeedInitObject<string> 赋值后状态 (对应 test2) ---");
            var wrapper = new NeedInitObject<string>();
            wrapper.Value = "asdasd";

            Assert.IsTrue(wrapper.Inited, "赋值后 Inited 应为 true");
            Assert.IsTrue(wrapper.HasValue, "赋值后 HasValue 应为 true");
            Assert.AreEqual("asdasd", wrapper.Value, "Value 应等于赋入的值");
            Assert.AreEqual("NeedInitObject<String>_Inited_[asdasd]", wrapper.ToString(), "ToString 状态字符串不匹配");

            Assert.ThrowsException<InvalidOperationException>(() => wrapper.Value = "第二次赋值",
                "已赋值后再次赋值应抛 InvalidOperationException");
            Log("已赋值后再次赋值抛 InvalidOperationException, 通过");

            Log($"Inited={wrapper.Inited}, HasValue={wrapper.HasValue}, Value={wrapper.Value}, ToString={wrapper}, 通过");
        }

        [TestMethod]
        public void Int32Wrapper_未赋值_初始状态()
        {
            Log("--- 测试: NeedInitObject<int> 未赋值初始状态 (对应 test3) ---");
            var wrapper = new NeedInitObject<int>();

            Assert.IsFalse(wrapper.Inited, "未赋值时 Inited 应为 false");
            // 注意: 无约束 T 的 T? 即 T (不是 Nullable<T>), 值类型默认值 0 != null 恒为 true
            Assert.IsTrue(wrapper.HasValue, "值类型 T 时 HasValue 恒为 true (默认值 0 != null)");
            Assert.AreEqual("NeedInitObject<Int32>_WaitingValue_[0]", wrapper.ToString(),
                "ToString 状态字符串不匹配 (值类型默认值显示 0 而非 null)");

            Log($"Inited={wrapper.Inited}, HasValue={wrapper.HasValue}, ToString={wrapper}, 通过");
        }

        [TestMethod]
        public void Int32Wrapper_赋值后_状态与值()
        {
            Log("--- 测试: NeedInitObject<int> 赋值后状态 (对应 test4) ---");
            var wrapper = new NeedInitObject<int>();
            wrapper.Value = 8898;

            Assert.IsTrue(wrapper.Inited, "赋值后 Inited 应为 true");
            Assert.AreEqual(8898, wrapper.Value, "Value 应等于赋入的值");
            Assert.AreEqual("NeedInitObject<Int32>_Inited_[8898]", wrapper.ToString(), "ToString 状态字符串不匹配");

            Assert.ThrowsException<InvalidOperationException>(() => wrapper.Value = 123,
                "已赋值后再次赋值应抛 InvalidOperationException");
            Log("已赋值后再次赋值抛 InvalidOperationException, 通过");

            Log($"Inited={wrapper.Inited}, Value={wrapper.Value}, ToString={wrapper}, 通过");
        }

        [TestMethod]
        public void 未赋值访问_String_抛异常()
        {
            Log("--- 测试: 未赋值访问 NeedInitObject<string>.Value 抛异常 (对应 test5) ---");
            var wrapper = new NeedInitObject<string>();

            Assert.ThrowsException<InvalidOperationException>(() => _ = wrapper.Value,
                "未赋值时读取 Value 应抛 InvalidOperationException");
            Log("未赋值读取 Value 抛 InvalidOperationException, 通过");
        }

        [TestMethod]
        public void 未赋值访问_Int32_抛异常()
        {
            Log("--- 测试: 未赋值访问 NeedInitObject<int>.Value 抛异常 (对应 test6) ---");
            var wrapper = new NeedInitObject<int>();

            Assert.ThrowsException<InvalidOperationException>(() => _ = wrapper.Value,
                "未赋值时读取 Value 应抛 InvalidOperationException");
            Log("未赋值读取 Value 抛 InvalidOperationException, 通过");
        }

        [TestMethod]
        public void SetValueToProperty_反射赋值_重复赋值抛异常()
        {
            Log("--- 测试: SetValueToProperty 反射给属性赋值 (对应 test7) ---");
            var item = new TestClass();
            Type type = typeof(TestClass);
            var property = type.GetProperty(nameof(TestClass.TestA))!;

            Log($"初始: {item}");
            Assert.AreEqual("TestClass NeedInitObject<String>_WaitingValue_[null]", item.ToString(), "初始 ToString 不匹配");
            Assert.IsFalse(item.TestA.Inited, "初始 TestA.Inited 应为 false");

            NeedInitObject.SetValueToProperty(item, property, "1q23123");
            Log($"赋值 \"1q23123\" 后: {item}");
            Assert.IsTrue(item.TestA.Inited, "SetValueToProperty 后 TestA 应已赋值");
            Assert.AreEqual("1q23123", item.TestA.Value, "TestA.Value 应为赋入的值");
            Assert.AreEqual("TestClass NeedInitObject<String>_Inited_[1q23123]", item.ToString(), "赋值后 ToString 不匹配");

            // 包装器已 Inited, 再经 set 赋值应抛 InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => NeedInitObject.SetValueToProperty(item, property, "qqq"),
                "TestA 已赋值后再次 SetValueToProperty 应抛 InvalidOperationException");
            Log("已赋值后再次 SetValueToProperty 抛 InvalidOperationException, 通过");
        }

        [TestMethod]
        public void SetValueToProperty_值类型不匹配抛异常()
        {
            Log("--- 测试: SetValueToProperty 传入类型不匹配的值 (对应 test8) ---");
            var item = new TestClass();
            Type type = typeof(TestClass);
            var property = type.GetProperty(nameof(TestClass.TestA))!;

            NeedInitObject.SetValueToProperty(item, property, "1q23123");
            Log($"赋值 \"1q23123\" 后: {item}");
            Assert.AreEqual("1q23123", item.TestA.Value, "TestA.Value 应为赋入的字符串");

            // 库内部经 INeedInitObject.Value 的 setter 执行 (T)value 装箱转换, int → string 转换失败
            Assert.ThrowsException<InvalidCastException>(() => NeedInitObject.SetValueToProperty(item, property, 123456),
                "向 NeedInitObject<string> 属性传入 int 应抛 InvalidCastException");
            Log("传入 int 123456 抛 InvalidCastException, 通过");
        }
    }
}
