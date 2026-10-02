namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Bool001
    /// 纯语言 bool 位运算符 (&, |, ^) 对 bool 的语义验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class BoolBitwiseTest : UnitTestBase
    {
        [DataTestMethod]
        [DataRow(true, true, true)]
        [DataRow(true, false, true)]
        [DataRow(false, true, true)]
        [DataRow(false, false, false)]
        public void Or_真值表(bool left, bool right, bool expected)
        {
            Log($"--- 测试: {left} | {right} ---");
            bool result = left | right;

            Assert.AreEqual(expected, result, $"{left} | {right} 应为 {expected}");
            Log($"{left} | {right} = {result}, 通过");
        }

        [DataTestMethod]
        [DataRow(true, true, true)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, false)]
        public void And_真值表(bool left, bool right, bool expected)
        {
            Log($"--- 测试: {left} & {right} ---");
            bool result = left & right;

            Assert.AreEqual(expected, result, $"{left} & {right} 应为 {expected}");
            Log($"{left} & {right} = {result}, 通过");
        }

        [DataTestMethod]
        [DataRow(true, true, false)]
        [DataRow(true, false, true)]
        [DataRow(false, true, true)]
        [DataRow(false, false, false)]
        public void Xor_真值表(bool left, bool right, bool expected)
        {
            Log($"--- 测试: {left} ^ {right} ---");
            bool result = left ^ right;

            Assert.AreEqual(expected, result, $"{left} ^ {right} 应为 {expected}");
            Log($"{left} ^ {right} = {result}, 通过");
        }

        [TestMethod]
        public void 源场景_多变量混合运算()
        {
            Log("--- 测试: 源测试场景 (b1=true, b2=true, b3=false) ---");
            bool b1 = true;
            bool b2 = true;
            bool b3 = false;

            bool or12 = b1 | b2;
            bool or13 = b1 | b3;
            bool or23 = b2 | b3;
            bool and12 = b1 & b2;
            bool and123 = b1 & b2 & b3;

            Assert.IsTrue(or12, "true | true 应为 true");
            Assert.IsTrue(or13, "true | false 应为 true");
            Assert.IsTrue(or23, "true | false 应为 true");
            Assert.IsTrue(and12, "true & true 应为 true");
            Assert.IsFalse(and123, "true & true & false 应为 false");
            Log($"b1|b2={or12}, b1|b3={or13}, b2|b3={or23}, b1&b2={and12}, b1&b2&b3={and123}, 全部通过");
        }

        [TestMethod]
        public void 位运算不短路_与逻辑运算短路_语义区别()
        {
            Log("--- 测试: & 不短路, && 短路 ---");
            int evalCount = 0;
            bool SideEffectTrue()
            {
                evalCount++;
                return true;
            }

            // & 两侧都会求值: 即使左侧为 false, 右侧仍执行
            bool andResult = SideEffectTrue() & SideEffectTrue();
            Assert.IsTrue(andResult);
            Assert.AreEqual(2, evalCount, "& 应对两侧都求值");
            Log($"& 两侧各求值一次, 总求值次数={evalCount}, 通过");

            // && 短路: 左侧为 false 时右侧不求值
            evalCount = 0;
            bool orResult = true || SideEffectTrue(); // 左侧 true 短路, 右侧不求值
            Assert.IsTrue(orResult);
            Assert.AreEqual(0, evalCount, "|| 左侧为 true 时右侧不应求值(短路)");
            Log($"|| 左侧 true 短路, 右侧未求值, 求值次数={evalCount}, 通过");
        }
    }
}
