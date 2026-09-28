using ChaoticKit.Data.Struct;

namespace ChaoticKit.LibTest.Unit.DataWrapper
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataWrapper.NeedInitObject002
    /// NeedInitObjectImmutable (结构体) 与 NeedInitObject (类) 的只读性/可写性验证:
    /// - 结构体包装器经只读属性暴露后, 外部对属性的写操作在编译期被拦截 (CS1612), 运行期仅影响属性返回的副本;
    /// - 类包装器属性返回引用, 外部可直接赋值;
    /// - 类内私有字段不受只读属性限制, 仍可写。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class NeedInitObjectImmutableTest : UnitTestBase
    {
        /// <summary>
        /// 模拟被测模型, 与源控制台测试中的 TestModel 保持一致
        /// </summary>
        private sealed class TestModel
        {
            /// <summary>只读自动属性 + 结构体包装器: 外部写 Int01.Value 编译不通过 (CS1612)</summary>
            public NeedInitObjectImmutable<int> Int01 { get; } = new();
            /// <summary>类包装器: 属性返回引用, 外部可直接写 Value</summary>
            public NeedInitObject<int> Int02 { get; } = new();
            /// <summary>私有字段 + 只读属性: 外部写 Int03.Value 编译不通过 (CS1612), 类内可写</summary>
            public NeedInitObjectImmutable<int> Int03 { get => int03; private set => int03 = value; }
            private NeedInitObjectImmutable<int> int03 = new();

            public void SetValue(int i1, int i2, int i3)
            {
                // Int01.Value = i1; // 编译不通过 (CS1612): 结构体经只读属性返回副本
                Int02.Value = i2;     // 类包装器, 可写
                int03.Value = i3;     // 类内直接访问私有字段, 可写
            }

            public override string ToString() => $"Int01:{Int01}\nInt02:{Int02}\nInt03:{Int03}";
        }

        [TestMethod]
        public void 初始状态_三个包装器均未赋值()
        {
            Log("--- 测试: 初始状态 ---");
            TestModel model = new();
            Log($"初始值: \n{model}");

            Assert.IsFalse(model.Int01.Inited, "Int01 初始应未赋值");
            Assert.IsFalse(model.Int02.Inited, "Int02 初始应未赋值");
            Assert.IsFalse(model.Int03.Inited, "Int03 初始应未赋值");

            const string expected = "Int01:NeedInitObjectImmutable<Int32>_WaitingValue_[0]\nInt02:NeedInitObject<Int32>_WaitingValue_[0]\nInt03:NeedInitObjectImmutable<Int32>_WaitingValue_[0]";
            Assert.AreEqual(expected, model.ToString(), "初始 ToString 应与源控制台打印一致");
            Log("初始状态验证通过");
        }

        [TestMethod]
        public void 可写路径_类包装器可直接赋值()
        {
            Log("--- 测试: 可写路径 (类包装器) ---");
            TestModel model = new();

            // model.Int01.Value = 100; // 编译不通过 (CS1612): 结构体经只读属性不可写
            model.Int02.Value = 200;
            // model.Int03.Value = 300; // 编译不通过 (CS1612)

            Assert.IsTrue(model.Int02.Inited, "Int02 (类包装器) 赋值后应 Inited=true");
            Assert.AreEqual(200, model.Int02.Value, "Int02.Value 应为 200");
            Assert.IsFalse(model.Int01.Inited, "Int01 不可写, 应保持未赋值");
            Assert.IsFalse(model.Int03.Inited, "Int03 不可写, 应保持未赋值");

            const string expected = "Int01:NeedInitObjectImmutable<Int32>_WaitingValue_[0]\nInt02:NeedInitObject<Int32>_Inited_[200]\nInt03:NeedInitObjectImmutable<Int32>_WaitingValue_[0]";
            Assert.AreEqual(expected, model.ToString(), "赋值后 ToString 应与源控制台打印一致");
            Log($"赋值后: \n{model}");
            Log("可写路径验证通过: 仅 Int02 (类包装器) 被写入 200");
        }

        [TestMethod]
        public void 不可写路径_结构体经只读属性仅影响副本()
        {
            Log("--- 测试: 不可写路径 (结构体经只读属性) ---");
            TestModel model = new();

            // 直接写 model.Int01.Value 在编译期被拦截 (CS1612),
            // 这里通过"取属性返回的副本再写"验证运行期行为: 对副本的修改不影响模型内部存储
            var copy = model.Int01;
            copy.Value = 999;
            Log($"对属性返回副本写值 999 后: 副本 Inited={copy.Inited}, Value={copy.Value}");

            Assert.IsTrue(copy.Inited, "属性返回的副本应已赋值");
            Assert.AreEqual(999, copy.Value, "副本 Value 应为 999");
            Assert.IsFalse(model.Int01.Inited, "模型内部 Int01 不应受影响, 应保持未赋值");
            Log("不可写路径验证通过: 结构体包装器经只读属性暴露后外部不可写");
        }

        [TestMethod]
        public void SetValue_类内可写_外部只读属性不受影响()
        {
            Log("--- 测试: SetValue(101, 202, 303) ---");
            TestModel model = new();
            Log($"初始值: \n{model}");

            model.SetValue(101, 202, 303);

            Assert.IsFalse(model.Int01.Inited, "Int01 经只读属性不可写, 应保持未赋值");
            Assert.IsTrue(model.Int02.Inited, "Int02 应已赋值");
            Assert.IsTrue(model.Int03.Inited, "Int03 类内写入私有字段应已赋值");
            Assert.AreEqual(202, model.Int02.Value, "Int02.Value 应为 202");
            Assert.AreEqual(303, model.Int03.Value, "Int03.Value 应为 303");

            const string expected = "Int01:NeedInitObjectImmutable<Int32>_WaitingValue_[0]\nInt02:NeedInitObject<Int32>_Inited_[202]\nInt03:NeedInitObjectImmutable<Int32>_Inited_[303]";
            Assert.AreEqual(expected, model.ToString(), "SetValue 后 ToString 应与源控制台打印一致");
            Log($"SetValue 后: \n{model}");
            Log("SetValue 验证通过: Int01 保持未赋值, Int02=202, Int03=303");
        }

        [TestMethod]
        public void 未赋值读取Value_抛InvalidOperationException()
        {
            Log("--- 测试: 异常路径 (未赋值读取) ---");
            NeedInitObjectImmutable<int> structWrapper = new();
            NeedInitObject<int> classWrapper = new();

            InvalidOperationException ex1 = Assert.ThrowsException<InvalidOperationException>(
                () => _ = structWrapper.Value, "结构体包装器未赋值读取应抛异常");
            InvalidOperationException ex2 = Assert.ThrowsException<InvalidOperationException>(
                () => _ = classWrapper.Value, "类包装器未赋值读取应抛异常");

            Log($"未赋值读取抛 InvalidOperationException: \"{ex1.Message}\" / \"{ex2.Message}\"");
            Log("未赋值读取异常验证通过");
        }

        [TestMethod]
        public void 重复赋值_抛InvalidOperationException()
        {
            Log("--- 测试: 异常路径 (已赋值后再次赋值) ---");
            NeedInitObjectImmutable<int> structWrapper = new();
            structWrapper.Value = 100;
            Assert.ThrowsException<InvalidOperationException>(
                () => structWrapper.Value = 200, "结构体包装器已赋值后再次赋值应抛异常");

            NeedInitObject<int> classWrapper = new();
            classWrapper.Value = 100;
            Assert.ThrowsException<InvalidOperationException>(
                () => classWrapper.Value = 200, "类包装器已赋值后再次赋值应抛异常");

            Log("重复赋值抛 InvalidOperationException, 通过");
        }

        [TestMethod]
        public void 赋值null_抛InvalidOperationException()
        {
            Log("--- 测试: 异常路径 (赋值 null) ---");
            NeedInitObjectImmutable<string> structWrapper = new();
            Assert.ThrowsException<InvalidOperationException>(
                () => structWrapper.Value = null!, "结构体包装器赋值 null 应抛异常");

            NeedInitObject<string> classWrapper = new();
            Assert.ThrowsException<InvalidOperationException>(
                () => classWrapper.Value = null!, "类包装器赋值 null 应抛异常");

            Log("赋值 null 抛 InvalidOperationException, 通过");
        }

        [TestMethod]
        public void 值类型默认值场景_HasValue不受Inited约束()
        {
            Log("--- 测试: 值类型默认值场景 (无约束 T 下 T? 即 T, 非 Nullable<T>) ---");
            // 无约束泛型下私有字段 T? value 对 int 即 int (默认 0), 0 != null 恒为 true,
            // 因此值类型包装器即使未赋值 HasValue 也为 true (HasValue 只判 null, 与 Inited 无关)
            NeedInitObjectImmutable<int> structWrapper = new();
            NeedInitObject<int> classWrapper = new();

            Assert.IsFalse(structWrapper.Inited, "未赋值 Inited 应为 false");
            Assert.IsTrue(structWrapper.HasValue, "int 默认值 0 非 null, HasValue 应为 true");
            Assert.IsFalse(classWrapper.Inited, "未赋值 Inited 应为 false");
            Assert.IsTrue(classWrapper.HasValue, "int 默认值 0 非 null, HasValue 应为 true");
            Log("值类型 (int) 未赋值时: Inited=false, HasValue=true (默认值 0 非 null), 通过");

            // 引用类型对照: string 默认 null, 未赋值 HasValue 为 false
            NeedInitObject<string> refWrapper = new();
            Assert.IsFalse(refWrapper.HasValue, "string 默认 null, 未赋值 HasValue 应为 false");
            Log("引用类型 (string) 未赋值时: Inited=false, HasValue=false, 通过");
        }
    }
}
