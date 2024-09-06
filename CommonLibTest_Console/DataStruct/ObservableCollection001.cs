using Common_Util.Extensions;
using Common_Util.Extensions.ObjectModel;
using Common_Util.Random;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class ObservableCollection001() : TestBase("对 ObservableCollection<T> 自定义的一些扩展方法的测试")
    {
        protected override void RunImpl()
        {
            test(c => c.Swap(3, 6));
            test(c => c.Swap(6, 3));
            test(c => c.Swap(0, 6));
            test(c => c.Swap(6, 0));
            test(c => c.Swap(0, 1));
            test(c => c.Swap(1, 0));

            WriteLine("g1");
            groupTest(
                c => c.Swap(1, 0),
                c => c.Swap(2, 6),
                c => c.Swap(0, 5),
                c => c.Swap(2, 5));

            WriteLine("g2");
            test(c =>
            {
                c.Swap(1, 0);
                c.Swap(2, 6);
                c.Swap(0, 5);
                c.Swap(2, 5);
            });

            WriteLine("g3");
            test(c =>
            {
                c.Swap(1, 0);
                c.Swap(2, 6);
                c.Swap(0, 5);
                c.Swap(2, 5);
                c.Sort();
            });


            randomTest1(c => c.Sort());
            randomTest2(c => c.Sort(i => i.ValueA));
            randomTest2(c => c.Sort(i => i.ValueB));
        }

        private void write(ObservableCollection<string> test, string s = ", ")
        {
            WriteLine($"{Common_Util.String.StringHelper.Concat(test.Select((s, i) => $"[{i}]{s}").ToList(), s, false)}");
        }

        private void write(ObservableCollection<TestClass> test, string s = ", ")
        {
            WriteLine($"{Common_Util.String.StringHelper.Concat(test.Select((s, i) => $"[{i}]{s}").ToList(), s, false)}");
        }

        private void test(Action<ObservableCollection<string>> action, [CallerArgumentExpression(nameof(action))]string actionText = "")
        {
            var c = create();
            write(c);
            action(c);
            WriteLine(actionText);
            write(c);
            WriteEmptyLine();
        }

        public void groupTest(params Action<ObservableCollection<string>>[] actions)
        {
            var c = create();
            write(c);
            foreach (var action in actions)
            {
                action(c);
                write(c);
            }
            WriteEmptyLine();
        }

        public void randomTest1(Action<ObservableCollection<string>> action, [CallerArgumentExpression(nameof(action))] string actionText = "")
        {
            var c = random();
            write(c, "\n");
            action(c);
            WriteLine(actionText);
            write(c, "\n");
            WriteEmptyLine();
        }
        public void randomTest2(Action<ObservableCollection<TestClass>> action, [CallerArgumentExpression(nameof(action))] string actionText = "")
        {
            var c = randomTestObj();
            write(c, "\n");
            action(c);
            WriteLine(actionText);
            write(c, "\n");
            WriteEmptyLine();
        }

        private ObservableCollection<string> create() => ["a", "b", "c", "d", "e", "f", "g"];

        private ObservableCollection<string> random() => new(
            new List<string>()
            .Append(20, i => Common_Util.Random.RandomStringHelper.GetRandomLowerEnglishString(30, Random.Shared)));

        private ObservableCollection<TestClass> randomTestObj() => new(
            new List<TestClass>()
            .Append(20, i => new TestClass()
            {
                ValueA = Random.Shared.Next(100),
                ValueB = Common_Util.Random.RandomStringHelper.GetRandomLowerEnglishString(30, Random.Shared),
            }));


        public class TestClass
        {
            public int ValueA { get; set; }

            public string ValueB { get; set; } = string.Empty;

            public override string ToString()
            {
                return $"({ValueA}, {ValueB})";
            }
        }
    }
}
