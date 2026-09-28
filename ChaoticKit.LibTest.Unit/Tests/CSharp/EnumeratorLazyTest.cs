using System.Collections.Generic;
using System.Linq;

namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Enumerator001
    /// 验证 yield 迭代器的惰性求值语义:
    /// - 完整遍历产出全部元素
    /// - break 提前终止时, 剩余元素不被产出 (惰性)
    /// - Any() 只消费首元素
    /// - 每次 GetEnumerator() 都是全新枚举器, Any() 消费后 foreach 仍能从头完整遍历
    /// 被消费次数通过迭代器内计数器 _producedCount 统计 (每次 yield 前 +1)。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class EnumeratorLazyTest : UnitTestBase
    {
        /// <summary>
        /// 迭代器实际产出的元素个数 (惰性求值探针)
        /// </summary>
        private int _producedCount;

        /// <summary>
        /// 惰性迭代器: 每次 yield 前记录一次产出, 后续语句仅在 MoveNext 时才执行
        /// </summary>
        IEnumerable<int> GetTestEnumerable()
        {
            Log("getTestEnumerable start");
            Log("getTestEnumerable 1");
            _producedCount++;
            yield return 1;
            Log("getTestEnumerable 2");
            _producedCount++;
            yield return 2;
            Log("getTestEnumerable 3");
            _producedCount++;
            yield return 3;
            Log("getTestEnumerable 4");
            _producedCount++;
            yield return 4;
            Log("getTestEnumerable end");
        }

        [TestMethod]
        public void 完整遍历_产出全部元素()
        {
            Log("--- 测试: 完整遍历 (test1) ---");
            _producedCount = 0;
            var seen = new List<int>();
            foreach (int i in GetTestEnumerable())
            {
                Log($"got: {i}");
                seen.Add(i);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, seen, "完整遍历应依次得到 1,2,3,4");
            Assert.AreEqual(4, _producedCount, "完整遍历应产出全部 4 个元素");
            Log($"完整遍历得到 {string.Join(",", seen)}, 产出 {_producedCount} 个元素, 通过");
        }

        [DataTestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 2)]
        [DataRow(3, 3)]
        [DataRow(4, 4)]
        public void Break提前终止_只产出到break位置(int breakAt, int expectedProduced)
        {
            Log($"--- 测试: break 提前终止 (breakAt={breakAt}) ---");
            _producedCount = 0;
            var seen = new List<int>();
            foreach (int i in GetTestEnumerable())
            {
                Log($"got: {i}");
                seen.Add(i);
                if (i == breakAt)
                {
                    Log("break");
                    break;
                }
            }

            CollectionAssert.AreEqual(Enumerable.Range(1, expectedProduced).ToArray(), seen,
                $"break 前应只得到 1..{expectedProduced}");
            Assert.AreEqual(expectedProduced, _producedCount,
                $"break 提前终止时只应产出 {expectedProduced} 个元素 (yield 惰性: 剩余元素未被枚举)");
            Log($"break 提前终止, 得到 {string.Join(",", seen)}, 产出 {_producedCount} 个元素, 通过");
        }

        [TestMethod]
        public void Any_只消费首元素()
        {
            Log("--- 测试: Any() 只消费首元素 (test3) ---");
            _producedCount = 0;
            bool any = GetTestEnumerable().Any();

            Assert.IsTrue(any, "Any() 应返回 true");
            Assert.AreEqual(1, _producedCount, "Any() 只应消费首元素 (yield 惰性: 只产出 1, 2/3/4 与 end 未执行)");
            Log($"Any() = {any}, 只产出 {_producedCount} 个元素, 通过");
        }

        [TestMethod]
        public void Any消费后_foreach仍从头完整遍历()
        {
            Log("--- 测试: Any() 后再 foreach (test4) ---");
            _producedCount = 0;
            IEnumerable<int> test = GetTestEnumerable();

            bool any = test.Any();
            Log($"Any() = {any}, 已产出 {_producedCount} 个元素");

            var seen = new List<int>();
            foreach (int i in test)
            {
                Log($"got: {i}");
                seen.Add(i);
            }

            Assert.IsTrue(any, "Any() 应返回 true");
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, seen,
                "Any() 消费后 foreach 应仍从头完整遍历 (每次 GetEnumerator() 都是全新枚举器)");
            Assert.AreEqual(5, _producedCount,
                "Any() 产出 1 个 + foreach 重新枚举产出 4 个 = 共 5 次产出");
            Log($"Any() 消费 {1} 个 + foreach 重新枚举 {4} 个, 总产出 {_producedCount} 个元素, 通过");
        }
    }
}
