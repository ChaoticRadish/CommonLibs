using ChaoticKit.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ChaoticKit.LibTest.Unit.Xml
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Xml.Read001
    /// XmlStreamHelper 写固定 XML / ReadUntilFindElementNode 查找元素节点(含深度限定) / ReadAs 反序列化字段的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 说明: XmlStreamHelper.Write 会把 string 属性写成 &lt;String&gt; 包裹节点(库特性), 断言按实际输出结构书写;
    /// 深度限定查找的深度为相对调用位置的元素嵌套深度, 固定 XML 下 TestClassB 位于深度 3。
    /// </remarks>
    [TestClass]
    public sealed class XmlStreamHelperReadTest : UnitTestBase
    {
        /// <summary>
        /// 固定数据: 写入的 TestClass 对象与断言期望值
        /// </summary>
        private const string ValueA_Expected = "测试A";
        private const string Str_Expected = "测试B";
        private const byte ValueB_Expected = 88;
        private const string Test_Expected = "temp";

        [TestMethod]
        public void Write_生成固定XML_内容与预期一致()
        {
            Log("--- 测试: XmlStreamHelper.Write 生成的 XML 内容 ---");
            string file = CreateTempXmlFile();
            try
            {
                WriteFixedXml(file);

                string xml = File.ReadAllText(file, Encoding.UTF8);
                Log($"写入的 XML 全文:\n{xml}");

                StringAssert.Contains(xml, "<TestClass>", "根节点应为 TestClass");
                StringAssert.Contains(xml, "<String>测试A</String>", "ValueA(string) 应写为 <String> 文本节点 (库特性)");
                StringAssert.Contains(xml, "<TestClassB Test=\"temp\">", "ValueB 应写为 TestClassB 元素, Test 应写为 XML Attribute");
                StringAssert.Contains(xml, "<String>测试B</String>", "Str(string) 应写为 <String> 文本节点 (库特性)");
                StringAssert.Contains(xml, "<ValueB>88</ValueB>", "byte 属性应直接写为文本 88");
                StringAssert.Contains(xml, "</TestClass>", "应有根节点结束标签");
                Log("Write 生成 XML 内容与预期一致, 通过");
            }
            finally
            {
                DeleteTempXmlFile(file);
            }
        }

        [TestMethod]
        public void ReadUntilFindElementNode_依次查找TestClass_ValueA_ValueB并ReadAs()
        {
            Log("--- 测试: 依次查找 TestClass / ValueA / ValueB 并 ReadAs ---");
            string file = CreateTempXmlFile();
            try
            {
                WriteFixedXml(file);
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using XmlReader reader = XmlReader.Create(stream);

                bool found = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass));
                Assert.IsTrue(found, "应能找到根节点 TestClass");
                Log($"寻找 {nameof(TestClass)}: {found}, 当前位置 {reader.NodeType} {reader.Name}");

                found = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass.ValueA));
                Assert.IsTrue(found, "应能找到 ValueA 节点");
                Log($"寻找 {nameof(TestClass.ValueA)}: {found}, 当前位置 {reader.NodeType} {reader.Name}");

                var valueA = XmlStreamHelper.ReadAs(reader, typeof(string), needReadToElementEnd: true) as string;
                Assert.AreEqual(ValueA_Expected, valueA, "ValueA 读取结果应还原为 测试A");
                Log($"ValueA => {valueA}, 读取后位置 {reader.NodeType} {reader.Name}");

                found = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass.ValueB));
                Assert.IsTrue(found, "应能找到 ValueB 节点");
                Log($"寻找 {nameof(TestClass.ValueB)}: {found}, 当前位置 {reader.NodeType} {reader.Name}");

                var valueB = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), needReadToElementEnd: true) as TestClassB;
                Assert.IsNotNull(valueB, "ValueB 应被反序列化为 TestClassB");
                AssertTestClassB(valueB);
                Log($"ValueB => Str={valueB.Str}, ValueB={valueB.ValueB}, Test={valueB.Test}, 通过");
            }
            finally
            {
                DeleteTempXmlFile(file);
            }
        }

        [TestMethod]
        public void ReadUntilFindElementNode_不限定深度直接查找嵌套TestClassB()
        {
            Log("--- 测试: 不限定深度直接查找嵌套的 TestClassB ---");
            string file = CreateTempXmlFile();
            try
            {
                WriteFixedXml(file);
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using XmlReader reader = XmlReader.Create(stream);

                bool found = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClassB));
                Assert.IsTrue(found, "不限定深度时应能找到嵌套的 TestClassB 节点");
                Log($"寻找 {nameof(TestClassB)}: {found}, 当前位置 {reader.NodeType} {reader.Name}");

                var result = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), existElementTag: false, needReadToElementEnd: true) as TestClassB;
                Assert.IsNotNull(result, "TestClassB 应被反序列化");
                AssertTestClassB(result);
                Log($"TestClassB => Str={result.Str}, ValueB={result.ValueB}, Test={result.Test}, 通过");
            }
            finally
            {
                DeleteTempXmlFile(file);
            }
        }

        [DataTestMethod]
        [DataRow(0, false)]
        [DataRow(1, false)]
        [DataRow(2, false)]
        [DataRow(3, true)]
        [DataRow(4, false)]
        public void ReadUntilFindElementNode_深度限定查找(int depth, bool expectedFound)
        {
            Log($"--- 测试: 限定深度 {depth} 查找 TestClassB ---");
            string file = CreateTempXmlFile();
            try
            {
                WriteFixedXml(file);
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using XmlReader reader = XmlReader.Create(stream);

                bool found = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClassB), depth);
                Assert.AreEqual(expectedFound, found, $"深度 {depth} 时查找 TestClassB 的结果不匹配");
                Log($"深度 {depth}: found={found}, 当前位置 {reader.NodeType} {reader.Name}");

                if (!found)
                {
                    return;
                }

                // 找到后按控制台测试原逻辑: 带 AppendAfterReadAttributes 额外属性处理读取
                var result = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), existElementTag: false, needReadToElementEnd: true,
                    extraPropertyArgs: new XmlStreamHelper.ExtraPropertyElementReadSetting()
                    {
                        AppendAfterReadAttributes = (dic) =>
                        {
                            if (dic.ContainsKey("Test"))
                            {
                                // 固定 XML 中 Test="temp" != "TEMP", 走追加 "wuwuwuw" 的分支 (XML 中无此节点, 不影响反序列化)
                                return dic["Test"] != "TEMP" ? new string[] { "wuwuwuw" } : Array.Empty<string>();
                            }
                            else
                            {
                                return Array.Empty<string>();
                            }
                        }
                    }) as TestClassB;

                Assert.IsNotNull(result, "深度 3 找到 TestClassB 后应能反序列化");
                AssertTestClassB(result);
                Log($"TestClassB => Str={result.Str}, ValueB={result.ValueB}, Test={result.Test}, 通过");
            }
            finally
            {
                DeleteTempXmlFile(file);
            }
        }

        /// <summary>
        /// 断言 TestClassB 各字段与固定数据一致
        /// </summary>
        private static void AssertTestClassB(TestClassB value)
        {
            Assert.AreEqual(Str_Expected, value.Str, "Str 字段应还原为 测试B");
            Assert.AreEqual(ValueB_Expected, value.ValueB, "ValueB 字段应还原为 88");
            Assert.AreEqual(Test_Expected, value.Test, "XmlAttribute Test 字段应还原为 temp");
        }

        /// <summary>
        /// 在 %TEMP%\ChaoticKitTest\{Guid} 下创建空临时文件并返回其路径
        /// </summary>
        private static string CreateTempXmlFile()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "Read001.xml");
        }

        /// <summary>
        /// 用 XmlStreamHelper.Write 把固定 TestClass 数据写入指定文件
        /// </summary>
        private static void WriteFixedXml(string file)
        {
            using FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
            });

            XmlStreamHelper.Write(writer, new TestClass()
            {
                ValueA = ValueA_Expected,
                ValueB = new TestClassB()
                {
                    Str = Str_Expected,
                    ValueB = ValueB_Expected,
                    Test = Test_Expected,
                }
            });
        }

        /// <summary>
        /// 删除临时文件所在目录 (finally 中调用, 保证清理)
        /// </summary>
        private static void DeleteTempXmlFile(string file)
        {
            string? dir = Path.GetDirectoryName(file);
            if (dir != null && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        private class TestClass
        {
            public string ValueA { get; set; } = string.Empty;

            public TestClassB? ValueB { get; set; }
        }

        private class TestClassB
        {
            public string Str { get; set; } = string.Empty;

            public byte ValueB { get; set; }

            [XmlAttribute]
            public string Test { get; set; } = string.Empty;
        }
    }
}
