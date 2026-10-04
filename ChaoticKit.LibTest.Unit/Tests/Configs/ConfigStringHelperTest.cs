using ChaoticKit.Module.Config;

namespace ChaoticKit.LibTest.Unit.Configs.Dynamic
{
    /// <summary>
    /// 验证配置字符串帮助类 (<see cref="ConfigStringHelper"/>) 的 Try 方法与回退语义:
    /// TryXxx 返回 <see langword="false"/> 表示配置字符串规则无法转换, 调用方可以接着尝试别的方案;
    /// 对应的非 Try 方法在失败时使用原有的回退值
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 断言基于 Debug 编译与 zh-CN 区域设置 (小数点为 ".") 下的行为。
    /// </remarks>
    [TestClass]
    public sealed class ConfigStringHelperTest : UnitTestBase
    {
        /// <summary>无法表达为配置字符串的类型: 只有 ToString 能给出字符串</summary>
        private sealed class OpaqueValue
        {
            public override string ToString() => "opaque-text";
        }

        /// <summary>内置类型、列表与 null 都能按配置字符串规则转换出来</summary>
        [TestMethod]
        public void TryObj2ConfigValue_ConvertsBuiltInValuesAndLists()
        {
            Assert.IsTrue(ConfigStringHelper.TryObj2ConfigValue(30, out var number), "int 应可转换");
            Assert.AreEqual("30", number, "int 应按不变区域设置转换");

            Assert.IsTrue(ConfigStringHelper.TryObj2ConfigValue(DayOfWeek.Friday, out var weekday), "枚举应可转换");
            Assert.AreEqual("Friday", weekday, "已定义的枚举应输出成员名");

            Assert.IsTrue(ConfigStringHelper.TryObj2ConfigValue(new[] { 1, 2, 3 }, out var list), "整数数组应可转换");
            Assert.AreEqual("1; 2; 3", list, "列表应用 \"; \" 连接");

            Assert.IsTrue(ConfigStringHelper.TryObj2ConfigValue(null, out var nullValue), "null 应视为空配置值");
            Assert.IsNull(nullValue, "null 的转换结果应为 null");
        }

        /// <summary>无法表达为配置字符串时返回 false, 结果不会落到 ToString</summary>
        [TestMethod]
        public void TryObj2ConfigValue_UnknownType_ReturnsFalse()
        {
            var opaque = new OpaqueValue();

            Assert.IsFalse(ConfigStringHelper.TryObj2ConfigValue(opaque, out var value), "无法表达为配置字符串的类型应返回 false");
            Assert.IsNull(value, "转换失败时结果应为 null");

            Assert.AreEqual("opaque-text", ConfigStringHelper.Obj2ConfigValue(opaque), "非 Try 方法在失败时按原有语义回退到 ToString");
        }

        /// <summary>字符串能转换时返回 true, 并给出转换结果</summary>
        [TestMethod]
        public void TryConfigValue2Obj_ConvertsSupportedTypes()
        {
            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj("25", typeof(int), out var number), "数字字符串应可转换");
            Assert.AreEqual(25, number, "数字字符串应转换为 int");

            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj("Friday", typeof(DayOfWeek), out var weekday), "枚举名应可转换");
            Assert.AreEqual(DayOfWeek.Friday, weekday, "枚举名应转换为枚举成员");

            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj("是", typeof(bool), out var enable), "宽松布尔字符串应可转换");
            Assert.AreEqual(true, enable, "宽松布尔规则应把 \"是\" 视为 true");

            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj("5", typeof(int?), out var nullable), "可空类型应可转换");
            Assert.AreEqual(5, nullable, "可空类型应转换为 5");
        }

        /// <summary>无法转换时返回 false, 结果不会取目标类型的默认值</summary>
        [TestMethod]
        public void TryConfigValue2Obj_Unconvertible_ReturnsFalse()
        {
            Assert.IsFalse(ConfigStringHelper.TryConfigValue2Obj("abc", typeof(int), out var converted), "无法转换时应返回 false");
            Assert.IsNull(converted, "转换失败时结果应为 null");

            Assert.AreEqual(0, ConfigStringHelper.ConfigValue2Obj("abc", typeof(int)), "非 Try 方法对非可空值类型回退到类型默认值");
            Assert.IsNull(ConfigStringHelper.ConfigValue2Obj("abc", typeof(int?)), "非 Try 方法对可空类型回退到 null");
            Assert.AreEqual(DayOfWeek.Sunday, ConfigStringHelper.ConfigValue2Obj("abc", typeof(DayOfWeek)), "非 Try 方法对枚举回退到零值");
        }

        /// <summary>输入为空属于"成功但不产生值": Try 返回 true 且结果为 null</summary>
        [TestMethod]
        public void TryConfigValue2Obj_EmptyInput_ReturnsTrueWithNull()
        {
            var empty2Null = new ConfigStringHelper.ConfigValue2ObjOptions
            {
                EmptyValueConvertWay = ConfigStringHelper.EmptyValueConvertWays.Empty2NullResult,
            };

            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj(null, typeof(int), out var nullValue), "null 输入应视为转换成功");
            Assert.IsNull(nullValue, "null 输入的结果应为 null");

            Assert.IsTrue(ConfigStringHelper.TryConfigValue2Obj("", typeof(int), out var emptyValue, empty2Null), "空值按选项应视为转换成功");
            Assert.IsNull(emptyValue, "空值按选项的结果应为 null");
            Assert.IsNull(ConfigStringHelper.ConfigValue2Obj("", typeof(int), empty2Null), "非 Try 方法应同样返回 null");

            Assert.AreEqual(0, ConfigStringHelper.ConfigValue2Obj("", typeof(int)), "未启用空值选项时, 空字符串按转换失败回退为默认值");
        }
    }
}
