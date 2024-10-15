using Common_Util.Extensions;
using CommonLibTest_Console.TestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommonLibTest_Console.Xml
{
    internal class Read002() : TestBase("测试 XML 解析时 CData 标签的使用")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        private void TestA()
        {
            string xml = @"<TestObject001>
<ValueA>ABCDEFGHIJKLMN</ValueA>
<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>
<ValueC><![CDATA[<ValueA/>]]></ValueC>
</TestObject001>
";
            test(xml);
        }
        [TestMethod]
        private void TestB()
        {
            string xml = @"<TestObject001>
<ValueA>ABCDEFGHIJKLMN</ValueA>
<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>
<ValueC><![CDATA[<Val                           ueA/>]]></ValueC>
</TestObject001>
";
            test(xml);
        }
        [TestMethod]
        private void TestC()
        {
            string xml = @"<TestObject001>
<ValueA>ABCDEFGHIJKLMN</ValueA>
<ValueB><![CDATA[ABCDEF<GHI>JKLMN]]></ValueB>
<ValueC><Val                           ueA/></ValueC>
</TestObject001>
";
            test(xml);
        }

        private void test(string xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TestObject001));
            using StringReader stringReader = new StringReader(xml);
            var result = (TestObject001?)xmlSerializer.Deserialize(stringReader);

            WritePair(result?.FullInfoString() ?? "<null>", split: " => \n");
        }
        
    }
}
