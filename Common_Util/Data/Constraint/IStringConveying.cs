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
    /// 与字符串可以互相转换的对象
    /// </summary>
    public interface IStringConveying
    {
        /// <summary>
        /// 将本实例转换为字符串, 转换后的字符串可以使用 <see cref="StringConveyingHelper.FromString"/> 生成与本实例等价的另一个实例
        /// </summary>
        /// <returns></returns>
        string ConvertToString();

        /// <summary>
        /// 修改当前实例数据为输入字符串代表的数据
        /// </summary>
        /// <param name="value"></param>
        void ChangeValue(string value);
    }
    public static class StringConveyingHelper
    {
        /// <summary>
        /// 实例化一个对象, 并使用指定字符串赋初值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static T FromString<T>(string str)
            where T : IStringConveying, new()
        {
            return _fromStringImpl<T>(str);
        }


        /// <summary>
        /// 实例化一个对象, 并使用指定字符串赋初值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object FromString(Type type, string str)
        {
            // 输入限制
            var ex = _creatableCheck(type);
            // 调用转换
            return _invokeFromStringImpl(type, str);
        }
        /// <summary>
        /// 实例化一个对象, 并使用指定字符串赋初值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFromString(Type type, string str, [NotNullWhen(true)] out object? output)
        {
            var ex = _creatableCheck(type);
            if (ex != null)
            {
                output = null;
                return false;
            }
            else
            {
                output = _invokeFromStringImpl(type, str);
                return true;
            }
        }

        #region 私有方法

        private static object _invokeFromStringImpl(Type type, string str)
        {
            MethodInfo method = _fromStringImplMethodInfo.Value.MakeGenericMethod(type);
            object output = method.Invoke(null, [str])!;
            return output;
        }

        private static Lazy<MethodInfo> _fromStringImplMethodInfo = new(() =>
        {
            return typeof(StringConveyingHelper)
                .GetMethod(nameof(_fromStringImpl), BindingFlags.NonPublic | BindingFlags.Static)!;
        });
        private static T _fromStringImpl<T>(string str)
            where T : IStringConveying, new()
        {
            T output = new();
            output.ChangeValue(str);
            return output;
        }

        /// <summary>
        /// 检查输入类型是否可以自动创建
        /// </summary>
        /// <remarks>
        /// 需符合以下要求: <br/>
        /// 1. 继承接口 <see cref="IStringConveying"/> <br/>
        /// 2. 具有公共无参构造函数
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Exception? _creatableCheck(Type type)
        {
            if (!type.IsAssignableTo(typeof(IStringConveying)))
            {
                return new ArgumentException($"输入类型 {type.Name} 不继承自 {typeof(IStringConveying).Name}", nameof(type));
            }
            if (!type.HavePublicEmptyCtor())
            {
                return new ArgumentException($"输入类型 {type.Name} 没用公共无参构造函数", nameof(type));
            }
            return null;
        }


        #endregion
    }

    public static class IStringConveyingExtension
    {
    }
}
