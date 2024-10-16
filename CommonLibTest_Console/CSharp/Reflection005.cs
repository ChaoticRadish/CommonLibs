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

            // WritePair(m1.FullInfoString());
            // WritePair(m2.FullInfoString());

            WritePair(m1.SequenceEqual(m2));
            WritePair(m2.SequenceEqual(m3));
            WritePair(m3.SequenceEqual(m4));
            WritePair(m1.SequenceEqual(m5));
            WritePair(m1.SequenceEqual(m6));
            WritePair(m5.SequenceEqual(m6));
            WritePair(m5.SequenceEqual(m7));

            var ec = EqualityComparer<ParameterInfo>.Create(
                (a, b) => 
                {
                    if (a != null && b != null)
                    {
                        if (a.ParameterType != b.ParameterType || a.IsOut != b.IsOut) return false;
                        return true;
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
        }

        public void M1(string str, int i) { }
        public void M2(string str, int i) { }
        public void M3(string str, int i, bool b) { }
        public void M4(string str, bool b, int i) { }
        public void M5(string str, ref int i) { }
        public void M6(string str, out int i) { i = 0; }
        public void M7(string str, ref int i) { }
    }
}
