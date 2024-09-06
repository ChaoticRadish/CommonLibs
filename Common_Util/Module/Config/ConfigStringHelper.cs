using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 配置值字符串帮助类
    /// </summary>
    public static class ConfigStringHelper
    {

        #region 值转换

        private const string COLLECTION_SPLIT = "; ";

        /// <summary>
        /// 对象转换为字符串配置值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? Obj2ConfigValue(object? obj)
        {
            if (obj == null) return null;

            if (obj is Type _type)
            {
                return _type.FullName;
            }
            if (typeof(IEnumerable<string>).IsAssignableFrom(obj.GetType()))
            {
                return StringHelper.Concat(((IEnumerable<string>)obj).ToList(), "; ", false);
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(obj.GetType()))
            {
                return StringHelper.Concat(((IEnumerable<int>)obj).Select(i => i.ToString()).ToList(), "; ", false);
            }
            if (obj is IStringConveying conveying)
            {
                return conveying.ConvertToString();
            }
            if ((obj.GetType().IsEnumerable() || obj.GetType().IsList())
                && obj.GetType().GenericTypeArguments.Length == 1
                && typeof(IStringConveying).IsAssignableFrom(obj.GetType().GenericTypeArguments[0]))
            {
                IEnumerable list = (IEnumerable)obj;
                List<string> valueStrings = new List<string>();
                Type type = obj.GetType().GenericTypeArguments[0];
                foreach (object item in list)
                {
                    valueStrings.Add(((IStringConveying)item).ConvertToString());
                }
                return StringHelper.Concat(valueStrings, COLLECTION_SPLIT, false);
            }
            else if ((obj.GetType().IsEnumerable() || obj.GetType().IsList())
                && obj.GetType().GenericTypeArguments.Length == 1
                && obj.GetType().GenericTypeArguments[0].IsEnum)
            {
                IEnumerable list = (IEnumerable)obj;
                List<string> valueStrings = new List<string>();
                Type type = obj.GetType().GenericTypeArguments[0];
                foreach (object item in list)
                {
                    string? str = Enum.GetName(type, item);
                    if (str != null)
                    {
                        valueStrings.Add(str);
                    }
                }
                return StringHelper.Concat(valueStrings, COLLECTION_SPLIT, false);
            }
            else
            {
                return obj.ToString();
            }
        }
        /// <summary>
        /// 字符串配置值转换为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T? ConfigValue2Obj<T>(string str)
        {
            object? obj = ConfigValue2Obj(str, typeof(T));
            if (obj == null) return default;
            else return (T)obj;
        }
        /// <summary>
        /// 字符串配置值转换为对象
        /// </summary>
        /// <param name="str"></param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static object? ConfigValue2Obj(string? str, Type targetType)
        {
            if (str == null) return null;

            if (targetType == typeof(string))
            {
                return str;
            }
            else if (targetType == typeof(bool))
            {
                return ValueHelper.IsTrueString(str);
            }
            else if (targetType == typeof(int))
            {
                return int.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(uint))
            {
                return uint.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(long))
            {
                return long.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(ulong))
            {
                return ulong.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(float))
            {
                return float.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(double))
            {
                return double.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(byte))
            {
                return byte.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(char))
            {
                return char.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(short))
            {
                return short.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(ushort))
            {
                return ushort.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(decimal))
            {
                return decimal.TryParse(str, out var val) ? val : 0;
            }
            else if (targetType == typeof(DateTime))
            {
                return DateTime.TryParse(str, out var val) ? val : default;
            }
            else if (targetType.IsAssignableTo(typeof(IStringConveying)))
            {
                return StringConveyingHelper.FromString(targetType, str);
            }
            else if (targetType.IsEnum)
            {
                return EnumHelper.Convert(targetType, str);
            }
            else if (targetType == typeof(Type))
            {
                Type? output = null;
                TypeHelper.ForeachCurrentDomainType(type =>
                {
                    if (type.FullName == str)
                    {
                        output = type;
                        return true;
                    }
                    return false;
                });
                return output;
            }
            else if (targetType.IsGenericType)
            {
                var generiacTypeDefinition = targetType.GetGenericTypeDefinition();
                if (generiacTypeDefinition == typeof(IEnumerable<>)
                    || generiacTypeDefinition == typeof(IList<>)
                    || generiacTypeDefinition == typeof(List<>))
                {
                    var genericArgs = targetType.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        IList? list = (IList?)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgs[0]));
                        if (list == null) return null;
                        string[] strs = str.Split(COLLECTION_SPLIT);
                        foreach (string _s in strs)
                        {
                            object? obj = ConfigValue2Obj(_s, genericArgs[0]);
                            list.Add(obj);
                        }
                        return list;
                    }
                }
            }
            return null;
        }
        #endregion

    }
}
