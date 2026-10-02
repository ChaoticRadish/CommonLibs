using ChaoticKit.Data.Structure.Map;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.DefaultValueMap001
    /// 默认值 Map DefaultValueMap 的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class DefaultValueMapTest : UnitTestBase
    {
        [TestMethod]
        public void BasicIndexer()
        {
            Log("--- 测试: 基础索引器 ---");
            var map = new DefaultValueMap<string, int>(-1);

            map["a"] = 1;
            map["b"] = 2;

            Assert.AreEqual(1, map["a"], "map[\"a\"] 应等于 1");
            Assert.AreEqual(2, map["b"], "map[\"b\"] 应等于 2");
            Assert.AreEqual(2, map.Count, "设置两个键后 Count 应为 2");
            Log($"[基础索引器] 通过。map[\"a\"]={map["a"]}, map[\"b\"]={map["b"]}, Count={map.Count}");
        }

        [TestMethod]
        public void DefaultValue()
        {
            Log("--- 测试: 默认值 ---");
            var map = new DefaultValueMap<string, int>(999);

            int value = map["not_exists"];
            Assert.AreEqual(999, value, "未设置的键应返回默认值 999");
            Log($"[默认值] 通过。未设置的键返回默认值 {value}");

            Assert.AreEqual(0, map.Count, "get 访问未设置的键不应增加 Count");
            Log($"[默认值] 通过。get 访问未增加 Count, Count={map.Count}");
        }

        [TestMethod]
        public void AutoAdd()
        {
            Log("--- 测试: 自动添加 ---");
            var map = new DefaultValueMap<string, int>(0);

            map["new_key"] = 100;

            Assert.IsTrue(map.ContainsKey("new_key"), "set 后应包含 new_key");
            Assert.AreEqual(100, map["new_key"], "new_key 的值应为 100");
            Assert.AreEqual(1, map.Count, "Count 应为 1");
            Log($"[自动添加] 通过。set 不存在的键自动添加, ContainsKey={map.ContainsKey("new_key")}, Count={map.Count}");
        }

        [TestMethod]
        public void DictionaryOperations()
        {
            Log("--- 测试: IDictionary 操作 ---");
            var map = new DefaultValueMap<string, int>(0);

            map.Add("x", 1);
            map.Add("y", 2);
            Assert.AreEqual(2, map.Count, "Add 两个后 Count 应为 2");
            Log("[Add] 通过。添加了 x, y");

            CollectionAssert.AreEquivalent(new[] { "x", "y" }, map.Keys.ToList(), "[Keys] 失败");
            Log($"[Keys] 通过。Keys=[{string.Join(", ", map.Keys)}]");

            CollectionAssert.AreEquivalent(new[] { 1, 2 }, map.Values.ToList(), "[Values] 失败");
            Log($"[Values] 通过。Values=[{string.Join(", ", map.Values)}]");

            Assert.IsTrue(map.TryGetValue("x", out var val) && val == 1, "[TryGetValue] 失败");
            Log($"[TryGetValue] 通过。TryGetValue(\"x\")={val}");

            Assert.IsFalse(map.TryGetValue("not_exists", out _), "[TryGetValue] 失败。不存在的键应返回 false");
            Log("[TryGetValue] 通过。不存在的键返回 false");

            bool removed = map.Remove("x");
            Assert.IsTrue(removed && !map.ContainsKey("x"), "[Remove] 失败");
            Log($"[Remove] 通过。移除了 x, removed={removed}");

            map.Clear();
            Assert.AreEqual(0, map.Count, "[Clear] 失败。清空后 Count 应为 0");
            Log("[Clear] 通过。清空后 Count=0");
        }

        [TestMethod]
        public void Enumeration()
        {
            Log("--- 测试: 遍历 ---");
            var map = new DefaultValueMap<int, string>("默认");
            map[1] = "One";
            map[2] = "Two";
            map[3] = "Three";

            var items = map.ToList();
            Assert.AreEqual(3, items.Count, "应遍历出 3 项");
            Log($"[遍历] 通过。共 {items.Count} 项");
            foreach (var kvp in items)
            {
                Log($"  [{kvp.Key}] = {kvp.Value}");
            }

            CollectionAssert.AreEquivalent(new[] { "One", "Two", "Three" }, items.Select(kv => kv.Value).ToArray(), "[遍历] 值集合不匹配");
        }
    }
}
