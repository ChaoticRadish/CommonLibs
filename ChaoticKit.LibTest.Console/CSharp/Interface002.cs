using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.CSharp
{
    internal class Interface002() : TestBase("接口默认实现的验证")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }
        [TestMethod]
        void Test01()
        {
            Impl01 impl01 = new(this);
            impl01.Write("WRITE 1");
            MyInterface myInterface = impl01;
            myInterface.DefaultWrite("WRITE 2");
        }

        /* 这两个测试均会输出重写后的内容 */
        [TestMethod]
        void Test02()
        {
            Impl02 impl02 = new(this);
            impl02.Write("WRITE 1");
            MyInterface myInterface = impl02;
            myInterface.DefaultWrite("WRITE 2");
        }
        [TestMethod]
        void Test03()
        {
            Impl02 impl02 = new(this);
            impl02.Write("WRITE 1");
            impl02.DefaultWrite("WRITE 2");
        }


        public interface MyInterface
        {
            void Write(string msg);

            public void DefaultWrite(string msg)
            {
                Write("默认实现: " + msg);
            }
        }

        public class Impl01(Interface002 parent) : MyInterface
        {
            private readonly Interface002 parent = parent;

            public void Write(string msg)
            {
                parent.WriteLine("Impl01: " + msg);
            }
        }
        public class Impl02(Interface002 parent) : MyInterface
        {
            private readonly Interface002 parent = parent;

            public void Write(string msg)
            {
                parent.WriteLine("Impl02: " + msg);
            }

            public void DefaultWrite(string msg)
            {
                parent.WriteLine("Impl02 - DefaultWrite: " + msg);
            }
        }
    }
}
