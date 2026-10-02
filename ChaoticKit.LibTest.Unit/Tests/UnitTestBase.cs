namespace ChaoticKit.LibTest.Unit
{
    /// <summary>
    /// 单元测试基类:
    /// - 提供 MSTest 自动注入的 <see cref="TestContext"/> 属性 (每个测试方法运行时注入)
    /// - 提供 <see cref="Log(string)"/> 便捷方法, 内部转发到 <see cref="TestLog"/>
    /// </summary>
    public abstract class UnitTestBase
    {
        /// <summary>
        /// MSTest 自动注入的测试上下文 (测试运行器注入, 无需手动赋值)
        /// </summary>
        public TestContext TestContext { get; set; } = null!;

        /// <summary>
        /// 记录一条日志到 TestContext 输出与日志文件
        /// </summary>
        protected void Log(string message)
        {
            TestLog.WriteLine(TestContext, GetType().Name, message);
        }
    }
}
