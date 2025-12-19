using Common_Util.Data.Constraint;
using Common_Util.Data.Structure.Value;
using Common_Util.Extensions;
using Common_Util.String;
using CommonLibTest_Console.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class LayeringAddressCode001() : TestBase("测试字符串标识与 '可与字符串互相转换对象' 标识时的情况")
    {
        protected override void RunImpl()
        {
            RunTest(test1, "AAA.QBQ.ASD.WEW.AA:AASG", "LayeringAddressCode 1");
            RunTest(test1, "A\\:AA.QBQ.A\\.SD.WEW.AA:AASG", "LayeringAddressCode 2");
            RunTest(test1, "A\\:AA.QBQ.A\\.SD.WEW.AA:AA\\\\SG", "LayeringAddressCode 3");


            RunTest(test2, ".asdasd.:q23q:", "TestLayerMark 1");
            RunTest(test2, ".as:dasd.q23q:", "TestLayerMark 2");
            RunTest(test2, "as:d:asd.q2.3q", "TestLayerMark 3");
            RunTest(test2, ".asd\\.asd.:q23q:", "TestLayerMark 4");

            RunTest(test3, new string[] { ".AAA.:123:", ".BBB.:QWEQWE:", ".CCC.:1:" }, "LayeringAddressCode<TestLayerMark> 1");
            RunTest(test3, new string[] { ".AAA.:12\\.3:", ".BBB.:QWEQWE:", ":1:" }, "LayeringAddressCode<TestLayerMark> 2");
            RunTest(test3, new string[] { ".A\\.AA.:123:", ".BBB.:Q\\.WEQ\\:WE:", ".CCC." }, "LayeringAddressCode<TestLayerMark> 3");
        }


        private void test1(string value)
        {
            WritePair(key: "字符串", value);

            LayeringAddressCode addressCode = value;

            WritePair(key: "转换后", addressCode.FullInfoString(),  " => \n");

            WritePair(key: "隐式转换回字符串", (string)addressCode, " => \n");

        }

        private void test2(string value)
        {
            WritePair(key: "字符串", value);

            TestLayerMark mark = value;

            WritePair(key: "转换后", mark.FullInfoString(), " => \n");

            WritePair(key: "隐式转换回字符串", (string)mark, " => \n");

        }

        private void test3(string[] strs)
        {
            WritePair(key: "字符串列表", strs.FullInfoString(), " => \n");

            LayeringAddressCode<TestLayerMark> addressCode = strs;

            WritePair(key: "转换后", addressCode.FullInfoString(), " => \n");

            WritePair(key: "隐式转换回字符串", (string)addressCode, " => \n");

        }

        private struct TestLayerMark : IStringConveying<TestLayerMark>
        {
            public string ValueA { get; set; }

            public string ValueB { get; set; }

            public void ChangeValue(string value)
            {
                StringBuilder sb1 = new();
                StringBuilder sb2 = new();
                bool aAction = false;
                bool bAction = false;    
                EscapeHelper.Ergodic(value, '\\',
                    (c, b) =>
                    {
                        if (!b)
                        {
                            switch (c)
                            {
                                case '.':
                                    aAction = !aAction;
                                    return;
                                case ':':
                                    bAction = !bAction;
                                    return;
                            }
                        }
                        if (aAction)
                        {
                            sb1.Append(c);
                        }
                        if (bAction)
                        {
                            sb2.Append(c);
                        }

                    });
                ValueA = sb1.ToString();
                ValueB = sb2.ToString();
            }

            public string ConvertToString()
            {
                return $".{EscapeHelper.AddEscape(ValueA, '\\', '.', ':')}.:{EscapeHelper.AddEscape(ValueB, '\\', '.', ':')}:";
            }

            public override string ToString() => ConvertToString();


            #region 隐式转换
            public static implicit operator TestLayerMark(string value)
            {
                TestLayerMark output = new();
                output.ChangeValue(value);
                return output;
            }
            public static implicit operator string(TestLayerMark value)
            {
                return value.ConvertToString();
            }

            #endregion

            #region 显式转换
            static explicit IStringConveying<TestLayerMark>.operator TestLayerMark(string s)
            {
                TestLayerMark output = new();
                output.ChangeValue(s);
                return output;
            }

            static explicit IStringConveying<TestLayerMark>.operator string(TestLayerMark t)
            {
                return t.ConvertToString();
            }

            #endregion
        }
    }
}
