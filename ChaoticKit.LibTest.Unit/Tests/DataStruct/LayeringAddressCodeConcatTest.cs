using ChaoticKit.Data.Structure.Value;
using ChaoticKit.Data.Structure.Value.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.LayeringAddressCode002
    /// 验证 LayeringAddressCode 固定编码列表经 ConcatRange 扩展 (范围编码在前) 拼接后的结果
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class LayeringAddressCodeConcatTest : UnitTestBase
    {
        /// <summary>
        /// 固定的编码列表 (含空编码、范围编码、项编码、多层级项编码等)
        /// 与原控制台测试的输入保持一致
        /// </summary>
        private static readonly string[] FixedCodes =
        {
            "a",
            "",
            ":b",
            "aaa.bb.cc",
            "aaa.bb:1",
            "aaa.bb:2",
            "aaa.bb.cc:1",
            "aaa.bb.cc:2",
            "aaa.dd:123",
            "aaa.dd.ee:123",
            "aaa.bb.cc:3",
            "aaa.dd:1234",
            "aaa.dd.ee:1234",
        };

        [DataTestMethod]
        [DataRow("a", "wu.a", true)]
        [DataRow("", "wu", true)]
        [DataRow(":b", "wu.:b", false)]
        [DataRow("aaa.bb.cc", "wu.aaa.bb.cc", true)]
        [DataRow("aaa.bb:1", "wu.aaa.bb:1", false)]
        [DataRow("aaa.bb:2", "wu.aaa.bb:2", false)]
        [DataRow("aaa.bb.cc:1", "wu.aaa.bb.cc:1", false)]
        [DataRow("aaa.bb.cc:2", "wu.aaa.bb.cc:2", false)]
        [DataRow("aaa.dd:123", "wu.aaa.dd:123", false)]
        [DataRow("aaa.dd.ee:123", "wu.aaa.dd.ee:123", false)]
        [DataRow("aaa.bb.cc:3", "wu.aaa.bb.cc:3", false)]
        [DataRow("aaa.dd:1234", "wu.aaa.dd:1234", false)]
        [DataRow("aaa.dd.ee:1234", "wu.aaa.dd.ee:1234", false)]
        public void ConcatRange_单条编码拼接结果(string input, string expectedFormat, bool expectedIsRange)
        {
            Log($"--- 测试: ConcatRange(\"wu\") 拼接 [{input}] ---");
            LayeringAddressCode code = input;
            LayeringAddressCode range = "wu";

            var newCode = new List<LayeringAddressCode> { code }.ConcatRange(range).Single();

            string actual = newCode.ToDefaultFormatString();
            Assert.AreEqual(expectedFormat, actual, $"拼接 [{input}] 的结果应为 [{expectedFormat}], 实际: [{actual}]");

            Assert.AreEqual(expectedIsRange, newCode.IsRange,
                $"拼接 [{input}] 的结果 IsRange 应为 {expectedIsRange}, 实际: {newCode.IsRange}");
            Log($"拼接 [{input}] => [{actual}] (IsRange={newCode.IsRange}), 通过");
        }

        [TestMethod]
        public void ConcatRange_整个固定列表_数量与顺序()
        {
            Log("--- 测试: 整个固定编码列表 ConcatRange(\"wu\") 拼接结果 ---");
            List<LayeringAddressCode> codes = FixedCodes.Select(s => (LayeringAddressCode)s).ToList();
            LayeringAddressCode range = "wu";

            var newCodes = codes.ConcatRange(range).ToArray();
            string[] actual = newCodes.Select(c => c.ToDefaultFormatString()).ToArray();

            string[] expected =
            {
                "wu.a",
                "wu",
                "wu.:b",
                "wu.aaa.bb.cc",
                "wu.aaa.bb:1",
                "wu.aaa.bb:2",
                "wu.aaa.bb.cc:1",
                "wu.aaa.bb.cc:2",
                "wu.aaa.dd:123",
                "wu.aaa.dd.ee:123",
                "wu.aaa.bb.cc:3",
                "wu.aaa.dd:1234",
                "wu.aaa.dd.ee:1234",
            };

            Assert.AreEqual(codes.Count, newCodes.Length, "结果数量应与源编码数量一致");
            CollectionAssert.AreEqual(expected, actual, "ConcatRange 拼接后的默认格式字符串序列与预期不符");
            Log($"拼接结果 ({actual.Length} 条): {string.Join(", ", actual)}");
        }

        [TestMethod]
        public void ConcatRange_拼接路径_等于范围路径加源路径()
        {
            Log("--- 测试: 拼接结果的 LayerValues 应等于 [\"wu\"] + 源 LayerValues ---");
            List<LayeringAddressCode> codes = FixedCodes.Select(s => (LayeringAddressCode)s).ToList();
            LayeringAddressCode range = "wu";
            string[] rangeLayers = range.LayerValues;
            Assert.AreEqual(1, rangeLayers.Length, "范围编码 \"wu\" 应为单层");

            var newCodes = codes.ConcatRange(range).ToArray();
            for (int i = 0; i < newCodes.Length; i++)
            {
                CollectionAssert.AreEqual(
                    rangeLayers.Concat(codes[i].LayerValues).ToArray(),
                    newCodes[i].LayerValues,
                    $"第 {i} 个结果 (源 [{FixedCodes[i]}]) 的 LayerValues 应等于 [\"wu\"] + 源 LayerValues");
            }
            Log($"全部 {newCodes.Length} 条结果的 LayerValues 均等于范围路径前置拼接, 通过");
        }

        [TestMethod]
        public void ConcatRange_非范围编码作范围_抛异常()
        {
            Log("--- 测试: range 不是范围编码时 ConcatRange 应抛 ArgumentException ---");
            LayeringAddressCode itemCode = "aaa.bb:1"; // 项编码, IsRange=false
            List<LayeringAddressCode> codes = new() { "a", "b:1" };

            Assert.ThrowsException<ArgumentException>(
                () => codes.ConcatRange(itemCode).ToArray(),
                "range 非范围编码时应抛 ArgumentException");
            Log("非范围编码作为 range 抛 ArgumentException, 通过");
        }
    }
}
