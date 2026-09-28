namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Bit001 (纯语言学习测试, 数据全部固定, 无随机)
    /// 验证 byte 的 &lt;&lt; / &gt;&gt; 移位行为, 断言各次移位后的固定二进制值:
    /// - byte 移位前先提升为 int, 结果再截断回 byte (取低 8 位)
    /// - 移位量按 int 规则掩码处理: 实际移位数 = count &amp; 0x1F (低 5 位),
    ///   故 &lt;&lt; -1 等价于 &lt;&lt; 31, &lt;&lt; 33 等价于 &lt;&lt; 1
    /// 期望值均取自源控制台测试打印输出 (Convert.ToString(b, 2).PadLeft(8, '0'))
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class BitShiftTest : UnitTestBase
    {
        /// <summary>
        /// byte 转 8 位二进制字符串, 与源控制台打印格式一致
        /// </summary>
        private static string Bin8(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        [TestMethod]
        public void Byte_左移_各次移位结果()
        {
            Log("--- 左移: 初值 0b0100_1100, 依次 << 2, << -1, << -3 ---");
            byte b = 0b0100_1100;
            Log($"初始       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b << 2);
            Log($"<< 2  后   = {Bin8(b)}");
            Assert.AreEqual("00110000", Bin8(b), "0b0100_1100 << 2 截断回 byte 应为 0b0011_0000");

            b = (byte)(b << -1);
            Log($"<< -1 后   = {Bin8(b)} (移位量 -1 & 0x1F = 31, 高位溢出为 0)");
            Assert.AreEqual("00000000", Bin8(b), "移位量 -1 按掩码处理为 << 31, 结果应为 0");

            b = (byte)(b << -3);
            Log($"<< -3 后   = {Bin8(b)} (移位量 -3 & 0x1F = 29)");
            Assert.AreEqual("00000000", Bin8(b), "0 << -3 应为 0");

            Log("--- 左移: 初值复位 0b0100_1100, 依次 << 1, << 4, << 9 ---");
            b = 0b0100_1100;
            Log($"复位       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b << 1);
            Log($"<< 1  后   = {Bin8(b)}");
            Assert.AreEqual("10011000", Bin8(b), "0b0100_1100 << 1 应为 0b1001_1000");

            b = (byte)(b << 4);
            Log($"<< 4  后   = {Bin8(b)} (溢出高位截断)");
            Assert.AreEqual("10000000", Bin8(b), "0b1001_1000 << 4 截断回 byte 应为 0b1000_0000");

            b = (byte)(b << 9);
            Log($"<< 9  后   = {Bin8(b)} (0b1000_0000 << 9 截断回 byte 为 0)");
            Assert.AreEqual("00000000", Bin8(b), "0b1000_0000 << 9 截断回 byte 应为 0");

            Log("--- 左移: 初值复位 0b0100_1100, << 33 (33 & 0x1F = 1, 等价 << 1) ---");
            b = 0b0100_1100;
            Log($"复位       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b << 33);
            Log($"<< 33 后   = {Bin8(b)} (33 & 0x1F = 1)");
            Assert.AreEqual("10011000", Bin8(b), "移位量 33 & 0x1F = 1, 等价 << 1, 应为 0b1001_1000");
        }

        [TestMethod]
        public void Byte_右移_各次移位结果()
        {
            Log("--- 右移: 初值 0b0100_1100, 依次 >> 2, >> -1, >> -3 ---");
            byte b = 0b0100_1100;
            Log($"初始       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b >> 2);
            Log($">> 2  后   = {Bin8(b)}");
            Assert.AreEqual("00010011", Bin8(b), "0b0100_1100 >> 2 应为 0b0001_0011");

            b = (byte)(b >> -1);
            Log($">> -1 后   = {Bin8(b)} (移位量 -1 & 0x1F = 31)");
            Assert.AreEqual("00000000", Bin8(b), "移位量 -1 按掩码处理为 >> 31, 结果应为 0");

            b = (byte)(b >> -3);
            Log($">> -3 后   = {Bin8(b)} (移位量 -3 & 0x1F = 29)");
            Assert.AreEqual("00000000", Bin8(b), "0 >> -3 应为 0");

            Log("--- 右移: 初值复位 0b0100_1100, 依次 >> 1, >> 4, >> 9 ---");
            b = 0b0100_1100;
            Log($"复位       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b >> 1);
            Log($">> 1  后   = {Bin8(b)}");
            Assert.AreEqual("00100110", Bin8(b), "0b0100_1100 >> 1 应为 0b0010_0110");

            b = (byte)(b >> 4);
            Log($">> 4  后   = {Bin8(b)}");
            Assert.AreEqual("00000010", Bin8(b), "0b0010_0110 >> 4 应为 0b0000_0010");

            b = (byte)(b >> 9);
            Log($">> 9  后   = {Bin8(b)} (移位量 9 & 0x1F = 9)");
            Assert.AreEqual("00000000", Bin8(b), "0b0000_0010 >> 9 应为 0");

            Log("--- 右移: 初值复位 0b0100_1100, >> 33 (33 & 0x1F = 1, 等价 >> 1) ---");
            b = 0b0100_1100;
            Log($"复位       = {Bin8(b)}");
            Assert.AreEqual("01001100", Bin8(b));

            b = (byte)(b >> 33);
            Log($">> 33 后   = {Bin8(b)} (33 & 0x1F = 1)");
            Assert.AreEqual("00100110", Bin8(b), "移位量 33 & 0x1F = 1, 等价 >> 1, 应为 0b0010_0110");
        }

        [DataTestMethod]
        [DataRow(0x4C, 2, "<<", "00110000")]
        [DataRow(0x30, -1, "<<", "00000000")]
        [DataRow(0x00, -3, "<<", "00000000")]
        [DataRow(0x4C, 1, "<<", "10011000")]
        [DataRow(0x98, 4, "<<", "10000000")]
        [DataRow(0x80, 9, "<<", "00000000")]
        [DataRow(0x4C, 33, "<<", "10011000")]
        [DataRow(0x4C, 2, ">>", "00010011")]
        [DataRow(0x13, -1, ">>", "00000000")]
        [DataRow(0x00, -3, ">>", "00000000")]
        [DataRow(0x4C, 1, ">>", "00100110")]
        [DataRow(0x26, 4, ">>", "00000010")]
        [DataRow(0x02, 9, ">>", "00000000")]
        [DataRow(0x4C, 33, ">>", "00100110")]
        public void Byte_移位_表驱动验证(int input, int shift, string op, string expectedBin)
        {
            byte b = (byte)input;
            byte actual = op == "<<" ? (byte)(b << shift) : (byte)(b >> shift);
            string actualBin = Bin8(actual);

            Assert.AreEqual(expectedBin, actualBin,
                $"0b{Convert.ToString(input, 2).PadLeft(8, '0')} {op} {shift} 应为 {expectedBin}, 实际 {actualBin}");
            Log($"0b{Convert.ToString(input, 2).PadLeft(8, '0')} {op} {shift} = {actualBin} (期望 {expectedBin}), 通过");
        }

        [TestMethod]
        public void Byte_移位量_按低5位掩码处理()
        {
            Log("--- 验证移位量掩码: 实际移位数 = count & 0x1F (低 5 位) ---");
            byte b = 0b0100_1100;
            Assert.AreEqual((byte)(b << -1), (byte)(b << 31), "<< -1 应等价于 << 31");
            Assert.AreEqual((byte)(b >> -1), (byte)(b >> 31), ">> -1 应等价于 >> 31");
            Assert.AreEqual((byte)(b << 33), (byte)(b << 1), "<< 33 应等价于 << 1");
            Assert.AreEqual((byte)(b >> 33), (byte)(b >> 1), ">> 33 应等价于 >> 1");
            Log("<< -1/>> -1 等价于 << 31/>> 31, << 33/>> 33 等价于 << 1/>> 1, 通过");
        }
    }
}
