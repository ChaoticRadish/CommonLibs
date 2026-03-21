using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibTest_Console.DataStruct
{
    internal class Tree006() : TestBase("测试 PreorderNode 条件遍历")
    {
        protected override void RunImpl()
        {
            var root = new SimpleMultiTreeNode<int>(1);
            var child1 = new SimpleMultiTreeNode<int>(2);
            var child2 = new SimpleMultiTreeNode<int>(3);
            var grandChild1 = new SimpleMultiTreeNode<int>(4);
            var grandChild2 = new SimpleMultiTreeNode<int>(5);
            var grandChild3 = new SimpleMultiTreeNode<int>(6);

            root.AddNode(child1);
            root.AddNode(child2);
            child1.AddNode(grandChild1);
            child1.AddNode(grandChild2);
            child2.AddNode(grandChild3);

            WriteLine("树结构:");
            WriteLine("    1");
            WriteLine("   / \\");
            WriteLine("  2   3");
            WriteLine(" / \\   \\");
            WriteLine("4   5   6");
            WriteLine();

            int[] expectedAll = [1, 2, 4, 5, 3, 6];
            int[] expectedFiltered = [1, 2, 4, 5, 3];
            int[] expectedFiltered2 = [1, 2, 3];

            WriteLine("=== 默认行为 (访问所有子节点) ===");
            var allNodes = root.PreorderNode().Select(n => n.NodeValue).ToArray();
            WriteLine($"执行结果: [{string.Join(", ", allNodes)}]");
            WriteLine($"预期结果: [{string.Join(", ", expectedAll)}]");
            WriteLine($"比较结果: {CompareResult(allNodes, expectedAll)}");
            WriteLine();

            WriteLine("=== 条件遍历: 只访问值 <= 2 的节点的子节点 ===");
            var filteredNodes = root.PreorderNode(n => n.NodeValue <= 2).Select(n => n.NodeValue).ToArray();
            WriteLine($"执行结果: [{string.Join(", ", filteredNodes)}]");
            WriteLine($"预期结果: [{string.Join(", ", expectedFiltered)}]");
            WriteLine($"比较结果: {CompareResult(filteredNodes, expectedFiltered)}");
            WriteLine("(节点 3 的子节点 6 被跳过)");
            WriteLine();

            WriteLine("=== 条件遍历: 只访问值 <= 1 的节点的子节点 ===");
            var filteredNodes2 = root.PreorderNode(n => n.NodeValue <= 1).Select(n => n.NodeValue).ToArray();
            WriteLine($"执行结果: [{string.Join(", ", filteredNodes2)}]");
            WriteLine($"预期结果: [{string.Join(", ", expectedFiltered2)}]");
            WriteLine($"比较结果: {CompareResult(filteredNodes2, expectedFiltered2)}");
            WriteLine("(节点 2 和 3 的子节点都被跳过)");
        }

        private static string CompareResult(int[] actual, int[] expected)
        {
            if (actual.Length != expected.Length)
            {
                return $"失败 (长度不一致: 实际 {actual.Length}, 预期 {expected.Length})";
            }
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != expected[i])
                {
                    return $"失败 (索引 {i} 处值不一致: 实际 {actual[i]}, 预期 {expected[i]})";
                }
            }
            return "通过";
        }
    }
}
