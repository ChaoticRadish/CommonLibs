using Common_Util.Extensions;
using Common_Util.Module.Config;
using CommonLibTest_Console.TestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Configs
{
    internal class JsonConfig001() : TestBase("测试配置帮助类, 使用Newtonsoft.Json实现")
    {
        public override void Setup()
        {
            ConfigHelper.OnlyAllowAdd = false;
            ConfigHelper.ClearCache();
            Directory.Delete(GetTestDir(), true);
        }
        protected override void RunImpl()
        {
            var impl = new NewtonsoftJsonConfigReadWriteImpl(GetTestDir(), false);
            ConfigHelper.SetDefaultImpl(impl);
            impl.InitConfig<TestModel002>();
            ConfigHelper.OnlyAllowAdd = true;
            var m = impl.LoadConfig<TestModel002>();
            WriteLine(m.FullInfoString());
            WriteLine();
            WriteLine();
            WriteLine();

        }
        protected override async Task RunImplAsync()
        {
            var m = ConfigHelper.GetConfig<TestModel002>();
            WriteLine(m.FullInfoString());
            await Task.Delay(1000);
            WriteLine();
            WriteLine();
            WriteLine();
            m.ABC = "99asdasdasd";
            WriteLine("m.ABC = \"99asdasdasd\";");
            if (m.Test != null)
            {
                m.Test[0] = "qqq!!!";
                WriteLine("m.Test[0] = \"qqq!!!\";");
            }
            m.T = typeof(ITest001);
            WriteLine("m.T = typeof(ITest001);");
            m.Pair = ("qqq", "1 3");
            WriteLine("m.Pair = (\"qqq\", \"1 3\");");
            WriteLine();
            WriteLine();
            ConfigHelper.SaveConfig(m);
            await Task.Delay(1000);
            m = ConfigHelper.GetConfig<TestModel002>();
            WriteLine(m.FullInfoString());
            WriteLine();
            WriteLine();


        }
    }
}
