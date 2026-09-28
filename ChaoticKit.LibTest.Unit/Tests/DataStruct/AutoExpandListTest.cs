using ChaoticKit.Data.Structure.Linear;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.AutoExpandList001
    /// 自动扩展列表 AutoExpandList&lt;T&gt; 的索引器/默认值/自动扩展/SetIndexes/ContainsIndex/IList 操作/遍历行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class AutoExpandListTest : UnitTestBase
    {
        /// <summary>
        /// 1. 基础索引器测试: 任意非负索引写入后可读回
        /// </summary>
        [TestMethod]
        public void BasicIndexer()
        {
            Log("--- 测试: 基础索引器 ---");
            var list = new AutoExpandList<int>(-1);

            list[0] = 10;
            Log("list[0] = 10");
            list[2] = 30;
            Log("list[2] = 30");

            Assert.AreEqual(10, list[0], "[基础索引器] 失败。list[0] 预期 10, 实际 " + list[0]);
            Assert.AreEqual(30, list[2], "[基础索引器] 失败。list[2] 预期 30, 实际 " + list[2]);
            Assert.AreEqual(3, list.Count, "[基础索引器] 失败。写入索引2后 Count 应为 3, 实际 " + list.Count);
            Log($"[基础索引器] 通过。list[0]=10, list[2]=30, Count={list.Count}");
        }

        /// <summary>
        /// 2. 默认值测试: 未设置的索引返回构造时指定的默认值, 且不扩展列表
        /// </summary>
        [TestMethod]
        public void DefaultValue()
        {
            Log("--- 测试: 默认值 ---");
            var list = new AutoExpandList<string>("DEFAULT");

            var value = list[100];
            Assert.AreEqual("DEFAULT", value, "[默认值] 失败。预期 'DEFAULT', 实际 '" + value + "'");
            Assert.AreEqual(0, list.Count, "[默认值] 失败。读取未设置索引不应扩展列表, Count 应为 0, 实际 " + list.Count);
            Assert.IsFalse(list.ContainsIndex(100), "[默认值] 失败。读取未设置索引不应记为已设置");
            Log("[默认值] 通过。未设置的索引返回默认值 'DEFAULT', Count=0, ContainsIndex(100)=false");
        }

        /// <summary>
        /// 3. 自动扩展测试: 写入越界索引自动扩展 Count, SetIndexes 记录实际写入的索引
        /// </summary>
        [TestMethod]
        public void AutoExpand()
        {
            Log("--- 测试: 自动扩展 ---");
            var list = new AutoExpandList<int>(0);

            list[5] = 50;
            Log("list[5] = 50");

            Assert.AreEqual(6, list.Count, "[自动扩展] 失败。预期 Count=6, 实际 Count=" + list.Count);
            Log($"[自动扩展] 通过。Count={list.Count}");

            var setIndexes = list.SetIndexes.ToList();
            CollectionAssert.AreEqual(new[] { 5 }, setIndexes, "[已设置索引] 失败。SetIndexes 应只有 5, 实际 [" + string.Join(", ", setIndexes) + "]");
            Log($"[已设置索引] 通过。SetIndexes=[{string.Join(", ", setIndexes)}]");
        }

        /// <summary>
        /// 4. ContainsIndex 测试: 已写入的索引返回 true, 未写入/负索引返回 false
        /// </summary>
        [DataTestMethod]
        [DataRow(0, true)]
        [DataRow(5, true)]
        [DataRow(3, false)]
        [DataRow(-1, false)]
        public void ContainsIndex(int index, bool expected)
        {
            Log($"--- 测试: ContainsIndex({index}) ---");
            var list = new AutoExpandList<int>(-1);
            list[0] = 100;
            list[5] = 500;

            var actual = list.ContainsIndex(index);
            Assert.AreEqual(expected, actual, $"[ContainsIndex({index})] 失败。预期={expected}, 实际={actual}");
            Log($"[ContainsIndex({index})] 通过。结果={actual}");
        }

        /// <summary>
        /// 5. IList&lt;T&gt; 操作测试: Add/Insert/RemoveAt/IndexOf/Contains/Clear
        /// </summary>
        [TestMethod]
        public void IListOperations()
        {
            Log("--- 测试: IList<T> 操作 ---");
            var list = new AutoExpandList<int>(0);

            list.Add(1);
            Log($"[Add] 列表内容: [{string.Join(", ", list)}]");
            list.Add(2);
            Log($"[Add] 列表内容: [{string.Join(", ", list)}]");
            list.Add(3);
            Log($"[Add] 列表内容: [{string.Join(", ", list)}]");

            Assert.AreEqual(3, list.Count, "[Add] 失败。Count 预期 3, 实际 " + list.Count);
            Assert.AreEqual(1, list[0], "[Add] 失败。list[0] 应为 1, 实际 " + list[0]);
            Assert.AreEqual(2, list[1], "[Add] 失败。list[1] 应为 2, 实际 " + list[1]);
            Assert.AreEqual(3, list[2], "[Add] 失败。list[2] 应为 3, 实际 " + list[2]);
            Log("[Add] 通过。添加了 1, 2, 3");

            list.Insert(1, 10);
            Log($"[Insert(1, 10)] 列表内容: [{string.Join(", ", list)}]");
            Assert.AreEqual(4, list.Count, "[Insert] 失败。插入后 Count 应为 4, 实际 " + list.Count);
            Assert.AreEqual(10, list[1], "[Insert] 失败。list[1] 应为 10, 实际 " + list[1]);
            Assert.AreEqual(2, list[2], "[Insert] 失败。list[2] 应为 2, 实际 " + list[2]);
            Log("[Insert] 通过。在索引1处插入10");

            list.RemoveAt(0);
            Log($"[RemoveAt(0)] 列表内容: [{string.Join(", ", list)}]");
            Assert.AreEqual(3, list.Count, "[RemoveAt] 失败。移除后 Count 应为 3, 实际 " + list.Count);
            Assert.AreEqual(10, list[0], "[RemoveAt] 失败。list[0] 应为 10, 实际 " + list[0]);
            Log("[RemoveAt] 通过。移除索引0");

            int index = list.IndexOf(2);
            Log($"[IndexOf(2)] 结果: {index}, 列表内容: [{string.Join(", ", list)}]");
            Assert.AreEqual(1, index, "[IndexOf] 失败。预期=1, 实际=" + index);
            Log("[IndexOf] 通过。IndexOf(2)=1");

            var contains = list.Contains(10);
            Log($"[Contains(10)] 结果: {contains}, 列表内容: [{string.Join(", ", list)}]");
            Assert.IsTrue(contains, "[Contains] 失败。应包含元素10");
            Log("[Contains] 通过。包含元素10");

            list.Clear();
            Log($"[Clear] 列表内容: [{string.Join(", ", list)}]");
            Assert.AreEqual(0, list.Count, "[Clear] 失败。清空后 Count 应为 0, 实际 " + list.Count);
            Assert.IsFalse(list.ContainsIndex(0), "[Clear] 失败。清空后不应再包含任何索引");
            Log("[Clear] 通过。清空后Count=0");
        }

        /// <summary>
        /// 6. 遍历测试: 未设置索引在遍历中输出默认值
        /// </summary>
        [TestMethod]
        public void Enumeration()
        {
            Log("--- 测试: 遍历 ---");
            var list = new AutoExpandList<string>("空");
            list[0] = "A";
            list[2] = "C";

            var items = list.ToList();
            var expected = new[] { "A", "空", "C" };
            CollectionAssert.AreEqual(expected, items, $"[遍历] 失败。预期=[{string.Join(", ", expected)}], 实际=[{string.Join(", ", items)}]");
            Log($"[遍历] 通过。结果=[{string.Join(", ", items)}]");
        }
    }
}
