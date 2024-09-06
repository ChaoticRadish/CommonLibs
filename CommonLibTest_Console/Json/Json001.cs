using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Json
{
    internal class Json001() : TestBase("NewtonsoftJson, 测试字典类型的序列化")
    {
        protected override void RunImpl()
        {
            Dictionary<string, object?> dic = new()
            {
                { "a", "123321" },
                { "b", null },
                { "c", new string?[] { "123", "111", "222", null, "333" } },
            };

            var str = outputJson("dic", dic);
            WriteLine();

            WriteLine("转换为 TestClass");
            TestClass? obj = null;
            try
            {
                 obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(str);
            }
            catch (Exception ex)
            {
                WriteLine("转换异常! " + ex.Message);
            }
            if (obj is null)
            {
                WriteLine("obj is null");
            }
            else
            {
                outputJson("obj", obj);
                WriteLine();

                WriteLine("FullInfoString");
                WriteLine(obj.FullInfoString());

            }



        }

        public class TestClass 
        {
            public string a { get; set; } = string.Empty;
            public string? b { get; set; }

            public string[]? c { get; set; }

        }

        private string outputJson(string name, object obj)
        {
            string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            WriteLine(name + ": ");
            WriteLine(str);
            return str;
        }
    }
}
