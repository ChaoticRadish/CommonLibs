using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

            var objType = obj.GetType();

            string? baseValueConverted = obj switch
            {
                bool v => v.ToString(CultureInfo.InvariantCulture),
                sbyte v => v.ToString(CultureInfo.InvariantCulture),
                byte v => v.ToString(CultureInfo.InvariantCulture),
                short v => v.ToString(CultureInfo.InvariantCulture),
                ushort v => v.ToString(CultureInfo.InvariantCulture),
                int v => v.ToString(CultureInfo.InvariantCulture),
                uint v => v.ToString(CultureInfo.InvariantCulture),
                long v => v.ToString(CultureInfo.InvariantCulture),
                ulong v => v.ToString(CultureInfo.InvariantCulture),
                float v => v.ToString(CultureInfo.InvariantCulture),
                double v => v.ToString(CultureInfo.InvariantCulture),
                decimal v => v.ToString(CultureInfo.InvariantCulture),
                Guid v => v.ToString(),
                DateTime v => v.ToString("o", CultureInfo.InvariantCulture),
                DateTimeOffset v => v.ToString("o", CultureInfo.InvariantCulture),
                DBNull v => "<null>",
                _ => null,
            };
            if (baseValueConverted != null) return baseValueConverted;

            if (obj is Type _type)
            {
                return _type.FullName;
            }
            if (typeof(IEnumerable<string>).IsAssignableFrom(objType))
            {
                return StringHelper.Concat(((IEnumerable<string>)obj).ToList(), "; ", false);
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(objType))
            {
                return StringHelper.Concat(((IEnumerable<int>)obj).Select(i => i.ToString()).ToList(), "; ", false);
            }
            if (StringConveyingHelper.ToStringIfConvertible(objType, obj, out var convertResult))
            {
                return convertResult;
            }
            if ((objType.IsEnumerable() || objType.IsList())
                && objType.GenericTypeArguments.Length == 1
                && StringConveyingHelper.ConvertibleCheck(objType.GenericTypeArguments[0]))
            {
                IEnumerable list = (IEnumerable)obj;
                List<string> valueStrings = new List<string>();
                Type type = objType.GenericTypeArguments[0];
                foreach (object item in list)
                {
                    valueStrings.Add(item == null ? string.Empty : StringConveyingHelper.ToString(item));
                }
                return StringHelper.Concat(valueStrings, COLLECTION_SPLIT, false);
            }
            else if ((objType.IsEnumerable() || objType.IsList())
                && objType.GenericTypeArguments.Length == 1
                && objType.GenericTypeArguments[0].IsEnum)
            {
                IEnumerable list = (IEnumerable)obj;
                List<string> valueStrings = new List<string>();
                Type type = objType.GenericTypeArguments[0];
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
        /// <param name="options">可选参数</param>
        /// <returns></returns>
        public static object? ConfigValue2Obj(string? str, Type targetType, ConfigValue2ObjOptions? options = null)
        {
            options ??= ConfigValue2ObjOptions.Default;
            if (options.Value.EmptyValueConvertWay.HasFlag(EmptyValueConvertWays.Empty2NullResult))
            {
                if (string.IsNullOrEmpty(str)) return null;
            }
            if (options.Value.EmptyValueConvertWay.HasFlag(EmptyValueConvertWays.WhiteSpace2NullResult))
            {
                if (string.IsNullOrWhiteSpace(str)) return null;
            }



            if (str == null) return null;

            Type? nullableTarget = targetType.NullableTarget();
            bool isNullable = nullableTarget != null;
            if (isNullable)
            {
                targetType = nullableTarget!;
            }

            if (targetType == typeof(string))
            {
                return str;
            }
            else if (targetType == typeof(bool))
            {
                if (ValueHelper.TryLooselyParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(int))
            {
                if (int.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(uint))
            {
                if (uint.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(long))
            {
                if (long.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(ulong))
            {
                if (ulong.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(float))
            {
                if (float.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(double))
            {
                if (double.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(sbyte))
            {
                if (sbyte.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(byte))
            {
                if (byte.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(char))
            {
                if (char.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(short))
            {
                if (short.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(ushort))
            {
                if (ushort.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (targetType == typeof(Guid))
            {
                if (Guid.TryParse(str, out var val)) return val;
                else goto ReturnDefault;
            }
            else if (StringConveyingHelper.ToObjectIfConvertible(targetType, str, out var convertResult))
            {
                return convertResult;
            }
            else if (targetType.IsEnum)
            {
                var val = EnumHelper.Convert(targetType, str);
                if (val != null) return val;
                else goto ReturnDefault;
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
                if (output != null) return output;
                else goto ReturnDefault;
            }
            else if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                if (elementType == null) goto ReturnDefault;
                string[] strs = str.Split(COLLECTION_SPLIT);
                var arr = Array.CreateInstance(elementType, strs.Length);
                foreach (var (index, _s) in strs.WithIndex())
                {
                    object? obj = ConfigValue2Obj(_s, elementType);
                    arr.SetValue(obj, index);
                }
                return arr;

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
                        if (list == null) goto ReturnDefault;
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

        ReturnDefault:
            if (isNullable || !targetType.IsValueType) return null;
            else return Activator.CreateInstance(targetType);

        }

        public readonly struct ConfigValue2ObjOptions
        {
            /// <summary>
            /// 默认值
            /// </summary>
            public static ConfigValue2ObjOptions Default => new ConfigValue2ObjOptions()
            {
                EmptyValueConvertWay = EmptyValueConvertWays.None,
            };

            /// <summary>
            /// 各类空值的转换方式
            /// </summary>
            public EmptyValueConvertWays EmptyValueConvertWay { get; init; }
        }
        /// <summary>
        /// 各类空值的转换方式
        /// </summary>
        [Flags]
        public enum EmptyValueConvertWays : int
        {
            /// <summary>
            /// 不做转换
            /// </summary>
            None = 0,
            /// <summary>
            /// 空字符串直接转换为 <see langword="null"/> 结果
            /// </summary>
            /// <remarks>
            /// 最高优先级
            /// </remarks>
            Empty2NullResult = 0b1,
            /// <summary>
            /// 空白字符串直接转换为 <see langword="null"/> 结果
            /// </summary>
            /// <remarks>
            /// 最高优先级
            /// </remarks>
            WhiteSpace2NullResult = 0b10,

            /// <summary>
            /// <see langword="null"/> 值输入先转换为空字符串再做后续操作
            /// </summary>
            Null2EmptyString = 0b100,
            /// <summary>
            /// 空白字符串先转换为空字符串再做后续操作
            /// </summary>
            WhiteSpace2EmptyResult = 0b1000,
        }
        #endregion

    }
}
