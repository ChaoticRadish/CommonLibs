namespace ChaoticKit.LibTest.Unit.CSharp
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.CSharp.Struct001
    /// 验证结构体 (struct) 值传递语义:
    /// - 直接调用实例方法 Set 会修改该实例的成员
    /// - 从方法返回值创建的实例同样可以被 Set 修改
    /// - struct 按值传入方法时, 方法内部对副本的修改不影响原变量
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class StructValueSemanticsTest : UnitTestBase
    {
        /// <summary>
        /// 与源测试一致的被测结构体: Set(test) 将 X 置为 test+1, Y 置为 test+2
        /// </summary>
        private struct TestStruct
        {
            public TestStruct(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }

            public int Y { get; set; }

            public void Set(int test)
            {
                X = test + 1;
                Y = test + 2;
            }
        }

        /// <summary>
        /// 测试第一部分: 直接创建的实例, 调用 Set 后成员被更新
        /// </summary>
        [TestMethod]
        public void 直接创建实例_Set后成员被更新()
        {
            Log("--- 测试第一部分: 直接创建的实例 ---");
            TestStruct t1 = new TestStruct(3, 6);
            Log("编辑前: " + WriteFull(t1));
            Assert.AreEqual(3, t1.X, "编辑前 X 应为构造参数 3");
            Assert.AreEqual(6, t1.Y, "编辑前 Y 应为构造参数 6");

            t1.Set(8);
            Log("编辑后 t1.Set(8); : " + WriteFull(t1));
            Assert.AreEqual(9, t1.X, "Set(8) 应把 X 置为 8+1=9");
            Assert.AreEqual(10, t1.Y, "Set(8) 应把 Y 置为 8+2=10");
        }

        /// <summary>
        /// 测试第二部分: 从方法返回值获取的实例, 调用 Set 后成员被更新
        /// </summary>
        [TestMethod]
        public void 方法返回值实例_Set后成员被更新()
        {
            Log("--- 测试第二部分: 方法返回值创建的实例 ---");
            TestStruct t2 = GetTestStruct();
            Log("编辑前: " + WriteFull(t2));
            Assert.AreEqual(7, t2.X, "编辑前 X 应为方法返回值 7");
            Assert.AreEqual(1, t2.Y, "编辑前 Y 应为方法返回值 1");

            t2.Set(8);
            Log("编辑后 t2.Set(8); : " + WriteFull(t2));
            Assert.AreEqual(9, t2.X, "Set(8) 应把 X 置为 8+1=9");
            Assert.AreEqual(10, t2.Y, "Set(8) 应把 Y 置为 8+2=10");
        }

        /// <summary>
        /// 测试第三部分: struct 按值传入方法, 方法内部修改的是副本, 不影响原变量
        /// </summary>
        [TestMethod]
        public void 按值传入方法_方法内修改不影响原变量()
        {
            Log("--- 测试第三部分: 值传递语义 ---");
            TestStruct t3 = new TestStruct(3, 6);

            ModifyByValue(t3);

            Log("测试后 test(t3); : " + WriteFull(t3));
            Assert.AreEqual(3, t3.X, "struct 按值传递, 方法内 Set(8) 不应影响原变量 X");
            Assert.AreEqual(6, t3.Y, "struct 按值传递, 方法内 Set(8) 不应影响原变量 Y");
        }

        /// <summary>
        /// 表驱动: Set(test) 按公式 X=test+1, Y=test+2 更新成员 (固定数据, 确定性)
        /// </summary>
        [DataTestMethod]
        [DataRow(8)]
        [DataRow(0)]
        [DataRow(-5)]
        public void Set_按公式更新成员(int value)
        {
            TestStruct s = new TestStruct(3, 6);
            s.Set(value);
            Assert.AreEqual(value + 1, s.X, $"Set({value}) 应把 X 置为 {value}+1");
            Assert.AreEqual(value + 2, s.Y, $"Set({value}) 应把 Y 置为 {value}+2");
            Log($"Set({value}); => X={s.X}, Y={s.Y}, 通过");
        }

        /// <summary>
        /// 返回固定结构体实例 (替代源测试中的局部函数 get)
        /// </summary>
        private static TestStruct GetTestStruct()
        {
            return new TestStruct(7, 1);
        }

        /// <summary>
        /// 按值接收结构体参数, 内部修改副本 (替代源测试中的局部方法 test)
        /// </summary>
        private void ModifyByValue(TestStruct value)
        {
            Log("(test方法)编辑前: " + WriteFull(value));
            value.Set(8);
            Log("(test方法)编辑后 value.Set(8); : " + WriteFull(value));
        }

        /// <summary>
        /// 格式化结构体成员, 供 Log 输出过程信息
        /// </summary>
        private static string WriteFull(TestStruct value)
        {
            return $"X={value.X}, Y={value.Y}";
        }
    }
}
