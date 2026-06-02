using ChaoticKit.Data.Structure.Map;
using ChaoticKit.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaoticKit.LibTest.Console.DataStruct
{
    internal class DefaultValueMap001() : TestBase("默认值Map基础功能测试")
    {
        private ILevelLogger _logger = null!;

        protected override void RunImpl()
        {
            _logger = GetLevelLogger("测试");

            RunTestMark();
        }

        [TestMethod("1. 基础索引器测试")]
        private void TestBasicIndexer()
        {
            _logger.Info("--- 测试: 基础索引器 ---");

            var map = new DefaultValueMap<string, int>(-1);

            map["a"] = 1;
            map["b"] = 2;

            if (map["a"] == 1 && map["b"] == 2)
            {
                _logger.Info("[基础索引器] 通过。map[\"a\"]=1, map[\"b\"]=2");
            }
            else
            {
                _logger.Error($"[基础索引器] 失败。map[\"a\"]={map["a"]}, map[\"b\"]={map["b"]}");
            }
        }

        [TestMethod("2. 默认值测试")]
        private void TestDefaultValue()
        {
            _logger.Info("--- 测试: 默认值 ---");

            var map = new DefaultValueMap<string, int>(999);

            var value = map["not_exists"];
            if (value == 999)
            {
                _logger.Info("[默认值] 通过。未设置的键返回默认值 999");
            }
            else
            {
                _logger.Error($"[默认值] 失败。预期 999, 实际 {value}");
            }

            if (map.Count == 0)
            {
                _logger.Info("[默认值] 通过。get 访问未增加 Count");
            }
            else
            {
                _logger.Error($"[默认值] 失败。Count={map.Count}");
            }
        }

        [TestMethod("3. 自动添加测试")]
        private void TestAutoAdd()
        {
            _logger.Info("--- 测试: 自动添加 ---");

            var map = new DefaultValueMap<string, int>(0);

            map["new_key"] = 100;

            if (map.ContainsKey("new_key") && map["new_key"] == 100)
            {
                _logger.Info("[自动添加] 通过。set 不存在的键自动添加");
            }
            else
            {
                _logger.Error($"[自动添加] 失败。ContainsKey={map.ContainsKey("new_key")}");
            }

            if (map.Count == 1)
            {
                _logger.Info($"[自动添加] 通过。Count={map.Count}");
            }
            else
            {
                _logger.Error($"[自动添加] 失败。预期 Count=1, 实际 Count={map.Count}");
            }
        }

        [TestMethod("4. IDictionary 操作测试")]
        private void TestIDictionaryOperations()
        {
            _logger.Info("--- 测试: IDictionary 操作 ---");

            var map = new DefaultValueMap<string, int>(0);

            map.Add("x", 1);
            map.Add("y", 2);

            if (map.Count == 2)
            {
                _logger.Info("[Add] 通过。添加了 x, y");
            }
            else
            {
                _logger.Error($"[Add] 失败。Count={map.Count}");
            }

            var keys = map.Keys.ToList();
            if (keys.Contains("x") && keys.Contains("y"))
            {
                _logger.Info($"[Keys] 通过。Keys=[{string.Join(", ", keys)}]");
            }
            else
            {
                _logger.Error($"[Keys] 失败。Keys=[{string.Join(", ", keys)}]");
            }

            var values = map.Values.ToList();
            if (values.Contains(1) && values.Contains(2))
            {
                _logger.Info($"[Values] 通过。Values=[{string.Join(", ", values)}]");
            }
            else
            {
                _logger.Error($"[Values] 失败。Values=[{string.Join(", ", values)}]");
            }

            if (map.TryGetValue("x", out var val) && val == 1)
            {
                _logger.Info($"[TryGetValue] 通过。TryGetValue(\"x\")={val}");
            }
            else
            {
                _logger.Error($"[TryGetValue] 失败。val={val}");
            }

            if (!map.TryGetValue("not_exists", out _))
            {
                _logger.Info("[TryGetValue] 通过。不存在的键返回 false");
            }
            else
            {
                _logger.Error("[TryGetValue] 失败。不存在的键应返回 false");
            }

            var removed = map.Remove("x");
            if (removed && !map.ContainsKey("x"))
            {
                _logger.Info("[Remove] 通过。移除了 x");
            }
            else
            {
                _logger.Error($"[Remove] 失败。removed={removed}");
            }

            map.Clear();
            if (map.Count == 0)
            {
                _logger.Info("[Clear] 通过。清空后 Count=0");
            }
            else
            {
                _logger.Error($"[Clear] 失败。Count={map.Count}");
            }
        }

        [TestMethod("5. 遍历测试")]
        private void TestEnumeration()
        {
            _logger.Info("--- 测试: 遍历 ---");

            var map = new DefaultValueMap<int, string>("默认");
            map[1] = "One";
            map[2] = "Two";
            map[3] = "Three";

            var items = map.ToList();
            if (items.Count == 3)
            {
                _logger.Info($"[遍历] 通过。共 {items.Count} 项");
                foreach (var kvp in items)
                {
                    _logger.Trace($"  [{kvp.Key}] = {kvp.Value}");
                }
            }
            else
            {
                _logger.Error($"[遍历] 失败。预期 3 项, 实际 {items.Count} 项");
            }
        }
    }
}
