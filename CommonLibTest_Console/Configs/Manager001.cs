using CommonLibTest_Console.TestModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtilConfig = Common_Util.Module.Config;

namespace CommonLibTest_Console.Configs
{
    internal class Manager001() : TestBase("测试 IConfigManager, 基础实现测试")
    {
        protected override void RunImpl()
        {
            init();

            
        }
        private void init()
        {
            WritePair(GetTestDir(), "清理测试目录");
            Directory.Delete(GetTestDir(), true);
            WriteLine("初始化实现配置");
            var jsonImpl = new UtilConfig.JsonConfigReadWriteImpl(Path.Combine(GetTestDir(), "Json"), false);
            UtilConfig.ConfigHelper.SetImpl(RWImpls.Json, jsonImpl);
            var xmlImpl = new UtilConfig.XmlConfigReadWriteImpl(Path.Combine(GetTestDir(), "Xml"), false);
            UtilConfig.ConfigHelper.SetImpl(RWImpls.Xml, xmlImpl);
        }

        protected override async Task RunImplAsync()
        {
            WriteLine("1. 初始取值, 使用 Json 实现获取默认值");
            var m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            WriteFullInfoPair(m);

            WriteLine("2. 简单校验");
#if DEBUG
            Assert.AreEqual("123456", m.ABC);
#else
            Assert.AreEqual("999", m.ABC);
#endif

            WriteLine("3. 修改实例值");
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

            WriteLine("4. 使用 Json / Xml 实现分别保存实例值");
            UtilConfig.ConfigHelper.SaveConfig(m, RWImpls.Json);
            UtilConfig.ConfigHelper.SaveConfig(m, RWImpls.Xml);
            await Task.Delay(1000);

            WriteLine("5. 使用 Json 重新获取实例值");
            m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            WriteFullInfoPair(m);
            WriteLine();
            WriteLine();

            WriteLine("6. 简单校验");
            Assert.AreEqual("99asdasdasd", m.ABC);

            WriteLine("7. 使用 Xml 重新获取实例值");
            m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Xml);
            WriteFullInfoPair(m);
            WriteLine();


            WriteLine("8. 修改实例值");
            m.ABC = "54856451asdas";
            WriteLine("m.ABC = \"54856451asdas\";");

            WriteLine("9. 使用 Xml 实现分别保存实例值");
            UtilConfig.ConfigHelper.SaveConfig(m, RWImpls.Xml);
            WriteLine();

            WriteLine("10. 使用 Xml 重新获取实例值");
            m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Xml);
            WriteFullInfoPair(m);
            WriteLine();

            WriteLine("11. 简单校验");
            Assert.AreEqual("54856451asdas", m.ABC);

            WriteLine("12. 启动缓存保护");
            UtilConfig.ConfigHelper.OnlyAllowAdd = true;

            WriteLine("13. 修改实例值");
            m.ABC = "dddddd";
            WriteLine("m.ABC = \"dddddd\";");

            WriteLine("14. 使用 Json 实现尝试保存");
            try
            {
                UtilConfig.ConfigHelper.SaveConfig(m, RWImpls.Json);
            }
            catch (Exception ex)
            {
                WritePair(ex);
            }
            WriteLine();

            WriteLine("15. 使用 Json 重新获取实例值, 当前值应为缓存值");
            m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json);
            WriteFullInfoPair(m);
            WriteLine();

            WriteLine("16. 简单校验");
            Assert.AreEqual("dddddd", m.ABC);

            WriteLine("17. 关闭缓存保护, 使用 Json 重新获取实例值, 传入重新加载参数, 当前值应为文件值");
            UtilConfig.ConfigHelper.OnlyAllowAdd = false;
            m = UtilConfig.ConfigHelper.GetConfig<TestModel002>(false, RWImpls.Json, reload: true);
            WriteFullInfoPair(m);
            WriteLine();

            WriteLine("18. 简单校验");
            Assert.AreEqual("99asdasdasd", m.ABC);
        }


        public enum RWImpls
        {
            Json,
            Xml
        }
    }
}
