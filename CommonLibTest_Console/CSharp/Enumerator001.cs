using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Enumerator001() : TestBase("对迭代器类型的学习")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void test1()
        {
            IEnumerable<int> test = getTestEnumerable();
            foreach (int i in test)
            {
                WriteLine("got: " + i);
            }
        }
        [TestMethod]
        void test2()
        {
            IEnumerable<int> test = getTestEnumerable();
            foreach (int i in test)
            {
                WriteLine("got: " + i);
                if (i == 3)
                {
                    WriteLine("break");
                    break;
                }
            }
        }
        [TestMethod]
        void test3()
        {
            IEnumerable<int> test = getTestEnumerable();
            WritePair(test.Any());
        }

        [TestMethod]
        void test4()
        {
            IEnumerable<int> test = getTestEnumerable();
            WritePair(test.Any());
            foreach (int i in test)
            {
                WriteLine("got: " + i);
            }
        }

        IEnumerable<int> getTestEnumerable()
        {
            WriteLine("getTestEnumerable start");
            WriteLine("getTestEnumerable 1");
            yield return 1;
            WriteLine("getTestEnumerable 2");
            yield return 2;
            WriteLine("getTestEnumerable 3");
            yield return 3;
            WriteLine("getTestEnumerable 4");
            yield return 4;
            WriteLine("getTestEnumerable end");
        }
    }
}
