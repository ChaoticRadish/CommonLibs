using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IndexOf001() : TestBase("测试查找泛型形参在目标类型的泛型形参列表中的索引值")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void test01()
        {
            var t1 = typeof(IndexOf001).GetMethod(nameof(method01))!.ReturnType;
            var t2 = typeof(IndexOf001).GetMethod(nameof(method02))!.ReturnType;
            var t3 = typeof(IndexOf001).GetMethod(nameof(method03))!.ReturnType;
        }
        [TestMethod]
        void test02()
        {
            Type type1 = typeof(Test<,>);
            Type type2 = typeof(Test<string,int>);

            MethodInfo m1_1 = type1.GetMethod("method01")!;
            MethodInfo m1_2 = type1.GetMethod("method02")!;
            MethodInfo m1_3 = type1.GetMethod("method03")!;
            MethodInfo m2_1 = type2.GetMethod("method01")!;
            MethodInfo m2_2 = type2.GetMethod("method02")!;
            MethodInfo m2_3 = type2.GetMethod("method03")!;

            var gType1 = m1_1.ReturnType;
            var gType2 = m1_2.ReturnType;
            var gType3 = m1_3.ReturnType;
            var gType4 = m2_1.ReturnType;
            var gType5 = m2_2.ReturnType;
            var gType6 = m2_3.ReturnType;

            WritePair(gType1);
            WritePair(gType2);
            WritePair(gType3);
            WritePair(gType4);
            WritePair(gType5);
            WritePair(gType6);

            WriteEmptyLine();

            TryRun(gType1, type1.GenericParamIndexOf);
            TryRun(gType2, type1.GenericParamIndexOf);
            TryRun(gType3, type1.GenericParamIndexOf);
            TryRun(gType4, type1.GenericParamIndexOf);
            TryRun(gType5, type1.GenericParamIndexOf);
            TryRun(gType6, type1.GenericParamIndexOf);

            WriteEmptyLine();

            TryRun(gType1, type2.GenericParamIndexOf);
            TryRun(gType2, type2.GenericParamIndexOf);
            TryRun(gType3, type2.GenericParamIndexOf);
            TryRun(gType4, type2.GenericParamIndexOf);
            TryRun(gType5, type2.GenericParamIndexOf);
            TryRun(gType6, type2.GenericParamIndexOf);

            WriteEmptyLine();

            TryRun(gType1, m1_1.GenericParamIndexOf);
            TryRun(gType2, m1_2.GenericParamIndexOf);
            TryRun(gType3, m1_3.GenericParamIndexOf);
            TryRun(gType4, m2_1.GenericParamIndexOf);
            TryRun(gType5, m2_2.GenericParamIndexOf);
            TryRun(gType6, m2_3.GenericParamIndexOf);

            WriteEmptyLine();

            TryRun(gType1, m1_1.GenericParamIndexOf);
            TryRun(gType2, m1_2.GenericParamIndexOf);
            TryRun(gType3, m1_3.GenericParamIndexOf);
            TryRun(gType4, m2_1.GenericParamIndexOf);
            TryRun(gType5, m2_2.GenericParamIndexOf);
            TryRun(gType6, m2_3.GenericParamIndexOf);
        }

        private void TryRun<T>(Func<T> action, [CallerArgumentExpression(nameof(action))] string key = "")
        {
            WritePair(key: key, value: DefaultValueHelper.TryRunToString(action));
        }
        private void TryRun<T1, T2>(T1 arg, Func<T1, T2> action, [CallerArgumentExpression(nameof(arg))] string argStr = "", [CallerArgumentExpression(nameof(action))] string key = "")
        {
            WritePair(key: $"arg:{arg} ::: {key}", value: DefaultValueHelper.TryRunToString(() => action(arg)));
        }

        public T1 method01<T1, T2, T3>()
        { throw new NotImplementedException(); }
        public T2 method02<T1, T2, T3>()
        { throw new NotImplementedException(); }
        public T3 method03<T1, T2, T3>()
        { throw new NotImplementedException(); }

        public class Test<T2, T3>
        {
            public T1 method01<T1>()
            { throw new NotImplementedException(); }
            public T2 method02<T1>()
            { throw new NotImplementedException(); }
            public T3 method03<T1>()
            { throw new NotImplementedException(); }
        }
    }
}
