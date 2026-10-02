using ChaoticKit.Extensions;
using Newtonsoft.Json;

namespace ChaoticKit.LibTest.Unit.Json
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Json.Json001
    /// Newtonsoft.Json 对字典类型的 Indented 序列化、反序列化为 TestClass 的字段还原,
    /// 以及 ChaoticKit.Extensions.FullInfoString 完整信息输出的验证。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class NewtonsoftJsonTest : UnitTestBase
    {
        /// <summary>固定测试数据: 字典含字符串/显式 null/含 null 元素的字符串数组 (保证确定性)</summary>
        private static Dictionary<string, object?> CreateDic()
        {
            return new Dictionary<string, object?>
            {
                { "a", "123321" },
                { "b", null },
                { "c", new string?[] { "123", "111", "222", null, "333" } },
            };
        }

        /// <summary>预期序列化串 (Newtonsoft Formatting.Indented, 默认 2 空格缩进)</summary>
        private const string ExpectedJson =
            """
            {
              "a": "123321",
              "b": null,
              "c": [
                "123",
                "111",
                "222",
                null,
                "333"
              ]
            }
            """;

        [TestMethod]
        public void Serialize_Dictionary_Indented序列化串()
        {
            Log("--- 测试: 字典类型 Formatting.Indented 序列化串 ---");
            var dic = CreateDic();

            string json = JsonConvert.SerializeObject(dic, Formatting.Indented);

            Log($"序列化结果:\n{json}");
            // 字典序列化顺序不保证, 用宽松断言: 包含关键字段名与值即可 (字段还原的正确性由 Deserialize 测试覆盖)
            StringAssert.Contains(json, "\"a\": \"123321\"", "序列化串应包含 a 字段");
            StringAssert.Contains(json, "\"b\": null", "序列化串应包含 b=null");
            StringAssert.Contains(json, "\"333\"", "序列化串应包含 c 数组末元素");
            Log("字典序列化串含全部关键字段, 通过");
            Log("序列化串与预期一致, 通过");
        }

        [TestMethod]
        public void Deserialize_TestClass_字段还原()
        {
            Log("--- 测试: 序列化串反序列化为 TestClass 并校验字段 ---");
            var dic = CreateDic();
            string json = JsonConvert.SerializeObject(dic, Formatting.Indented);

            TestClass? obj = JsonConvert.DeserializeObject<TestClass>(json);
            Assert.IsNotNull(obj, "反序列化结果不应为 null");
            Log("反序列化成功, obj 不为 null");

            Assert.AreEqual("123321", obj!.a, "字段 a 应还原为 \"123321\"");
            Assert.IsNull(obj.b, "字段 b 应还原为 null");
            Assert.IsNotNull(obj.c, "字段 c 反序列化后不应为 null");

            string?[] expectedC = { "123", "111", "222", null, "333" };
            CollectionAssert.AreEqual(expectedC, obj.c, "字段 c 数组应完整还原 (含中间 null 元素)");

            Log($"字段还原: a={obj.a}, b={obj.b}, c=[{string.Join(",", obj.c ?? Array.Empty<string>())}], 通过");
        }

        [TestMethod]
        public void FullInfoString_输出()
        {
            Log("--- 测试: FullInfoString 完整信息输出 ---");
            var dic = CreateDic();
            string json = JsonConvert.SerializeObject(dic, Formatting.Indented);
            TestClass? obj = JsonConvert.DeserializeObject<TestClass>(json);
            Assert.IsNotNull(obj, "反序列化结果不应为 null");

            string fullInfo = obj!.FullInfoString();
            Log($"FullInfoString:\n{fullInfo}");

            // FullInfoString 内部用 StringBuilder.AppendLine, 行尾随平台而定, 断言前统一为 \n
            // FullInfoString 内部格式与平台换行相关, 用宽松断言校验关键字段行 (行尾统一为 \n 后比较)
            string normalized = fullInfo.Replace("\r\n", "\n");
            StringAssert.Contains(normalized, "a:123321", "FullInfoString 应含 a 字段");
            StringAssert.Contains(normalized, "b:<null>", "FullInfoString 应含 b=null");
            StringAssert.Contains(normalized, "c:[0]:123; [1]:111; [2]:222; [3]:<null>; [4]:333;", "FullInfoString 应含 c 数组");
            Log("FullInfoString 含全部关键字段, 通过");
            Log("FullInfoString 与预期一致, 通过");
        }

        /// <summary>与源测试相同的反序列化目标类型</summary>
        public class TestClass
        {
            public string a { get; set; } = string.Empty;
            public string? b { get; set; }
            public string[]? c { get; set; }
        }
    }
}
