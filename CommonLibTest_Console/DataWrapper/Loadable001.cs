using Common_Util.Extensions;
using Common_Util.Interfaces.Behavior;
using Common_Util.Log;
using Common_Util.Module.Loadable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataWrapper
{
    internal class Loadable001() : TestBase("对 RefCountedLoader 的一些测试")
    {
        protected override void RunImpl()
        {
            var logger = this.GetLevelLogger("Body");
            TestLoadable temp = new(this.GetLevelLogger(nameof(TestLoadable)))
            {
                Source = "123",
            };

            using var reference1 = temp.Loader.Obtain();
            logger.Info("reference1: " + reference1.Data);
            using var reference2 = temp.Loader.Obtain();
            logger.Info("reference2: " + reference2.Data);
            using var reference3 = temp.Loader.Obtain();
            logger.Info("reference3: " + reference3.Data);
            using var reference4 = temp.Loader.Obtain();
            logger.Info("reference4: " + reference4.Data);

            try
            {
                TestLoadable2 temp2 = new(this.GetLevelLogger(nameof(TestLoadable2) + "_temp2"))
                {
                    Source = "",
                };

                using var reference5 = temp2.Loader.Obtain();
                logger.Info("reference5: " + reference5.Data);

                TestLoadable2 temp3 = new(this.GetLevelLogger(nameof(TestLoadable2) + "_temp3"))
                {
                    Source = "qa",
                };

                using var reference6 = temp3.Loader.Obtain();
                logger.Info("reference6: " + reference6.Data);
            }
            catch (Exception ex)
            {
                logger.Error("发生异常", ex);
            }

        }

        class TestLoadable : ILoadable
        {
            public TestLoadable(ILevelLogger logger)
            {
                this.logger = logger;
                Loader = new(this, (loadable) => loadable.Data!);
            }


            private readonly ILevelLogger logger;

            public required string Source { get; set; }
            public string? Data { get; set; }

            public RefCountedLoader<TestLoadable, string> Loader { get; } 

            public void Load()
            {
                logger.Info("加载资源");

                Data = Source.Repeat(5);

                logger.Info($"加载完成, 当前 Data => {Data ?? "<null>"}");
            }

            public void Unload()
            {
                logger.Info("卸载资源");

                Data = null;

                logger.Info($"卸载完成, 当前 Data => {Data ?? "<null>"}");
            }
        }


        class TestLoadable2 : ILoadable
        {
            public TestLoadable2(ILevelLogger logger)
            {
                this.logger = logger;
                Loader = new(this, (loadable) => loadable.Data!);
            }


            private readonly ILevelLogger logger;

            public required string Source { get; set; }
            public string? Data { get; set; }

            public RefCountedLoader<TestLoadable2, string> Loader { get; }

            public void Load()
            {
                logger.Info("加载资源");

                Data = Source.IsEmpty() ? null : Source.Repeat(5);

                logger.Info($"加载完成, 当前 Data => {Data ?? "<null>"}");
            }

            public void Unload()
            {
                logger.Info("卸载资源");

                Data = null;

                logger.Info($"卸载完成, 当前 Data => {Data ?? "<null>"}");
            }
        }
    }
}
