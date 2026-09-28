using ChaoticKit.Attributes.General;
using ChaoticKit.Data.Structure.Pair;
using ChaoticKit.Module.Config;

namespace ChaoticKit.LibTest.Unit.Configs.Manager
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Configs.Manager001
    /// 验证 ConfigHelper + Json/Xml 双读写实现 + 缓存保护 (OnlyAllowAdd) 的全流程行为:
    /// 初始默认值 → 修改实例 → 双实现保存 → 缓存命中重取 → 仅 Xml 保存 → 缓存保护下保存被拒 → 关闭保护后 reload 取文件值
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// ① 断言已固定为 DEBUG 分支: 库的 DefaultValueAttribute("999", "123456") 在 DEBUG 编译下取调试值 "123456"
    ///    (Release 编译下为 "999"), 且 DEBUG 下 Json 实现写 "*.debug.json" 文件、Xml 实现写 DebugValue 节点。
    ///    本测试按 dotnet test 默认 Debug 配置设计, 若在 Release 配置下运行, 库行为整体不同, 需另行调整。
    /// ② 原控制台测试中的 Task.Delay(1000) 已移除 (文件写入为同步 + 释放即刷盘, 无需等待)。
    /// ③ 缓存是"按类型"的单一实例: Json/Xml 实现共用同一缓存对象, 修改引用即修改缓存, 与源测试语义一致。
    /// ④ ConfigHelper 为静态共享状态, 本类加 [DoNotParallelize] 避免与并行执行的其他测试互相干扰。
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public sealed class ConfigManagerTest : UnitTestBase
    {
        /// <summary>固定断言: DEBUG 分支下 TestModel002.ABC 的默认值 (DefaultValueAttribute 第二个参数)</summary>
        private const string ExpectedInitialAbc = "123456";

        private string _tempDir = null!;

        /// <summary>Json 实现的保存目录</summary>
        private string JsonDirPath => Path.Combine(_tempDir, "Json");

        /// <summary>Xml 实现的保存目录</summary>
        private string XmlDirPath => Path.Combine(_tempDir, "Xml");

        /// <summary>DEBUG 编译下 Json 实现写入的文件 (CreateDebugFilePath 追加 ".debug" 后缀)</summary>
        private string JsonConfigFilePath => Path.Combine(JsonDirPath, "TestModel002.debug.json");

        /// <summary>Xml 实现写入的文件 (无 debug 后缀, DEBUG 差异体现在节点名上)</summary>
        private string XmlConfigFilePath => Path.Combine(XmlDirPath, "TestModel002.xml");

        /// <summary>每个测试用例独立临时目录, 并在共享的 ConfigHelper 上注册指向该目录的 Json/Xml 实现</summary>
        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));

            // 复位共享静态状态, 避免跨测试用例残留
            ConfigHelper.OnlyAllowAdd = false;
            ConfigHelper.ClearCache();

            JsonConfigReadWriteImpl jsonImpl = new(JsonDirPath, false);
            ConfigHelper.SetImpl(RWImpls.Json, jsonImpl);
            XmlConfigReadWriteImpl xmlImpl = new(XmlDirPath, false);
            ConfigHelper.SetImpl(RWImpls.Xml, xmlImpl);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ConfigHelper.OnlyAllowAdd = false;
            ConfigHelper.ClearCache();
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, true);
                }
                catch (Exception ex)
                {
                    Log($"清理临时目录失败 (不影响测试结果): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 初始取值: 无文件时 Json 实现按 DefaultValue 特性生成默认值
        /// (ABC 固定断言 DEBUG 分支 "123456"; Release 分支为 "999")
        /// </summary>
        [TestMethod]
        public void GetConfig_Json实现_初始默认值()
        {
            Log("--- 测试: 使用 Json 实现获取默认值 ---");

            TestModel002 m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);

            Assert.AreEqual(ExpectedInitialAbc, m.ABC, "DEBUG 分支下 ABC 默认值应为 \"123456\"");
            Log($"m.ABC = \"{m.ABC}\", 通过");

            Assert.IsNotNull(m.Test, "Test 应有默认列表值");
            List<string> test = m.Test!;
            Assert.AreEqual(4, test.Count, "默认 Test 应为 4 项");
            Assert.AreEqual("999", test[0]);
            Assert.AreEqual("123456", test[1]);
            Assert.AreEqual("999", test[2]);
            Assert.AreEqual("12aqw", test[3]);
            Log($"m.Test = [{string.Join(", ", test)}], 通过");

            Assert.IsNull(m.T, "T 无默认值应为 null");
            Log("m.T = null, 通过");
        }

        /// <summary>
        /// 修改实例后用 Json / Xml 双实现分别保存, 重新获取 (命中缓存) 各字段一致;
        /// 再仅用 Xml 保存修改值, 重新获取仍一致
        /// </summary>
        [TestMethod]
        public void SaveConfig_Json与Xml双实现_保存后重新获取一致()
        {
            Log("--- 测试: Json / Xml 双实现保存后重新获取一致 ---");

            TestModel002 m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            m.ABC = "99asdasdasd";
            Assert.IsNotNull(m.Test, "Test 应非 null 才能修改首项");
            m.Test![0] = "qqq!!!";
            m.T = typeof(ITest001);
            m.Pair = ("qqq", "1 3");
            Log("m.ABC = \"99asdasdasd\"; m.Test[0] = \"qqq!!!\"; m.T = typeof(ITest001); m.Pair = (\"qqq\", \"1 3\");");

            ConfigHelper.SaveConfig(m, RWImpls.Json);
            ConfigHelper.SaveConfig(m, RWImpls.Xml);

            // 保存应真实写出文件 (DEBUG 下 Json 为 *.debug.json)
            Assert.IsTrue(File.Exists(JsonConfigFilePath), $"Json 配置文件应已写出: {JsonConfigFilePath}");
            Assert.IsTrue(File.Exists(XmlConfigFilePath), $"Xml 配置文件应已写出: {XmlConfigFilePath}");
            Log($"Json 文件已写出: {JsonConfigFilePath}");
            Log($"Xml 文件已写出: {XmlConfigFilePath}");

            // 使用 Json 重新获取 (命中缓存, 与 m 同一实例)
            m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            Assert.AreEqual("99asdasdasd", m.ABC, "Json 重新获取 ABC 应一致");
            Assert.AreEqual("qqq!!!", m.Test![0], "Json 重新获取 Test[0] 应一致");
            Assert.AreEqual(typeof(ITest001), m.T, "Json 重新获取 T 应一致");
            Assert.AreEqual("qqq", m.Pair.Group, "Json 重新获取 Pair.Group 应一致");
            Assert.AreEqual("1 3", m.Pair.Id, "Json 重新获取 Pair.Id 应一致");
            Log($"Json 重新获取: ABC=\"{m.ABC}\", Test[0]=\"{m.Test![0]}\", T={m.T?.Name}, Pair=(\"{m.Pair.Group}\", \"{m.Pair.Id}\"), 通过");

            // 使用 Xml 重新获取 (命中缓存)
            m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Xml);
            Assert.AreEqual("99asdasdasd", m.ABC, "Xml 重新获取 ABC 应一致");
            Log($"Xml 重新获取: ABC=\"{m.ABC}\", 通过");

            // 修改实例, 仅用 Xml 实现保存
            m.ABC = "54856451asdas";
            Log("m.ABC = \"54856451asdas\"; 仅用 Xml 保存;");
            ConfigHelper.SaveConfig(m, RWImpls.Xml);

            // 使用 Xml 重新获取
            m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Xml);
            Assert.AreEqual("54856451asdas", m.ABC, "Xml 保存后重新获取 ABC 应一致");
            Log($"Xml 重新获取: ABC=\"{m.ABC}\", 通过");
        }

        /// <summary>
        /// 缓存保护: OnlyAllowAdd = true 时 SaveConfig 被拒 (抛 InvalidOperationException),
        /// 缓存中的实例引用不受影响; 关闭保护后 reload:true 重新加载, 取到的是文件中的旧值
        /// </summary>
        [TestMethod]
        public void 缓存保护_OnlyAllowAdd时保存被拒_关闭后reload取文件值()
        {
            Log("--- 测试: 缓存保护 (OnlyAllowAdd) 行为 ---");

            TestModel002 m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            m.ABC = "99asdasdasd";
            Assert.IsNotNull(m.Test, "Test 应非 null 才能修改首项");
            m.Test![0] = "qqq!!!";
            m.T = typeof(ITest001);
            m.Pair = ("qqq", "1 3");
            ConfigHelper.SaveConfig(m, RWImpls.Json);
            ConfigHelper.SaveConfig(m, RWImpls.Xml);
            Assert.IsTrue(File.Exists(JsonConfigFilePath), "保存后 Json 文件应存在");
            Assert.IsTrue(File.Exists(XmlConfigFilePath), "保存后 Xml 文件应存在");

            // 启动缓存保护后修改引用 (缓存实例同一引用, 修改即生效于缓存)
            ConfigHelper.OnlyAllowAdd = true;
            m.ABC = "dddddd";
            Log("OnlyAllowAdd = true; m.ABC = \"dddddd\"; 尝试用 Json 保存;");

            // 保护状态下 SaveConfig 返回 false → ConfigHelper 抛 InvalidOperationException("保存配置信息失败")
            InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
                () => ConfigHelper.SaveConfig(m, RWImpls.Json),
                "缓存保护下 SaveConfig(Json) 应被拒绝并抛异常");
            Log($"缓存保护下保存被拒: {ex.Message}, 通过");

            // Json 重新获取: 命中缓存, 前面修改的引用就是缓存中的对象
            m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            Assert.AreEqual("dddddd", m.ABC, "保护下重新获取应为缓存值 \"dddddd\"");
            Log($"Json 重新获取 (缓存): ABC=\"{m.ABC}\", 通过");

            // 关闭缓存保护, reload:true 重新加载 → 取文件值 (保存被拒未写文件, 文件仍为旧值)
            ConfigHelper.OnlyAllowAdd = false;
            m = ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json, reload: true);
            Assert.AreEqual("99asdasdasd", m.ABC, "关闭保护后 reload 应取到 Json 文件中的旧值");
            Assert.AreEqual("qqq!!!", m.Test![0], "文件往返: Test[0] 应还原");
            Assert.AreEqual(typeof(ITest001), m.T, "文件往返: T 应还原 (FullName 匹配)");
            Assert.AreEqual("qqq", m.Pair.Group, "文件往返: Pair.Group 应还原");
            Assert.AreEqual("1 3", m.Pair.Id, "文件往返: Pair.Id 应还原");
            Log($"reload 后: ABC=\"{m.ABC}\", Test[0]=\"{m.Test![0]}\", T={m.T?.Name}, Pair=(\"{m.Pair.Group}\", \"{m.Pair.Id}\"), 通过");
        }

        /// <summary>配置读写实现的枚举名字 (与源测试一致, 作为 impl 名注册到 ConfigHelper)</summary>
        private enum RWImpls
        {
            Json,
            Xml
        }
    }

    /// <summary>
    /// 与 ChaoticKit.LibTest.Console.TestModels.TestModel002 同结构的本地模型
    /// (Console 工程的模型为 internal, Unit 工程未引用 Console, 故本地自带)
    /// </summary>
    internal sealed class TestModel002
    {
        /// <summary>显式公共无参构造器, 满足 TypeKeyedConfigReadWriteImplBase.IsAvailable 的 HavePublicEmptyCtor 检查</summary>
        public TestModel002() { }

        [DefaultValue("999", "123456")]
        public string ABC { get; set; } = string.Empty;

        [DefaultValue("999; 123456; 999; 12aqw")]
        public List<string>? Test { get; set; }

        public Type? T { get; set; }

        [DefaultValue("aaa:bbb")]
        public GroupIdPair Pair { get; set; }
    }

    /// <summary>
    /// 与 ChaoticKit.LibTest.Console.TestModels.ITest001 同结构的本地接口,
    /// 用作 T 属性的 Type 值 (经 ConfigStringHelper 以 FullName 字符串往返)
    /// </summary>
    internal interface ITest001
    {
        int M1(int x, int y);
        string M2(string x, string y);

        Task<int> M3(int x, int y, int z);

        Task M4(string str);
    }
}
