using System.Numerics;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Bit002 (确定性部分, 输入全部固定)
    /// 验证 System.Numerics.BitOperations.PopCount 对固定 int 值计算非 0 位数量
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class PopCountTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow(int.MaxValue, 31, "int.MaxValue")]
        [DataRow(0b111_0111_1_0101, 9, "0b111_0111_1_0101")]
        [DataRow(0b111_0101_1_0101, 8, "0b111_0101_1_0101")]
        public void PopCount_固定输入(int value, int expected, string inputName)
        {
            Log($"--- 测试: PopCount({inputName}) ---");

            int actual = BitOperations.PopCount((uint)value);

            Assert.AreEqual(expected, actual,
                $"{inputName} (0x{value:X8}) 的非 0 位数量应为 {expected}, 实际为 {actual}");
            Log($"{inputName} = 0x{value:X8}, PopCount = {actual}, 期望 {expected}, 通过");
        }
    }
}
