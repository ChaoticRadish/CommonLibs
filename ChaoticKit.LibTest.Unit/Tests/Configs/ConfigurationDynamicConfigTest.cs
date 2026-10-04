using ChaoticKit.Microsoft.Configuration;
using ChaoticKit.Module.Config;
using Microsoft.Extensions.Configuration;

namespace ChaoticKit.LibTest.Unit.Configs.Dynamic
{
    /// <summary>
    /// 验证基于配置节 (Microsoft.Extensions.Configuration) 的动态配置读取器行为:
    /// 标量取值与类型转换、嵌套节绑定为对象、别名映射、路径键, 以及取不到值与转换失败时的默认值
    /// </summary>
    [TestClass]
    public sealed class ConfigurationDynamicConfigTest : UnitTestBase
    {
        /// <summary>配置节绑定的目标类型 (需要公开无参构造函数与可写属性)</summary>
        public sealed class MailCredentials
        {
            public string? User { get; set; }
            public string? Password { get; set; }
        }

        /// <summary>CreateInstance 使用的配置类型, 含一个由子节绑定的对象属性</summary>
        public sealed class MailSettings
        {
            public string? Server { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public int Timeout { get; set; }
            public DayOfWeek Weekday { get; set; }
            public MailCredentials? Credentials { get; set; }
        }

        /// <summary>构造测试用配置</summary>
        private static IConfigurationRoot CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Mail:Server"] = "smtp.example.com",
                    ["Mail:Port"] = "587",
                    ["Mail:EnableSsl"] = "true",
                    ["Mail:Timeout"] = "30",
                    ["Mail:Weekday"] = "Friday",
                    ["Mail:Credentials:User"] = "user01",
                    ["Mail:Credentials:Password"] = "p@ssw0rd",
                })
                .Build();
        }

        /// <summary>把 Mail 配置节适配为动态配置读取器</summary>
        private static IDynamicConfig CreateMailConfig(IReadOnlyDictionary<string, string>? aliases = null)
        {
            return CreateConfiguration().GetSection("Mail").AsDynamicConfig(aliases);
        }

        /// <summary>标量键的取值与类型转换</summary>
        [TestMethod]
        public void FromSection_ReadsScalarsAndConvertsTypes()
        {
            IDynamicConfig config = CreateMailConfig();

            Assert.AreEqual("smtp.example.com", config.GetValue<string>("Server"), "配置节的标量值应取出");
            Assert.AreEqual(587, config.GetValue<int>("Port"), "配置节的字符串值应转换为 int");
            Assert.IsTrue(config.GetValue<bool>("EnableSsl"), "配置节的字符串值应转换为 bool");
            Assert.AreEqual(30, config.GetValue<int>("Timeout"), "配置节的字符串值应转换为 int");
            Assert.AreEqual(DayOfWeek.Friday, config.GetValue<DayOfWeek>("Weekday"), "配置节的字符串值应转换为枚举");
            Assert.AreEqual("smtp.example.com", config.GetValue<string>("server"), "IConfiguration 的键本身大小写不敏感");
        }

        /// <summary>键指向子节时, 整个子节应按目标类型绑定为对象</summary>
        [TestMethod]
        public void FromSection_BindsNestedSectionToObject()
        {
            IDynamicConfig config = CreateMailConfig();

            MailCredentials? credentials = config.GetValue<MailCredentials>("Credentials");

            Assert.IsNotNull(credentials, "嵌套配置节应绑定为目标类型的实例");
            Assert.AreEqual("user01", credentials!.User, "绑定后 User 应被填充");
            Assert.AreEqual("p@ssw0rd", credentials.Password, "绑定后 Password 应被填充");
        }

        /// <summary>别名表把取值的键替换为映射到的键</summary>
        [TestMethod]
        public void FromSection_AliasMapping()
        {
            var aliases = new Dictionary<string, string>
            {
                ["host"] = "Server",
                ["ssl"] = "EnableSsl",
            };

            IDynamicConfig config = CreateMailConfig(aliases);

            Assert.AreEqual("smtp.example.com", config.GetValue<string>("host"), "别名 host 应映射到 Server");
            Assert.IsTrue(config.GetValue<bool>("ssl"), "别名 ssl 应映射到 EnableSsl 并转换类型");
            Assert.AreEqual("smtp.example.com", config.GetValue<string>("Server"), "别名是叠加映射, 原键仍可直接使用");
            Assert.IsNull(config.GetValue<string>("smtp"), "未命中别名的键应取不到值");
        }

        /// <summary>键不存在或值无法转换时返回默认值, 不抛异常</summary>
        [TestMethod]
        public void FromSection_MissingOrUnconvertible_ReturnsDefaultValue()
        {
            IDynamicConfig config = CreateMailConfig();

            Assert.IsNull(config.GetValue<string>("Missing"), "键不存在时应返回 null");
            Assert.AreEqual(-1, config.GetValue<int>("Missing", -1), "键不存在时应返回传入的默认值");
            Assert.AreEqual(-1, config.GetValue<int>("Server", -1), "值无法转换为目标类型时应返回默认值");
            Assert.AreEqual("fallback", config.GetValue<string>("Credentials", "fallback"), "子节无法绑定为目标类型时应返回默认值");
        }

        /// <summary>根节点也可以适配, 并支持带冒号的路径键</summary>
        [TestMethod]
        public void FromRootConfiguration_PathKeysAndAliases()
        {
            IDynamicConfig config = CreateConfiguration().AsDynamicConfig();

            Assert.AreEqual("smtp.example.com", config.GetValue<string>("Mail:Server"), "根节点的路径键应可取值");
            Assert.AreEqual(587, config.GetValue<int>("Mail:Port"), "根节点的路径键应可转换类型");

            var aliases = new Dictionary<string, string> { ["smtp"] = "Mail:Server" };
            IDynamicConfig aliased = CreateConfiguration().AsDynamicConfig(aliases);
            Assert.AreEqual("smtp.example.com", aliased.GetValue<string>("smtp"), "别名应可映射到带冒号的路径键");
        }

        /// <summary>泛型取值、TryGetValue 与 CreateInstance 在配置节来源下同样可用</summary>
        [TestMethod]
        public void FromSection_GenericGetAndCreateInstance()
        {
            IDynamicConfig config = CreateMailConfig();

            Assert.IsTrue(config.TryGetValue("Timeout", out int timeout), "TryGetValue 应取到值");
            Assert.AreEqual(30, timeout, "TryGetValue 取出的值应正确");

            MailSettings settings = config.CreateInstance<MailSettings>();

            Assert.AreEqual("smtp.example.com", settings.Server, "CreateInstance 应填充标量属性");
            Assert.AreEqual(587, settings.Port, "CreateInstance 应转换并填充数值属性");
            Assert.IsTrue(settings.EnableSsl, "CreateInstance 应转换并填充布尔属性");
            Assert.AreEqual(30, settings.Timeout, "CreateInstance 应转换并填充数值属性");
            Assert.AreEqual(DayOfWeek.Friday, settings.Weekday, "CreateInstance 应转换并填充枚举属性");
            Assert.IsNotNull(settings.Credentials, "CreateInstance 应把子节填充为对象属性");
            Assert.AreEqual("user01", settings.Credentials!.User, "子节对象的属性应被填充");
        }

        /// <summary>配置节为 null 时抛出 ArgumentNullException</summary>
        [TestMethod]
        public void AsDynamicConfig_ThrowsOnNullSection()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => DynamicConfigConfigurationExtensions.AsDynamicConfig((IConfiguration)null!),
                "配置节为 null 应抛 ArgumentNullException");
        }
    }
}
