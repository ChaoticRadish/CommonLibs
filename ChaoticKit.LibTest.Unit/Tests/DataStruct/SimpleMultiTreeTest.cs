using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree004
    /// 简易多叉树 SimpleMultiTree / SimpleMultiTreeNode 的行为验证:
    /// - TryAdd 逐层构建固定结构的树
    /// - ToGeneralTree 转换为通用树
    /// - AsSimpleMultiTree 从通用树转回简易多叉树 (结构保持)
    /// - 自定义节点子类: InitCreateChildrenList 预置子节点 + AddNode 头插
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class SimpleMultiTreeTest : UnitTestBase
    {
        /// <summary>
        /// 构建固定结构的普通节点树: Root 下 Node01/Node02/Node03, 各层子节点结构固定
        /// </summary>
        private static TestSimpleMultiTree1 BuildPlainTree()
        {
            TestSimpleMultiTree1 tree = new();
            SimpleMultiTreeNode<NodeValue> root = new(new NodeValue("Root"));
            tree.Root = root;

            Assert.IsTrue(root.TryAdd(new NodeValue("Node01"), out SimpleMultiTreeNode<NodeValue>? node01), "创建节点 01 失败");
            Assert.IsTrue(root.TryAdd(new NodeValue("Node02"), out SimpleMultiTreeNode<NodeValue>? node02), "创建节点 02 失败");
            Assert.IsTrue(root.TryAdd(new NodeValue("Node03"), out SimpleMultiTreeNode<NodeValue>? node03), "创建节点 03 失败");
            Assert.IsNotNull(node01);
            Assert.IsNotNull(node02);
            Assert.IsNotNull(node03);

            Assert.IsTrue(node01.TryAdd(new NodeValue("Node04"), out SimpleMultiTreeNode<NodeValue>? node04), "创建节点 04 失败");
            Assert.IsTrue(node02.TryAdd(new NodeValue("Node05"), out SimpleMultiTreeNode<NodeValue>? node05), "创建节点 05 失败");
            Assert.IsTrue(node02.TryAdd(new NodeValue("Node06"), out SimpleMultiTreeNode<NodeValue>? node06), "创建节点 06 失败");
            Assert.IsNotNull(node04);
            Assert.IsNotNull(node05);
            Assert.IsNotNull(node06);

            Assert.IsTrue(node05.TryAdd(new NodeValue("Node07"), out SimpleMultiTreeNode<NodeValue>? node07), "创建节点 07 失败");
            Assert.IsTrue(node05.TryAdd(new NodeValue("Node08"), out SimpleMultiTreeNode<NodeValue>? node08), "创建节点 08 失败");
            Assert.IsTrue(node05.TryAdd(new NodeValue("Node09"), out SimpleMultiTreeNode<NodeValue>? node09), "创建节点 09 失败");

            Assert.IsTrue(node03.TryAdd(new NodeValue("Node10"), out SimpleMultiTreeNode<NodeValue>? node10), "创建节点 10 失败");
            Assert.IsTrue(node03.TryAdd(new NodeValue("Node11"), out SimpleMultiTreeNode<NodeValue>? node11), "创建节点 11 失败");
            Assert.IsTrue(node03.TryAdd(new NodeValue("Node12"), out SimpleMultiTreeNode<NodeValue>? node12), "创建节点 12 失败");

            return tree;
        }

        /// <summary>
        /// 构建固定结构的自定义子类树 (AddNode 头插, 每个有子节点的节点末尾预置 1 个 "Inited")
        /// </summary>
        private static TestSimpleMultiTree2 BuildCustomTree()
        {
            TestSimpleMultiTree2 tree = new();
            TestSimpleMultiTreeNode root = new(new NodeValue("Root"));
            tree.Root = root;

            Assert.IsTrue(root.TryAdd(new NodeValue("Node01"), out TestSimpleMultiTreeNode? node01), "创建节点 01 失败");
            Assert.IsTrue(root.TryAdd(new NodeValue("Node02"), out TestSimpleMultiTreeNode? node02), "创建节点 02 失败");
            Assert.IsTrue(root.TryAdd(new NodeValue("Node03"), out TestSimpleMultiTreeNode? node03), "创建节点 03 失败");
            Assert.IsNotNull(node01);
            Assert.IsNotNull(node02);
            Assert.IsNotNull(node03);

            Assert.IsTrue(node01.TryAdd(new NodeValue("Node04"), out TestSimpleMultiTreeNode? node04), "创建节点 04 失败");
            Assert.IsTrue(node02.TryAdd(new NodeValue("Node05"), out TestSimpleMultiTreeNode? node05), "创建节点 05 失败");
            Assert.IsTrue(node02.TryAdd(new NodeValue("Node06"), out TestSimpleMultiTreeNode? node06), "创建节点 06 失败");
            Assert.IsNotNull(node04);
            Assert.IsNotNull(node05);
            Assert.IsNotNull(node06);

            Assert.IsTrue(node05.TryAdd(new NodeValue("Node07"), out TestSimpleMultiTreeNode? node07), "创建节点 07 失败");
            Assert.IsTrue(node05.TryAdd(new NodeValue("Node08"), out TestSimpleMultiTreeNode? node08), "创建节点 08 失败");
            Assert.IsTrue(node05.TryAdd(new NodeValue("Node09"), out TestSimpleMultiTreeNode? node09), "创建节点 09 失败");

            Assert.IsTrue(node03.TryAdd(new NodeValue("Node10"), out TestSimpleMultiTreeNode? node10), "创建节点 10 失败");
            Assert.IsTrue(node03.TryAdd(new NodeValue("Node11"), out TestSimpleMultiTreeNode? node11), "创建节点 11 失败");
            Assert.IsTrue(node03.TryAdd(new NodeValue("Node12"), out TestSimpleMultiTreeNode? node12), "创建节点 12 失败");

            return tree;
        }

        /// <summary>
        /// 收集通用树的先序遍历节点值序列 (null 值表示为 "&lt;null&gt;")
        /// </summary>
        private static List<string> CollectPreorderValues(GeneralTree<string, NodeValue> tree)
        {
            var values = new List<string>();
            tree.Preorder((node, _) => values.Add(node.NodeValue?.Value ?? "<null>"));
            return values;
        }

        [TestMethod]
        public void BuildWithTryAdd_PlainNodes()
        {
            Log("--- 测试: 普通节点 TryAdd 构建固定结构 ---");
            TestSimpleMultiTree1 tree = BuildPlainTree();

            // 逐层构建的节点数量
            Assert.AreEqual(3, tree.Root!.Childrens!.Count, "Root 应有 3 个子节点");
            Assert.AreEqual(1, tree.Root.Childrens[0].Childrens!.Count, "Node01 应有 1 个子节点");
            Assert.AreEqual(2, tree.Root.Childrens[1].Childrens!.Count, "Node02 应有 2 个子节点");
            Assert.AreEqual(3, tree.Root.Childrens[1].Childrens[0].Childrens!.Count, "Node05 应有 3 个子节点");
            Assert.AreEqual(3, tree.Root.Childrens[2].Childrens!.Count, "Node03 应有 3 个子节点");

            // ToGeneralTree 转换后先序遍历序列
            GeneralTree<string, NodeValue> generalTree = tree.ToGeneralTree(node => node.NodeValue.Value);
            CollectionAssert.AreEqual(
                new[] { "Root", "Node01", "Node04", "Node02", "Node05", "Node07", "Node08", "Node09", "Node06", "Node03", "Node10", "Node11", "Node12" },
                CollectPreorderValues(generalTree),
                "ToGeneralTree 转换后先序遍历序列不符");

            Log($"generalTree 先序: {string.Join(", ", CollectPreorderValues(generalTree))}");
            Log($"generalTree 树形:\n{generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value)}");
            Log("普通节点树构建与 ToGeneralTree 转换, 通过");
        }

        [TestMethod]
        public void RoundTrip_AsSimpleMultiTree_PlainNodes()
        {
            Log("--- 测试: AsSimpleMultiTree 往返转换保持结构 (普通节点) ---");
            TestSimpleMultiTree1 tree = BuildPlainTree();
            GeneralTree<string, NodeValue> generalTree = tree.ToGeneralTree(node => node.NodeValue.Value);

            var simpleTree = generalTree.AsSimpleMultiTree().ToGeneralTree(node => node.NodeValue?.Value ?? "<null>");

            CollectionAssert.AreEqual(
                CollectPreorderValues(generalTree),
                CollectPreorderValues(simpleTree),
                "AsSimpleMultiTree 往返后先序遍历序列应保持一致");

            string originalText = generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value);
            string roundTripText = simpleTree.GetSimpleTreeString(nodeValue => nodeValue?.Value ?? "<null>");
            Assert.AreEqual(originalText, roundTripText, "往返转换后的树形字符串应一致");

            Log($"往返一致, 树形:\n{roundTripText}");
            Log("普通节点 AsSimpleMultiTree 往返转换, 通过");
        }

        [TestMethod]
        public void BuildWithTryAdd_CustomSubclass()
        {
            Log("--- 测试: 自定义子类 TryAdd (InitCreateChildrenList 预置 + AddNode 头插) ---");
            TestSimpleMultiTree2 tree = BuildCustomTree();

            // AddNode 头插: 新节点在前, InitCreateChildrenList 预置的 Inited 在末尾
            CollectionAssert.AreEqual(
                new[] { "Node03", "Node02", "Node01", "Inited" },
                tree.Root!.Childrens!.Select(c => c.NodeValue.Value).ToArray(),
                "Root 子节点顺序应为新节点头插在前, Inited 在末尾");
            CollectionAssert.AreEqual(
                new[] { "Node06", "Node05", "Inited" },
                tree.Root.Childrens[1].Childrens!.Select(c => c.NodeValue.Value).ToArray(),
                "node02 子节点顺序不符");
            CollectionAssert.AreEqual(
                new[] { "Node09", "Node08", "Node07", "Inited" },
                tree.Root.Childrens[1].Childrens[1].Childrens!.Select(c => c.NodeValue.Value).ToArray(),
                "node05 子节点顺序不符");
            CollectionAssert.AreEqual(
                new[] { "Node04", "Inited" },
                tree.Root.Childrens[2].Childrens!.Select(c => c.NodeValue.Value).ToArray(),
                "node01 子节点顺序不符");

            // ToGeneralTree 转换后先序遍历序列 (含 Inited 节点)
            GeneralTree<string, NodeValue> generalTree = tree.ToGeneralTree(node => node.NodeValue.Value);
            CollectionAssert.AreEqual(
                new[] {
                    "Root", "Node03", "Node12", "Node11", "Node10", "Inited",
                    "Node02", "Node06", "Node05", "Node09", "Node08", "Node07", "Inited", "Inited",
                    "Node01", "Node04", "Inited", "Inited"
                },
                CollectPreorderValues(generalTree),
                "自定义子类树 ToGeneralTree 后先序遍历序列不符");

            Log($"generalTree 先序: {string.Join(", ", CollectPreorderValues(generalTree))}");
            Log($"generalTree 树形:\n{generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value)}");
            Log("自定义子类树构建与 ToGeneralTree 转换, 通过");
        }

        [TestMethod]
        public void RoundTrip_AsSimpleMultiTree_CustomSubclass()
        {
            Log("--- 测试: AsSimpleMultiTree 往返转换保持结构 (自定义子类) ---");
            TestSimpleMultiTree2 tree = BuildCustomTree();
            GeneralTree<string, NodeValue> generalTree = tree.ToGeneralTree(node => node.NodeValue.Value);

            var simpleTree = generalTree.AsSimpleMultiTree().ToGeneralTree(node => node.NodeValue?.Value ?? "<null>");

            CollectionAssert.AreEqual(
                CollectPreorderValues(generalTree),
                CollectPreorderValues(simpleTree),
                "AsSimpleMultiTree 往返后先序遍历序列应保持一致");

            string originalText = generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value);
            string roundTripText = simpleTree.GetSimpleTreeString(nodeValue => nodeValue?.Value ?? "<null>");
            Assert.AreEqual(originalText, roundTripText, "往返转换后的树形字符串应一致");

            Log($"往返一致, 树形:\n{roundTripText}");
            Log("自定义子类 AsSimpleMultiTree 往返转换, 通过");
        }

        [TestMethod]
        public void CheckInitChildrenList_LazyInitializeWithInited()
        {
            Log("--- 测试: InitCreateChildrenList 惰性预置子节点 ---");
            var node = new TestSimpleMultiTreeNode(new NodeValue("NodeX"));

            Assert.IsNull(node.Childrens, "未触发添加前 Childrens 应为 null");

            node.CheckInitChildrenList();
            Assert.IsNotNull(node.Childrens, "CheckInitChildrenList 后 Childrens 应已初始化");
            CollectionAssert.AreEqual(
                new[] { "Inited" },
                node.Childrens!.Select(c => c.NodeValue.Value).ToArray(),
                "InitCreateChildrenList 应预置 1 个 Inited 子节点");

            Log($"CheckInitChildrenList 后子节点: {string.Join(", ", node.Childrens.Select(c => c.NodeValue.Value))}, 通过");
        }

        private class TestSimpleMultiTree1 : SimpleMultiTree<NodeValue>
        {
        }

        private class TestSimpleMultiTree2 : SimpleMultiTree<NodeValue, TestSimpleMultiTreeNode>
        {
        }

        private class TestSimpleMultiTreeNode : SimpleMultiTreeNode<NodeValue, TestSimpleMultiTreeNode>
        {
            public TestSimpleMultiTreeNode(NodeValue nodeValue, IList<TestSimpleMultiTreeNode>? initChildrens = null) : base(nodeValue, initChildrens)
            {
            }

            protected override IList<TestSimpleMultiTreeNode> InitCreateChildrenList()
            {
                return new List<TestSimpleMultiTreeNode>()
                {
                    new TestSimpleMultiTreeNode(new NodeValue("Inited"))
                };
            }

            public override void AddNode(TestSimpleMultiTreeNode node)
            {
                CheckInitChildrenList();
                Childrens.Insert(0, node);
            }

            protected override TestSimpleMultiTreeNode CreateChildren(NodeValue value)
            {
                return new(value);
            }
        }

        private class NodeValue(string str)
        {
            public string Value { get; init; } = str;
        }
    }
}
