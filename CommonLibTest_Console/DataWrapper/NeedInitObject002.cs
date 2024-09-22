using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataWrapper
{
    internal class NeedInitObject002() : TestBase("测试 NeedInitObjectImmutable, INeedInitObject 的结构体实现")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod("测试01")]
        private void test1()
        {
            TestModel model = new();

            WritePair(key: "初始值", model.ToString(), " => \n");

            // model.Int01.Value = 100; // 编译不通过
            model.Int02.Value = 200;
            // model.Int03.Value = 300; // 编译不通过

            WritePair(key: "M1", model.ToString(), " => \n");
        }
        [TestMethod("测试02")]
        private void test2()
        {
            TestModel model = new();

            WritePair(key: "初始值", model.ToString(), " => \n");

            model.SetValue(101, 202, 303);


            WritePair(key: "M2", model.ToString(), " => \n");
        }


        class TestModel
        {

            public NeedInitObjectImmutable<int> Int01 { get; } = new();
            public NeedInitObject<int> Int02 { get; } = new();
            public NeedInitObjectImmutable<int> Int03 { get => int03; private set => int03 = value; }
            private NeedInitObjectImmutable<int> int03 = new();
            public void SetValue(int i1, int i2, int i3)
            {
                // Int01.Value = i1; //编译不通过
                Int02.Value = i2;
                int03.Value = i3;
            }

            public override string ToString()
            {
                return $"Int01:{Int01}\nInt02:{Int02}\nInt03:{Int03}";
            }
        }
    }
}
