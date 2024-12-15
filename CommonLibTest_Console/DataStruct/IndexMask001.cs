using Common_Util.Data.Struct;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class IndexMask001() : TestBase("测试 IndexMask")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        static int[] TestInts = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        [TestMethod]
        void Test_ulong隐式转换()
        {
            ulong value = 0b_10101111_11001100;
            IndexMask mask = value;
            foreach (var (index, b) in mask.All().WithIndex())
            {
                WriteLine($"{index} => {b.ToString("1", "")}");
            }
        }

        [TestMethod]
        void Test_FromIEnumerableAndLength()
        {
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 8).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 13).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 16).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 25).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 32).ToFullString());
            WriteEmptyLine();
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 8).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 13).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 16).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 25).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 32).ToFullString());

            WriteEmptyLine();
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 8, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 13, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 16, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 25, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1".Select(i => i == '1'), 32, true).ToFullString());
            WriteEmptyLine();
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 8, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 13, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 16, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 25, true).ToFullString());
            WritePair(new IndexMask("1..1.1.1.1".Select(i => i == '1'), 32, true).ToFullString());
        }

        [TestMethod]
        void Test_Call()
        {
            IndexMask mask = IndexMask.FromTrueChar(".1.1.1111.", '1');
            WritePair(mask);
            mask.Reverse();
            WritePair(mask);
            _subCall1(mask);
            WritePair(mask);
            _subCall2(mask);
            WritePair(mask);
        }
        void _subCall1(IndexMask mask)
        {
            mask[1] = true;
            mask[2] = true;
            mask[3] = true;

            WritePair(nameof(_subCall1), mask);
        }
        void _subCall2(IndexMask mask)
        {
            mask.Reverse();

            WritePair(nameof(_subCall2), mask);
        }

        #region 左右移

        [TestMethod]
        void Test_左移_1()
        {
            IndexMask mask = IndexMask.FromTrueChar("....1.1.1111.", '1');
            WritePair(key: "左移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask << 1;
                WritePair(key: "  左移", mask.ToString(falseChar: ' '));
            }

        }
        [TestMethod]
        void Test_左移_8()
        {
            IndexMask mask = IndexMask.FromTrueChar("....1.1.1111..1...1.1.1111.", '1');
            WritePair(key: "左移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask << 8;
                WritePair(key: "  左移", mask.ToString(falseChar: ' '));
            }

        }
        [TestMethod]
        void Test_左移_9()
        {
            IndexMask mask = IndexMask.FromTrueChar(".1...1.1.111..1...1.1.1111.", '1');
            WritePair(key: "左移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask << 9;
                WritePair(key: "  左移", mask.ToString(falseChar: ' '));
            }

        }
        [TestMethod]
        void Test_右移_1()
        {
            IndexMask mask = IndexMask.FromTrueChar("....1.1.1111.", '1');
            WritePair(key: "右移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask >> 1;
                WritePair(key: "  右移", mask.ToString(falseChar: ' '));
            }

        }
        [TestMethod]
        void Test_右移_8()
        {
            IndexMask mask = IndexMask.FromTrueChar("....1.1.1111..1...1.1.1111.", '1');
            WritePair(key: "右移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask >> 8;
                WritePair(key: "  右移", mask.ToString(falseChar: ' '));
            }

        }
        [TestMethod]
        void Test_右移_9()
        {
            IndexMask mask = IndexMask.FromTrueChar(".1...1.1.111..1...1.1.1111.", '1');
            WritePair(key: "右移前", mask.ToString(falseChar: ' '));
            for (int i = 0; i < 10; i++)
            {
                mask = mask >> 9;
                WritePair(key: "  右移", mask.ToString(falseChar: ' '));
            }

        }
        #endregion

        #region 其他位运算
        [TestMethod]
        void Test_其他位运算_等长()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111...1.11.", '1');
            WritePair(key: "                                  mask1", mask1);
            WritePair(key: "                                  mask2", mask2);
            WritePair(key: "                                 ~mask1", ~mask1);
            WritePair(key: "                                 ~mask2", ~mask2);
            WritePair(key: "                          mask1 & mask2", mask1 & mask2);
            WritePair(key: "                          mask1 | mask2", mask1 | mask2);
            WritePair(key: "                     ~(~mask1 & ~mask2)", ~(~mask1 & ~mask2));
            WritePair(key: "~(mask1 & mask2) & (~(~mask1 & ~mask2))", ~(mask1 & mask2) & (~(~mask1 & ~mask2)));
            WritePair(key: "                          mask1 ^ mask2", mask1 ^ mask2);
        }
        [TestMethod]
        void Test_其他位运算_不等长_1()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.1111..111..11....", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111...1.11.", '1');

            WritePair(key: "                                  mask1", mask1);
            WritePair(key: "                                  mask2", mask2);
            WritePair(key: "                          mask1 & mask2", mask1 & mask2);
            WritePair(key: "                          mask1 | mask2", mask1 | mask2);
            WritePair(key: "                     ~(~mask1 & ~mask2)", ~(~mask1 & ~mask2));
            WritePair(key: "~(mask1 & mask2) & (~(~mask1 & ~mask2))", ~(mask1 & mask2) & (~(~mask1 & ~mask2)));
            WritePair(key: "                          mask1 ^ mask2", mask1 ^ mask2);

            WritePair((~(mask1 & mask2) & (~(~mask1 & ~mask2))) == (mask1 ^ mask2));
        }
        [TestMethod]
        void Test_其他位运算_不等长_2()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.1111..111..11....", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111...", '1');

            WritePair(key: "                                  mask1", mask1);
            WritePair(key: "                                  mask2", mask2);
            WritePair(key: "                          mask1 & mask2", mask1 & mask2);
            WritePair(key: "                          mask1 | mask2", mask1 | mask2);
            WritePair(key: "                     ~(~mask1 & ~mask2)", ~(~mask1 & ~mask2));
            WritePair(key: "~(mask1 & mask2) & (~(~mask1 & ~mask2))", ~(mask1 & mask2) & (~(~mask1 & ~mask2)));
            WritePair(key: "                          mask1 ^ mask2", mask1 ^ mask2);

            WritePair((~(mask1 & mask2) & (~(~mask1 & ~mask2))) == (mask1 ^ mask2));
        }
        [TestMethod]
        void Test_其他位运算_不等长_3()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.1111..111..1.1.1.", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111.1....................11", '1');

            WritePair(key: "                                  mask1", mask1);
            WritePair(key: "                                  mask2", mask2);
            WritePair(key: "                          mask1 & mask2", mask1 & mask2);
            WritePair(key: "                          mask1 | mask2", mask1 | mask2);
            WritePair(key: "                     ~(~mask1 & ~mask2)", ~(~mask1 & ~mask2));
            WritePair(key: "~(mask1 & mask2) & (~(~mask1 & ~mask2))", ~(mask1 & mask2) & (~(~mask1 & ~mask2)));
            WritePair(key: "                          mask1 ^ mask2", mask1 ^ mask2);

            WritePair((~(mask1 & mask2) & (~(~mask1 & ~mask2))) == (mask1 ^ mask2));
        }
        #endregion

        #region 相等比较
        [TestMethod]
        void Test_相等比较()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111...1.11.", '1');
            IndexMask mask3 = IndexMask.FromTrueChar(".1111...1.11.", '1');
            IndexMask mask4 = IndexMask.FromTrueChar(".1111...1.11..", '1');

            WritePair(mask1);
            WritePair(mask2);
            WritePair(mask3);
            WritePair(mask4);
            WritePair(mask1 == mask2);
            WritePair(mask2 == mask3);
            WritePair(mask3 == mask4);
        }
        #endregion

        [TestMethod]
        void Test_ToString_1()
        {
            ulong value = 0b_10101111_11001100;
            IndexMask mask = value;
            WritePair(mask.ToString());
            WritePair(mask.ToString(true));
            WritePair(mask.ToString(false));

        }

        [TestMethod]
        void Test_ToString_2()
        {
            IndexMask mask = 0b_10101111_11001100;
            WritePair(mask.ToString());
            WritePair(mask.ToString(true));
            WritePair(mask.ToString(false));
        }

        [TestMethod]
        void Test_ToString_3()
        {
            IndexMask mask = 0b_11001010;
            WritePair(mask.ToString());
            WritePair(mask.ToString(true));
            WritePair(mask.ToString(false));
        }

        [TestMethod]
        void Test_GetSet()
        {
            IndexMask mask = 0b_11001010;
            WritePair(mask);    // 高位在前

            WritePair(mask[3]);
            WritePair(mask[4]);

            WriteLine("mask[3] = false;");
            mask[3] = false;
            WritePair(mask);

            WriteLine("mask[4] = false;");
            mask[4] = false;
            WritePair(mask);

            WritePair(mask[3]);
            WritePair(mask[4]);
        }

        [TestMethod]
        void Test_FromICollection()
        {
            ICollection<bool> bs = [true, false, true, false, false, true];
            IndexMask mask = new(bs);
            WritePair(mask);

            WritePair(mask = new([false, false, false, true, true, false, true, false]));

            IEnumerable<bool> bs2 = [false, false, false, true, true, false, true, false];
            mask = new(bs2);
            WritePair(mask);

            mask = new bool[] { true, false, false, true };
            WritePair(mask);
        }

        [TestMethod]
        void Test_Filtering()
        {
            WritePair(TestInts.Filtering(new bool[] { true, false, true, false, true, true }, true, true).ToArray().FullInfoString());
            WritePair(TestInts.Filtering(new bool[] { true, false, true, false, true, true }, false, true).ToArray().FullInfoString());
            WritePair(TestInts.Filtering(new bool[] { true, false, true, false, true, true }, false, false).ToArray().FullInfoString());
            WritePair(TestInts.Filtering(new bool[] { true, false, true, false, true, true }, true, false).ToArray().FullInfoString());
        }
        [TestMethod]
        void Test_Replace()
        {
            WritePair(TestInts.Replace(IndexMask.FromTrueChar(".1..1.", '1'), i => -i, true, true).ToArray().FullInfoString());

            WritePair(TestInts.Replace(new bool[] { true, false, true, false, true, true }, i => -i, true, true).ToArray().FullInfoString());
            WritePair(TestInts.Replace(new bool[] { true, false, true, false, true, true }, i => -i, false, true).ToArray().FullInfoString());
            WritePair(TestInts.Replace(new bool[] { true, false, true, false, true, true }, i => -i, false, false).ToArray().FullInfoString());
            WritePair(TestInts.Replace(new bool[] { true, false, true, false, true, true }, i => -i, true, false).ToArray().FullInfoString());
        }

    }
}
