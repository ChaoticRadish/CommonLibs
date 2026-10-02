using ChaoticKit.Data.Exceptions;
using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.OperationResult002
    /// OperationFailureException 与 OperationResult / OperationResultEx 异常封装行为的验证:
    /// 失败结果 -> 异常抛出, 异常类型 / Message / 内部异常, 以及异常结果转 OperationResult 的行为
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class OperationFailureExceptionTest : UnitTestBase
    {
        private const string RawExceptionMessage = "异常测试!!! ";

        #region 还原控制台测试的三个场景 (success=true/false 表驱动)

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OperationResult_DirectThrowScenario(bool success)
        {
            Log($"--- 测试 OperationResult (发生异常), success={success} ---");
            if (success)
            {
                OperationResult result = CreateResultWithRawException(success);
                Assert.IsTrue(result.IsSuccess, "成功时 IsSuccess 应为 true, 不应抛异常");
                Log($"通过: success=true 不抛异常, result={result}");
            }
            else
            {
                // 失败时 CreateResultWithRawException 直接抛出原始异常, 封装代码不会执行
                Exception ex = Assert.ThrowsException<Exception>(
                    () =>
                    {
                        var result = CreateResultWithRawException(success);
                        if (!result)
                        {
                            throw new OperationFailureException(result);
                        }
                    },
                    "失败时应抛异常(原始异常)");
                Assert.AreEqual(typeof(Exception), ex.GetType(), "应抛出原始 System.Exception, 而非 OperationFailureException 封装");
                Assert.AreEqual(RawExceptionMessage, ex.Message, "原始异常 Message 应保留");
                Log($"通过: 抛出原始异常 {ex.GetType().Name}, Message=[{ex.Message}]");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OperationResult_FailedResultScenario(bool success)
        {
            Log($"--- 测试 OperationResult (返回失败), success={success} ---");
            if (success)
            {
                OperationResult result = CreateResultWithFailedResult(success);
                Assert.IsTrue(result.IsSuccess, "成功时 IsSuccess 应为 true, 不应抛异常");
                Log($"通过: success=true 不抛异常, result={result}");
            }
            else
            {
                // 失败时返回 "测试失败" 字符串隐式转换的失败结果, 经 if(!result) 封装为 OperationFailureException
                OperationFailureException ex = Assert.ThrowsException<OperationFailureException>(
                    () =>
                    {
                        var result = CreateResultWithFailedResult(success);
                        if (!result)
                        {
                            throw new OperationFailureException(result);
                        }
                    },
                    "失败结果应封装为 OperationFailureException");
                Assert.AreEqual("测试失败", ex.Message, "Message 应取失败结果 FailureReason");
                Assert.IsNotNull(ex.Result, "Result 不应为 null");
                Assert.IsFalse(ex.Result.IsSuccess, "Result.IsSuccess 应为 false");
                Assert.AreEqual("测试失败", ex.Result.FailureReason, "Result.FailureReason 应为 '测试失败'");
                Assert.IsNull(ex.InnerException, "普通 OperationResult 封装不应有 InnerException");
                Log($"通过: OperationFailureException, Message=[{ex.Message}], Result.IsFailure={ex.Result.IsFailure}, InnerException=null");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OperationResultEx_ExceptionCapturedScenario(bool success)
        {
            Log($"--- 测试 OperationResultEx, success={success} ---");
            if (success)
            {
                OperationResultEx result = CreateResultExWithExceptionCaptured(success);
                Assert.IsTrue(result.IsSuccess, "成功时 IsSuccess 应为 true, 不应抛异常");
                Assert.IsFalse(result.HasException, "成功时 HasException 应为 false");
                Log($"通过: success=true 不抛异常, result={result}");
            }
            else
            {
                // 失败时把异常封装进 OperationResultEx, 经 if(!result) 封装为 OperationFailureException
                OperationFailureException ex = Assert.ThrowsException<OperationFailureException>(
                    () =>
                    {
                        var result = CreateResultExWithExceptionCaptured(success);
                        if (!result)
                        {
                            throw new OperationFailureException(result);
                        }
                    },
                    "OperationResultEx 失败结果应封装为 OperationFailureException");
                Assert.AreEqual("发生异常: " + RawExceptionMessage, ex.Message, "Message 应取 FailureReason ('发生异常: ...')");
                Assert.IsNotNull(ex.Result, "Result 不应为 null");
                Assert.IsFalse(ex.Result.IsSuccess, "Result.IsSuccess 应为 false");
                Assert.IsTrue(ex.Result is IOperationResultEx { HasException: true }, "Result 应为携带异常的 OperationResultEx");
                Assert.IsNotNull(ex.InnerException, "OperationResultEx 携带的异常应成为 InnerException");
                Assert.AreEqual(typeof(Exception), ex.InnerException!.GetType(), "InnerException 类型应为原始 System.Exception");
                Assert.AreEqual(RawExceptionMessage, ex.InnerException!.Message, "InnerException 应为原始异常实例的消息");
                Log($"通过: OperationFailureException, Message=[{ex.Message}], InnerException={ex.InnerException!.GetType().Name}, HasException=true");
            }
        }

        #endregion

        #region 构造与 ThrowIfFailure

        [TestMethod]
        public void Constructor_FromString()
        {
            Log("--- 测试 new OperationFailureException(\"测试失败\") ---");
            var ex = new OperationFailureException("测试失败");

            Assert.AreEqual("测试失败", ex.Message, "Message 应取失败原因字符串");
            Assert.IsNotNull(ex.Result, "Result 不应为 null");
            Assert.IsFalse(ex.Result.IsSuccess, "Result.IsSuccess 应为 false");
            Assert.AreEqual("测试失败", ex.Result.FailureReason, "Result.FailureReason 应等于传入字符串");
            Assert.IsNull(ex.InnerException, "字符串构造不应有 InnerException");
            Log($"通过: Message=[{ex.Message}], Result.FailureReason=[{ex.Result.FailureReason}], InnerException=null");
        }

        [TestMethod]
        public void Constructor_FromOperationResult()
        {
            Log("--- 测试 new OperationFailureException((OperationResult)\"测试失败\") ---");
            OperationResult result = "测试失败";   // string => OperationResult 隐式转换 (失败)
            var ex = new OperationFailureException(result);

            Assert.AreEqual("测试失败", ex.Message, "Message 应取结果 FailureReason");
            Assert.IsFalse(ex.Result.IsSuccess, "Result.IsSuccess 应为 false");
            Assert.AreEqual(result.FailureReason, ex.Result.FailureReason, "Result 应保留传入的结果内容");
            Assert.IsNull(ex.InnerException, "普通 OperationResult 封装不应有 InnerException");
            Log($"通过: Message=[{ex.Message}], Result.IsFailure={ex.Result.IsFailure}, InnerException=null");
        }

        [TestMethod]
        public void Constructor_FromOperationResultEx()
        {
            Log("--- 测试 new OperationFailureException(OperationResultEx 携带异常) ---");
            OperationResultEx result = OperationResultEx.Failure(new Exception(RawExceptionMessage));
            var ex = new OperationFailureException(result);

            Assert.AreEqual("发生异常: " + RawExceptionMessage, ex.Message, "Message 应取结果 FailureReason");
            Assert.IsNotNull(ex.InnerException, "OperationResultEx 携带的异常应成为 InnerException");
            Assert.AreSame(result.Exception, ex.InnerException, "InnerException 应为结果中保存的同一异常实例");
            Assert.AreEqual(RawExceptionMessage, ex.InnerException!.Message, "InnerException.Message 应保留");
            Assert.IsFalse(ex.Result.IsSuccess, "Result.IsSuccess 应为 false");
            Log($"通过: Message=[{ex.Message}], InnerException={ex.InnerException!.GetType().Name}, 同一实例={ReferenceEquals(result.Exception, ex.InnerException)}");
        }

        [TestMethod]
        public void ThrowIfFailure_OnFailure_Throws()
        {
            Log("--- 测试 ThrowIfFailure(失败结果) ---");
            OperationResult result = "测试失败";

            OperationFailureException ex = Assert.ThrowsException<OperationFailureException>(
                () => OperationFailureException.ThrowIfFailure(result),
                "失败结果应抛 OperationFailureException");
            Assert.AreEqual("测试失败", ex.Message, "Message 应取失败原因");
            Log($"通过: 抛出 OperationFailureException, Message=[{ex.Message}]");
        }

        [TestMethod]
        public void ThrowIfFailure_OnSuccess_NoThrow()
        {
            Log("--- 测试 ThrowIfFailure(成功结果) ---");
            OperationResult result = true;

            OperationFailureException.ThrowIfFailure(result);   // 成功结果不应抛异常

            Log("通过: 成功结果不抛异常");
        }

        #endregion

        #region 异常结果转 OperationResult 的行为

        [TestMethod]
        public void ToOperationResult_WithException_ReturnsFailedResult()
        {
            Log("--- 测试 OperationResultEx.ToOperationResult() (失败带异常) ---");
            OperationResultEx exResult = OperationResultEx.Failure(new Exception(RawExceptionMessage));

            OperationResult op = exResult.ToOperationResult();

            // 回归点: 此前方法体写成 "if (Success)", 引用的是静态属性 OperationResultEx.Success (恒为成功),
            // 导致失败结果也被转换成成功结果; 已修正为 "if (IsSuccess)"。
            Assert.IsFalse(op.IsSuccess, "失败结果转换后 IsSuccess 应为 false");
            Assert.AreEqual("发生异常: " + RawExceptionMessage, op.FailureReason, "应保留 FailureReason 中的异常信息");
            Log($"通过: ToOperationResult()={op}, IsSuccess={op.IsSuccess}");
        }

        [TestMethod]
        public void ToOperationResult_ExceptionOnly_NoFailureReason_FallbackToExceptionMessage()
        {
            Log("--- 测试 OperationResultEx.ToOperationResult() (仅异常, 无 FailureReason) ---");
            OperationResultEx exResult = new OperationResultEx
            {
                IsSuccess = false,
                Exception = new Exception(RawExceptionMessage),
            };

            OperationResult op = exResult.ToOperationResult();

            // 回归点: 同 ToOperationResult_WithException_ReturnsFailedResult (恒成功 bug 已修复);
            // 此用例无 FailureReason, 应走 else 分支回退到异常消息。
            Assert.IsFalse(op.IsSuccess, "失败结果转换后 IsSuccess 应为 false");
            Assert.AreEqual("发生异常: " + RawExceptionMessage, op.FailureReason, "无 FailureReason 时应回退到异常消息");
            Log($"通过: ToOperationResult()={op}, IsSuccess={op.IsSuccess}");
        }

        [TestMethod]
        public void ToOperationResult_SuccessWithInfo_ReturnsSuccess()
        {
            Log("--- 测试 OperationResultEx.ToOperationResult() (成功结果, 防止修复过度) ---");
            OperationResultEx okResult = OperationResultEx.SuccessWithInfo("一切正常");

            OperationResult op = okResult.ToOperationResult();

            Assert.IsTrue(op.IsSuccess, "成功结果转换后 IsSuccess 应为 true");
            Assert.AreEqual("一切正常", op.SuccessInfo, "应保留 SuccessInfo");
            Log($"通过: ToOperationResult()={op}, IsSuccess={op.IsSuccess}");
        }

        [TestMethod]
        public void ImplicitConversion_OperationResultExToOperationResult()
        {
            Log("--- 测试 OperationResultEx => OperationResult 隐式转换 (失败带异常) ---");
            OperationResultEx exResult = OperationResultEx.Failure(new Exception(RawExceptionMessage));

            OperationResult op = exResult;   // 隐式转换

            Assert.IsFalse(op.IsSuccess, "转换结果 IsSuccess 应为 false");
            Assert.AreEqual("发生异常: " + RawExceptionMessage, op.FailureReason, "隐式转换应保留失败原因");
            Log($"通过: 隐式转换 result={op}");
        }

        #endregion

        #region 辅助方法 (还原控制台测试中的各种返回写法)

        /// <summary>
        /// 还原 test1: 失败时直接抛出原始异常 (不封装)
        /// </summary>
        private static OperationResult CreateResultWithRawException(bool success)
        {
            if (success)
            {
                return true;   // bool => OperationResult 隐式转换 (成功)
            }
            else
            {
                throw new Exception(RawExceptionMessage);
            }
        }

        /// <summary>
        /// 还原 test3: 失败时返回 "测试失败" 字符串隐式转换的失败结果
        /// </summary>
        private static OperationResult CreateResultWithFailedResult(bool success)
        {
            if (success)
            {
                return true;   // bool => OperationResult 隐式转换 (成功)
            }
            else
            {
                return "测试失败";   // string => OperationResult 隐式转换 (失败)
            }
        }

        /// <summary>
        /// 还原 test2: 失败时捕获异常并封装进 OperationResultEx (Exception 隐式转换)
        /// </summary>
        private static OperationResultEx CreateResultExWithExceptionCaptured(bool success)
        {
            try
            {
                if (success)
                {
                    return true;   // bool => OperationResultEx 隐式转换 (成功)
                }
                else
                {
                    throw new Exception(RawExceptionMessage);
                }
            }
            catch (Exception ex)
            {
                return ex;   // Exception => OperationResultEx 隐式转换 (失败 + 携带异常)
            }
        }

        #endregion
    }
}
