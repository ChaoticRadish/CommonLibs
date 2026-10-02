using ChaoticKit.Extensions;
using ChaoticKit.Module.Config;

namespace ChaoticKit.LibTest.Unit.Configs
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Configs.XmlConfig001
    /// ConfigHelper + XmlConfigReadWriteImpl 配置保存/重载的行为验证
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// ConfigHelper 为静态全局状态, 本类 [DoNotParallelize] 避免与并行测试冲突。
    /// 模型 TestModel002/ITest001 复用 JsonConfigNewtonsoftTest.cs 中的定义 (同一命名空间)。
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public sealed class XmlConfigTest : UnitTestBase
    {
        [TestMethod]
        public void Xml配置_保存重载字段一致()
        {
            Log("--- 测试: Xml 配置 保存/重载 ---");
            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(dir);
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();

                var impl = new XmlConfigReadWriteImpl(dir, false);
                ConfigHelper.SetDefaultImpl(impl);
                impl.InitConfig<TestModel002>();

                var m = ConfigHelper.GetConfig<TestModel002>();
                Log($"初始配置: {m.FullInfoString()}");

                m.ABC = "99asdasdasd";
                if (m.Test != null) m.Test[0] = "qqq!!!";
                m.T = typeof(ITest001);
                m.Pair = ("qqq", "1 3");
                ConfigHelper.SaveConfig(m);

                // 清缓存后重载, 字段应一致
                ConfigHelper.ClearCache();
                var reloaded = ConfigHelper.GetConfig<TestModel002>();
                Assert.AreEqual("99asdasdasd", reloaded.ABC, "ABC 重载应一致");
                if (m.Test != null)
                {
                    Assert.IsNotNull(reloaded.Test, "Test 重载不应为 null");
                    Assert.AreEqual("qqq!!!", reloaded.Test![0], "Test[0] 重载应一致");
                }
                Assert.AreEqual(typeof(ITest001), reloaded.T, "T 重载应一致");
                Assert.AreEqual(m.Pair, reloaded.Pair, "Pair 重载应一致");
                Log($"重载后配置: {reloaded.FullInfoString()}, 通过");
            }
            finally
            {
                ConfigHelper.OnlyAllowAdd = false;
                ConfigHelper.ClearCache();
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }
}
