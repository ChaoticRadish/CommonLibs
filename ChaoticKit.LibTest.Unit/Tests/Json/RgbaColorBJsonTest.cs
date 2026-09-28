using ChaoticKit.Data.Structure.Pair;
using System.Text.Json;

namespace ChaoticKit.LibTest.Unit.Json
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Json.Color001
    /// RgbaColorB 的 System.Text.Json 固定值纯序列化与反序列化往返验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// RgbaColorB 通过 [JsonConverter(RgbaColorBJsonConverter)] 序列化为 uint 十进制数字 (非字符串/非对象);
    /// 测试固定值 0xFF112233 与改 R 通道后的 0x66112233, 断言序列化字符串与反序列化往返。
    /// </remarks>
    [TestClass]
    public sealed class RgbaColorBJsonTest : UnitTestBase
    {
        /// <summary>固定初始色值: R=FF G=11 B=22 A=33</summary>
        private const uint InitialColor = 0xFF112233;

        [TestMethod]
        public void Serialize_固定值纯序列化()
        {
            Log("--- 测试: RgbaColorB 固定值纯序列化 ---");
            RgbaColorB color = InitialColor;

            string value = JsonSerializer.Serialize(color);
            Log($"Serialize(0xFF112233) = {value}");

            // 转换器输出 uint 十进制数字, 0xFF112233 = 4279312947
            Assert.AreEqual("4279312947", value, "0xFF112233 应序列化为十进制数字 4279312947");
            Assert.AreEqual("FF112233", uint.Parse(value).ToString("X2"), "十进制数字按 X2 还原应为 FF112233");
            Log("序列化字符串与十六进制还原均符合固定值, 通过");
        }

        [TestMethod]
        public void Deserialize_往返还原()
        {
            Log("--- 测试: RgbaColorB 反序列化往返还原 ---");
            RgbaColorB color = InitialColor;

            RgbaColorB back = JsonSerializer.Deserialize<RgbaColorB>("4279312947");
            Log($"Deserialize(4279312947) = {back}");

            Assert.AreEqual(color, back, "反序列化结果应与原颜色值相等");
            Assert.AreEqual((byte)0xFF, back.R, "R 通道应还原为 0xFF");
            Assert.AreEqual((byte)0x11, back.G, "G 通道应还原为 0x11");
            Assert.AreEqual((byte)0x22, back.B, "B 通道应还原为 0x22");
            Assert.AreEqual((byte)0x33, back.A, "A 通道应还原为 0x33");
            Log("往返还原一致, 四通道断言通过");
        }

        [TestMethod]
        public void SetR_改R通道后序列化与往返()
        {
            Log("--- 测试: 修改 R 通道后再次序列化与往返 ---");
            RgbaColorB color = InitialColor;

            byte assigned = color.R = 0x66; // 0xFF112233 → 0x66112233
            Log($"color.R = 0x66, 赋值表达式返回 {assigned}");
            Assert.AreEqual((byte)0x66, assigned, "R 赋值表达式应返回 0x66");
            Assert.AreEqual((byte)0x66, color.R, "R 通道应更新为 0x66");

            string value = JsonSerializer.Serialize(color);
            Log($"Serialize(0x66112233) = {value}");

            // 0x66112233 = 1712398899
            Assert.AreEqual("1712398899", value, "0x66112233 应序列化为十进制数字 1712398899");
            Assert.AreEqual("66112233", uint.Parse(value).ToString("X2"), "十进制数字按 X2 还原应为 66112233");

            RgbaColorB back = JsonSerializer.Deserialize<RgbaColorB>(value);
            Assert.AreEqual(color, back, "反序列化结果应与修改后的颜色值相等");
            Assert.AreEqual((byte)0x66, back.R, "R 通道应还原为 0x66");
            Assert.AreEqual((byte)0x11, back.G, "G 通道应还原为 0x11");
            Assert.AreEqual((byte)0x22, back.B, "B 通道应还原为 0x22");
            Assert.AreEqual((byte)0x33, back.A, "A 通道应还原为 0x33");
            Log("修改 R 后序列化/往返还原一致, 通过");
        }
    }
}
