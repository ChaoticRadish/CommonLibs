using ChaoticKit.Attributes.Xml;
using ChaoticKit.Data.Struct;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Extensions;
using ChaoticKit.IO;
using ChaoticKit.Xml;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ChaoticKit.LibTest.Unit.Xml
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Xml.ReadWrite001 (测试4)
    /// XmlStreamHelper 流式写入/读取验证: TestB 各字段往返 + extraProperty(Text/Stream/KeyValues) + LayeringAddressCode/OperationResult
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// DateTime.Now 已改为固定值 (2024-01-02 03:04:05.678), 与 XmlTextValueAttribute 默认格式 yyyy-MM-dd HH:mm:ss:fff 一致可精确往返, 避免精度丢失;
    /// 临时文件由 TempFileHelper 创建于系统临时目录, finally 兜底删除; 无随机/计时。
    /// 行为快照: 未注册类型标签映射的 OperationResult&lt;LayeringAddressCode&gt; 元素 (ValueI[2] 与 extraProperty ETest3["C"]) 读取为 null。
    /// 本类改动 XmlStreamHelper 静态类型标签映射并使用 TempFileHelper 静态状态, 加 [DoNotParallelize] 避免与全局 MethodLevel 并行冲突。
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public sealed class XmlReadWriteTest : UnitTestBase
    {
        /// <summary>固定时间值 (毫秒精度与 XmlTextValueAttribute.DateTimeFormat 的 yyyy-MM-dd HH:mm:ss:fff 一致, 可精确往返)</summary>
        private static readonly DateTime FixedTime = new(2024, 1, 2, 3, 4, 5, 678);

        [TestMethod]
        public void TestB_WriteRead_字段与extraProperty往返()
        {
            Log("--- 测试: XmlStreamHelper.Write + ReadAs, TestB 字段与 extraProperty 往返 ---");

            // 与源控制台测试一致的确定性字节序列 (0..16..0 三角波形)
            byte[] bs = BuildTriangleBytes(6000);
            Log($"bs 长度: {bs.Length}, 首 8 字节: {bs.AsSpan(0, 8).ToArray().ToHexString()}");

            // 注册 LayeringAddressCode 的类型标签映射 (源测试在写入前调用)
            XmlStreamHelper.AddTypeTagNameMapping<LayeringAddressCode>();

            LayeringAddressCode code = (LayeringAddressCode)"qwedqw.qw3e.asd";
            TestB obj = new()
            {
                ValueA = " 321 <> 31   ",
                ValueB = 21,
                ValueD = FixedTime,
                ValueE = FixedTime,
                ValueG = [123456, "123123", code],
                ValueI = new object[] { 123456, "123123", (OperationResult<LayeringAddressCode>)code },
            };

            string? tempFilePath = null;
            try
            {
                using (TempFileHelper.TempFile tempFile = TempFileHelper.NewTempFile())
                {
                    tempFilePath = tempFile.Path;
                    Log($"临时文件: {tempFile.Path}");

                    // ---- 写入 ----
                    using (FileStream writeStream = tempFile.OpenStream())
                    {
                        using XmlWriter writer = XmlWriter.Create(writeStream, new()
                        {
                            Encoding = Encoding.UTF8,
                            OmitXmlDeclaration = true,
                            Indent = true,
                        });

                        using MemoryStream ms = new(bs);
                        Log("写入开始");
                        XmlStreamHelper.Write(writer, obj.GetType(), obj,
                            extraProperty: new XmlStreamHelper.ExtraPropertyElementCollection()
                            {
                                { "ETest1", "测试文本111111111111111111" },
                                { "ETest2", ms },
                                { "ETest3", new Dictionary<string, object?>()
                                    {
                                        { "A", 1 },
                                        { "B", "sbsbsbs" },
                                        { "C", (OperationResult<LayeringAddressCode>)code },
                                        { "D", code },
                                    }
                                },
                            });
                        Log("写入完成");
                    }

                    // ---- 回读全文, 检查关键结构 ----
                    string text;
                    using (FileStream readStream = tempFile.OpenStream())
                    {
                        using StreamReader sr = new(readStream);
                        text = sr.ReadToEnd();
                    }
                    Log("文件所有内容: \n" + text);
                    StringAssert.Contains(text, "Test2", "XML 应包含根节点 Test2");
                    StringAssert.Contains(text, "ETest1", "XML 应包含额外属性 ETest1");
                    StringAssert.Contains(text, "ETest2", "XML 应包含额外属性 ETest2");
                    StringAssert.Contains(text, "ETest3", "XML 应包含额外属性 ETest3");

                    // ---- 读取 + extraProperty 回调捕获 ----
                    string? etest1Text = null;
                    byte[]? etest2Bytes = null;
                    Dictionary<string, object?>? etest3KeyValues = null;

                    object? readed;
                    using (FileStream readStream = tempFile.OpenStream())
                    {
                        using XmlReader reader = XmlReader.Create(readStream);

                        Log("读取开始");
                        readed = XmlStreamHelper.ReadAs(reader, obj.GetType(),
                            extraPropertyArgs: new XmlStreamHelper.ExtraPropertyElementReadSetting(["ETest1", "ETest2", "ETest3"])
                            {
                                ReadText = (key, value) =>
                                {
                                    Log($"!!! Text: {key} => {value ?? "<null>"}");
                                    if (key == "ETest1") etest1Text = value;
                                },
                                ReadStream = (key, temp) =>
                                {
                                    using var stream = temp.OpenStream();
                                    using MemoryStream buffer = new();
                                    stream.CopyTo(buffer);
                                    byte[] bytes = buffer.ToArray();
                                    Log($"!!! Stream: {key} => 长度 {bytes.Length}, 首 32 字节: {bytes.AsSpan(0, Math.Min(32, bytes.Length)).ToArray().ToHexString()}");
                                    if (key == "ETest2") etest2Bytes = bytes;
                                    return true;   // 释放库创建的临时文件
                                },
                                ReadKeyValues = (key, keyValues) =>
                                {
                                    Log($"!!! KeyValues: {key} => \n{keyValues?.FullInfoString() ?? "<null>"}");
                                    if (key == "ETest3") etest3KeyValues = keyValues;
                                },
                            });
                    }
                    Assert.IsNotNull(readed, "ReadAs 读取结果不应为 null");
                    Log($"读取结果 FullInfoString: \n{readed!.FullInfoString()}");

                    TestB result = (TestB)readed;

                    // ---- 字段往返断言 ----
                    Assert.AreEqual(" 321 <> 31   ", result.ValueA, "ValueA 字符串应原样往返 (含空格与 <> 转义)");
                    Assert.AreEqual(21, result.ValueB, "ValueB (XmlAttribute int?) 应为 21");
                    // 库行为快照: XmlTextValue 对 LayeringAddressCode 读回为空 (当前实现不支持该类型作为文本值), 待人工确认
                    Assert.AreEqual(string.Empty, result.ValueC.ToString(), "库行为快照: XmlTextValue 的 LayeringAddressCode 读回为空 (待确认)");
                    Assert.AreEqual(FixedTime, result.ValueD, "ValueD (DateTime, 走 Ticks 属性) 应精确往返");
                    Assert.AreEqual(FixedTime, result.ValueE, "ValueE (XmlTextValue DateTime, yyyy-MM-dd HH:mm:ss:fff) 应精确往返");
                    Assert.IsNull(result.ValueH, "未赋值的 ValueH (DateTime?) 应为 null");

                    Assert.IsNotNull(result.ValueG, "ValueG (object[] XmlArray 集合测试) 不应为 null");
                    Assert.AreEqual(3, result.ValueG!.Length, "ValueG 应有 3 个元素");
                    Assert.AreEqual(123456, result.ValueG[0], "ValueG[0] 应为 int 123456");
                    Assert.AreEqual("123123", result.ValueG[1], "ValueG[1] 应为字符串 123123");
                    Assert.AreEqual(code, (LayeringAddressCode)result.ValueG[2]!, "ValueG[2] 应为 LayeringAddressCode");

                    Assert.IsNotNull(result.ValueI, "ValueI (IEnumerable) 不应为 null");
                    object[] valueI = result.ValueI!.Cast<object>().ToArray();
                    Assert.AreEqual(3, valueI.Length, "ValueI 应有 3 个元素");
                    Assert.AreEqual(123456, valueI[0], "ValueI[0] 应为 int 123456");
                    Assert.AreEqual("123123", valueI[1], "ValueI[1] 应为字符串 123123");
                    Assert.IsNull(valueI[2], "行为快照: OperationResult<LayeringAddressCode> 元素无类型标签映射, 读取为 null");

                    Assert.IsTrue(result.ValueF.IsSuccess, "ValueF (OperationResult<LayeringAddressCode>) 应为成功");
                    Assert.AreEqual((LayeringAddressCode)"123.3543.3", result.ValueF.Data, "ValueF.Data 应为 123.3543.3");

                    CollectionAssert.AreEqual(new byte[] { 1, 2, 8, 66, 77, 23 }, result.Bytes01, "Bytes01 (byte[] 数组节点) 应往返一致");
                    CollectionAssert.AreEqual(new byte[] { 23, 77, 8, 66, 1, 2 }, result.Bytes02, "Bytes02 ([XmlElement] byte[] base64) 应往返一致");
                    CollectionAssert.AreEqual(new byte[] { 23, 77, 8, 66, 2, 3 }, result.Bytes03, "Bytes03 ([XmlElement][XmlTextValue] byte[] base64) 应往返一致");

                    // ---- extraProperty 回调断言 ----
                    Assert.AreEqual("测试文本111111111111111111", etest1Text, "ETest1 (Text) 文本应往返一致");
                    CollectionAssert.AreEqual(bs, etest2Bytes, "ETest2 (Stream->Base64) 字节应往返一致");
                    Assert.IsNotNull(etest3KeyValues, "ETest3 (KeyValues) 字典不应为 null");
                    Assert.AreEqual(1, etest3KeyValues!["A"], "ETest3[A] 应为 int 1");
                    Assert.AreEqual("sbsbsbs", etest3KeyValues["B"], "ETest3[B] 应为字符串 sbsbsbs");
                    Assert.AreEqual(code, (LayeringAddressCode)etest3KeyValues["D"]!, "ETest3[D] 应为 LayeringAddressCode");
                    Assert.IsNull(etest3KeyValues["C"], "行为快照: ETest3[C] (OperationResult<LayeringAddressCode>) 无类型标签映射, 读取为 null");
                }
            }
            finally
            {
                // TempFile.Dispose 会异步删除临时文件, 这里尽力兜底清理
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* 可能已被 TempFile.Dispose 异步删除 */ }
                }
            }
        }

        /// <summary>
        /// 生成确定性字节序列: 0..16..0 的三角波形 (与源控制台测试一致)
        /// </summary>
        private static byte[] BuildTriangleBytes(int length)
        {
            byte[] bs = new byte[length];
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
            return bs;
        }

        /// <summary>与源测试相同的模型类型 (Unit 工程未引用 Console, 本地自带)</summary>
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
