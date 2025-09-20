using BenchmarkDotNet.Loggers;
using Common_Util.Data.Structure.Map;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Dict002() : TestBase("测试双向字典, 不同类型下的测试")
    {
        private ILevelLogger _logger => __logger ?? GetLevelLogger("测试");
        private ILevelLogger? __logger = null;

        protected override void RunImpl()
        {
            _logger.Info("开始运行双向字典类型测试");

            TestIntString();
            TestStringInt();
            TestCustomClassString();
            TestStringEnum();
            TestGuidString();
            TestBoolString();
            TestStringString();

            _logger.Info("所有类型测试完成");
        }

        private void TestIntString()
        {
            _logger.Info("测试类型: int -> string");
            var dict = new BidirectionalDictionary<int, string>();

            try
            {
                dict.Add(1, "One");
                dict.Add(2, "Two");

                _logger.Info($"通过键获取值: dict[1] = {dict[1]}");
                _logger.Info($"通过值获取键: dict(\"Two\") = {dict["Two"]}");

                dict.RemoveByKey(1);
                _logger.Info("删除键1成功");

                _logger.Info("int -> string 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"int -> string 测试失败: {ex.Message}");
            }
        }

        private void TestStringInt()
        {
            _logger.Info("测试类型: string -> int");
            var dict = new BidirectionalDictionary<string, int>();

            try
            {
                dict.Add("One", 1);
                dict.Add("Two", 2);

                _logger.Info($"通过键获取值: dict[\"One\"] = {dict["One"]}");
                _logger.Info($"通过值获取键: dict(2) = {dict[2]}");

                dict.RemoveByKey("One");
                _logger.Info("删除键\"One\"成功");

                _logger.Info("string -> int 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"string -> int 测试失败: {ex.Message}");
            }
        }

        private void TestCustomClassString()
        {
            _logger.Info("测试类型: 自定义类 -> string");
            var dict = new BidirectionalDictionary<Person, string>();

            try
            {
                var person1 = new Person { Id = 1, Name = "Alice" };
                var person2 = new Person { Id = 2, Name = "Bob" };

                dict.Add(person1, "Developer");
                dict.Add(person2, "Manager");

                _logger.Info($"通过键获取值: dict[person1] = {dict[person1]}");
                _logger.Info($"通过值获取键: dict(\"Manager\") = {dict["Manager"]?.Name}");

                dict.RemoveByKey(person1);
                _logger.Info("删除键person1成功");

                _logger.Info("自定义类 -> string 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"自定义类 -> string 测试失败: {ex.Message}");
            }
        }

        private void TestStringEnum()
        {
            _logger.Info("测试类型: string -> 枚举");
            var dict = new BidirectionalDictionary<string, DayOfWeek>();

            try
            {
                dict.Add("WorkDay", DayOfWeek.Monday);
                dict.Add("Weekend", DayOfWeek.Saturday);

                _logger.Info($"通过键获取值: dict[\"WorkDay\"] = {dict["WorkDay"]}");
                _logger.Info($"通过值获取键: dict(DayOfWeek.Saturday) = {dict[DayOfWeek.Saturday]}");

                dict.RemoveByKey("WorkDay");
                _logger.Info("删除键\"WorkDay\"成功");

                _logger.Info("string -> 枚举 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"string -> 枚举 测试失败: {ex.Message}");
            }
        }

        private void TestGuidString()
        {
            _logger.Info("测试类型: Guid -> string");
            var dict = new BidirectionalDictionary<Guid, string>();

            try
            {
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();

                dict.Add(guid1, "First");
                dict.Add(guid2, "Second");

                _logger.Info($"通过键获取值: dict[guid1] = {dict[guid1]}");
                _logger.Info($"通过值获取键: dict(\"Second\") = {dict["Second"]}");

                dict.RemoveByKey(guid1);
                _logger.Info("删除键guid1成功");

                _logger.Info("Guid -> string 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"Guid -> string 测试失败: {ex.Message}");
            }
        }

        private void TestBoolString()
        {
            _logger.Info("测试类型: bool -> string");
            var dict = new BidirectionalDictionary<bool, string>();

            try
            {
                dict.Add(true, "Yes");
                dict.Add(false, "No");

                _logger.Info($"通过键获取值: dict[true] = {dict[true]}");
                _logger.Info($"通过值获取键: dict(\"No\") = {dict["No"]}");

                dict.RemoveByKey(true);
                _logger.Info("删除键true成功");

                _logger.Info("bool -> string 测试通过");
            }
            catch (Exception ex)
            {
                _logger.Error($"bool -> string 测试失败: {ex.Message}");
            }
        }
        private void TestStringString()
        {
            _logger.Info("测试类型: string -> string");
            var dict = new BidirectionalDictionary<string, string>();

            // 添加键值对
            dict.Add("key1", "value1");
            dict.Add("key2", "value2");
            _logger.Info($"添加键值对: key1->value1, key2->value2");

            // 测试通过键获取值
            _logger.Info($"通过键获取值: dict.GetValue(\"key1\") = {dict.GetValue("key1")}");

            // 测试通过值获取键
            _logger.Info($"通过值获取键: dict.GetKey(\"value2\") = {dict.GetKey("value2")}");

            // 测试删除
            dict.RemoveByKey("key1");
            _logger.Info("删除键key1成功");

            // 验证删除后状态
            try
            {
                var value = dict.GetValue("key1");
                _logger.Error("错误：删除后仍能访问已删除的键");
            }
            catch (KeyNotFoundException)
            {
                _logger.Info("验证删除后：访问已删除的键正确抛出异常");
            }

            _logger.Info("string -> string 测试通过");
        }
    }

    // 自定义类用于测试
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            if (obj is Person other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

}
