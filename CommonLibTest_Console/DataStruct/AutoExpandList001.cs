using Common_Util.Data.Structure.Linear;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibTest_Console.DataStruct
{
    internal class AutoExpandList001() : TestBase("自动扩展列表基础功能测试")
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

            var list = new AutoExpandList<int>(-1);

            list[0] = 10;
            list[2] = 30;

            if (list[0] == 10 && list[2] == 30)
            {
                _logger.Info("[基础索引器] 通过。list[0]=10, list[2]=30");
            }
            else
            {
                _logger.Error($"[基础索引器] 失败。list[0]={list[0]}, list[2]={list[2]}");
            }
        }

        [TestMethod("2. 默认值测试")]
        private void TestDefaultValue()
        {
            _logger.Info("--- 测试: 默认值 ---");

            var list = new AutoExpandList<string>("DEFAULT");

            var value = list[100];
            if (value == "DEFAULT")
            {
                _logger.Info("[默认值] 通过。未设置的索引返回默认值 'DEFAULT'");
            }
            else
            {
                _logger.Error($"[默认值] 失败。预期 'DEFAULT', 实际 '{value}'");
            }
        }

        [TestMethod("3. 自动扩展测试")]
        private void TestAutoExpand()
        {
            _logger.Info("--- 测试: 自动扩展 ---");

            var list = new AutoExpandList<int>(0);

            list[5] = 50;

            if (list.Count == 6)
            {
                _logger.Info($"[自动扩展] 通过。Count={list.Count}");
            }
            else
            {
                _logger.Error($"[自动扩展] 失败。预期 Count=6, 实际 Count={list.Count}");
            }

            var setIndexes = list.SetIndexes.ToList();
            if (setIndexes.Count == 1 && setIndexes[0] == 5)
            {
                _logger.Info($"[已设置索引] 通过。SetIndexes=[{string.Join(", ", setIndexes)}]");
            }
            else
            {
                _logger.Error($"[已设置索引] 失败。SetIndexes=[{string.Join(", ", setIndexes)}]");
            }
        }

        [TestMethod("4. ContainsIndex 测试")]
        private void TestContainsIndex()
        {
            _logger.Info("--- 测试: ContainsIndex ---");

            var list = new AutoExpandList<int>(-1);
            list[0] = 100;
            list[5] = 500;

            var tests = new[]
            {
                (index: 0, expected: true),
                (index: 5, expected: true),
                (index: 3, expected: false),
                (index: -1, expected: false),
            };

            foreach (var (index, expected) in tests)
            {
                var actual = list.ContainsIndex(index);
                if (actual == expected)
                {
                    _logger.Info($"[ContainsIndex({index})] 通过。结果={actual}");
                }
                else
                {
                    _logger.Error($"[ContainsIndex({index})] 失败。预期={expected}, 实际={actual}");
                }
            }
        }

        [TestMethod("5. IList<T> 操作测试")]
        private void TestIListOperations()
        {
            _logger.Info("--- 测试: IList<T> 操作 ---");

            var list = new AutoExpandList<int>(0);

            list.Add(1);
            _logger.Info($"[Add] 列表内容: [{string.Join(", ", list)}]");

            list.Add(2);
            _logger.Info($"[Add] 列表内容: [{string.Join(", ", list)}]");

            list.Add(3);
            _logger.Info($"[Add] 列表内容: [{string.Join(", ", list)}]");

            if (list.Count == 3 && list[0] == 1 && list[1] == 2 && list[2] == 3)
            {
                _logger.Info("[Add] 通过。添加了 1, 2, 3");
            }
            else
            {
                _logger.Error($"[Add] 失败。Count={list.Count}");
            }

            list.Insert(1, 10);
            _logger.Info($"[Insert(1, 10)] 列表内容: [{string.Join(", ", list)}]");

            if (list[1] == 10 && list[2] == 2)
            {
                _logger.Info("[Insert] 通过。在索引1处插入10");
            }
            else
            {
                _logger.Error($"[Insert] 失败。list[1]={list[1]}, list[2]={list[2]}");
            }

            list.RemoveAt(0);
            _logger.Info($"[RemoveAt(0)] 列表内容: [{string.Join(", ", list)}]");

            if (list[0] == 10 && list.Count == 3)
            {
                _logger.Info("[RemoveAt] 通过。移除索引0");
            }
            else
            {
                _logger.Error($"[RemoveAt] 失败。list[0]={list[0]}, Count={list.Count}");
            }

            var index = list.IndexOf(2);
            _logger.Info($"[IndexOf(2)] 结果: {index}, 列表内容: [{string.Join(", ", list)}]");

            if (index == 1)
            {
                _logger.Info($"[IndexOf] 通过。IndexOf(2)={index}");
            }
            else
            {
                _logger.Error($"[IndexOf] 失败。预期=1, 实际={index}");
            }

            var contains = list.Contains(10);
            _logger.Info($"[Contains(10)] 结果: {contains}, 列表内容: [{string.Join(", ", list)}]");

            if (contains)
            {
                _logger.Info("[Contains] 通过。包含元素10");
            }
            else
            {
                _logger.Error("[Contains] 失败。应包含元素10");
            }

            list.Clear();
            _logger.Info($"[Clear] 列表内容: [{string.Join(", ", list)}]");

            if (list.Count == 0)
            {
                _logger.Info("[Clear] 通过。清空后Count=0");
            }
            else
            {
                _logger.Error($"[Clear] 失败。Count={list.Count}");
            }
        }

        [TestMethod("6. 遍历测试")]
        private void TestEnumeration()
        {
            _logger.Info("--- 测试: 遍历 ---");

            var list = new AutoExpandList<string>("空");
            list[0] = "A";
            list[2] = "C";

            var items = list.ToList();
            var expected = new[] { "A", "空", "C" };

            if (items.SequenceEqual(expected))
            {
                _logger.Info($"[遍历] 通过。结果=[{string.Join(", ", items)}]");
            }
            else
            {
                _logger.Error($"[遍历] 失败。预期=[{string.Join(", ", expected)}], 实际=[{string.Join(", ", items)}]");
            }
        }
    }
}
