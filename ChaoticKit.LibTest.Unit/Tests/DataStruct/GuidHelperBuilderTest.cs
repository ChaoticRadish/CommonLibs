using ChaoticKit.Data;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Guid001
    /// GuidHelper.Builder() 字节打包 / Reset / CurrentIndex 行为的验证
    /// </summary>
    /// <remarks>
    /// 覆盖点:
    /// - Add(byte[]): 按索引顺序依次写入 (小端序效果), 填满 16 字节后产出完整 Guid
    /// - Add(int): 默认大端序, bigEndian=false 时低位优先
    /// - Add(long): 默认大端序, 8 字节高位优先
    /// - AddRandom: 随机字节填充, 只断言填充长度与不抛异常 (随机值无法断言具体内容)
    /// - Reset: 重置 CurrentIndex 与字节内容
    /// - CurrentIndex: 追加过程中索引推进, 达到上限 16 后不再继续添加
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class GuidHelperBuilderTest : UnitTestBase
    {
        [TestMethod]
        public void AddBytes_16字节按序填充_Build生成完整Guid()
        {
            Log("--- 测试: Add(byte[]) 按序填充 16 字节 ---");
            var builder = GuidHelper.Builder();
            builder.Add(1, 2, 3, 4);
            builder.Add(5, 6, 7, 8);
            builder.Add(9, 10, 11, 12);
            builder.Add(13, 14, 15, 16);

            Guid guid = builder.Build();
            Assert.AreEqual(new Guid("04030201-0605-0807-090a-0b0c0d0e0f10"), guid,
                "按索引顺序写入 01..10 后, Guid 应为 04030201-0605-0807-090a-0b0c0d0e0f10, 实际: " + guid);
            Log($"Add(1..16) -> {guid}, 通过");
        }

        [DataTestMethod]
        [DataRow(99, true, "63000000-0000-0000-0000-000000000000")]
        [DataRow(99, false, "00000063-0000-0000-0000-000000000000")]
        public void AddInt_Build_按大小端序打包(int value, bool bigEndian, string expectedGuid)
        {
            Log($"--- 测试: Add(int, bigEndian={bigEndian}) ---");
            var builder = GuidHelper.Builder();
            builder.Add(value, bigEndian);

            Guid guid = builder.Build();
            Assert.AreEqual(new Guid(expectedGuid), guid,
                $"Add({value}, bigEndian={bigEndian}) 的打包结果不匹配, 预期: {expectedGuid}, 实际: {guid}");
            Log($"Add({value}, bigEndian={bigEndian}) -> {guid}, 通过");
        }

        [TestMethod]
        public void AddLong_Build_大端序高位优先打包()
        {
            Log("--- 测试: Add(long) 默认大端序 ---");
            var builder = GuidHelper.Builder();
            builder.Add(99L);

            Guid guid = builder.Build();
            Assert.AreEqual(new Guid("00000000-0000-6300-0000-000000000000"), guid,
                "Add(99L) 大端序应把 0x63 写入第 8 字节 (Data3 高位), 实际: " + guid);
            Log($"Add(99L) -> {guid}, 通过");
        }

        [TestMethod]
        public void AddRandom_填充至上限_不抛异常()
        {
            Log("--- 测试: AddRandom() 随机填充 (弱断言: 仅长度与不抛异常) ---");
            // 随机字节无法断言具体值, 用固定种子保证确定性, 只校验填充长度与 Build 不抛异常
            var builder = GuidHelper.Builder();
            builder.AddRandom(new System.Random(20260927));

            Guid guid = builder.Build();
            Assert.AreEqual(16, builder.CurrentIndex, "AddRandom() 应把 16 字节全部填满");
            Assert.IsNotNull(guid, "Build() 应正常返回 Guid");
            Log($"AddRandom() -> {guid}, CurrentIndex={builder.CurrentIndex}, 通过");
        }

        [TestMethod]
        public void Add_Build_Reset_索引推进与重置()
        {
            Log("--- 测试: 连续 Add + Build + Reset 的索引推进与内容重置 ---");
            var builder = GuidHelper.Builder();

            builder.Add(99164L);
            Guid out1 = builder.Build();
            Assert.AreEqual(new Guid("00000000-0100-5c83-0000-000000000000"), out1,
                "out 1 不匹配, 实际: " + out1);
            Log($"out 1 = {out1}, 通过");

            builder.Add(12345);
            Guid out2 = builder.Build();
            Assert.AreEqual(new Guid("00000000-0100-5c83-0000-303900000000"), out2,
                "out 2 不匹配, 实际: " + out2);
            Log($"out 2 = {out2}, 通过");

            builder.Add((byte)12);
            Guid out3 = builder.Build();
            Assert.AreEqual(new Guid("00000000-0100-5c83-0000-30390c000000"), out3,
                "out 3 不匹配, 实际: " + out3);
            Log($"out 3 = {out3}, 通过");

            builder.Add(113513155312345_5613L);
            Guid out4 = builder.Build();
            Assert.AreEqual(new Guid("00000000-0100-5c83-0000-30390c0fc0cc"), out4,
                "out 4 不匹配 (剩余 3 字节应写入 0f c0 cc), 实际: " + out4);
            Assert.AreEqual(16, builder.CurrentIndex, "填满后 CurrentIndex 应为 16, 实际: " + builder.CurrentIndex);
            Log($"out 4 = {out4}, CurrentIndex={builder.CurrentIndex}, 通过");

            builder.Reset();
            Log("Reset");
            Assert.AreEqual(0, builder.CurrentIndex, "Reset 后 CurrentIndex 应为 0, 实际: " + builder.CurrentIndex);
            Guid out5 = builder.Build();
            Assert.AreEqual(Guid.Empty, out5, "Reset 后 Build 应得到空 Guid, 实际: " + out5);
            Log($"out 5 = {out5}, 通过");

            builder.Add(12345);
            Guid out6 = builder.Build();
            Assert.AreEqual(new Guid("39300000-0000-0000-0000-000000000000"), out6,
                "out 6 不匹配, 实际: " + out6);
            Log($"out 6 = {out6}, 通过");

            builder.Add((byte)12);
            Guid out7 = builder.Build();
            Assert.AreEqual(new Guid("39300000-000c-0000-0000-000000000000"), out7,
                "out 7 不匹配, 实际: " + out7);
            Log($"out 7 = {out7}, 通过");

            builder.Add(113513155312345_5613L);
            Guid out8a = builder.Build();
            Assert.AreEqual(new Guid("39300000-0f0c-ccc0-2225-42d27d000000"), out8a,
                "out 8 (第一次) 不匹配, 实际: " + out8a);
            Log($"out 8a = {out8a}, 通过");

            builder.Add(113513155312345_5613L);
            Guid out8b = builder.Build();
            Assert.AreEqual(new Guid("39300000-0f0c-ccc0-2225-42d27d0fc0cc"), out8b,
                "out 8 (第二次) 不匹配 (剩余 3 字节应写入 0f c0 cc), 实际: " + out8b);
            Assert.AreEqual(16, builder.CurrentIndex, "最终 CurrentIndex 应为 16, 实际: " + builder.CurrentIndex);
            Log($"out 8b = {out8b}, CurrentIndex={builder.CurrentIndex}, 通过");
        }
    }
}
