using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Ref001() : TestBase("ref 关键字的一些学习")
    {
        protected override void RunImpl()
        {
            TestA a = new TestA();
            WritePair(a.ToString());
            WritePair(a.Change1((ref TestModel m) => { m.ValueA = 20; m.ValueB = "Lambda"; }).ToString());
            WritePair(a.Change1(ChangeHandler).ToString());
            WritePair(a.Change2(50, "Change2").ToString());
            WritePair(a.Clear().ToString());

        }
        void ChangeHandler(ref TestModel model)
        {
            model.ValueA = 100;
            model.ValueB = "ChangeHandler";
        }

        public class TestA
        {
            public TestModel M1;

            public delegate void ChangeHandler(ref TestModel model);
            public TestA Change1(ChangeHandler hander)
            {
                hander(ref M1);
                return this;
            }
            public TestA Change2(int valueA, string valueB)
            {
                M1.ValueA = valueA;
                M1.ValueB = valueB;
                return this;
            }
            public TestA Clear()
            {
                M1.ValueA = default;
                M1.ValueB = default(string) ?? string.Empty;
                return this;
            }
            public override string ToString()
            {
                return $"{nameof(TestA)}-M1: {M1}";
            }
        }

        public struct TestModel
        {
            public int ValueA { get; set; }
            public string ValueB { get; set; }
            public readonly override string ToString()
            {
                return $"[{nameof(TestModel)}-{ValueA} -{ValueB}]";
            }
        }
    }
}
