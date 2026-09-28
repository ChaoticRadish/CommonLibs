using ChaoticKit.Extensions;
using System.Reflection;

namespace ChaoticKit.LibTest.Unit.Generics
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Generics.IndexOf001 (test02 确定性部分; 源 test01 无输出无断言, 属空壳, 未迁移)
    /// ChaoticKit.Extensions 的 Type / MethodInfo 泛型形参索引扩展 GenericParamIndexOf 验证:
    /// 在目标泛型形参列表中找到返回位置索引, 找不到返回 -1, 传入非泛型形参抛 ArgumentException
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 源测试中方法级 GenericParamIndexOf 的 6 组调用被重复打印两次 (源码复制粘贴所致, 74-79 行与 83-88 行完全相同), 此处仅断言一次。
    /// </remarks>
    [TestClass]
    public sealed class GenericParamIndexOfTest : UnitTestBase
    {
        /// <summary>与源测试同构的嵌套泛型类型: 类型形参 T2/T3, 各方法携带方法级形参 T1</summary>
        public sealed class Test<T2, T3>
        {
            public T1 method01<T1>() => throw new NotImplementedException();
            public T2 method02<T1>() => throw new NotImplementedException();
            public T3 method03<T1>() => throw new NotImplementedException();
        }

        /// <summary>
        /// 6 个被测返回值类型, 与源测试 gType1..gType6 一一对应:
        /// [0] T1 (开放类型 method01 的方法级形参) / [1] T2 (类型形参 0) / [2] T3 (类型形参 1)
        /// [3] T1 (构造类型 method01 的方法级形参) / [4] string (T2 绑定) / [5] int (T3 绑定)
        /// </summary>
        private static readonly Type[] GTypes = BuildGTypes();

        private static Type[] BuildGTypes()
        {
            Type type1 = typeof(Test<,>);
            Type type2 = typeof(Test<string, int>);

            MethodInfo m1_1 = type1.GetMethod("method01")!;
            MethodInfo m1_2 = type1.GetMethod("method02")!;
            MethodInfo m1_3 = type1.GetMethod("method03")!;
            MethodInfo m2_1 = type2.GetMethod("method01")!;
            MethodInfo m2_2 = type2.GetMethod("method02")!;
            MethodInfo m2_3 = type2.GetMethod("method03")!;

            return
            [
                m1_1.ReturnType,
                m1_2.ReturnType,
                m1_3.ReturnType,
                m2_1.ReturnType,
                m2_2.ReturnType,
                m2_3.ReturnType,
            ];
        }

        [TestMethod]
        public void Type_GenericParamIndexOf_开放泛型定义()
        {
            Log("--- 场景 A: Type.GenericParamIndexOf, 目标 = 开放泛型类型定义 Test<,> ---");
            Type type1 = typeof(Test<,>);

            int i1 = type1.GenericParamIndexOf(GTypes[0]);
            int i2 = type1.GenericParamIndexOf(GTypes[1]);
            int i3 = type1.GenericParamIndexOf(GTypes[2]);
            int i4 = type1.GenericParamIndexOf(GTypes[3]);

            Assert.AreEqual(-1, i1, "方法级形参 T1 不属于类型形参列表 [T2,T3], 应返回 -1");
            Assert.AreEqual(0, i2, "T2 是类型第 0 个形参, 应返回 0");
            Assert.AreEqual(1, i3, "T3 是类型第 1 个形参, 应返回 1");
            Assert.AreEqual(-1, i4, "构造类型 method01 的方法级形参 T1 同样不属于类型形参列表, 应返回 -1");
            Log($"type1.GenericParamIndexOf: T1→{i1}, T2→{i2}, T3→{i3}, T1(构造方法)→{i4}, 通过");
        }

        [TestMethod]
        public void Type_GenericParamIndexOf_构造泛型类型()
        {
            Log("--- 场景 B: Type.GenericParamIndexOf, 目标 = 构造泛型类型 Test<string,int> ---");
            Type type2 = typeof(Test<string, int>);

            int i1 = type2.GenericParamIndexOf(GTypes[0]);
            int i2 = type2.GenericParamIndexOf(GTypes[1]);
            int i3 = type2.GenericParamIndexOf(GTypes[2]);
            int i4 = type2.GenericParamIndexOf(GTypes[3]);

            Assert.AreEqual(-1, i1, "方法级形参 T1 不属于类型形参列表 [T2,T3], 应返回 -1");
            Assert.AreEqual(0, i2, "T2 位于类型定义 Test<,> 第 0 位, 应返回 0");
            Assert.AreEqual(1, i3, "T3 位于类型定义 Test<,> 第 1 位, 应返回 1");
            Assert.AreEqual(-1, i4, "构造类型 method01 的方法级形参 T1 不属于类型形参列表, 应返回 -1");
            Log($"type2.GenericParamIndexOf: T1→{i1}, T2→{i2}, T3→{i3}, T1(构造方法)→{i4}, 通过");
        }

        [TestMethod]
        public void MethodInfo_GenericParamIndexOf()
        {
            Log("--- 场景 C: MethodInfo.GenericParamIndexOf, 各方法形参列表均为 [T1] ---");
            Type type1 = typeof(Test<,>);
            Type type2 = typeof(Test<string, int>);
            MethodInfo m1_1 = type1.GetMethod("method01")!;
            MethodInfo m1_2 = type1.GetMethod("method02")!;
            MethodInfo m1_3 = type1.GetMethod("method03")!;
            MethodInfo m2_1 = type2.GetMethod("method01")!;

            Assert.AreEqual(0, m1_1.GenericParamIndexOf(GTypes[0]), "method01<T1> 返回自身方法级形参 T1, 位置 0");
            Assert.AreEqual(-1, m1_2.GenericParamIndexOf(GTypes[1]), "T2 是类型形参, 不在方法形参列表 [T1] 中, 应返回 -1");
            Assert.AreEqual(-1, m1_3.GenericParamIndexOf(GTypes[2]), "T3 是类型形参, 不在方法形参列表 [T1] 中, 应返回 -1");
            Assert.AreEqual(0, m2_1.GenericParamIndexOf(GTypes[3]), "构造类型上 method01<T1> 返回自身方法级形参 T1, 位置 0");
            Log($"m1_1(T1)→0, m1_2(T2)→-1, m1_3(T3)→-1, m2_1(T1)→0, 通过");
        }

        [DataTestMethod]
        [DataRow("TypeOpen", 5)]        // Test<,> 上查找 string (gType5)
        [DataRow("TypeOpen", 6)]        // Test<,> 上查找 int (gType6)
        [DataRow("TypeClosed", 5)]      // Test<string,int> 上查找 string
        [DataRow("TypeClosed", 6)]      // Test<string,int> 上查找 int
        [DataRow("MethodClosed02", 5)]  // 构造类型 method02<T1> 上查找 string
        [DataRow("MethodClosed03", 6)]  // 构造类型 method03<T1> 上查找 int
        public void GenericParamIndexOf_传入非泛型形参抛ArgumentException(string target, int caseIndex)
        {
            Type g = GTypes[caseIndex - 1];
            Func<Type, int> lookup = target switch
            {
                "TypeOpen" => typeof(Test<,>).GenericParamIndexOf,
                "TypeClosed" => typeof(Test<string, int>).GenericParamIndexOf,
                "MethodClosed02" => typeof(Test<string, int>).GetMethod("method02")!.GenericParamIndexOf,
                "MethodClosed03" => typeof(Test<string, int>).GetMethod("method03")!.GenericParamIndexOf,
                _ => throw new ArgumentOutOfRangeException(nameof(target)),
            };

            ArgumentException ex = Assert.ThrowsException<ArgumentException>(
                () => lookup(g), $"[{target}, case={caseIndex}] 传入 {g} (非泛型形参) 应抛 ArgumentException");
            StringAssert.Contains(ex.Message, "不是泛型形参", "异常信息应指明传入的不是泛型形参");
            Log($"[{target}] GenericParamIndexOf({g}) 抛 ArgumentException: {ex.Message}, 通过");
        }

        [TestMethod]
        public void GenericParamIndexOf_非泛型目标返回负一()
        {
            Log("--- 场景 D: 目标非泛型时返回 -1 (扩展方法文档行为, 源测试未覆盖, 补充验证) ---");
            Type nonGenericType = typeof(object);
            MethodInfo nonGenericMethod = typeof(object).GetMethod("ToString")!;

            Assert.AreEqual(-1, nonGenericType.GenericParamIndexOf(GTypes[1]), "非泛型类型 object 上查找泛型形参 T2 应返回 -1");
            Assert.AreEqual(-1, nonGenericMethod.GenericParamIndexOf(GTypes[1]), "非泛型方法 ToString 上查找泛型形参 T2 应返回 -1");
            Log($"object.GenericParamIndexOf(T2) = -1, object.ToString.GenericParamIndexOf(T2) = -1, 通过");
        }
    }
}
