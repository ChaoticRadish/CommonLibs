using Common_Util.Data.Struct;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Json
{
    internal class Json002() : TestBase("NewtonsoftJson, 测试struct的序列化和非序列化")
    {
        protected override void RunImpl()
        {
            TestModel testModel = new("111", "222", true);
            OperationResult<TestModel> test = testModel;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(test);
            WriteLine("序列化: ");
            WriteLine(json);
            WriteLine();

            object? obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(OperationResult<TestModel>));
            WriteLine("反序列化(json, Type)");
            WriteLine(obj?.FullInfoString() ?? "<NULL>");
            WriteLine();

            var obj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<TestModel>>(json);
            WriteLine("反序列化<T>(json)");
            WriteLine(obj2.FullInfoString() ?? "<NULL>");
            WriteLine();

        }

        public class TestModel(string a, string b, bool c)
        {
            public string A { get; set; } = a;

            public string B { get; set; } = b;

            public bool C { get; set; } = c;

        }
    }
}
