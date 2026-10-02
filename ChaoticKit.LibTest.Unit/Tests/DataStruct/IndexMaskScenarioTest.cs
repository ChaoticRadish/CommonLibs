using ChaoticKit.Data.Struct;
using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.IndexMask002
    /// IndexMask 场景: FromTrueChar 构造 + All/Select 按前 N 个 true 位构造新遮罩 + Filtering/WithIndex 扩展 (ChaoticKit.Data / ChaoticKit.Extensions)
    /// </summary>
    /// <remarks>
    /// 固定数据, 无随机/计时依赖, 确定性验证。
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class IndexMaskScenarioTest : UnitTestBase
    {
        /// <summary>
        /// 固定的被测数据序列
        /// </summary>
        private static readonly int[] Ints = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        /// <summary>
        /// 从 "..111.1.1.1" (长度 11) 按 '1' 构造遮罩, 作为各测试共享的输入
        /// </summary>
        private static IndexMask CreateMask1() => IndexMask.FromTrueChar("..111.1.1.1", '1');

        [TestMethod]
        public void FromTrueChar_固定数据构造位序列()
        {
            Log("--- 测试: IndexMask.FromTrueChar(\"..111.1.1.1\", '1') ---");
            IndexMask mask1 = CreateMask1();

            Assert.AreEqual(11, mask1.Length, "遮罩长度应为 11");
            CollectionAssert.AreEqual(
                new[] { false, false, true, true, true, false, true, false, true, false, true },
                mask1.All().ToArray(),
                "mask1 各位序列不匹配");
            Assert.AreEqual("00111010101", mask1.ToFullString(), "mask1.ToFullString() 不匹配");

            Log($"mask1: Length={mask1.Length}, ToFullString=\"{mask1.ToFullString()}\", 通过");
        }

        [TestMethod]
        public void All_Select_按前五个true位构造新遮罩()
        {
            Log("--- 测试: new IndexMask(mask1.All().Select(... counter++ < 5)) ---");
            IndexMask mask1 = CreateMask1();
            int counter = 0;
            const int targetCount = 5;
            IndexMask mask2 = new(mask1.All().Select(i =>
            {
                if (!i) return false;
                return counter++ < targetCount;
            }));

            Assert.AreEqual(11, mask2.Length, "mask2 长度应为 11");
            CollectionAssert.AreEqual(
                new[] { false, false, true, true, true, false, true, false, true, false, false },
                mask2.All().ToArray(),
                "mask2 各位序列不匹配: 应只保留 mask1 中前 5 个 true 位");
            Assert.AreEqual("00111010100", mask2.ToFullString(), "mask2.ToFullString() 不匹配");

            Log($"mask2: Length={mask2.Length}, ToFullString=\"{mask2.ToFullString()}\", 通过");
        }

        [DataTestMethod]
        [DataRow(0, 0, 1)]
        [DataRow(1, 1, 2)]
        [DataRow(2, 2, 6)]
        [DataRow(3, 3, 8)]
        [DataRow(4, 4, 10)]
        public void Filtering_WithIndex_过滤后重编索引(int position, int expectedIndex, int expectedValue)
        {
            IndexMask mask1 = CreateMask1();
            var pairs = Ints.Filtering(mask1).WithIndex().ToArray();

            Assert.AreEqual(5, pairs.Length, "过滤后应剩 5 项");
            (int index, int value) actual = pairs[position];
            Assert.AreEqual(expectedIndex, actual.index, $"[{position}] 索引不匹配。预期: {expectedIndex}, 实际: {actual.index}");
            Assert.AreEqual(expectedValue, actual.value, $"[{position}] 值不匹配。预期: {expectedValue}, 实际: {actual.value}");

            Log($"ints.Filtering(mask1).WithIndex()[{position}] = ({actual.index}, {actual.value}), 通过");
        }

        [DataTestMethod]
        [DataRow(0, 0, 1)]
        [DataRow(1, 1, 2)]
        [DataRow(2, 5, 6)]
        [DataRow(3, 7, 8)]
        [DataRow(4, 9, 10)]
        public void WithIndex_Filtering_保留原始索引(int position, int expectedIndex, int expectedValue)
        {
            IndexMask mask1 = CreateMask1();
            var pairs = Ints.WithIndex().Filtering(mask1).ToArray();

            Assert.AreEqual(5, pairs.Length, "过滤后应剩 5 项");
            (int index, int value) actual = pairs[position];
            Assert.AreEqual(expectedIndex, actual.index, $"[{position}] 索引不匹配。预期: {expectedIndex}, 实际: {actual.index}");
            Assert.AreEqual(expectedValue, actual.value, $"[{position}] 值不匹配。预期: {expectedValue}, 实际: {actual.value}");

            Log($"ints.WithIndex().Filtering(mask1)[{position}] = ({actual.index}, {actual.value}), 通过");
        }
    }
}
