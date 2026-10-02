using ChaoticKit.Data.Struct;
using ChaoticKit.Exceptions.General;

namespace ChaoticKit.LibTest.Unit.Operation
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Operation.Result004
    /// IOperationResult.GetBrief() 简述文本格式验证 (ChaoticKit.Data)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 断言具体文本格式: 成功/失败前缀 + 成功信息/失败原因 + 可选的 <c>[数据类型 数据简述]</c> 段 + 可选的 <c>异常: 异常类型 异常消息</c> 段。
    /// 注意: GetBrief 只输出异常类型名 + Message; ValueMismatchException 通过集合初始化器添加的多条不匹配项
    /// 不会进入简述文本 (库实现只取 ex.GetType().Name 与 ex.Message), 测试中额外断言条目数量以固定该行为。
    /// </remarks>
    [TestClass]
    public sealed class OperationResultBriefTest : UnitTestBase
    {
        /// <summary>
        /// 断言某操作结果的 GetBrief() 简述文本与预期完全一致
        /// </summary>
        private void VerifyBrief(IOperationResult result, string expected, string testName)
        {
            string actual = result.GetBrief();
            Assert.AreEqual(expected, actual, $"[{testName}] GetBrief 简述文本不匹配。预期: \"{expected}\", 实际: \"{actual}\"");
            Log($"[{testName}] 通过。GetBrief = \"{actual}\"");
        }

        [TestMethod]
        public void Success_NoData()
        {
            Log("--- 测试01 无数据成功 OperationResult.Success ---");
            IOperationResult result = OperationResult.Success;

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            VerifyBrief(result, "<成功>", "无数据成功");
        }

        [TestMethod]
        public void Failure_NoData_FromString()
        {
            Log("--- 测试02 字符串隐式转换失败 (OperationResult)\"测试测试\" ---");
            IOperationResult result = (OperationResult)"测试测试";

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            VerifyBrief(result, "<失败> 测试测试", "字符串隐式失败");
        }

        [TestMethod]
        public void Ex_GenericException_NoData()
        {
            Log("--- 测试03 无数据 Exception 隐式转换 (OperationResultEx) ---");
            IOperationResult result = (OperationResultEx)new Exception("awawaw");

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result is IOperationResultEx, "结果应为 IOperationResultEx");
            VerifyBrief(result, "<失败> 发生异常: awawaw\n异常: Exception awawaw", "Exception 结果");
        }

        [TestMethod]
        public void Ex_ImpossibleForkException_NoData()
        {
            Log("--- 测试04 无数据 ImpossibleForkException 隐式转换 (OperationResultEx) ---");
            IOperationResult result = (OperationResultEx)new ImpossibleForkException("123123123");

            VerifyBrief(result, "<失败> 发生异常: 代码走到了一个理论上不可能发生的分支! 123123123\n异常: ImpossibleForkException 代码走到了一个理论上不可能发生的分支! 123123123", "ImpossibleForkException 结果");
        }

        [TestMethod]
        public void Ex_ValueMismatchException_MultiEntry_NoData()
        {
            Log("--- 测试05 无数据 ValueMismatchException 多条目 (OperationResultEx) ---");
            var exception = new ValueMismatchException("QQQQQ")
            {
                new("AAA", "BBB"),
                new("CCC", "cCc"),
                new("eee", "ddd"),
            };

            // 集合初始化器应添加 3 条不匹配项
            Assert.AreEqual(3, exception.Count(), "ValueMismatchException 应包含 3 条不匹配项");
            Log($"ValueMismatchException 包含 {exception.Count()} 条不匹配项");

            IOperationResult result = (OperationResultEx)exception;

            // 注意: 多条目不进入简述文本, 仅输出异常类型名 + Message
            string brief = result.GetBrief();
            Assert.AreEqual("<失败> 发生异常: QQQQQ\n异常: ValueMismatchException QQQQQ", brief, "ValueMismatchException 简述文本不匹配");
            Assert.IsFalse(brief.Contains("AAA"), "简述文本不应包含多条目内容 (仅异常类型名+Message)");
            Assert.IsFalse(brief.Contains("BBB"), "简述文本不应包含多条目内容 (仅异常类型名+Message)");
            Log("[ValueMismatchException 多条目] 通过。简述文本不含条目内容。");
        }

        [TestMethod]
        public void GenericSuccess_NullData()
        {
            Log("--- 测试06/09 带数据成功, 数据为 null OperationResult<TestClass>.Success(null) ---");
            // 源测试第 6、9 个调用相同 (重复调用), 一并覆盖
            IOperationResult first = OperationResult<TestClass>.Success(null);
            VerifyBrief(first, "<成功>\n[TestClass 无数据!]", "Success(null) #1");

            IOperationResult second = OperationResult<TestClass>.Success(null);
            VerifyBrief(second, "<成功>\n[TestClass 无数据!]", "Success(null) #2");
        }

        [TestMethod]
        public void ManualImpl_Success_NoData()
        {
            Log("--- 测试07 手动实现 IOperationResult<TestClass> 成功 (TestResult) ---");
            IOperationResult result = new TestResult() { IsSuccess = true };

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            VerifyBrief(result, "<成功>\n[TestClass 无数据!]", "TestResult 成功");
        }

        [TestMethod]
        public void ManualImplEx_Success_NoData()
        {
            Log("--- 测试08 手动实现 IOperationResultEx 成功无异常 (TestResultEx) ---");
            IOperationResult result = new TestResultEx() { IsSuccess = true };

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.IsFalse(((IOperationResultEx)result).HasException, "无异常时 HasException 应为 false");
            VerifyBrief(result, "<成功>\n[TestClass 无数据!]", "TestResultEx 成功");
        }

        [TestMethod]
        public void GenericSuccess_WithData()
        {
            Log("--- 测试10 带数据成功, 数据非 null OperationResult<TestClass>.Success(new(\"AAA\")) ---");
            IOperationResult result = OperationResult<TestClass>.Success(new TestClass("AAA"));

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            VerifyBrief(result, "<成功>\n[TestClass [TestClass_AAA]]", "Success(有数据)");
        }

        [TestMethod]
        public void GenericFailure_FromString()
        {
            Log("--- 测试11 字符串隐式转换失败 (OperationResult<TestClass>)\"测试测试\" ---");
            IOperationResult result = (OperationResult<TestClass>)"测试测试";

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            VerifyBrief(result, "<失败> 测试测试\n[TestClass 无数据!]", "带数据字符串失败");
        }

        [TestMethod]
        public void GenericEx_GenericException()
        {
            Log("--- 测试12 带数据 Exception 隐式转换 (OperationResultEx<TestClass>) ---");
            IOperationResult result = (OperationResultEx<TestClass>)new Exception("awawaw");

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result is IOperationResultEx, "结果应为 IOperationResultEx");
            VerifyBrief(result, "<失败> 发生异常: awawaw\n[TestClass 无数据!]\n异常: Exception awawaw", "带数据 Exception 结果");
        }

        [TestMethod]
        public void GenericEx_ImpossibleForkException()
        {
            Log("--- 测试13 带数据 ImpossibleForkException 隐式转换 (OperationResultEx<TestClass>) ---");
            IOperationResult result = (OperationResultEx<TestClass>)new ImpossibleForkException("123123123");

            VerifyBrief(result, "<失败> 发生异常: 代码走到了一个理论上不可能发生的分支! 123123123\n[TestClass 无数据!]\n异常: ImpossibleForkException 代码走到了一个理论上不可能发生的分支! 123123123", "带数据 ImpossibleForkException 结果");
        }

        [TestMethod]
        public void GenericEx_ValueMismatchException_MultiEntry()
        {
            Log("--- 测试14 带数据 ValueMismatchException 多条目 (OperationResultEx<TestClass>) ---");
            var exception = new ValueMismatchException("QQQQQ")
            {
                new("AAA", "BBB"),
                new("CCC", "cCc"),
                new("eee", "ddd"),
            };

            Assert.AreEqual(3, exception.Count(), "ValueMismatchException 应包含 3 条不匹配项");

            IOperationResult result = (OperationResultEx<TestClass>)exception;

            string brief = result.GetBrief();
            Assert.AreEqual("<失败> 发生异常: QQQQQ\n[TestClass 无数据!]\n异常: ValueMismatchException QQQQQ", brief, "带数据 ValueMismatchException 简述文本不匹配");
            Assert.IsFalse(brief.Contains("AAA"), "简述文本不应包含多条目内容 (仅异常类型名+Message)");
            Assert.IsFalse(brief.Contains("BBB"), "简述文本不应包含多条目内容 (仅异常类型名+Message)");
            Log("[带数据 ValueMismatchException 多条目] 通过。简述文本不含条目内容。");
        }

        #region 源测试私有类型复刻

        private sealed class TestClass(string v)
        {
            public string Value { get; } = v;

            public override string ToString()
            {
                return $"[TestClass_{Value}]";
            }
        }

        private class TestResult : IOperationResult<TestClass>
        {
            public TestClass? Data { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
            public string? FailureReason { get; set; }
            public string? SuccessInfo { get; set; }
        }

        private sealed class TestResultEx : TestResult, IOperationResultEx
        {
            public bool HasException { get => Exception != null; set => throw new NotSupportedException(); }
            public Exception? Exception { get; set; }
        }

        #endregion
    }
}
