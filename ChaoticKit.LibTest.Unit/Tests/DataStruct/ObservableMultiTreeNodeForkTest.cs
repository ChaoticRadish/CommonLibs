using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;
using ChaoticKit.Data.Structure.Value;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Tree002 (测试 1~5)
    /// ObservableMultiTreeNode.CheckCodeTreeFork 分叉检测结果 (CodeTreeForkCheckResultEnum) 的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class ObservableMultiTreeNodeForkTest : UnitTestBase
    {
        /// <summary>
        /// 执行分叉检测并断言结果与预期枚举值一致
        /// </summary>
        private void AssertForkResult(
            string testName,
            ObservableMultiTreeNode<LayeringAddressCode> node,
            IMultiTreeExtensions.CodeTreeForkCheckResultEnum expected)
        {
            var result = node.CheckCodeTreeFork<string, LayeringAddressCode>();
            Log($"[{testName}] CheckCodeTreeFork 结果: {result} ({(int)result})");
            Assert.AreEqual(expected, result, $"[{testName}] 分叉检测结果不匹配。预期: {expected} ({(int)expected}), 实际: {result} ({(int)result})");
        }

        [TestMethod]
        public void Test1_同层两个等价项编码_报缺失层与重复节点值()
        {
            Log("--- 测试 1: 根 aa 下两个等价项编码 aa.bb:1 ---");
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa.bb:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb:1");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            AssertForkResult(
                "测试 1",
                testNode1,
                IMultiTreeExtensions.CodeTreeForkCheckResultEnum.MissingLayer
                    | IMultiTreeExtensions.CodeTreeForkCheckResultEnum.RepeatNodeValue);
            Log("[测试 1] 通过: 父节点 aa 与子项前一级范围 aa.bb 不一致 => MissingLayer, 同层两个 bb:1 等价 => RepeatNodeValue");
        }

        [TestMethod]
        public void Test2_同层不同末端值的项编码_仅报缺失层()
        {
            Log("--- 测试 2: 根 aa 下项编码 aa.bb.cc:1 与 aa.bb.cc:2 ---");
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa.bb.cc:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb.cc:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            AssertForkResult(
                "测试 2",
                testNode1,
                IMultiTreeExtensions.CodeTreeForkCheckResultEnum.MissingLayer);
            Log("[测试 2] 通过: 缺 bb.cc 两层 => MissingLayer, 同层末端值 1/2 不同 => 无 RepeatNodeValue, 均在 aa 范围内 => 无 ErrorStruct");
        }

        [TestMethod]
        public void Test3_子项不在父节点范围内_报缺失层与结构错误()
        {
            Log("--- 测试 3: 根 aa 下 cc:1 与 cc.2:a (超出 aa 范围) ---");
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "cc:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "cc.2:a");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            AssertForkResult(
                "测试 3",
                testNode1,
                IMultiTreeExtensions.CodeTreeForkCheckResultEnum.MissingLayer
                    | IMultiTreeExtensions.CodeTreeForkCheckResultEnum.ErrorStruct);
            Log("[测试 3] 通过: 缺 aa 层 => MissingLayer, 子项 cc:* 不属于父范围 aa => ErrorStruct");
        }

        [TestMethod]
        public void Test4_混合深度项编码_仅报缺失层()
        {
            Log("--- 测试 4: 根 aa 下 aa:1 与 aa.bb:2 ---");
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            AssertForkResult(
                "测试 4",
                testNode1,
                IMultiTreeExtensions.CodeTreeForkCheckResultEnum.MissingLayer);
            Log("[测试 4] 通过: aa:1 层级完整, aa.bb:2 缺 bb 层 => MissingLayer; 两者均在 aa 范围内 => 无 ErrorStruct, 末端值 1/2 不同 => 无 RepeatNodeValue");
        }

        [TestMethod]
        public void Test5_完整分层分支_结果为Full()
        {
            Log("--- 测试 5: 完整分层分支 aa / aa:1, aa.bb / aa.bb:2 ---");
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb");
            ObservableMultiTreeNode<LayeringAddressCode> testNode4 = new(testNode3, "aa.bb:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);
            testNode3.Childrens.Add(testNode4);

            var checkResult = testNode1.CheckCodeTreeFork<string, LayeringAddressCode>();
            Log($"[测试 5] CheckCodeTreeFork 结果: {checkResult} ({(int)checkResult})");

            AssertForkResult("测试 5", testNode1, IMultiTreeExtensions.CodeTreeForkCheckResultEnum.Full);
            Assert.IsTrue(
                checkResult == IMultiTreeExtensions.CodeTreeForkCheckResultEnum.Full,
                "[测试 5] checkResult == Full 应为 true");
            Assert.IsFalse(checkResult > 0, "[测试 5] checkResult > 0 应为 false (Full=0)");
            Log("[测试 5] 通过: 每一层均完整 (aa => aa:1 / aa.bb => aa.bb:2), 无任何缺陷, 结果为 Full");
        }
    }
}
