using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.Operation
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Operation.Result002
    /// OperationResult / OperationResultEx / IOperationResult 类型转换验证:
    /// 元组赋值与子结果转换 (模型对象/字符串 => OperationResult&lt;T&gt;, (IOperationResult&lt;T&gt;, Exception?) => OperationResultEx&lt;T&gt;, bool 隐式转换)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 说明: 本测试为固定数据、无随机/计时, 确定性; 源控制台测试依赖 internal 的 Console.TestModels.TestModel002,
    /// 此处用等价私有模型替代 (仅保留源测试用到的 ABC 属性)。
    /// </remarks>
    [TestClass]
    public sealed class OperationResultConversionTest : UnitTestBase
    {
        /// <summary>
        /// 本地测试模型, 替代控制台工程的 internal TestModel002 (等价, 固定数据)
        /// </summary>
        private sealed class TestModel002
        {
            public string ABC { get; set; } = string.Empty;
        }

        private const string SubTest1ABC = "qweqaweqaweqaweda";
        private const string SubTest2Reason = "失败子测试2";
        private const string ExceptionMessage = "附加异常";

        #region 辅助方法 (还原控制台子测试写法)

        /// <summary>
        /// 控制台 subTest1: 模型对象 => OperationResult&lt;T&gt; 隐式转换, 成功
        /// </summary>
        private static IOperationResult<TestModel002> SubTest1()
        {
            OperationResult<TestModel002> result;
            return result = new TestModel002() { ABC = SubTest1ABC };
        }

        /// <summary>
        /// 控制台 subTest2: 字符串 => OperationResult&lt;T&gt; 隐式转换, 失败
        /// </summary>
        private static IOperationResult<TestModel002> SubTest2()
        {
            OperationResult<TestModel002> result;
            return result = SubTest2Reason;
        }

        /// <summary>
        /// 还原控制台 test(): 子结果失败时以元组 (test2, null) 赋值返回
        /// </summary>
        private static IOperationResultEx<TestModel002> Test()
        {
            OperationResultEx<TestModel002> result;
            var test1 = SubTest1();
            if (!test1.IsSuccess)
            {
                return result = (test1, null);
            }
            var test2 = SubTest2();
            if (!test2.IsSuccess)
            {
                return result = (test2, null);
            }
            return result = true;
        }

        /// <summary>
        /// 全部分支成功时走到 `return result = true;` 的 bool 隐式转换分支
        /// </summary>
        private static IOperationResultEx<TestModel002> TestAllSuccess()
        {
            OperationResultEx<TestModel002> result;
            var test1 = SubTest1();
            if (!test1.IsSuccess)
            {
                return result = (test1, null);
            }
            var test2 = SubTest1();
            if (!test2.IsSuccess)
            {
                return result = (test2, null);
            }
            return result = true;
        }

        #endregion

        [TestMethod]
        public void SubTest1_模型对象隐式转换为成功子结果()
        {
            Log("--- 测试: subTest1 模型对象 => OperationResult<T> 隐式转换 ---");
            IOperationResult<TestModel002> test1 = SubTest1();

            Assert.IsTrue(test1.IsSuccess, "模型对象隐式转换后 IsSuccess 应为 true");
            Assert.IsFalse(test1.IsFailure, "模型对象隐式转换后 IsFailure 应为 false");
            Assert.IsNull(test1.FailureReason, "成功结果 FailureReason 应为 null");
            Assert.IsNotNull(test1.Data, "成功结果 Data 不应为 null");
            Assert.AreEqual(SubTest1ABC, test1.Data!.ABC, "Data.ABC 应保留模型对象的值");
            Assert.AreEqual("<成功>", test1.ToString(), "成功结果 ToString 应为 <成功>");

            // 反向: OperationResult<T> 隐式转换为 bool
            OperationResult<TestModel002> op = new TestModel002() { ABC = SubTest1ABC };
            bool asBool = op;
            Assert.IsTrue(asBool, "成功结果隐式转换为 bool 应为 true");

            Log($"通过: IsSuccess={test1.IsSuccess}, Data.ABC={test1.Data!.ABC}, ToString={test1}");
        }

        [TestMethod]
        public void SubTest2_字符串隐式转换为失败子结果()
        {
            Log("--- 测试: subTest2 字符串 => OperationResult<T> 隐式转换 ---");
            IOperationResult<TestModel002> test2 = SubTest2();

            Assert.IsFalse(test2.IsSuccess, "字符串隐式转换后 IsSuccess 应为 false");
            Assert.IsTrue(test2.IsFailure, "字符串隐式转换后 IsFailure 应为 true");
            Assert.AreEqual(SubTest2Reason, test2.FailureReason, "FailureReason 应等于字符串内容");
            Assert.IsNull(test2.Data, "失败结果 Data 应为 null");
            Assert.AreEqual($"<失败> {SubTest2Reason}", test2.ToString(), "失败结果 ToString 不匹配");

            Log($"通过: IsSuccess={test2.IsSuccess}, FailureReason={test2.FailureReason}, ToString={test2}");
        }

        [TestMethod]
        public void Test_子结果失败时以元组赋值返回失败()
        {
            Log("--- 测试: test() 子结果失败, (test2, null) 元组赋值返回 ---");
            IOperationResultEx<TestModel002> result = Test();

            Assert.IsFalse(result.IsSuccess, "子结果失败时最终结果 IsSuccess 应为 false");
            Assert.AreEqual(SubTest2Reason, result.FailureReason, "元组赋值应保留子结果 FailureReason");
            Assert.IsNull(result.Data, "失败子结果 Data 为 null, 元组转换后应保持 null");
            Assert.IsNull(result.SuccessInfo, "SuccessInfo 应为 null");
            Assert.IsNull(result.Exception, "元组第二元素为 null 时 Exception 应为 null");
            Assert.IsFalse(result.HasException, "HasException 应为 false");
            Assert.AreEqual($"<失败> {SubTest2Reason}", result.ToString(), "最终结果 ToString 不匹配");

            Log($"通过: IsSuccess={result.IsSuccess}, FailureReason={result.FailureReason}, ToString={result}");
        }

        [TestMethod]
        public void Test_全部子结果成功时bool隐式转换返回成功()
        {
            Log("--- 测试: 全部子结果成功, `return result = true;` bool 隐式转换 ---");
            IOperationResultEx<TestModel002> result = TestAllSuccess();

            Assert.IsTrue(result.IsSuccess, "全部成功后 IsSuccess 应为 true");
            Assert.IsNull(result.FailureReason, "成功结果 FailureReason 应为 null");
            Assert.IsNull(result.Data, "bool=true 转换走 Success(default), Data 应为 null");
            Assert.IsNull(result.SuccessInfo, "bool=true 转换 SuccessInfo 应为 null");
            Assert.IsFalse(result.HasException, "HasException 应为 false");
            Assert.AreEqual("<成功>", result.ToString(), "成功结果 ToString 应为 <成功>");

            // 反向: 结构体隐式转换为 bool
            bool asBool = (OperationResultEx<TestModel002>)result;
            Assert.IsTrue(asBool, "成功结果隐式转换为 bool 应为 true");

            Log($"通过: IsSuccess={result.IsSuccess}, ToString={result}");
        }

        [DataTestMethod]
        [DataRow(true, true, null)]
        [DataRow(false, false, "失败!")]
        public void Bool_隐式转换为操作结果(bool input, bool expectedIsSuccess, string? expectedFailureReason)
        {
            Log($"--- 测试: bool={input} => OperationResultEx<T> 隐式转换 ---");
            OperationResultEx<TestModel002> result = input;

            Assert.AreEqual(expectedIsSuccess, result.IsSuccess, $"bool={input} 转换后 IsSuccess 不匹配");
            if (expectedIsSuccess)
            {
                Assert.IsNull(result.FailureReason, "成功转换 FailureReason 应为 null");
                Assert.IsNull(result.Data, "成功转换 Data 应为 null (Success(default))");
                Assert.IsNull(result.SuccessInfo, "成功转换 SuccessInfo 应为 null");
                Assert.AreEqual("<成功>", result.ToString(), "成功 ToString 应为 <成功>");
            }
            else
            {
                Assert.AreEqual(expectedFailureReason, result.FailureReason, "失败转换 FailureReason 不匹配");
                Assert.IsNull(result.Data, "失败转换 Data 应为 null");
                Assert.AreEqual($"<失败> {expectedFailureReason}", result.ToString(), "失败 ToString 不匹配");
            }

            bool back = result;
            Assert.AreEqual(expectedIsSuccess, back, "结构体反向隐式转换为 bool 应一致");

            Log($"通过: input={input}, IsSuccess={result.IsSuccess}, FailureReason={result.FailureReason}, ToString={result}");
        }

        [TestMethod]
        public void Tuple_成功子结果元组赋值()
        {
            Log("--- 测试: (成功子结果, null) 元组赋值到 OperationResultEx<T> ---");
            OperationResultEx<TestModel002> result = (SubTest1(), null);

            Assert.IsTrue(result.IsSuccess, "成功子结果元组转换后 IsSuccess 应为 true");
            Assert.IsNotNull(result.Data, "成功子结果 Data 应保留");
            Assert.AreEqual(SubTest1ABC, result.Data!.ABC, "Data.ABC 应保留子结果的数据");
            Assert.IsNull(result.Exception, "Exception 应为 null");
            Assert.IsNull(result.SuccessInfo, "SuccessInfo 应为 null");

            Log($"通过: IsSuccess={result.IsSuccess}, Data.ABC={result.Data!.ABC}, ToString={result}");
        }

        [TestMethod]
        public void Tuple_失败子结果元组赋值()
        {
            Log("--- 测试: (失败子结果, null) 元组赋值到 OperationResultEx<T> ---");
            OperationResultEx<TestModel002> result = (SubTest2(), null);

            Assert.IsFalse(result.IsSuccess, "失败子结果元组转换后 IsSuccess 应为 false");
            Assert.AreEqual(SubTest2Reason, result.FailureReason, "FailureReason 应保留子结果的失败原因");
            Assert.IsNull(result.Data, "失败子结果 Data 为 null, 应保持 null");
            Assert.IsNull(result.Exception, "Exception 应为 null");
            Assert.IsFalse(result.HasException, "HasException 应为 false");
            Assert.AreEqual($"<失败> {SubTest2Reason}", result.ToString(), "ToString 不匹配");

            Log($"通过: IsSuccess={result.IsSuccess}, FailureReason={result.FailureReason}, ToString={result}");
        }

        [TestMethod]
        public void Tuple_携带异常的子结果元组赋值()
        {
            Log("--- 测试: (失败子结果, Exception) 元组赋值到 OperationResultEx<T> ---");
            OperationResultEx<TestModel002> result = (SubTest2(), new Exception(ExceptionMessage));

            Assert.IsFalse(result.IsSuccess, "IsSuccess 应为 false");
            Assert.AreEqual(SubTest2Reason, result.FailureReason, "FailureReason 应保留子结果的失败原因");
            Assert.IsNull(result.Data, "Data 应为 null");
            Assert.IsTrue(result.HasException, "携带异常时 HasException 应为 true");
            Assert.IsNotNull(result.Exception, "Exception 不应为 null");
            Assert.AreEqual(ExceptionMessage, result.Exception!.Message, "异常消息应保留元组传入的异常");
            Assert.IsTrue(result.ToString()!.StartsWith("<异常>"), "携带异常时 ToString 应以 <异常> 开头");

            Log($"通过: HasException={result.HasException}, Exception={result.Exception?.GetType().Name}, ToString={result}");
        }

        [TestMethod]
        public void OperationResultT_隐式转换为OperationResultExT()
        {
            Log("--- 测试: OperationResult<T> => OperationResultEx<T> 隐式转换 (数据/状态/信息保留) ---");
            OperationResult<TestModel002> success = new TestModel002() { ABC = SubTest1ABC };
            OperationResultEx<TestModel002> exSuccess = success;

            Assert.IsTrue(exSuccess.IsSuccess, "成功结果转换后 IsSuccess 应为 true");
            Assert.IsNotNull(exSuccess.Data, "Data 应保留");
            Assert.AreEqual(SubTest1ABC, exSuccess.Data!.ABC, "Data.ABC 应保留");
            Assert.IsNull(exSuccess.Exception, "转换后 Exception 应为 null");
            Assert.IsFalse(exSuccess.HasException, "HasException 应为 false");

            OperationResult<TestModel002> failure = SubTest2Reason;
            OperationResultEx<TestModel002> exFailure = failure;

            Assert.IsFalse(exFailure.IsSuccess, "失败结果转换后 IsSuccess 应为 false");
            Assert.AreEqual(SubTest2Reason, exFailure.FailureReason, "FailureReason 应保留");
            Assert.IsNull(exFailure.Data, "失败结果 Data 应为 null");
            Assert.AreEqual($"<失败> {SubTest2Reason}", exFailure.ToString(), "ToString 不匹配");

            Log($"通过: 成功转换 ToString={exSuccess}, 失败转换 ToString={exFailure}");
        }
    }
}
