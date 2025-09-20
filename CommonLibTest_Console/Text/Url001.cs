using BenchmarkDotNet.Running;
using Common_Util.String;
using Common_Util.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CommonLibTest_Console.Text
{
    internal class Url001() : TestBase("测试 UrlHelper 切分访问路径字符串方法")
    {
        List<string> failures = new List<string>();
        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;

            // 基础路径测试
            RunTest("/", true, new[] { "" }, "1. 根路径");
            RunTest("/user", true, new[] { "user" }, "2. 单级路径");
            RunTest("/user/my", true, new[] { "user", "my" }, "3. 两级路径");
            RunTest("/user/my/test", true, new[] { "user", "my", "test" }, "4. 多级路径");

            // 特殊字符和转义测试
            RunTest("/user%2Fdir", true, new[] { "user/dir" }, "5. 包含转义斜杠");
            RunTest("/user%20name", true, new[] { "user name" }, "6. 包含转义空格");
            RunTest("/path%2Bwith%2Bplus", true, new[] { "path+with+plus" }, "7. 包含转义加号");

            // 边界情况测试
            RunTest("", true, [], "8. 空字符串输入");
            RunTest(null, true, [], "9. null输入");
            RunTest("invalid", false, null, "10. 无斜杠开头");

            // 异常格式测试
            RunTest("//", true, new[] { "", "" }, "11. 双斜杠");
            RunTest("/user//test", true, new[] { "user", "", "test" }, "12. 中间空段");
            RunTest("/user/", true, new[] { "user", "" }, "13. 以斜杠结尾");


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
        private void RunTest(string? 输入值, bool 预期结果, string[]? 预期输出, string 测试描述)
        {
            var Logger = GetLevelLogger("测试");
            Logger.Info($"{测试描述}");
            Logger.Info($"输入: {(输入值 == null ? "<null>" : $"\"{输入值}\"")}, 预期结果: {预期结果}, 预期输出: {结果字符串(预期输出)}");

            try
            {
                bool result = UrlHelper.SplitPath(输入值, out string[]? output);
                

                if (result == 预期结果)
                {
                    if (result)
                    {
                        if (UrlHelper.PathEquals(output, 预期输出))
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
        private string 结果字符串(string[]? 结果)
        {
            return (结果 == null ? "<null>" : $"[{string.Join(", ", 结果.Select(str => str == null ? "<null>" : $"\"{str}\""))}]");
        }
    }
}
