using ChaoticKit.Data.Structure.Linear;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.CycleContainer001
    /// 循环容器 CycleContainer 的添加(环绕覆盖)、Peek、包装共享数组、清空 行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// - 源 test1 的逐次 Add 打印改为逐步断言, 断言遍历顺序 (遍历从距当前索引最远端到当前位置);
    /// - 源 test2 用 try/catch 吞掉负数容量异常, 此处改为确定性异常断言:
    ///   库源码 CycleContainer.cs 构造函数内 ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0) 直接抛出,
    ///   无静态构造函数包装, 断言外层即为 ArgumentOutOfRangeException;
    /// - 源 test4 的 Wrapper 与底层数组共享存储, 容器 Add/Clear 与原数组改值互相可见, 断言覆盖该共享语义;
    /// - 补充一个值类型默认值场景 (T 无约束时 default(T)=0) 覆盖值类型填充。
    /// </remarks>
    [TestClass]
    public sealed class CycleContainerTest : UnitTestBase
    {
        /// <summary>
        /// 校验容器遍历内容 (FarToCurrent 顺序, 即从距当前索引最远端到当前位置)
        /// </summary>
        private void VerifyContent(CycleContainer<string> container, string?[] expected, string testName)
        {
            string?[] actual = container.ToArray();
            CollectionAssert.AreEqual(expected, actual,
                $"[{testName}] 遍历内容不匹配。预期: [{string.Join(", ", expected)}], 实际: [{string.Join(", ", actual)}]");
            Log($"[{testName}] 通过。Capacity={container.Capacity}, 遍历内容: [{string.Join(", ", actual)}]");
        }

        [TestMethod]
        public void Add_依次填充与环绕覆盖()
        {
            Log("--- 测试: 固定容量 8 依次 Add, 填满后环绕覆盖, 最后 Peek 最近三项 ---");
            var c1 = new CycleContainer<string>(8, "+");
            Assert.AreEqual(8, c1.Capacity, "容量应为 8");
            VerifyContent(c1, ["+", "+", "+", "+", "+", "+", "+", "+"], "初始(全为默认值 +)");

            c1.Add("1");
            VerifyContent(c1, ["+", "+", "+", "+", "+", "+", "+", "1"], "Add(\"1\")");
            c1.Add("2");
            VerifyContent(c1, ["+", "+", "+", "+", "+", "+", "1", "2"], "Add(\"2\")");
            c1.Add("3");
            VerifyContent(c1, ["+", "+", "+", "+", "+", "1", "2", "3"], "Add(\"3\")");
            c1.Add("4");
            VerifyContent(c1, ["+", "+", "+", "+", "1", "2", "3", "4"], "Add(\"4\")");
            c1.Add("5");
            VerifyContent(c1, ["+", "+", "+", "1", "2", "3", "4", "5"], "Add(\"5\")");
            c1.Add("6");
            VerifyContent(c1, ["+", "+", "1", "2", "3", "4", "5", "6"], "Add(\"6\")");
            c1.Add("7");
            VerifyContent(c1, ["+", "1", "2", "3", "4", "5", "6", "7"], "Add(\"7\")");
            c1.Add("8");
            VerifyContent(c1, ["1", "2", "3", "4", "5", "6", "7", "8"], "Add(\"8\") 填满");
            c1.Add("9");
            VerifyContent(c1, ["2", "3", "4", "5", "6", "7", "8", "9"], "Add(\"9\") 环绕覆盖位置1");
            c1.Add("10");
            VerifyContent(c1, ["3", "4", "5", "6", "7", "8", "9", "10"], "Add(\"10\") 环绕覆盖位置2");

            string[] peek3 = c1.Peek(3).ToArray();
            CollectionAssert.AreEqual(new[] { "8", "9", "10" }, peek3, "Peek(3) 应返回距当前索引最近的 3 项");
            Log($"[Peek(3)] 通过。结果: [{string.Join(", ", peek3)}]");
        }

        [TestMethod]
        public void Null默认值填充与负数容量抛异常()
        {
            Log("--- 测试: 默认值 null 填充 (可空引用类型) 与 负数容量抛异常 ---");

            var c1 = new CycleContainer<string?>(8, null);
            Assert.AreEqual(8, c1.Capacity, "容量应为 8");
            string?[] arr = c1.ToArray();
            Assert.AreEqual(8, arr.Length, "遍历长度应为 8");
            Assert.IsTrue(arr.All(x => x == null), "默认值 null 填充后全部元素应为 null");
            Log($"[null 默认值] 通过。Capacity=8, 遍历出 {arr.Length} 项, 全部为 null");

            // 负数容量: 库源码 CycleContainer.cs 构造函数调用
            // ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0) 直接抛出, 无静态构造函数包装, 断言外层即可。
            ArgumentOutOfRangeException ex = Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new CycleContainer<string>(-8, "+"),
                "负数容量应抛 ArgumentOutOfRangeException, 实际未抛出");
            Log($"[负数容量] 通过。抛出 {ex.GetType().Name}: {ex.Message}");
        }

        [TestMethod]
        public void 集合构造_初始值为传入序列()
        {
            Log("--- 测试: 从集合构造, 容量为序列长度, 索引定位于末项 ---");
            var c1 = new CycleContainer<string>(new[] { "1", "2", "3" });
            Assert.AreEqual(3, c1.Capacity, "容量应为 3");
            VerifyContent(c1, ["1", "2", "3"], "集合构造");
        }

        [TestMethod]
        public void Wrapper_包装共享底层数组与Clear()
        {
            Log("--- 测试: Wrapper 包装数组共享底层存储, Add/Clear/外部改值互相可见 ---");
            string[] arr = ["1", "2", "3"];
            var c1 = CycleContainer<string>.Wrapper(arr);

            VerifyContent(c1, ["1", "2", "3"], "包装初始");
            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, arr, "包装后原数组不变");

            c1.Add("4");
            VerifyContent(c1, ["2", "3", "4"], "Add(\"4\") 后容器");
            CollectionAssert.AreEqual(new[] { "4", "2", "3" }, arr, "Add(\"4\") 应同步写入原数组 (共享存储)");

            arr[0] = "5";
            VerifyContent(c1, ["2", "3", "5"], "arr[0]=\"5\" 后容器");
            CollectionAssert.AreEqual(new[] { "5", "2", "3" }, arr, "原数组改值");

            c1.Clear();
            VerifyContent(c1, [null, null, null], "Clear() 后容器 (默认值 null)");
            CollectionAssert.AreEqual(new string?[] { null, null, null }, arr, "Clear() 应把原数组同步清为 null");

            c1.Clear("芜湖");
            VerifyContent(c1, ["芜湖", "芜湖", "芜湖"], "Clear(\"芜湖\") 后容器");
            CollectionAssert.AreEqual(new[] { "芜湖", "芜湖", "芜湖" }, arr, "Clear(\"芜湖\") 应同步写入原数组");
        }

        [TestMethod]
        public void 值类型默认值零填充()
        {
            Log("--- 补充: 值类型 T 无约束时 default(T)=0, Add 环绕覆盖 ---");
            var c1 = new CycleContainer<int>(3);
            int[] init = c1.ToArray();
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, init, "值类型默认填充应为 0");
            Log($"[初始] 通过。内容: [{string.Join(", ", init)}]");

            c1.Add(1);
            c1.Add(2);
            int[] after = c1.ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, after, "Add(1)/Add(2) 后遍历内容不匹配");
            Log($"[Add(1)/Add(2)] 通过。内容: [{string.Join(", ", after)}]");
        }
    }
}
