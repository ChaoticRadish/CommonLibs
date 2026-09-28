using ChaoticKit.Attributes.General;
using ChaoticKit.Log;

namespace ChaoticKit.LibTest.Unit.Log
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Log.EnumLog001
    /// LevelLoggerHelper.EnumLog 依据枚举成员上的 <see cref="LoggerAttribute"/> 将 Level/Category 映射到 LogData 的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 捕获方式: 通过 EnumLogHelper&lt;LogEnum&gt;.SetDealFunc 注册回调拦截 LogData 并返回 null 以抑制全局输出,
    /// 不依赖控制台输出, 无随机与计时; 断言 Level/Category/SubCategory/Message/Exception 的映射关系。
    /// </remarks>
    [TestClass]
    public sealed class EnumLogTest : UnitTestBase
    {
        /// <summary>
        /// 与源测试一致的日志枚举, 各成员通过 <see cref="LoggerAttribute"/> 声明 Category (SubCategory 缺省为空)
        /// </summary>
        public enum LogEnum
        {
            [Logger("AAA")]
            AAA,
            [Logger("BBB")]
            BBB,
            [Logger("CCC")]
            CCC,
        }

        [DataTestMethod]
        [DataRow(LogEnum.AAA, "AAA")]
        [DataRow(LogEnum.BBB, "BBB")]
        [DataRow(LogEnum.CCC, "CCC")]
        public void EnumLog_LevelCategory映射(LogEnum e, string expectedCategory)
        {
            Log($"--- 测试: EnumLog({e}) 各级别 Level/Category 映射 ---");

            // 注册回调捕获 LogData, 返回 null 表示拦截, 不进入全局日志输出
            List<LogData> captured = new();
            EnumLogHelper<LogEnum>.SetDealFunc(e, data =>
            {
                captured.Add(data);
                return null;
            });

            ILevelLogger logger = LevelLoggerHelper.EnumLog(e);

            logger.Info("测试 Info");
            logger.Debug("测试 Debug");
            logger.Warning("测试 Warning", null, true);
            Exception ex = new("触发异常");
            logger.Error("测试 Error", ex);

            // 每个级别应各产出 1 条 LogData
            Assert.AreEqual(4, captured.Count, $"EnumLog({e}) 应捕获 4 条日志, 实际 {captured.Count} 条");

            // Info: Level=Info, Category 来自 Logger 属性, SubCategory 缺省为空
            Assert.AreEqual("Info", captured[0].Level, "Info 级别应映射为 Level=Info");
            Assert.AreEqual(expectedCategory, captured[0].Category, "Info 的 Category 应来自 Logger 属性");
            Assert.AreEqual(string.Empty, captured[0].SubCategory, "未指定子分类时 SubCategory 应为空");
            Assert.AreEqual("测试 Info", captured[0].Message, "Info 消息文本应原样保留");
            Assert.IsNull(captured[0].Exception, "Info 不应携带异常");

            // Debug
            Assert.AreEqual("Debug", captured[1].Level, "Debug 级别应映射为 Level=Debug");
            Assert.AreEqual(expectedCategory, captured[1].Category, "Debug 的 Category 应来自 Logger 属性");
            Assert.AreEqual(string.Empty, captured[1].SubCategory, "未指定子分类时 SubCategory 应为空");
            Assert.AreEqual("测试 Debug", captured[1].Message, "Debug 消息文本应原样保留");
            Assert.IsNull(captured[1].Exception, "Debug 不应携带异常");

            // Warning: 源测试以 logTrack=true 调用, 应捕获堆栈帧
            Assert.AreEqual("Warning", captured[2].Level, "Warning 级别应映射为 Level=Warning");
            Assert.AreEqual(expectedCategory, captured[2].Category, "Warning 的 Category 应来自 Logger 属性");
            Assert.AreEqual("测试 Warning", captured[2].Message, "Warning 消息文本应原样保留");
            Assert.IsNull(captured[2].Exception, "Warning 传入 null 异常, Exception 应为空");
            Assert.IsNotNull(captured[2].StackFrames, "Warning 以 logTrack=true 调用应携带堆栈帧");

            // Error: 应携带传入的异常实例
            Assert.AreEqual("Error", captured[3].Level, "Error 级别应映射为 Level=Error");
            Assert.AreEqual(expectedCategory, captured[3].Category, "Error 的 Category 应来自 Logger 属性");
            Assert.AreEqual("测试 Error", captured[3].Message, "Error 消息文本应原样保留");
            Assert.AreSame(ex, captured[3].Exception, "Error 应携带传入的异常实例");

            Log($"EnumLog({e}) 捕获 {captured.Count} 条: Info→{captured[0].Category}, Debug→{captured[1].Category}, Warning→{captured[2].Category}, Error→{captured[3].Category}, 映射正确, 通过");
        }
    }
}
