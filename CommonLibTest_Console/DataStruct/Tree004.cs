using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Tree004() : TestBase("测试简易多叉树")
    {
        protected override void RunImpl()
        {
            WritePair(run1(), "测试1");
            WritePair(run2(), "测试2");
        }
        private string run1()
        {
            TestSimpleMultiTree1 tree = new();
            tree.Root = new SimpleMultiTreeNode<NodeValue>(new NodeValue("Root"));

            if (!tree.Root.TryAdd(new("Node01"), out SimpleMultiTreeNode<NodeValue>? node01)) return "创建节点 01 失败";
            if (!tree.Root.TryAdd(new("Node02"), out SimpleMultiTreeNode<NodeValue>? node02)) return "创建节点 02 失败";
            if (!tree.Root.TryAdd(new("Node03"), out SimpleMultiTreeNode<NodeValue>? node03)) return "创建节点 03 失败";


            if (!node01.TryAdd(new("Node04"), out SimpleMultiTreeNode<NodeValue>? node04)) return "创建节点 04 失败";
            if (!node02.TryAdd(new("Node05"), out SimpleMultiTreeNode<NodeValue>? node05)) return "创建节点 05 失败";
            if (!node02.TryAdd(new("Node06"), out SimpleMultiTreeNode<NodeValue>? node06)) return "创建节点 06 失败";

            if (!node05.TryAdd(new("Node07"), out SimpleMultiTreeNode<NodeValue>? node07)) return "创建节点 07 失败";
            if (!node05.TryAdd(new("Node08"), out SimpleMultiTreeNode<NodeValue>? node08)) return "创建节点 08 失败";
            if (!node05.TryAdd(new("Node09"), out SimpleMultiTreeNode<NodeValue>? node09)) return "创建节点 09 失败";

            if (!node03.TryAdd(new("Node10"), out SimpleMultiTreeNode<NodeValue>? node10)) return "创建节点 10 失败";
            if (!node03.TryAdd(new("Node11"), out SimpleMultiTreeNode<NodeValue>? node11)) return "创建节点 11 失败";
            if (!node03.TryAdd(new("Node12"), out SimpleMultiTreeNode<NodeValue>? node12)) return "创建节点 12 失败";

            var generalTree = tree.ToGeneralTree((node) => node.NodeValue.Value);
            WritePair(generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value), split: "\n");

            var simpleTree = generalTree.AsSimpleMultiTree().ToGeneralTree((node) => node.NodeValue?.Value ?? "<null>");
            WritePair(simpleTree.GetSimpleTreeString(nodeValue => nodeValue?.Value ?? "<null>"), split: "\n");

            return "执行完成";
        }

        private string run2()
        {
            TestSimpleMultiTree2 tree = new();
            tree.Root = new TestSimpleMultiTreeNode(new NodeValue("Root"));

            if (!tree.Root.TryAdd(new("Node01"), out TestSimpleMultiTreeNode? node01)) return "创建节点 01 失败";
            if (!tree.Root.TryAdd(new("Node02"), out TestSimpleMultiTreeNode? node02)) return "创建节点 02 失败";
            if (!tree.Root.TryAdd(new("Node03"), out TestSimpleMultiTreeNode? node03)) return "创建节点 03 失败";


            if (!node01.TryAdd(new("Node04"), out TestSimpleMultiTreeNode? node04)) return "创建节点 04 失败";
            if (!node02.TryAdd(new("Node05"), out TestSimpleMultiTreeNode? node05)) return "创建节点 05 失败";
            if (!node02.TryAdd(new("Node06"), out TestSimpleMultiTreeNode? node06)) return "创建节点 06 失败";

            if (!node05.TryAdd(new("Node07"), out TestSimpleMultiTreeNode? node07)) return "创建节点 07 失败";
            if (!node05.TryAdd(new("Node08"), out TestSimpleMultiTreeNode? node08)) return "创建节点 08 失败";
            if (!node05.TryAdd(new("Node09"), out TestSimpleMultiTreeNode? node09)) return "创建节点 09 失败";

            if (!node03.TryAdd(new("Node10"), out TestSimpleMultiTreeNode? node10)) return "创建节点 10 失败";
            if (!node03.TryAdd(new("Node11"), out TestSimpleMultiTreeNode? node11)) return "创建节点 11 失败";
            if (!node03.TryAdd(new("Node12"), out TestSimpleMultiTreeNode? node12)) return "创建节点 12 失败";

            var generalTree = tree.ToGeneralTree((node) => node.NodeValue.Value);
            WritePair(generalTree.GetSimpleTreeString(nodeValue => nodeValue.Value), split: "\n");

            var simpleTree = generalTree.AsSimpleMultiTree().ToGeneralTree((node) => node.NodeValue?.Value ?? "<null>");
            WritePair(simpleTree.GetSimpleTreeString(nodeValue => nodeValue?.Value ?? "<null>"), split: "\n");

            return "执行完成";
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
