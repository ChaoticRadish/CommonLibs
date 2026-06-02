using Common_Util.Extensions;
using Common_Util.Module.DynamicIL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DynamicIL
{
    internal class DuckType001() : TestBase("鸭子类型测试: 是否看着像")
    {
        protected override void RunImpl()
        {
            RunTestMark();

        }

        [TestMethod]
        void test_比较是否看着像()
        {
            WritePair(DuckType.LooksLike<TestA1, ITestA>());
            WritePair(DuckType.LooksLike<TestA1, ITestA>(new()
            {
                Rule = DuckType.MatchingRule.Assignable | DuckType.MatchingRule.Match2Instance,
                CaseSensitiveWhenFindMethod = true,
                FindMethod = null,
            }));


            WritePair(DuckType.LooksLike<ITestA>(typeof(TestSA)));
            WritePair(DuckType.LooksLike<ITestA>(typeof(TestSA), new()
            {
                Rule = DuckType.MatchingRule.Exact | DuckType.MatchingRule.Match2Static,
                CaseSensitiveWhenFindMethod = true,
                FindMethod = null,
            }));
            WritePair(DuckType.LooksLike<ITestA>(typeof(TestSA), new()
            {
                Rule = DuckType.MatchingRule.Assignable | DuckType.MatchingRule.Match2Static,
                CaseSensitiveWhenFindMethod = true,
                FindMethod = null,
            }));


            test_比较是否看着像(typeof(TestB1<TestA3>), typeof(ITestB<TestA3>), DuckType.MatchingRule.Assignable | DuckType.MatchingRule.Match2Instance);
        }
        private void test_比较是否看着像(Type target, Type duck, DuckType.MatchingRule rule)
        {
            WriteLine("==================");
            DuckType.MatchingRuleArgs args = new()
            {
                Rule = rule,
                CaseSensitiveWhenFindMethod = true,
                FindMethod = null,
            };
            WriteLine($"Target ( {target} )\nDuck ( {duck} )\n => {DuckType.LooksLike(target, duck, args)}");
            foreach (var findResult in DuckType.AllLooksLikeMethod(target, duck, args))
            {
                WriteLine($"- {findResult.MethodInDuckInterface} => {(findResult.MethodInTargetType?.ToString() ?? "<not found>")}");
            }

            WriteLine("==================");
        }

        [TestMethod]
        void test_执行看着像方法的搜索() 
        {
            foreach (var type in new Type[] { typeof(TestA1), typeof(TestA2), typeof(TestSA) })
            {
                WriteLine(type.FullName + " : ");
                foreach (var m in DuckType.AllLooksLikeMethod(type, typeof(ITestA), new()
                {
                    Rule = DuckType.MatchingRule.Assignable | DuckType.MatchingRule.Match2Instance,
                    CaseSensitiveWhenFindMethod = true,
                    FindMethod = null,
                }))
                {
                    WriteLine($"- {m.MethodInDuckInterface} {m.IsFound.ToString("=>", "=/>")} {m.MethodInTargetType}");
                }
                WriteEmptyLine();
            }
            
        }

        public interface ITestA
        {
            string Property { get; set; }
            void Method01(string strA, string strB);

            void Method02(string strA, string strB);
        }
        public class TestA1
        {
            public string Property { get; set; } = string.Empty;
            public void Method01(string strA, string strB)
            {

            }
            public string Method02(string strA, string strB)
            {
                return strA + " " + strB;
            }
        }
        public class TestA2
        {
            public string Property { get; set; } = string.Empty;
            public void Method01(string strA, string strB)
            {

            }
        }

        public class TestA3 : ITestA
        {
            public string Property { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void Method01(string strA, string strB)
            {
                throw new NotImplementedException();
            }

            public void Method02(string strA, string strB)
            {
                throw new NotImplementedException();
            }
        }
        public static class TestSA
        {
            public static string Property { get; set; } = string.Empty;
            public static void Method01(string strA, string strB)
            {

            }
            public static string Method02(string strA, string strB)
            {
                return strA + " " + strB;
            }
        }

        public interface ITestB<T>
            where T : ITestA
        {
            T Object { get; set; }

            T1 TestMethod01<T1>(T obj);

            Dictionary<T1, T> TestMethod02<T1>(T obj) where T1 : IList<T>, new();
        }

        public class TestB1<T>
        {
            public T? Object { get; set; }

            public T1 TestMethod01<T1>(T obj)
            {
                throw new NotImplementedException();
            }

            public Dictionary<T1, T> TestMethod02<T1>(T obj) where T1 : IList<T>, new()
            {
                throw new NotImplementedException();
            }
        }

    }
}
