using System;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Converter001
    /// 验证 System 下的转换方法是否支持 "0x" 十六进制前缀字符串:
    /// - Convert.ToInt32("0x111") 默认不支持 "0x" 前缀, 应抛 FormatException
    /// - int.TryParse("0x111", out v) 默认样式不支持 "0x" 前缀, 应返回 false 且 v 为 0
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class ConverterTest : UnitTestBase
    {
        [TestMethod]
        public void ConvertToInt32_十六进制前缀字符串_抛FormatException()
        {
            Log("--- 测试: Convert.ToInt32(\"0x111\") ---");
            Assert.ThrowsException<FormatException>(
                () => Convert.ToInt32("0x111"),
                "Convert.ToInt32 默认不支持 \"0x\" 十六进制前缀, 应抛 FormatException");
            Log("Convert.ToInt32(\"0x111\") 抛 FormatException, 通过");
        }

        [TestMethod]
        public void IntTryParse_十六进制前缀字符串_返回False且输出0()
        {
            Log("--- 测试: int.TryParse(\"0x111\", out v) ---");
            bool success = int.TryParse("0x111", out int value);
            Assert.IsFalse(success, "int.TryParse 默认样式不支持 \"0x\" 前缀, 应返回 false");
            Assert.AreEqual(0, value, "TryParse 失败时 out 参数应保持为 0");
            Log($"int.TryParse(\"0x111\") 返回 {success}, v = {value}, 通过");
        }
    }
}
