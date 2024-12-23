using Common_Util.Data.Structure.Linear;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class CycleContainer001() : TestBase("对循环容器 CycleContainer 开发过程中的测试")
    {
        protected override void RunImpl()
        {
            RunTestMark();

        }

        [TestMethod]
        void test1()
        {
            CycleContainer<string> c1 = new CycleContainer<string>(8, "+");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("1");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("2");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("3");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("4");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("5");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("6");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("7");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("8");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("9");
            WritePair(c1.ToArray().FullInfoString());
            c1.Add("10");
            WritePair(c1.ToArray().FullInfoString());
            WritePair(c1.Peek(3).ToArray().FullInfoString());

        }

        [TestMethod]
        void test2()
        {
            try
            {
                CycleContainer<string?> c1 = new CycleContainer<string?>(8, null);
                WritePair(c1.ToArray().FullInfoString());
                CycleContainer<string> c2 = new CycleContainer<string>(-8, "+");
                WritePair(c2.ToArray().FullInfoString());
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        void test3()
        {
            CycleContainer<string> c1 = new CycleContainer<string>(["1", "2", "3"]);
            WritePair(c1.ToArray().FullInfoString());
        }

        [TestMethod]
        void test4()
        {
            string[] arr = ["1", "2", "3"];
            CycleContainer<string> c1 = CycleContainer<string>.Wrapper(arr);
            WritePair(c1.ToArray().FullInfoString());
            WritePair(arr.FullInfoString());

            c1.Add("4");
            WritePair(c1.ToArray().FullInfoString());
            WritePair(arr.FullInfoString());

            arr[0] = "5";
            WritePair(c1.ToArray().FullInfoString());
            WritePair(arr.FullInfoString());

            c1.Clear();
            WritePair(c1.ToArray().FullInfoString());
            WritePair(arr.FullInfoString());

            c1.Clear("芜湖");
            WritePair(c1.ToArray().FullInfoString());
            WritePair(arr.FullInfoString());
        }
    }
}