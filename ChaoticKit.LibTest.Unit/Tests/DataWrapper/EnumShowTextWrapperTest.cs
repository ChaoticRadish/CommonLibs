using ChaoticKit.Attributes.General;
using ChaoticKit.Data.Wrapped;
using ChaoticKit.Extensions;

namespace ChaoticKit.LibTest.Unit.DataWrapper
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataWrapper.ShowText001
    /// 枚举显示文本包装器 EnumShowTextWrapper&lt;T&gt; 与 EnumShowTextWrapperHelper.CreateList 的行为验证:
    /// 固定枚举, 断言 ShowText 取值与列表顺序 (含 Append / InsertHead 后顺序)
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class EnumShowTextWrapperTest : UnitTestBase
    {
        /// <summary>
        /// 被测固定枚举: BBB 带 EnumDesc 特性, 其余无特性
        /// </summary>
        private enum TestEnum
        {
            AAA,
            [EnumDesc("测试B")]
            BBB,
            CCC,
        }

        /// <summary>
        /// 被测包装器: 包装 TestEnum? (与源测试同名同结构, 含 Empty 静态项与隐式转换)
        /// </summary>
        private sealed class EnumTest : EnumShowTextWrapper<TestEnum?>
        {
            public EnumTest() : base() { }
            public EnumTest(TestEnum? value, string? showText = null) : base(value, showText) { }

            public static EnumTest Empty { get; } = new(null, "空选项");
            public static EnumShowTextWrapper<TestEnum?> Empty2 { get; set; } = new(null, "空选项2");

            #region 隐式转换
            public static implicit operator TestEnum?(EnumTest wrapper)
            {
                return wrapper == null ? default : wrapper.Value;
            }
            public static implicit operator EnumTest(TestEnum? property)
            {
                return property == null ? Empty : new(property);
            }
            public static implicit operator EnumTest((TestEnum? property, string showText) obj)
            {
                return new(obj.property, obj.showText);
            }
            public static implicit operator EnumTest((string showText, TestEnum? property) obj)
            {
                return new(obj.property, obj.showText);
            }
            #endregion
        }

        /// <summary>
        /// 无 EnumDesc 特性的枚举值: ShowText 取枚举名字, Value 保持原值
        /// </summary>
        [TestMethod]
        public void ShowText_无EnumDesc取枚举名()
        {
            Log("--- 测试: 无 EnumDesc 特性的枚举值, ShowText 取枚举名 ---");
            EnumTest test1 = TestEnum.AAA;

            Assert.AreEqual("AAA", test1.ShowText, $"无特性枚举值 ShowText 应为枚举名 AAA, 实际: {test1.ShowText}");
            Assert.AreEqual(TestEnum.AAA, test1.Value, $"Value 应保持 TestEnum.AAA, 实际: {test1.Value}");
            Log($"TestEnum.AAA.ShowText = {test1.ShowText}, Value = {test1.Value}, 通过");
        }

        /// <summary>
        /// 带 EnumDesc 特性的枚举值: ShowText 取特性描述
        /// </summary>
        [TestMethod]
        public void ShowText_有EnumDesc取描述()
        {
            Log("--- 测试: 带 EnumDesc 特性的枚举值, ShowText 取特性描述 ---");
            EnumTest test2 = TestEnum.BBB;

            Assert.AreEqual("测试B", test2.ShowText, $"带 EnumDesc 特性枚举值 ShowText 应为 测试B, 实际: {test2.ShowText}");
            Assert.AreEqual(TestEnum.BBB, test2.Value, $"Value 应保持 TestEnum.BBB, 实际: {test2.Value}");
            Log($"TestEnum.BBB.ShowText = {test2.ShowText}, Value = {test2.Value}, 通过");
        }

        /// <summary>
        /// null 枚举值: 经隐式转换落到 Empty 静态项, ShowText 取自定义文本
        /// </summary>
        [TestMethod]
        public void ShowText_null隐式转换为Empty()
        {
            Log("--- 测试: null 枚举值隐式转换, ShowText 取 Empty 自定义文本 ---");
            EnumTest test3 = (TestEnum?)null;

            Assert.AreSame(EnumTest.Empty, test3, "null 经隐式转换应返回同一 Empty 静态实例");
            Assert.AreEqual("空选项", test3.ShowText, $"Empty.ShowText 应为 空选项, 实际: {test3.ShowText}");
            Assert.IsNull(test3.Value, "Empty 的 Value 应为 null");
            Log($"null.ShowText = {test3.ShowText}, Value = null, 通过");
        }

        /// <summary>
        /// CreateList: 遍历固定枚举, 按声明顺序生成, ShowText 顺序与 Value 顺序一致
        /// </summary>
        [TestMethod]
        public void CreateList_固定枚举_按声明顺序()
        {
            Log("--- 测试: CreateList 遍历固定枚举, 按声明顺序生成 ---");
            var list = EnumShowTextWrapperHelper.CreateList<TestEnum, EnumTest>();

            Assert.AreEqual(3, list.Count, $"固定枚举 3 项, 实际列表数量: {list.Count}");
            CollectionAssert.AreEqual(
                new[] { "AAA", "测试B", "CCC" },
                list.Select(i => i.ShowText).ToArray(),
                "列表 ShowText 顺序应遵循枚举声明顺序");
            CollectionAssert.AreEqual(
                new TestEnum?[] { TestEnum.AAA, TestEnum.BBB, TestEnum.CCC },
                list.Select(i => i.Value).ToArray(),
                "列表 Value 顺序应遵循枚举声明顺序");
            Log($"CreateList ShowText = {list.Select(i => i.ShowText).ToArray().FullInfoString()}, 通过");
        }

        /// <summary>
        /// Append: 向列表尾部追加, 原有顺序保持, 新项在末尾
        /// </summary>
        [TestMethod]
        public void Append_尾部追加保持顺序()
        {
            Log("--- 测试: Append 向列表尾部追加, 顺序保持 ---");
            var list = EnumShowTextWrapperHelper.CreateList<TestEnum, EnumTest>();
            list.Append(EnumTest.Empty);

            Assert.AreEqual(4, list.Count, $"Append 后应为 4 项, 实际: {list.Count}");
            CollectionAssert.AreEqual(
                new[] { "AAA", "测试B", "CCC", "空选项" },
                list.Select(i => i.ShowText).ToArray(),
                "Append 后 ShowText 顺序: 原 3 项 + 尾部 空选项");
            Log($"Append 后 ShowText = {list.Select(i => i.ShowText).ToArray().FullInfoString()}, 通过");
        }

        /// <summary>
        /// InsertHead: 向列表头部插入, 原有顺序保持, 新项在头部
        /// </summary>
        [TestMethod]
        public void InsertHead_头部插入保持顺序()
        {
            Log("--- 测试: InsertHead 向列表头部插入, 顺序保持 ---");
            var list = EnumShowTextWrapperHelper.CreateList<TestEnum, EnumTest>();
            list.InsertHead(EnumTest.Empty);

            Assert.AreEqual(4, list.Count, $"InsertHead 后应为 4 项, 实际: {list.Count}");
            CollectionAssert.AreEqual(
                new[] { "空选项", "AAA", "测试B", "CCC" },
                list.Select(i => i.ShowText).ToArray(),
                "InsertHead 后 ShowText 顺序: 头部 空选项 + 原 3 项");
            Log($"InsertHead 后 ShowText = {list.Select(i => i.ShowText).ToArray().FullInfoString()}, 通过");
        }

        /// <summary>
        /// 非枚举泛型参数: EnumShowTextWrapper&lt;T&gt; 静态构造函数抛异常,
        /// 首次访问时 .NET 包装为 TypeInitializationException, 内部异常才是真正的错误
        /// </summary>
        [TestMethod]
        public void 非枚举泛型_静态构造抛TypeInitializationException()
        {
            Log("--- 测试: 非枚举泛型参数, 静态构造函数抛异常 ---");
            TypeInitializationException ex = Assert.ThrowsException<TypeInitializationException>(
                () => new EnumShowTextWrapper<int>(),
                "泛型参数非枚举时应抛异常(静态初始化)");

            Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException),
                $"内部异常应为 InvalidOperationException, 实际: {ex.InnerException?.GetType().Name}");
            Log($"非枚举泛型抛 TypeInitializationException, 内部异常为 {ex.InnerException?.GetType().Name}, 通过");
        }
    }
}
