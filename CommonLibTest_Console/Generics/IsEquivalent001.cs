using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IsEquivalent001() : TestBase("测试泛型参数是否等价的判断方法 IsEquivalent ")
    {
        protected override void RunImpl()
        {
            var t1 = GetReturnType(nameof(method01));
            var t2 = GetReturnType(nameof(method02));
            var t3 = GetReturnType(nameof(method03));
            var t4 = GetReturnType(nameof(method04));
            var t5 = GetReturnType(nameof(method05));
            var t6 = GetReturnType(nameof(method06));
            var t7 = GetReturnType(nameof(method06));
            var t8 = GetReturnType(nameof(method08));
            var t9 = GetReturnType(nameof(method09));
            var t10 = GetReturnType(nameof(method10));
            var t11 = GetReturnType(nameof(method11));
            var t12 = GetReturnType(nameof(method12));
            var t13 = GetReturnType(nameof(method13));

            WritePair(ReflectionHelper.IsEquivalent(t1, t2));
            WritePair(ReflectionHelper.IsEquivalent(t1, t3));
            WritePair(ReflectionHelper.IsEquivalent(t1, t4));
            WritePair(ReflectionHelper.IsEquivalent(t1, t5));
            WritePair(ReflectionHelper.IsEquivalent(t1, t6));
            WritePair(ReflectionHelper.IsEquivalent(t1, t7));
            WritePair(ReflectionHelper.IsEquivalent(t1, t8));
            WritePair(ReflectionHelper.IsEquivalent(t1, t9));

            WritePair(ReflectionHelper.IsEquivalent(t2, t3));
            WritePair(ReflectionHelper.IsEquivalent(t3, t4));

            WritePair(ReflectionHelper.IsEquivalent(t10, t11));
            WritePair(ReflectionHelper.IsEquivalent(t11, t12 /* 泛型形参只要有相同约束就视为等价 */));
            WritePair(ReflectionHelper.IsEquivalent(t11, t12, gParamEquivalentCompareFunc: (_t1, _t2) => _t1 == _t2));
            WritePair(ReflectionHelper.IsEquivalent(t11, t12, gParamEquivalentCompareFunc: (_t1, _t2) => _t1?.Name == _t2?.Name && (_t1 != null && _t2 != null && ReflectionHelper.GenericParameterHasSameConstraints(_t1, _t2))));
            WritePair(ReflectionHelper.IsEquivalent(t11, t12, gParamEquivalentCompareFunc: check01));
            WritePair(ReflectionHelper.IsEquivalent(t11, t13 /* 泛型形参只要有相同约束就视为等价 */));
        }

        private bool check01(Type t1, Type t2)
        {
            return GetMethod("method11").GenericParamIndexOf(t1) == GetMethod("method12").GenericParamIndexOf(t2) // 在对应方法上的位置相同
                && ReflectionHelper.GenericParameterHasSameConstraints(t1, t2);   // 且约束相同
        }

        private MethodInfo GetMethod(string methodName)
        {
            return typeof(IsEquivalent001)
                .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!;
        }
        private Type? GetReturnType(string methodName)
        {
            return GetMethod(methodName).ReturnType;
        }

        private List<Dictionary<string, T>> method01<T>() where T : class
        {
            throw new NotImplementedException();
        }

        private List<Dictionary<string, T>> method02<T>()
        {
            throw new NotImplementedException();
        }
        private List<Dictionary<string, T>> method03<T>() where T : TestBase
        {
            throw new NotImplementedException();
        }
        private List<Dictionary<string, T>> method04<T>() where T : class
        {
            throw new NotImplementedException();
        }
        private List<Dictionary<string, T>> method05<T>() where T : struct
        {
            throw new NotImplementedException();
        }
        private IEnumerable<Dictionary<string, T>> method06<T>() where T : class
        {
            throw new NotImplementedException();
        }
        private List<IDictionary<string, T>> method07<T>() where T : class
        {
            throw new NotImplementedException();
        }
        private List<Dictionary<T, string>> method08<T>() where T : class
        {
            throw new NotImplementedException();
        }
        private Dictionary<string, T> method09<T>() where T : class
        {
            throw new NotImplementedException();
        }
        private (T1, T2) method10<T1, T2>()
        {
            throw new NotImplementedException();
        }
        private (T1, T2) method11<T1, T2>()
        {
            throw new NotImplementedException();
        }
        private (T2, T1) method12<T1, T2>()
        {
            throw new NotImplementedException();
        }
        private (T2, T1) method13<T1, T2>() where T1 : struct
        {
            throw new NotImplementedException();
        }
    }
}
