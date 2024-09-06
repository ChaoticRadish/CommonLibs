using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Struct001() : TestBase("测试结构体方法修改成员值")
    {
        protected override void RunImpl()
        {
            WriteLine("测试第一部分: ");
            TestStruct t1 = new TestStruct(3, 6);
            WriteLine("编辑前");
            WriteFull(t1);
            t1.Set(8);
            WriteLine("编辑后 t1.Set(8); ");
            WriteFull(t1);

            WriteEmptyLine(3);
            WriteLine("测试第二部分: ");
            TestStruct t2 = get();
            WriteLine("编辑前");
            WriteFull(t2);
            t2.Set(8);
            WriteLine("编辑后 t2.Set(8); ");
            WriteFull(t2);

            WriteEmptyLine(3);
            WriteLine("测试第三部分: ");
            TestStruct t3 = new TestStruct(3, 6);
            test(t3);
            WriteLine("测试后 test(t3); ");
            WriteFull(t3);

        }


        TestStruct get()
        {
            return new(7, 1);
        }

        void test(TestStruct test)
        {
            WriteLine("(test方法)编辑前");
            WriteFull(test);
            test.Set(8);
            WriteLine("(test方法)编辑后 test.Set(8); ");
            WriteFull(test);


        }

        struct TestStruct(int x, int y)
        {

            public int X { get; set; } = x;

            public int Y { get; set; } = y;


            public void Set(int test)
            {
                X = test + 1;
                Y = test + 2;
            }
        }
    }
}
