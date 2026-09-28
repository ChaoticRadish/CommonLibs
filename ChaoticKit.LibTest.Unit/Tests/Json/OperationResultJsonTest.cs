using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.Json
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Json.Json002
    /// OperationResult&lt;T&gt; struct 经 Newtonsoft.Json 序列化/反序列化往返字段一致性的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 依赖 ChaoticKit.NewtonsoftJson (Newtonsoft.Json 13.0.3)。OperationResult&lt;T&gt; 为 struct,
    /// 序列化会输出全部公开读写属性 (IsSuccess/IsFailure/SuccessInfo/FailureReason/Data);
    /// 反序列化时 IsFailure 的 setter 会回写 IsSuccess, 往返后各字段应与原值一致。
    /// 测试全部使用固定数据, 无随机/计时参与断言, 确定性可复现。
    /// </remarks>
    [TestClass]
    public sealed class OperationResultJsonTest : UnitTestBase
    {
        /// <summary>
        /// 断言反序列化结果与原操作结果的各字段一致 (struct 往返校验)
        /// </summary>
        private void VerifyRoundTrip(OperationResult<TestModel> expected, OperationResult<TestModel> actual, string json)
        {
            Assert.AreEqual(expected.IsSuccess, actual.IsSuccess, "IsSuccess 往返不一致");
            Assert.AreEqual(expected.IsFailure, actual.IsFailure, "IsFailure 往返不一致");
            Assert.AreEqual(expected.SuccessInfo, actual.SuccessInfo, "SuccessInfo 往返不一致");
            Assert.AreEqual(expected.FailureReason, actual.FailureReason, "FailureReason 往返不一致");
            Assert.IsNotNull(actual.Data, "反序列化后 Data 不应为 null");
            Assert.AreEqual(expected.Data!.A, actual.Data!.A, "Data.A 往返不一致");
            Assert.AreEqual(expected.Data!.B, actual.Data!.B, "Data.B 往返不一致");
            Assert.AreEqual(expected.Data!.C, actual.Data!.C, "Data.C 往返不一致");
            Assert.AreEqual(expected.Data!.ToString(), actual.Data!.ToString(), "Data.ToString 往返不一致");
            Log($"往返一致, JSON: {json}");
        }

        [TestMethod]
        public void Serialize_成功结果_JSON包含全部字段()
        {
            Log("--- 测试: 序列化 OperationResult<TestModel> (成功) ---");
            OperationResult<TestModel> test = new TestModel("111", "222", true);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(test);
            Log($"序列化: {json}");

            // struct 的公开读写属性全部进入 JSON
            Assert.IsTrue(json.Contains("\"IsSuccess\":true"), "JSON 应包含 IsSuccess=true");
            Assert.IsTrue(json.Contains("\"IsFailure\":false"), "JSON 应包含 IsFailure=false");
            Assert.IsTrue(json.Contains("\"Data\""), "JSON 应包含 Data 字段");
            // 嵌套数据对象字段
            Assert.IsTrue(json.Contains("\"A\":\"111\""), "JSON 应包含 Data.A=\"111\"");
            Assert.IsTrue(json.Contains("\"B\":\"222\""), "JSON 应包含 Data.B=\"222\"");
            Assert.IsTrue(json.Contains("\"C\":true"), "JSON 应包含 Data.C=true");
        }

        [DataTestMethod]
        [DataRow("111", "222", true)]
        [DataRow("", "中文内容", false)]
        [DataRow("a b c", "!@#", true)]
        public void RoundTrip_反序列化ByType_字段一致(string a, string b, bool c)
        {
            Log($"--- 测试: 反序列化(json, Type) 往返, 模型 (A=\"{a}\", B=\"{b}\", C={c}) ---");
            OperationResult<TestModel> test = new TestModel(a, b, c);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(test);

            object? obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(OperationResult<TestModel>));
            Assert.IsNotNull(obj, "反序列化(json, Type) 结果不应为 null");
            Log($"反序列化(json, Type) 得到类型: {obj!.GetType().FullName}");

            var typed = (OperationResult<TestModel>)obj;
            VerifyRoundTrip(test, typed, json);
        }

        [DataTestMethod]
        [DataRow("111", "222", true)]
        [DataRow("", "中文内容", false)]
        [DataRow("a b c", "!@#", true)]
        public void RoundTrip_反序列化泛型_字段一致(string a, string b, bool c)
        {
            Log($"--- 测试: 反序列化<OperationResult<TestModel>>(json) 往返, 模型 (A=\"{a}\", B=\"{b}\", C={c}) ---");
            OperationResult<TestModel> test = new TestModel(a, b, c);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(test);

            var typed = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<TestModel>>(json);
            VerifyRoundTrip(test, typed, json);
        }

        [TestMethod]
        public void RoundTrip_失败结果_字段一致()
        {
            Log("--- 测试: 失败结果 (带原因与数据) 序列化/反序列化往返 ---");
            OperationResult<TestModel> failure = ("连接失败", new TestModel("err", "E1", false));
            Assert.IsFalse(failure.IsSuccess, "前置: 结果应为失败");
            Assert.AreEqual("连接失败", failure.FailureReason, "前置: FailureReason 应为 \"连接失败\"");
            Assert.IsNotNull(failure.Data, "前置: Data 不应为 null");

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(failure);
            Log($"序列化: {json}");
            Assert.IsTrue(json.Contains("\"IsSuccess\":false"), "JSON 应包含 IsSuccess=false");

            var typed = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<TestModel>>(json);
            VerifyRoundTrip(failure, typed, json);
        }

        /// <summary>源测试的 TestModel 复刻 (固定属性)</summary>
        private sealed class TestModel(string a, string b, bool c)
        {
            public string A { get; set; } = a;

            public string B { get; set; } = b;

            public bool C { get; set; } = c;
        }
    }
}
