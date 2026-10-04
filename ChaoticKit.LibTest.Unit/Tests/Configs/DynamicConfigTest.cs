using ChaoticKit.Data.Constraint;
using ChaoticKit.Module.Config;
using System.Collections.Specialized;
using System.Globalization;

namespace ChaoticKit.LibTest.Unit.Configs.Dynamic
{
    /// <summary>
    /// 验证动态配置读取器 (<see cref="DynamicConfig"/>) 的行为:
    /// 对象来源、字符串字典来源、带类型字典来源与 NameValueCollection 来源,
    /// 以及别名、大小写敏感度、类型转换、默认值、泛型取值、TryGetValue 与 CreateInstance
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// ① 全部断言基于 Debug 编译与 zh-CN 区域设置 (小数点为 ".") 下的行为。
    /// ② 库的取值规则是"静默降级": 取不到值或转换失败都返回默认值, 不抛异常, 本测试锁定该行为。
    /// </remarks>
    [TestClass]
    public sealed class DynamicConfigTest : UnitTestBase
    {
        /// <summary>测试用固定 Guid, 用于验证 Guid 与字符串之间的转换</summary>
        private static readonly Guid SampleId = new("0f8fad5b-d9cb-469f-a165-70867728950e");

        /// <summary>测试用固定时间, 用于验证 DateTime 读取</summary>
        private static readonly DateTime SampleTime = new(2024, 1, 2, 3, 4, 5);

        /// <summary>测试用配置对象, 覆盖字符串、数值、布尔、枚举、Guid、时间、可空类型、嵌套对象、只读属性、索引器与字段</summary>
        public sealed class SampleOptions
        {
            /// <summary>公开字段: 不属于动态配置的读取范围</summary>
            public string PublicField = "field-value";

            public string Server { get; set; } = "mail.example.com";
            public string Port { get; set; } = "25";
            public bool EnableSsl { get; set; } = true;
            public int Timeout { get; set; } = 30;
            public double Ratio { get; set; } = 0.5;
            public DayOfWeek Weekday { get; set; } = DayOfWeek.Friday;
            public Guid Id { get; set; } = SampleId;
            public DateTime Created { get; set; } = SampleTime;
            public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
            public int? Retry { get; set; } = 3;
            public int? NoRetry { get; set; }
            public NestedOptions Nested { get; set; } = new();

            /// <summary>只读属性: 可以读取, 但不会被 CreateInstance 填充</summary>
            public string ReadOnlyText { get; } = "readonly-value";

            /// <summary>索引器: 不属于动态配置的读取范围</summary>
            public string this[int index] => $"index-{index}";
        }

        /// <summary>用于验证嵌套对象读取的类型</summary>
        public sealed class NestedOptions
        {
            public string? Text { get; set; } = "nested-text";
        }

        /// <summary>用于验证 CreateInstance 填充行为的类型</summary>
        public sealed class InstanceOptions
        {
            public string? Server { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public int Timeout { get; set; }
            public DayOfWeek Weekday { get; set; } = DayOfWeek.Sunday;
            public int? Retry { get; set; }
            public NestedOptions? Nested { get; set; }

            /// <summary>只读属性: 应被跳过并保留原值</summary>
            public string ReadOnlyText { get; } = "keep-me";

            /// <summary>索引器: 应被跳过</summary>
            public string this[int index] => $"index-{index}";
        }

        /// <summary>实现了 IStringConveying 的类型: 按约定与字符串无损互转</summary>
        public sealed class ConveyingValue : IStringConveying<ConveyingValue>
        {
            public int Number { get; set; }

            public static explicit operator ConveyingValue(string s)
                => new() { Number = int.Parse(s.TrimStart('#'), CultureInfo.InvariantCulture) };

            public static explicit operator string(ConveyingValue t)
                => "#" + t.Number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>用于验证"对象 → 配置字符串 → 对象"往返的类型</summary>
        public sealed class ConveyingOptions
        {
            public ConveyingValue Value { get; set; } = new() { Number = 1 };
            public List<int> Ports { get; set; } = new() { 1, 2, 3 };
            public DateTime Created { get; set; } = SampleTime;
            public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
        }

        #region 对象来源

        /// <summary>对象来源下, 字符串、数值与布尔值按目标类型互相转换</summary>
        [TestMethod]
        public void FromObject_KeyedLookup_ConvertsStringNumberAndBoolean()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.AreEqual("mail.example.com", config.GetValue<string>("Server"), "字符串属性应原样取出");
            Assert.AreEqual("25", config.GetValue<string>("Port"), "字符串属性取字符串应原样返回");
            Assert.AreEqual(25, config.GetValue<int>("Port"), "字符串 \"25\" 应转换为 int 25");
            Assert.IsTrue(config.GetValue<bool>("EnableSsl"), "布尔属性应取出 true");
            Assert.AreEqual("True", config.GetValue<string>("EnableSsl"), "布尔转字符串应得到 \"True\"");
            Assert.AreEqual(30, config.GetValue<int>("Timeout"), "int 属性应原样取出");
            Assert.AreEqual("30", config.GetValue<string>("Timeout"), "int 转字符串应得到 \"30\"");
            Assert.AreEqual(0.5, config.GetValue<double>("Ratio"), "double 属性应原样取出");
        }

        /// <summary>对象来源下, 枚举、Guid、DateTime、TimeSpan 与可空类型的转换</summary>
        [TestMethod]
        public void FromObject_ConvertsEnumGuidAndTime()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.AreEqual(DayOfWeek.Friday, config.GetValue<DayOfWeek>("Weekday"), "枚举属性应原样取出");
            Assert.AreEqual("Friday", config.GetValue<string>("Weekday"), "枚举转字符串应得到成员名");
            Assert.AreEqual(SampleId, config.GetValue<Guid>("Id"), "Guid 属性应原样取出");
            Assert.AreEqual(SampleId.ToString(), config.GetValue<string>("Id"), "Guid 转字符串应得到默认格式");
            Assert.AreEqual(SampleTime, config.GetValue<DateTime>("Created"), "DateTime 属性应原样取出");
            Assert.AreEqual(TimeSpan.FromMinutes(5), config.GetValue<TimeSpan>("Interval"), "TimeSpan 属性应原样取出");
            Assert.AreEqual(3, config.GetValue<int?>("Retry"), "可空 int 属性应取出 3");
            Assert.IsNull(config.GetValue<int?>("NoRetry"), "值为 null 的可空属性应返回 null");
        }

        /// <summary>别名表把取值的键替换为映射到的键, 且不影响原键直接取值</summary>
        [TestMethod]
        public void FromObject_AliasMapping()
        {
            var aliases = new Dictionary<string, string>
            {
                ["host"] = "Server",
                ["port"] = "Port",
            };

            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions(), aliases);

            Assert.AreEqual("mail.example.com", config.GetValue<string>("host"), "别名 host 应映射到 Server");
            Assert.AreEqual(25, config.GetValue<int>("port"), "别名 port 应映射到 Port 并转换类型");
            Assert.AreEqual(30, config.GetValue<int>("Timeout"), "未命中别名的键仍按原名读取");
            Assert.AreEqual("mail.example.com", config.GetValue<string>("Server"), "别名是叠加映射, 原键仍可直接使用");
        }

        /// <summary>默认大小写不敏感, 也可以指定 Ordinal 改成大小写敏感</summary>
        [TestMethod]
        public void FromObject_KeyComparison_DefaultInsensitiveButCanBeOrdinal()
        {
            IDynamicConfig insensitive = DynamicConfig.FromObject(new SampleOptions());
            Assert.AreEqual("mail.example.com", insensitive.GetValue<string>("SERVER"), "默认比较器应大小写不敏感");
            Assert.AreEqual(30, insensitive.GetValue<int>("TIMEOUT"), "默认比较器应大小写不敏感");

            IDynamicConfig sensitive = DynamicConfig.FromObject(new SampleOptions(), keyComparer: StringComparer.Ordinal);
            Assert.IsNull(sensitive.GetValue<string>("server"), "Ordinal 比较器下键大小写不一致应取不到值");
            Assert.AreEqual("mail.example.com", sensitive.GetValue<string>("Server"), "Ordinal 比较器下键大小写一致应取到值");
        }

        /// <summary>键不存在时返回调用方传入的默认值</summary>
        [TestMethod]
        public void FromObject_MissingKey_ReturnsDefaultValue()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.IsNull(config.GetValue<string>("Missing"), "引用类型未取到值应返回 null");
            Assert.AreEqual(0, config.GetValue<int>("Missing"), "值类型未取到值应返回 default");
            Assert.AreEqual(-1, config.GetValue<int>("Missing", -1), "未取到值应返回传入的默认值");
            Assert.AreEqual("fallback", config.GetValue<string>("Missing", "fallback"), "未取到值应返回传入的默认值");
        }

        /// <summary>公开字段与索引器都不属于读取范围, 且索引器不会导致异常</summary>
        [TestMethod]
        public void FromObject_IgnoresFieldsAndIndexers()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.AreEqual("readonly-value", config.GetValue<string>("ReadOnlyText"), "只读属性应可读取");
            Assert.IsNull(config.GetValue<string>("PublicField"), "公开字段不属于读取范围");
            Assert.IsNull(config.GetValue<string>("Item"), "索引器不属于读取范围");
        }

        /// <summary>复杂对象无法表达为配置字符串, 转字符串应失败; 但按自身类型可以取出</summary>
        [TestMethod]
        public void FromObject_ComplexObject_IsNotFlattenedToString()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.IsNull(config.GetValue<string>("Nested"), "配置字符串规则无法表达复杂对象, 应返回默认值");
            Assert.IsInstanceOfType(config.GetValue<NestedOptions>("Nested"), typeof(NestedOptions), "复杂对象按其自身类型应可取出");
            Assert.IsNotNull(config.GetValue<object>("Nested"), "目标类型为 object 时应取出原对象");
        }

        #endregion

        #region 字典与集合来源

        /// <summary>字符串字典来源: 字符串值按目标类型转换</summary>
        [TestMethod]
        public void FromDictionary_StringValues_ConvertByTargetType()
        {
            var source = new Dictionary<string, string?>
            {
                ["Host"] = "srv-01",
                ["Port"] = "8080",
                ["Enable"] = "true",
                ["Ratio"] = "1.25",
                ["Weekday"] = "Friday",
                ["Created"] = "2024-01-02T03:04:05",
                ["Id"] = SampleId.ToString(),
            };

            IDynamicConfig config = DynamicConfig.FromDictionary(source);

            Assert.AreEqual("srv-01", config.GetValue<string>("Host"), "字符串值应原样取出");
            Assert.AreEqual(8080, config.GetValue<int>("Port"), "字符串值应转换为 int");
            Assert.IsTrue(config.GetValue<bool>("Enable"), "字符串值应转换为 bool");
            Assert.AreEqual(1.25, config.GetValue<double>("Ratio"), "字符串值应转换为 double");
            Assert.AreEqual(DayOfWeek.Friday, config.GetValue<DayOfWeek>("Weekday"), "字符串值应转换为枚举");
            Assert.AreEqual(new DateTime(2024, 1, 2, 3, 4, 5), config.GetValue<DateTime>("Created"), "字符串值应转换为 DateTime");
            Assert.AreEqual(SampleId, config.GetValue<Guid>("Id"), "字符串值应转换为 Guid");
            Assert.AreEqual("srv-01", config.GetValue<string>("host"), "默认比较器应大小写不敏感");
        }

        /// <summary>已带类型的值字典: 值本身已是目标类型时直接命中, 内置类型仍可转为字符串</summary>
        [TestMethod]
        public void FromDictionary_TypedValues_AreUsedAsIs()
        {
            var nested = new NestedOptions { Text = "nested-from-dictionary" };
            var source = new Dictionary<string, object?>
            {
                ["Count"] = 10,
                ["Enable"] = true,
                ["Created"] = new DateTime(2024, 5, 6, 7, 8, 9),
                ["Nested"] = nested,
            };

            IDynamicConfig config = DynamicConfig.FromDictionary(source);

            Assert.AreEqual(10, config.GetValue<int>("Count"), "已带类型的值应原样取出");
            Assert.AreEqual("10", config.GetValue<string>("Count"), "内置类型应可转为字符串");
            Assert.IsTrue(config.GetValue<bool>("Enable"), "已带类型的布尔值应原样取出");
            Assert.AreEqual(new DateTime(2024, 5, 6, 7, 8, 9), config.GetValue<DateTime>("Created"), "已带类型的时间应原样取出");
            Assert.AreSame(nested, config.GetValue<NestedOptions>("Nested"), "已带类型的复杂对象应取到同一实例");
        }

        /// <summary>NameValueCollection 来源: 与索引器一致, 多值取逗号连接的字符串</summary>
        [TestMethod]
        public void FromNameValueCollection_ReadsValuesAndJoinsMultipleValues()
        {
            var collection = new NameValueCollection
            {
                ["Server"] = "srv-02",
                ["Timeout"] = "30",
            };
            collection.Add("Tag", "a");
            collection.Add("Tag", "b");

            IDynamicConfig config = DynamicConfig.FromNameValueCollection(collection);

            Assert.AreEqual("srv-02", config.GetValue<string>("Server"), "应按键取出字符串值");
            Assert.AreEqual(30, config.GetValue<int>("Timeout"), "字符串值应转换为 int");
            Assert.AreEqual("srv-02", config.GetValue<string>("server"), "默认比较器应大小写不敏感");
            Assert.AreEqual("a,b", config.GetValue<string>("Tag"), "同一个键有多个值时, 应取逗号连接的字符串");
        }

        #endregion

        #region 泛型取值与实例填充

        /// <summary>泛型 GetValue 与 TryGetValue 的行为</summary>
        [TestMethod]
        public void GenericGetValue_And_TryGetValue()
        {
            IDynamicConfig config = DynamicConfig.FromObject(new SampleOptions());

            Assert.AreEqual(30, config.GetValue<int>("Timeout"), "泛型取值应返回 int");
            Assert.AreEqual(30, config.GetValue("Timeout", 0), "泛型取值应能由默认值推断类型参数");

            Assert.IsTrue(config.TryGetValue("Timeout", out int timeout), "取到值时应返回 true");
            Assert.AreEqual(30, timeout, "TryGetValue 取出的值应正确");

            Assert.IsTrue(config.TryGetValue("Server", out string? server), "字符串键应能取到值");
            Assert.AreEqual("mail.example.com", server, "TryGetValue 取出的字符串应正确");

            Assert.IsFalse(config.TryGetValue("Missing", out string? missing), "键不存在时应返回 false");
            Assert.IsNull(missing, "键不存在时 out 参数应为 null");
        }

        /// <summary>CreateInstance 按属性名逐项填充, 并跳过只读属性与索引器</summary>
        [TestMethod]
        public void CreateInstance_FillsWritableProperties()
        {
            var source = new Dictionary<string, string?>
            {
                ["Server"] = "srv-03",
                ["Port"] = "8080",
                ["EnableSsl"] = "true",
                ["Timeout"] = "45",
                ["Weekday"] = "Monday",
                ["Retry"] = "7",
                ["Unknown"] = "ignored",
            };

            InstanceOptions instance = DynamicConfig.FromDictionary(source).CreateInstance<InstanceOptions>();

            Assert.AreEqual("srv-03", instance.Server, "字符串属性应被填充");
            Assert.AreEqual(8080, instance.Port, "字符串值应转换后填充到 int 属性");
            Assert.IsTrue(instance.EnableSsl, "字符串值应转换后填充到 bool 属性");
            Assert.AreEqual(45, instance.Timeout, "字符串值应转换后填充到 int 属性");
            Assert.AreEqual(DayOfWeek.Monday, instance.Weekday, "字符串值应转换后填充到枚举属性");
            Assert.AreEqual(7, instance.Retry, "字符串值应转换后填充到可空 int 属性");
            Assert.AreEqual("keep-me", instance.ReadOnlyText, "只读属性应被跳过并保留原值");
            Assert.IsNull(instance.Nested, "来源中没有的属性应保留默认值");
        }

        /// <summary>来源为空时, CreateInstance 得到保留默认值的实例</summary>
        [TestMethod]
        public void CreateInstance_EmptySource_KeepsDefaults()
        {
            IDynamicConfig config = DynamicConfig.FromDictionary(new Dictionary<string, string?>());

            InstanceOptions instance = config.CreateInstance<InstanceOptions>();

            Assert.IsNull(instance.Server, "取不到值时应保留 null");
            Assert.AreEqual(0, instance.Port, "取不到值时应保留 default");
            Assert.AreEqual(DayOfWeek.Sunday, instance.Weekday, "取不到值时应保留属性初始值");
            Assert.AreEqual("keep-me", instance.ReadOnlyText, "只读属性应保留其原值");
        }

        #endregion

        #region 配置字符串规则

        /// <summary>字符串取值遵循 ChaoticKit 的配置字符串规则 (宽松布尔、已定义枚举数值), 未覆盖的类型由通用转换器兜底</summary>
        [TestMethod]
        public void FromDictionary_StringValues_FollowConfigStringRules()
        {
            var source = new Dictionary<string, string?>
            {
                ["Enable"] = "是",
                ["Disable"] = "不",
                ["Weekday"] = "5",
                ["Interval"] = "00:05:00",
            };

            IDynamicConfig config = DynamicConfig.FromDictionary(source);

            Assert.IsTrue(config.GetValue<bool>("Enable"), "宽松布尔规则应把 \"是\" 视为 true");
            Assert.IsFalse(config.GetValue<bool>("Disable"), "宽松布尔规则应把 \"不\" 视为 false");
            Assert.AreEqual(DayOfWeek.Friday, config.GetValue<DayOfWeek>("Weekday"), "已定义的枚举数值应转换为枚举成员");
            Assert.AreEqual(TimeSpan.FromMinutes(5), config.GetValue<TimeSpan>("Interval"), "配置字符串规则未覆盖的类型应由通用转换器兜底");
        }

        /// <summary>值无法按配置字符串规则转换时仍返回默认值, 不会因为规则严格而误判为成功</summary>
        [TestMethod]
        public void FromDictionary_UnconvertibleStrings_ReturnDefaultValue()
        {
            var source = new Dictionary<string, string?>
            {
                ["Port"] = "smtp.example.com",
                ["Weekday"] = "99",
            };

            IDynamicConfig config = DynamicConfig.FromDictionary(source);

            Assert.AreEqual(-1, config.GetValue<int>("Port", -1), "无法转换的字符串应返回默认值");
            Assert.IsFalse(config.TryGetValue<TimeSpan>("Port", out _), "无法转换为 TimeSpan 时应返回 false");
            Assert.AreEqual(
                (DayOfWeek)99,
                config.GetValue<DayOfWeek>("Weekday"),
                "配置字符串规则判失败后, 宽松的通用转换器仍会接受未定义的枚举数值");
        }

        /// <summary>实现了 IStringConveying 的类型可以与配置字符串无损互转</summary>
        [TestMethod]
        public void FromObject_IStringConveyingType_ConvertsBothWays()
        {
            IDynamicConfig fromObject = DynamicConfig.FromObject(new ConveyingOptions());

            Assert.AreEqual("#1", fromObject.GetValue<string>("Value"), "IStringConveying 类型应按约定的字符串形式输出");

            IDynamicConfig fromText = DynamicConfig.FromDictionary(new Dictionary<string, string?> { ["Value"] = "#42" });

            ConveyingValue? value = fromText.GetValue<ConveyingValue>("Value");
            Assert.IsNotNull(value, "IStringConveying 类型应可由配置字符串还原");
            Assert.AreEqual(42, value!.Number, "还原后的值应正确");

            ConveyingValue created = fromText.CreateInstance<ConveyingOptions>().Value;
            Assert.AreEqual(42, created.Number, "CreateInstance 也应能填充 IStringConveying 类型的属性");
        }

        /// <summary>对象 → 配置字符串 → 对象 的往返: IStringConveying、列表、DateTime; 另验证规则未覆盖类型的兜底方向</summary>
        [TestMethod]
        public void ConfigString_RoundTripsObjectToTextAndBack()
        {
            IDynamicConfig fromObject = DynamicConfig.FromObject(new ConveyingOptions());

            string? conveyingText = fromObject.GetValue<string>("Value");
            string? portsText = fromObject.GetValue<string>("Ports");
            string? createdText = fromObject.GetValue<string>("Created");

            Assert.AreEqual("1; 2; 3", portsText, "列表应按配置字符串规则用 \"; \" 连接");
            Assert.AreEqual(SampleTime.ToString("o", CultureInfo.InvariantCulture), createdText, "DateTime 应按可往返的 \"o\" 格式输出");
            Assert.IsNull(fromObject.GetValue<string>("Interval"), "配置字符串规则未覆盖 TimeSpan, 转字符串应失败且不退回 ToString");

            IDynamicConfig fromText = DynamicConfig.FromDictionary(new Dictionary<string, string?>
            {
                ["Value"] = conveyingText,
                ["Ports"] = portsText,
                ["Created"] = createdText,
                ["Interval"] = "00:05:00",
            });

            ConveyingOptions restored = fromText.CreateInstance<ConveyingOptions>();

            Assert.AreEqual(1, restored.Value.Number, "IStringConveying 类型应可由配置字符串还原");
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, restored.Ports, "列表应可由配置字符串还原");
            Assert.AreEqual(SampleTime, restored.Created, "DateTime 应可由 \"o\" 格式字符串还原");
            Assert.AreEqual(TimeSpan.FromMinutes(5), restored.Interval, "配置字符串规则未覆盖的类型应由通用转换器兜底还原");
        }

        #endregion

        #region 参数校验与转换工具

        /// <summary>工厂方法对 null 参数抛出 ArgumentNullException</summary>
        [TestMethod]
        public void Factories_ThrowOnNullArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => DynamicConfig.FromObject(null!), "目标对象为 null 应抛 ArgumentNullException");
            Assert.ThrowsException<ArgumentNullException>(
                () => DynamicConfig.FromDictionary((IReadOnlyDictionary<string, string?>)null!), "字符串字典为 null 应抛 ArgumentNullException");
            Assert.ThrowsException<ArgumentNullException>(
                () => DynamicConfig.FromDictionary((IReadOnlyDictionary<string, object?>)null!), "对象字典为 null 应抛 ArgumentNullException");
            Assert.ThrowsException<ArgumentNullException>(
                () => DynamicConfig.FromNameValueCollection(null!), "名称/值集合为 null 应抛 ArgumentNullException");
        }

        /// <summary>值转换工具对 null 与不可转换值的处理</summary>
        [TestMethod]
        public void TryConvertValue_HandlesNullAndUnconvertibleValues()
        {
            Assert.IsTrue(DynamicConfig.TryConvertValue(null, typeof(string), out var nullString), "null 转引用类型应成功");
            Assert.IsNull(nullString, "null 转引用类型的结果应为 null");

            Assert.IsTrue(DynamicConfig.TryConvertValue(null, typeof(int?), out var nullNullable), "null 转可空值类型应成功");
            Assert.IsNull(nullNullable, "null 转可空值类型的结果应为 null");

            Assert.IsFalse(DynamicConfig.TryConvertValue(null, typeof(int), out var nullInt), "null 转非可空值类型应失败");
            Assert.IsNull(nullInt, "转换失败时结果应为 null");

            Assert.IsTrue(DynamicConfig.TryConvertValue("5", typeof(int?), out var convertible), "字符串应可转为可空 int");
            Assert.AreEqual(5, convertible, "字符串 \"5\" 应转为 5");

            Assert.IsFalse(DynamicConfig.TryConvertValue("abc", typeof(int), out var failed), "无法转换的字符串应返回 false");
            Assert.IsNull(failed, "转换失败时结果应为 null");
        }

        #endregion
    }
}
