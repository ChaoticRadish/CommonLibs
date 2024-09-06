using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common_Util.Extensions;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Common_Util.Module.DynamicIL
{
    /// <summary>
    /// IL动态生成方法代码时常用的逻辑
    /// </summary>
    public static class CommonMethodLogic
    {
        #region 方法原型

        /// <summary>
        /// 方法原型的字典
        /// </summary>
        private static ConcurrentDictionary<string, MethodInfo> prototypeDic = new();

        /// <summary>
        /// 记录方法原型到静态缓存
        /// </summary>
        /// <param name="method"></param>
        /// <returns>记录的key值, 即原型的完整名</returns>
        private static string RecordPrototype(MethodInfo method)
        {
            string key = $"{method.DeclaringType?.Name ?? "null"} -> {method.Name}({String.StringHelper.Concat(method.GetParameters().Select(i => i.ParameterType.Name).ToList(), ", ")})";
            prototypeDic.AddOrUpdate(key, method, (key, oldValue) => method);
            return key;
        }
        #endregion

        #region 对象引用

        /// <summary>
        /// 对象静态缓存字典
        /// </summary>
        private static ConcurrentDictionary<string, object> objectDic = new();

        /// <summary>
        /// 记录对象到静态缓存
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>记录的key值, 动态生成</returns>
        private static string RecordObject(object obj)
        {
            var exist = objectDic.FirstOrDefault(i => i.Value == obj);
            if (exist.Key.IsEmpty())
            {
                string key = Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objectDic.AddOrUpdate(key, obj, (key, old) => obj);
                return key;
            }
            else
            {
                return exist.Key;
            }
        }
        /// <summary>
        /// 获取记录到的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object? GetObject(string key)
        {
            return objectDic.TryGetValue(key, out var obj) ? obj : null;
        }

        #endregion

        public class MethodInput
        {
            public MethodInput(string prototypeName)
            {
                Prototype = prototypeDic[prototypeName];
                ParameterInfo[] parameters = Prototype.GetParameters();
                Parameters = parameters.ToDictionary(i => i, i => i.DefaultValue);
            }

            /// <summary>
            /// 被调用的方法原型
            /// </summary>
            public MethodInfo Prototype { get; set; }

            /// <summary>
            /// 输入参数, 其中 Key: 对应原型方法的参数信息; Value: 参数输入值
            /// </summary>
            public Dictionary<ParameterInfo, object?> Parameters { get; set; }

            /// <summary>
            /// 字段, 其中 Key: 字段名; Value: 调用方法时取得的值
            /// </summary>
            public Dictionary<string, object?> Fields { get; set; } = [];

            /// <summary>
            /// 设置输入参数 (方法参数)
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void Set(string name, object? value)
            {
                ParameterInfo? parameter = Parameters.Keys.FirstOrDefault(i => i.Name == name);
                if (parameter == null)
                {
                    return;
                }
                Parameters[parameter] = value;
            }

            /// <summary>
            /// 获取输入参数 (方法参数)
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public object? Get(string name)
            {
                ParameterInfo? parameter = Parameters.Keys.FirstOrDefault(i => i.Name == name);
                if (parameter == null)
                {
                    return null;
                }
                else
                {
                    object? output = Parameters[parameter];
                    if (output != null && output.Equals(DBNull.Value))
                    {
                        return null;
                    }
                    else
                    {
                        return output;
                    }
                }
            }

            /// <summary>
            /// 设置字段值 
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void SetField(string name, object? value)
            {
                Fields.Set(name, value);
            }
            /// <summary>
            /// 获取字段值
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public object? GetField(string name)
            {
                Fields.TryGetValue(name, out var output);
                return output;
            }


            /// <summary>
            /// 原型返回值
            /// </summary>
            public Type ReturnType { get => Prototype.ReturnType; }
        }

        public delegate object? UnityDealHandler(MethodInput input);
        public delegate Task<object?> UnityAsyncDealHandler(MethodInput input);

        #region 仅使用传入参数, 不使用字段

        /// <summary>
        /// 统一的处理逻辑
        /// </summary>
        /// <param name="il"></param>
        /// <param name="prototype"></param>
        /// <param name="dealHandler"></param>
        public static void UnityDeal(
            ILGenerator il,
            MethodInfo prototype,
            UnityDealHandler dealHandler)
        {
            UnityDeal(il, prototype, [], dealHandler);
        }
        /// <summary>
        /// 统一的处理逻辑, 异步执行, 返回结果需继承自 Task
        /// </summary>
        /// <param name="il"></param>
        /// <param name="prototype"></param>
        /// <param name="dealHandler"></param>
        public static void UnityDealAsync(
            ILGenerator il,
            MethodInfo prototype,
            UnityAsyncDealHandler dealHandler)
        {
            UnityDealAsync(il, prototype, [], dealHandler);
        }

        #endregion

        #region 使用传入参数以及一些字段

        /// <summary>
        /// 统一的处理逻辑, 指定的成员变量会被一同传给处理方法使用
        /// </summary>
        /// <param name="il"></param>
        /// <param name="prototype"></param>
        /// <param name="fields">需要被读取的字段</param>
        /// <param name="dealHandler"></param>
        public static void UnityDeal(
            ILGenerator il,
            MethodInfo prototype,
            FieldBuilder[] fields,
            UnityDealHandler dealHandler)
        {

            string key = RecordPrototype(prototype);
            string? handlerTargetKey = null;
            if (dealHandler.Target != null)
            {
                handlerTargetKey = RecordObject(dealHandler.Target);
            }

            ConstructorInfo inputCtor = typeof(MethodInput).GetConstructor([typeof(string)])!;
            MethodInfo inputSet = typeof(MethodInput).GetMethod(nameof(MethodInput.Set), BindingFlags.Public | BindingFlags.Instance)!; // 设置传入参数到数据包的方法
            MethodInfo fieldSet = typeof(MethodInput).GetMethod(nameof(MethodInput.SetField), BindingFlags.Public | BindingFlags.Instance)!;    // 设置字段到数据包的方法
            MethodInfo dealMethod = dealHandler.GetMethodInfo();

            bool returnVoid = prototype.ReturnType == typeof(void);


            #region 生成的方法参考
            /* private int intField;    // 调用方法时要求的需要被使用的字段
             * public string template(string str, Model01 model)
             * {
             *     MethodInput input = new(prototype.key);
             *     
             *     input.Set("str", (object)str);
             *     input.Set("model", (object)model);
             *     
             *     input.SetField("intField", (object)intField);
             *     
             *     object? obj = dealHandler(input);
             *     
             *     return (string)obj;
             * }
             * public void template(string str, Model01 model)
             * {
             *     MethodInput input = new(prototype.key);
             *     
             *     input.Set("str", (object)str);
             *     input.Set("model", (object)model);
             *     
             *     input.SetField("intField", (object)intField);
             *     
             *     object? obj = dealHandler(input);
             *     
             *     return;
             * }
             */
            #endregion


            il.Emit(OpCodes.Nop);

            // 声明 MethodInput input
            LocalBuilder local_input = il.DeclareLocal(typeof(MethodInput));
            // 实例化 input
            il.Emit(OpCodes.Ldstr, key);
            il.Emit(OpCodes.Newobj, inputCtor);
            // 赋值到 input
            il.Emit(OpCodes.Stloc, local_input);

            // 声明变量 paramValue 用于存放参数值
            LocalBuilder local_tempValue = il.DeclareLocal(typeof(object));

            // 方法参数设置到输入参数对象
            ParameterInfo[] parameters = prototype.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                // 获取参数值
                il.Emit(OpCodes.Ldarg, i + 1);  // 传入参数索引从 1 开始
                if (param.ParameterType.IsValueType)
                {
                    // 值类型需先装箱
                    il.Emit(OpCodes.Box, param.ParameterType);
                }
                il.Emit(OpCodes.Stloc, local_tempValue);

                // 载入 input
                il.Emit(OpCodes.Ldloc, local_input);
                // 载入 参数变量名
                il.Emit(OpCodes.Ldstr, param.Name!);
                // 载入 参数变量值
                il.Emit(OpCodes.Ldloc, local_tempValue);
                // 调用 Set方法
                il.Emit(OpCodes.Call, inputSet);
            }

            // 字段值设置到输入从参数对象
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                // 获取字段值
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                {
                    // 值类型需先装箱
                    il.Emit(OpCodes.Box, field.FieldType);
                }
                il.Emit(OpCodes.Stloc, local_tempValue);

                // 载入 input
                il.Emit(OpCodes.Ldloc, local_input);
                // 载入 字段名
                il.Emit(OpCodes.Ldstr, field.Name);
                // 载入 字段值
                il.Emit(OpCodes.Ldloc, local_tempValue);
                // 调用 Set方法
                il.Emit(OpCodes.Call, fieldSet);

            }

            // 声明 obj
            LocalBuilder local_obj = il.DeclareLocal(typeof(object));
            // 调用 dealHandler
            if (handlerTargetKey == null)
            {
            }
            else
            {
                // 调用 GetObject, 载入处理方法的 Target
                MethodInfo getObjectMethod = typeof(CommonMethodLogic).GetMethod(nameof(GetObject), BindingFlags.Public | BindingFlags.Static)!;
                il.Emit(OpCodes.Ldstr, handlerTargetKey);
                il.Emit(OpCodes.Call, getObjectMethod);
            }
            // 载入 input
            il.Emit(OpCodes.Ldloc, local_input);
            // 调用处理方法
            il.Emit(OpCodes.Call, dealMethod);
            // 赋值到 obj
            il.Emit(OpCodes.Stloc, local_obj);

            if (!returnVoid)
            {
                // 加载 obj 用于返回
                il.Emit(OpCodes.Ldloc, local_obj);

                if (prototype.ReturnType.IsValueType)
                {
                    // 拆箱
                    il.Emit(OpCodes.Unbox_Any, prototype.ReturnType);
                }
                else
                {
                    // 强制转换变量为指定类型
                    il.Emit(OpCodes.Castclass, prototype.ReturnType);
                }

            }
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 统一的处理逻辑, 异步执行, 返回结果需继承自 Task, 指定的成员变量会被一同传给处理方法使用
        /// </summary>
        /// <param name="il"></param>
        /// <param name="prototype"></param>
        /// <param name="fields">需要被读取的字段</param>
        /// <param name="dealHandler"></param>
        public static void UnityDealAsync(
            ILGenerator il,
            MethodInfo prototype,
            FieldBuilder[] fields,
            UnityAsyncDealHandler dealHandler)
        {
            if (!prototype.ReturnType.IsAssignableTo(typeof(Task)))
            {
                throw new Exception("返回结果不继承自 Task");
            }

            string key = RecordPrototype(prototype);
            string? handlerTargetKey = null;
            if (dealHandler.Target != null)
            {
                handlerTargetKey = RecordObject(dealHandler.Target);
            }

            ConstructorInfo inputCtor = typeof(MethodInput).GetConstructor([typeof(string)])!;
            MethodInfo inputSet = typeof(MethodInput).GetMethod(nameof(MethodInput.Set), BindingFlags.Public | BindingFlags.Instance)!; // 设置传入参数到数据包的方法
            MethodInfo fieldSet = typeof(MethodInput).GetMethod(nameof(MethodInput.SetField), BindingFlags.Public | BindingFlags.Instance)!;    // 设置字段到数据包的方法
            MethodInfo dealMethod = dealHandler.GetMethodInfo();

            bool withData = prototype.ReturnType != typeof(Task);

            il.Emit(OpCodes.Nop);

            // 声明 MethodInput input
            LocalBuilder local_input = il.DeclareLocal(typeof(MethodInput));
            // 实例化 input
            il.Emit(OpCodes.Ldstr, key);
            il.Emit(OpCodes.Newobj, inputCtor);
            // 赋值到 input
            il.Emit(OpCodes.Stloc, local_input);

            // 声明变量 paramValue 用于临时存放参数值或字段值等
            LocalBuilder local_tempValue = il.DeclareLocal(typeof(object));

            // 方法参数设置到输入参数对象
            ParameterInfo[] parameters = prototype.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                // 获取参数值
                il.Emit(OpCodes.Ldarg, i + 1);  // 传入参数索引从 1 开始
                if (param.ParameterType.IsValueType)
                {
                    // 值类型需先装箱
                    il.Emit(OpCodes.Box, param.ParameterType);
                }
                il.Emit(OpCodes.Stloc, local_tempValue);

                // 载入 input
                il.Emit(OpCodes.Ldloc, local_input);
                // 载入 参数变量名
                il.Emit(OpCodes.Ldstr, param.Name!);
                // 载入 参数变量值
                il.Emit(OpCodes.Ldloc, local_tempValue);
                // 调用 Set方法
                il.Emit(OpCodes.Call, inputSet);
            }

            // 字段值设置到输入从参数对象
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                // 获取字段值
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                {
                    // 值类型需先装箱
                    il.Emit(OpCodes.Box, field.FieldType);
                }
                il.Emit(OpCodes.Stloc, local_tempValue);

                // 载入 input
                il.Emit(OpCodes.Ldloc, local_input);
                // 载入 字段名
                il.Emit(OpCodes.Ldstr, field.Name);
                // 载入 字段值
                il.Emit(OpCodes.Ldloc, local_tempValue);
                // 调用 Set方法
                il.Emit(OpCodes.Call, fieldSet);

            }

            // 声明 task
            LocalBuilder local_task = il.DeclareLocal(typeof(object));
            // 调用 dealHandler
            if (handlerTargetKey == null)
            {
            }
            else
            {
                // 调用 GetObject
                MethodInfo getObjectMethod = typeof(CommonMethodLogic).GetMethod(nameof(GetObject), BindingFlags.Public | BindingFlags.Static)!;
                il.Emit(OpCodes.Ldstr, handlerTargetKey);
                il.Emit(OpCodes.Call, getObjectMethod);
            }
            // 载入 input
            il.Emit(OpCodes.Ldloc, local_input);
            // 调用处理方法
            il.Emit(OpCodes.Call, dealMethod);
            // 赋值到 task
            il.Emit(OpCodes.Stloc, local_task);

            if (withData)
            {
                Type resultType = prototype.ReturnType.GetGenericArguments()[0];
                if (resultType.IsGenericParameter)
                {
                    throw new Exception("返回结果不能是泛型形参! ");
                }

                MethodInfo taskConvertMethod = typeof(CommonMethodLogic).GetMethod(
                    resultType.IsValueType ? nameof(ConvertTaskValueTypeResult) : nameof(ConvertTaskResult),
                    BindingFlags.Public | BindingFlags.Static)!;
                taskConvertMethod = taskConvertMethod.MakeGenericMethod([resultType]);

                il.Emit(OpCodes.Ldloc, local_task);
                il.Emit(OpCodes.Call, taskConvertMethod);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, local_task);
                il.Emit(OpCodes.Castclass, typeof(Task));
            }


            il.Emit(OpCodes.Ret);

        }
        #endregion

        #region 测试
        private static void InsertTestPoint(ILGenerator il, LocalBuilder? loc = null)
        {
#if DEBUG
            if (loc != null)
            {
                if (loc.LocalType.IsValueType)
                {
                    il.Emit(OpCodes.Ldloc, loc);
                    il.Emit(OpCodes.Box, loc.LocalType);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, loc);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
            //il.Emit(OpCodes.Ldftn, typeof(CommonMethodLogic).GetMethod(nameof(TestPoint), BindingFlags.Public | BindingFlags.Static, [typeof(object)])!);
            //il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), [typeof(object)], null);
            il.Emit(OpCodes.Call, typeof(CommonMethodLogic).GetMethod(nameof(TestPoint), BindingFlags.Public | BindingFlags.Static)!);
#endif
        }
#if DEBUG
        public static void TestPoint(object obj)
        {
            if (obj == null)
            {

            }
            else
            {
                Type type = obj.GetType();
            }
            Console.WriteLine(obj?.FullInfoString());
        }
#endif
        #endregion

        #region 转换
        /// <summary>
        /// 待传入参数执行完后, 将结果转换为指定的类型 T 返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<T> ConvertTaskValueTypeResult<T>(Task<object?> task)
        {
            object? obj = await task;
            if (obj == null)
            {
                return default!;
            }
            else
            {
                return (T)obj;
            }
        }
        /// <summary>
        /// 待传入参数执行完后, 将结果转换为指定的类型 T 返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<T?> ConvertTaskResult<T>(Task<object?> task)
        {
            object? obj = await task;
            return (T?)obj;
        }
        #endregion
    }
}
