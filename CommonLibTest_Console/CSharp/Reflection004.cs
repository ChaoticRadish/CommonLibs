using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_Util;
using Common_Util.Extensions;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection004() : TestBase("测试比较两个类型是否为具有相同约束条件的泛型参数")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void test0()
        {
            var t1 = typeof(Test1<,>);
            var b = typeof(Test1<,>).GetGenericArgument(1);
            WritePair(ReflectionHelper.GenericParameterHasSameConstraints(t1, b));
        }

        [TestMethod]
        void test1()
        {
            var a = typeof(Test1<,>).GetGenericArgument(0);
            var b = typeof(Test1<,>).GetGenericArgument(1);
            WritePair(ReflectionHelper.GenericParameterHasSameConstraints(a, b));
        }

        [TestMethod]
        void test2()
        {
            var t1a = typeof(Test1<,>).GetGenericArgument(0);
            var t2a = typeof(Test2<,>).GetGenericArgument(0);
            WritePair(ReflectionHelper.GenericParameterHasSameConstraints(t1a, t2a));
        }

        [TestMethod]
        void test3()
        {
            var t1a = typeof(Test1<,>).GetGenericArgument(0);
            var t3a = typeof(Test3<,>).GetGenericArgument(0);
            WritePair(ReflectionHelper.GenericParameterHasSameConstraints(t1a, t3a));
        }


        public class Test1<A, B>
        {

        }
        public class Test2<A, B>
        {

        }
        public class Test3<A, B>
            where A : struct
        {
        }
    }
}
