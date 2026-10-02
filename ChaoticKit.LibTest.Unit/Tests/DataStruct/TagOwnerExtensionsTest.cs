using ChaoticKit.Data.Extensions;
using ChaoticKit.Data.Structure.Map;
using ChaoticKit.Interfaces.Owner;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.TagOwner001
    /// ITagsOwner 集合扩展方法 (QueryAllNoTag / QueryExistTag / QueryWhere / QueryMatchingTag / AsTagsMap)
    /// 与 TagsMap 的行为验证, 覆盖 null 标签、空标签等边界
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class TagOwnerExtensionsTest : UnitTestBase
    {
        /// <summary>
        /// 固定测试数据: 含无标签 (C)、全 null 标签 (F/G)、null 与非 null 混合 (E)、普通多标签 (A/B/D) 各种形态
        /// </summary>
        private static List<TestItem> BuildTestItems() => new()
        {
            new("A", "11", "22", "33", "qqq"),
            new("B", "DD", "AA", "1", "qqq"),
            new("C"),
            new("D", "DD", "22", "33", "99"),
            new("E", null, "22", "1", "qqq"),
            new("F", null, null, null),
            new("G", null, null),
        };

        /// <summary>
        /// 取查询结果的 Name 列表, 便于断言与日志
        /// </summary>
        private static string[] Names(IEnumerable<TestItem> items) => items.Select(i => i.Name).ToArray();

        [TestMethod]
        public void QueryAllNoTag_忽略Null标签()
        {
            Log("--- 测试: QueryAllNoTag(默认 ignoreNull=true) ---");
            var test = BuildTestItems();

            var result = test.QueryAllNoTag().ToList();

            CollectionAssert.AreEqual(
                new[] { "C", "F", "G" },
                Names(result),
                "无标签的 C 与全 null 标签的 F/G 应被查出, 且保持源顺序");

            Log($"[QueryAllNoTag] 通过。查出: {string.Join(", ", Names(result))}");
        }

        [TestMethod]
        public void QueryAllNoTag_不忽略Null标签()
        {
            Log("--- 测试: QueryAllNoTag(ignoreNull=false) ---");
            var test = BuildTestItems();

            var result = test.QueryAllNoTag(ignoreNull: false).ToList();

            CollectionAssert.AreEqual(
                new[] { "C" },
                Names(result),
                "ignoreNull=false 时只有无标签的 C 应被查出, F/G 因存在 null 标签被排除");

            Log($"[QueryAllNoTag(false)] 通过。查出: {string.Join(", ", Names(result))}");
        }

        [DataTestMethod]
        [DataRow(new[] { "1" }, "B,E")]
        [DataRow(new[] { "22" }, "A,D,E")]
        [DataRow(new[] { "33" }, "A,D")]
        [DataRow(new[] { "qqq" }, "A,B,E")]
        public void QueryExistTag_存在任一标签(string[] queryTags, string expectedNames)
        {
            Log($"--- 测试: QueryExistTag({string.Join(",", queryTags.Select(t => $"\"{t}\""))}) ---");
            var test = BuildTestItems();

            var result = test.QueryExistTag(queryTags).ToList();

            CollectionAssert.AreEqual(
                expectedNames.Split(','),
                Names(result),
                $"QueryExistTag 结果不匹配。预期: {expectedNames}, 实际: {string.Join(",", Names(result))}");

            Log($"[QueryExistTag] 通过。查出: {string.Join(",", Names(result))}");
        }

        [TestMethod]
        public void QueryExistTag_空参数不返回任何项()
        {
            Log("--- 测试: QueryExistTag() 空参数 ---");
            var test = BuildTestItems();

            var result = test.QueryExistTag().ToList();

            Assert.AreEqual(0, result.Count, "空标签参数应 yield break, 不返回任何项");
            Log("[QueryExistTag(空)] 通过。返回 0 项");
        }

        [TestMethod]
        public void QueryWhere_自定义条件()
        {
            Log("--- 测试: QueryWhere(i => i.Count() == 3 && i.All(s => s == null)) ---");
            var test = BuildTestItems();

            var result = test.QueryWhere(i => i.Count() == 3 && i.All(s => s == null)).ToList();

            CollectionAssert.AreEqual(
                new[] { "F" },
                Names(result),
                "仅 F (3 个标签且全 null) 应满足条件");

            Log($"[QueryWhere] 通过。查出: {string.Join(", ", Names(result))}");
        }

        [DataTestMethod]
        [DataRow(new[] { "1", "AA", "DD" }, "B")]
        [DataRow(new[] { "DD" }, "B,D")]
        [DataRow(new[] { "22", "33" }, "A,D")]
        [DataRow(new[] { "qqq" }, "A,B,E")]
        public void QueryMatchingTag_需包含全部标签(string[] queryTags, string expectedNames)
        {
            Log($"--- 测试: QueryMatchingTag({string.Join(",", queryTags.Select(t => $"\"{t}\""))}) ---");
            var test = BuildTestItems();

            var result = test.QueryMatchingTag(queryTags).ToList();

            CollectionAssert.AreEqual(
                expectedNames.Split(','),
                Names(result),
                $"QueryMatchingTag 结果不匹配。预期: {expectedNames}, 实际: {string.Join(",", Names(result))}");

            Log($"[QueryMatchingTag] 通过。查出: {string.Join(",", Names(result))}");
        }

        [TestMethod]
        public void QueryMatchingTag_空参数返回全部()
        {
            Log("--- 测试: QueryMatchingTag() 空参数 ---");
            var test = BuildTestItems();

            var result = test.QueryMatchingTag().ToList();

            Assert.AreEqual(7, result.Count, "空标签参数应返回全部项");
            Log($"[QueryMatchingTag(空)] 通过。返回 {result.Count} 项");
        }

        [TestMethod]
        public void AsTagsMap_构建并过滤Null标签()
        {
            Log("--- 测试: AsTagsMap 构建 TagsMap 并过滤 null 标签 ---");
            var test = BuildTestItems();

            var map = test.AsTagsMap(i => i.Name);

            Assert.AreEqual(7, map.Count, "AsTagsMap 应包含 7 个条目");
            Log($"[AsTagsMap] 通过。Count={map.Count}");

            CollectionAssert.AreEquivalent(
                new[] { "A", "B", "C", "D", "E", "F", "G" },
                map.Keys.ToList(),
                "键集合应为 A~G");

            var tagsOf = map.ToDictionary(kv => kv.Key, kv => kv.Value.Tags);

            CollectionAssert.AreEqual(new[] { "11", "22", "33", "qqq" }, tagsOf["A"], "A 的标签应完整保留");
            CollectionAssert.AreEqual(new[] { "DD", "AA", "1", "qqq" }, tagsOf["B"], "B 的标签应完整保留");
            CollectionAssert.AreEqual(Array.Empty<string>(), tagsOf["C"], "C 无标签应为空数组");
            CollectionAssert.AreEqual(new[] { "DD", "22", "33", "99" }, tagsOf["D"], "D 的标签应完整保留");
            CollectionAssert.AreEqual(new[] { "22", "1", "qqq" }, tagsOf["E"], "E 的 null 标签应被过滤");
            CollectionAssert.AreEqual(Array.Empty<string>(), tagsOf["F"], "F 全 null 标签过滤后应为空数组");
            CollectionAssert.AreEqual(Array.Empty<string>(), tagsOf["G"], "G 全 null 标签过滤后应为空数组");

            Log("[AsTagsMap] 通过。null 标签已过滤, 各条目标签校验一致");
        }

        [TestMethod]
        public void TagsMapValues_QueryAllNoTag()
        {
            Log("--- 测试: TagsMap.Values 上继续调用 QueryAllNoTag ---");
            var test = BuildTestItems();
            var map = test.AsTagsMap(i => i.Name);

            var noTag = map.Values.QueryAllNoTag().Select(v => v.Value!.Name).ToList();

            CollectionAssert.AreEquivalent(
                new[] { "C", "F", "G" },
                noTag,
                "C/F/G 在 TagsMap 内无标签(全 null 已被过滤为空), 应被 QueryAllNoTag 查出");

            Log($"[TagsMap.Values.QueryAllNoTag] 通过。查出: {string.Join(", ", noTag)}");
        }

        /// <summary>
        /// 被测项: 实现 <see cref="ITagsOwner"/> 的简单数据类 (含 null 标签场景)
        /// </summary>
        private sealed class TestItem(string name, params string?[] tags) : ITagsOwner
        {
            public string Name { get; } = name;

            public string?[] Tags { get; } = tags;

            IEnumerable<string?> ITagsOwner.Tags => Tags;

            public override string ToString()
            {
                return $"{Name} [{string.Join(", ", Tags.Select(i => i ?? "<null>"))}]";
            }
        }
    }
}
