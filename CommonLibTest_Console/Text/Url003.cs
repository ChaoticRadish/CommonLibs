using Common_Util.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Url003() : TestBase("测试 HttpUrlInfo 结构体的相等比较")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // 1. 常见情况比较，预期相等
            RunTestEqual(
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = 443,
                    Path = new[] { "api", "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("sort", "name"),
                        new("order", "asc")
                    }
                },
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "example.com",
                    Port = 443,
                    Path = new[] { "api", "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("sort", "name"),
                        new("order", "asc")
                    }
                },
                true,
                "1. 常见情况比较，预期相等"
            );

            // 2. 协议不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Protocol = "https", Host = "example.com" },
                new HttpUrlInfo { Protocol = "http", Host = "example.com" },
                false,
                "2. 协议不同，预期不相等"
            );

            // 3. 端口显式与隐式默认值（https端口443），视为不相等
            RunTestEqual(
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = 443 },
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = null },
                false,
                "3. 端口显式与隐式默认值（https端口443），视为不相等"
            );

            // 4. 路径分段顺序不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", Path = new[] { "a", "b" } },
                new HttpUrlInfo { Host = "example.com", Path = new[] { "b", "a" } },
                false,
                "4. 路径分段顺序不同，预期不相等"
            );

            // 5. 查询参数顺序不同但内容相同，预期不相等（因需保留顺序）
            RunTestEqual(
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("a", "1"),
                        new("b", "2")
                    }
                },
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("b", "2"),
                        new("a", "1")
                    }
                },
                false,
                "5. 查询参数顺序不同但内容相同，预期不相等（因需保留顺序）"
            );

            // 6. 查询参数重复且顺序相同，预期相等
            RunTestEqual(
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("a", "1"),
                        new("a", "2")
                    }
                },
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("a", "1"),
                        new("a", "2")
                    }
                },
                true,
                "6. 查询参数重复且顺序相同，预期相等"
            );

            // 7. 空路径与根路径比较，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", Path = new string[0] },
                new HttpUrlInfo { Host = "example.com", Path = new[] { "" } },
                false,
                "7. 空路径与根路径比较，预期不相等"
            );

            // 8. 双斜杠路径比较，预期相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", Path = new[] { "help", "", "get" } },
                new HttpUrlInfo { Host = "example.com", Path = new[] { "help", "", "get" } },
                true,
                "8. 双斜杠路径比较，预期相等"
            );

            // 9. 缺省端口与显式端口比较，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = null },
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = 443 },
                false,
                "9. 缺省端口与显式端口比较，预期不相等"
            );

            // 10. 协议大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Protocol = "HTTP", Host = "example.com" },
                new HttpUrlInfo { Protocol = "http", Host = "example.com" },
                false,
                "10. 协议大小写不同，预期不相等"
            );

            // 11. 主机名大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "Example.com" },
                new HttpUrlInfo { Host = "example.com" },
                false,
                "11. 主机名大小写不同，预期不相等"
            );

            // 12. 路径分段大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", Path = new[] { "API", "users" } },
                new HttpUrlInfo { Host = "example.com", Path = new[] { "api", "users" } },
                false,
                "12. 路径分段大小写不同，预期不相等"
            );

            // 13. 查询参数键大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("SORT", "name") }
                },
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("sort", "name") }
                },
                false,
                "13. 查询参数键大小写不同，预期不相等"
            );

            // 14. 查询参数值大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("sort", "NAME") }
                },
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("sort", "name") }
                },
                false,
                "14. 查询参数值大小写不同，预期不相等"
            );

            // 15. 完全相同的复杂URL，预期相等
            RunTestEqual(
                new HttpUrlInfo
                {
                    Protocol = "HTTPS",
                    Host = "API.Example.com",
                    Port = 8443,
                    Path = new[] { "V1", "Users", "Profile" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("ID", "123"),
                        new("Format", "JSON")
                    }
                },
                new HttpUrlInfo
                {
                    Protocol = "HTTPS",
                    Host = "API.Example.com",
                    Port = 8443,
                    Path = new[] { "V1", "Users", "Profile" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("ID", "123"),
                        new("Format", "JSON")
                    }
                },
                true,
                "15. 完全相同的复杂URL，预期相等"
            );

            // 16. 相同端口号但协议不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 80 },
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = 80 },
                false,
                "16. 相同端口号但协议不同，预期不相等"
            );

            // 17. 空查询参数列表与null查询参数比较，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", QueryArgs = new List<KeyValuePair<string, string>>() },
                new HttpUrlInfo { Host = "example.com", QueryArgs = null! },
                false,
                "17. 空查询参数列表与null查询参数比较，预期不相等"
            );

            // 18. 空路径数组与null路径比较，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Host = "example.com", Path = new string[0] },
                new HttpUrlInfo { Host = "example.com", Path = null! },
                false,
                "18. 空路径数组与null路径比较，预期不相等"
            );

            // 19. 协议大小写不同但等价，预期相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "HTTP", Host = "example.com", Port = 80 },
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 80 },
                true,
                "19. 协议大小写不同但等价，预期相等"
            );

            // 20. 主机名大小写不同但等价，预期相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "http", Host = "Example.com", Port = 80 },
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 80 },
                true,
                "20. 主机名大小写不同但等价，预期相等"
            );

            // 21. 缺省端口与显式默认端口等价，预期相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = null },
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 80 },
                true,
                "21. 缺省端口与显式默认端口等价，预期相等"
            );

            // 22. HTTPS缺省端口与显式默认端口等价，预期相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = null },
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = 443 },
                true,
                "22. HTTPS缺省端口与显式默认端口等价，预期相等"
            );

            // 23. 非标准端口与缺省端口不等价，预期不相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 8080 },
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = null },
                false,
                "23. 非标准端口与缺省端口不等价，预期不相等"
            );

            // 24. 路径大小写不同，预期不相等（路径不参与等价比较）
            RunTestEquivalent(
                new HttpUrlInfo { Host = "example.com", Path = new[] { "API", "users" } },
                new HttpUrlInfo { Host = "example.com", Path = new[] { "api", "users" } },
                false,
                "24. 路径大小写不同，预期不相等（路径不参与等价比较）"
            );

            // 25. 查询参数大小写不同，预期不相等（查询参数不参与等价比较）
            RunTestEquivalent(
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("SORT", "name") }
                },
                new HttpUrlInfo
                {
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("sort", "name") }
                },
                false,
                "25. 查询参数大小写不同，预期不相等（查询参数不参与等价比较）"
            );

            // 26. 复杂URL等价比较，预期相等
            RunTestEquivalent(
                new HttpUrlInfo
                {
                    Protocol = "HTTPS",
                    Host = "API.EXAMPLE.COM",
                    Port = null, // 缺省端口443
                    Path = new[] { "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("id", "123"),
                        new("format", "json")
                    }
                },
                new HttpUrlInfo
                {
                    Protocol = "https",
                    Host = "api.example.com",
                    Port = 443, // 显式默认端口
                    Path = new[] { "v1", "users" },
                    QueryArgs = new List<KeyValuePair<string, string>>
                    {
                        new("id", "123"),
                        new("format", "json")
                    }
                },
                true,
                "26. 复杂URL等价比较，预期相等"
            );

            // 27. 协议不同但端口相同，预期不相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Port = 443 },
                new HttpUrlInfo { Protocol = "https", Host = "example.com", Port = 443 },
                false,
                "27. 协议不同但端口相同，预期不相等"
            );

            // 28. 协议和主机相同但路径不同，预期不相等
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Path = new[] { "api" } },
                new HttpUrlInfo { Protocol = "http", Host = "example.com", Path = new[] { "web" } },
                false,
                "28. 协议和主机相同但路径不同，预期不相等"
            );

            // 29. 协议和主机相同但查询参数不同，预期不相等
            RunTestEquivalent(
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("a", "1") }
                },
                new HttpUrlInfo
                {
                    Protocol = "http",
                    Host = "example.com",
                    QueryArgs = new List<KeyValuePair<string, string>> { new("b", "2") }
                },
                false,
                "29. 协议和主机相同但查询参数不同，预期不相等"
            );

            // 30. 未知协议缺省端口处理，预期不相等（无法确定缺省端口）
            RunTestEquivalent(
                new HttpUrlInfo { Protocol = "ftp", Host = "example.com", Port = null },
                new HttpUrlInfo { Protocol = "ftp", Host = "example.com", Port = 21 },
                false,
                "30. 未知协议缺省端口处理，预期不相等（无法确定缺省端口）"
            );

            // 31. Fragment不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "section1" },
                new HttpUrlInfo { Fragment = "section2" },
                false,
                "31. Fragment不同，预期不相等"
            );

            // 32. Fragment大小写不同，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "Section" },
                new HttpUrlInfo { Fragment = "section" },
                false,
                "32. Fragment大小写不同，预期不相等"
            );

            // 33. 一个有Fragment一个没有，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "section" },
                new HttpUrlInfo { Fragment = null },
                false,
                "33. 一个有Fragment一个没有，预期不相等"
            );

            // 34. 两个都没有Fragment，预期相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = null },
                new HttpUrlInfo { Fragment = null },
                true,
                "34. 两个都没有Fragment，预期相等"
            );

            // 35. 空字符串Fragment与null Fragment，预期不相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "" },
                new HttpUrlInfo { Fragment = null },
                false,
                "35. 空字符串Fragment与null Fragment，预期不相等"
            );

            // 36. Fragment相同，预期等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = "section" },
                new HttpUrlInfo { Fragment = "section" },
                true,
                "36. Fragment相同，预期等价"
            );

            // 37. Fragment不同，预期不等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = "section1" },
                new HttpUrlInfo { Fragment = "section2" },
                false,
                "37. Fragment不同，预期不等价"
            );

            // 38. Fragment大小写不同，预期不等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = "Section" },
                new HttpUrlInfo { Fragment = "section" },
                false,
                "38. Fragment大小写不同，预期不等价"
            );

            // 39. 一个有Fragment一个没有，预期不等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = "section" },
                new HttpUrlInfo { Fragment = null },
                false,
                "39. 一个有Fragment一个没有，预期不等价"
            );

            // 40. 两个都没有Fragment，预期等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = null },
                new HttpUrlInfo { Fragment = null },
                true,
                "40. 两个都没有Fragment，预期等价"
            );

            // 41. 空字符串Fragment与null Fragment，预期不等价
            RunTestEquivalent(
                new HttpUrlInfo { Fragment = "" },
                new HttpUrlInfo { Fragment = null },
                false,
                "41. 空字符串Fragment与null Fragment，预期不等价"
            );

            // 42. 包含特殊字符的Fragment比较，预期相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "section-1_2%20test" },
                new HttpUrlInfo { Fragment = "section-1_2%20test" },
                true,
                "42. 包含特殊字符的Fragment比较，预期相等"
            );

            // 43. 包含中文的Fragment比较，预期相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "中文片段" },
                new HttpUrlInfo { Fragment = "中文片段" },
                true,
                "43. 包含中文的Fragment比较，预期相等"
            );

            // 44. 包含编码字符的Fragment比较，预期相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "section%20with%20spaces" },
                new HttpUrlInfo { Fragment = "section%20with%20spaces" },
                true,
                "44. 包含编码字符的Fragment比较，预期相等"
            );

            // 45. Fragment相同，预期相等
            RunTestEqual(
                new HttpUrlInfo { Fragment = "section1" },
                new HttpUrlInfo { Fragment = "section1" },
                true,
                "45. Fragment相同，预期相等"
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
        /// 执行测试, 是否相等
        /// </summary>
        /// <param name="输入1"></param>
        /// <param name="输入2"></param>
        /// <param name="预期结果"></param>
        /// <param name="测试描述"></param>
        private void RunTestEqual(HttpUrlInfo 输入1, HttpUrlInfo 输入2, bool 预期结果, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入1: {输入1}\n输入2: {输入2}\n 预期结果: {预期结果}");

            try
            {
                bool 结果 = 输入1 == 输入2;
                if (结果 == 预期结果)
                {
                    Logger.Info($"√ 成功");
                }
                else
                {
                    Logger.Warning($"X !失败!: 返回值不匹配 (预期{预期结果}, 实际{结果})");
                    failures.Add(测试描述);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"! 异常: {ex.Message}");
            }

            Logger.Info("----------------------------------------");
        }


        /// <summary>
        /// 执行测试, 是否等价
        /// </summary>
        /// <remarks>
        /// 等价: 端口为 null 时, 取缺省值. <see cref="HttpUrlInfo.Protocol"/> 和 <see cref="HttpUrlInfo.Host"/> 允许大小写不同
        /// </remarks>
        /// <param name="输入1"></param>
        /// <param name="输入2"></param>
        /// <param name="预期结果"></param>
        /// <param name="测试描述"></param>
        private void RunTestEquivalent(HttpUrlInfo 输入1, HttpUrlInfo 输入2, bool 预期结果, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入1: {输入1}\n输入2: {输入2}\n 预期结果: {预期结果}");

            try
            {
                bool 结果 = 输入1.IsEquivalent(输入2);
                if (结果 == 预期结果)
                {
                    Logger.Info($"√ 成功");
                }
                else
                {
                    Logger.Warning($"X !失败!: 返回值不匹配 (预期{预期结果}, 实际{结果})");
                    failures.Add(测试描述);
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
