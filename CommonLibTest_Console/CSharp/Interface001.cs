using Common_Util.Attributes.General;
using Common_Util.Extensions;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Interface001() : TestBase("测试接口声明的静态虚方法和静态抽象方法")
    {
        protected override void RunImpl()
        {
            LogBuilderConfig.HeadSameLine = true;
            ILoggerOutEmptyLine = false;
            GlobalLoggerManager.CurrentLogger = this;

            RunTestMark();
        }

        [TestMethod]
        void printITestAllThings()
        {
            printAllThings(typeof(ITest<>));
            WriteEmptyLine();
            printAllThings(typeof(ImplA));
            WriteEmptyLine();
            printAllThings(typeof(ImplB));
            WriteEmptyLine();
            printAllThings(typeof(ImplDuck));
        }

        void printAllThings(Type type)
        {
            Logger.Test.Info(type.ToString());
            Logger.Test.Info("type.GetProperties()");
            foreach (var (index, property) in type.GetProperties().WithIndex())
            {
                Logger.Test.Info($"{index}. {property} ({property.GetMethod.GetIsNullString("Getter")} {property.SetMethod.GetIsNullString("Setter")})");
            }
            Logger.Test.Info("type.GetMethods()");
            StringBuilder sb = new();
            static string paramStr(ParameterInfo p)
            {
                string output = string.Empty;
                if (p.GetCustomAttribute<OutAttribute>() != null)
                {
                    output += "[out] ";
                }
                else if (p.ParameterType.IsByRef)
                {
                    output += "[ref] ";
                }
                output += $"{p.ParameterType.Name} {p.Name}";
                if (p.HasDefaultValue)
                {
                    if (p.ParameterType == typeof(string))
                    {
                        string? defaultValue = (string?)p.DefaultValue;
                        output += " = " + (defaultValue == null ? "null" : $"\"{defaultValue}\"");
                    }
                    else
                    {
                        output += " = " + (p.DefaultValue?.ToString() ?? "null");
                    }
                }
                return output;
            }
            foreach (var (index, method) in type.GetMethods().WithIndex())
            {
                var ps = method.GetParameters();

                sb.Clear();
                sb.AppendLine($"{index}. {method} ({method.IsStatic.ToString("静态")} {method.IsVirtual.ToString("虚")} {method.IsAbstract.ToString("抽象")})");
                sb.AppendLine($"形参: ({Common_Util.String.StringHelper.Concat(ps.Select(paramStr).ToList(), ", ")})");
                sb.AppendLine($"返回值: {method.ReturnType.Name}");

                Logger.Test.Info(sb.ToString());
            }
        }

        [TestMethod]
        void test1()
        {
            ImplB.Method02("测试");
        }

        enum Logger
        {
            [Logger(nameof(Test))]
            Test,
            [Logger(nameof(Interface))]
            Interface,
            [Logger(nameof(ImplA))]
            ImplA,
            [Logger(nameof(ImplB))]
            ImplB,
            [Logger(nameof(ImplDuck))]
            ImplDuck,
        }

        interface ITestBase
        {
            static virtual void Method02(string msg)
            {
                Logger.Interface.Info("Method02: " + msg);
            }
        }
        interface ITest<T> : ITestBase
            where T : ITest<T>
        {
            void Method01();
            string Property1 { get; }
            int Property2 { get; }
            static abstract void Method03(string msg);

            static abstract void Method04(ref string msg);

            static abstract void Method05(out string msg);

            static abstract bool operator >(T t1, T t2);
            static abstract bool operator <(T t1, T t2);
        }

        class ImplA : ITest<ImplA>
        {
            public virtual string Property1 => "ImplA";

            public int Property2 { get; set; } = 1;

            public static void Method03(string msg)
            {
                Logger.ImplA.Info("Method03: " + msg);
            }

            public static void Method04(ref string msg)
            {
                Logger.ImplA.Info("Method04: " + msg);
            }

            public static void Method05(out string msg)
            {
                msg = "ImplA Method05";
                Logger.ImplA.Info("Method05: " + msg);
            }

            public void Method01()
            {
                Logger.ImplA.Info("Method01!");
            }


            public static bool operator <(ImplA t1, ImplA t2)
            {
                Logger.ImplA.Info($"ImplA({t1.Property2}) < ImplA({t2.Property2}) ?");
                return t1.Property2 < t2.Property2;
            }

            public static bool operator >(ImplA t1, ImplA t2)
            {
                Logger.ImplA.Info($"ImplA({t1.Property2}) > ImplA({t2.Property2}) ?");
                return t1.Property2 > t2.Property2;
            }
        }

        class ImplB : ImplA, ITest<ImplB>
        {
            public override string Property1 => "ImplB";
            public static void Method02(string msg = "123")
            {
                Logger.ImplB.Info("Method02: " + msg);
            }

            public static bool operator <(ImplB t1, ImplB t2)
            {
                Logger.ImplB.Info($"ImplB({t1.Property2}) < ImplB({t2.Property2}) ?");
                return t1.Property2 < t2.Property2;
            }

            public static bool operator >(ImplB t1, ImplB t2)
            {
                Logger.ImplB.Info($"ImplB({t1.Property2}) < ImplB({t2.Property2}) ?");
                return t1.Property2 < t2.Property2;
            }
        }

        class ImplDuck 
        {
            public string Property1 => "ImplDuck";

            public int Property2 { get; set; } = 4;

            public static void Method03(string msg)
            {
                Logger.ImplDuck.Info("Method03: " + msg);
            }
            public static void Method04(ref string msg)
            {
                Logger.ImplDuck.Info("Method04: " + msg);
            }

            public static void Method05(out string msg)
            {
                msg = "ImplDuck Method05";
                Logger.ImplDuck.Info("Method05: " + msg);
            }

            public void Method1()
            {
                Logger.ImplDuck.Info("Method01!");
            }


            public static bool operator <(ImplDuck t1, ImplDuck t2)
            {
                Logger.ImplDuck.Info($"ImplDuck({t1.Property2}) < ImplDuck({t2.Property2}) ?");
                return t1.Property2 < t2.Property2;
            }

            public static bool operator >(ImplDuck t1, ImplDuck t2)
            {
                Logger.ImplDuck.Info($"ImplDuck({t1.Property2}) > ImplDuck({t2.Property2}) ?");
                return t1.Property2 > t2.Property2;
            }
        }
    }
}
