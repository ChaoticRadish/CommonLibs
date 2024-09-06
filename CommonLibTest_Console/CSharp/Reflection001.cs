using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection001() : TestBase("反射测试, 反射取得方法信息")
    {
        ClassTest? test;
        protected override void RunImpl()
        {
            test = new(this);

            testGet(() => typeof(ClassTest).GetMethod(nameof(ClassTest.Method1)));

            testGet(() => typeof(ClassTest).GetMethod(nameof(ClassTest.Method2))); // 预期异常: 不明确的匹配

            testGet(() => typeof(ClassTest).GetMethod(nameof(ClassTest.Method2), [typeof(string), typeof(string)]), "aaa", "bbb");

            testGet(() => typeof(ClassTest).GetMethod(nameof(ClassTest.Method2), []));
        }

        int index = 0;

        private void testGet(Func<MethodInfo?> func, params object[] invokeArgs)
        {
            index++;

            WriteLine($"测试 {index} :");

            MethodInfo? method = null;
            try
            {
                method = func();
            }
            catch (Exception ex)
            {

                WriteLine("异常! " + ex.Message);
            }

            if (method == null)
            {
                WriteLine("method is null");
            }
            else
            {
                try
                {
                    method.Invoke(test, invokeArgs);
                }
                catch (Exception ex)
                {
                    WriteLine("异常! " + ex.Message);
                }

                WriteLine($"method.DeclaringType: {method.DeclaringType}");
                WriteLine($"method.MemberType: {method.MemberType}");
            }

            WriteLine();
            WriteLine();
        }

        public class ClassTest(TestBase test)
        {
            public TestBase Test { get; } = test;

            public void Method1()
            {
                Test.WriteLine("Method1()");   
            }
            public void Method2()
            {
                Test.WriteLine("Method2()");
            }
            public void Method2(string x, string y)
            {
                Test.WriteLine($"Method2({x}, {y})");
            }
        }
    }
}
