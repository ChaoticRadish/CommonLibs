namespace ChaoticKit.LibTest.Unit
{
    /// <summary>
    /// 测试日志辅助类: 把测试过程信息输出到 MSTest 的 <see cref="TestContext"/>,
    /// 可直接在测试面板 (VS 测试资源管理器 -> 选中测试 -> 输出) 中查看, 不写任何文件。
    /// 测试失败时, 该输出也会出现在失败详情 / TRX 报告中, 便于排查。
    /// </summary>
    public static class TestLog
    {
        /// <summary>
        /// 记录一行日志到当前测试的 TestContext 输出
        /// </summary>
        /// <param name="context">当前测试的 TestContext, 可为 null (为空时忽略)</param>
        /// <param name="testClassName">测试类名, 用于日志前缀, 便于区分并行执行的测试</param>
        /// <param name="message">日志内容</param>
        public static void WriteLine(TestContext? context, string testClassName, string message)
        {
            context?.WriteLine($"[{testClassName}] {message}");
        }
    }
}
