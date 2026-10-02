using ChaoticKit.Data.Structure.Tree;
using ChaoticKit.Data.Structure.Tree.Extensions;
using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Data.Structure.Value.Extensions;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.LayeringAddressCode003
    /// LayeringAddressCodeHelper.Random 随机生成编码 + AsMultiTree 多叉树转换 + ConcatRange 拼接的行为验证。
    /// 迁移要点: 原测试使用无种子 System.Random, 迁移后固定种子 new Random(Seed),
    /// 且因无法先行运行确定具体数值, 断言采用与种子无关的结构性质 (层数范围/层值来源/编码格式/包含关系/树完整性)。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class LayeringAddressCodeRandomTest : UnitTestBase
    {
        /// <summary>
        /// 固定随机种子, 保证生成结果确定
        /// </summary>
        private const int Seed = 20260927;

        /// <summary>
        /// 路径可用值 (非末端层值 / 范围编码的末端层值)
        /// </summary>
        private static readonly string[] PathValues = ["aaa", "bb", "c"];

        /// <summary>
        /// 项编码的路径末端可用值
        /// </summary>
        private static readonly string[] EndPointValues = ["1", "2", "3", "4", "5", "6", "7", "8"];

        /// <summary>
        /// 使用固定种子生成一组编码 (与源测试相同的参数: minDepth=2, maxDepth=4, minForkingCount=15, maxForkingCount=20, entryRangeProbability=0.95, rangeProbability=0.5)
        /// <para>Random 返回惰性 IEnumerable, 必须先 ToList 固化, 否则重复枚举会得到不同结果</para>
        /// </summary>
        private static List<ILayeringAddressCode<string>> GenerateCodes(int seed = Seed)
            => LayeringAddressCodeHelper.Random(PathValues, EndPointValues, 2, 4, 15, 20, 0.95, 0.5, random: new System.Random(seed)).ToList();

        /// <summary>
        /// 将编码转换为默认实现的 <see cref="LayeringAddressCode"/> 并输出标准格式字符串
        /// </summary>
        private static string ToCodeString(ILayeringAddressCode<string> code)
            => ((LayeringAddressCode)(code.IsRange, code.LayerValues)).ToString();

        /// <summary>
        /// 递归收集多叉树全部节点的值
        /// </summary>
        private static void CollectNodeValues(IMultiTreeNode<ILayeringAddressCode<string>>? node, List<ILayeringAddressCode<string>> output)
        {
            if (node == null) return;
            output.Add(node.NodeValue);
            foreach (var child in node.Childrens)
            {
                CollectNodeValues(child, output);
            }
        }

        [TestMethod]
        public void Random_固定种子生成结果确定()
        {
            Log("--- 测试: 固定种子生成结果确定 ---");
            var codes1 = GenerateCodes(Seed);
            var codes2 = GenerateCodes(Seed);

            string[] str1 = codes1.Select(ToCodeString).ToArray();
            string[] str2 = codes2.Select(ToCodeString).ToArray();

            Assert.AreEqual(str1.Length, str2.Length, $"相同种子两次生成的编码数量应一致。实际: {str1.Length} vs {str2.Length}");
            CollectionAssert.AreEqual(str1, str2, "相同种子两次生成的编码序列应完全一致");
            Log($"固定种子 {Seed} 生成 {codes1.Count} 个编码, 两次生成结果完全一致, 通过");
        }

        [TestMethod]
        public void Random_生成编码结构与层值合法()
        {
            Log("--- 测试: 随机生成编码的结构与层值合法性 ---");
            var codes = GenerateCodes(Seed);

            Log($"共生成 {codes.Count} 个编码:");
            for (int i = 0; i < codes.Count; i++)
            {
                Log($"  [{i}] {ToCodeString(codes[i])} (IsRange={codes[i].IsRange}, LayerCount={codes[i].LayerCount})");
            }

            // 最小层数要求为 2 (>1), 入口编码必为范围编码, 且至少生成一个子编码
            Assert.IsTrue(codes.Count >= 2, $"入口为范围编码时至少应有一个子编码, 实际数量: {codes.Count}");
            Assert.IsTrue(codes[0].IsRange, "入口编码应为范围编码");
            Assert.AreEqual(2, codes[0].LayerCount, "入口编码层数应为 minDepth=2");

            // 所有编码深度在 [minDepth, maxDepth] = [2, 4] 内
            foreach (var code in codes)
            {
                Assert.IsTrue(code.LayerCount >= 2 && code.LayerCount <= 4,
                    $"编码层数应在 [2, 4] 内, 实际: {ToCodeString(code)} LayerCount={code.LayerCount}");
            }

            // 层值来源: 非末端层值必须来自 pathValues; 末端层值: 范围编码来自 pathValues, 项编码来自 endPointValues
            foreach (var code in codes)
            {
                for (int i = 0; i < code.LayerCount - 1; i++)
                {
                    Assert.IsTrue(PathValues.Contains(code.LayerValues[i]),
                        $"非末端层值应来自 pathValues, 实际: [{i}]={code.LayerValues[i]} ({ToCodeString(code)})");
                }
                string last = code.LayerValues[^1];
                if (code.IsRange)
                {
                    Assert.IsTrue(PathValues.Contains(last),
                        $"范围编码末端层值应来自 pathValues, 实际: {last} ({ToCodeString(code)})");
                }
                else
                {
                    Assert.IsTrue(EndPointValues.Contains(last),
                        $"项编码末端层值应来自 endPointValues, 实际: {last} ({ToCodeString(code)})");
                }
            }

            // 不应出现重复编码
            Assert.AreEqual(codes.Count, codes.Select(ToCodeString).Distinct().Count(), "生成的编码不应有重复");

            // 首个编码(范围编码)应包含其余所有编码 (入口范围覆盖整组编码)
            for (int i = 1; i < codes.Count; i++)
            {
                Assert.IsTrue(codes[i].IsIn(codes[0]),
                    $"第 {i} 个编码应位于入口范围编码之内: {ToCodeString(codes[i])}");
            }

            Log("结构/层值来源/去重/包含关系全部通过");
        }

        [TestMethod]
        public void Random_生成编码可转换为完整多叉树()
        {
            Log("--- 测试: 生成编码转换为多叉树 ---");
            var codes = GenerateCodes(Seed);

            // AsMultiTree: 生成集合符合多叉树完整结构, 转换时无需补充范围编码
            // → 树节点数 = 编码数 + 1 (根), 且每个编码都能在树中找到对应节点
            var tree = codes.AsMultiTree();
            Assert.IsNotNull(tree.Root, "多叉树根节点不应为空");
            Assert.IsTrue(tree.Root!.NodeValue.IsAll(), "根节点值应为完全范围 (All)");

            var nodeValues = new List<ILayeringAddressCode<string>>();
            CollectNodeValues(tree.Root, nodeValues);

            // AsMultiTree 转换时可能补充范围节点 (固定种子下实际节点数 = 编码数 + 1(根) + 补充数),
            // 因此断言节点数 >= 编码数+1, 且每个编码都能在树中找到对应节点。
            Assert.IsTrue(nodeValues.Count >= codes.Count + 1,
                $"树节点数应 >= 编码数+1(根)。实际节点数: {nodeValues.Count}, 编码数: {codes.Count}");
            foreach (var code in codes)
            {
                Assert.IsTrue(nodeValues.Any(nodeValue => nodeValue.Equals(code)),
                    $"树中应包含生成的编码: {ToCodeString(code)}");
            }

            // ToGeneralTree + GetSimpleTreeString: 根显示 ALL!, 各节点显示其路径末端值
            string treeString = tree
                .ToGeneralTree(node => (LayeringAddressCode)(node.NodeValue.IsRange, node.NodeValue.LayerValues))
                .GetSimpleTreeString(
                    value => value.IsAll() ? "ALL!" : value.Endpoint(),
                    scope => scope.IsAll() ? "ALL!" : scope.Endpoint());
            Log($"多叉树字符串:\n{treeString}");

            Assert.IsFalse(string.IsNullOrEmpty(treeString), "树字符串不应为空");
            Assert.IsTrue(treeString.Contains("ALL!"), "树字符串应包含根节点标记 ALL!");
            foreach (string endpoint in codes.Select(code => code.Endpoint()).Distinct())
            {
                Assert.IsTrue(treeString.Contains(endpoint),
                    $"树字符串应包含路径末端值: {endpoint}");
            }
            Log("多叉树转换与树字符串输出通过");
        }

        [TestMethod]
        public void ConcatRange_拼接范围编码保持范围标志与层值()
        {
            Log("--- 测试: ConcatRange 拼接范围编码 ---");
            // 注意: 原控制台测试未固化 codes1 就重复枚举 (惰性 IEnumerable 会以同一 Random 重新生成随机值),
            // 迁移后先 ToList 固化再拼接, 保证确定性
            var codes1 = GenerateCodes(Seed);
            Log($"codes 1 ({codes1.Count} 个):");
            for (int i = 0; i < codes1.Count; i++)
            {
                Log($"  [{i}] {ToCodeString(codes1[i])}");
            }

            // "qqq.xx" 无项标记 ':', 解析为 2 层的范围编码
            var codes2 = codes1.ConcatRange((LayeringAddressCode)"qqq.xx").ToList();
            Log($"codes 2 ({codes2.Count} 个):");
            for (int i = 0; i < codes2.Count; i++)
            {
                Log($"  [{i}] {ToCodeString(codes2[i])}");
            }

            Assert.AreEqual(codes1.Count, codes2.Count, "拼接后编码数量应与原集合一致");
            for (int i = 0; i < codes1.Count; i++)
            {
                var original = codes1[i];
                var combined = codes2[i];

                Assert.AreEqual(original.IsRange, combined.IsRange, $"[{i}] 范围标志应保持不变");
                Assert.AreEqual(original.LayerCount + 2, combined.LayerCount,
                    $"[{i}] 拼接后层数应为原层数 + 2 (范围编码 \"qqq.xx\" 为 2 层)");
                Assert.AreEqual("qqq", combined.LayerValues[0], $"[{i}] 拼接前缀第 1 层应为 qqq");
                Assert.AreEqual("xx", combined.LayerValues[1], $"[{i}] 拼接前缀第 2 层应为 xx");
                CollectionAssert.AreEqual(original.LayerValues, combined.LayerValues[2..],
                    $"[{i}] 拼接后的原层值部分应保持顺序不变");

                // 编码格式: 范围编码用 '.' 连接且不含 ':', 项编码在末端值前有 ':'
                string str = ToCodeString(combined);
                if (combined.IsRange)
                {
                    Assert.IsFalse(str.Contains(':'), $"[{i}] 范围编码字符串不应含 ':' : {str}");
                    string[] parts = str.Split('.');
                    Assert.AreEqual(combined.LayerCount, parts.Length, $"[{i}] 范围编码字符串层级数不匹配: {str}");
                    CollectionAssert.AreEqual(combined.LayerValues, parts, $"[{i}] 范围编码字符串层值与层值数组不匹配: {str}");
                }
                else
                {
                    string[] parts = str.Split(':');
                    Assert.AreEqual(2, parts.Length, $"[{i}] 项编码字符串应恰好含一个 ':' : {str}");
                    Assert.AreEqual(combined.LayerValues[^1], parts[1], $"[{i}] ':' 后应为路径末端值: {str}");
                    string[] prefixParts = parts[0].Split('.');
                    CollectionAssert.AreEqual(combined.LayerValues[..^1], prefixParts, $"[{i}] ':' 前的路径部分不匹配: {str}");
                }
            }
            Log("ConcatRange 拼接结果全部通过");
        }
    }
}
