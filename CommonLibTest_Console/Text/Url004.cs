using Common_Util.Extensions;
using Common_Util.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Url004() : TestBase("测试 UrlHelper 解析 http 字符串方法")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // 1. 基本HTTP URL解析测试
            RunTest(
                "http://example.com",
                true,
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "1. 基本HTTP URL解析测试"
            );

            // 2. 基本HTTPS URL解析测试
            RunTest(
                "https://example.com",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "2. 基本HTTPS URL解析测试"
            );

            // 3. 带端口号URL解析测试
            RunTest(
                "http://example.com:8080",
                true,
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "example.com",
                    Port = 8080,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "3. 带端口号URL解析测试"
            );

            // 4. 带路径URL解析测试
            RunTest(
                "https://example.com/api/v1/users",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "api", "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "4. 带路径URL解析测试"
            );

            // 5. 带查询参数URL解析测试
            RunTest(
                "https://example.com/search?q=test&sort=date",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "search" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("q", "test"),
                        new("sort", "date")
                    }
                },
                "5. 带查询参数URL解析测试"
            );

            // 6. 带空路径段URL解析测试
            RunTest(
                "https://example.com/api//v1",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "api", "", "v1" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "6. 带空路径段URL解析测试"
            );

            // 7. 带根路径URL解析测试
            RunTest(
                "https://example.com/",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "7. 带根路径URL解析测试"
            );

            // 8. 带转义字符URL解析测试
            RunTest(
                "https://example.com/path%20with%20spaces?key=value%26with%3Dspecial",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "path with spaces" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("key", "value&with=special")
                    }
                },
                "8. 带转义字符URL解析测试"
            );

            // 9. 带IPv4地址URL解析测试
            RunTest(
                "http://192.168.1.1:8080/api",
                true,
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "192.168.1.1",
                    Port = 8080,
                    Path = new[] { "api" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "9. 带IPv4地址URL解析测试"
            );

            // 10. 带IPv6地址URL解析测试
            RunTest(
                "http://[2001:db8::1]:8080/api",
                true,
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "2001:db8::1",
                    Port = 8080,
                    Path = new[] { "api" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "10. 带IPv6地址URL解析测试"
            );

            // 11. 带重复查询参数URL解析测试
            RunTest(
                "https://example.com/search?q=test&q=another",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "search" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("q", "test"),
                        new("q", "another")
                    }
                },
                "11. 带重复查询参数URL解析测试"
            );

            // 12. 带空值查询参数URL解析测试
            RunTest(
                "https://example.com/search?q=&empty",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "search" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("q", ""),
                        new("empty", "")
                    }
                },
                "12. 带空值查询参数URL解析测试"
            );

            // 13. 复杂URL解析测试
            RunTest(
                "https://example.com:8443/api/v1/users?sort=name&order=asc&page=1#section",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = 8443,
                    Path = new[] { "api", "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("sort", "name"),
                        new("order", "asc"),
                        new("page", "1")
                    },
                    Fragment = "section"
                },
                "13. 复杂URL解析测试（忽略用户信息和片段）"
            );

            // 14. 无效URL解析测试
            RunTest(
                "not_a_valid_url",
                false,
                default(HttpUrlInfo),
                "14. 无效URL解析测试"
            );

            // 15. 空字符串URL解析测试
            RunTest(
                "",
                false,
                default(HttpUrlInfo),
                "15. 空字符串URL解析测试"
            );

            // 16. null输入解析测试
            RunTest(
                null,
                false,
                default(HttpUrlInfo),
                "16. null输入解析测试"
            );

            // 17. 带特殊字符主机名URL解析测试
            RunTest(
                "http://example-test.com",
                true,
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "example-test.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "17. 带特殊字符主机名URL解析测试"
            );

            // 18. 带中文路径URL解析测试
            RunTest(
                "https://example.com/中文路径",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "中文路径" },
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "18. 带中文路径URL解析测试"
            );

            // 19. 带中文查询参数URL解析测试
            RunTest(
                "https://example.com/search?关键词=测试",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "search" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
            new("关键词", "测试")
                    }
                },
                "19. 带中文查询参数URL解析测试"
            );

            // 20. 混合大小写协议URL解析测试
            RunTest(
                "HTTP://example.com",
                true,
                new HttpUrlInfo
                {
                    Protocol = "HTTP",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>()
                },
                "20. 混合大小写协议URL解析测试"
            );

            // 21. 带片段的基本URL解析测试
            RunTest(
                "https://example.com#section1",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "section1" // 原始值，未解码
                },
                "21. 带片段的基本URL解析测试"
            );

            // 22. 带片段和路径的URL解析测试
            RunTest(
                "https://example.com/api#section2",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "api" },
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "section2" // 原始值，未解码
                },
                "22. 带片段和路径的URL解析测试"
            );

            // 23. 带片段和查询参数的URL解析测试
            RunTest(
                "https://example.com?page=1#section3",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("page", "1")
                    },
                    Fragment = "section3" // 原始值，未解码
                },
                "23. 带片段和查询参数的URL解析测试"
            );

            // 24. 带片段、路径和查询参数的完整URL解析测试
            RunTest(
                "https://example.com/api/v1?sort=name#results",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "api", "v1" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("sort", "name")
                    },
                    Fragment = "results" // 原始值，未解码
                },
                "24. 带片段、路径和查询参数的完整URL解析测试"
            );

            // 25. 带特殊字符片段的URL解析测试
            RunTest(
                "https://example.com#section-1_2%20test",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "section-1_2%20test" // 原始值，未解码（%20 保持原样）
                },
                "25. 带特殊字符片段的URL解析测试"
            );

            // 26. 带中文片段的URL解析测试
            RunTest(
                "https://example.com#中文片段",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "中文片段" // 原始值，未解码
                },
                "26. 带中文片段的URL解析测试"
            );

            // 27. 带编码字符片段的URL解析测试
            RunTest(
                "https://example.com#section%20with%20spaces",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "section%20with%20spaces" // 原始值，未解码（%20 保持原样）
                },
                "27. 带编码字符片段的URL解析测试"
            );

            // 28. 空片段URL解析测试
            RunTest(
                "https://example.com#",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = "" // 空字符串片段
                },
                "28. 空片段URL解析测试"
            );

            // 29. 不带片段的URL解析测试（确保Fragment为null）
            RunTest(
                "https://example.com",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new string[0],
                    QueryArgs = new List<KeyValuePair<string, string>>(),
                    Fragment = null // 没有片段
                },
                "29. 不带片段的URL解析测试（确保Fragment为null）"
            );

            // 30. 复杂URL解析测试（包含所有部分）
            RunTest(
                "https://example.com:8443/api/v1/users?sort=name&order=asc&page=1#section",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = 8443,
                    Path = new[] { "api", "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("sort", "name"),
                        new("order", "asc"),
                        new("page", "1")
                    },
                    Fragment = "section" // 原始值，未解码
                },
                "30. 复杂URL解析测试（包含所有部分）"
            );

            // 31. 带特殊字符的复杂片段解析测试
            RunTest(
                "https://example.com/search?q=test#results%20page%201",
                true,
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = null,
                    Path = new[] { "search" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("q", "test")
                    },
                    Fragment = "results%20page%201" // 原始值，未解码
                },
                "31. 带特殊字符的复杂片段解析测试"
            );

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
        private void RunTest(string? 输入值, bool 预期结果, HttpUrlInfo 预期输出, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入: {(输入值 == null ? "<null>" : $"\"{输入值}\"")}, 预期结果: {预期结果}, 预期输出: {结果字符串(预期输出)}");

            try
            {
                bool result = UrlHelper.TryParseHttp(输入值, out var output);


                if (result == 预期结果)
                {
                    if (result)
                    {
                        if (output == 预期输出)
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
        private string 结果字符串(HttpUrlInfo 结果)
        {
            if (结果.IsEmpty) return "<Empty>";
            else return 结果.ToString();
        }
    }
}
