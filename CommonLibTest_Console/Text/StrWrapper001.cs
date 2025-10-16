using BenchmarkDotNet.Running;
using Common_Util.Data.Wrapper;
using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class StrWrapper001() : TestBase("测试字符串集合包装器")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // 准备一个标准的测试数组
            string[] sampleArray = { "hello", null!, " world ", "", "~" };
            // 包装后的内容是: "hello world ~" (长度为 13)

            WriteLine("========= 测试基础操作 =========");

            var wrapper = StringEnumerableWrapper.Create(sampleArray);
            WritePair(wrapper[3]);
            WritePair(wrapper[^3]);
            WritePair(new string(wrapper[1..^1].ToArray()));
            WritePair(new string(wrapper[..5].ToArray()));

            WriteEmptyLine(3);



            var Logger = GetLevelLogger("测试");

            Logger.Info("========= Index 测试 =========");
            // 1. 正常情况测试
            test2(sampleArray, 0, false, 'h', "1. 获取首个字符");
            test2(sampleArray, ^1, false, '~', "2. 获取末尾字符");
            test2(sampleArray, 4, false, 'o', "3. 获取第一个字符串的末尾字符");
            test2(sampleArray, 5, false, ' ', "4. 获取第二个字符串的首字符");
            test2(sampleArray, 11, false, ' ', "5. 获取倒数第二个字符 (空格)");

            // 2. 异常情况测试
            test2(sampleArray, 13, true, '\0', "6. 越界访问 (index == Count)");
            // test2(sampleArray, -1, true, '\0', "7. 负索引访问"); 负数转换为 Index 就会报错了
            test2(new string[] { null!, "" }, 0, true, '\0', "8. 空内容访问");

            Logger.Info("========= Range 测试 =========");
            // 1. 正常情况测试
            test1(sampleArray, .., false, "hello world ~", "1. 截取全文");
            test1(sampleArray, 0..5, false, "hello", "2. 截取第一个单词");
            test1(sampleArray, 5.., false, " world ~", "3. 从第5个字符截取到末尾");
            test1(sampleArray, ..^1, false, "hello world ", "4. 截取到倒数第一个字符");
            test1(sampleArray, 6..11, false, "world", "5. 截取第二个单词和符号");
            test1(sampleArray, ^6..^1, false, "orld ", "6. 使用负索引截取中间部分");

            // 2. 边界情况测试
            test1(sampleArray, 0..0, false, "", "7. 空范围 (start == end)");
            test1(sampleArray, 5..5, false, "", "8. 空范围 (start == end, 非零)");
            test1(sampleArray, 20..30, true, "", "9. 越界范围 (start > Count)");    // 触发的是 Range 的范围判断
            test1(sampleArray, 8..2, true, "", "10. 起始索引大于结束索引");  // 触发的是 Range 的范围判断
            test1(new string[] { null!, "" }, .., false, "", "11. 空内容范围");


            Logger.Info("========= 结算 =========");

            if (failures.Count != 0)
            {
                Logger.Info("失败列表: " + string.Join("\n", failures));
            }
            else
            {
                Logger.Info("均成功");
            }
        }

        /// <summary>
        /// 执行测试
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="range"></param>
        /// <param name="预期异常"></param>
        /// <param name="预期结果"></param>
        /// <param name="测试描述">格式为 数字. 描述内容, 如: 1. 截取全文</param>
        private void test1(string[] arr, Range range, bool 预期异常, string 预期结果, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入: {string.Join(",", arr)}; 预期: {(预期异常 ? "异常" : $"\"{预期结果}\"")}");

            try
            {
                var wrapper = StringEnumerableWrapper.Create(arr);
                try
                {
                    string got = new string(wrapper[range].ToArray());
                    if (预期异常)
                    {
                        Logger.Info($"X !失败!: 未如预期发生异常");
                        failures.Add(测试描述);
                    }
                    if (got == 预期结果)
                    {
                        Logger.Info($"√ 成功: 取得预期结果: {got}");
                    }
                    else
                    {
                        Logger.Info($"X !失败!: 输出不匹配 (预期\"{预期结果}\", 实际\"{got}\")");
                        failures.Add(测试描述);
                    }
                }
                catch (Exception ex)
                {
                    if (预期异常)
                    {
                        Logger.Info($"√ 成功: 如预期发生了异常, 异常信息: {ex.Message}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"! 异常: {ex.Message}");
                failures.Add(测试描述);
            }

            Logger.Info("----------------------------------------");
        }

        /// <summary>
        /// 执行测试
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        /// <param name="预期异常"></param>
        /// <param name="预期结果"></param>
        /// <param name="测试描述">格式为 数字. 描述内容, 如: 1. 获取首个字符</param>
        private void test2(string[] arr, Index index, bool 预期异常, char 预期结果, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入: {string.Join(",", arr)}; 预期: {(预期异常 ? "异常" : $"'{预期结果}'")}");

            try
            {
                var wrapper = StringEnumerableWrapper.Create(arr);
                try
                {
                    char got = wrapper[index];
                    if (预期异常)
                    {
                        Logger.Info($"X !失败!: 未如预期发生异常");
                        failures.Add(测试描述);
                    }
                    if (got == 预期结果)
                    {
                        Logger.Info($"√ 成功: 取得预期结果: {got}");
                    }
                    else
                    {
                        Logger.Info($"X !失败!: 输出不匹配 (预期'{预期结果}', 实际'{got}')");
                        failures.Add(测试描述);
                    }
                }
                catch (Exception ex)
                {
                    if (预期异常)
                    {
                        Logger.Info($"√ 成功: 如预期发生了异常, 异常信息: {ex.Message}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"! 异常: {ex.Message}");
                failures.Add(测试描述);
            }

            Logger.Info("----------------------------------------");
        }
    }
}
