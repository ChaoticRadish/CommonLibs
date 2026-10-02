using ChaoticKit.Log;

namespace ChaoticKit.LibTest.Unit.Log
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Log.LogTo001
    /// LevelLoggerHelper.LogTo 包装自定义 ILogger 的验证: 级别/分类/子分类/消息/异常传递
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// Time 为真实时间 (DateTime.Now), 断言已排除; StackFrames 为调用栈信息, 仅做宽松断言 (Error 非空, Info/Debug 为 null)。
    /// </remarks>
    [TestClass]
    public sealed class LogToTest : UnitTestBase
    {
        /// <summary>收集 LogData 的自定义 ILogger, 用于断言传入内容</summary>
        private sealed class TestLogger : ILogger
        {
            public List<LogData> Logs { get; } = new();

            public void Log(LogData log)
            {
                Logs.Add(log);
            }
        }

        [TestMethod]
        public void LogTo_默认配置_分类子分类为空()
        {
            Log("--- 测试: LogTo(logger) 默认配置, 分类/子分类应为空 ---");
            TestLogger logger = new();
            ILevelLogger levelLogger = LevelLoggerHelper.LogTo(logger);

            RunScenario(levelLogger, logger, string.Empty, string.Empty);
        }

        [TestMethod]
        public void LogTo_指定分类与子分类()
        {
            Log("--- 测试: LogTo(logger, \"测试2\", \"子分类1\") ---");
            TestLogger logger = new();
            ILevelLogger levelLogger = LevelLoggerHelper.LogTo(logger, "测试2", "子分类1");

            RunScenario(levelLogger, logger, "测试2", "子分类1");
        }

        /// <summary>
        /// 复现源测试场景: Info -> Debug -> 抛异常后 Error,
        /// 逐条断言传入 TestLogger 的 LogData (Time 真实时间不参与断言, StackFrames 宽松断言)
        /// </summary>
        private void RunScenario(ILevelLogger levelLogger, TestLogger logger, string expectCategory, string expectSubCategory)
        {
            levelLogger.Info("测试");
            try
            {
                levelLogger.Debug("测试 Debug");
                throw new Exception("123123123");
            }
            catch (Exception ex)
            {
                levelLogger.Error("发生异常", ex);
            }

            Assert.AreEqual(3, logger.Logs.Count, "应产生 Info/Debug/Error 共 3 条日志");

            LogData info = logger.Logs[0];
            Assert.AreEqual("Info", info.Level, "Info 级别");
            Assert.AreEqual(expectCategory, info.Category, "Info 分类");
            Assert.AreEqual(expectSubCategory, info.SubCategory, "Info 子分类");
            Assert.AreEqual("测试", info.Message, "Info 消息");
            Assert.IsNull(info.Exception, "Info 不应携带异常");
            Assert.IsNull(info.StackFrames, "Info 不追踪堆栈");
            Log($"Info: Level={info.Level}, Category=[{info.Category}], SubCategory=({info.SubCategory}), Message=\"{info.Message}\", Time={info.Time:yyyy-MM-dd HH:mm:ss.fff} (真实时间, 不参与断言), 通过");

            LogData debug = logger.Logs[1];
            Assert.AreEqual("Debug", debug.Level, "Debug 级别");
            Assert.AreEqual(expectCategory, debug.Category, "Debug 分类");
            Assert.AreEqual(expectSubCategory, debug.SubCategory, "Debug 子分类");
            Assert.AreEqual("测试 Debug", debug.Message, "Debug 消息");
            Assert.IsNull(debug.Exception, "Debug 不应携带异常");
            Assert.IsNull(debug.StackFrames, "Debug 不追踪堆栈");
            Log($"Debug: Level={debug.Level}, Message=\"{debug.Message}\", 通过");

            LogData error = logger.Logs[2];
            Assert.AreEqual("Error", error.Level, "Error 级别");
            Assert.AreEqual(expectCategory, error.Category, "Error 分类");
            Assert.AreEqual(expectSubCategory, error.SubCategory, "Error 子分类");
            Assert.AreEqual("发生异常", error.Message, "Error 消息");
            Assert.IsNotNull(error.Exception, "Error 应携带异常");
            Assert.AreEqual("123123123", error.Exception!.Message, "异常消息应原样传递");
            Assert.IsNotNull(error.StackFrames, "Error 应追踪堆栈");
            Assert.IsTrue(error.StackFrames!.Length > 0, "Error 堆栈帧数应大于 0 (宽松断言)");
            Log($"Error: Level={error.Level}, Message=\"{error.Message}\", Exception={error.Exception.Message}, StackFrames={error.StackFrames.Length} 帧, 通过");
        }
    }
}
