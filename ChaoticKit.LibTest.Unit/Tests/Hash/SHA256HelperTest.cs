using ChaoticKit.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Hash
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Hash.SHA256_001
    /// 使用 SHA256 算法对固定 UTF-8 字符串生成哈希值, 验证哈希结果与 ChaoticKit.Extensions.ToHexString 输出
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 断言目标: 固定字符串 UTF-8 字节、SHA256 哈希值的已知值 (独立计算), 以及 ToHexString 与 System.Convert.ToHexString 的一致性。
    /// </remarks>
    [TestClass]
    public sealed class SHA256HelperTest : UnitTestBase
    {
        /// <summary>源控制台测试的固定输入串</summary>
        private const string TestSource1 = "测试使用 SHA_256 算法生成哈希值";

        /// <summary>TestSource1 的 UTF-8 字节 (大写, 连续无分隔)</summary>
        private const string TestSource1Utf8Hex = "E6B58BE8AF95E4BDBFE794A8205348415F32353620E7AE97E6B395E7949FE68890E59388E5B88CE580BC";

        /// <summary>SHA256(TestSource1) 已知值 (大写, 连续无分隔)</summary>
        private const string TestSource1Sha256Hex = "E30E61E8BC981C31EE270EF00B7AA3DBBCB6AC45E321D93F9C7F7907F80D3105";

        [TestMethod]
        public void SHA256_固定串_UTF8字节与哈希已知值校验()
        {
            Log("--- 测试: SHA256(固定UTF8串) 的字节与哈希值对照已知值 ---");
            byte[] bs = Encoding.UTF8.GetBytes(TestSource1);

            string utf8Hex = Convert.ToHexString(bs);
            Assert.AreEqual(TestSource1Utf8Hex, utf8Hex, "固定串的 UTF-8 字节应与已知值一致");
            Log($"UTF-8 数据: {bs.ToHexString()}");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] result = sha256.ComputeHash(bs);
                string hashHex = Convert.ToHexString(result);
                Assert.AreEqual(TestSource1Sha256Hex, hashHex, "SHA256(固定串) 应与已知值一致");
                Log($"哈希值: {result.ToHexString()}");
                Log($"已知值: {TestSource1Sha256Hex}, 通过");
            }
        }

        [TestMethod]
        public void SHA256_ToHexString与SystemConvert_一致()
        {
            Log("--- 测试: ChaoticKit ToHexString 与 System.Convert.ToHexString 一致性 ---");
            byte[] bs = Encoding.UTF8.GetBytes(TestSource1);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] result = sha256.ComputeHash(bs);

                // split="" 时 ToHexString 应与 Convert.ToHexString 完全一致 (连续大写)
                Assert.AreEqual(Convert.ToHexString(result), result.ToHexString(""), "split=\"\" 时 ToHexString 应等于 Convert.ToHexString");
                // 默认 split=" " 时按每字节空格分隔
                string hex = Convert.ToHexString(result);
                string joined = string.Join(" ", Enumerable.Range(0, hex.Length / 2).Select(i => hex.Substring(i * 2, 2)));
                Assert.AreEqual(joined, result.ToHexString(), "默认 split=\" \" 时 ToHexString 应为空格分隔的大写十六进制");
            }
            Log("ToHexString 与 System.Convert.ToHexString 一致, 通过");
        }

        [DataTestMethod]
        [DataRow("测试使用 SHA_256 算法生成哈希值", "E30E61E8BC981C31EE270EF00B7AA3DBBCB6AC45E321D93F9C7F7907F80D3105")]
        [DataRow("abc", "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
        [DataRow("ChaoticKit", "472826F2AB5D8E91A41B81E913428EABFA939872A6EE10B8401725A1223B1612")]
        [DataRow("", "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855")]
        public void SHA256_多组固定串_已知值校验(string source, string expectedHex)
        {
            byte[] bs = Encoding.UTF8.GetBytes(source);
            using (SHA256 sha256 = SHA256.Create())
            {
                string actual = Convert.ToHexString(sha256.ComputeHash(bs));
                Assert.AreEqual(expectedHex, actual, $"SHA256(\"{source}\") 应与已知值一致");
                Log($"SHA256(\"{source}\") = {actual}, 与已知值一致, 通过");
            }
        }

        /// <summary>把连续十六进制串每两个字符切一组 (辅助方法, 供 split=" " 格式断言)</summary>
        private static IEnumerable<string> Select2HexChars(string hex)
        {
            for (int i = 0; i < hex.Length; i += 2)
            {
                yield return hex.Substring(i, 2);
            }
        }
    }
}
