using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection005() : TestBase("测试比较两组形参是否一致")
    {
        protected override void RunImpl()
        {
            var m1 = GetType().GetMethod(nameof(M1))!.GetParameters();
            var m2 = GetType().GetMethod(nameof(M2))!.GetParameters();
            var m3 = GetType().GetMethod(nameof(M3))!.GetParameters();
            var m4 = GetType().GetMethod(nameof(M4))!.GetParameters();
            var m5 = GetType().GetMethod(nameof(M5))!.GetParameters();
            var m6 = GetType().GetMethod(nameof(M6))!.GetParameters();
            var m7 = GetType().GetMethod(nameof(M7))!.GetParameters();
            var m8 = GetType().GetMethod(nameof(M8))!.GetParameters();
            var m9 = GetType().GetMethod(nameof(M9))!.GetParameters();
            var m10 = GetType().GetMethod(nameof(M10))!.GetParameters();
            var m11 = GetType().GetMethod(nameof(M11))!.GetParameters();
            var m12 = GetType().GetMethod(nameof(M12))!.GetParameters();
            var m13 = GetType().GetMethod(nameof(M13))!.GetParameters();
            var m14 = GetType().GetMethod(nameof(M14))!.GetParameters();
            var m15 = GetType().GetMethod(nameof(M15))!.GetParameters();

            // WritePair(m1.FullInfoString());
            // WritePair(m2.FullInfoString());

            WritePair(m1.SequenceEqual(m2));
            WritePair(m2.SequenceEqual(m3));
            WritePair(m3.SequenceEqual(m4));
            WritePair(m1.SequenceEqual(m5));
            WritePair(m1.SequenceEqual(m6));
            WritePair(m5.SequenceEqual(m6));
            WritePair(m5.SequenceEqual(m7));
            WritePair(m8.SequenceEqual(m9));
            WritePair(m10.SequenceEqual(m11));
            WritePair(m10.SequenceEqual(m12));
            WritePair(m13.SequenceEqual(m14));
            WritePair(m14.SequenceEqual(m15));

            var ec = EqualityComparer<ParameterInfo>.Create(
                (a, b) => 
                {
                    if (a != null && b != null)
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
                    else
                    {
                        return a?.ParameterType == b?.ParameterType;
                    }
                }, 
                p => p.ParameterType.GetHashCode());

            WritePair(m1.SequenceEqual(m2, ec));
            WritePair(m2.SequenceEqual(m3, ec));
            WritePair(m3.SequenceEqual(m4, ec));
            WritePair(m1.SequenceEqual(m5, ec));
            WritePair(m1.SequenceEqual(m6, ec));
            WritePair(m5.SequenceEqual(m6, ec));
            WritePair(m5.SequenceEqual(m7, ec));
            WritePair(m8.SequenceEqual(m9, ec));
            WritePair(m10.SequenceEqual(m11, ec));
            WritePair(m10.SequenceEqual(m12, ec));
            WritePair(m13.SequenceEqual(m14, ec));
            WritePair(m14.SequenceEqual(m15, ec));
        }

        public void M1(string str, int i) { }
        public void M2(string str, int i) { }
        public void M3(string str, int i, bool b) { }
        public void M4(string str, bool b, int i) { }
        public void M5(string str, ref int i) { }
        public void M6(string str, out int i) { i = 0; }
        public void M7(string str, ref int i) { }
        public void M8<T>(string str, T i) { }
        public void M9<T>(string str, T i) { }
        public void M10<T>(string str, T i) where T : struct { }
        public void M11<T>(string str, T i) where T : struct { }
        public void M12<T>(string str, T i) where T : class { }
        public void M13<T1, T2>(string str, Dictionary<T2, T1> i) 
            where T1 : notnull, new()
            where T2 : struct
        { }
        public void M14<T1, T2>(string str, Dictionary<T1, T2> i)
            where T1 : notnull, new()
            where T2 : struct
        { }
        public void M15<T1, T2>(string str, Dictionary<T1, string> i)
            where T1 : notnull, new()
            where T2 : struct
        { }
    }
}
