using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.Operation
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Operation.Result003
    /// OperationResultHelper.TryDo&lt;T&gt; 捕获成功/失败/异常的行为验证
    /// </summary>
    /// <remarks>
    /// 控制台测试 test2 用实例字段 index 计数器驱动 4 次调用的不同分支 (失败串 / 抛异常 / 子测试失败 / 成功),
    /// 迁移后: 顺序调用测试保留调用顺序依赖, 另拆 4 个独立用例逐一验证各分支。
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class OperationResultHelperTryDoTest : UnitTestBase
    {
        /// <summary>
        /// 与原控制台测试 test2 一致的实例计数器: 每次调用先自增再分支,
        /// 第 1~4 次分别命中: 失败串 / 抛异常 / 子测试失败(FailureEx) / 默认成功。
        /// MSTest 每个测试方法使用新的类实例, 计数器每次从 0 开始, 完全确定性。
        /// </summary>
        private int _index = 0;

        /// <summary>
        /// 对应控制台 test2: 依据调用次数切换分支返回 <see cref="OperationResultEx"/>
        /// </summary>
        private OperationResultEx NextStep()
        {
            _index++;
            switch (_index)
            {
                case 0:
                    return true;
                case 1:
                    return "失败";
                case 2:
                    throw new Exception("测试异常");
                case 3:
                    TestResult childResult = ChildStep();
                    if (childResult.IsSuccess)
                    {
                        return (true, "子测试 3 成功! ");
                    }
                    return OperationResultHelper.FailureEx<OperationResultEx>(childResult, "子测试 3 ");
                default:
                    return true;
            }
        }

        /// <summary>
        /// 对应控制台 test3: 内部捕获异常并返回失败结果
        /// </summary>
        private static TestResult ChildStep()
        {
            try
            {
                throw new Exception("测试异常 3");
            }
            catch (Exception ex)
            {
                return OperationResultHelper.Failure<TestResult>(ex);
            }
        }

        /// <summary>
        /// 对应控制台 test1: 经 OperationResultHelper.TryDo 执行 NextStep
        /// </summary>
        private OperationResultEx RunTryDoOnce()
        {
            return OperationResultHelper.TryDo(NextStep);
        }

        /// <summary>
        /// 对应控制台 test2/test3 使用的自定义结果类型 (实现 <see cref="IOperationResultEx"/>)
        /// </summary>
        private sealed class TestResult : IOperationResultEx
        {
            public bool IsSuccess { get; set; }
            public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
            public string? SuccessInfo { get; set; }
            public string? FailureReason { get; set; }
            public bool HasException { get => Exception != null; set => throw new NotSupportedException(); }
            public Exception? Exception { get; set; }
        }

        /// <summary>
        /// 保持顺序: 连续调用 4 次, 复刻控制台 RunImpl 的 4 次 RunTest 调用,
        /// 依次验证: 失败串 / 捕获异常 / 子测试失败(FailureEx) / 成功
        /// </summary>
        [TestMethod]
        public void TryDo_四次顺序调用_覆盖失败串_异常_子失败_成功()
        {
            Log("--- 测试: 顺序调用 4 次 (依赖 index 计数器递增) ---");

            OperationResultEx r1 = RunTryDoOnce();
            Log($"第1次: {r1}");
            Assert.IsFalse(r1.IsSuccess, "第1次应返回失败结果");
            Assert.IsTrue(r1.IsFailure, "第1次 IsFailure 应为 true");
            Assert.AreEqual("失败", r1.FailureReason, "第1次 FailureReason 应为 '失败'");
            Assert.IsFalse(r1.HasException, "第1次不应携带异常");
            Assert.IsNull(r1.Exception, "第1次 Exception 应为 null");

            OperationResultEx r2 = RunTryDoOnce();
            Log($"第2次: {r2}");
            Assert.IsFalse(r2.IsSuccess, "第2次 body 抛异常, TryDo 应转为失败结果");
            Assert.AreEqual("发生异常", r2.FailureReason, "第2次 FailureReason 应为 '发生异常'");
            Assert.IsTrue(r2.HasException, "第2次应携带异常");
            Assert.IsNotNull(r2.Exception, "第2次 Exception 不应为 null");
            Assert.AreEqual("测试异常", r2.Exception!.Message, "第2次异常消息应为 '测试异常'");

            OperationResultEx r3 = RunTryDoOnce();
            Log($"第3次: {r3}");
            Assert.IsFalse(r3.IsSuccess, "第3次应返回子测试失败结果");
            Assert.AreEqual("子测试 3: 发生异常", r3.FailureReason, "第3次 FailureReason 应为 '子测试 3: 发生异常'");
            Assert.IsTrue(r3.HasException, "第3次应携带子测试异常");
            Assert.IsNotNull(r3.Exception, "第3次 Exception 不应为 null");
            Assert.AreEqual("测试异常 3", r3.Exception!.Message, "第3次异常消息应为 '测试异常 3'");

            OperationResultEx r4 = RunTryDoOnce();
            Log($"第4次: {r4}");
            Assert.IsTrue(r4.IsSuccess, "第4次应返回成功结果");
            Assert.IsFalse(r4.IsFailure, "第4次 IsFailure 应为 false");
            Assert.IsFalse(r4.HasException, "第4次不应携带异常");
            Assert.IsNull(r4.Exception, "第4次 Exception 应为 null");
        }

        /// <summary>
        /// 独立用例: body 返回失败结果串 (不抛异常), TryDo 原样返回
        /// </summary>
        [TestMethod]
        public void TryDo_失败结果串_原样返回()
        {
            Log("--- 测试: body 返回失败结果串 (不抛异常) ---");
            _index = 0;     // 调用后 index=1, 命中 case 1: return "失败";

            OperationResultEx result = RunTryDoOnce();

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result.IsFailure, "IsFailure 应为 true");
            Assert.AreEqual("失败", result.FailureReason, "FailureReason 应为 '失败'");
            Assert.IsFalse(result.HasException, "失败结果串不应携带异常");
            Assert.IsNull(result.Exception, "Exception 应为 null");
            Log($"通过: FailureReason={result.FailureReason}, HasException={result.HasException}");
        }

        /// <summary>
        /// 独立用例: body 抛出异常, TryDo 捕获并转为携带异常的失败结果
        /// </summary>
        [TestMethod]
        public void TryDo_body抛异常_转为携带异常的失败结果()
        {
            Log("--- 测试: body 抛出异常, TryDo 捕获转换 ---");
            _index = 1;     // 调用后 index=2, 命中 case 2: throw new Exception("测试异常");

            OperationResultEx result = RunTryDoOnce();

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.AreEqual("发生异常", result.FailureReason, "FailureReason 应为 '发生异常'");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.IsNotNull(result.Exception, "Exception 不应为 null");
            Assert.AreEqual("测试异常", result.Exception!.Message, "异常消息应为 '测试异常'");
            Log($"通过: FailureReason={result.FailureReason}, Exception={result.Exception!.GetType().Name}: {result.Exception.Message}");
        }

        /// <summary>
        /// 独立用例: body 内子测试失败, FailureEx 包装为携带子异常的失败结果
        /// </summary>
        [TestMethod]
        public void TryDo_子测试失败_经FailureEx包装携带子异常()
        {
            Log("--- 测试: 子测试失败经 FailureEx 包装 ---");
            _index = 2;     // 调用后 index=3, 命中 case 3: 子测试失败分支

            OperationResultEx result = RunTryDoOnce();

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.AreEqual("子测试 3: 发生异常", result.FailureReason, "FailureReason 应为 '子测试 3: 发生异常'");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.IsNotNull(result.Exception, "Exception 不应为 null");
            Assert.AreEqual("测试异常 3", result.Exception!.Message, "异常消息应为 '测试异常 3'");
            Log($"通过: FailureReason={result.FailureReason}, Exception={result.Exception!.GetType().Name}: {result.Exception.Message}");
        }

        /// <summary>
        /// 独立用例: body 返回成功结果, TryDo 原样返回
        /// </summary>
        [TestMethod]
        public void TryDo_成功结果_原样返回()
        {
            Log("--- 测试: body 返回成功结果 ---");
            _index = 3;     // 调用后 index=4, 命中 default: return true;

            OperationResultEx result = RunTryDoOnce();

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.IsFalse(result.IsFailure, "IsFailure 应为 false");
            Assert.IsFalse(result.HasException, "成功结果不应携带异常");
            Assert.IsNull(result.Exception, "Exception 应为 null");
            Log($"通过: IsSuccess={result.IsSuccess}, ToString={result}");
        }
    }
}
