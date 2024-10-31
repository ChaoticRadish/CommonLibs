using Common_Util.Data.Structure.Linear;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    class CyclePointer001() : TestBase("循环指针开发过程中的一些测试")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }


        [TestMethod]
        void test1()
        {
            CyclePointer<int> p = new(true, 1, 5, 9);
            foreach (int i in 9.ForUntil())
            {
                WritePair(p.Current);
                p.Move(1);
            }
        }
        [TestMethod]
        void test2()
        {
            CyclePointer<int> p = new(true, 1, 5, 9);
            foreach (int i in 9.ForUntil())
            {
                WritePair(p.Current);
                p.Move(-1);
            }
        }
        [TestMethod]
        void test3()
        {
            CyclePointer<int> p = new(true, 1, 5, 9);
            foreach (int i in 9.ForUntil())
            {
                WritePair(p.Current);
                p.Move(2);
            }
        }
        [TestMethod]
        void test4()
        {
            CyclePointer<int> p = new(true, 1, 5, 9);
            foreach (int i in 9.ForUntil())
            {
                WritePair(p.Current);
                p.Move(-2);
            }
        }


        struct TestData(int i) : IIncrementOperators<TestData>, IDecrementOperators<TestData>, IComparisonOperators<TestData, TestData, bool>
        {
            public int X = i;
            public int Y = i;

            public override string ToString()
            {
                return $"X: {X} - Y: {Y}";
            }
            public static TestData operator ++(TestData value)
            {
                return new TestData
                {
                    X = value.X + 5,
                    Y = value.Y + 5
                };
            }

            public static TestData operator --(TestData value)
            {
                return new TestData
                {
                    X = value.X - 5,
                    Y = value.Y - 5
                };
            }

            public static bool operator ==(TestData left, TestData right)
            {
                return left.X == right.X && left.Y == right.Y;
            }

            public static bool operator !=(TestData left, TestData right)
            {
                return !(left == right);
            }

            public static bool operator <(TestData left, TestData right)
            {
                return left.X < right.X;
            }

            public static bool operator >(TestData left, TestData right)
            {
                return !(left == right) && !(left < right);
            }

            public static bool operator <=(TestData left, TestData right)
            {
                return (left == right) || (left < right);
            }

            public static bool operator >=(TestData left, TestData right)
            {
                return (left == right) || (left > right);
            }

            public readonly override bool Equals(object? obj)
            {
                if (obj is TestData other)
                {
                    return this == other;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return X;
            }
        }
        [TestMethod]
        void test5()
        {
            TestData test = new TestData(10);
            WritePair(test);
            WritePair(test++);
            WritePair(test++);
            WritePair(test++);
            WritePair(test--);
            WritePair(test--);
            WritePair(test--);
            WritePair(test);
            WritePair(++test);
            WritePair(++test);
            WritePair(++test);
            WritePair(--test);
            WritePair(--test);
            WritePair(--test);
            WritePair(test);
        }

        [TestMethod]
        void test6()
        {
            CyclePointer<TestData> p = new(true, new(0), new(5), new(100));
            WriteLine($"Length:{p.Length} Current:{p.Current} Min:{p.Min} Max:{p.Max}");
        }
        [TestMethod]
        void test7()
        {
            try
            {
                CyclePointer<TestData> p = new(true, new(0), new(5), new(96));
                WriteLine($"Length:{p.Length} Current:{p.Current} Min:{p.Min} Max:{p.Max}");
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
        [TestMethod]
        void test8()
        {
            try
            {
                CyclePointer<TestData> p = new(true, new(6), new(5), new(100));
                foreach (int i in 25.ForUntil())
                {
                    WritePair(p.Current);
                    p.Move(1);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
        [TestMethod]
        void test9()
        {
            try
            {
                CyclePointer<TestData> p = new(true, new(30), new(5), new(100));
                foreach (int i in 25.ForUntil())
                {
                    WritePair(p.Current);
                    p.Move(1);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
    }
}
