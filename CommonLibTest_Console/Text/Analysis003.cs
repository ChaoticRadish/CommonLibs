using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Analysis003() : TestBase("字符串解析测试 002, 测试 TryReadUntilOrEnd 寻找特定一组字符串 (匹配其中任一项)")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // AI 生成的测试内容, 有重复
            // 1. 正常情况：能找到其中一个字符串
            RunTest("hello world", new[] { "world", "abc" }, 0, null, true, "hello ", "world", "1. 正常查找，匹配第一个");

            // 2. 正常情况：匹配数组中的第二个字符串
            RunTest("hello world", new[] { "abc", "world" }, 0, null, true, "hello ", "world", "2. 正常查找，匹配第二个");

            // 3. 找不到任何字符串的情况，返回范围内的所有字符
            RunTest("hello world", new[] { "abc", "xyz" }, 0, null, true, "hello world", null, "3. 找不到任何字符串，返回全部");

            // 4. 优先顺序测试：匹配第一个出现的字符串
            RunTest("hello world", new[] { "world", "hello" }, 0, null, true, "", "hello", "4. 优先顺序测试，匹配第一个");

            // 5. startIndex 为负数，maxReadLength 减少后不足以找到任何字符串，返回范围内的字符
            RunTest("hello", new[] { "llo", "abc" }, -2, 5, true, "hel", null, "5. startIndex 为负数，maxReadLength 减少后不足");

            // 6. startIndex 为负数，maxReadLength 减少后为负数
            RunTest("hello", new[] { "llo", "abc" }, -5, 3, true, "", null, "6. startIndex 为负数，maxReadLength 减少后为负数");

            // 7. startIndex 超过字符串长度，返回空字符串
            RunTest("hello", new[] { "o", "abc" }, 10, null, true, "", null, "7. startIndex 超出长度");

            // 8. maxReadLength 为 0，返回空字符串
            RunTest("hello", new[] { "h", "abc" }, 0, 0, true, "", null, "8. maxReadLength 为 0");

            // 9. maxReadLength 限制导致找不到任何字符串，返回范围内的字符
            RunTest("hello world", new[] { "world", "abc" }, 0, 5, true, "hello", null, "9. maxReadLength 过小无法找到任何字符串");

            // 10. 空输入字符串
            RunTest("", new[] { "test", "abc" }, 0, null, true, "", null, "10. 空输入字符串");

            // 11. 查找字符串数组包含空字符串（input 不为 null，返回 true 和空字符串）
            RunTest("hello", new[] { "", "abc" }, 2, null, true, "", "", "11. 查找字符串数组包含空字符串");

            // 12. 查找字符串数组为空（input 不为 null，返回 true 和空字符串）
            RunTest("hello", new string[0], 0, null, true, "", "", "12. 查找字符串数组为空");

            // 13. 查找字符串数组全部为空字符串
            RunTest("hello", new[] { "", "" }, 0, null, true, "", "", "13. 查找字符串数组全部为空字符串");

            // 14. maxReadLength 与 startIndex 联动调整（缩减后不足）
            RunTest("hello world", new[] { "world", "abc" }, -3, 10, true, "hello w", null, "14. startIndex 负数与 maxReadLength 联动（缩减后不足）");

            // 15. Unicode 字符测试
            RunTest("hello 世界", new[] { "世界", "abc" }, 0, null, true, "hello ", "世界", "15. Unicode 字符");

            // 16. 开始位置在字符串中间
            RunTest("hello world", new[] { "world", "abc" }, 6, null, true, "", "world", "16. 从中间位置开始查找");

            // 17. 重叠查找模式
            RunTest("ababab", new[] { "abab", "baba" }, 0, null, true, "", "abab", "17. 重叠模式查找");

            // 18. 查找单个字符
            RunTest("hello", new[] { "l", "x" }, 0, null, true, "he", "l", "18. 查找单个字符");

            // 19. 最大读取长度限制在匹配开始处，返回范围内的字符
            RunTest("hello world", new[] { "world", "abc" }, 0, 7, true, "hello w", null, "19. maxReadLength 终止在匹配开始处");

            // 20. 查找字符串在开头
            RunTest("test123", new[] { "test", "123" }, 0, null, true, "", "test", "20. 查找字符串在开头");

            // 21. 查找字符串在末尾
            RunTest("123test", new[] { "test", "123" }, 0, null, true, "", "123", "21. 查找字符串在末尾");

            // 22. 多次出现查找第一个
            RunTest("test test", new[] { "test", "abc" }, 0, null, true, "", "test", "22. 多次出现查找第一个");

            // 23. 从中间开始查找后面的出现
            RunTest("test test", new[] { "test", "abc" }, 4, null, true, " ", "test", "23. 从中间开始查找后面的出现");

            // 24. 最大读取长度刚好截断在查找字符串前，返回范围内的字符
            RunTest("123456", new[] { "456", "789" }, 0, 3, true, "123", null, "24. 最大读取长度刚好截断在查找字符串前");

            // 25. 最大读取长度包含部分查找字符串但不完整，返回范围内的字符
            RunTest("123456", new[] { "456", "789" }, 0, 4, true, "1234", null, "25. 最大读取长度包含部分查找字符串但不完整");

            // 26. 边界情况：最大读取长度刚好包含整个查找字符串
            RunTest("123456", new[] { "456", "789" }, 0, 6, true, "123", "456", "26. 最大读取长度刚好包含整个查找字符串");

            // 27. 特殊情况：从中间开始查找，找不到任何字符串时返回剩余部分
            RunTest("hello world", new[] { "abc", "xyz" }, 6, null, true, "world", null, "27. 从中间开始查找，找不到任何字符串时返回剩余部分");

            // 28. 特殊情况：maxReadLength 限制返回的部分
            RunTest("hello world", new[] { "abc", "xyz" }, 6, 3, true, "wor", null, "28. maxReadLength 限制返回的部分");

            // 29. 特殊情况：查找字符串数组包含空字符串和有效字符串
            RunTest("hello", new[] { "", "ll" }, 0, null, true, "", "", "29. 查找字符串数组包含空字符串和有效字符串");

            // 30. 优先级测试：前面的字符串部分匹配，后面的字符串完全匹配
            RunTest("hello world", new[] { "hell", "world" }, 0, null, true, "", "hell", "30. 优先级测试，前面的字符串部分匹配");

            // 31. 优先级测试：前面的字符串完全匹配
            RunTest("hello world", new[] { "hello", "world" }, 0, null, true, "", "hello", "31. 优先级测试，前面的字符串完全匹配");

            // 32. 部分匹配测试：多个字符串都有部分匹配
            RunTest("abcdef", new[] { "abc", "cde", "def" }, 0, null, true, "", "abc", "32. 多个字符串都有部分匹配，优先匹配第一个");

            // 33. 边界情况：查找字符串数组包含 null 值
            RunTest("hello", new[] { "ll", null }, 0, null, true, "", "", "33. 查找字符串数组包含 null 值");

            // 34. 边界情况：所有查找字符串都为 null
            RunTest("hello", new string?[] { null, null }, 0, null, true, "", "", "34. 所有查找字符串都为 null");

            // 35. 查找字符串数组为 null
            RunTest("hello", null!, 0, null, true, "", "", "35. 查找字符串数组为 null");

            // 36. 查找字符串数组为 []
            RunTest("hello", [], 0, null, true, "", "", "36. 查找字符串数组为 []");

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
        /// <param name="findStrs">需要查找的值, 匹配其中任一项</param>
        /// <param name="startIndex">在 <paramref name="input"/> 中的查找起点</param>
        /// <param name="maxReadLength">非 null 时, 限制从 <paramref name="startIndex"/> 开始允许读取多少字符</param>
        /// <param name="expectedResult">预期返回值</param>
        /// <param name="expectedOutput">预期 output 值</param>
        /// <param name="description">测试的描述信息</param>
        private void RunTest(string input, string?[] findStrs, int startIndex, int? maxReadLength,
                              bool expectedResult, string? expectedOutput, string? expectedFoundStr, string description)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{description}");
            Logger.Info($"输入: \"{input}\", 查找: \"{(findStrs == null ? "<null>" : string.Join(',', findStrs.Select(i => i ?? "<null>")))}\", 起始: {startIndex}, 最大长度: {maxReadLength?.ToString() ?? "无限制"}");

            try
            {
                bool result = StringAnalysis.TryReadUntilOrEnd(input, findStrs!, startIndex, maxReadLength, out string? foundStr, out string? output);

                if (result == expectedResult)
                {
                    if (result)
                    {
                        if (output == expectedOutput)
                        {
                            if (expectedFoundStr == foundStr)
                            {
                                Logger.Info($"√ 成功: 返回True, 输出\"{output}\", 查找到字符串\"{foundStr ?? "<null>"}\"");
                            }
                            else
                            {
                                Logger.Warning($"X !失败!: 查找到的字符串不匹配 (预期\"{expectedFoundStr ?? "<null>"}\", 实际\"{foundStr ?? "<null>"}\")");
                                failures.Add(description);
                            }
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
