using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Analysis005() : TestBase("测试字符序列读取器")
    {

        protected override void RunImpl()
        {
            runSuccessTest(chars => new CharSequenceReader(chars));
            runSuccessTest2(chars => new CharSequenceReader(chars));
            runBusinessTest(chars => new CharSequenceReader(chars));
        }

        int testIndex = 0;
        private void runSuccessTest(Func<IEnumerable<char>, CharSequenceReader> getReaderFunc)
        {
            WriteLine("-------------------------------------------");
            WriteLine($"执行测试 {++testIndex} 预期一切顺利");
            try
            {
                string str = "ABCDEFGHIJKLMN{123}111{321}21\"321\"AA321";

                WriteLine("原文:" + str);
                var reader = getReaderFunc(str);
                WritePair(reader.GetStatusString(), "初始状态", "\n");
                WriteLine();
                reader.Skip(5);
                WritePair(reader.GetStatusString(), "Skip(5)", "\n");
                WriteLine();

                bool readResult1 = reader.TryPeekValue(8, out char output1);
                WritePair(reader.GetStatusString(), $"TryPeekValue(8), result:{readResult1}, output:{output1}", "\n");  // 当前缓存应为: FGHIJKLMN, output 为 N
                WriteLine();

                bool readResult2 = reader.TryPeekValue(5, out char output2);
                WritePair(reader.GetStatusString(), $"TryPeekValue(5), result:{readResult2}, output:{output2}", "\n");  // 当前缓存应为: FGHIJKLMN, output 为 K
                WriteLine();

                bool readResult3 = reader.TryReadUntil("LMN", out var output3);
                WritePair(reader.GetStatusString(), $"TryReadUntil(\"LMN\"), result:{readResult3}, output:{output3}", "\n");    // 当前缓存应为: LMN, output 为 FGHIJK
                WriteLine();

                bool readResult4 = reader.TryReadUntil("123", out var output4);
                WritePair(reader.GetStatusString(), $"TryReadUntil(\"123\"), result:{readResult4}, output:{output4}", "\n");    // 当前缓存应为: 123, output 为 LMN{
                WriteLine();

                bool readResult5 = reader.TryReadUntilIgnoreStringText("321", out var output5);
                WritePair(reader.GetStatusString(), $"TryReadUntilIgnoreStringText(\"321\"), result:{readResult5}, output:{output5}", "\n");    // 当前缓存应为: 321, output 为 123}111{
                WriteLine();

                reader.Skip(3);
                WritePair(reader.GetStatusString(), "Skip(3)", "\n");
                WriteLine();

                bool readResult6 = reader.TryReadUntilIgnoreStringText("321", out var output6);
                WritePair(reader.GetStatusString(), $"TryReadUntilIgnoreStringText(\"321\"), result:{readResult6}, output:{output6}", "\n");    // 当前缓存应为: 321, output 为 }21"321"AA
            }
            catch (Exception ex)
            {
                WriteLine("执行异常: \n" + ex.ToString()); ;
            }
            WriteLine("-------------------------------------------");
            WriteLine();
        }

        private void runSuccessTest2(Func<IEnumerable<char>, CharSequenceReader> getReaderFunc)
        {
            WriteLine("-------------------------------------------");
            WriteLine($"执行测试 {++testIndex} 预期一切顺利");
            try
            {
                string str = "ABCDEFGHIJKLMN{123}111{321}21\"321\"AA321";

                WriteLine("原文:" + str);
                var reader = getReaderFunc(str);
                WritePair(reader.GetStatusString(), "初始状态", "\n");
                WriteLine();
                reader.Skip(5);
                WritePair(reader.GetStatusString(), "Skip(5)", "\n");
                WriteLine();

                bool readResult1 = reader.TryReadUntil("F", out var output1);
                WritePair(reader.GetStatusString(), $"TryReadUntil(\"F\"), result:{readResult1}, output:{output1}", "\n");  // 当前缓存应为: 空, output 为 空
                WriteLine();

                reader.Skip(1);
                WritePair(reader.GetStatusString(), "Skip(1)", "\n");
                WriteLine();

                bool readResult2 = reader.TryReadUntil("F", out var output2);
                WritePair(reader.GetStatusString(), $"TryReadUntil(\"F\"), result:{readResult2}, output:{output2}", "\n");  // 当前缓存应为: 空, output 为 GHIJKLMN{123}111{321}21\"321\"AA321
                WriteLine();

            }
            catch (Exception ex)
            {
                WriteLine("执行异常: \n" + ex.ToString()); ;
            }
            WriteLine("-------------------------------------------");
            WriteLine();
        }

        private void runBusinessTest(Func<IEnumerable<char>, CharSequenceReader> getReaderFunc)
        {
            WriteLine("-------------------------------------------");
            WriteLine($"执行测试 {++testIndex} 测试业务场景, 预期一切顺利");
            try
            {
                void test(string str)
                {
                    WriteLine("=".Repeat(10));
                    WriteLine("原文:" + str);
                    var reader = getReaderFunc(str);
                    WritePair(reader.GetStatusString(), "初始状态", "\n");

                    int index = 0;
                    string? found;
                    while (reader.TryReadUntilIgnoreStringText(index % 2 == 0 ? "{" : "}", out found))
                    {
                        WritePair((index % 2 == 0 ? string.Empty : "[inner]") + found.WhenEmptyDefault("<empty>"), $"Pair {index}");
                        index++;
                        reader.Skip(1);
                    }
                    WritePair((index % 2 == 0 ? string.Empty : "[inner]") + found.WhenEmptyDefault("<empty>"), $"Last");
                }

                test("ABCDEFGHIJKLMN{123}111{321}21{getValue(\"321\")}AA321");
                // Pair 0 => ABCDEFGHIJKLMN
                // Pair 1 => [inner]123
                // Pair 2 => 111
                // Pair 3 => [inner]321
                // Pair 4 => 21
                // Pair 5 => [inner]getValue("321")
                // Last => AA321

                test("{123}111{321}21{getValue(\"321\")}");
                // Pair 0 => <empty>
                // Pair 1 => [inner]123
                // Pair 2 => 111
                // Pair 3 => [inner]321
                // Pair 4 => 21
                // Pair 5 => [inner]getValue("321")
                // Last => <empty>

                test("{123{111}321}21{getValue(\"321\")}");
                // Pair 0 => <empty>
                // Pair 1 => [inner]123{111
                // Pair 2 => 321}21
                // Pair 3 => [inner]getValue("321")
                // Last => <empty>
            }
            catch (Exception ex)
            {
                WriteLine("执行异常: \n" + ex.ToString()); ;
            }
            WriteLine("-------------------------------------------");
            WriteLine();
        }
    }
}
