using ChaoticKit.Data.Structure.Map;

namespace ChaoticKit.LibTest.Unit.DataStruct
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DataStruct.Dict002
    /// 双向字典 BidirectionalDictionary (ChaoticKit.Data) 在多种键值类型组合下的行为验证:
    /// 正反向映射 (键->值 / 值->键)、索引器、RemoveByKey 删除及删除后状态。
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class BidirectionalDictionaryTest : UnitTestBase
    {
        /// <summary>
        /// 自定义类, 用于测试引用类型作为键 (按 Id+Name 做值相等)
        /// </summary>
        private sealed class Person
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;

            public override bool Equals(object? obj) => obj is Person other && Id == other.Id && Name == other.Name;
            public override int GetHashCode() => HashCode.Combine(Id, Name);
            public override string ToString() => $"Person(Id={Id}, Name={Name})";
        }

        [TestMethod]
        public void IntString_AddReverseAndRemove()
        {
            Log("--- 测试类型: int -> string ---");
            var dict = new BidirectionalDictionary<int, string>();

            dict.Add(1, "One");
            dict.Add(2, "Two");

            Assert.AreEqual("One", dict[1], "通过键获取值失败。dict[1] 应为 \"One\"");
            Assert.AreEqual("One", dict.GetValue(1), "GetValue(1) 应返回 \"One\"");
            Assert.AreEqual(2, dict["Two"], "通过值获取键失败。dict[\"Two\"] 应为 2 (键 2 对应值 \"Two\")");
            Assert.AreEqual(2, dict.GetKey("Two"), "GetKey(\"Two\") 应返回 2");
            Assert.AreEqual(2, dict.Count, "Count 应为 2");
            Assert.IsTrue(dict.ContainsKey(2), "ContainsKey(2) 应为 true");
            Assert.IsTrue(dict.ContainsValue("Two"), "ContainsValue(\"Two\") 应为 true");
            Log("添加并正反向查询 1<->One, 2<->Two 通过");

            Assert.IsTrue(dict.RemoveByKey(1), "RemoveByKey(1) 应返回 true");
            Assert.IsFalse(dict.ContainsKey(1), "删除后 ContainsKey(1) 应为 false");
            Assert.IsFalse(dict.ContainsValue("One"), "删除后 ContainsValue(\"One\") 应为 false");
            Assert.AreEqual(1, dict.Count, "删除后 Count 应为 1");
            Assert.ThrowsException<KeyNotFoundException>(() => dict.GetValue(1), "删除后按键取值应抛 KeyNotFoundException");
            Assert.ThrowsException<KeyNotFoundException>(() => dict.GetKey("One"), "删除后按值取键应抛 KeyNotFoundException");
            Log("RemoveByKey(1) 后正反向均不可达, 通过");
        }

        [TestMethod]
        public void StringInt_AddReverseAndRemove()
        {
            Log("--- 测试类型: string -> int ---");
            var dict = new BidirectionalDictionary<string, int>();

            dict.Add("One", 1);
            dict.Add("Two", 2);

            Assert.AreEqual(1, dict["One"], "通过键获取值失败。dict[\"One\"] 应为 1");
            Assert.AreEqual(1, dict.GetValue("One"), "GetValue(\"One\") 应返回 1");
            Assert.AreEqual("Two", dict[2], "通过值获取键失败。dict[2] 应为 \"Two\" (键 2 对应值 \"Two\")");
            Assert.AreEqual("Two", dict.GetKey(2), "GetKey(2) 应返回 \"Two\"");
            Log("添加并正反向查询 \"One\"<->1, \"Two\"<->2 通过");

            Assert.IsTrue(dict.RemoveByKey("One"), "RemoveByKey(\"One\") 应返回 true");
            Assert.IsFalse(dict.ContainsKey("One"), "删除后 ContainsKey(\"One\") 应为 false");
            Assert.IsFalse(dict.ContainsValue(1), "删除后 ContainsValue(1) 应为 false");
            Assert.ThrowsException<KeyNotFoundException>(() => dict["One"], "删除后按键取值应抛 KeyNotFoundException");
            Assert.ThrowsException<KeyNotFoundException>(() => dict[1], "删除后按值取键应抛 KeyNotFoundException");
            Log("RemoveByKey(\"One\") 后正反向均不可达, 通过");
        }

        [TestMethod]
        public void CustomClassString_AddReverseAndRemove()
        {
            Log("--- 测试类型: 自定义类 Person -> string ---");
            var dict = new BidirectionalDictionary<Person, string>();

            var person1 = new Person { Id = 1, Name = "Alice" };
            var person2 = new Person { Id = 2, Name = "Bob" };

            dict.Add(person1, "Developer");
            dict.Add(person2, "Manager");

            Assert.AreEqual("Developer", dict[person1], "通过键获取值失败。dict[person1] 应为 \"Developer\"");
            Assert.AreEqual("Manager", dict.GetValue(person2), "GetValue(person2) 应返回 \"Manager\"");
            // 反方向: 值 -> 键, 返回的 Person 按 Id+Name 值相等
            Assert.AreEqual(person2, dict["Manager"], "通过值获取键失败。dict[\"Manager\"] 应等于 person2");
            Assert.AreEqual(person1, dict.GetKey("Developer"), "GetKey(\"Developer\") 应等于 person1");
            Assert.AreEqual(2, dict.Count, "Count 应为 2");
            Log("添加并正反向查询 person1<->Developer, person2<->Manager 通过");

            Assert.IsTrue(dict.RemoveByKey(person1), "RemoveByKey(person1) 应返回 true");
            Assert.IsFalse(dict.ContainsKey(person1), "删除后 ContainsKey(person1) 应为 false");
            Assert.IsFalse(dict.ContainsValue("Developer"), "删除后 ContainsValue(\"Developer\") 应为 false");
            Assert.AreEqual(person2, dict["Manager"], "删除 person1 后 person2<->Manager 应仍可用");
            Log("RemoveByKey(person1) 后 person1 记录不可达, 其余记录不受影响, 通过");
        }

        [TestMethod]
        public void StringEnum_AddReverseAndRemove()
        {
            Log("--- 测试类型: string -> 枚举 DayOfWeek ---");
            var dict = new BidirectionalDictionary<string, DayOfWeek>();

            dict.Add("WorkDay", DayOfWeek.Monday);
            dict.Add("Weekend", DayOfWeek.Saturday);

            Assert.AreEqual(DayOfWeek.Monday, dict["WorkDay"], "通过键获取值失败。dict[\"WorkDay\"] 应为 DayOfWeek.Monday");
            Assert.AreEqual(DayOfWeek.Monday, dict.GetValue("WorkDay"), "GetValue(\"WorkDay\") 应返回 DayOfWeek.Monday");
            Assert.AreEqual("Weekend", dict[DayOfWeek.Saturday], "通过值获取键失败。dict[DayOfWeek.Saturday] 应为 \"Weekend\"");
            Assert.AreEqual("Weekend", dict.GetKey(DayOfWeek.Saturday), "GetKey(DayOfWeek.Saturday) 应返回 \"Weekend\"");
            Log("添加并正反向查询 \"WorkDay\"<->Monday, \"Weekend\"<->Saturday 通过");

            Assert.IsTrue(dict.RemoveByKey("WorkDay"), "RemoveByKey(\"WorkDay\") 应返回 true");
            Assert.IsFalse(dict.ContainsKey("WorkDay"), "删除后 ContainsKey(\"WorkDay\") 应为 false");
            Assert.IsFalse(dict.ContainsValue(DayOfWeek.Monday), "删除后 ContainsValue(DayOfWeek.Monday) 应为 false");
            Assert.AreEqual(DayOfWeek.Saturday, dict["Weekend"], "删除后剩余记录按键取值应仍可用");
            Assert.AreEqual("Weekend", dict[DayOfWeek.Saturday], "删除后剩余记录按值取键应仍可用");
            Log("RemoveByKey(\"WorkDay\") 后 WorkDay 记录不可达, 通过");
        }

        [TestMethod]
        public void GuidString_AddReverseAndRemove()
        {
            Log("--- 测试类型: Guid -> string (固定 Guid, 确定性) ---");
            // Guid 仅作为键使用, 不参与断言比较, 用固定值保证确定性
            var guid1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var guid2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var dict = new BidirectionalDictionary<Guid, string>();

            dict.Add(guid1, "First");
            dict.Add(guid2, "Second");

            Assert.AreEqual("First", dict[guid1], "通过键获取值失败。dict[guid1] 应为 \"First\"");
            Assert.AreEqual("First", dict.GetValue(guid1), "GetValue(guid1) 应返回 \"First\"");
            Assert.AreEqual(guid2, dict["Second"], "通过值获取键失败。dict[\"Second\"] 应等于 guid2 (键 2 对应值 \"Second\")");
            Assert.AreEqual(guid2, dict.GetKey("Second"), "GetKey(\"Second\") 应等于 guid2");
            Log("添加并正反向查询 固定Guid<->First/Second 通过");

            Assert.IsTrue(dict.RemoveByKey(guid1), "RemoveByKey(guid1) 应返回 true");
            Assert.IsFalse(dict.ContainsKey(guid1), "删除后 ContainsKey(guid1) 应为 false");
            Assert.IsFalse(dict.ContainsValue("First"), "删除后 ContainsValue(\"First\") 应为 false");
            Assert.AreEqual("Second", dict.GetValue(guid2), "删除后剩余记录应仍可用");
            Log("RemoveByKey(guid1) 后 guid1 记录不可达, 通过");
        }

        [TestMethod]
        public void BoolString_AddReverseAndRemove()
        {
            Log("--- 测试类型: bool -> string ---");
            var dict = new BidirectionalDictionary<bool, string>();

            dict.Add(true, "Yes");
            dict.Add(false, "No");

            Assert.AreEqual("Yes", dict[true], "通过键获取值失败。dict[true] 应为 \"Yes\"");
            Assert.AreEqual("Yes", dict.GetValue(true), "GetValue(true) 应返回 \"Yes\"");
            Assert.AreEqual(false, dict["No"], "通过值获取键失败。dict[\"No\"] 应为 false");
            Assert.AreEqual(false, dict.GetKey("No"), "GetKey(\"No\") 应返回 false");
            Log("添加并正反向查询 true<->Yes, false<->No 通过");

            Assert.IsTrue(dict.RemoveByKey(true), "RemoveByKey(true) 应返回 true");
            Assert.IsFalse(dict.ContainsKey(true), "删除后 ContainsKey(true) 应为 false");
            Assert.IsFalse(dict.ContainsValue("Yes"), "删除后 ContainsValue(\"Yes\") 应为 false");
            Assert.AreEqual("No", dict[false], "删除后剩余记录应仍可用");
            Log("RemoveByKey(true) 后 true 记录不可达, 通过");
        }

        [TestMethod]
        public void StringString_AddRemoveAndDeletedKeyThrows()
        {
            Log("--- 测试类型: string -> string ---");
            var dict = new BidirectionalDictionary<string, string>();

            dict.Add("key1", "value1");
            dict.Add("key2", "value2");

            // 注意: string/string 时键值索引器参数类型相同会歧义, 一律用 GetValue/GetKey (与原控制台测试一致)
            Assert.AreEqual("value1", dict.GetValue("key1"), "GetValue(\"key1\") 应返回 \"value1\"");
            Assert.AreEqual("key2", dict.GetKey("value2"), "GetKey(\"value2\") 应返回 \"key2\"");
            Assert.AreEqual(2, dict.Count, "Count 应为 2");
            Log("添加 key1->value1, key2->value2 并正反向查询通过");

            Assert.IsTrue(dict.RemoveByKey("key1"), "RemoveByKey(\"key1\") 应返回 true");
            Assert.AreEqual(1, dict.Count, "删除后 Count 应为 1");

            // 删除后访问已删除键应抛异常 (对照控制台测试的删除后状态验证)
            Assert.ThrowsException<KeyNotFoundException>(() => dict.GetValue("key1"), "删除后 GetValue(\"key1\") 应抛 KeyNotFoundException");
            Assert.ThrowsException<KeyNotFoundException>(() => dict.GetKey("value1"), "删除后 GetKey(\"value1\") 应抛 KeyNotFoundException");
            Assert.IsFalse(dict.TryGetByKey("key1", out _), "删除后 TryGetByKey(\"key1\") 应返回 false");
            Log("删除 key1 后访问已删除键正确抛出 KeyNotFoundException, 通过");
        }

        [DataTestMethod]
        [DataRow("key1", "value1")]
        [DataRow("alpha", "beta")]
        [DataRow("中文键", "中文值")]
        public void StringString_DataDriven_RoundTrip(string key, string value)
        {
            Log($"--- 表驱动: string -> string 往返 [{key}] <-> [{value}] ---");
            var dict = new BidirectionalDictionary<string, string>();

            dict.Add(key, value);

            Assert.AreEqual(value, dict.GetValue(key), "正方向 GetValue 不匹配");
            Assert.AreEqual(key, dict.GetKey(value), "反方向 GetKey 不匹配");
            Assert.IsTrue(dict.ContainsKey(key), "ContainsKey 应为 true");
            Assert.IsTrue(dict.ContainsValue(value), "ContainsValue 应为 true");

            Assert.IsTrue(dict.TryGetByKey(key, out var roundTrip), "TryGetByKey 应返回 true");
            Assert.AreEqual(value, roundTrip, "TryGetByKey 取出的值不匹配");
            Assert.IsTrue(dict.TryGetByValue(value, out var roundTripKey), "TryGetByValue 应返回 true");
            Assert.AreEqual(key, roundTripKey, "TryGetByValue 取出的键不匹配");

            Assert.IsTrue(dict.RemoveByValue(value), "RemoveByValue 应返回 true");
            Assert.IsFalse(dict.ContainsKey(key), "RemoveByValue 后 ContainsKey 应为 false");
            Assert.IsFalse(dict.ContainsValue(value), "RemoveByValue 后 ContainsValue 应为 false");
            Log($"[{key}] <-> [{value}] 正反向往返一致, 删除后双向清除, 通过");
        }
    }
}
