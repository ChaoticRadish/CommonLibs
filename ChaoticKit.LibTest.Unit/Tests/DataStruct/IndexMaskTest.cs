using ChaoticKit.Data.Struct;
using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.IndexMask001
    /// IndexMask 位遮罩: 隐式转换 / ToString / 位运算 / 相等比较 / 索引器 / Filtering / Replace
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 遍历顺序 All(false) 从低位(索引小)到高位; ToString 前缀 IndexMask_{Length}_。
    /// </remarks>
    [TestClass]
    public sealed class IndexMaskTest : UnitTestBase
    {
        private static readonly int[] TestInts = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        [TestMethod]
        public void 隐式转换_ulong()
        {
            Log("--- 测试: ulong 隐式转换 ---");
            ulong value = 0b_10101111_11001100;
            IndexMask mask = value;

            Assert.AreEqual(64, mask.Length, "ulong 应生成长度 64 的遮罩");
            // 低字节 0xCC: bit0-7 = 0,0,1,1,0,0,1,1; 高字节 0xAF: bit8-15 = 1,1,1,1,0,1,0,1
            bool[] expected = [false, false, true, true, false, false, true, true,
                               true, true, true, true, false, true, false, true];
            CollectionAssert.AreEqual(expected, mask.All().Take(16).ToArray(), "前 16 位应与数值二进制一致 (低位在前)");
            Log($"ulong 0b10101111_11001100 => 前16位 [{string.Join(",", mask.All().Take(16).Select(b => b ? 1 : 0))}], 通过");
        }

        [TestMethod]
        public void 隐式转换_byte与ToString()
        {
            Log("--- 测试: byte 隐式转换 + ToString ---");
            IndexMask mask = 0b_11001010;
            Assert.AreEqual(8, mask.Length, "byte 应生成长度 8 的遮罩");
            // bit0..7 of 0b11001010 = 0,1,0,1,0,0,1,1
            CollectionAssert.AreEqual(new[] { false, true, false, true, false, false, true, true },
                mask.All().ToArray(), "All(false) 从低位开始");

            Assert.AreEqual("IndexMask_8_01010011", mask.ToString(), "ToString() 低位在前");
            Assert.AreEqual("IndexMask_8_11001010", mask.ToString(true), "ToString(true) 高位在前");
            Log($"ToString()={mask.ToString()}, ToString(true)={mask.ToString(true)}, 通过");
        }

        [TestMethod]
        public void FromTrueChar与FromIEnumerableAndLength()
        {
            Log("--- 测试: FromTrueChar 与 IEnumerable+Length 构造 ---");
            IndexMask mask = IndexMask.FromTrueChar(".1.1.1111.", '1');
            Assert.AreEqual(10, mask.Length, "FromTrueChar 长度应与字符数一致");
            // '.1.1.1111.' 中 '1' 在索引 1,3,5,6,7,8
            CollectionAssert.AreEqual(new[] { false, true, false, true, false, true, true, true, true, false },
                mask.All().ToArray(), "'.1.1.1111.' 中 '1' 位置应为 true");

            // IEnumerable<bool> + length: 超出部分默认 false
            IndexMask m2 = new("1..1.1.1".Select(c => c == '1'), 13);
            Assert.AreEqual(13, m2.Length, "长度应为 13");
            Assert.AreEqual("10010101" + "00000", m2.ToFullString(), "超出部分应为 false");
            Log($"FromTrueChar('.1.1.1111.') 与 IndexMask(bool, 13).ToFullString()={m2.ToFullString()}, 通过");
        }

        [TestMethod]
        public void 索引器_GetSet()
        {
            Log("--- 测试: 索引器 Get/Set ---");
            IndexMask mask = 0b_11001010;
            // 0b11001010 = 202 = 128+64+8+2 => bit1,bit3,bit6,bit7 为 true
            Assert.IsTrue(mask[3], "0b11001010 的 bit3 应为 true");
            Assert.IsFalse(mask[4], "0b11001010 的 bit4 应为 false");
            Log("mask[3]=true, mask[4]=false (初始)");

            mask[3] = false;
            mask[4] = false;
            Assert.IsFalse(mask[3], "set false 后 bit3 应为 false");
            Assert.IsFalse(mask[4], "set false 后 bit4 应为 false");
            Log($"set 后 ToString()={mask.ToString()}, 通过");
        }

        [TestMethod]
        public void 位运算_等价式与相等比较()
        {
            Log("--- 测试: 位运算等价式与相等比较 ---");
            IndexMask mask1 = IndexMask.FromTrueChar("....1.1.1111.", '1');
            IndexMask mask2 = IndexMask.FromTrueChar(".1111...1.11.", '1');
            IndexMask mask3 = IndexMask.FromTrueChar(".1111...1.11.", '1');
            IndexMask mask4 = IndexMask.FromTrueChar(".1111...1.11..", '1');

            Assert.IsTrue((~(mask1 & mask2) & (~(~mask1 & ~mask2))) == (mask1 ^ mask2),
                "德摩根等价式应成立: ~(a&b) & ~(~a&~b) == a^b");
            Log("位运算等价式 (~(m1&m2) & ~(~m1&~m2)) == (m1^m2) 成立");

            Assert.IsFalse(mask1 == mask2, "不同位型不应相等");
            Assert.IsTrue(mask2 == mask3, "相同内容应相等");
            Assert.IsFalse(mask3 == mask4, "长度不同不应相等");
            Log("相等比较: m1!=m2, m2==m3, m3!=m4(长度不同), 通过");
        }

        [TestMethod]
        public void Filtering_四种组合()
        {
            Log("--- 测试: Filtering ---");
            bool[] bs = [true, false, true, false, true, true];

            CollectionAssert.AreEqual(new[] { 2, 4 }, TestInts.Filtering(bs, true, true).ToArray(), "filtering=true,overMask=true");
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 6, 7, 8, 9, 10 }, TestInts.Filtering(bs, false, true).ToArray(), "filtering=false,overMask=true");
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 6 }, TestInts.Filtering(bs, false, false).ToArray(), "filtering=false,overMask=false");
            CollectionAssert.AreEqual(new[] { 2, 4, 7, 8, 9, 10 }, TestInts.Filtering(bs, true, false).ToArray(), "filtering=true,overMask=false");
            Log("Filtering 四种组合均通过");
        }

        [TestMethod]
        public void Replace_四种组合()
        {
            Log("--- 测试: Replace ---");
            bool[] bs = [true, false, true, false, true, true];

            CollectionAssert.AreEqual(new[] { 1, -2, 3, -4, 5, 6, 7, 8, 9, 10 }, TestInts.Replace(bs, i => -i, true, true).ToArray(), "filtering=true,overMask=true");
            CollectionAssert.AreEqual(new[] { -1, 2, -3, 4, -5, -6, -7, -8, -9, -10 }, TestInts.Replace(bs, i => -i, false, true).ToArray(), "filtering=false,overMask=true");
            CollectionAssert.AreEqual(new[] { -1, 2, -3, 4, -5, -6, 7, 8, 9, 10 }, TestInts.Replace(bs, i => -i, false, false).ToArray(), "filtering=false,overMask=false");
            CollectionAssert.AreEqual(new[] { 1, -2, 3, -4, 5, 6, -7, -8, -9, -10 }, TestInts.Replace(bs, i => -i, true, false).ToArray(), "filtering=true,overMask=false");
            Log("Replace 四种组合均通过");
        }
    }
}
