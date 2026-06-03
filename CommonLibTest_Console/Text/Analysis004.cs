using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Analysis004() : TestBase("测试略过 C# 字符串值作操作")
    {
        protected override void RunImpl()
        {
            string testString1 = "mytest=\"plan=123\"";
            string testString2 = "key1=value1,key2=\"value,with,commas\",key3=value3";
            string testString3 = "path=\"C:\\Program Files\\App\\config.ini\",arg1=\"name=\\\"My App\\\"\",test=\"Hello, World! \"";
            string testString4 = "111,222,333,   , , 4 4 ,";

            WriteLine("--- Testing SplitIgnoreStringValue ---");
            WriteLine($"Original: '{testString1}'");
            foreach (var part in testString1.SplitIgnoreStringValue('='))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: 'mytest'
            // - Part: '"plan=123"'

            WriteLine("\n" + new string('-', 40));
            WriteLine($"Original: '{testString2}'");
            foreach (var part in testString2.SplitIgnoreStringValue(','))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: 'key1=value1'
            // - Part: 'key2="value,with,commas"'
            // - Part: 'key3=value3'

            WriteLine("\n--- Testing IndexOfIgnoreStringValue ---");
            int firstCommaIndex = testString2.IndexOfIgnoreStringValue(',');
            WriteLine($"First comma in '{testString2}' is at index: {firstCommaIndex}"); // 预期: 11

            int firstEqualsInQuotes = testString1.IndexOfIgnoreStringValue('=');
            WriteLine($"First equals in '{testString1}' is at index: {firstEqualsInQuotes}"); // 预期: 6

            WriteLine("\n--- Testing LastIndexOfIgnoreStringValue ---");
            int lastCommaIndex = testString2.LastIndexOfIgnoreStringValue(',');
            WriteLine($"Last comma in '{testString2}' is at index: {lastCommaIndex}"); // 预期: 36

            WriteLine("\n--- Testing with Escapes ---");
            WriteLine($"Original: '{testString3}'");
            foreach (var part in testString3.SplitIgnoreStringValue(','))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: 'path="C:\Program Files\App\config.ini"'
            // - Part: 'arg1="name=\"My App\""'
            // - Part: 'test="Hello, World! "'

            WriteLine("\n--- Testing with SplitOptions ---");
            WriteLine("StringSplitOptions.None");
            WriteLine($"Original: '{testString4}'");
            foreach (var part in testString4.SplitIgnoreStringValue(',', StringSplitOptions.None))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: '111'
            // - Part: '222'
            // - Part: '333'
            // - Part: '   '
            // - Part: ' '
            // - Part: ' 4 4 '
            // - Part: ''

            WriteLine("\nStringSplitOptions.RemoveEmptyEntries");
            WriteLine($"Original: '{testString4}'");
            foreach (var part in testString4.SplitIgnoreStringValue(',', StringSplitOptions.RemoveEmptyEntries))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: '111'
            // - Part: '222'
            // - Part: '333'
            // - Part: '   '
            // - Part: ' '
            // - Part: ' 4 4 '

            WriteLine("\nStringSplitOptions.TrimEntries");
            WriteLine($"Original: '{testString4}'");
            foreach (var part in testString4.SplitIgnoreStringValue(',', StringSplitOptions.TrimEntries))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: '111'
            // - Part: '222'
            // - Part: '333'
            // - Part: ''
            // - Part: ''
            // - Part: '4 4'
            // - Part: ''

            WriteLine("\nStringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries");
            WriteLine($"Original: '{testString4}'");
            foreach (var part in testString4.SplitIgnoreStringValue(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                WriteLine($"- Part: '{part}'");
            }
            // 预期输出:
            // - Part: '111'
            // - Part: '222'
            // - Part: '333'
            // - Part: '4 4'
        }
    }
}
