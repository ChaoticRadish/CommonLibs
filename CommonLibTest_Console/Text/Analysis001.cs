using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Analysis001() : TestBase("字符串解析测试 001, 测试 TryReadUntil 寻找特定字符串")
    {
        List<string> failures = new List<string>();

        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // AI 生成的测试内容, 有重复
            // 1. 正常情况：能找到字符串
            RunTest("hello world", "world", 0, null, true, "hello ", "1. 正常查找");

            // 2. 找不到字符串的情况
            RunTest("hello world", "abc", 0, null, false, null, "2. 找不到字符串");

            // 3. startIndex 为负数（调整为 0，maxReadLength 不变）
            RunTest("hello", "llo", -2, null, true, "he", "3. startIndex 为负数，maxReadLength 为 null");

            // 4. startIndex 为负数，maxReadLength 减少后不足以找到字符串
            RunTest("hello", "llo", -2, 5, false, null, "4. startIndex 为负数，maxReadLength 减少后不足以找到字符串");

            // 5. startIndex 为负数，maxReadLength 减少后为负数（返回失败）
            RunTest("hello", "llo", -5, 3, false, null, "5. startIndex 为负数，maxReadLength 减少后为负数");

            // 6. startIndex 超过字符串长度
            RunTest("hello", "o", 10, null, false, null, "6. startIndex 超出长度");

            // 7. maxReadLength 为 0
            RunTest("hello", "h", 0, 0, false, null, "7. maxReadLength 为 0");

            // 8. maxReadLength 为负数
            RunTest("hello", "h", 0, -1, false, null, "8. maxReadLength 为负数");

            // 9. maxReadLength 限制导致找不到
            RunTest("hello world", "world", 0, 5, false, null, "9. maxReadLength 过小无法找到");

            // 10. maxReadLength 刚好能找到
            RunTest("hello world", "world", 0, 11, true, "hello ", "10. maxReadLength 刚好足够");

            // 11. 空输入字符串
            RunTest("", "test", 0, null, false, null, "11. 空输入字符串");

            // 12. 查找空字符串（input 不为 null，返回 true 和空字符串）
            RunTest("hello", "", 2, null, true, "", "12. 查找空字符串，input 不为 null");

            // 13. 查找空字符串，startIndex 为负数
            RunTest("hello", "", -2, null, true, "", "13. 查找空字符串，startIndex 为负数");

            // 14. 查找空字符串，maxReadLength 限制但足够
            RunTest("hello", "", 0, 5, true, "", "14. 查找空字符串，maxReadLength 足够");

            // 15. 查找空字符串，maxReadLength 减少后为负数
            RunTest("hello", "", -5, 3, true, "", "15. 查找空字符串，maxReadLength 减少后为负数");

            // 16. 查找字符串等于输入
            RunTest("test", "test", 0, null, true, "", "16. 查找字符串等于输入");

            // 17. maxReadLength 与 startIndex 联动调整（缩减后不足）
            RunTest("hello world", "world", -3, 10, false, null, "17. startIndex 负数与 maxReadLength 联动（缩减后不足）");

            // 18. maxReadLength 与 startIndex 联动调整（缩减后足够）
            RunTest("hello world", "world", -3, 14, true, "hello ", "18. startIndex 负数与 maxReadLength 联动（缩减后足够）");

            // 19. Unicode 字符测试
            RunTest("hello 世界", "世界", 0, null, true, "hello ", "19. Unicode 字符");

            // 20. 开始位置在字符串中间
            RunTest("hello world", "world", 6, null, true, "", "20. 从中间位置开始查找");

            // 21. maxReadLength 精确截断
            RunTest("hello world", "world", 0, 6, false, null, "21. maxReadLength 精确截断在匹配前");

            // 22. 重叠查找模式
            RunTest("ababab", "abab", 0, null, true, "", "22. 重叠模式查找");

            // 23. 查找单个字符
            RunTest("hello", "l", 0, null, true, "he", "23. 查找单个字符");

            // 24. 最大读取长度限制在匹配开始处
            RunTest("hello world", "world", 0, 7, false, null, "24. maxReadLength 终止在匹配开始处");

            // 25. 查找字符串在开头
            RunTest("test123", "test", 0, null, true, "", "25. 查找字符串在开头");

            // 26. 查找字符串在末尾
            RunTest("123test", "test", 0, null, true, "123", "26. 查找字符串在末尾");

            // 27. 多次出现查找第一个
            RunTest("test test", "test", 0, null, true, "", "27. 多次出现查找第一个");

            // 28. 从中间开始查找后面的出现
            RunTest("test test", "test", 4, null, true, " ", "28. 从中间开始查找后面的出现");

            // 29. 最大读取长度刚好截断在查找字符串前
            RunTest("123456", "456", 0, 3, false, null, "29. 最大读取长度刚好截断在查找字符串前");

            // 30. 最大读取长度包含部分查找字符串但不完整
            RunTest("123456", "456", 0, 4, false, null, "30. 最大读取长度包含部分查找字符串但不完整");

            // 31. startIndex 为负数，maxReadLength 减少后为 0
            RunTest("hello", "llo", -3, 3, false, null, "31. startIndex 为负数，maxReadLength 减少后为 0");

            // 32. startIndex 为负数，maxReadLength 减少后刚好足够
            RunTest("hello", "he", -2, 4, true, "", "32. startIndex 为负数，maxReadLength 减少后刚好足够");

            // 33. 查找空字符串，input 为空字符串
            RunTest("", "", 0, null, true, "", "33. 查找空字符串，input 为空字符串");

            // 34. 查找空字符串，input 为空字符串，startIndex 为负数
            RunTest("", "", -2, null, true, "", "34. 查找空字符串，input 为空字符串，startIndex 为负数");

            // 35. 查找空字符串，input 为空字符串，maxReadLength 减少后为负数
            RunTest("", "", -2, 1, true, "", "35. 查找空字符串，input 为空字符串，maxReadLength 减少后为负数");

            // 36. 边界情况：最大读取长度刚好在查找字符串开始处
            RunTest("123456", "456", 0, 3, false, null, "36. 最大读取长度刚好在查找字符串开始处");

            // 37. 边界情况：最大读取长度包含查找字符串的一部分
            RunTest("123456", "456", 0, 4, false, null, "37. 最大读取长度包含查找字符串的一部分");

            // 38. 边界情况：最大读取长度刚好包含整个查找字符串
            RunTest("123456", "456", 0, 6, true, "123", "38. 最大读取长度刚好包含整个查找字符串");

            if (failures.Count != 0)
            {
                WriteList(failures, "失败列表", true, "");
            }
            else
            {
                WriteLine("均成功");
            }
        }
        /// <param name="input">输入值</param>
        /// <param name="findStr">需要查找的值</param>
        /// <param name="startIndex">在 <paramref name="input"/> 中的查找起点</param>
        /// <param name="maxReadLength">非 null 时, 限制从 <paramref name="startIndex"/> 开始允许读取多少字符</param>
        /// <param name="expectedResult">预期返回值</param>
        /// <param name="expectedOutput">预期 output 值</param>
        /// <param name="description">测试的描述信息</param>
        private void RunTest(string input, string findStr, int startIndex, int? maxReadLength,
                              bool expectedResult, string? expectedOutput, string description)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{description}");
            Logger.Info($"输入: \"{input}\", 查找: \"{findStr}\", 起始: {startIndex}, 最大长度: {maxReadLength?.ToString() ?? "无限制"}");

            try
            {
                bool result = StringAnalysis.TryReadUntil(input, findStr, startIndex, maxReadLength, out string? output);

                if (result == expectedResult)
                {
                    if (result)
                    {
                        if (output == expectedOutput)
                        {
                            Logger.Info($"√ 成功: 返回True, 输出\"{output}\"");
                        }
                        else
                        {
                            Logger.Warning($"X !失败!: 输出不匹配 (预期\"{expectedOutput}\", 实际\"{output}\")");
                            failures.Add(description);
                        }
                    }
                    else
                    {
                        Logger.Info($"√ 成功: 返回False, 输出null");
                    }
                }
                else
                {
                    Logger.Warning($"X !失败!: 返回值不匹配 (预期{expectedResult}, 实际{result})");
                    failures.Add(description);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"! 异常: {ex.Message}");
            }

            Logger.Info("----------------------------------------");
        }
    }
}
