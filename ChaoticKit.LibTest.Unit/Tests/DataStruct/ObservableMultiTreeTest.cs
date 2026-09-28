using ChaoticKit.Data.Struct;
using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Data.Structure.Value.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree003
    /// 验证 ObservableMultiTree&lt;LayeringAddressCode&gt; 的 Add / FindNode 行为:
    /// 固定编码列表添加后构建多叉树, 并查找项编码 / 范围编码 / 未命中编码
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class ObservableMultiTreeTest : UnitTestBase
    {
        /// <summary>
        /// 固定的待添加编码列表, 与原控制台测试 Tree003 的输入保持一致
        /// </summary>
        private static readonly string[] FixedCodes =
        {
            "aa.bb:1", "aa.bb:2", "aa.bb:3",
            "aa.b1.a1:1", "aa.b1.a1:2", "aa.b1.a1:3",
            "aa.b1.a2:1", "aa.b1.a2:2", "aa.b1.a2:3",
            "aa.b1.a3:1", "aa.b1.a3:2", "aa.b1.a3:3",
        };

        /// <summary>
        /// 构建与原控制台测试相同的测试树:
        /// 根节点为范围编码 "aa", 随后依次添加全部固定编码 (项编码会自动创建中间范围节点)
        /// </summary>
        private ObservableMultiTree<LayeringAddressCode> CreateTestTree()
        {
            ObservableMultiTreeNode<LayeringAddressCode> rootNode = new(null, "aa");
            ObservableMultiTree<LayeringAddressCode> tree = new();
            tree.SetRootNode(rootNode);

            foreach (string code in FixedCodes)
            {
                var addResult = tree.Add<string, LayeringAddressCode>(
                    (LayeringAddressCode)code, LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
                Assert.IsTrue(addResult.IsSuccess, $"添加 {code} 应成功, 实际: {addResult}");
                Log($"添加 {code} 成功: {addResult}");
            }
            return tree;
        }

        /// <summary>
        /// 在树上查找指定编码的节点, 并记录查找结果 (与原控制台测试 find 方法的输出对应)
        /// </summary>
        private IOperationResultEx<ObservableMultiTreeNode<LayeringAddressCode>> FindNodeInTree(ObservableMultiTree<LayeringAddressCode> tree, string code)
        {
            var findResult = tree.FindNode<string, LayeringAddressCode, ObservableMultiTreeNode<LayeringAddressCode>, ObservableMultiTree<LayeringAddressCode>>((LayeringAddressCode)code);
            Log($"尝试查找节点: {code} => {findResult}, Data={findResult.Data?.NodeValue.ToString() ?? "<null>"}");
            return findResult;
        }

        /// <summary>
        /// 将编码转换为字符串: null => "&lt;null&gt;", 完全范围 => "&lt;ALL&gt;", 否则按层级拼接 (范围以 '.' 结尾, 项以 ':' 结尾)
        /// </summary>
        private string CodeToString(ILayeringAddressCode<string>? code)
        {
            if (code == null)
            {
                return "<null>";
            }
            if (code.IsAll())
            {
                return "<ALL>";
            }
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string str in code.LayerValues)
            {
                sb.Append(str);
                index++;
                if (index < code.LayerCount - 1)
                {
                    sb.Append('.');
                }
                else if (index == code.LayerCount - 1)
                {
                    sb.Append(code.IsRange ? '.' : ':');
                }
            }
            return sb.ToString();
        }

        [TestMethod]
        public void Add_固定编码列表_全部添加成功()
        {
            Log("--- 测试: 固定编码列表全部添加成功 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            // 总节点数: 根 "aa" + 范围 "aa.bb"/"aa.b1" + 范围 "aa.b1.a1"/"a2"/"a3" + 12 个项 = 18
            Assert.AreEqual(18, tree.Count(),
                "树应包含根节点、中间范围节点与全部固定编码项节点 (共 18 个)");
            Log($"树节点总数: {tree.Count()}, 通过");
        }

        [TestMethod]
        public void Add_构建的树_节点数量与结构正确()
        {
            Log("--- 测试: 添加后树的节点数量与结构 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            // 总节点数: 根 "aa" 1, 范围 "aa.bb" 1, "aa.b1" 1, "aa.b1.a1/a2/a3" 3, 项 12 => 18
            Assert.AreEqual(18, tree.Count(), $"树节点总数应为 18, 实际: {tree.Count()}");

            ObservableMultiTreeNode<LayeringAddressCode>? root = tree.Root;
            Assert.IsNotNull(root, "树应拥有根节点");
            Assert.AreEqual("aa", root.NodeValue.ToString(), "根节点值应为范围编码 aa");
            Assert.IsTrue(root.NodeValue.IsRange, "根节点应为范围编码");
            Assert.AreEqual(1, root.NodeValue.LayerCount, "根节点应只有 1 层");

            Assert.AreEqual(2, root.Childrens.Count, "根节点下应有 aa.bb 与 aa.b1 两个范围子节点");

            ObservableMultiTreeNode<LayeringAddressCode> bb = root.Childrens.Single(n => n.NodeValue.ToString() == "aa.bb");
            Assert.AreEqual(3, bb.Childrens.Count, "aa.bb 下应有 3 个项节点");
            CollectionAssert.AreEqual(
                new[] { "aa.bb:1", "aa.bb:2", "aa.bb:3" },
                bb.Childrens.Select(n => n.NodeValue.ToString()).ToArray(),
                "aa.bb 下的项编码序列不匹配");

            ObservableMultiTreeNode<LayeringAddressCode> b1 = root.Childrens.Single(n => n.NodeValue.ToString() == "aa.b1");
            Assert.AreEqual(3, b1.Childrens.Count, "aa.b1 下应有 a1/a2/a3 三个范围子节点");
            foreach (var sub in b1.Childrens)
            {
                Assert.IsTrue(sub.NodeValue.IsRange, $"节点 {sub.NodeValue} 应为范围编码");
                Assert.AreEqual(3, sub.Childrens.Count, $"节点 {sub.NodeValue} 下应有 3 个项节点");
                Log($"节点 {sub.NodeValue} 下 3 个项: {string.Join(", ", sub.Childrens.Select(n => n.NodeValue.ToString()))}");
            }
            Log("树结构验证通过");
        }

        [TestMethod]
        public void ToGeneralTree_树字符串_包含全部节点编码()
        {
            Log("--- 测试: 转换为 GeneralTree 并输出简易树字符串 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            var tree2 = tree.ToGeneralTree(node => node.NodeValue);
            string treeString = tree2.GetSimpleTreeString(
                getScopeStringFunc: code => CodeToString(code),
                getValueStringFunc: code => CodeToString(code));

            Log("简易树字符串信息:");
            Log(treeString);

            foreach (string code in FixedCodes)
            {
                Assert.IsTrue(treeString.Contains(code), $"树字符串应包含节点编码 {code}");
            }
            Assert.IsTrue(treeString.Contains("aa.bb"), "树字符串应包含范围节点 aa.bb");
            Assert.IsTrue(treeString.Contains("aa.b1"), "树字符串应包含范围节点 aa.b1");
            Log($"树字符串包含全部 {FixedCodes.Length} 个项编码与中间范围编码, 通过");
        }

        [TestMethod]
        public void FindNode_命中项编码_返回节点()
        {
            Log("--- 测试: 查找已添加的项编码 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            var findResult = FindNodeInTree(tree, "aa.b1.a3:2");
            Assert.IsTrue(findResult.IsSuccess, "查找 aa.b1.a3:2 应成功");
            Assert.IsNotNull(findResult.Data, "命中时 Data 不应为 null");
            Assert.AreEqual("aa.b1.a3:2", findResult.Data.NodeValue.ToString(), "命中的节点值应为 aa.b1.a3:2");
            Assert.IsFalse(findResult.Data.NodeValue.IsRange, "aa.b1.a3:2 应为项编码");
            Assert.AreEqual(4, findResult.Data.NodeValue.LayerCount, "aa.b1.a3:2 应有 4 层");
            Log("命中 aa.b1.a3:2, 通过");

            // 原控制台测试对同一编码查找两次, 保持该检查点
            var findResult2 = FindNodeInTree(tree, "aa.b1.a3:2");
            Assert.IsTrue(findResult2.IsSuccess, "重复查找 aa.b1.a3:2 应仍成功");
            Assert.AreSame(findResult.Data, findResult2.Data, "重复查找应命中同一节点实例");
            Log("重复查找命中同一节点, 通过");
        }

        [TestMethod]
        public void FindNode_命中范围编码_返回节点()
        {
            Log("--- 测试: 查找已添加的范围编码 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            var findResult = FindNodeInTree(tree, "aa.b1.a3");
            Assert.IsTrue(findResult.IsSuccess, "查找 aa.b1.a3 应成功");
            Assert.IsNotNull(findResult.Data, "命中时 Data 不应为 null");
            Assert.AreEqual("aa.b1.a3", findResult.Data.NodeValue.ToString(), "命中的节点值应为 aa.b1.a3");
            Assert.IsTrue(findResult.Data.NodeValue.IsRange, "aa.b1.a3 应为范围编码");
            Assert.AreEqual(3, findResult.Data.NodeValue.LayerCount, "aa.b1.a3 应有 3 层");
            Log("命中范围编码 aa.b1.a3, 通过");
        }

        [DataTestMethod]
        [DataRow("aa.b1.a3:4", "未找到")]          // 范围 aa.b1.a3 下不存在项 4
        [DataRow("b1.a3:4", "根节点之前")]          // 编码不属于根节点 "aa" 的分支
        public void FindNode_未命中_返回失败(string code, string expectedReasonPart)
        {
            Log($"--- 测试: 查找未添加的编码 {code} 应失败 ---");
            ObservableMultiTree<LayeringAddressCode> tree = CreateTestTree();

            var findResult = FindNodeInTree(tree, code);
            Assert.IsTrue(findResult.IsFailure, $"查找 {code} 应失败, 实际: {findResult}");
            Assert.IsNull(findResult.Data, $"查找 {code} 失败时 Data 应为 null");
            Assert.IsNotNull(findResult.FailureReason, "失败时应带有失败原因");
            Assert.IsTrue(findResult.FailureReason.Contains(expectedReasonPart),
                $"失败原因应包含 [{expectedReasonPart}], 实际: {findResult.FailureReason}");
            Log($"查找 {code} 返回失败, 原因: {findResult.FailureReason}, 通过");
        }
    }
}
