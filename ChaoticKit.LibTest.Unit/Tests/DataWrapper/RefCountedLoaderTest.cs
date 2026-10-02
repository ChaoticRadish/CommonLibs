using ChaoticKit.Extensions;
using ChaoticKit.Interfaces.Behavior;
using ChaoticKit.Module.Loadable;

namespace ChaoticKit.LibTest.Unit.DataWrapper
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataWrapper.Loadable001
    /// RefCountedLoader&lt;TLoadable, TData&gt; 引用计数加卸载行为验证:
    /// 引用计数 0→1 触发 <see cref="ILoadable.Load"/>, 1→0 触发 <see cref="ILoadable.Unload"/>,
    /// 多次 Obtain 共享同一加载状态, 引用计数随引用释放逐级递减。
    /// </summary>
    /// <remarks>
    /// 全部使用固定数据, 无随机/计时依赖; 断言引用计数与 Load/Unload 前后的 Data 状态。
    /// 边界: 加载后 Data 为 null 时 Obtain 在写入引用数据阶段抛出异常, 且计数已递增不复位(库的实现行为)。
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class RefCountedLoaderTest : UnitTestBase
    {
        /// <summary>
        /// 可加载对象: Load 时将 Data 置为 Source 重复 5 次, Unload 时置为 null
        /// </summary>
        private sealed class TestLoadable : ILoadable
        {
            public TestLoadable(string source)
            {
                Source = source;
                Loader = new(this, loadable => loadable.Data!);
            }

            public string Source { get; }
            public string? Data { get; set; }
            public int LoadCount { get; private set; }
            public int UnloadCount { get; private set; }

            public RefCountedLoader<TestLoadable, string> Loader { get; }

            public void Load()
            {
                LoadCount++;
                Data = Source.Repeat(5);
            }

            public void Unload()
            {
                UnloadCount++;
                Data = null;
            }
        }

        /// <summary>
        /// 可加载对象: Load 时若 Source 为空则 Data 保持 null (用于验证 Data 为 null 的边界), 否则重复 5 次
        /// </summary>
        private sealed class TestLoadable2 : ILoadable
        {
            public TestLoadable2(string source)
            {
                Source = source;
                Loader = new(this, loadable => loadable.Data!);
            }

            public string Source { get; }
            public string? Data { get; set; }
            public int LoadCount { get; private set; }
            public int UnloadCount { get; private set; }

            public RefCountedLoader<TestLoadable2, string> Loader { get; }

            public void Load()
            {
                LoadCount++;
                Data = Source.IsEmpty() ? null : Source.Repeat(5);
            }

            public void Unload()
            {
                UnloadCount++;
                Data = null;
            }
        }

        /// <summary>
        /// 值类型 Data 的可加载对象: Load 时 Data = LoadValue, Unload 时回到默认值 0
        /// </summary>
        private sealed class TestLoadableInt : ILoadable
        {
            public TestLoadableInt(int loadValue)
            {
                LoadValue = loadValue;
                Loader = new(this, loadable => loadable.Data);
            }

            public int LoadValue { get; }
            public int Data { get; set; }
            public int LoadCount { get; private set; }
            public int UnloadCount { get; private set; }

            public RefCountedLoader<TestLoadableInt, int> Loader { get; }

            public void Load()
            {
                LoadCount++;
                Data = LoadValue;
            }

            public void Unload()
            {
                UnloadCount++;
                Data = 0;
            }
        }

        [DataTestMethod]
        [DataRow("123", "123123123123123")]
        [DataRow("qa", "qaqaqaqaqa")]
        [DataRow("ab", "ababababab")]
        public void Obtain_首次加载_Data为源串重复五次(string source, string expected)
        {
            Log($"--- 测试: Obtain 首次加载 (Source=\"{source}\") ---");
            var loadable = new TestLoadable(source);

            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "初始引用计数应为 0");
            Assert.AreEqual(0, loadable.LoadCount, "初始 Load 次数应为 0");

            using (var reference = loadable.Loader.Obtain())
            {
                Assert.AreEqual(1, loadable.Loader.ReferenceCount, "Obtain 后引用计数应为 1");
                Assert.AreEqual(1, loadable.LoadCount, "0→1 应触发一次 Load");
                Assert.AreEqual(0, loadable.UnloadCount, "加载期间不应触发 Unload");
                Assert.AreEqual(expected, reference.Data, $"引用数据应为 \"{expected}\", 实际: \"{reference.Data}\"");
                Assert.AreEqual(expected, loadable.Data, "加载对象 Data 状态应与引用数据一致");
                Log($"reference.Data = \"{reference.Data}\", ReferenceCount={loadable.Loader.ReferenceCount}, LoadCount={loadable.LoadCount}, 通过");
            }

            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "释放后引用计数应回到 0");
            Assert.AreEqual(1, loadable.UnloadCount, "1→0 应触发一次 Unload");
            Assert.IsNull(loadable.Data, "Unload 后加载对象 Data 应为 null");
            Log("释放引用后 Unload 已触发, Data=null, 通过");
        }

        [TestMethod]
        public void Obtain_多个引用_计数递增且仅首次触发Load()
        {
            Log("--- 测试: 多个引用共享加载状态 (对应原控制台 reference1~4) ---");
            var loadable = new TestLoadable("123");
            const string expected = "123123123123123";

            var reference1 = loadable.Loader.Obtain();
            Log("reference1: " + reference1.Data);
            Assert.AreEqual(1, loadable.Loader.ReferenceCount, "第 1 个引用后计数应为 1");
            Assert.AreEqual(1, loadable.LoadCount, "首次 Obtain 应触发 Load");
            Assert.AreEqual(expected, reference1.Data, "reference1.Data 不匹配");

            var reference2 = loadable.Loader.Obtain();
            Log("reference2: " + reference2.Data);
            Assert.AreEqual(2, loadable.Loader.ReferenceCount, "第 2 个引用后计数应为 2");
            Assert.AreEqual(1, loadable.LoadCount, "计数 1→2 不应再次 Load");
            Assert.AreSame(reference1.Data, reference2.Data, "同一加载状态下多个引用应共享同一 Data 实例");

            var reference3 = loadable.Loader.Obtain();
            Log("reference3: " + reference3.Data);
            Assert.AreEqual(3, loadable.Loader.ReferenceCount, "第 3 个引用后计数应为 3");
            Assert.AreEqual(1, loadable.LoadCount, "计数 2→3 不应再次 Load");

            var reference4 = loadable.Loader.Obtain();
            Log("reference4: " + reference4.Data);
            Assert.AreEqual(4, loadable.Loader.ReferenceCount, "第 4 个引用后计数应为 4");
            Assert.AreEqual(1, loadable.LoadCount, "计数 3→4 不应再次 Load");
            Assert.AreEqual(0, loadable.UnloadCount, "全部引用持有期间不应 Unload");

            // 按获得顺序的逆序释放 (与 using 声明离开作用域的顺序一致)
            reference4.Dispose();
            Assert.AreEqual(3, loadable.Loader.ReferenceCount, "释放第 4 个引用后计数应为 3");
            Assert.AreEqual(expected, loadable.Data, "仍有引用时 Data 应保持加载状态");

            reference3.Dispose();
            Assert.AreEqual(2, loadable.Loader.ReferenceCount, "释放第 3 个引用后计数应为 2");

            reference2.Dispose();
            Assert.AreEqual(1, loadable.Loader.ReferenceCount, "释放第 2 个引用后计数应为 1");

            reference1.Dispose();
            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "释放最后一个引用后计数应为 0");
            Assert.AreEqual(1, loadable.UnloadCount, "计数 1→0 应触发一次 Unload");
            Assert.IsNull(loadable.Data, "全部释放后 Data 应被 Unload 置为 null");
            Log("4 个引用全部释放, 计数 4→0, Unload 已触发, Data=null, 通过");
        }

        [TestMethod]
        public void Obtain_空数据_抛出异常且计数不复位()
        {
            Log("--- 测试: 加载后 Data 为 null 时 Obtain 抛异常 (对应原控制台 temp2 空 Source) ---");
            var loadable = new TestLoadable2("");

            InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
                () => loadable.Loader.Obtain(),
                "Data 为 null 时 Obtain 应抛 InvalidOperationException");

            StringAssert.Contains(ex.Message, "此类型的值不接受空值", "异常消息应说明不接受空值, 实际: " + ex.Message);
            Log($"Obtain 抛出 InvalidOperationException: \"{ex.Message}\", 通过");

            Assert.AreEqual(1, loadable.LoadCount, "Obtain 内部已先触发 Load (0→1)");
            Assert.AreEqual(1, loadable.Loader.ReferenceCount, "计数已递增且未复位 (引用未成功返回, 无 Dispose 递减)");
            Assert.IsNull(loadable.Data, "Load 后 Data 仍为 null");
            Log($"异常后 LoadCount={loadable.LoadCount}, ReferenceCount={loadable.Loader.ReferenceCount}, Data=null, 通过");
        }

        [TestMethod]
        public void TestLoadable2_非空源_正常加载与卸载()
        {
            Log("--- 测试: TestLoadable2 非空源正常加载卸载 (对应原控制台 temp3 Source=\"qa\") ---");
            var loadable = new TestLoadable2("qa");
            const string expected = "qaqaqaqaqa";

            using (var reference = loadable.Loader.Obtain())
            {
                Assert.AreEqual(1, loadable.Loader.ReferenceCount, "Obtain 后计数应为 1");
                Assert.AreEqual(1, loadable.LoadCount, "应触发一次 Load");
                Assert.AreEqual(expected, reference.Data, $"引用数据应为 \"{expected}\", 实际: \"{reference.Data}\"");
                Assert.AreEqual(expected, loadable.Data, "加载对象 Data 状态不匹配");
                Log($"reference.Data = \"{reference.Data}\", ReferenceCount={loadable.Loader.ReferenceCount}, 通过");
            }

            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "释放后计数应回到 0");
            Assert.AreEqual(1, loadable.UnloadCount, "应触发一次 Unload");
            Assert.IsNull(loadable.Data, "Unload 后 Data 应为 null");
            Log("释放后 Unload 已触发, Data=null, 通过");
        }

        [TestMethod]
        public void 值类型Data_默认值表示卸载状态_可重新加载()
        {
            Log("--- 测试: 值类型 Data 的引用计数循环 (TData=int) ---");
            var loadable = new TestLoadableInt(42);

            Assert.AreEqual(0, loadable.Data, "未加载时值类型 Data 为默认值 0");
            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "初始引用计数应为 0");

            using (var reference = loadable.Loader.Obtain())
            {
                Assert.AreEqual(1, loadable.Loader.ReferenceCount, "Obtain 后计数应为 1");
                Assert.AreEqual(1, loadable.LoadCount, "应触发一次 Load");
                Assert.AreEqual(42, reference.Data, "引用数据应为 42");
                Assert.AreEqual(42, loadable.Data, "加载对象 Data 应为 42");
                Log($"reference.Data = {reference.Data}, ReferenceCount={loadable.Loader.ReferenceCount}, 通过");
            }

            Assert.AreEqual(0, loadable.Loader.ReferenceCount, "释放后计数应为 0");
            Assert.AreEqual(1, loadable.UnloadCount, "应触发一次 Unload");
            Assert.AreEqual(0, loadable.Data, "Unload 后值类型 Data 回到默认值 0");
            Log("释放后 Unload 已触发, Data=0, 通过");

            using (var reference = loadable.Loader.Obtain())
            {
                Assert.AreEqual(2, loadable.LoadCount, "重新 Obtain 应再次触发 Load");
                Assert.AreEqual(42, reference.Data, "重新加载后 Data 应为 42");
                Log($"重新 Obtain 后 Data = {reference.Data}, LoadCount={loadable.LoadCount}, 通过");
            }
        }

        [TestMethod]
        public void 两个加载器实例互不影响()
        {
            Log("--- 测试: 两个独立加载器实例互不影响 ---");
            var loadableA = new TestLoadable("1");
            var loadableB = new TestLoadable("2");

            var referenceA = loadableA.Loader.Obtain();
            var referenceB = loadableB.Loader.Obtain();

            Assert.AreEqual(1, loadableA.Loader.ReferenceCount, "A 计数应为 1");
            Assert.AreEqual(1, loadableB.Loader.ReferenceCount, "B 计数应为 1");
            Assert.AreEqual("11111", loadableA.Data, "A.Data 应为 \"1\" 重复 5 次");
            Assert.AreEqual("22222", loadableB.Data, "B.Data 应为 \"2\" 重复 5 次");
            Assert.AreEqual(1, loadableA.LoadCount, "A 应 Load 一次");
            Assert.AreEqual(1, loadableB.LoadCount, "B 应 Load 一次");
            Log($"A.Data=\"{loadableA.Data}\", B.Data=\"{loadableB.Data}\", 各自计数=1, 通过");

            referenceA.Dispose();
            Assert.AreEqual(0, loadableA.Loader.ReferenceCount, "释放 A 后 A 计数应为 0");
            Assert.AreEqual(1, loadableB.Loader.ReferenceCount, "A 的释放不应影响 B 的计数");
            Assert.IsNull(loadableA.Data, "A 卸载后 Data 应为 null");
            Assert.AreEqual("22222", loadableB.Data, "B 仍应保持加载状态");
            Log("释放 A 后: A 已卸载, B 仍加载, 通过");

            referenceB.Dispose();
            Assert.AreEqual(0, loadableB.Loader.ReferenceCount, "释放 B 后 B 计数应为 0");
            Assert.IsNull(loadableB.Data, "B 卸载后 Data 应为 null");
            Log("释放 B 后: B 已卸载, 通过");
        }
    }
}
