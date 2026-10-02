using ChaoticKit.Extensions;
using System.Xml;
using System.Xml.Serialization;

namespace ChaoticKit.LibTest.Unit.Xml
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Xml.Read002
    /// XmlSerializer 反序列化含 CDATA 内容的 XML: CDATA 内的尖括号/空格内容原样还原,
    /// 并验证 ChaoticKit.Extensions.FullInfoString 输出覆盖全部字段。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。纯字符串解析, 无 I/O。
    /// </remarks>
    [TestClass]
    public sealed class XmlCDataTest : UnitTestBase
    {
        private const string ValueAExpected = "ABCDEFGHIJKLMN";
        private const string ValueBExpected = "ABCDEF<GHI>JKLMN";

        [TestMethod]
        public void CData_ValueA普通文本ValueB尖括号_原样还原()
        {
            Log("--- 测试: CDATA 内尖括号内容原样还原 ---");
            string xml =
                "<TestObject001>\n" +
                "<ValueA>ABCDEFGHIJKLMN</ValueA>\n" +
                "<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>\n" +
                "<ValueC><![CDATA[<ValueA/>]]></ValueC>\n" +
                "</TestObject001>\n";

            TestObject001? result = Deserialize(xml);
            Assert.IsNotNull(result, "反序列化结果不应为 null");
            Assert.AreEqual(ValueAExpected, result!.ValueA, "ValueA 普通文本解析");
            Assert.AreEqual(ValueBExpected, result.ValueB, "ValueB CDATA 内尖括号应原样保留");
            Assert.AreEqual("<ValueA/>", result.ValueC, "ValueC CDATA 内 <ValueA/> 应原样保留");

            AssertFullInfoContains(result, "ValueC:<ValueA/>");
        }

        [TestMethod]
        public void CData_ValueC含27空格_原样保留()
        {
            Log("--- 测试: CDATA 内 27 个空格原样保留 ---");
            string valueC = "<Val" + new string(' ', 27) + "ueA/>";
            string xml =
                "<TestObject001>\n" +
                "<ValueA>ABCDEFGHIJKLMN</ValueA>\n" +
                "<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>\n" +
                "<ValueC><![CDATA[" + valueC + "]]></ValueC>\n" +
                "</TestObject001>\n";

            TestObject001? result = Deserialize(xml);
            Assert.IsNotNull(result, "反序列化结果不应为 null");
            Assert.AreEqual(ValueAExpected, result!.ValueA, "ValueA 普通文本解析");
            Assert.AreEqual(ValueBExpected, result.ValueB, "ValueB CDATA 内尖括号应原样保留");
            Assert.AreEqual(valueC, result.ValueC, "ValueC CDATA 内 27 个空格应原样保留");

            AssertFullInfoContains(result, "ValueC:" + valueC);
        }

        [TestMethod]
        public void 非CData的尖括号内容_解析抛异常()
        {
            Log("--- 测试: 非 CDATA 的 <Val ... /> 无法作为文本解析, 应抛异常 ---");
            string xml =
                "<TestObject001>\n" +
                "<ValueA>ABCDEFGHIJKLMN</ValueA>\n" +
                "<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>\n" +
                "<ValueC><Val" + new string(' ', 27) + "ueA/></ValueC>\n" +
                "</TestObject001>\n";

            InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
                () => { _ = Deserialize(xml); }, "含未转义尖括号的 XML 应解析失败");
            Log($"预期异常: {ex.Message}");
            Assert.IsInstanceOfType(ex.InnerException, typeof(XmlException), "内层应为 XmlException (XML 格式错误)");
        }

        /// <summary>从 XML 字符串反序列化 <see cref="TestObject001"/></summary>
        private static TestObject001? Deserialize(string xml)
        {
            XmlSerializer serializer = new(typeof(TestObject001));
            using StringReader reader = new(xml);
            return (TestObject001?)serializer.Deserialize(reader);
        }

        /// <summary>校验 FullInfoString 输出包含指定字段行 (换行符统一为 \n 后比较, 不依赖属性顺序)</summary>
        private void AssertFullInfoContains(TestObject001 obj, string expectedLine)
        {
            string fullInfo = obj.FullInfoString().Replace("\r\n", "\n");
            Log($"FullInfoString:\n{fullInfo}");
            StringAssert.Contains(fullInfo, expectedLine, "FullInfoString 应包含字段行: " + expectedLine);
        }

        /// <summary>与源测试相同的反序列化目标类型 (Unit 工程未引用 Console, 本地自带)</summary>
        public class TestObject001
        {
            public string ValueA { get; set; } = string.Empty;
            public string ValueB { get; set; } = string.Empty;
            public string ValueC { get; set; } = string.Empty;
        }
    }
}
