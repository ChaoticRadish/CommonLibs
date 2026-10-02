using ChaoticKit.Attributes.General;
using ChaoticKit.Data.Structure.Pair;
using ChaoticKit.Module.Config;
using System.Text.Json;

namespace ChaoticKit.LibTest.Unit.Configs.SystemTextJson
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Configs.JsonConfig002
    /// ConfigHelper + JsonConfigReadWriteImpl (System.Text.Json 实现) 的
    /// 特性默认值 / InitConfig 落盘 / 修改后保存与重载 的验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// 说明:
    /// - 需要临时目录时用 Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid) 自建, 并在 finally 中删除, 不硬编码绝对路径。
    /// - 已移除源测试中的 Task.Delay, 无随机/计时参与断言。
    /// - DefaultValueAttribute(string, string) 的 #if DEBUG 是在 ChaoticKit 程序集编译时求值:
    ///   DEBUG 构建 ABC 默认值取 "123456", Release 取 "999"; 本测试用同样的 #if DEBUG 镜像该取值, 保证确定性。
    /// - JsonConfigReadWriteImpl 在 DEBUG 构建下实际读写的文件是 *.debug.json (见其 DEBUG 分支), 断言路径时做了同名镜像。
    /// - ConfigHelper 是共享静态状态, 每个用例开头重置 OnlyAllowAdd/ClearCache, 避免用例间相互影响。
    /// - 已知库行为: CacheProtecting=true (OnlyAllowAdd) 时 ConfigHelper.SaveConfig 会被拦截返回失败并抛异常,
    ///   因此保存/重载断言在保护关闭下进行; OnlyAllowAdd 本身以 "ClearCache 不生效" 方式单独验证。
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public sealed class JsonConfigSystemTextJsonTest : UnitTestBase
    {
        /// <summary>Test 属性的默认值拆分结果 (固定, 与源测试的 DefaultValue 一致)</summary>
        private static readonly string[] DefaultTestValues = { "999", "123456", "999", "12aqw" };

        /// <summary>修改 Test[0] 后的列表 (与源控制台测试一致)</summary>
        private static readonly string[] ModifiedTestValues = { "qqq!!!", "123456", "999", "12aqw" };

#if DEBUG
        /// <summary>DefaultValueAttribute("999", "123456") 在 DEBUG 构建下的默认值</summary>
        private const string AbcDefaultValue = "123456";
#else
        /// <summary>DefaultValueAttribute("999", "123456") 在 Release 构建下的默认值</summary>
        private const string AbcDefaultValue = "999";
#endif

        /// <summary>
        /// 取得 JsonConfigReadWriteImpl 实际写入的配置文件名
        /// (镜像实现内的 DEBUG 分支: DEBUG 下文件为 TestModel002.debug.json)
        /// </summary>
        private static string WrittenConfigPath(string dir)
        {
            string path = Path.Combine(dir, nameof(TestModel002) + ".json");
#if DEBUG
            return Path.ChangeExtension(path, "debug" + Path.GetExtension(path));
#else
            return path;
#endif
        }

        /// <summary>
        /// 测试: 无配置文件时, TryLoadConfig 应把各属性的 DefaultValue 特性应用为默认值
        /// </summary>
        [TestMethod]
        public void TryLoadConfig_无配置文件时应用特性默认值()
        {
            Log("--- 测试: TryLoadConfig 无配置文件时应用特性默认值 ---");

            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            try
            {
                var impl = new JsonConfigReadWriteImpl(dir, isFile: false);
                ConfigHelper.SetDefaultImpl(impl);
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();

                var m = impl.LoadConfig<TestModel002>();

                Assert.AreEqual(AbcDefaultValue, m.ABC, "ABC 应取 DefaultValue 特性的默认值");
                Assert.IsNotNull(m.Test, "Test 应为非 null 的默认列表");
                CollectionAssert.AreEqual(DefaultTestValues, m.Test, "Test 默认列表应等于特性值拆分结果");
                Assert.IsNull(m.T, "T 无默认值, 应为 null");
                Assert.AreEqual("aaa", m.Pair.Group, "Pair.Group 默认值应为 aaa");
                Assert.AreEqual("bbb", m.Pair.Id, "Pair.Id 默认值应为 bbb");

                Log($"默认值: ABC={m.ABC}, Test=[{string.Join(", ", m.Test!)}], T=null, Pair={m.Pair.Group}:{m.Pair.Id}, 通过");
            }
            finally
            {
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();
                TryDeleteDir(dir);
            }
        }

        /// <summary>
        /// 测试: InitConfig 应把默认值写入配置文件, 磁盘 JSON 内容与再次读取一致
        /// </summary>
        [TestMethod]
        public void InitConfig_默认值落盘_磁盘内容与读回一致()
        {
            Log("--- 测试: InitConfig 将默认值写入配置文件, 磁盘内容与读回一致 ---");

            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            try
            {
                var impl = new JsonConfigReadWriteImpl(dir, isFile: false);
                ConfigHelper.SetDefaultImpl(impl);
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();

                impl.InitConfig<TestModel002>();

                string filePath = WrittenConfigPath(dir);
                Log($"配置文件路径: {filePath}");
                Assert.IsTrue(File.Exists(filePath), "InitConfig 应生成配置文件");

                // 直接解析磁盘 JSON, 校验默认值已落盘
                using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(filePath)))
                {
                    JsonElement root = doc.RootElement;
                    Assert.AreEqual(AbcDefaultValue, root.GetProperty(nameof(TestModel002.ABC)).GetString(), "磁盘 JSON 中 ABC 应为默认值");
                    Assert.AreEqual("aaa:bbb", root.GetProperty(nameof(TestModel002.Pair)).GetString(), "磁盘 JSON 中 Pair 应为默认值字符串");

                    JsonElement testElem = root.GetProperty(nameof(TestModel002.Test));
                    Assert.AreEqual(JsonValueKind.Array, testElem.ValueKind, "磁盘 JSON 中 Test 应为数组");
                    string[] testArr = testElem.EnumerateArray().Select(e => e.GetString()!).ToArray();
                    CollectionAssert.AreEqual(DefaultTestValues, testArr, "磁盘 JSON 中 Test 数组内容应与默认值一致");

                    Assert.AreEqual(JsonValueKind.Null, root.GetProperty(nameof(TestModel002.T)).ValueKind, "磁盘 JSON 中 T 应为 null");
                }

                // 用全新实现从磁盘读回, 验证默认值可完整还原
                var fresh = new JsonConfigReadWriteImpl(dir, isFile: false);
                var back = fresh.LoadConfig<TestModel002>();
                Assert.AreEqual(AbcDefaultValue, back.ABC, "读回 ABC 应与默认值一致");
                Assert.IsNotNull(back.Test, "读回 Test 不应为 null");
                CollectionAssert.AreEqual(DefaultTestValues, back.Test, "读回 Test 应与默认值一致");
                Assert.IsNull(back.T, "读回 T 应为 null");
                Assert.AreEqual("aaa", back.Pair.Group, "读回 Pair.Group 应一致");
                Assert.AreEqual("bbb", back.Pair.Id, "读回 Pair.Id 应一致");

                Log("磁盘 JSON 内容与全新实现读回均保持默认值, 通过");
            }
            finally
            {
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();
                TryDeleteDir(dir);
            }
        }

        /// <summary>
        /// 测试: 修改配置对象后 SaveConfig 保存, 经 ConfigHelper 缓存重载与磁盘全新实现重载均保持修改值;
        /// 同时验证 OnlyAllowAdd=true 时缓存对象受保护 (ClearCache 不生效)
        /// </summary>
        [TestMethod]
        public void 修改后保存_缓存与磁盘重载均保持修改值()
        {
            Log("--- 测试: 修改配置后 SaveConfig 保存, 缓存与磁盘重载均保持修改值 ---");

            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            try
            {
                var impl = new JsonConfigReadWriteImpl(dir, isFile: false);
                ConfigHelper.SetDefaultImpl(impl);
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();

                impl.InitConfig<TestModel002>();
                var m = impl.LoadConfig<TestModel002>();
                Assert.AreEqual(AbcDefaultValue, m.ABC, "初始化后应读到默认值");
                Assert.IsNotNull(m.Test, "初始化后 Test 不应为 null");
                Assert.AreEqual(DefaultTestValues[0], m.Test[0], "初始化后 Test[0] 应为默认首项");

                // 按源控制台测试修改字段
                m.ABC = "99asdasdasd";
                m.Test[0] = "qqq!!!";
                m.T = typeof(ITest001);
                m.Pair = ("qqq", "1 3");
                Log($"修改后: ABC={m.ABC}, Test[0]={m.Test[0]}, T={m.T.FullName}, Pair={m.Pair.ConvertToString()}");

                ConfigHelper.SaveConfig(m);
                Log("ConfigHelper.SaveConfig 完成");

                // 经 ConfigHelper 重载 (缓存未加载过, 从磁盘载入缓存)
                var reloaded = ConfigHelper.GetConfig<TestModel002>();
                AssertModifiedValues(reloaded, "ConfigHelper 缓存重载");

                // 用全新实现从磁盘重载, 验证修改值真正落盘
                var fresh = new JsonConfigReadWriteImpl(dir, isFile: false);
                var fromDisk = fresh.LoadConfig<TestModel002>();
                AssertModifiedValues(fromDisk, "磁盘全新实现重载");

                // OnlyAllowAdd=true 时缓存对象受保护, ClearCache 不应清除已缓存对象 (源测试设置的标志位行为)
                ConfigHelper.OnlyAllowAdd = true;
                ConfigHelper.ClearCache();
                Assert.IsTrue(ConfigHelper.IsLoaded<TestModel002>(), "OnlyAllowAdd=true 时 ClearCache 不应清除已缓存对象");
                ConfigHelper.OnlyAllowAdd = false;
                Log("OnlyAllowAdd 保护标志位: ClearCache 被拦截, 缓存保留, 通过");

                Log("保存/重载/缓存保护检查全部通过");
            }
            finally
            {
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();
                TryDeleteDir(dir);
            }
        }

        /// <summary>断言修改后的四个字段在指定阶段的重载对象上都保持修改值</summary>
        private void AssertModifiedValues(TestModel002 m, string stage)
        {
            Assert.AreEqual("99asdasdasd", m.ABC, $"{stage}: ABC 应保持修改值");
            Assert.IsNotNull(m.Test, $"{stage}: Test 不应为 null");
            CollectionAssert.AreEqual(ModifiedTestValues, m.Test, $"{stage}: Test 列表应保持修改值");
            Assert.AreEqual(typeof(ITest001), m.T, $"{stage}: T 应保持 typeof(ITest001)");
            Assert.AreEqual("qqq", m.Pair.Group, $"{stage}: Pair.Group 应保持修改值");
            Assert.AreEqual("1 3", m.Pair.Id, $"{stage}: Pair.Id 应保持修改值 (含空格)");
            Log($"{stage}: ABC={m.ABC}, Test=[{string.Join(", ", m.Test!)}], T={m.T!.FullName}, Pair={m.Pair.Group}:{m.Pair.Id}");
        }

        /// <summary>尽力删除测试临时目录, 清理失败不掩盖测试结果</summary>
        private static void TryDeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, recursive: true); } catch { /* 尽力清理 */ }
            }
        }
    }

    /// <summary>
    /// 源测试 TestModel002 在本工程内的镜像 (用于 T 属性保存/重载的类型标记)
    /// </summary>
    public interface ITest001
    {
        int M1(int x, int y);
    }

    /// <summary>
    /// 源测试 TestModel002 在本工程内的镜像, 特性默认值与原模型一致
    /// </summary>
    public sealed class TestModel002
    {
        [DefaultValue("999", "123456")]
        public string ABC { get; set; } = string.Empty;

        [DefaultValue("999; 123456; 999; 12aqw")]
        public List<string>? Test { get; set; }

        public Type? T { get; set; }

        [DefaultValue("aaa:bbb")]
        public GroupIdPair Pair { get; set; }
    }
}
