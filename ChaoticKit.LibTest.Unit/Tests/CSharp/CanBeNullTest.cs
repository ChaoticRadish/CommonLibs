using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Reflection008 (可空类型的判断)
    /// ChaoticKit.Extensions.TypeExtension.CanBeNull 对值类型/引用类型/可空值类型的判断结果验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// CanBeNull 语义: 非值类型(引用类型/接口)为 true; 值类型中只有 Nullable&lt;T&gt; 为 true, 其余为 false。
    /// </remarks>
    [TestClass]
    public sealed class CanBeNullTest : UnitTestBase
    {
        private struct MyStruct
        {
        }

        private interface MyInterface
        {
        }

        [DataTestMethod]
        [DataRow(typeof(string), true)]          // 引用类型
        [DataRow(typeof(int?), true)]            // 可空值类型
        [DataRow(typeof(int), false)]            // 值类型
        [DataRow(typeof(MyStruct), false)]       // 自定义值类型
        [DataRow(typeof(MyInterface), true)]     // 接口(引用类型)
        [DataRow(typeof(CanBeNullTest), true)]   // 类(引用类型)
        public void CanBeNull_各类型判断结果(Type type, bool expected)
        {
            Log($"--- 测试: CanBeNull({type.Name}) ---");

            bool actual = type.CanBeNull();

            Assert.AreEqual(expected, actual, $"CanBeNull({type.FullName}) 结果应为 {expected}, 实际: {actual}");
            Log($"CanBeNull({type.FullName}) = {actual}, 通过");
        }

        [TestMethod]
        public void CanBeNull_带底层类型输出()
        {
            Log("--- 测试: CanBeNull(out underlyingType) ---");

            // 可空值类型: 返回 true, 底层类型为 T
            bool nullableResult = typeof(int?).CanBeNull(out Type nullableUnderlying);
            Assert.IsTrue(nullableResult, "int? 应可空");
            Assert.AreEqual(typeof(int), nullableUnderlying, "int? 的底层类型应为 int");
            Log($"CanBeNull(typeof(int?), out) = {nullableResult}, underlying = {nullableUnderlying.Name}, 通过");

            // 普通值类型: 返回 false, 底层类型输出自身
            bool valueResult = typeof(int).CanBeNull(out Type valueUnderlying);
            Assert.IsFalse(valueResult, "int 不应可空");
            Assert.AreEqual(typeof(int), valueUnderlying, "非 Nullable 值类型应输出类型自身");
            Log($"CanBeNull(typeof(int), out) = {valueResult}, underlying = {valueUnderlying.Name}, 通过");

            // 引用类型: 返回 true, 底层类型输出自身
            bool refResult = typeof(string).CanBeNull(out Type refUnderlying);
            Assert.IsTrue(refResult, "string 应可空");
            Assert.AreEqual(typeof(string), refUnderlying, "引用类型应输出类型自身");
            Log($"CanBeNull(typeof(string), out) = {refResult}, underlying = {refUnderlying.Name}, 通过");
        }
    }
}
