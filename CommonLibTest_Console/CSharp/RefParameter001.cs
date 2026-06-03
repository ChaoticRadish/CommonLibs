using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class RefParameter001() : TestBase("测试 ref 形参")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod("传入 null")]
        protected void test1()
        {
            Dictionary<string, string>? dic = null;
            myMethod(ref dic);
            WritePair(dic.FullInfoString(), split: "\n");
        }

        [TestMethod("传入非 null")]
        protected void test2()
        {
            Dictionary<string, string>? dic = new() { { "测试1", "值1" }, { "测试2", "值2" }, };
            myMethod(ref dic);
            WritePair(dic.FullInfoString(), split: "\n");
        }

        private void myMethod([NotNull] ref Dictionary<string, string>? dic)
        {
            dic ??= [];
            dic.Add("Test001", "good");
            dic.Add("Test002", "bad");
        }
    }
}
