using ChaoticKit.Attributes.General;
using ChaoticKit.Data.Structure.Pair;
using ChaoticKit.Module.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChaoticKit.LibTest.Unit.Configs
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.Configs.JsonConfig001
    /// 验证 ConfigHelper + NewtonsoftJsonConfigReadWriteImpl (ChaoticKit.NewtonsoftJson) 的
    /// 配置初始化(默认值写入) / 加载 / 保存后重载字段值一致 的完整读写链路
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// - 使用临时目录 Path.GetTempPath()\ChaoticKitTest\{Guid} 保存配置文件, finally 递归删除, 不硬编码绝对路径;
    /// - 已移除原控制台测试中的 Task.Delay(1000), 无随机/计时参与断言, 全部固定数据;
    /// - 配置模型 TestModel002 与接口 ITest001 为本文件自带 (Console 工程内的为 internal, 且 Unit 工程未引用 Console);
    /// - DEBUG 构建下库会把配置文件写入 "&lt;类型名&gt;.debug.json" (NewtonsoftJsonConfigReadWriteImpl 的 #if DEBUG),
    ///   配置文件路径断言按 #if DEBUG 区分;
    /// - 库行为快照: CacheProtecting(OnlyAllowAdd=true) 时 ICachedConfigManager.Save 直接返回 false,
    ///   ConfigHelper.SaveConfig 因而抛出 InvalidOperationException; 因此保存/重载流程须在 OnlyAllowAdd=false 下进行
    ///   (原控制台测试在 OnlyAllowAdd=true 状态下调用 SaveConfig, 疑似误用或库缺陷, 待人工确认)。
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public sealed class JsonConfigNewtonsoftTest : UnitTestBase
    {
        /// <summary>
        /// 创建并返回一个独立的临时目录 (Path.GetTempPath()\ChaoticKitTest\{Guid})
        /// </summary>
        private static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ChaoticKitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// 计算当前构建配置下配置文件的路径 (DEBUG 下为 &lt;类型名&gt;.debug.json)
        /// </summary>
        private static string ConfigFilePath(string dir)
        {
            string fileName = typeof(TestModel002).Name + ".json";
#if DEBUG
            fileName = Path.ChangeExtension(fileName, "debug" + Path.GetExtension(fileName));
#endif
            return Path.Combine(dir, fileName);
        }

        /// <summary>
        /// 重置 ConfigHelper 的全局静态状态 (OnlyAllowAdd 关闭 + 清缓存), 保证用例间隔离
        /// </summary>
        private static void ResetConfigHelperState()
        {
            ConfigHelper.OnlyAllowAdd = false;
            ConfigHelper.ClearCache();
        }

        [TestMethod]
        public void InitConfig_生成默认值配置文件并读回默认值()
        {
            Log("--- 测试: 无配置文件时 LoadConfig 返回特性默认值; InitConfig 生成文件后可读回相同默认值 ---");
            string dir = CreateTempDir();
            try
            {
                ResetConfigHelperState();
                var impl = new NewtonsoftJsonConfigReadWriteImpl(dir, false);
                ConfigHelper.SetDefaultImpl(impl);

                // 1) 无配置文件时加载 → 全部取 DefaultValue 特性默认值
                var m1 = impl.LoadConfig<TestModel002>();
                string expectedAbc = new DefaultValueAttribute("999", "123456").ValueString!; // DEBUG=123456, Release=999, 与库取法一致
                Assert.AreEqual(expectedAbc, m1.ABC, "无文件时 ABC 应取 DefaultValue 特性默认值");
                Assert.IsNotNull(m1.Test, "无文件时 Test 应取 DefaultValue 特性默认值 (非 null)");
                CollectionAssert.AreEqual(new[] { "999", "123456", "999", "12aqw" }, m1.Test, "无文件时 Test 列表默认值不匹配");
                Assert.IsNull(m1.T, "T 无 DefaultValue 特性, 应为 null");
                Assert.AreEqual("aaa", m1.Pair.Group, "无文件时 Pair.Group 默认值不匹配");
                Assert.AreEqual("bbb", m1.Pair.Id, "无文件时 Pair.Id 默认值不匹配");
                Log($"无文件默认值: ABC={m1.ABC}, Test=[{string.Join(", ", m1.Test)}], T=<null>, Pair={m1.Pair}");

                // 2) InitConfig 生成默认值配置文件
                impl.InitConfig<TestModel002>();
                string configPath = ConfigFilePath(dir);
                Assert.IsTrue(File.Exists(configPath), $"InitConfig 后配置文件应已生成: {configPath}");
                Log($"配置文件已生成: {configPath}");

                // 3) 从文件读回 → 默认值应与无文件时一致
                var m2 = impl.LoadConfig<TestModel002>();
                Assert.AreEqual(expectedAbc, m2.ABC, "读文件后 ABC 应与默认值一致");
                Assert.IsNotNull(m2.Test);
                CollectionAssert.AreEqual(new[] { "999", "123456", "999", "12aqw" }, m2.Test, "读文件后 Test 应与默认值一致");
                Assert.IsNull(m2.T, "读文件后 T 应为 null");
                Assert.AreEqual("aaa", m2.Pair.Group, "读文件后 Pair.Group 应与默认值一致");
                Assert.AreEqual("bbb", m2.Pair.Id, "读文件后 Pair.Id 应与默认值一致");
                Log($"读回默认值: ABC={m2.ABC}, Test=[{string.Join(", ", m2.Test)}], T=<null>, Pair={m2.Pair}, 通过");
            }
            finally
            {
                ResetConfigHelperState();
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void 保存后重载_各字段值一致()
        {
            Log("--- 测试: 修改各字段 → SaveConfig → 清缓存重载, 断言字段值一致 ---");
            string dir = CreateTempDir();
            try
            {
                ResetConfigHelperState();
                var impl = new NewtonsoftJsonConfigReadWriteImpl(dir, false);
                ConfigHelper.SetDefaultImpl(impl);
                impl.InitConfig<TestModel002>();

                var m = ConfigHelper.GetConfig<TestModel002>();
                Assert.IsNotNull(m.Test, "加载后的 Test 列表不应为 null");
                m.ABC = "99asdasdasd";
                m.Test[0] = "qqq!!!";
                m.T = typeof(ITest001);
                m.Pair = ("qqq", "1 3");
                Log($"修改后: ABC={m.ABC}, Test=[{string.Join(", ", m.Test)}], T={m.T!.FullName}, Pair={m.Pair}");

                // 保存 (OnlyAllowAdd=false, 见类备注: 保护状态下 Save 会被库拒绝)
                ConfigHelper.SaveConfig(m);
                Log("SaveConfig 完成");

                // 清空缓存后重载, 验证从磁盘读回的值与保存时一致
                ConfigHelper.ClearCache();
                var reloaded = ConfigHelper.GetConfig<TestModel002>();
                Assert.AreNotSame(m, reloaded, "清缓存后重载应得到新的对象实例");

                Assert.AreEqual("99asdasdasd", reloaded.ABC, "ABC 保存后重载应一致");
                Assert.IsNotNull(reloaded.Test);
                CollectionAssert.AreEqual(new[] { "qqq!!!", "123456", "999", "12aqw" }, reloaded.Test, "Test 保存后重载应一致");
                Assert.AreEqual(typeof(ITest001), reloaded.T, "T 保存后重载应一致 (Type 以字符串存盘后还原)");
                Assert.AreEqual("qqq", reloaded.Pair.Group, "Pair.Group 保存后重载应一致");
                Assert.AreEqual("1 3", reloaded.Pair.Id, "Pair.Id 保存后重载应一致");
                Log($"重载后: ABC={reloaded.ABC}, Test=[{string.Join(", ", reloaded.Test)}], T={reloaded.T}, Pair={reloaded.Pair}, 通过");
            }
            finally
            {
                ResetConfigHelperState();
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void OnlyAllowAdd保护缓存时_SaveConfig抛出异常()
        {
            Log("--- 测试: OnlyAllowAdd=true (缓存保护) 时 SaveConfig 行为快照 ---");
            string dir = CreateTempDir();
            try
            {
                ResetConfigHelperState();
                var impl = new NewtonsoftJsonConfigReadWriteImpl(dir, false);
                ConfigHelper.SetDefaultImpl(impl);
                impl.InitConfig<TestModel002>();

                var m = ConfigHelper.GetConfig<TestModel002>();
                m.ABC = "保护状态下尝试保存";

                ConfigHelper.OnlyAllowAdd = true;
                // 库行为快照: CacheProtecting 时 ICachedConfigManager.Save 返回 false,
                // ConfigHelper.SaveConfig 抛出 InvalidOperationException("保存配置信息失败")。
                // 原控制台测试 JsonConfig001 正是在 OnlyAllowAdd=true 下调用 SaveConfig 的, 此快照锁定当前行为, 待人工确认。
                Assert.ThrowsException<InvalidOperationException>(() => ConfigHelper.SaveConfig(m),
                    "OnlyAllowAdd=true 时 SaveConfig 应抛出 InvalidOperationException");
                Log("OnlyAllowAdd=true 时 SaveConfig 抛出 InvalidOperationException, 与库当前行为一致 (快照, 待人工确认)");
            }
            finally
            {
                ResetConfigHelperState();
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }

    /// <summary>
    /// 测试用配置模型 (对应 Console 工程的 TestModel002, 本测试自带以保持自包含)。
    /// 显式声明 public 无参构造器, 满足库的 HavePublicEmptyCtor 判定与 Activator.CreateInstance 需求。
    /// </summary>
    internal class TestModel002
    {
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
    /// 测试用接口 (对应 Console 工程的 ITest001), 用于验证 Type 类型字段的存盘与还原
    /// </summary>
    public interface ITest001
    {
        int M1(int x, int y);
        string M2(string x, string y);
    }
}
