using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree006
    /// PreorderNode 条件遍历的行为验证 (ChaoticKit.Data.Structure.Tree.Extensions)
    /// 固定树 + 内置 expected 数组, 天然适合直接断言
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class PreorderNodeTest : UnitTestBase
    {
        /// <summary>
        /// 构造固定测试树:
        /// <code>
        ///     1
        ///    / \
        ///   2   3
        ///  / \   \
        /// 4   5   6
        /// </code>
        /// </summary>
        private static SimpleMultiTreeNode<int> BuildTestTree()
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
            return root;
        }

        /// <summary>
        /// 输出树结构到测试面板 (VS 测试资源管理器 -> 选中测试 -> 输出)
        /// </summary>
        private void LogTreeStructure()
        {
            Log("树结构:");
            Log("    1");
            Log("   / \\");
            Log("  2   3");
            Log(" / \\   \\");
            Log("4   5   6");
        }

        /// <summary>
        /// 默认行为: PreorderNode() 无谓词调用, 应访问所有子节点
        /// (覆盖扩展方法内部 shouldVisitChildren=null 走 "访问全部" 的分支)
        /// </summary>
        [TestMethod]
        public void PreorderNode_默认行为_访问所有子节点()
        {
            Log("--- 测试: 默认行为 (无谓词, 访问所有子节点) ---");
            var root = BuildTestTree();
            LogTreeStructure();

            var allNodes = root.PreorderNode().Select(n => n.NodeValue).ToArray();
            int[] expected = [1, 2, 4, 5, 3, 6];

            Log($"执行结果: [{string.Join(", ", allNodes)}]");
            Log($"预期结果: [{string.Join(", ", expected)}]");

            CollectionAssert.AreEqual(expected, allNodes, "[默认行为] 先根遍历结果与预期不一致。");
            Log("[默认行为] 通过。");
        }

        /// <summary>
        /// 条件遍历: 传入谓词 (n.NodeValue &lt;= maxValue), 谓词返回 false 的节点的子节点被跳过
        /// </summary>
        /// <param name="maxValue">谓词阈值: 只访问值 &lt;= maxValue 的节点的子节点</param>
        /// <param name="expected">预期的先根遍历值序列</param>
        /// <param name="scenario">场景说明, 用于日志</param>
        [DataTestMethod]
        [DataRow(2, new int[] { 1, 2, 4, 5, 3 }, "只访问值<=2的节点的子节点 (节点3的子节点6被跳过)")]
        [DataRow(1, new int[] { 1, 2, 3 }, "只访问值<=1的节点的子节点 (节点2和3的子节点都被跳过)")]
        public void PreorderNode_条件遍历_按谓词跳过子节点(int maxValue, int[] expected, string scenario)
        {
            Log($"--- 测试: 条件遍历: {scenario} ---");
            var root = BuildTestTree();

            var filteredNodes = root.PreorderNode(n => n.NodeValue <= maxValue).Select(n => n.NodeValue).ToArray();

            Log($"执行结果: [{string.Join(", ", filteredNodes)}]");
            Log($"预期结果: [{string.Join(", ", expected)}]");

            CollectionAssert.AreEqual(expected, filteredNodes, $"[条件遍历 maxValue={maxValue}] 先根遍历结果与预期不一致。");
            Log($"[条件遍历 maxValue={maxValue}] 通过。");
        }
    }
}
