using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using Common_Util.Extensions;
using CommonLibTest_Console.TestModels;

namespace CommonLibTest_Console.DynamicIL
{
    public class Method001() : TestBase("动态生成代码 emit 实现统一调用某个方法")
    {
        protected override void RunImpl()
        {
            AssemblyName assemblyName = new AssemblyName("CommonLibTest_Console.DynamicIL.ImplTest");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Method001");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "ITest001Impl", 
                TypeAttributes.Public | TypeAttributes.Class,
                parent: null,
                interfaces: [typeof(ITest001)]);

            MethodInfo[] methods = typeof(ITest001).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in methods)
            {
                if (method.IsGenericMethod)
                {
                    throw new NotSupportedException("不支持动态生成泛型方法! ");
                }

                WriteLine("生成方法: " + method.Name);

                ParameterInfo[] parameters = method.GetParameters();
                Type[] parameterTypes = parameters.Select(i => i.ParameterType).ToArray();
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    method.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    method.ReturnType,
                    parameterTypes);
                ILGenerator iLGenerator = methodBuilder.GetILGenerator();
                
                if (method.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    Common_Util.Module.DynamicIL.CommonMethodLogic.UnityDealAsync(
                        iLGenerator, method,
                        dealHandlerAsync);
                }
                else
                {
                    Common_Util.Module.DynamicIL.CommonMethodLogic.UnityDeal(
                        iLGenerator, method,
                        dealHandler);
                }
            }

            Type type = typeBuilder.CreateType();
            WriteLine("生成类型: " + type.FullName);

            object? obj = Activator.CreateInstance(type);

            WriteLine("生成对象: ");
            if (obj == null)
            {
                WriteLine("null");
                return;
            }
            else
            {
                WriteLine(obj.FullInfoString());
            }

            WriteLine("调用测试: ");
            WriteLine("M1(3, 8)", ((ITest001)obj).M1(3, 8));
            WriteLine("M2(\"3\", \"8\")", ((ITest001)obj).M2("3", "8"));

            Task.Run(async () =>
            {
                WriteLine("M3(1, 2, 3)", await ((ITest001)obj).M3(1, 2, 3));
                await ((ITest001)obj).M4("!!! !!! 123123");
            }).GetAwaiter().GetResult();
        }


        public object? dealHandler(Common_Util.Module.DynamicIL.CommonMethodLogic.MethodInput input)
        {
            WriteLine("方法调用, 原型: " + input.Prototype.Name);
            WriteLine("数据: ");
            WriteLine(input.Parameters.FullInfoString());

            if (input.Prototype.Name == nameof(ITest001.M1))
            {
                int x = (int?)input.Get("x") ?? 0;
                int y = (int?)input.Get("y") ?? 0;
                return x + y;
            }
            else if (input.Prototype.Name == nameof(ITest001.M2))
            {
                string x = (string?)input.Get("x") ?? "x_null";
                string y = (string?)input.Get("y") ?? "y_null";
                return x + " ! " + y;
            }

            return null;
        }


        public async Task<object?> dealHandlerAsync(Common_Util.Module.DynamicIL.CommonMethodLogic.MethodInput input)
        {
            WriteLine("方法调用, 原型: " + input.Prototype.Name);
            await Task.Delay(1000);
            WriteLine("数据: ");
            await Task.Delay(1000);
            WriteLine(input.Parameters.FullInfoString());
            await Task.Delay(1000);

            if (input.Prototype.Name == nameof(ITest001.M3))
            {
                int x = (int?)input.Get("x") ?? 0;
                int y = (int?)input.Get("y") ?? 0;
                int z = (int?)input.Get("z") ?? 0;
                return x + y + z;
            }
            else if (input.Prototype.Name == nameof(ITest001.M4))
            {
                await Task.Delay(1000);
                WriteLine(input.Get("str"));
                await Task.Delay(1000);
                WriteLine(input.Get("str"));
            }

            return null;
        }
    }

}
