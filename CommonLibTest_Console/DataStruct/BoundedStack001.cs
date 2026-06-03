using Common_Util.Data.Structure.Linear;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class BoundedStack001() : TestBase("简单测试 BoundedStack 结构")
    {
        ILevelLogger logger = LevelLoggerHelper.Empty();

        protected override void RunImpl()
        {
            logger = GetLevelLogger("测试");

            RunTestMark();
        }

        private bool VerifyStackContent(BoundedStack<int> stack, int[] expected, string testName)
        {
            // 1. 检查数量
            if (stack.Count != expected.Length)
            {
                logger.Error($"[{testName}] 失败! 数量不匹配。预期: {expected.Length}, 实际: {stack.Count}");
                return false;
            }

            // 2. 逐位比较 (利用 IEnumerable 从底到顶遍历的特性)
            int index = 0;
            foreach (var actualItem in stack)
            {
                if (actualItem != expected[index])
                {
                    logger.Error($"[{testName}] 失败! 数据不匹配。位置: {index}, 预期: {expected[index]}, 实际: {actualItem}");
                    return false;
                }
                index++;
            }

            logger.Info($"[{testName}] 通过。内容: [{string.Join(", ", expected)}]");
            return true;
        }

        [TestMethod("1. 基础测试：入栈与遍历")]
        private void TestBasicPushAndIteration()
        {
            logger.Info("--- 测试: 基础入栈与遍历 ---");
            var stack = new BoundedStack<int>(3);

            stack.Push(10);
            logger.Trace("Push(10)");
            stack.Push(20);
            logger.Trace("Push(20)");
            stack.Push(30);
            logger.Trace("Push(30)");

            // 预期: [10, 20, 30] (底 -> 顶)
            VerifyStackContent(stack, new[] { 10, 20, 30 }, "基础入栈");
        }

        [TestMethod("2. 核心测试：溢出行为（环形覆盖）")]
        private void TestOverflowBehavior()
        {
            logger.Info("--- 测试: 溢出覆盖行为 ---");
            var stack = new BoundedStack<int>(3); // 容量3

            // 填满
            stack.Push(1);
            logger.Trace("Push(1)");
            stack.Push(2);
            logger.Trace("Push(2)");
            stack.Push(3);
            logger.Trace("Push(3)");

            // 触发溢出
            stack.Push(4); // 应该挤掉 1
                           // 预期: [2, 3, 4]
            logger.Trace("Push(4)");
            VerifyStackContent(stack, new[] { 2, 3, 4 }, "溢出Push(4)");

            stack.Push(5); // 应该挤掉 2
                           // 预期: [3, 4, 5]
            logger.Trace("Push(5)");
            VerifyStackContent(stack, new[] { 3, 4, 5 }, "溢出Push(5)");
        }


        [TestMethod("3. 出栈与查看")]
        private void TestPopAndPeek()
        {
            logger.Info("--- 测试: 出栈与查看 ---");
            var stack = new BoundedStack<int>(3);
            stack.Push(100);
            logger.Trace("Push(100)");
            stack.Push(200);
            logger.Trace("Push(200)");

            // Peek 测试
            int top = stack.Peek();
            logger.Trace("Peek()");
            if (top == 200) logger.Info("[Peek测试] 通过。");
            else logger.Error($"[Peek测试] 失败。预期 200, 实际 {top}");

            // Pop 测试
            int popped = stack.Pop();
            logger.Trace("Pop()");
            if (popped == 200) logger.Info("[Pop测试] 返回值正确。");
            else logger.Error($"[Pop测试] 返回值错误。预期 200, 实际 {popped}");

            // Pop 后内部状态验证
            // 预期: [100]
            VerifyStackContent(stack, new[] { 100 }, "Pop后状态");
        }

        [TestMethod("4. 空栈行为")]
        private void TestEmptyStackBehavior()
        {
            logger.Info("--- 测试: 空栈行为 ---");
            var stack = new BoundedStack<int>(2);

            // 空栈 Pop
            try
            {
                stack.Pop();
                logger.Trace("Pop()");
                logger.Error("[空栈Pop] 失败。预期抛出异常，但未抛出。");
            }
            catch (InvalidOperationException)
            {
                logger.Info("[空栈Pop] 通过。正确抛出 InvalidOperationException。");
            }
            catch (Exception ex)
            {
                logger.Error($"[空栈Pop] 失败。抛出了意外的异常类型: {ex.GetType().Name}");
            }

            // 空栈遍历
            VerifyStackContent(stack, new int[0], "空栈遍历");
        }

        [TestMethod("5. 边界情况：容量为1")]
        private void TestSingleCapacity()
        {
            logger.Info("--- 测试: 容量为1的边界情况 ---");
            var stack = new BoundedStack<int>(1);

            stack.Push(99);
            logger.Trace("Push(99)");
            VerifyStackContent(stack, new[] { 99 }, "容量1入栈");

            stack.Push(88); // 覆盖
            logger.Trace("Push(88)");
            VerifyStackContent(stack, new[] { 88 }, "容量1覆盖");

            stack.Pop();
            logger.Trace("Pop(0)");
            VerifyStackContent(stack, new int[0], "容量1清空");
        }
    }
}
