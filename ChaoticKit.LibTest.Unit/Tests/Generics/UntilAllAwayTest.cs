using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.Generics
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Generics.IEnumerableExtension001
    /// ChaoticKit.Extensions 的 (t1, t2).UntilAllAway / UntilAllAwayWithIndex 双序列对齐扩展验证:
    /// 两序列同时遍历直到均耗尽, 一方先耗尽时取值 default(引用/可空类型为 null),
    /// WithIndex 变体在耗尽方的下标为 -1
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class UntilAllAwayTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow("string含null, 左长右短(3 vs 2)")]
        [DataRow("int, 左短右长(3 vs 5)")]
        [DataRow("int?, 左短右长(3 vs 5)")]
        public void UntilAllAway_双序列对齐直到两者均耗尽(string scenario)
        {
            Log($"--- 测试: UntilAllAway 用例 [{scenario}] ---");
            switch (scenario)
            {
                case "string含null, 左长右短(3 vs 2)":
                    RunUntilAllAway<string>(
                        ["111", null, "222"], ["333", null],
                        [("111", "333"), (null, null), ("222", null)],
                        scenario);
                    break;
                case "int, 左短右长(3 vs 5)":
                    RunUntilAllAway<int>(
                        [123, 32, 1], [1, 2, 3, 4, 5],
                        new (int, int)[] { (123, 1), (32, 2), (1, 3), (0, 4), (0, 5) },
                        scenario);
                    break;
                case "int?, 左短右长(3 vs 5)":
                    RunUntilAllAway<int?>(
                        [123, 32, 1], [1, 2, 3, 4, 5],
                        [(123, 1), (32, 2), (1, 3), (null, 4), (null, 5)],
                        scenario);
                    break;
                default:
                    Assert.Fail($"未知用例: {scenario}");
                    break;
            }
        }

        [DataTestMethod]
        [DataRow("string含null, 左长右短(3 vs 2)")]
        [DataRow("int, 左短右长(3 vs 5)")]
        [DataRow("int?, 左短右长(3 vs 5)")]
        public void UntilAllAwayWithIndex_耗尽方下标为负一(string scenario)
        {
            Log($"--- 测试: UntilAllAwayWithIndex 用例 [{scenario}] ---");
            switch (scenario)
            {
                case "string含null, 左长右短(3 vs 2)":
                    RunUntilAllAwayWithIndex<string>(
                        ["111", null, "222"], ["333", null],
                        [((0, "111"), (0, "333")), ((1, null), (1, null)), ((2, "222"), (-1, null))],
                        scenario);
                    break;
                case "int, 左短右长(3 vs 5)":
                    RunUntilAllAwayWithIndex<int>(
                        [123, 32, 1], [1, 2, 3, 4, 5],
                        new ((int, int), (int, int))[] {
                            ((0, 123), (0, 1)), ((1, 32), (1, 2)), ((2, 1), (2, 3)),
                            ((-1, 0), (3, 4)), ((-1, 0), (4, 5)) },
                        scenario);
                    break;
                case "int?, 左短右长(3 vs 5)":
                    RunUntilAllAwayWithIndex<int?>(
                        [123, 32, 1], [1, 2, 3, 4, 5],
                        [((0, 123), (0, 1)), ((1, 32), (1, 2)), ((2, 1), (2, 3)),
                         ((-1, null), (3, 4)), ((-1, null), (4, 5))],
                        scenario);
                    break;
                default:
                    Assert.Fail($"未知用例: {scenario}");
                    break;
            }
        }

        /// <summary>
        /// 执行一个 UntilAllAway 用例: 遍历 (arr1, arr2).UntilAllAway(),
        /// 断言迭代次数 = max(两序列长度), 且每项的 (t1, t2) 与期望逐项一致
        /// </summary>
        private void RunUntilAllAway<T>(T[] arr1, T[] arr2, (T? t1, T? t2)[] expected, string label)
        {
            int index = 0;
            var actual = new List<(T? t1, T? t2)>();
            foreach ((T? t1, T? t2) in (arr1, arr2).UntilAllAway())
            {
                actual.Add((t1, t2));
                Log($"[{label}] {index}  =>  t1: {t1?.ToString() ?? "<null>"} ::: t2: {t2?.ToString() ?? "<null>"}");
                index++;
            }

            Assert.AreEqual(expected.Length, actual.Count, $"[{label}] 迭代次数应为 max(两序列长度)={expected.Length}, 实际 {actual.Count}");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].t1, actual[i].t1, $"[{label}] 第 {i} 项 t1 不匹配");
                Assert.AreEqual(expected[i].t2, actual[i].t2, $"[{label}] 第 {i} 项 t2 不匹配");
            }
        }

        /// <summary>
        /// 执行一个 UntilAllAwayWithIndex 用例: 遍历 (arr1, arr2).UntilAllAwayWithIndex(),
        /// 断言迭代次数 = max(两序列长度), 每项下标与值逐项一致, 已耗尽一方的下标为 -1
        /// </summary>
        private void RunUntilAllAwayWithIndex<T>(
            T[] arr1, T[] arr2,
            ((int i1, T? t1), (int i2, T? t2))[] expected,
            string label)
        {
            int index = 0;
            var actual = new List<((int i1, T? t1), (int i2, T? t2))>();
            foreach (var ((i1, t1), (i2, t2)) in (arr1, arr2).UntilAllAwayWithIndex())
            {
                actual.Add(((i1, t1), (i2, t2)));
                Log($"[{label}] {index}  =>  t1: [{i1}]{t1?.ToString() ?? "<null>"} ::: t2: [{i2}]{t2?.ToString() ?? "<null>"}");
                index++;
            }

            Assert.AreEqual(expected.Length, actual.Count, $"[{label}] 迭代次数应为 max(两序列长度)={expected.Length}, 实际 {actual.Count}");
            for (int i = 0; i < expected.Length; i++)
            {
                var (e1, e2) = expected[i];
                var (a1, a2) = actual[i];
                Assert.AreEqual(e1.i1, a1.i1, $"[{label}] 第 {i} 项 t1 下标不匹配 (期望 {e1.i1}, 实际 {a1.i1})");
                Assert.AreEqual(e1.t1, a1.t1, $"[{label}] 第 {i} 项 t1 值不匹配");
                Assert.AreEqual(e2.i2, a2.i2, $"[{label}] 第 {i} 项 t2 下标不匹配 (期望 {e2.i2}, 实际 {a2.i2})");
                Assert.AreEqual(e2.t2, a2.t2, $"[{label}] 第 {i} 项 t2 值不匹配");
            }
        }
    }
}
