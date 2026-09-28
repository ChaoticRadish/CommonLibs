using ChaoticKit.Data.Constraint;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Extensions;
using ChaoticKit.String;
using System.Text;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.LayeringAddressCode001
    /// 验证 LayeringAddressCode (层级标识为 string) 与 IStringConveying&lt;T&gt; (TestLayerMark)
    /// 与字符串的互转: 固定字符串用例, 断言 string -> 对象 -> string 往返与字段解析
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class LayeringAddressCodeTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("AAA.QBQ.ASD.WEW.AA:AASG", new string[] { "AAA", "QBQ", "ASD", "WEW", "AA", "AASG" })]
        [DataRow("A\\:AA.QBQ.A\\.SD.WEW.AA:AASG", new string[] { "A:AA", "QBQ", "A.SD", "WEW", "AA", "AASG" })]
        [DataRow("A\\:AA.QBQ.A\\.SD.WEW.AA:AA\\\\SG", new string[] { "A:AA", "QBQ", "A.SD", "WEW", "AA", "AA\\SG" })]
        public void LayeringAddressCode_字符串往返与层级解析(string value, string[] expectedLayers)
        {
            Log($"--- 测试: LayeringAddressCode 字符串转换 [{value}] ---");

            LayeringAddressCode code = value;

            string roundTrip = (string)code;
            Assert.AreEqual(value, roundTrip, "string -> LayeringAddressCode -> string 应往返一致");
            Assert.IsFalse(code.IsRange, "含项标识字符 ':' 的输入应为项编码 (IsRange=false)");
            Assert.AreEqual(expectedLayers.Length, code.LayerCount, "层级数量不匹配");
            CollectionAssert.AreEqual(expectedLayers, code.LayerValues, "解析出的层级值不匹配");

            Log($"转换后 FullInfoString: {code.FullInfoString()}");
            Log($"隐式转换回字符串: {roundTrip}");
            Log($"解析层级: [{string.Join(", ", code.LayerValues)}], IsRange={code.IsRange}, 通过");
        }

        [DataTestMethod]
        [DataRow(".asdasd.:q23q:", "asdasd", "q23q", ".asdasd.:q23q:")]
        [DataRow(".as:dasd.q23q:", "asdasd", "dasdq23q", ".asdasd.:dasdq23q:")]
        [DataRow("as:d:asd.q2.3q", "q2", "d", ".q2.:d:")]
        [DataRow(".asd\\.asd.:q23q:", "asd.asd", "q23q", ".asd\\.asd.:q23q:")]
        public void TestLayerMark_字符串往返与字段解析(string value, string expectedValueA, string expectedValueB, string expectedRoundTrip)
        {
            Log($"--- 测试: TestLayerMark 字符串转换 [{value}] ---");

            TestLayerMark mark = value;

            Assert.AreEqual(expectedValueA, mark.ValueA, "ValueA 解析不匹配");
            Assert.AreEqual(expectedValueB, mark.ValueB, "ValueB 解析不匹配");

            string roundTrip = (string)mark;
            Assert.AreEqual(expectedRoundTrip, roundTrip, "string -> TestLayerMark -> string 的结果不匹配");

            // 二次往返: 解析输出字符串应得到相同字段
            TestLayerMark mark2 = roundTrip;
            Assert.AreEqual(mark, mark2, "往返后的字符串再次解析应得到相同对象");

            Log($"转换后 FullInfoString: {mark.FullInfoString()}");
            Log($"隐式转换回字符串: {roundTrip}");
            Log($"ValueA=[{mark.ValueA}], ValueB=[{mark.ValueB}], 通过");
        }

        [DataTestMethod]
        [DataRow(new string[] { ".AAA.:123:", ".BBB.:QWEQWE:", ".CCC.:1:" }, "AAA", "123", "BBB", "QWEQWE", "CCC", "1")]
        [DataRow(new string[] { ".AAA.:12\\.3:", ".BBB.:QWEQWE:", ":1:" }, "AAA", "12.3", "BBB", "QWEQWE", "", "1")]
        [DataRow(new string[] { ".A\\.AA.:123:", ".BBB.:Q\\.WEQ\\:WE:", ".CCC." }, "A.AA", "123", "BBB", "Q.WEQ:WE", "CCC", "")]
        public void LayeringAddressCodeOfTestLayerMark_字符串数组往返与字段解析(
            string[] strs, string l0ValueA, string l0ValueB, string l1ValueA, string l1ValueB, string l2ValueA, string l2ValueB)
        {
            Log($"--- 测试: LayeringAddressCode<TestLayerMark> 字符串数组转换 [{string.Join(" | ", strs)}] ---");

            LayeringAddressCode<TestLayerMark> code = strs;

            Assert.AreEqual(strs.Length, code.LayerCount, "层级数量应与输入数组长度一致");
            Assert.IsFalse(code.IsRange, "由字符串数组转换的编码应为项编码 (IsRange=false)");

            Assert.AreEqual(l0ValueA, code.LayerValues[0].ValueA, "第 0 层 ValueA 不匹配");
            Assert.AreEqual(l0ValueB, code.LayerValues[0].ValueB, "第 0 层 ValueB 不匹配");
            Assert.AreEqual(l1ValueA, code.LayerValues[1].ValueA, "第 1 层 ValueA 不匹配");
            Assert.AreEqual(l1ValueB, code.LayerValues[1].ValueB, "第 1 层 ValueB 不匹配");
            Assert.AreEqual(l2ValueA, code.LayerValues[2].ValueA, "第 2 层 ValueA 不匹配");
            Assert.AreEqual(l2ValueB, code.LayerValues[2].ValueB, "第 2 层 ValueB 不匹配");

            // string -> 对象 -> string 往返: 转换结果再次解析应得到相同编码
            string roundTrip = (string)code;
            LayeringAddressCode<TestLayerMark> reparsed = roundTrip;
            Assert.AreEqual(code, reparsed, "string -> LayeringAddressCode<TestLayerMark> -> string -> 对象 应往返一致");
            string roundTrip2 = (string)reparsed;
            Assert.AreEqual(roundTrip, roundTrip2, "两次 string 转换结果应一致");

            Log($"转换后 FullInfoString: {code.FullInfoString()}");
            Log($"隐式转换回字符串: {roundTrip}");
            Log($"各层 ValueA/ValueB: {string.Join(", ", code.LayerValues.Select(l => $"[{l.ValueA}/{l.ValueB}]"))}, 通过");
        }

        /// <summary>
        /// 可与字符串互相转换的测试层级标识 (与源控制台测试 LayeringAddressCode001 中的定义保持一致)
        /// </summary>
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
