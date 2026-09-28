using ChaoticKit.Check;
using ChaoticKit.Extensions;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Check
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Check.CRC001
    /// CRCHelper.CRC8 计算法 (多项式 0x07, 初始值 00, 无输入/输出反转, 无结果异或) 固定数据已知值验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// DataRow 输入字节序列以十六进制字符串表示, 测试内用 Convert.FromHexString 解码为 byte[];
    /// string 输入按 UTF-8 编码为字节后计算。已知值由脚本计算并与手工推导交叉验证。
    /// </remarks>
    [TestClass]
    public sealed class CRCHelperTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("99851210", 0x36)]
        [DataRow("1144121088", 0x01)]
        [DataRow("114412108899", 0xC1)]
        [DataRow("11441210889900", 0x49)]
        public void CRC8_byte数组已知值(string hex, int expected)
        {
            Log($"--- 测试: CRC8(byte[]) 已知值 (hex={hex}) ---");
            byte[] data = Convert.FromHexString(hex);

            byte actual = CRCHelper.CRC8(data);

            Assert.AreEqual((byte)expected, actual, $"CRC8({data.ToHexString()}) 应为 0x{expected:X2}");
            Log($"CRC8(byte[{data.ToHexString()}]) = {actual} (0x{actual:X2}), 通过");
        }

        [DataTestMethod]
        [DataRow("[0x99, 0x85, 0x12, 0x10]", 0x99)]
        [DataRow("[0x11, 0x44, 0x12, 0x10, 0x88]", 0x04)]
        [DataRow("[0x11, 0x44, 0x12, 0x10, 0x88, 0x99]", 0xA8)]
        public void CRC8_string输入已知值(string str, int expected)
        {
            Log("--- 测试: CRC8(UTF8 字符串) 已知值 ---");
            byte[] data = Encoding.UTF8.GetBytes(str);

            byte actual = CRCHelper.CRC8(data);

            Assert.AreEqual((byte)expected, actual, $"CRC8(UTF8(\"{str}\")) 应为 0x{expected:X2}");
            Log($"CRC8(\"{str}\") UTF8 字节 {data.ToHexString()} = {actual} (0x{actual:X2}), 通过");
        }

        [TestMethod]
        public void CRC8_null输入抛ArgumentNullException()
        {
            Log("--- 测试: CRC8(null) 抛 ArgumentNullException ---");
            byte[]? data = null;

            Assert.ThrowsException<ArgumentNullException>(() => CRCHelper.CRC8(data));
            Log("CRC8(null) 抛出 ArgumentNullException, 通过");
        }
    }
}
