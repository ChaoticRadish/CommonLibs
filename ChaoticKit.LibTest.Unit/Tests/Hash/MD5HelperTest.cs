using ChaoticKit.String;
using System.Text;

namespace ChaoticKit.LibTest.Unit.Hash
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.MD5.MD5001
    /// MD5Helper.Md5Digit32 从字符串/byte[]/Stream 生成 32 位十六进制 MD5 的一致性验证,
    /// 含 toUpper 开关 (string 重载曾忽略该参数, 已修复) 与弃用重载 MD5_32 的兼容性验证
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
        public void Md5Digit32_字符串与byte与流一致()
        {
            Log("--- 测试: Md5Digit32 三种输入形态结果一致 ---");
            string input = "测试从流生成 MD5 哈希值 abc123";
            byte[] bs = Encoding.UTF8.GetBytes(input);

            string fromString = MD5Helper.Md5Digit32(input);
            string fromBytes = MD5Helper.Md5Digit32(bs);
            string fromStream;
            using (MemoryStream ms = new(bs))
            {
                fromStream = MD5Helper.Md5Digit32(ms);
            }

            Assert.AreEqual(fromBytes, fromString, "string 与 byte[] 输入结果应一致");
            Assert.AreEqual(fromBytes, fromStream, "byte[] 与 Stream 输入结果应一致");
            Log($"Md5Digit32 一致: {fromBytes}, 通过");
        }

        [TestMethod]
        public void Md5Digit32_已知值校验()
        {
            Log("--- 测试: Md5Digit32 与已知 MD5 值对比 ---");
            string actual = MD5Helper.Md5Digit32(Encoding.UTF8.GetBytes("abc"));
            Assert.AreEqual(MD5OfAbcUpper, actual, "MD5(\"abc\") 应与已知值一致 (大写)");
            Log($"Md5Digit32(\"abc\") = {actual}, 与已知值一致, 通过");
        }

        [TestMethod]
        public void Md5Digit32_toUpper开关()
        {
            Log("--- 测试: toUpper 开关 (byte[] 与 string 重载) ---");
            byte[] bs = Encoding.UTF8.GetBytes("abc");

            string upperFromBytes = MD5Helper.Md5Digit32(bs, toUpper: true);
            string lowerFromBytes = MD5Helper.Md5Digit32(bs, toUpper: false);
            Assert.AreEqual(MD5OfAbcUpper, upperFromBytes, "byte[] 重载 toUpper=true 应为大写");
            Assert.AreEqual(MD5OfAbcUpper.ToLowerInvariant(), lowerFromBytes, "byte[] 重载 toUpper=false 应为小写");
            Log($"byte[] 重载: toUpper=true: {upperFromBytes}, toUpper=false: {lowerFromBytes}");

            // 回归点: string 重载此前忽略 toUpper 参数 (始终大写), 修复后应与 byte[] 重载一致
            string upperFromString = MD5Helper.Md5Digit32("abc", toUpper: true);
            string lowerFromString = MD5Helper.Md5Digit32("abc", toUpper: false);
            Assert.AreEqual(MD5OfAbcUpper, upperFromString, "string 重载 toUpper=true 应为大写");
            Assert.AreEqual(MD5OfAbcUpper.ToLowerInvariant(), lowerFromString, "string 重载 toUpper=false 应为小写 (bug 已修复)");
            Log($"string 重载: toUpper=true: {upperFromString}, toUpper=false: {lowerFromString}, 通过");
        }

        [TestMethod]
        public void 弃用重载MD5_32_仍可用且转发到新方法()
        {
            Log("--- 测试: 弃用重载 MD5_32 的兼容性 (应转发到 Md5Digit32) ---");
#pragma warning disable CS0618 // 此处刻意验证已弃用重载的兼容行为
            string fromStringUpper = MD5Helper.MD5_32("abc");
            string fromStringLower = MD5Helper.MD5_32("abc", toUpper: false);
            string fromBytes = MD5Helper.MD5_32(Encoding.UTF8.GetBytes("abc"));
            string fromStream;
            using (MemoryStream ms = new(Encoding.UTF8.GetBytes("abc")))
            {
                fromStream = MD5Helper.MD5_32(ms);
            }
#pragma warning restore CS0618

            Assert.AreEqual(MD5OfAbcUpper, fromStringUpper, "弃用 string 重载应转发到新方法并返回大写");
            Assert.AreEqual(MD5OfAbcUpper.ToLowerInvariant(), fromStringLower, "弃用 string 重载应遵守 toUpper (随新方法一并修复)");
            Assert.AreEqual(MD5OfAbcUpper, fromBytes, "弃用 byte[] 重载结果应与新方法一致");
            Assert.AreEqual(MD5OfAbcUpper, fromStream, "弃用 Stream 重载结果应与新方法一致");

            Assert.AreEqual(fromStringUpper, MD5Helper.Md5Digit32("abc"), "新旧 string 重载结果应一致");
            Log($"弃用重载兼容性通过: 大写 {fromStringUpper}, 小写 {fromStringLower}");
        }

        [TestMethod]
        public void Md5Utf8String_与32位码转换结果一致()
        {
            Log("--- 测试: Md5Utf8String 与 ConvertMd5Digit32ToUtf8String 的一致性 ---");
            const string input = "测试 Md5Utf8String abc123";

            string direct = MD5Helper.Md5Utf8String(input);

            // 交叉验证: 直接 UTF-8 解码哈希字节
            string expected = Encoding.UTF8.GetString(Convert.FromHexString(MD5OfAbcUpper));
            Assert.AreEqual(expected, MD5Helper.Md5Utf8String("abc"), "Md5Utf8String(\"abc\") 应为哈希字节的 UTF-8 解码结果");

            // 性质: 32 位十六进制码还原后再 UTF-8 解码, 应与直接解码一致
            string viaDigit32 = MD5Helper.ConvertMd5Digit32ToUtf8String(MD5Helper.Md5Digit32(input));
            Assert.AreEqual(direct, viaDigit32, "32 位码转换结果应与 Md5Utf8String 一致");

            // 大小写不敏感
            string viaLowerDigit32 = MD5Helper.ConvertMd5Digit32ToUtf8String(MD5Helper.Md5Digit32(input, toUpper: false));
            Assert.AreEqual(direct, viaLowerDigit32, "小写 32 位码转换结果应与大写一致");
            Log($"Md5Utf8String 与 32 位码转换一致 (长度 {direct.Length}), 通过");
        }

        [TestMethod]
        public void 弃用重载MD5与Convert_Str32ToUTF8_仍可用且转发到新方法()
        {
            Log("--- 测试: 弃用重载 MD5 / Convert_Str32ToUTF8 的兼容性 ---");
            const string input = "测试 Md5Utf8String abc123";

#pragma warning disable CS0618 // 此处刻意验证已弃用重载的兼容行为
            string viaOldMd5 = MD5Helper.MD5(input);
            string viaOldConvert = MD5Helper.Convert_Str32ToUTF8(MD5OfAbcUpper);
#pragma warning restore CS0618

            Assert.AreEqual(MD5Helper.Md5Utf8String(input), viaOldMd5, "弃用 MD5 应转发到 Md5Utf8String");
            Assert.AreEqual(MD5Helper.ConvertMd5Digit32ToUtf8String(MD5OfAbcUpper), viaOldConvert,
                "弃用 Convert_Str32ToUTF8 应转发到 ConvertMd5Digit32ToUtf8String");
            Log("弃用重载 MD5 / Convert_Str32ToUTF8 兼容性通过");
        }
    }
}
