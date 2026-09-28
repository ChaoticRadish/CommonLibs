using ChaoticKit.Extensions.ObjectModel;
using ChaoticKit.Random;
using System.Collections.ObjectModel;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.ObservableCollection001
    /// ObservableCollection&lt;T&gt; 扩展 Swap / Sort 的行为验证
    /// (ChaoticKit 核心扩展: 交换位置与就地排序, 均不产生事件通知断言)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class ObservableCollectionExtensionTest : UnitTestBase
    {
        /// <summary>
        /// 排序用的测试对象: 含一个 int 键与一个 string 键
        /// </summary>
        private sealed class TestClass
        {
            public int ValueA { get; set; }

            public string ValueB { get; set; } = string.Empty;

            public override string ToString() => $"({ValueA}, {ValueB})";
        }

        /// <summary>
        /// 固定数据集合: a~g
        /// </summary>
        private static ObservableCollection<string> CreateStringCollection()
        {
            return new ObservableCollection<string>(["a", "b", "c", "d", "e", "f", "g"]);
        }

        /// <summary>
        /// 校验 ObservableCollection&lt;string&gt; 的内容与期望完全一致
        /// </summary>
        private static void AssertStringCollection(ObservableCollection<string> actual, string[] expected, string testName)
        {
            CollectionAssert.AreEqual(expected, actual.ToArray(), $"[{testName}] 集合内容不匹配。预期: [{string.Join(", ", expected)}], 实际: [{string.Join(", ", actual)}]");
        }

        /// <summary>
        /// 校验序列按指定比较器非降序排列 (排序通过交换实现且不稳定, 故只断言非降序性质)
        /// </summary>
        private static void AssertNonDescending<T>(IEnumerable<T> sequence, IComparer<T> comparer, string testName)
        {
            var list = sequence.ToList();
            Assert.IsTrue(list.Count > 0, $"[{testName}] 集合不应为空");
            for (int i = 1; i < list.Count; i++)
            {
                int cmp = comparer.Compare(list[i - 1], list[i]);
                Assert.IsTrue(cmp <= 0, $"[{testName}] 排序错误: 位置 {i - 1}({list[i - 1]}) 应不大于位置 {i}({list[i]})");
            }
        }

        #region Swap 基础交换

        [DataTestMethod]
        [DataRow(3, 6, "a,b,c,g,e,f,d")]
        [DataRow(6, 3, "a,b,c,g,e,f,d")]
        [DataRow(0, 6, "g,b,c,d,e,f,a")]
        [DataRow(6, 0, "g,b,c,d,e,f,a")]
        [DataRow(0, 1, "b,a,c,d,e,f,g")]
        [DataRow(1, 0, "b,a,c,d,e,f,g")]
        public void Swap_两个索引交换位置(int indexA, int indexB, string expectedCsv)
        {
            Log($"--- 测试: Swap({indexA}, {indexB}) ---");
            var c = CreateStringCollection();
            Log($"初始: [{string.Join(", ", c)}]");

            c.Swap(indexA, indexB);

            string[] expected = expectedCsv.Split(',');
            AssertStringCollection(c, expected, $"Swap({indexA},{indexB})");
            Log($"Swap({indexA}, {indexB}) 后: [{string.Join(", ", c)}], 通过");
        }

        #endregion

        #region 连续交换 (g1 / g2)

        [TestMethod]
        public void SwapGroup_连续交换分步断言()
        {
            Log("--- 测试(g1): 连续四次 Swap, 分步断言 ---");
            var c = CreateStringCollection();
            Log($"初始: [{string.Join(", ", c)}]");

            c.Swap(1, 0);
            AssertStringCollection(c, ["b", "a", "c", "d", "e", "f", "g"], "Swap(1,0)");
            Log($"Swap(1,0) 后: [{string.Join(", ", c)}]");

            c.Swap(2, 6);
            AssertStringCollection(c, ["b", "a", "g", "d", "e", "f", "c"], "Swap(2,6)");
            Log($"Swap(2,6) 后: [{string.Join(", ", c)}]");

            c.Swap(0, 5);
            AssertStringCollection(c, ["f", "a", "g", "d", "e", "b", "c"], "Swap(0,5)");
            Log($"Swap(0,5) 后: [{string.Join(", ", c)}]");

            c.Swap(2, 5);
            AssertStringCollection(c, ["f", "a", "b", "d", "e", "g", "c"], "Swap(2,5)");
            Log($"Swap(2,5) 后: [{string.Join(", ", c)}], 全部通过");
        }

        [TestMethod]
        public void SwapGroup_连续交换一次执行断言最终()
        {
            Log("--- 测试(g2): 连续四次 Swap, 一次执行后断言最终 ---");
            var c = CreateStringCollection();
            Log($"初始: [{string.Join(", ", c)}]");

            c.Swap(1, 0);
            c.Swap(2, 6);
            c.Swap(0, 5);
            c.Swap(2, 5);

            AssertStringCollection(c, ["f", "a", "b", "d", "e", "g", "c"], "连续交换最终状态");
            Log($"连续交换后: [{string.Join(", ", c)}], 通过");
        }

        [TestMethod]
        public void SwapGroup_连续交换后排序()
        {
            Log("--- 测试(g3): 连续四次 Swap 后再 Sort() ---");
            var c = CreateStringCollection();
            c.Swap(1, 0);
            c.Swap(2, 6);
            c.Swap(0, 5);
            c.Swap(2, 5);
            Log($"连续交换后: [{string.Join(", ", c)}]");

            c.Sort();

            AssertStringCollection(c, ["a", "b", "c", "d", "e", "f", "g"], "Sort() 后应恢复升序");
            Log($"Sort() 后: [{string.Join(", ", c)}], 通过");
        }

        #endregion

        #region Sort 随机数据 (固定种子)

        [TestMethod]
        public void Sort_随机字符串集合排序后非降序()
        {
            Log("--- 测试(randomTest1): 20 个随机小写字符串排序 (固定种子) ---");
            // 原控制台测试使用 System.Random.Shared(无种子), 迁移为固定种子以保证确定性
            var rng = new System.Random(20260927);
            var items = Enumerable.Range(0, 20)
                .Select(_ => RandomStringHelper.GetRandomLowerEnglishString(30, rng))
                .ToList();

            var c = new ObservableCollection<string>(items);
            Log($"排序前(前5个): [{string.Join(", ", c.Take(5))}] ...");

            c.Sort();

            AssertNonDescending(c, Comparer<string>.Default, "Sort() 默认比较器");
            Log($"Sort() 后(前5个): [{string.Join(", ", c.Take(5))}] ..., 非降序, 通过");
        }

        [TestMethod]
        public void Sort_按ValueA键排序后非降序()
        {
            Log("--- 测试(randomTest2): 按 ValueA(int) 键排序 (固定种子) ---");
            var rng = new System.Random(20260927);
            var items = Enumerable.Range(0, 20)
                .Select(_ => new TestClass
                {
                    ValueA = rng.Next(100),
                    ValueB = RandomStringHelper.GetRandomLowerEnglishString(30, rng),
                })
                .ToList();

            var c = new ObservableCollection<TestClass>(items);
            Log($"排序前 ValueA(前5个): [{string.Join(", ", c.Take(5).Select(i => i.ValueA))}] ...");

            c.Sort(i => i.ValueA);

            AssertNonDescending(c.Select(i => i.ValueA), Comparer<int>.Default, "Sort(i => i.ValueA)");
            Log($"Sort(ValueA) 后 ValueA(前5个): [{string.Join(", ", c.Take(5).Select(i => i.ValueA))}] ..., 非降序, 通过");
        }

        [TestMethod]
        public void Sort_按ValueB键排序后非降序()
        {
            Log("--- 测试(randomTest2): 按 ValueB(string) 键排序 (固定种子) ---");
            var rng = new System.Random(20260927);
            var items = Enumerable.Range(0, 20)
                .Select(_ => new TestClass
                {
                    ValueA = rng.Next(100),
                    ValueB = RandomStringHelper.GetRandomLowerEnglishString(30, rng),
                })
                .ToList();

            var c = new ObservableCollection<TestClass>(items);
            Log($"排序前 ValueB(前5个): [{string.Join(", ", c.Take(5).Select(i => i.ValueB))}] ...");

            c.Sort(i => i.ValueB);

            AssertNonDescending(c.Select(i => i.ValueB), Comparer<string>.Default, "Sort(i => i.ValueB)");
            Log($"Sort(ValueB) 后 ValueB(前5个): [{string.Join(", ", c.Take(5).Select(i => i.ValueB))}] ..., 非降序, 通过");
        }

        #endregion
    }
}
