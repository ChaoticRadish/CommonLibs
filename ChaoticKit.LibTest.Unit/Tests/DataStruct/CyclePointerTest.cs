using ChaoticKit.Data.Structure.Linear;
using ChaoticKit.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.CyclePointer001
    /// CyclePointer&lt;T&gt; 循环移动 / 边界处理 / 自定义泛型运算符的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class CyclePointerTest : UnitTestBase
    {
        /// <summary>
        /// 自定义测试数据类型: 自增/自减每次步长 5, 相等比较按 X 与 Y, 大小比较仅按 X
        /// </summary>
        private struct TestData : IIncrementOperators<TestData>, IDecrementOperators<TestData>, IComparisonOperators<TestData, TestData, bool>
        {
            public int X;
            public int Y;

            public TestData(int i)
            {
                X = i;
                Y = i;
            }

            public override string ToString()
            {
                return $"X: {X} - Y: {Y}";
            }

            public static TestData operator ++(TestData value)
            {
                return new TestData
                {
                    X = value.X + 5,
                    Y = value.Y + 5
                };
            }

            public static TestData operator --(TestData value)
            {
                return new TestData
                {
                    X = value.X - 5,
                    Y = value.Y - 5
                };
            }

            public static bool operator ==(TestData left, TestData right)
            {
                return left.X == right.X && left.Y == right.Y;
            }

            public static bool operator !=(TestData left, TestData right)
            {
                return !(left == right);
            }

            public static bool operator <(TestData left, TestData right)
            {
                return left.X < right.X;
            }

            public static bool operator >(TestData left, TestData right)
            {
                return !(left == right) && !(left < right);
            }

            public static bool operator <=(TestData left, TestData right)
            {
                return (left == right) || (left < right);
            }

            public static bool operator >=(TestData left, TestData right)
            {
                return (left == right) || (left > right);
            }

            public readonly override bool Equals(object? obj)
            {
                if (obj is TestData other)
                {
                    return this == other;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return X;
            }
        }

        [DataTestMethod]
        [DataRow(1, new int[] { 5, 6, 7, 8, 9, 5, 6, 7, 8 })]
        [DataRow(-1, new int[] { 5, 9, 8, 7, 6, 5, 9, 8, 7 })]
        [DataRow(2, new int[] { 5, 7, 9, 6, 8, 5, 7, 9, 6 })]
        [DataRow(-2, new int[] { 5, 8, 6, 9, 7, 5, 8, 6, 9 })]
        public void Move_Int_步进循环序列(int moveAmount, int[] expected)
        {
            Log($"--- 测试: CyclePointer<int>(true, 1, 5, 9) Move({moveAmount}) 的循环移动序列 ---");
            CyclePointer<int> p = new(true, 1, 5, 9);
            Log($"初始化: Current={p.Current}, Min={p.Min}, Max={p.Max}, Length={p.Length}");

            Assert.AreEqual(5, p.Length, "Min=5 到 Max=9 应包含 5 个位置");
            Assert.AreEqual(5, p.Current, "起始值 1 低于 Min=5, 应钳制到 Min");

            var actual = new List<int>();
            foreach (int i in 9.ForUntil())
            {
                actual.Add(p.Current);
                p.Move(moveAmount);
            }

            CollectionAssert.AreEqual(expected, actual,
                $"[Move({moveAmount})] 移动序列不匹配。预期: [{string.Join(", ", expected)}], 实际: [{string.Join(", ", actual)}]");
            Log($"[Move({moveAmount})] 移动序列: [{string.Join(", ", actual)}], 通过");
        }

        [TestMethod]
        public void TestData_自增自减运算符_前后缀语义()
        {
            Log("--- 测试: TestData 自定义 ++/-- 运算符 (每次步长 5) 的前缀/后缀语义 ---");
            TestData test = new TestData(10);
            var actual = new List<int>();

            void Capture(string label, TestData value)
            {
                actual.Add(value.X);
                Log($"{label}: {value}");
            }

            Capture("初始", test);
            Capture("后置++ 返回旧值", test++);
            Capture("后置++ 返回旧值", test++);
            Capture("后置++ 返回旧值", test++);
            Capture("后置-- 返回旧值", test--);
            Capture("后置-- 返回旧值", test--);
            Capture("后置-- 返回旧值", test--);
            Capture("当前值", test);
            Capture("前置++ 返回新值", ++test);
            Capture("前置++ 返回新值", ++test);
            Capture("前置++ 返回新值", ++test);
            Capture("前置-- 返回新值", --test);
            Capture("前置-- 返回新值", --test);
            Capture("前置-- 返回新值", --test);
            Capture("最终值", test);

            int[] expected = { 10, 10, 15, 20, 25, 20, 15, 10, 15, 20, 25, 20, 15, 10, 10 };
            CollectionAssert.AreEqual(expected, actual,
                $"++/-- 前后缀语义序列不匹配。预期: [{string.Join(", ", expected)}], 实际: [{string.Join(", ", actual)}]");
            Log($"[TestData 运算符] 值序列: [{string.Join(", ", actual)}], 通过");
        }

        [TestMethod]
        public void TestData_构造_越界起始值钳制与Length()
        {
            Log("--- 测试: CyclePointer<TestData>(true, 0, 5, 100) 构造: 起始值低于 Min 应钳制, Length 计算 ---");
            CyclePointer<TestData> p = new(true, new TestData(0), new TestData(5), new TestData(100));
            Log($"Length:{p.Length} Current:{p.Current} Min:{p.Min} Max:{p.Max}");

            Assert.AreEqual(20, p.Length, "从 Min=5 到 Max=100 (每次步长 5) 应包含 20 个位置");
            Assert.AreEqual(new TestData(5), p.Current, "起始值 0 低于 Min=5, 应钳制到 Min");
            Assert.AreEqual(new TestData(5), p.Min, "Min 应为 5");
            Assert.AreEqual(new TestData(100), p.Max, "Max 应为 100");
            Log("构造钳制与 Length 计算, 通过");
        }

        [TestMethod]
        public void TestData_构造_无法自增到达Max_抛ArgumentException()
        {
            Log("--- 测试: CyclePointer<TestData>(true, 0, 5, 96) 构造: Max-Min=91 不是步长 5 的倍数, 应抛 ArgumentException ---");
            ArgumentException ex = Assert.ThrowsException<ArgumentException>(
                () => new CyclePointer<TestData>(true, new TestData(0), new TestData(5), new TestData(96)),
                "从 Min=5 无法以自增到达 Max=96 时应抛 ArgumentException");
            Log($"抛 ArgumentException: {ex.Message}, 通过");
        }

        [TestMethod]
        public void TestData_Move_越过最大值_抛InvalidOperationException()
        {
            Log("--- 测试: 起始值 6 不在 5 的等差点上, 移动越过 Max=100 时应抛 InvalidOperationException ---");
            CyclePointer<TestData> p = new(true, new TestData(6), new TestData(5), new TestData(100));

            var actual = new List<int>();
            InvalidOperationException? caught = null;
            foreach (int i in 25.ForUntil())
            {
                actual.Add(p.Current.X);
                Log($"第 {i} 次: Current={p.Current}");
                try
                {
                    p.Move(1);
                }
                catch (InvalidOperationException ex)
                {
                    caught = ex;
                    break;
                }
            }

            Assert.IsNotNull(caught, "越过 Max 时应抛 InvalidOperationException, 但未抛出");
            int[] expected = { 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76, 81, 86, 91, 96, 101 };
            CollectionAssert.AreEqual(expected, actual, "抛异常前输出的移动序列不匹配");
            Log($"抛 InvalidOperationException: {caught?.Message}, 通过");
        }

        [TestMethod]
        public void TestData_Move_有效起点循环25步()
        {
            Log("--- 测试: CyclePointer<TestData>(true, 30, 5, 100) 起始值在范围内, 25 次 Move(1) 的循环序列 ---");
            CyclePointer<TestData> p = new(true, new TestData(30), new TestData(5), new TestData(100));

            var actual = new List<int>();
            foreach (int i in 25.ForUntil())
            {
                actual.Add(p.Current.X);
                p.Move(1);
            }

            int[] expected = { 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
            CollectionAssert.AreEqual(expected, actual,
                $"25 步循环序列不匹配。预期: [{string.Join(", ", expected)}], 实际: [{string.Join(", ", actual)}]");
            Log($"[25 步循环] 移动序列: [{string.Join(", ", actual)}], 通过");
        }
    }
}
