using Common_Util.Data.Structure.Pair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class DisorderPair001() : TestBase("无序对开发过程中的测试")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void Test_单泛型参数_1()
        {
            DisorderPair<string> p1 = ("123", "321");
            DisorderPair<string> p2 = new KeyValuePair<string, string>("321", "123");
            WritePair(p1 == p2);
        }
        [TestMethod]
        void Test_单泛型参数_2()
        {
            DisorderPair<string> p1 = ("123", "32");
            DisorderPair<string> p2 = new KeyValuePair<string, string>("321", "123");
            WritePair(p1 == p2);
        }

        [TestMethod]
        void Test_双泛型参数_1()
        {
            DisorderPair<string, int> p1 = ("123", 321);
            DisorderPair<string, int> p2 = new KeyValuePair<int, string>(321, "123");
            WritePair(p1 == p2);
        }

        [TestMethod]
        void Test_双泛型参数_2()
        {
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<string, int> p2 = new KeyValuePair<int, string>(321, "123");
            WritePair(p1 == p2);
        }
        [TestMethod]
        void Test_双泛型参数_3()
        {
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<int, string> p2 = p1;
            WritePair(p1 == p2);
        }
        [TestMethod]
        void Test_双泛型参数_4()
        {
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<int, string> p2 = new KeyValuePair<int, string>(32, "123");
            WritePair(p1 == p2);
        }
    }
}
