using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection003() : TestBase("测试 TypeExtension 新增的查找属性/方法的方法")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void test1()
        {
            if (typeof(Test).TryFindPublicStaticMethod("Run1", typeof(string), out var method))
            {
                WriteLine($"找到目标方法: {method}");
            }
            else
            {
                WriteLine($"未找到目标方法");
            }
        }
        [TestMethod]
        void test1_f()
        {
            if (typeof(Test).TryFindPublicStaticMethod("Run1", typeof(bool), out var method))
            {
                WriteLine($"找到目标方法: {method}");
            }
            else
            {
                WriteLine($"未找到目标方法");
            }
        }
        [TestMethod]
        void test2()
        {
            bool b = typeof(Test).TryFindPublicStaticMethodAndInvoke("Run2");
            WriteLine($"是否调用成功: {b}");
        }
        [TestMethod]
        void test2_f()
        {
            bool b = typeof(Test).TryFindPublicStaticMethodAndInvoke("Run3");
            WriteLine($"是否调用成功: {b}");
        }
        [TestMethod]
        void test3()
        {
            if (typeof(Test).TryFindPublicStaticMethodAndInvoke("Run3", out string? result))
            {
                WriteLine($"执行并取得目标类型: {result}");
            }
            else
            {
                WriteLine($"未执行取得目标类型");
            }
        }
        [TestMethod]
        void test3_f()
        {
            if (typeof(Test).TryFindPublicStaticMethodAndInvoke("Run3", out bool? result))
            {
                WriteLine($"执行并取得目标类型: {result}");
            }
            else
            {
                WriteLine($"未执行取得目标类型");
            }
        }
        [TestMethod]
        void test4()
        {
            if (typeof(Test).TryFindPublicStaticProperty("Test1", out string? str1))
            {
                WriteLine($"找到 Test1: {str1}");
            }
            if (typeof(Test).TryFindPublicStaticProperty("Test2", out string? str2))
            {
                WriteLine($"找到 Test2: {str2}");
            }
            if (typeof(Test).TryFindPublicStaticProperty("Test3", out string? str3))
            {
                WriteLine($"找到 Test3: {str3}");
            }
        }

        static class Test
        {
            public static string Run1() { return "Run1"; }

            public static void Run2() { }
            public static string Run3() { return "Run3"; }


            public static string Test1 { get; set; } = "qqq";
            public static string Test2 = "qqq";
            public static bool Test3 = true;
        }
    }
}
