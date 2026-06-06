using ChaoticKit.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.DynamicIL
{
    public static class ILDebugger
    {
        /// <summary>
        /// 往 <paramref name="il"/> 添加一段调用 <see cref="System.Diagnostics.Debugger.Break"/> 的语句
        /// </summary>
        /// <param name="il"></param>
        public static void Break(ILGenerator il)
        {
            var breakMethod = typeof(System.Diagnostics.Debugger)
                .GetMethod(nameof(System.Diagnostics.Debugger.Break), BindingFlags.Public | BindingFlags.Static)!;
            il.Emit(OpCodes.Call, breakMethod);
        }

        /// <summary>
        /// 往 <paramref name="il"/> 添加一段调用 <see cref="EnumLogExtensions.Debug{TEnum}"/> 的语句
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="il"></param>
        /// <param name="enum"></param>
        /// <param name="message"></param>
        public static void LogDebug<TEnum>(ILGenerator il, TEnum @enum, string message)
            where TEnum : Enum
        {
            var logMethod = typeof(EnumLogExtensions).GetMethod(nameof(EnumLogExtensions.Debug))!
                .MakeGenericMethod(typeof(TEnum));

            ILConstantHelper.Load(il, @enum);
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Call, logMethod);
        }
        public static void LogDebug<TEnum>(ILGenerator il, TEnum @enum, LocalBuilder local)
            where TEnum : Enum
        {
            var logMethod = typeof(EnumLogExtensions).GetMethod(nameof(EnumLogExtensions.Debug))!
                .MakeGenericMethod(typeof(TEnum));

            ILConstantHelper.Load(il, @enum);
            LoadLocalToString(il, local);
            il.Emit(OpCodes.Call, logMethod);
        }


        /// <summary>
        /// 往 <paramref name="il"/> 添加一段调用 <see cref="EnumLogExtensions.Trace{TEnum}"/> 的语句
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="il"></param>
        /// <param name="enum"></param>
        /// <param name="message"></param>
        public static void LogTrace<TEnum>(ILGenerator il, TEnum @enum, string message)
            where TEnum : Enum
        {
            var logMethod = typeof(EnumLogExtensions).GetMethod(nameof(EnumLogExtensions.Trace))!
                .MakeGenericMethod(typeof(TEnum));

            ILConstantHelper.Load(il, @enum);
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Call, logMethod);
        }
        public static void LogTrace<TEnum>(ILGenerator il, TEnum @enum, LocalBuilder local)
            where TEnum : Enum
        {
            var logMethod = typeof(EnumLogExtensions).GetMethod(nameof(EnumLogExtensions.Trace))!
                .MakeGenericMethod(typeof(TEnum));

            ILConstantHelper.Load(il, @enum);
            LoadLocalToString(il, local);
            il.Emit(OpCodes.Call, logMethod);
        }

        private static void LoadLocalToString(ILGenerator il, LocalBuilder local)
        {
            if (local.LocalType.IsValueType)
            {
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Box, local.LocalType);
                il.Emit(OpCodes.Callvirt, MethodInfo_ObjectToString.Value);
            }
            else
            {
                Label labelNotNull = il.DefineLabel();
                Label labelEnd = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Brtrue_S, labelNotNull);
                // local == null
                il.Emit(OpCodes.Ldstr, $"<{local.LocalType.Name}:null>");
                il.Emit(OpCodes.Br_S, labelEnd);
                // local != null
                il.MarkLabel(labelNotNull);
                il.Emit(OpCodes.Ldloc, local);
                if (local.LocalType != typeof(string))
                {
                    il.Emit(OpCodes.Callvirt, MethodInfo_ObjectToString.Value);
                }
                il.MarkLabel(labelEnd);
            }

        }
        private static Lazy<MethodInfo> MethodInfo_ObjectToString = new(() => typeof(object).GetMethod(nameof(object.ToString))!);

    }
}
