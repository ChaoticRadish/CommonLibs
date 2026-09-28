using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.Operation
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Operation.Result001
    /// 可携带异常/数据的操作结果结构体 OperationResultEx 的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class OperationResultExTest : UnitTestBase
    {
        private sealed class TestClass
        {
            public string AValue { get; set; } = string.Empty;
            public string BValue { get; set; } = string.Empty;
        }

        private const string ExceptionMessage = "测试测测试";

        #region 不附带数据

        [TestMethod]
        public void NoData_FailureException_ReturnInterface()
        {
            Log("--- 测试01 不附带数据, 返回接口, 预期返回异常 ---");
            IOperationResultEx result = GetFailureEx();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.IsNotNull(result.Exception, "Exception 不应为 null");
            Assert.AreEqual(ExceptionMessage, result.Exception!.Message, "异常消息不匹配");
            Assert.IsTrue(result.FailureReason!.StartsWith("发生异常: "), "FailureReason 应以 '发生异常: ' 开头");
            Assert.IsTrue(result.ToString()!.StartsWith("<异常>"), "ToString 应以 '<异常>' 开头");
            Log($"通过: IsSuccess={result.IsSuccess}, HasException={result.HasException}, FailureReason={result.FailureReason}, ToString={result}");
        }

        [TestMethod]
        public void NoData_SuccessWithInfo_ReturnInterface()
        {
            Log("--- 测试02 不附带数据, 返回接口, 预期返回成功 ---");
            IOperationResultEx result = GetSuccessEx();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.IsFalse(result.HasException, "HasException 应为 false");
            Assert.AreEqual("成功了家人们", result.SuccessInfo, "SuccessInfo 不匹配");
            Log($"通过: SuccessInfo={result.SuccessInfo}, ToString={result}");
        }

        [TestMethod]
        public void NoData_FailureException_ReturnStruct()
        {
            Log("--- 测试03 不附带数据, 返回结构体, 预期返回异常 ---");
            OperationResultEx result = GetFailureExStruct();

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.AreEqual(ExceptionMessage, result.Exception?.Message, "隐式转换自 Exception 时消息应保留");
            Log($"通过: Exception={result.Exception?.GetType().Name}, ToString={result}");
        }

        [TestMethod]
        public void NoData_SuccessTuple_ReturnStruct()
        {
            Log("--- 测试04 不附带数据, 返回结构体, 预期返回成功 ---");
            OperationResultEx result = GetSuccessStruct();

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.AreEqual("成功了家人们", result.SuccessInfo, "(true, info) 元组隐式转换的 SuccessInfo 不匹配");
            Assert.IsFalse(result.HasException);
            Log($"通过: SuccessInfo={result.SuccessInfo}, ToString={result}");

            // 附加: bool 隐式转换
            bool asBool = result;
            Assert.IsTrue(asBool, "成功结果隐式转换为 bool 应为 true");
            Log($"通过: 成功结果隐式转换 bool={asBool}");
        }

        #endregion

        #region 附带数据

        [TestMethod]
        public void WithData_FailureException_ReturnInterface()
        {
            Log("--- 测试05 附带数据, 返回接口, 预期返回异常 ---");
            IOperationResultEx<TestClass> result = GetGenericFailureEx();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.AreEqual(ExceptionMessage, result.Exception?.Message);
            Assert.IsNull(result.Data, "失败时 Data 应为 null");
            Log($"通过: HasException={result.HasException}, Data={result.Data?.ToString() ?? "<null>"}, ToString={result}");
        }

        [TestMethod]
        public void WithData_Success_ReturnInterface()
        {
            Log("--- 测试06 附带数据, 返回接口, 预期返回成功 ---");
            IOperationResultEx<TestClass> result = GetGenericSuccessEx();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.AreEqual("成功了家人们", result.SuccessInfo, "SuccessInfo 不匹配");
            Assert.IsNotNull(result.Data, "Data 不应为 null");
            Assert.AreEqual(" c而是A值", result.Data!.AValue, "Data.AValue 不匹配");
            Assert.AreEqual("测试B值", result.Data.BValue, "Data.BValue 不匹配");
            Log($"通过: Data.AValue={result.Data.AValue}, SuccessInfo={result.SuccessInfo}, ToString={result}");
        }

        [TestMethod]
        public void WithData_FailureException_ReturnStruct()
        {
            Log("--- 测试07 附带数据, 返回结构体, 预期返回异常 ---");
            OperationResultEx<TestClass> result = GetGenericFailureExStruct();

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.IsTrue(result.HasException, "HasException 应为 true");
            Assert.AreEqual(ExceptionMessage, result.Exception?.Message);
            Log($"通过: Exception={result.Exception?.GetType().Name}, ToString={result}");
        }

        [TestMethod]
        public void WithData_SuccessImplicitFromData_ReturnStruct()
        {
            Log("--- 测试08 附带数据, 返回结构体, 预期返回成功 ---");
            OperationResultEx<TestClass> result = GetGenericSuccessStruct();

            Assert.IsTrue(result.IsSuccess, "IsSuccess 应为 true");
            Assert.IsNotNull(result.Data, "Data 不应为 null");
            Assert.AreEqual(" c而是A值", result.Data!.AValue, "Data.AValue 不匹配");
            Assert.AreEqual("测试B值", result.Data.BValue, "Data.BValue 不匹配");
            Log($"通过: Data.AValue={result.Data.AValue}, Data.BValue={result.Data.BValue}, ToString={result}");

            bool asBool = result;
            Assert.IsTrue(asBool, "成功结果隐式转换为 bool 应为 true");
            Log($"通过: 成功结果隐式转换 bool={asBool}");
        }

        #endregion

        #region 辅助方法 (还原控制台测试中的各种返回写法)

        private static IOperationResultEx GetFailureEx()
        {
            try
            {
                throw new Exception(ExceptionMessage);
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }

        private static IOperationResultEx GetSuccessEx()
        {
            try
            {
                return OperationResultEx.SuccessWithInfo("成功了家人们");
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }

        private static OperationResultEx GetFailureExStruct()
        {
            try
            {
                throw new Exception(ExceptionMessage);
            }
            catch (Exception ex)
            {
                return ex;      // Exception => OperationResultEx 隐式转换
            }
        }

        private static OperationResultEx GetSuccessStruct()
        {
            try
            {
                return (true, "成功了家人们");      // (bool, string?) => OperationResultEx 隐式转换
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static IOperationResultEx<TestClass> GetGenericFailureEx()
        {
            try
            {
                throw new Exception(ExceptionMessage);
            }
            catch (Exception ex)
            {
                return OperationResultEx<TestClass>.Failure(ex);
            }
        }

        private static IOperationResultEx<TestClass> GetGenericSuccessEx()
        {
            try
            {
                return OperationResultEx<TestClass>.Success(new TestClass()
                {
                    AValue = " c而是A值",
                    BValue = "测试B值"
                }, "成功了家人们");
            }
            catch (Exception ex)
            {
                return OperationResultEx<TestClass>.Failure(ex);
            }
        }

        private static OperationResultEx<TestClass> GetGenericFailureExStruct()
        {
            try
            {
                throw new Exception(ExceptionMessage);
            }
            catch (Exception ex)
            {
                return ex;      // Exception => OperationResultEx<T> 隐式转换
            }
        }

        private static OperationResultEx<TestClass> GetGenericSuccessStruct()
        {
            try
            {
                return new TestClass()      // T => OperationResultEx<T> 隐式转换
                {
                    AValue = " c而是A值",
                    BValue = "测试B值"
                };
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        #endregion
    }
}
