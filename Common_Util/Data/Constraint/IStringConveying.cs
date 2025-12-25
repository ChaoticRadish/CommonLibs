using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Constraint
{
    /// <summary>
    /// 与字符串可以互相无损转换的对象
    /// </summary>
    /// <remarks>
    /// 每一个具体的、非抽象的类型都必须为它自己 <see cref="TSelf"/> 提供 static 运算符的实现。
    /// </remarks>
    public interface IStringConveying<TSelf>
        where TSelf : IStringConveying<TSelf>
    {
        static abstract explicit operator TSelf(string s);
        static abstract explicit operator string(TSelf t);
    }
    public static class StringConveyingHelper
    {
        /// <summary>
        /// 以显式转换的方式, 将字符串转换为目标类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static T FromString<T>(string str)
            where T : IStringConveying<T>
        {
            return (T)str;
        }


        /// <summary>
        /// 以显式转换的方式, 将字符串转换为目标类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object FromString(Type type, string str)
        {
            return _toObj(type, str);
        }

        /// <summary>
        /// 以显式转换的方式, 将对象转换为字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(obj))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ToString(object? obj)
        {
            return obj == null ? null : _toStr(obj.GetType(), obj);
        }
        [return: NotNullIfNotNull(nameof(obj))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ToString(Type type, object? obj)
        {
            return obj == null ? null : _toStr(type, obj);
        }


        /// <summary>
        /// 判断一个类型是否允许与字符串互相转换
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertibleCheck(Type type)
        {
            return _convertibleCheck(type) == null;
        }
        /// <summary>
        /// 检查 <paramref name="type"/> 是否可以与字符串互相转换, 如果可以, 则将其实例 <paramref name="obj"/> 转换并输出到 <paramref name="convertResult"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj">如果是 <see langword="null"/>, <see cref="convertResult"> 有可能是 <see langword="null"/></param>
        /// <param name="convertResult">转换结果</param>
        /// <returns></returns>
        public static bool ToStringIfConvertible(Type type, object obj, [NotNullWhen(true)] out string? convertResult)
        {
            if (ConvertibleCheck(type))
            {
                convertResult = _toStr(type, obj);
                return true;
            }
            else
            {
                convertResult = null;
                return false;
            }
        }
        /// <summary>
        /// 检查 <paramref name="type"/> 是否可以与字符串互相转换, 如果可以, 则将字符串 <paramref name="str"/> 转换并输出到 <paramref name="convertResult"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str">如果是 <see langword="null"/>, <see cref="convertResult"> 有可能是 <see langword="null"/></param>
        /// <param name="convertResult">转换结果</param>
        /// <returns></returns>
        public static bool ToObjectIfConvertible(Type type, string str, [NotNullWhen(true)] out object? convertResult)
        {
            if (ConvertibleCheck(type))
            {
                convertResult = _toObj(type, str);
                return true;
            }
            else
            {
                convertResult = null;
                return false;
            }
        }

        #region 私有方法


        /// <summary>
        /// 检查输入类型是否可以自动创建
        /// </summary>
        /// <remarks>
        /// 需符合以下要求: <br/>
        /// 1. 继承接口 <see cref="IStringConveying{TSelf}"/> <br/>
        /// 2. 具有公共无参构造函数
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Exception? _convertibleCheck(Type type)
        {
            if (!TypeHelper.ExistInterfaceIsDefinitionFrom(type, typeof(IStringConveying<>), out Type[] matches) 
                || 
                // 没有任何一个匹配接口的泛型参数是传入类型
                !matches.Any(t =>
                {
                    var gArgs = t.GetGenericArguments();
                    return gArgs.Length > 0 && gArgs[0] == type;
                }))
            {
                return new ArgumentException($"输入类型 {type.Name} 不实现 {typeof(IStringConveying<>)}", nameof(type));
            }
            return null;
        }

        private static string _toStr(Type type, object obj)
        {
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => _splitMethodName(m.Name) == "op_Explicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == type)
                .First();
            var result = method.Invoke(null, [obj]);
            if (obj != null && (result == null || result is not string))
                throw new Common_Util.Exceptions.General.ImplementationException($"显示转换接口未按预期返回非 null 字符串值");
            return (string)result!;
        }
        private static object _toObj(Type type, string str)
        {
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => _splitMethodName(m.Name) == "op_Explicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string))
                .First();
            var result = method.Invoke(null, [str]);
            if (str != null && (result == null || result.GetType() != type))
                throw new Common_Util.Exceptions.General.ImplementationException($"显示转换接口未按预期返回非 null 值");
            return result!;
        }
        private static string _splitMethodName(string originName)
        {
            var index = originName.LastIndexOf('.');
            if (index < 0) return originName;
            return originName.Substring(index + 1);
        }

        #endregion
    }

    public static class IStringConveyingExtension
    {
    }
}
