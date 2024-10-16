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

            // WritePair(m1.FullInfoString());
            // WritePair(m2.FullInfoString());

            WritePair(m1.SequenceEqual(m2));
            WritePair(m2.SequenceEqual(m3));
            WritePair(m3.SequenceEqual(m4));

            var ec = EqualityComparer<ParameterInfo>.Create(
                (a, b) => a?.ParameterType == b?.ParameterType, 
                p => p.ParameterType.GetHashCode());

            WritePair(m1.SequenceEqual(m2, ec));
            WritePair(m2.SequenceEqual(m3, ec));
            WritePair(m3.SequenceEqual(m4, ec));
        }

        public void M1(string str, int i) { }
        public void M2(string str, int i) { }
        public void M3(string str, int i, bool b) { }
        public void M4(string str, bool b, int i) { }
    }
}
