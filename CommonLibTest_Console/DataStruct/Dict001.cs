using Common_Util.Data.Structure.Map;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Dict001() : TestBase("测试双向字典, 基础功能测试与多线程测试")
    {
        private ILevelLogger _logger => __logger ?? GetLevelLogger("测试");
        private ILevelLogger? __logger = null;
        protected override void RunImpl()
        {
            _logger.Info("Starting BidirectionalDictionary tests");

            try
            {
                TestBasicOperations();
                TestConcurrentOperations();
                TestErrorHandling();
                TestPerformance();
                TestEnumeration();

                _logger.Info("All tests completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Test suite failed: {ex.Message}");
                throw;
            }
        }

        private void TestBasicOperations()
        {
            _logger.Info("Running basic operations test");

            var dict = new BidirectionalDictionary<int, string>() { AddIfNoExists = true };

            // Add operations
            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict[3] = "Three";

            // Retrieve operations
            var value = dict[1];
            var key = dict["Two"];

            // Contains operations
            var hasKey = dict.ContainsKey(2);
            var hasValue = dict.ContainsValue("Three");

            // Remove operations
            dict.RemoveByKey(1);
            dict.RemoveByValue("Two");

            _logger.Info("Basic operations test completed");
        }

        private void TestConcurrentOperations()
        {
            _logger.Info("Running concurrent operations test");

            var dict = new BidirectionalDictionary<int, string>()
            {
                AddIfNoExists = true
            };
            const int itemCount = 10000;

            Parallel.For(0, itemCount, i =>
            {
                if (i % 2 == 0)
                {
                    dict.Add(i, $"Value_{i}");
                }
                else
                {
                    dict[i] = $"Value_{i}";
                }
            });

            Parallel.For(0, itemCount, i =>
            {
                if (dict.TryGetByKey(i, out var value))
                {
                    if (dict.TryGetByValue(value, out var key))
                    {
                        if (key != i)
                        {
                            _logger.Error($"Key mismatch: expected {i}, got {key}");
                        }
                    }
                }
            });

            _logger.Info($"Concurrent operations test completed with {dict.Count} items");
        }

        private void TestErrorHandling()
        {
            _logger.Info("Running error handling test");

            var dict = new BidirectionalDictionary<int, string>();

            try
            {
                dict.Add(1, "One");
                dict.Add(1, "Duplicate"); // Should fail
                _logger.Error("Duplicate key test failed");
            }
            catch (Exception ex)
            {
                _logger.Info($"Duplicate key correctly handled: {ex.Message}");
            }

            try
            {
                dict.Add(2, "One"); // Should fail
                _logger.Error("Duplicate value test failed");
            }
            catch (Exception ex)
            {
                _logger.Info($"Duplicate value correctly handled: {ex.Message}");
            }

            try
            {
                var value = dict[99]; // Should fail
                _logger.Error("Missing key test failed");
            }
            catch (Exception ex)
            {
                _logger.Info($"Missing key correctly handled: {ex.Message}");
            }

            _logger.Info("Error handling test completed");
        }

        private void TestPerformance()
        {
            _logger.Info("Running performance test");

            var dict = new BidirectionalDictionary<int, string>();
            const int itemCount = 100000;

            var start = DateTime.Now;

            // Add items
            for (int i = 0; i < itemCount; i++)
            {
                dict.Add(i, $"Value_{i}");
            }

            var addTime = DateTime.Now - start;
            _logger.Info($"Added {itemCount} items in {addTime.TotalMilliseconds}ms");

            start = DateTime.Now;

            // Read items
            for (int i = 0; i < itemCount; i++)
            {
                var value = dict[i];
                var key = dict[value];
            }

            var readTime = DateTime.Now - start;
            _logger.Info($"Read {itemCount} items in {readTime.TotalMilliseconds}ms");

            start = DateTime.Now;

            // Remove items
            for (int i = 0; i < itemCount; i++)
            {
                dict.RemoveByKey(i);
            }

            var removeTime = DateTime.Now - start;
            _logger.Info($"Removed {itemCount} items in {removeTime.TotalMilliseconds}ms");

            _logger.Info("Performance test completed");
        }

        private void TestEnumeration()
        {
            _logger.Info("Running enumeration test");

            var dict = new BidirectionalDictionary<int, string>();

            // Add items
            for (int i = 0; i < 100; i++)
            {
                dict.Add(i, $"Value_{i}");
            }

            // Enumerate while modifying
            var enumerated = 0;
            Parallel.Invoke(
                () =>
                {
                    foreach (var kvp in dict)
                    {
                        Interlocked.Increment(ref enumerated);
                        Thread.Sleep(1);
                    }
                },
                () =>
                {
                    for (int i = 100; i < 200; i++)
                    {
                        dict.Add(i, $"Value_{i}");
                    }
                });

            _logger.Info($"Enumerated {enumerated} items while dictionary was modified");
            _logger.Info("Enumeration test completed");
        }
    }
}
