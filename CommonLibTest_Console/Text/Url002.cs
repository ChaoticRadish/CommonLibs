using Common_Util.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Url002() : TestBase("测试 UrlHelper 切分参数字符串方法")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // 基础查询参数测试
            RunTest("arg0=1&arg1=test&arg2=yes", true,
                new[] {
                    new KeyValuePair<string, string>("arg0", "1"),
                    new KeyValuePair<string, string>("arg1", "test"),
                    new KeyValuePair<string, string>("arg2", "yes")
                },
                "1. 基础查询参数");

            // URL 编码参数测试
            RunTest("name=John%20Doe&city=New%20York", true,
                new[] {
                    new KeyValuePair<string, string>("name", "John Doe"),
                    new KeyValuePair<string, string>("city", "New York")
                },
                "2. URL 编码参数");

            // 重复键测试
            RunTest("color=red&color=blue&color=green", true,
                new[] {
                    new KeyValuePair<string, string>("color", "red"),
                    new KeyValuePair<string, string>("color", "blue"),
                    new KeyValuePair<string, string>("color", "green")
                },
                "3. 重复键参数");

            // 空值和缺失值测试
            RunTest("key1=&key2&key3=value", true,
                new[] {
                    new KeyValuePair<string, string>("key1", ""),
                    new KeyValuePair<string, string>("key2", ""),
                    new KeyValuePair<string, string>("key3", "value")
                },
                "4. 空值和缺失值");

            // 特殊字符测试
            RunTest("search=hello%2Bworld%3F&price=100%24", true,
                new[] {
                    new KeyValuePair<string, string>("search", "hello+world?"),
                    new KeyValuePair<string, string>("price", "100$")
                },
                "5. 特殊字符编码");

            // 边界情况测试
            RunTest("", true, [], "6. 空字符串输入");
            RunTest(null, true, [], "7. null输入");

            // 复杂查询测试
            RunTest("q=search%20term&page=1&sort=name&order=asc&filter=category%3Abooks", true,
                new[] {
                    new KeyValuePair<string, string>("q", "search term"),
                    new KeyValuePair<string, string>("page", "1"),
                    new KeyValuePair<string, string>("sort", "name"),
                    new KeyValuePair<string, string>("order", "asc"),
                    new KeyValuePair<string, string>("filter", "category:books")
                },
                "8. 复杂查询参数");

            // 只有键没有值的测试
            RunTest("flag&debug&verbose", true,
                new[] {
                    new KeyValuePair<string, string>("flag", ""),
                    new KeyValuePair<string, string>("debug", ""),
                    new KeyValuePair<string, string>("verbose", "")
                },
                "9. 只有键没有值");

            // 混合情况测试
            RunTest("a=1&b&c=hello%20world&a=2&d=test", true,
                new[] {
                    new KeyValuePair<string, string>("a", "1"),
                    new KeyValuePair<string, string>("b", ""),
                    new KeyValuePair<string, string>("c", "hello world"),
                    new KeyValuePair<string, string>("a", "2"),
                    new KeyValuePair<string, string>("d", "test")
                },
                "10. 混合情况测试");


            // 包含多个等号的测试
            RunTest("key=value=extra", true,
                new[] {
                    new KeyValuePair<string, string>("key", "value=extra")
                },
                "11. 值中包含等号");

            RunTest("key==value", true,
                new[] {
                    new KeyValuePair<string, string>("key", "=value")
                },
                "12. 值以等号开头");

            RunTest("key=value==", true,
                new[] {
                    new KeyValuePair<string, string>("key", "value==")
                },
                "13. 值以多个等号结尾");

            RunTest("=value", true,
                new[] {
                    new KeyValuePair<string, string>("", "value")
                },
                "14. 空键名");

            RunTest("key1=val=ue1&key2=val=ue=2", true,
                new[] {
                    new KeyValuePair<string, string>("key1", "val=ue1"),
                    new KeyValuePair<string, string>("key2", "val=ue=2")
                },
                "15. 多个参数值包含等号");

            RunTest("a==b&c=d=e=f", true,
                new[] {
                    new KeyValuePair<string, string>("a", "=b"),
                    new KeyValuePair<string, string>("c", "d=e=f")
                },
                "16. 复杂等号组合");

            RunTest("&", true, 
                new[] {
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", "")
                }, "17. 只有分隔符");
            RunTest("&&&", true,
                new[] {
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", "")
                }, "18. 多个连续分隔符");
            RunTest("&test1=1&", true,
                new[] {
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("test1", "1"),
                    new KeyValuePair<string, string>("", ""),
                },
                "19. 开头和结尾的分隔符");

            RunTest("test1=1&&test2=b", true,
                new[] {
                    new KeyValuePair<string, string>("test1", "1"),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("test2", "b")
                },
                "20. 中间有空段");

            RunTest("=", true,
                new[] {
                    new KeyValuePair<string, string>("", "")
                },
                "21. 只有等号");

            RunTest("&test1=1&test2=b", true,
                new[] {
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("test1", "1"),
                    new KeyValuePair<string, string>("test2", "b")
                },
                "22. 开头有空段");
            RunTest("test1=1&test2=b&", true,
                new[] {
                    new KeyValuePair<string, string>("test1", "1"),
                    new KeyValuePair<string, string>("test2", "b"),
                    new KeyValuePair<string, string>("", ""),
                },
                "23. 结尾有空段");
            RunTest("&&test1=1&&test2=b&&", true,
                new[] {
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("test1", "1"),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("test2", "b"),
                    new KeyValuePair<string, string>("", ""),
                    new KeyValuePair<string, string>("", ""),
                },
                "24. 多个连续空段");

            RunTest(" = & = ", true,
                new[] {
                    new KeyValuePair<string, string>(" ", " "),
                    new KeyValuePair<string, string>(" ", " ")
                },
                "25. 只有空格键值");

            if (failures.Count != 0)
            {
                WriteList(failures, "失败列表", true, "");
            }
            else
            {
                WriteLine("均成功");
            }
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="输入值"></param>
        /// <param name="预期结果"></param>
        /// <param name="预期输出"></param>
        /// <param name="测试描述"></param>
        private void RunTest(string? 输入值, bool 预期结果, KeyValuePair<string, string>[]? 预期输出, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入: {(输入值 == null ? "<null>" : $"\"{输入值}\"")}, 预期结果: {预期结果}, 预期输出: {结果字符串(预期输出)}");

            try
            {
                bool result = UrlHelper.SplitQuery(输入值, out var output);


                if (result == 预期结果)
                {
                    if (result)
                    {
                        if (UrlHelper.QueryEquals(output, 预期输出))
                        {
                            Logger.Info($"√ 成功: 返回True, 输出: {结果字符串(output)}");
                        }
                        else
                        {
                            Logger.Warning($"X !失败!: 输出不匹配 (实际: {结果字符串(output)})");
                            failures.Add(测试描述);
                        }
                    }
                    else
                    {
                        Logger.Info($"√ 成功: 返回False, 输出null");
                    }
                }
                else
                {
                    Logger.Warning($"X !失败!: 返回值不匹配 (预期{预期结果}, 实际{result})");
                    failures.Add(测试描述);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"! 异常: {ex.Message}");
            }

            Logger.Info("----------------------------------------");
        }
        private string 结果字符串(IEnumerable<KeyValuePair<string, string>>? 结果)
        {
            return (结果 == null ? "<null>" : $"[{string.Join(", ", 结果.Select(kvp => $"{kvp.Key}={kvp.Value}"))}]");
        }
    }
}
