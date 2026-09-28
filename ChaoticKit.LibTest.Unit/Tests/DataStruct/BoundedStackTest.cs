using ChaoticKit.Data.Structure.Linear;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.BoundedStack001
    /// 有限容量栈 BoundedStack 的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class BoundedStackTest : UnitTestBase
    {
        /// <summary>
        /// 校验栈内容: 数量 + 从栈底到栈顶逐位比较
        /// </summary>
        private void VerifyStackContent(BoundedStack<int> stack, int[] expected, string testName)
        {
            Assert.AreEqual(expected.Length, stack.Count, $"[{testName}] 数量不匹配。预期: {expected.Length}, 实际: {stack.Count}");

            int index = 0;
            foreach (var actualItem in stack)
            {
                Assert.AreEqual(expected[index], actualItem, $"[{testName}] 数据不匹配。位置: {index}, 预期: {expected[index]}, 实际: {actualItem}");
                index++;
            }

            Log($"[{testName}] 通过。内容: [{string.Join(", ", expected)}], Count={stack.Count}, Capacity={stack.Capacity}, IsFull={stack.IsFull}");
        }

        [TestMethod]
        public void BasicPushAndIteration()
        {
            Log("--- 测试: 基础入栈与遍历 ---");
            var stack = new BoundedStack<int>(3);

            stack.Push(10);
            Log("Push(10)");
            stack.Push(20);
            Log("Push(20)");
            stack.Push(30);
            Log("Push(30)");

            VerifyStackContent(stack, new[] { 10, 20, 30 }, "基础入栈");
            Assert.IsTrue(stack.IsFull, "填满 3 个后应 IsFull=true");
            Assert.AreEqual(3, stack.Capacity, "容量应为 3");
        }

        [TestMethod]
        public void OverflowBehavior()
        {
            Log("--- 测试: 溢出覆盖行为 ---");
            var stack = new BoundedStack<int>(3);

            stack.Push(1);
            Log("Push(1)");
            stack.Push(2);
            Log("Push(2)");
            stack.Push(3);
            Log("Push(3)");

            stack.Push(4);
            Log("Push(4) 触发溢出, 应挤掉栈底 1");
            VerifyStackContent(stack, new[] { 2, 3, 4 }, "溢出Push(4)");

            stack.Push(5);
            Log("Push(5) 触发溢出, 应挤掉栈底 2");
            VerifyStackContent(stack, new[] { 3, 4, 5 }, "溢出Push(5)");
        }

        [TestMethod]
        public void PopAndPeek()
        {
            Log("--- 测试: 出栈与查看 ---");
            var stack = new BoundedStack<int>(3);
            stack.Push(100);
            Log("Push(100)");
            stack.Push(200);
            Log("Push(200)");

            int top = stack.Peek();
            Assert.AreEqual(200, top, "[Peek测试] 失败。预期 200, 实际 " + top);
            Log("[Peek测试] 通过");

            int popped = stack.Pop();
            Assert.AreEqual(200, popped, "[Pop测试] 返回值错误。预期 200, 实际 " + popped);
            Log("[Pop测试] 通过");

            VerifyStackContent(stack, new[] { 100 }, "Pop后状态");
        }

        [TestMethod]
        public void EmptyStackBehavior()
        {
            Log("--- 测试: 空栈行为 ---");
            var stack = new BoundedStack<int>(2);

            Assert.ThrowsException<InvalidOperationException>(() => stack.Pop(), "[空栈Pop] 失败。预期抛出 InvalidOperationException，但未抛出。");
            Log("[空栈Pop] 通过。正确抛出 InvalidOperationException。");

            Assert.ThrowsException<InvalidOperationException>(() => stack.Peek(), "[空栈Peek] 失败。预期抛出 InvalidOperationException，但未抛出。");
            Log("[空栈Peek] 通过。正确抛出 InvalidOperationException。");

            Assert.IsFalse(stack.TryPop(out _), "[空栈TryPop] 失败。空栈应返回 false。");
            Assert.IsFalse(stack.TryPeek(out _), "[空栈TryPeek] 失败。空栈应返回 false。");
            Log("[空栈TryPop/TryPeek] 通过。均返回 false。");

            VerifyStackContent(stack, Array.Empty<int>(), "空栈遍历");
        }

        [TestMethod]
        public void SingleCapacity()
        {
            Log("--- 测试: 容量为1的边界情况 ---");
            var stack = new BoundedStack<int>(1);

            stack.Push(99);
            Log("Push(99)");
            VerifyStackContent(stack, new[] { 99 }, "容量1入栈");

            stack.Push(88);
            Log("Push(88) 触发覆盖");
            VerifyStackContent(stack, new[] { 88 }, "容量1覆盖");

            stack.Pop();
            Log("Pop()");
            VerifyStackContent(stack, Array.Empty<int>(), "容量1清空");
        }

        [TestMethod]
        public void ClearAndContains()
        {
            Log("--- 测试: Clear 与 Contains ---");
            var stack = new BoundedStack<int>(3);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            Assert.IsTrue(stack.Contains(2), "Contains(2) 应返回 true");
            Assert.IsFalse(stack.Contains(9), "Contains(9) 应返回 false");
            Log("[Contains] 通过");

            stack.Clear();
            Assert.AreEqual(0, stack.Count, "Clear 后 Count 应为 0");
            Assert.IsFalse(stack.IsFull, "Clear 后 IsFull 应为 false");
            Log("[Clear] 通过。Count=0, IsFull=false");
        }
    }
}
