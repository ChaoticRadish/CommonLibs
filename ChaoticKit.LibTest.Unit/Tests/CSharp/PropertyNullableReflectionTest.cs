using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Reflection007
    /// 通过反射读取属性的可空标记 (依赖编译器生成的 NullableAttribute / NullableContextAttribute),
    /// 验证声明为 string 的属性被标为非空、声明为 string? 的属性被标为可空。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 编译器对可空标记的落点有两条路径: 属性自身标 [Nullable(2)] / [Nullable(1)],
    /// 或声明类型上统一标 [NullableContext(1)]。GetNullableAnnotationByte 做"属性自身→声明类型上下文"的
    /// 二级回退, 两种落点都能确定性判定 (字节 1=非空, 2=可空, 0=无标记)。
    /// </remarks>
    [TestClass]
    public sealed class PropertyNullableReflectionTest : UnitTestBase
    {
        /// <summary>被测样例类型: Test01 为非空 string, Test02 为可空 string?</summary>
        private sealed class Sample
        {
            public string Test01 { get; set; } = string.Empty;
            public string? Test02 { get; set; } = string.Empty;
        }

        [DataTestMethod]
        [DataRow(nameof(Sample.Test01), typeof(string), false, (byte)0)] // 非空: 上下文统一 enable 且无 #nullable 指令时编译器省略冗余标记, 实际无标记(0)
        [DataRow(nameof(Sample.Test02), typeof(string), true, (byte)2)]  // 可空: 与上下文不一致, 属性级标记字节 2
        public void 属性可空标记反射_按声明判定可空(string propertyName, Type expectedType, bool expectedNullable, byte expectedFlag)
        {
            PropertyInfo property = typeof(Sample).GetProperty(propertyName)!;

            Assert.IsNotNull(property, $"属性 {propertyName} 应能被反射找到");
            Assert.AreEqual(expectedType, property.PropertyType, $"属性 {propertyName} 的类型应匹配声明");
            Assert.AreEqual(PropertyAttributes.None, property.Attributes, $"属性 {propertyName} 的 PropertyAttributes 应为 None");

            byte flag = GetNullableAnnotationByte(property);
            Assert.AreEqual(expectedFlag, flag, $"属性 {propertyName} 的可空标记字节应为 {expectedFlag} (1=非空, 2=可空)");
            Assert.AreEqual(expectedNullable, flag == 2, $"属性 {propertyName} 的可空判定应匹配声明");

            Log($"属性 {propertyName}: Type={property.PropertyType}, Attributes={property.Attributes}, " +
                $"自身特性=[{string.Join(", ", property.GetCustomAttributes().Select(a => a.GetType().Name))}], " +
                $"可空标记字节={flag} (1=非空, 2=可空), 通过");
        }

        [TestMethod]
        public void GetProperties_枚举全部声明属性()
        {
            Log("--- 测试: GetProperties 枚举 ---");

            PropertyInfo[] properties = typeof(Sample).GetProperties();

            Assert.AreEqual(2, properties.Length, "应恰好枚举出 2 个属性");
            CollectionAssert.AreEquivalent(
                new[] { nameof(Sample.Test01), nameof(Sample.Test02) },
                properties.Select(p => p.Name).ToArray(),
                "枚举出的属性名集合应等于声明的两个属性");

            Log($"GetProperties 枚举出: {string.Join(", ", properties.Select(p => $"{p.Name}:{p.PropertyType.Name}"))}, 通过");
        }

        /// <summary>
        /// 读取属性生效的可空标记字节: 1=非空, 2=可空, 0=无标记。
        /// 优先读属性自身的 NullableAttribute, 缺失时回退到声明类型的 NullableContextAttribute。
        /// </summary>
        private static byte GetNullableAnnotationByte(PropertyInfo property)
        {
            NullableAttribute? nullable = property.GetCustomAttributes<NullableAttribute>(inherit: false).FirstOrDefault();
            if (nullable is not null && nullable.NullableFlags.Length > 0)
            {
                return nullable.NullableFlags[0];
            }

            NullableContextAttribute? context = property.DeclaringType
                ?.GetCustomAttributes<NullableContextAttribute>(inherit: false)
                .FirstOrDefault();
            return context?.Flag ?? 0;
        }
    }
}
