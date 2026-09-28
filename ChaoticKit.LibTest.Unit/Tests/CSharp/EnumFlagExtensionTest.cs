using ChaoticKit.Exceptions.General;
using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Enum001 (功能验证部分)
    /// 位标志枚举扩展 AddFlagWhen 的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class EnumFlagExtensionTest : UnitTestBase
    {
        [Flags]
        private enum E1 : int
        {
            A = 0b001,
            B = 0b010,
            C = 0b100,
        }

        private enum E2 : byte
        {
            A = 1,
            B = 2,
        }

        private enum E3 : long
        {
            A = 1,
            B = 2,
        }

        [TestMethod]
        public void UnderlyingType()
        {
            Log("--- 测试: 枚举底层类型 ---");
            Type t1 = Enum.GetUnderlyingType(typeof(E1));
            Type t2 = Enum.GetUnderlyingType(typeof(E2));
            Type t3 = Enum.GetUnderlyingType(typeof(E3));

            Assert.AreEqual(typeof(int), t1, "E1 底层类型应为 int");
            Assert.AreEqual(typeof(byte), t2, "E2 底层类型应为 byte");
            Assert.AreEqual(typeof(long), t3, "E3 底层类型应为 long");
            Log($"E1={t1.Name}, E2={t2.Name}, E3={t3.Name}, 通过");
        }

        [TestMethod]
        public void AddFlagWhen_SingleTrue()
        {
            Log("--- 测试: AddFlagWhen(true, flag) ---");
            E1 output = E1.A.AddFlagWhen(true, E1.B);

            Assert.AreEqual(E1.A | E1.B, output, "条件为 true 时应加上 E1.B");
            Log($"E1.A.AddFlagWhen(true, E1.B) = {output} ({(int)output}), 通过");
        }

        [TestMethod]
        public void AddFlagWhen_SingleFalse_Unchanged()
        {
            Log("--- 测试: AddFlagWhen(false, flag) ---");
            E1 output = E1.A.AddFlagWhen(false, E1.B);

            Assert.AreEqual(E1.A, output, "条件为 false 时应保持不变");
            Log($"E1.A.AddFlagWhen(false, E1.B) = {output} ({(int)output}), 保持不变, 通过");
        }

        [TestMethod]
        public void AddFlagWhen_MultiConditions()
        {
            Log("--- 测试: AddFlagWhen(多个条件) ---");
            E1 output = E1.A.AddFlagWhen((true, E1.B), (true, E1.C));
            Assert.AreEqual(E1.A | E1.B | E1.C, output, "两个 true 条件都应加上");
            Log($"E1.A.AddFlagWhen((true,B),(true,C)) = {output} ({(int)output}), 通过");

            E1 output2 = E1.A.AddFlagWhen((true, E1.B), (false, E1.C));
            Assert.AreEqual(E1.A | E1.B, output2, "false 条件不应生效");
            Log($"混合条件 (true,B),(false,C) = {output2} ({(int)output2}), 通过");
        }

        [TestMethod]
        public void AddFlagWhen_NonFlagsEnumThrows()
        {
            Log("--- 测试: 非位标志枚举调用 AddFlagWhen 应抛异常 ---");
            // 注意: FlagsEnumHelper<TEnum> 的静态构造函数会抛出 TypeNotSupportedException,
            // 首次访问该类型时 .NET 会将其包装为 TypeInitializationException, 内部异常才是真正的错误
            TypeInitializationException ex = Assert.ThrowsException<TypeInitializationException>(
                () => E2.A.AddFlagWhen(true, E2.B),
                "非位标志枚举应抛异常(静态初始化)");

            Assert.IsInstanceOfType(ex.InnerException, typeof(TypeNotSupportedException),
                "内部异常应为 TypeNotSupportedException, 实际: " + ex.InnerException?.GetType().Name);
            Log($"非 Flags 枚举调用 AddFlagWhen 抛 TypeInitializationException, 内部异常为 {ex.InnerException?.GetType().Name}, 通过");
        }
    }
}
