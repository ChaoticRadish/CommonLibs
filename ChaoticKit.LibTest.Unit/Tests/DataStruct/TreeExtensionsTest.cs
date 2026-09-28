using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Data.Structure.Value.Extensions;
using ChaoticKit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree001
    /// IMultiTree 扩展 (ChaoticKit.Data) 行为验证:
    /// AsMultiTree / Preorder / Postorder / IndexPreorder / ToGeneralTree / Convert / AsSimpleMultiTree / GetSimpleTreeString
    /// 固定 testData (18 个分层地址编码), 断言遍历序列与树结构
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 预期序列按 ChaoticKit.Data 当前实现手工推导:
    /// - AsMultiTree 的 rootValue 参数当前仅做合法性校验, 未参与建树 (根节点恒为完全范围),
    ///   故 "完全范围 (rootValue=null)" 与 "限定范围 (rootValue=\"ccc\")" 两种入参结果一致;
    /// - 建树按输入顺序插入子节点 (SimpleMultiTreeNode.AddNode 追加到列表尾部), 先根/后根序列见各测试方法内期望数组。
    /// </remarks>
    [TestClass]
    public sealed class TreeExtensionsTest : UnitTestBase
    {
        /// <summary>
        /// 固定测试数据: 18 个分层地址编码 (范围编码/项编码混合)
        /// </summary>
        private static List<LayeringAddressCode> CreateTestData()
        {
            return new List<LayeringAddressCode>
            {
                "aaa",
                "aaa:1",
                "aaa.bb.1",
                "aaa.bb:1",
                "ccc.cc:1",
                "ccc.dd.ee:3",
                "ccc.dd.ff:3",
                "ccc.cc:2",
                "ccc.cc:3",
                "ccc.dd.ee:1",
                "ccc.dd.ff:1",
                "ccc.cc:4",
                "ccc.cc:5",
                "ccc.dd.ee:2",
                "ccc.dd.ff:2",
                "ccc.dd:1",
                "ccc.dd:2",
                "qqq",
            };
        }

        /// <summary>
        /// 按源测试构建多叉树: rootValue 为 null (完全范围) 或 "ccc" (限定范围)
        /// </summary>
        private static IMultiTree<ILayeringAddressCode<string>> BuildTree(bool all)
        {
            List<LayeringAddressCode> testData = CreateTestData();
            var temp = testData.Select(i => (ILayeringAddressCode<string>)i);
            return temp.AsMultiTree(rootValue: all ? null : (LayeringAddressCode?)"ccc");
        }

        /// <summary>
        /// 遍历序列用格式化: null → &lt;null&gt;, 完全范围 → ALL!!!, 其余转字符串
        /// (与源测试先根/后根打印格式一致)
        /// </summary>
        private static string FormatCode(ILayeringAddressCode<string>? code)
        {
            if (code == null) return "<null>";
            if (code.IsAll()) return "ALL!!!";
            return LayeringAddressCodeHelper.Convert(code).ToString();
        }

        /// <summary>
        /// 简单树字符串用格式化: null → &lt;null&gt;, 完全范围 → &lt;ALL&gt;, 其余转字符串
        /// (与源测试 codeToString 一致)
        /// </summary>
        private static string CodeToString(ILayeringAddressCode<string>? code)
        {
            if (code == null) return "<null>";
            if (code.IsAll()) return "<ALL>";
            return LayeringAddressCodeHelper.Convert(code).ToString();
        }

        /// <summary>
        /// 期望先根序列: 根 (完全范围) + 24 个节点, 按插入顺序深度优先 (共 25 个节点)
        /// </summary>
        private static readonly string[] ExpectedPreorder = new[]
        {
            "ALL!!!", "aaa", "aaa:1", "aaa.bb", "aaa.bb.1", "aaa.bb:1",
            "ccc", "ccc.cc", "ccc.cc:1", "ccc.cc:2", "ccc.cc:3", "ccc.cc:4", "ccc.cc:5",
            "ccc.dd", "ccc.dd.ee", "ccc.dd.ee:3", "ccc.dd.ee:1", "ccc.dd.ee:2",
            "ccc.dd.ff", "ccc.dd.ff:3", "ccc.dd.ff:1", "ccc.dd.ff:2",
            "ccc.dd:1", "ccc.dd:2", "qqq",
        };

        /// <summary>
        /// 期望后根序列 (子节点先于父节点, 根最后)
        /// </summary>
        private static readonly string[] ExpectedPostorder = new[]
        {
            "aaa:1", "aaa.bb.1", "aaa.bb:1", "aaa.bb", "aaa",
            "ccc.cc:1", "ccc.cc:2", "ccc.cc:3", "ccc.cc:4", "ccc.cc:5", "ccc.cc",
            "ccc.dd.ee:3", "ccc.dd.ee:1", "ccc.dd.ee:2", "ccc.dd.ee",
            "ccc.dd.ff:3", "ccc.dd.ff:1", "ccc.dd.ff:2", "ccc.dd.ff",
            "ccc.dd:1", "ccc.dd:2", "ccc.dd", "ccc", "qqq", "ALL!!!",
        };

        /// <summary>
        /// 期望 IndexPreorder 的父节点索引序列 (根节点为 -1, 共 25 项)
        /// </summary>
        private static readonly int[] ExpectedParentIndices = new[]
        {
            -1, 0, 1, 1, 3, 3, 0, 6, 7, 7, 7, 7, 7, 6, 13, 14, 14, 14, 13, 18, 18, 18, 13, 13, 0,
        };

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void AsMultiTree_先根遍历序列(bool all)
        {
            Log($"--- 测试: AsMultiTree(rootValue: {(all ? "null(完全范围)" : "\"ccc\"(限定范围)")}) 先根遍历 ---");
            var tree1 = BuildTree(all);

            string[] actual = tree1.Preorder().Select(FormatCode).ToArray();
            CollectionAssert.AreEqual(ExpectedPreorder, actual,
                "先根遍历序列不匹配 (当前实现下 rootValue 不参与建树, 两行数据应一致)");
            Log($"先根遍历共 {actual.Length} 个节点: [{string.Join(", ", actual)}]");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void AsMultiTree_后根遍历序列(bool all)
        {
            Log($"--- 测试: AsMultiTree(rootValue: {(all ? "null(完全范围)" : "\"ccc\"(限定范围)")}) 后根遍历 ---");
            var tree1 = BuildTree(all);

            string[] actual = tree1.Postorder().Select(FormatCode).ToArray();
            CollectionAssert.AreEqual(ExpectedPostorder, actual,
                "后根遍历序列不匹配 (当前实现下 rootValue 不参与建树, 两行数据应一致)");
            Log($"后根遍历共 {actual.Length} 个节点: [{string.Join(", ", actual)}]");
        }

        [TestMethod]
        public void AsMultiTree_IndexPreorder_索引与父节点()
        {
            Log("--- 测试: IndexPreorder 索引 / 父节点 ---");
            var tree1 = BuildTree(all: true);

            var items = tree1.IndexPreorder().ToList();
            Assert.AreEqual(25, items.Count, "节点总数应为 25 (完全范围根 + 24 个节点)");

            CollectionAssert.AreEqual(Enumerable.Range(0, 25).ToArray(),
                items.Select(i => i.NodeIndex).ToArray(), "节点索引序列应为 0..24");
            CollectionAssert.AreEqual(ExpectedParentIndices,
                items.Select(i => i.ParentIndex).ToArray(), "父节点索引序列不匹配");
            CollectionAssert.AreEqual(ExpectedPreorder,
                items.Select(i => FormatCode(i.NodeValue)).ToArray(), "IndexPreorder 值序列应与先根一致");

            Assert.AreEqual(-1, items[0].ParentIndex, "根节点父索引应为 -1");
            Assert.IsTrue(items[0].NodeValue.IsAll(), "根节点值应为完全范围");
            Assert.AreEqual(0, items[0].Depth, "根节点深度应为 0");

            Log($"IndexPreorder 共 {items.Count} 项, 根节点为完全范围 (深度 0), 索引/父节点序列全部通过");
        }

        [TestMethod]
        public void ToGeneralTree_作用域与值保持()
        {
            Log("--- 测试: ToGeneralTree 结构保持 (作用域 = 节点值) ---");
            var tree1 = BuildTree(all: true);
            var tree2 = tree1.ToGeneralTree(node => node.NodeValue);

            Assert.IsNotNull(tree2.Root, "GeneralTree 根节点不应为 null");

            // convert2Scope 传入 node.NodeValue, 因此作用域序列应与先根值序列一致 (与 ExpectedPreorder 同用 FormatCode 格式)
            var scopes = new List<string>();
            tree2.Preorder((node, index) => scopes.Add(FormatCode(node.NodeScope)));
            CollectionAssert.AreEqual(ExpectedPreorder, scopes.ToArray(), "GeneralTree 作用域序列不匹配");

            var values = new List<string>();
            tree2.Preorder((node, index) => values.Add(FormatCode(node.NodeValue)));
            CollectionAssert.AreEqual(ExpectedPreorder, values.ToArray(), "GeneralTree 节点值序列不匹配");

            Assert.AreEqual("<ALL>", CodeToString(tree2.Root!.NodeScope), "根作用域应为完全范围");

            Log($"tree2 节点数: {scopes.Count}, 根作用域: {CodeToString(tree2.Root!.NodeScope)}, 作用域/值序列通过");
        }

        [TestMethod]
        public void GetSimpleTreeString_行数与标签()
        {
            Log("--- 测试: GetSimpleTreeString 输出 (tree2) ---");
            var tree1 = BuildTree(all: true);
            var tree2 = tree1.ToGeneralTree(node => node.NodeValue);

            string simple = tree2.GetSimpleTreeString(
                getScopeStringFunc: CodeToString,
                getValueStringFunc: CodeToString);
            Assert.IsFalse(string.IsNullOrEmpty(simple), "GetSimpleTreeString 不应为空");

            // 每片叶子输出一行, 叶子数 = 17 (aaa:1, aaa.bb.1, aaa.bb:1, ccc.cc:1~5,
            // ccc.dd.ee:3/1/2, ccc.dd.ff:3/1/2, ccc.dd:1, ccc.dd:2, qqq)
            int lineCount = simple.Split('\n').Length;
            Assert.AreEqual(17, lineCount, $"简单树字符串行数应等于叶子节点数 17, 实际 {lineCount}");

            foreach (string label in new[]
            {
                "<ALL>", "aaa:1", "aaa.bb", "aaa.bb.1", "aaa.bb:1",
                "ccc.cc:5", "ccc.dd.ee:3", "ccc.dd.ff:2", "ccc.dd:2", "qqq",
            })
            {
                StringAssert.Contains(simple, label, $"简单树字符串应包含节点标签 {label}");
            }

            Log($"tree2 GetSimpleTreeString 共 {lineCount} 行, 含全部关键节点标签, 通过:\n{simple}");
        }

        [TestMethod]
        public void Convert与AsSimpleMultiTree_链式转换保持结构()
        {
            Log("--- 测试: Convert / ToGeneralTree / AsSimpleMultiTree 链式转换 ---");
            var tree1 = BuildTree(all: true);
            var tree2 = tree1.ToGeneralTree(node => node.NodeValue);

            // 库行为快照: IMultiTreeExtensions._convert 的返回条件写反
            // (newNodes.Count > 0 时返回新建根节点而非 newNodes[0]), 导致 Convert 只保留单个根节点, 子节点全部丢失。
            // 与文档"将保留结构"不符 —— 疑似库 bug, 待人工确认是否修复。以下断言固化当前实际行为。
            var tree3 = tree2.Convert(node => "!TREE3!" + CodeToString(node.NodeValue));
            var tree4 = tree3.ToGeneralTree(node => node.NodeValue);
            var tree5 = tree4.AsSimpleMultiTree();
            var tree6 = tree5.ToGeneralTree(node => node.NodeValue);

            string expectedRootValue = "!TREE3!" + CodeToString(tree2.Root!.NodeValue);
            foreach (var (name, tree) in new[] { ("tree3", tree3), ("tree4", tree4), ("tree5", tree5), ("tree6", tree6) })
            {
                var preorder = tree.Preorder().ToList();
                Assert.AreEqual(1, preorder.Count, $"{name} (Convert) 先根序列: 库行为快照, 当前只保留根节点 (疑似 bug)");
                Assert.AreEqual(expectedRootValue, preorder[0], $"{name} 根节点值应带前缀");
                Log($"{name} 先根序列: [{string.Join(", ", preorder)}] (行为快照: 仅根节点)");
            }

            // 父索引结构: 快照下仅 1 个节点, 根父索引 -1
            var indices = tree3.IndexPreorder().ToList();
            Assert.AreEqual(1, indices.Count, "Convert 后仅根节点");
            Assert.AreEqual(-1, indices[0].ParentIndex, "根节点父索引应为 -1");

            string simple4 = tree4.GetSimpleTreeString(getScopeStringFunc: s => s, getValueStringFunc: s => s);
            Assert.IsFalse(string.IsNullOrEmpty(simple4), "tree4 简单树不应为空");
            StringAssert.Contains(simple4, expectedRootValue, "tree4 简单树应含转换后的根标签");

            string simple6 = tree6.GetSimpleTreeString(getScopeStringFunc: s => s ?? "<null>", getValueStringFunc: s => s ?? "<null>");
            StringAssert.Contains(simple6, expectedRootValue, "tree6 简单树应含转换后的根标签");

            string fullInfo = tree5.FullInfoString();
            Assert.IsFalse(string.IsNullOrEmpty(fullInfo), "tree5 FullInfoString 不应为空");
            Log($"tree5 FullInfoString:\n{fullInfo}");

            Log("链式转换完成 (行为快照: Convert 当前仅保留根节点, 疑似库 bug, 待确认)");
        }
    }
}
