using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Regex001() : TestBase("测试匹配标识符与参数的正则表达式")
    {
        /* 在 AI 生成示例的基础上调整测试
         * 1. C# 变量名/方法名, 允许中文, 不需要考虑是否与关键字冲突, 不需要考虑 @ 开头的情况
         * 2. 方法调用, 在 1 的基础上加上 (arg1, arg2 .... argn) 的参数, 只支持数字, 比如 123, 1.23 等, 简单字符串, 比如 "hello", 字符
         */

        List<string> testStrings = new List<string>
        {
            "",
            "()",
            "myVariable",
            "中文方法",
            "MyMethod()",
            "Calculate(123)",
            "Print(\"hello world\")",
            "Add(1, 2.5)",
            "Log(\"start\", 1, 'c')",
        };

        protected override void RunImpl()
        {
            string pattern = @"(?<identifier>[\p{L}][\p{L}0-9]*)(?<args>\((\s*(?:\d+(?:\.\d+)?|""[^""]*""|'[^']*')\s*(?:,\s*(?:\d+(?:\.\d+)?|""[^""]*""|'[^']*')\s*)*)?\))?";

            Regex regex = new Regex(pattern);
            foreach (string str in testStrings)
            {
                Match match = regex.Match(str);
                if (match.Success)
                {
                    // 核心：检查名为 'args' 的捕获组是否匹配成功
                    if (match.Groups["args"].Success)
                    {
                        WriteLine($"[方法调用] '{str}' -> 标识符: {match.Groups["identifier"].Value} 参数: {match.Groups["args"].Value}");
                    }
                    else
                    {
                        WriteLine($"[变量名/方法名] '{str}' -> 标识符: {match.Groups["identifier"].Value}");
                    }
                }
                else
                {
                    WriteLine($"[不匹配] '{str}'");
                }
                WriteLine();
            }


        }
    }
}
