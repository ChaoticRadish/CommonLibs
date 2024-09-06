using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CommonLibTest_Console.Xml
{
    internal class Read001() : TestBase("测试 XmlStreamHelper 寻找节点的方法")
    {
        protected override void RunImpl()
        {
            using var tempFile = TempFileHelper.NewTempFile();
            write(tempFile);
            readAll(tempFile);
            readTest1(tempFile);
            readTest2(tempFile);
            readTest3(tempFile, 0);
            readTest3(tempFile, 1);
            readTest3(tempFile, 2);
            readTest3(tempFile, 3);
            readTest3(tempFile, 4);
        }

        private void write(TempFileHelper.TempFile tempFile)
        {
            using var stream = tempFile.OpenStream();
            using XmlWriter writer = XmlWriter.Create(stream, new()
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
            });

            XmlStreamHelper.Write(writer, new TestClass()
            {
                ValueA = "测试A",
                ValueB = new()
                {
                    Str = "测试B",
                    ValueB = 88,
                    Test = "temp",
                }
            });
        }

        private void readAll(TempFileHelper.TempFile tempFile)
        {
            using var stream = tempFile.OpenStream();
            using StreamReader sr = new StreamReader(stream);
            var allStr = sr.ReadToEnd();
            WriteLine(allStr);
        }

        private void readTest1(TempFileHelper.TempFile tempFile)
        {
            WriteEmptyLine();
            WriteLine("测试1");

            using var stream = tempFile.OpenStream();
            using XmlReader reader = XmlReader.Create(stream);

            void printPosition()
            {
                WriteLine($"当前位置: {reader.NodeType} {reader.Name} ");
            }

            bool flag;
            WriteLine("寻找 TestClass ");
            flag = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass));
            if (!flag)
            {
                WriteLine("未找到");
                printPosition();
                return;
            }
            WriteLine("寻找 ValueA");
            flag = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass.ValueA));
            if (!flag)
            {
                WriteLine("未找到");
                printPosition();
                return;
            }
            else
            {
                printPosition();
                var result = XmlStreamHelper.ReadAs(reader, typeof(string), needReadToElementEnd: true);
                WriteLine($"ValueA => \n{result?.FullInfoString() ?? "<null>"}");
                printPosition();
            }
            WriteLine("寻找 ValueB");
            flag = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClass.ValueB));
            if (!flag)
            {
                WriteLine("未找到");
                printPosition();
                return;
            }
            else
            {
                printPosition();
                var result = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), needReadToElementEnd: true);
                WriteLine($"ValueB => \n{result?.FullInfoString() ?? "<null>"}");

            }

        }


        private void readTest2(TempFileHelper.TempFile tempFile)
        {
            WriteEmptyLine();
            WriteLine("测试2");

            using var stream = tempFile.OpenStream();
            using XmlReader reader = XmlReader.Create(stream);

            void printPosition()
            {
                WriteLine($"当前位置: {reader.NodeType} {reader.Name} ");
            }

            bool flag;

            WriteLine("寻找 TestClassB");
            flag = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClassB));
            if (!flag)
            {
                WriteLine("未找到");
                printPosition();
                return;
            }
            else
            {
                printPosition();
                var result = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), existElementTag: false, needReadToElementEnd: true);
                WriteLine($"{result?.FullInfoString() ?? "<null>"}");

            }
        }

        private void readTest3(TempFileHelper.TempFile tempFile, int depth)
        {
            WriteEmptyLine();
            WriteLine("测试3");

            using var stream = tempFile.OpenStream();
            using XmlReader reader = XmlReader.Create(stream);

            void printPosition()
            {
                WriteLine($"当前位置: {reader.NodeType} {reader.Name} ");
            }

            bool flag;

            WriteLine($"寻找 TestClassB, 限定深度 {depth}");
            flag = XmlStreamHelper.ReadUntilFindElementNode(reader, nameof(TestClassB), depth);
            if (!flag)
            {
                WriteLine("未找到");
                printPosition();
                return;
            }
            else
            {
                printPosition();
                //if (XmlStreamHelper.TryReadAttribute(reader, nameof(TestClassB.Test), out string? strTest))
                //{
                //    WriteLine("读取到属性值: " + (strTest ?? "<nul>"));
                //}

                var result = XmlStreamHelper.ReadAs(reader, typeof(TestClassB), existElementTag: false, needReadToElementEnd: true, 
                    extraPropertyArgs: new()
                    {
                        AppendAfterReadAttributes = (dic) =>
                        {
                            if (dic.ContainsKey("Test"))
                            {
                                if (dic["Test"] != "TEMP")
                                {
                                    return new string[] { "wuwuwuw" };
                                }
                                else
                                {
                                    return Array.Empty<string>();
                                }
                            }
                            else
                            {
                                return Array.Empty<string>();
                            }
                        }
                    });
                WriteLine($"{result?.FullInfoString() ?? "<null>"}");

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
