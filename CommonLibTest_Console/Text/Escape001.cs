using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Escape001() : TestBase("测试 EscapeHelper 及实现过程中的一些代码")
    {
        protected override void RunImpl()
        {
            RunTest(test1, "测试整理需转义字符");
            RunTest(test2, "asdqwaiofujewqg12564321asasdasdqwrf", "测试插入转义字符");
            RunTest(test2, "12312364asd56", "测试 2");
        }

        private void test1()
        {
            outputNeedEscapeChars('a', '1', '2', '3', '2', '1');
            outputNeedEscapeChars('a', '1', '1', '1', 'a', '1');
        }

        private void outputNeedEscapeChars(char escapeChar, params char[] needEscapes)
        {
            WriteLine("转义字符: " + escapeChar);
            WriteList(needEscapes.ToList(), "输入的需转义字符");

            char[] cArr = new char[needEscapes.Length + 1];
            cArr[0] = escapeChar;
            int totalChar = 1;
            bool existSame;
            for (int i = 0; i < needEscapes.Length; i++)
            {
                char inputNeed = needEscapes[i];
                existSame = false;
                for (int j = 0; j < totalChar; j++)
                {
                    if (inputNeed == cArr[j])
                    {
                        existSame = true;
                        break;
                    }
                }
                if (!existSame)
                {
                    cArr[totalChar] = inputNeed;
                    totalChar++;
                }
            }
            Span<char> needEscapeSpan = new Span<char>(cArr, 0, totalChar);

            WriteList(needEscapeSpan.ToArray().ToList(), "整理后的需转义字符");
        }

        private void test2(string input)
        {
            WriteLine();

            string t1 = EscapeHelper.AddEscape(input, 'a', '1', '2', '3', '2', '1');
            string t2 = EscapeHelper.RemoveEscape(t1, 'a');

            WritePair(key: "输    入", input);
            WritePair(key: "插入转义", t1);
            WritePair(key: "移除转义", t2);

            WriteLine();


            WriteLine("在插入转义字符后, 随机找个三个位置插入 1 2 3 ");
            WritePair(key: "插入转义前", t1);
            t1 = t1.Insert(Random.Shared.Next(t1.Length), "1");
            t1 = t1.Insert(Random.Shared.Next(t1.Length), "2");
            t1 = t1.Insert(Random.Shared.Next(t1.Length), "3");
            WritePair(key: "插入转义后", t1);

            WriteLine("遍历插入转义后的字符串");
            EscapeHelper.Ergodic(t1, 'a', 
                (c, b) =>
                {
                    string print = c.ToString();

                    if (!b)
                    {
                        switch (c)
                        {
                            case '1':
                                print += "   !!!";
                                break;
                            case '2':
                                print += "   2 2 2";
                                break;
                            case '3':
                                print += "   sss";
                                break;
                        }
                    }

                    if (b)
                    {
                        WritePair(key: "被转义字符: ", print);
                    }
                    else
                    {
                        WritePair(key: "普通字符:   ", print);
                    }
                });
        }
    }
}
