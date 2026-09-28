using ChaoticKit.Data.Structure.Pair;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.DisorderPair001
    /// 无序对 DisorderPair 的相等语义验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class DisorderPairTest : UnitTestBase
    {
        [TestMethod]
        public void SingleGeneric_OrderIrrelevantEqual()
        {
            Log("--- 测试: 单泛型参数 顺序无关相等 ---");
            DisorderPair<string> p1 = ("123", "321");
            DisorderPair<string> p2 = new KeyValuePair<string, string>("321", "123");

            Assert.IsTrue(p1 == p2, "无序对 (123,321) 与 (321,123) 应相等");
            Assert.IsTrue(p1.Equals(p2), "Equals 也应返回 true");
            Log($"p1=({p1.Item1},{p1.Item2}) == p2=({p2.Item1},{p2.Item2}) => true, 通过");
        }

        [TestMethod]
        public void SingleGeneric_ValueDifferentNotEqual()
        {
            Log("--- 测试: 单泛型参数 值不同不相等 ---");
            DisorderPair<string> p1 = ("123", "32");
            DisorderPair<string> p2 = new KeyValuePair<string, string>("321", "123");

            Assert.IsFalse(p1 == p2, "值不同的无序对不应相等");
            Assert.IsTrue(p1 != p2, "!= 应返回 true");
            Log("值不同的无序对 != 通过");
        }

        [TestMethod]
        public void DoubleGeneric_FromKeyValuePair()
        {
            Log("--- 测试: 双泛型参数 跨类型 KeyValuePair 转换 ---");
            DisorderPair<string, int> p1 = ("123", 321);
            DisorderPair<string, int> p2 = new KeyValuePair<int, string>(321, "123");

            Assert.IsTrue(p1 == p2, "双泛型无序对与 KVP<int,string> 转换后应相等");
            Log($"DisorderPair<string,int>({p1.Item1},{p1.Item2}) == 转换自 KVP<int,string>(321,123) => true, 通过");
        }

        [TestMethod]
        public void DoubleGeneric_ValueDifferentNotEqual()
        {
            Log("--- 测试: 双泛型参数 值不同不相等 ---");
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<string, int> p2 = new KeyValuePair<int, string>(321, "123");

            Assert.IsFalse(p1 == p2, "值不同 (32 vs 321) 不应相等");
            Assert.IsTrue(p1 != p2);
            Log("值不同的双泛型无序对 != 通过");
        }

        [TestMethod]
        public void DoubleGeneric_CrossArityConversion()
        {
            Log("--- 测试: 双泛型参数 跨泛型实参顺序隐式转换 ---");
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<int, string> p2 = p1;

            Assert.IsTrue(p1 == p2, "DisorderPair<string,int> 与 DisorderPair<int,string> 应可互相比较相等");
            Log("跨泛型实参顺序的隐式转换 + 相等比较通过");
        }

        [TestMethod]
        public void DoubleGeneric_DirectCrossArityConstruct()
        {
            Log("--- 测试: 双泛型参数 直接构造跨序实例 ---");
            DisorderPair<string, int> p1 = ("123", 32);
            DisorderPair<int, string> p2 = new KeyValuePair<int, string>(32, "123");

            Assert.IsTrue(p1 == p2, "KVP<int,string>(32,123) 构造的跨序实例应相等");
            Log("直接构造跨序实例并相等, 通过");
        }
    }
}
