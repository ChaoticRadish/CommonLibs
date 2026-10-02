using ChaoticKit.Data.Enums;
using ChaoticKit.Data.Struct.Modal;

namespace ChaoticKit.LibTest.Unit.Operation
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Operation.Result005
    /// 模态结果接口 ITagModalResult&lt;T&gt; / ITagModalResult 的标签与结果状态验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class TagModalResultTest : UnitTestBase
    {
        /// <summary>
        /// 实现 ITagModalResult&lt;string&gt; 的自定义类型 (对应控制台测试中的 MyTest)
        /// </summary>
        private sealed class MyTest : ITagModalResult<string>
        {
            public required string Tag { get; set; }

            public ModalResult Result { get; set; }
        }

        [TestMethod]
        public void GenericInterface_ExposesTagAndResult()
        {
            Log("--- 测试: ITagModalResult<string> 标签与结果状态 ---");
            ITagModalResult<string> temp = new MyTest { Tag = "123", Result = ModalResult.Yes };
            Log($"temp.Tag = {temp.Tag}");

            Assert.AreEqual("123", temp.Tag, "泛型接口 Tag 应为 '123'");
            Assert.AreEqual(ModalResult.Yes, temp.Result, "泛型接口 Result 应为 ModalResult.Yes");
            Log($"通过: Tag={temp.Tag}, Result={temp.Result}({temp.Result.DefaultChineseName()})");
        }

        [TestMethod]
        public void NonGenericInterface_ExposesSameTagAsObject()
        {
            Log("--- 测试: 泛型接口转非泛型 ITagModalResult ---");
            ITagModalResult<string> temp = new MyTest { Tag = "123", Result = ModalResult.Yes };
            ITagModalResult temp2 = temp;
            Log($"temp2.Tag = {temp2.Tag}");

            Assert.AreEqual("123", temp2.Tag, "非泛型接口 Tag(object) 应为 '123'");
            Assert.AreEqual(typeof(string), temp2.Tag?.GetType(), "非泛型接口 Tag 装箱后类型应为 string");
            Assert.AreEqual(ModalResult.Yes, temp2.Result, "非泛型接口 Result 应保持一致");
            Log($"通过: temp2.Tag={temp2.Tag}(类型 {temp2.Tag?.GetType().Name}), Result={temp2.Result}");
        }

        [DataTestMethod]
        [DataRow("123", ModalResult.Yes)]
        [DataRow("abc", ModalResult.No)]
        [DataRow("确认", ModalResult.Ok)]
        public void TableDriven_TagAndResult_BothInterfaces(string tag, ModalResult result)
        {
            Log($"--- 测试: 表驱动 (tag={tag}, result={result}) ---");
            ITagModalResult<string> temp = new MyTest { Tag = tag, Result = result };

            Assert.AreEqual(tag, temp.Tag, "泛型接口 Tag 不匹配");
            Assert.AreEqual(result, temp.Result, "泛型接口 Result 不匹配");

            ITagModalResult nonGeneric = temp;
            Assert.AreEqual(tag, nonGeneric.Tag, "非泛型接口 Tag 不匹配");
            Assert.AreEqual(result, nonGeneric.Result, "非泛型接口 Result 不匹配");
            Log($"通过: tag={temp.Tag}, result={temp.Result}({result.DefaultChineseName()}), 泛型/非泛型一致");
        }
    }
}
