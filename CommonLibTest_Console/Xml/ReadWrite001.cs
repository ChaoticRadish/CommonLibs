using Common_Util.Attributes.Xml;
using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Xml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CommonLibTest_Console.Xml
{
    internal class ReadWrite001() : TestBase("XML 流式读写测试")
    {
        byte[] bs = [];

        protected override void RunImpl()
        {
            bs = new byte[6000];
            int newValue = 0;
            bool add = true;
            for (int i = 0; i < bs.Length; i++)
            {
                if (newValue == 0)
                {
                    add = true;
                }
                else if (newValue == 16)
                {
                    add = false;
                }
                newValue += add ? 1 : -1;
                bs[i] = (byte)newValue;
            }

            XmlStreamHelper.AddTypeTagNameMapping<LayeringAddressCode>();

            //RunTest(testWriteRead, new TestA(), "测试1");
            //RunTest(testWriteRead, new TestB() 
            //{ 
            //    ValueA = " 321  31   ", 
            //    ValueB = null,
            //    ValueC = "aa.b.ccc.qwe12"
            //}, "测试2");
            //RunTest(testWriteRead, new TestB()
            //{
            //    ValueA = " 321 <> 31   ",
            //    ValueB = 21,
            //    ValueC = "qawse.qwe.z",
            //}, "测试3");
            RunTest(testWriteRead, new TestB()
            {
                ValueA = " 321 <> 31   ",
                ValueB = 21,
                ValueD = DateTime.Now,
                ValueE = DateTime.Now,
                ValueG = [123456, "123123", (LayeringAddressCode)"qwedqw.qw3e.asd"],
                ValueI = new object[] { 123456, "123123", (OperationResult<LayeringAddressCode>)((LayeringAddressCode)"qwedqw.qw3e.asd") },
            }, "测试4");
        }

        private void testWriteRead(object obj)
        {
            using var tempFile = TempFileHelper.NewTempFile();
            WriteLine("临时文件: " + tempFile.Path);

            using (var writeStream = tempFile.OpenStream())
            {
                using XmlWriter writer = XmlWriter.Create(writeStream, new()
                {
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = true,
                    Indent = true,
                });


                using MemoryStream ms = new MemoryStream(bs);
                WriteLine("写入开始");
                XmlStreamHelper.Write(writer, obj.GetType(), obj,
                    extraProperty: new()
                    {
                        { "ETest1", "测试文本111111111111111111" },
                        { "ETest2", ms },
                        { "ETest3", new Dictionary<string, object?>() 
                            { 
                                { "A", 1 },
                                { "B", "sbsbsbs" },
                                { "C", (OperationResult<LayeringAddressCode>)((LayeringAddressCode)"qwedqw.qw3e.asd") },
                                { "D", (LayeringAddressCode)"qwedqw.qw3e.asd" },
                            } 
                        },
                    });
                WriteLine("写入完成");
            }

            using (var readStream = tempFile.OpenStream())
            {
                using StreamReader sr = new StreamReader(readStream);
                string text = sr.ReadToEnd();
                WriteLine("文件所有内容: ");
                WriteLine(text);
            }

            using (var readStream = tempFile.OpenStream())
            {
                using XmlReader reader = XmlReader.Create(readStream);

                WriteLine("读取开始");
                object? readed = XmlStreamHelper.ReadAs(reader, obj.GetType(), 
                    extraPropertyArgs: new(["ETest1", "ETest2", "ETest3"]) 
                    {
                        ReadText = (key, value) =>
                        {
                            WriteLine($"!!! Text: {key} => {value ?? "<null>"} ");
                        },
                        ReadStream = (key, tempFile) =>
                        {
                            using var stream = tempFile.OpenStream();
                            byte[] buffer = new byte[1024];
                            int i;
                            WriteLine($"!!! Stream: {key} => ");
                            StringBuilder sb = new();
                            while ((i = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                sb.Append(buffer.AsSpan(0, i).ToArray().ToHexString()).Append('\n');
                            }
                            WriteLine(sb);
                            return true;
                        },
                        ReadKeyValues = (key, dir) =>
                        {
                            WriteLine($"!!! Text: {key} => \n{dir?.FullInfoString() ?? "<null>"} ");
                        },
                    });
                if (readed == null)
                {
                    WriteLine("null");
                }
                else
                {
                    WriteLine(readed.FullInfoString());
                }
            }

            WriteEmptyLine();
        }





        [XmlRoot("Test1")]
        [XmlTextNode]
        public class TestA
        {
            public override string ToString()
            {
                return "TestAAAAA";
            }
        }

        [XmlRoot("Test2")]
        public class TestB
        {
            public string ValueA { get; set; } = string.Empty;

            [XmlAttribute]
            public required int? ValueB { get; set; }

            [XmlTextValue]
            public LayeringAddressCode ValueC { get; set; }

            public DateTime ValueD { get; set; }
            public DateTime? ValueH { get; set; }

            [XmlTextValue]
            public DateTime ValueE { get; set; }

            [XmlArray("集合测试")]
            [XmlComment("注释测试2222")]
            public object[]? ValueG { get; set; }

            public IEnumerable? ValueI { get; set; }

            public byte[] Bytes01 { get; set; } = [1, 2, 8, 66, 77, 23];

            [XmlElement]
            public byte[] Bytes02 { get; set; } = [23, 77, 8, 66, 1, 2];

            [XmlElement]
            [XmlTextValue]
            public byte[] Bytes03 { get; set; } = [23, 77, 8, 66, 2, 3];

            [XmlNoTypeTag]
            [XmlComment("注释测试")]
            public OperationResult<LayeringAddressCode> ValueF { get; set; } = (LayeringAddressCode)"123.3543.3";
        }
    }
}
