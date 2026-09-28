using System.Reflection;
using System.Reflection.Emit;
using ChaoticKit.Module.DynamicIL;

namespace ChaoticKit.LibTest.Unit.DynamicIL
{
    /// <summary>
    /// 改写自 ChaoticKit.LibTest.Console.DynamicIL.Method001
    /// 验证 CommonMethodLogic.UnityDeal / UnityDealAsync 动态生成 IL:
    /// - 同步方法 (M1/M2): 动态生成类型实现接口后, 调用时把参数传给处理方法, 返回值正确
    /// - 异步方法 (M3/M4): 动态生成类型实现接口后, 调用时把参数传给处理方法, await 后返回值正确
    /// 迁移时移除原测试中的 5 处 Task.Delay(1000), 异步调用改为直接 await, 保证确定性与速度
    /// </summary>
    /// <remarks>
    /// ⚠ 本测试类由 AI 编写, 未经人工审核, 使用前请另行确认。
    /// </remarks>
    [TestClass]
    public sealed class CommonMethodLogicTest : UnitTestBase
    {
        /// <summary>
        /// 被测接口 (原 Console 工程 TestModels.ITest001 的自包含副本):
        /// 动态生成类型将实现该接口, 生成的方法委托给 UnityDeal / UnityDealAsync 的处理方法
        /// </summary>
        public interface ITest001
        {
            int M1(int x, int y);

            string M2(string x, string y);

            Task<int> M3(int x, int y, int z);

            Task M4(string str);
        }

        /// <summary>
        /// 动态生成实现 ITest001 的类型实例, 流程与原测试一致
        /// </summary>
        private ITest001 CreateImpl()
        {
            AssemblyName assemblyName = new AssemblyName("ChaoticKit.LibTest.Unit.DynamicIL.ImplTest");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("CommonMethodLogicTest");

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

                Log("生成方法: " + method.Name);

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
                    CommonMethodLogic.UnityDealAsync(iLGenerator, method, DealHandlerAsync);
                }
                else
                {
                    CommonMethodLogic.UnityDeal(iLGenerator, method, DealHandler);
                }
            }

            Type type = typeBuilder.CreateTypeInfo().AsType();
            Log("生成类型: " + type.FullName);

            object? obj = Activator.CreateInstance(type);
            Assert.IsNotNull(obj, "动态生成类型实例化失败");
            Assert.IsInstanceOfType(obj, typeof(ITest001), "动态生成类型应实现 ITest001 接口");
            return (ITest001)obj;
        }

        [DataTestMethod]
        [DataRow(3, 8, 11)]
        [DataRow(-5, 7, 2)]
        [DataRow(0, 0, 0)]
        public void UnityDeal_M1_同步方法求和(int x, int y, int expected)
        {
            Log("--- 测试: UnityDeal 生成同步方法 M1 (x+y) ---");
            ITest001 impl = CreateImpl();

            int actual = impl.M1(x, y);

            Assert.AreEqual(expected, actual, $"M1({x}, {y}) 应等于 {expected}");
            Log($"M1({x}, {y}) = {actual}, 通过");
        }

        [DataTestMethod]
        [DataRow("3", "8", "3 ! 8")]
        [DataRow("a", "b", "a ! b")]
        public void UnityDeal_M2_同步方法字符串拼接(string x, string y, string expected)
        {
            Log("--- 测试: UnityDeal 生成同步方法 M2 (x + \" ! \" + y) ---");
            ITest001 impl = CreateImpl();

            string actual = impl.M2(x, y);

            Assert.AreEqual(expected, actual, $"M2(\"{x}\", \"{y}\") 应等于 \"{expected}\"");
            Log($"M2(\"{x}\", \"{y}\") = \"{actual}\", 通过");
        }

        [DataTestMethod]
        [DataRow(1, 2, 3, 6)]
        [DataRow(0, 0, 0, 0)]
        [DataRow(10, 20, 30, 60)]
        public async Task UnityDealAsync_M3_异步方法求和(int x, int y, int z, int expected)
        {
            Log("--- 测试: UnityDealAsync 生成异步方法 M3 (x+y+z), await 返回值 ---");
            ITest001 impl = CreateImpl();

            int actual = await impl.M3(x, y, z);

            Assert.AreEqual(expected, actual, $"M3({x}, {y}, {z}) 应等于 {expected}");
            Log($"M3({x}, {y}, {z}) = {actual}, 通过");
        }

        [TestMethod]
        public async Task UnityDealAsync_M4_无返回值异步方法可执行()
        {
            Log("--- 测试: UnityDealAsync 生成异步无返回值方法 M4, 可 await 执行 ---");
            ITest001 impl = CreateImpl();

            await impl.M4("!!! !!! 123123");

            Log("M4(\"!!! !!! 123123\") 执行完成, 无异常, 通过");
        }

        /// <summary>
        /// 同步处理方法: M1 返回 x+y, M2 返回 x + " ! " + y (与原测试逻辑一致)
        /// </summary>
        public object? DealHandler(CommonMethodLogic.MethodInput input)
        {
            Log("方法调用(同步), 原型: " + input.Prototype.Name);

            if (input.Prototype.Name == nameof(ITest001.M1))
            {
                int x = (int?)input.Get("x") ?? 0;
                int y = (int?)input.Get("y") ?? 0;
                Log($"M1 参数 x={x}, y={y}");
                return x + y;
            }
            else if (input.Prototype.Name == nameof(ITest001.M2))
            {
                string x = (string?)input.Get("x") ?? "x_null";
                string y = (string?)input.Get("y") ?? "y_null";
                Log($"M2 参数 x={x}, y={y}");
                return x + " ! " + y;
            }

            return null;
        }

        /// <summary>
        /// 异步处理方法: M3 返回 x+y+z, M4 仅读取参数 str
        /// (已移除原测试的 Task.Delay(1000), 无真实异步工作, 直接返回已完成任务)
        /// </summary>
        public Task<object?> DealHandlerAsync(CommonMethodLogic.MethodInput input)
        {
            Log("方法调用(异步), 原型: " + input.Prototype.Name);

            if (input.Prototype.Name == nameof(ITest001.M3))
            {
                int x = (int?)input.Get("x") ?? 0;
                int y = (int?)input.Get("y") ?? 0;
                int z = (int?)input.Get("z") ?? 0;
                Log($"M3 参数 x={x}, y={y}, z={z}");
                return Task.FromResult<object?>(x + y + z);
            }
            else if (input.Prototype.Name == nameof(ITest001.M4))
            {
                Log("M4 参数 str=" + input.Get("str"));
            }

            return Task.FromResult<object?>(null);
        }
    }
}
