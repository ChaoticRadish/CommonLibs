using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.Generics
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Generics.IListExtension001
    /// ChaoticKit.Extensions 的 IList TryGet / GetOrDefault 取值扩展方法验证:
    /// 有效索引返回元素, 越界/负索引/空或 null 数组返回默认值
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class IListExtensionTest : UnitTestBase
    {
        private static readonly int[] IntArr1 = [1, 2, 3, 4, 5];
        private static readonly int?[] IntArr2 = [1, 2, null, 4, 5];
        private static readonly string[] StrArr3 = ["1", "2", "3", "4", "5"];

        [TestMethod]
        public void TryGet_源场景_有效索引返回元素()
        {
            Log("--- 测试: TryGet 有效索引 (index=3) ---");

            Assert.IsTrue(IntArr1.TryGet(3, out var val1), "int[] 索引 3 应取成功");
            Assert.AreEqual(4, val1, "int[] 索引 3 的值应为 4");
            Log($"intArr1.TryGet(3) = true, val={val1}, 通过");

            Assert.IsTrue(IntArr2.TryGet(3, out var val2), "int?[] 索引 3 应取成功");
            Assert.AreEqual(4, val2, "int?[] 索引 3 的值应为 4");
            Log($"intArr2.TryGet(3) = true, val={val2}, 通过");

            Assert.IsTrue(StrArr3.TryGet(3, out var val3), "string[] 索引 3 应取成功");
            Assert.AreEqual("4", val3, "string[] 索引 3 的值应为 \"4\"");
            Log($"strArr3.TryGet(3) = true, val={val3}, 通过");
        }

        [DataTestMethod]
        [DataRow(5)]   // 等于数组长度
        [DataRow(6)]   // 超过数组长度
        [DataRow(-1)]  // 负数索引
        public void TryGet_越界返回false且out值为默认(int index)
        {
            Log($"--- 测试: TryGet 越界 (index={index}) ---");
            bool ok = IntArr1.TryGet(index, out var item);

            Assert.IsFalse(ok, $"索引 {index} 越界, TryGet 应返回 false");
            Assert.AreEqual(0, item, $"索引 {index} 越界, out 值应为 default(int)=0");
            Log($"IntArr1.TryGet({index}) = false, item={item}, 通过");
        }

        [TestMethod]
        public void TryGet_空数组与null数组返回false()
        {
            Log("--- 测试: TryGet 空数组 / null 数组 ---");
            int[] emptyArr = [];
            int[]? nullArr = null;

            Assert.IsFalse(emptyArr.TryGet(0, out var v1), "空数组应返回 false");
            Assert.AreEqual(0, v1, "空数组 out 值应为默认值");
            Assert.IsFalse(nullArr.TryGet(0, out var v2), "null 数组应返回 false");
            Assert.AreEqual(0, v2, "null 数组 out 值应为默认值");
            Log("空数组 / null 数组 TryGet 均返回 false, 通过");
        }

        [DataTestMethod]
        [DataRow(3, 4)]
        [DataRow(0, 1)]
        [DataRow(4, 5)]
        public void GetOrDefault_有效索引返回元素(int index, int expected)
        {
            Log($"--- 测试: GetOrDefault 有效索引 (index={index}) ---");
            int value = IntArr1.GetOrDefault(index);

            Assert.AreEqual(expected, value, $"索引 {index} 应返回元素 {expected}");
            Log($"IntArr1.GetOrDefault({index}) = {value}, 通过");
        }

        [DataTestMethod]
        [DataRow(5, 0)]
        [DataRow(6, 0)]
        [DataRow(-1, 0)]
        public void GetOrDefault_越界返回默认值(int index, int expected)
        {
            Log($"--- 测试: GetOrDefault 越界 (index={index}) ---");
            int value = IntArr1.GetOrDefault(index);

            Assert.AreEqual(expected, value, $"索引 {index} 越界应返回 default(int)=0");
            Log($"IntArr1.GetOrDefault({index}) = {value}, 通过");
        }

        [DataTestMethod]
        [DataRow(5, null)]
        [DataRow(6, null)]
        [DataRow(-1, null)]
        public void GetOrDefault_越界返回null(int index, string? expected)
        {
            Log($"--- 测试: GetOrDefault 越界返回 null (index={index}) ---");
            string? value = StrArr3.GetOrDefault(index);

            Assert.AreEqual(expected, value, $"索引 {index} 越界应返回 default(string)=null");
            Log($"StrArr3.GetOrDefault({index}) = {(value ?? "<null>")}, 通过");
        }

        [TestMethod]
        public void GetOrDefault_越界返回指定默认值()
        {
            Log("--- 测试: GetOrDefault 越界返回指定默认值 ---");
            string str2 = StrArr3.GetOrDefault(6, "<null>");

            Assert.AreEqual("<null>", str2, "索引 6 越界应返回传入的默认值 \"<null>\"");
            Log($"StrArr3.GetOrDefault(6, \"<null>\") = {str2}, 通过");
        }
    }
}
