using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataWrapper
{
    internal class NeedInitObject001() : TestBase($"测试 {nameof(NeedInitObject)} ")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod(nameof(test1))]
        private void test1()
        {
            NeedInitObject<string> wrapper = new NeedInitObject<string>();

            WritePair(wrapper);
        }
        [TestMethod(nameof(test2))]
        private void test2()
        {
            NeedInitObject<string> wrapper = new NeedInitObject<string>();

            wrapper.Value = "asdasd";

            WritePair(wrapper);
        }
        [TestMethod(nameof(test3))]
        private void test3()
        {
            NeedInitObject<int> wrapper = new NeedInitObject<int>();

            WritePair(wrapper);
        }
        [TestMethod(nameof(test4))]
        private void test4()
        {
            NeedInitObject<int> wrapper = new NeedInitObject<int>();

            wrapper.Value = 8898;

            WritePair(wrapper);
        }
        [TestMethod(nameof(test5))]
        private void test5()
        {
            NeedInitObject<string> wrapper = new NeedInitObject<string>();

            WritePair(wrapper.Value);
        }
        [TestMethod(nameof(test6))]
        private void test6()
        {
            NeedInitObject<int> wrapper = new NeedInitObject<int>();

            WritePair(wrapper.Value);
        }
    }
}
