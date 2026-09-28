using ChaoticKit.String;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Hash
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.MD5.MD5001
    /// MD5Helper 从字符串/byte[]/Stream 生成 MD5 的一致性验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class MD5HelperTest : UnitTestBase
    {
        /// <summary>MD5("abc") 的已知值 (大写, 默认 toUpper=true)</summary>
        private const string MD5OfAbcUpper = "900150983CD24FB0D6963F7D28E17F72";

        [TestMethod]
        public void MD5_32_字符串与byte与流一致()
        {
            Log("--- 测试: MD5_32 三种输入形态结果一致 ---");
            string input = "测试从流生成 MD5 哈希值 abc123";
            byte[] bs = Encoding.UTF8.GetBytes(input);

            string fromString = MD5Helper.MD5_32(input);
            string fromBytes = MD5Helper.MD5_32(bs);
            string fromStream;
            using (MemoryStream ms = new(bs))
            {
                fromStream = MD5Helper.MD5_32(ms);
            }

            Assert.AreEqual(fromBytes, fromString, "string 与 byte[] 输入结果应一致");
            Assert.AreEqual(fromBytes, fromStream, "byte[] 与 Stream 输入结果应一致");
            Log($"MD5_32 一致: {fromBytes}, 通过");
        }

        [TestMethod]
        public void MD5_32_已知值校验()
        {
            Log("--- 测试: MD5_32 与已知 MD5 值对比 ---");
            string actual = MD5Helper.MD5_32(Encoding.UTF8.GetBytes("abc"));
            Assert.AreEqual(MD5OfAbcUpper, actual, "MD5(\"abc\") 应与已知值一致 (大写)");
            Log($"MD5_32(\"abc\") = {actual}, 与已知值一致, 通过");
        }

        [TestMethod]
        public void MD5_32_toUpper开关()
        {
            Log("--- 测试: toUpper 开关 (byte[] 重载) ---");
            byte[] bs = Encoding.UTF8.GetBytes("abc");
            string upper = MD5Helper.MD5_32(bs, toUpper: true);
            string lower = MD5Helper.MD5_32(bs, toUpper: false);

            Assert.AreEqual(MD5OfAbcUpper, upper, "toUpper=true 应为大写");
            Assert.AreEqual(MD5OfAbcUpper.ToLowerInvariant(), lower, "toUpper=false 应为小写");
            Log($"byte[] 重载: toUpper=true: {upper}, toUpper=false: {lower}, 通过");

            // 库行为快照: string 重载当前忽略 toUpper 参数 (始终大写), 此差异需人工确认是否修复
            string viaString = MD5Helper.MD5_32("abc", toUpper: false);
            Assert.AreEqual(MD5OfAbcUpper, viaString, "string 重载当前忽略 toUpper(行为快照)");
            Log("注意: string 重载忽略 toUpper 参数, 始终返回大写 (库行为快照, 待人工确认)");
        }
    }
}
