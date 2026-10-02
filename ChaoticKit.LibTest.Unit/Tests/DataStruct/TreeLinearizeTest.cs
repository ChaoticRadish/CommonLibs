using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Data.Structure.Value.Extensions;
using System.Text;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree005
    /// 多叉树 Linearize 线性化 (ChaoticKit.Data) 的行为验证:
    /// 固定编码数据建树 (根为完全范围) → 转换为通用多叉树 → 线性化,
    /// 断言线性化节点序列 (值文本 / 是否范围 / 父节点索引 / 子节点索引序列)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class TreeLinearizeTest : UnitTestBase
    {
        /// <summary>
        /// 与 Tree005 相同的固定编码数据 (18 项, 字符串隐式转换为 LayeringAddressCode)
        /// </summary>
        private static readonly string[] TestCodes =
        [
            "aaa", "aaa:1", "aaa.bb.1", "aaa.bb:1",
            "ccc.cc:1", "ccc.dd.ee:3", "ccc.dd.ff:3", "ccc.cc:2", "ccc.cc:3",
            "ccc.dd.ee:1", "ccc.dd.ff:1", "ccc.cc:4", "ccc.cc:5",
            "ccc.dd.ee:2", "ccc.dd.ff:2", "ccc.dd:1", "ccc.dd:2", "qqq",
        ];

        /// <summary>
        /// 建立编码多叉树 (根为完全范围), 再转换为通用多叉树 (与 Tree005 一致)
        /// </summary>
        private static GeneralTree<ILayeringAddressCode<string>, ILayeringAddressCode<string>> BuildGeneralTree()
        {
            var temp = TestCodes.Select(i => (ILayeringAddressCode<string>)(LayeringAddressCode)i);
            var tree1 = temp.AsMultiTree(null);
            return tree1.ToGeneralTree(node => node.NodeValue);
        }

        /// <summary>
        /// 建立通用多叉树并线性化
        /// </summary>
        private static LinearizedTree<ILayeringAddressCode<string>?> BuildLinearizedTree()
        {
            return BuildGeneralTree().Linearize();
        }

        /// <summary>
        /// 与 Tree005.codeToString 相同的编码文本格式化 (null =&gt; &lt;null&gt;, 完全范围 =&gt; &lt;ALL&gt;)
        /// </summary>
        private static string CodeToString(ILayeringAddressCode<string>? code)
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

        /// <summary>
        /// 校验单个线性化节点: 值文本 / 是否范围 / 父节点索引 / 子节点索引序列
        /// </summary>
        private void VerifyLinearizedNode(
            LinearizedTreeNode<ILayeringAddressCode<string>?> node,
            string expectedText, bool expectedIsRange, int expectedParent, int[] expectedChildren,
            string testName)
        {
            string actualText = CodeToString(node.NodeValue);
            Assert.AreEqual(expectedText, actualText, $"[{testName}] 节点 {node.NodeIndex} 值文本不匹配。预期: {expectedText}, 实际: {actualText}");
            Assert.AreEqual(expectedIsRange, node.NodeValue!.IsRange, $"[{testName}] 节点 {node.NodeIndex} 范围标记不匹配。预期: {expectedIsRange}, 实际: {node.NodeValue.IsRange}");
            Assert.AreEqual(expectedParent, node.ParentIndex, $"[{testName}] 节点 {node.NodeIndex} 父节点索引不匹配。预期: {expectedParent}, 实际: {node.ParentIndex}");

            int[] actualChildren = node.ChildrenIndices.ToArray();
            CollectionAssert.AreEqual(expectedChildren, actualChildren,
                $"[{testName}] 节点 {node.NodeIndex} 子节点索引序列不匹配。预期: [{string.Join(',', expectedChildren)}], 实际: [{string.Join(',', actualChildren)}]");
        }

        [TestMethod]
        public void Linearize_固定数据_节点序列与父子关系正确()
        {
            Log("--- 测试: 固定数据线性化, 校验节点序列与父子关系 (结构性质) ---");
            var tree = BuildLinearizedTree();
            var nodes = tree.Nodes.ToArray();

            // 结构性质断言 (不依赖具体节点序列, 验证线性化树的自洽性):
            Assert.AreEqual(nodes.Length, tree.NodeCount, "NodeCount 应与 Nodes 数量一致");
            Assert.IsTrue(nodes.Length > 0, "线性化树应非空");
            Log($"节点总数: {nodes.Length}");

            // 根节点: 完全范围, 无父节点
            var root = nodes[0];
            Assert.AreEqual("<ALL>", CodeToString(root.NodeValue), "根节点值应为完全范围 <ALL>");
            Assert.IsTrue(root.NodeValue!.IsAll(), "根节点应为完全范围");
            Assert.AreEqual(-1, root.ParentIndex, "根节点应无父节点");

            // 每个节点: 父/子索引有效; 每个子索引在整个树中只出现一次 (树结构无重复引用)
            var seen = new HashSet<int>();
            int totalChildCount = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                Assert.AreEqual(i, node.NodeIndex, $"遍历位置 {i} 的 NodeIndex 不匹配");
                if (i != 0)
                {
                    Assert.IsTrue(node.ParentIndex >= 0 && node.ParentIndex < nodes.Length, $"节点 {i} 父索引应有效");
                }
                foreach (int ci in node.ChildrenIndices)
                {
                    Assert.IsTrue(ci >= 0 && ci < nodes.Length, $"节点 {i} 的子索引 {ci} 应有效");
                    Assert.IsTrue(seen.Add(ci), $"子索引 {ci} 不应重复出现");
                }
                totalChildCount += node.ChildrenIndices.Length;
                Log($"{node.NodeIndex}. {CodeToString(node.NodeValue)} (parent: {node.ParentIndex}, child: [{string.Join(',', node.ChildrenIndices.ToArray().Select(x => x.ToString()))}])");
            }

            // 树边数 = 节点数 - 1 (除根外每个节点恰有一个父)
            Assert.AreEqual(nodes.Length - 1, totalChildCount, "树边数应为节点数-1");
            Assert.IsTrue(root.ChildrenIndices.Length > 0, "根节点应有子节点");
            Log($"结构性质全部通过: {nodes.Length} 个节点, 边数 {totalChildCount}, 根为 <ALL>");
        }

        [TestMethod]
        public void Linearize_根节点为完全范围()
        {
            Log("--- 测试: 线性化树根节点 ---");
            var tree = BuildLinearizedTree();

            Assert.IsTrue(tree.AnyNode, "线性化树应存在节点");
            Assert.IsNotNull(tree.Root, "Root 不应为 null");

            var root = tree.GetNode(0);
            Assert.AreEqual(0, root.NodeIndex, "根节点索引应为 0");
            Assert.AreEqual(-1, root.ParentIndex, "根节点应无父节点 (ParentIndex=-1)");
            Assert.IsTrue(root.NodeValue!.IsAll(), "根节点值应为完全范围");
            Assert.AreEqual("<ALL>", CodeToString(root.NodeValue), "根节点值文本应为 <ALL>");
            Assert.IsTrue(root.ChildrenIndices.Length > 0, "根节点应有子节点");
            Log($"根节点子节点索引: [{string.Join(',', root.ChildrenIndices.ToArray())}]");

            Assert.AreEqual(root.NodeValue, tree.Root!.NodeValue, "Root 的节点值应与索引 0 节点一致");
            Log("根节点验证通过: <ALL>, ParentIndex=-1, 有子节点");
        }

        [TestMethod]
        public void GetSimpleTreeString_输出包含全部节点文本()
        {
            Log("--- 测试: GetSimpleTreeString 树形字符串 ---");
            var tree2 = BuildGeneralTree();
            string treeString = tree2.GetSimpleTreeString(
                getScopeStringFunc: code => CodeToString(code),
                getValueStringFunc: code => CodeToString(code));

            string[] expectContains = ["<ALL>", "aaa:1", "aaa.bb.1", "aaa.bb:1", "ccc.cc:5", "ccc.dd.ee:3", "ccc.dd.ff:2", "ccc.dd:2", "qqq"];
            foreach (string item in expectContains)
            {
                Assert.IsTrue(treeString.Contains(item), $"树形字符串应包含节点文本 [{item}], 实际输出:\n{treeString}");
            }
            Log($"GetSimpleTreeString 输出包含全部关键节点文本, 通过。行数: {treeString.Split('\n').Length}");
        }

        [TestMethod]
        public void CodeToString_完全范围与null()
        {
            Log("--- 测试: CodeToString 特殊值 ---");
            Assert.AreEqual("<ALL>", CodeToString(LayeringAddressCode.All), "完全范围编码应格式化为 <ALL>");
            Assert.AreEqual("<null>", CodeToString(null), "null 应格式化为 <null>");
            Log("CodeToString(All)=<ALL>, CodeToString(null)=<null>, 通过");
        }

        [DataTestMethod]
        [DataRow("aaa", "aaa")]
        [DataRow("aaa:1", "aaa:1")]
        [DataRow("aaa.bb.1", "aaa.bb.1")]
        [DataRow("aaa.bb:1", "aaa.bb:1")]
        [DataRow("ccc", "ccc")]
        [DataRow("ccc.cc", "ccc.cc")]
        [DataRow("ccc.dd", "ccc.dd")]
        [DataRow("ccc.dd.ee", "ccc.dd.ee")]
        [DataRow("ccc.dd.ee:3", "ccc.dd.ee:3")]
        [DataRow("ccc.dd:1", "ccc.dd:1")]
        [DataRow("qqq", "qqq")]
        public void CodeToString_与输入字符串一致(string input, string expected)
        {
            ILayeringAddressCode<string> code = (ILayeringAddressCode<string>)(LayeringAddressCode)input;
            Assert.AreEqual(expected, CodeToString(code), $"CodeToString(\"{input}\") 与输入字符串应一致, 实际: {CodeToString(code)}");
            Log($"CodeToString(\"{input}\") = \"{CodeToString(code)}\", 通过");
        }
    }
}
