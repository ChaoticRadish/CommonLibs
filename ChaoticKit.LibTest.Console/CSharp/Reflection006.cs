using ChaoticKit;
using ChaoticKit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ChaoticKit.LibTest.Console.Xml.ReadWrite001;

namespace ChaoticKit.LibTest.Console.CSharp
{
    internal class Reflection006() : TestBase("遍历泛型参数")
    {

        protected override void RunImpl()
        {
            RunTestMark();
        }

        #region 获取类型信息的测试
        [TestMethod]
        void test_获取类型信息的测试()
        {
            test(typeof(string));
            test(typeof(int?));
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method01))!.ReturnType);
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType);
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType, false);
            test_allGParams(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType);
            test_allGParams(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType, false);
        }

        private void test(Type type, bool includeSelf = true)
        {
            WriteLine("类型: " + type.ToString() + $"  ({includeSelf.ToString("includeSelf!", "not includeSelf")})");
            int index = 0;
            foreach (Type t in ReflectionHelper.PreorderGenericParameterTree(type, includeSelf))
            {
                WriteLine($"{index}. " + t.ToString());
                index++;
            }
            WriteEmptyLine();
        }
        private void test_allGParams(Type type, bool includeSelf = true)
        {
            WriteLine("类型: " + type.ToString() + $"  ({includeSelf.ToString("includeSelf!", "not includeSelf")})");
            int index = 0;
            foreach (Type t in ReflectionHelper.PreorderGenericParameterTree(type, includeSelf))
            {
                if (!t.IsGenericParameter)
                {
                    continue;
                }
                WriteLine($"{index}. " + t.ToString());
                index++;
            }
            WriteEmptyLine();
        }


        #endregion


        class TestA<T>
        {
            public Dictionary<string, TA> Method01<TA>()
            {
                return new();
            }
            public Dictionary<Dictionary<Dictionary<T1, string>, TA>, Dictionary<T1, TA>> Method02<T1, TA>()
                where T1 : notnull
            {
                return new();
            }

            public Dictionary<string, TA> Method03<TA>(List<TA> tas)
            {
                return new();
            }
            public Dictionary<string, TA> Method04<TA, TB>(List<(TA, TB)> tas, TB tb)
            {
                return new();
            }
        }

        class TestB<T>
        {
            public Dictionary<string, TA> Method01<TA>() where TA : notnull
            {
                return new();
            }

            public Dictionary<string, TA> Method03<TA>(List<TA> tas) where TA : notnull
            {
                return new();
            }
            public Dictionary<string, TA> Method04<TA, TB>(List<(TA, TB)> tas, TB tb) where TA : notnull
            {
                return new();
            }
        }

        #region 参数比较测试

        [TestMethod]
        void test_参数比较测试()
        {
            var m1 = typeof(TestA<>).GetMethod("Method01")!;
            var m2 = typeof(TestB<>).GetMethod("Method01")!;
            var m3 = typeof(TestA<>).GetMethod("Method03")!;
            var m4 = typeof(TestB<>).GetMethod("Method03")!;
            var m5 = typeof(TestA<>).GetMethod("Method04")!;
            var m6 = typeof(TestB<>).GetMethod("Method04")!;

            compare01(m1, m2);
            compare01(m3, m4);
            compare01(m5, m6);

        }

        void compare01(MethodInfo methodA, MethodInfo methodB)
        {
            WriteLine($"使用 {nameof(isEquivalent01)} 比较形参列表");
            
            var psA = methodA.GetParameters();
            var psB = methodB.GetParameters();

            WritePair(methodA);
            WritePair(methodB);

            foreach (var (pA, pB) in (psA, psB).UntilAllAway())
            {
                WriteLine($"pA: {pA} ::::: pB: {pB}");
                if (pA != null && pB != null)
                {
                    WritePair(isEquivalent01(pA, pB));
                }
                else
                {
                    WritePair("pA pB 其中至少一个为 null");
                }
            }


        }

        /// <summary>
        /// 判断两个形参是否等价, 如果是泛型形参, 则约束条件相同即可 (不考虑形参是包含了泛型参数的某个泛型类型, 只考虑形参直接是泛型参数的情况)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool isEquivalent01(ParameterInfo a, ParameterInfo b)
        {
            if (a.IsOut != b.IsOut) return false;
            if (a.ParameterType.IsGenericParameter != b.ParameterType.IsGenericParameter) return false;
            if (a.ParameterType.IsGenericParameter)
            {
                return ReflectionHelper.GenericParameterHasSameConstraints(a.ParameterType, b.ParameterType);
            }
            else
            {
                return a.ParameterType == b.ParameterType;
            }
        }
        #endregion
    }
}
