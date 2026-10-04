using ChaoticKit.Data.Constraint;
using ChaoticKit.Extensions;
using ChaoticKit.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Config
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
        /// <returns>转换结果; 转换失败时退回 <see cref="object.ToString"/></returns>
        /// <remarks>
        /// 实际转换见 <see cref="TryObj2ConfigValue"/>, 本方法在转换失败时使用 <see cref="object.ToString"/> 作为回退
        /// </remarks>
        public static string? Obj2ConfigValue(object? obj)
        {
            return TryObj2ConfigValue(obj, out var value) ? value : obj?.ToString();
        }
        /// <summary>
        /// 尝试将对象转换为字符串配置值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">
        /// 转换结果; 输入为 <see langword="null"/> 时结果为 <see langword="null"/> (空配置值)
        /// </param>
        /// <returns>是否转换成功; 返回 <see langword="false"/> 时表示该对象无法表达为配置字符串</returns>
        public static bool TryObj2ConfigValue(object? obj, out string? value)
        {
            if (obj == null)
            {
                value = null;
                return true;
            }

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
            if (baseValueConverted != null)
            {
                value = baseValueConverted;
                return true;
            }

            if (obj is Type _type)
            {
                value = _type.FullName;
                return true;
            }
            if (objType.IsEnum)
            {
                Enum enumValue = (Enum)obj;
                if (Enum.IsDefined(objType, enumValue))
                    value = enumValue.ToString();
                else
                    value = enumValue.ToString("D");
                return true;
            }
            if (typeof(IEnumerable<string>).IsAssignableFrom(objType))
            {
                value = StringHelper.Concat(((IEnumerable<string>)obj).ToList(), "; ", false);
                return true;
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(objType))
            {
                value = StringHelper.Concat(((IEnumerable<int>)obj).Select(i => i.ToString()).ToList(), "; ", false);
                return true;
            }
            if (StringConveyingHelper.ToStringIfConvertible(objType, obj, out var convertResult))
            {
                value = convertResult;
                return true;
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
                value = StringHelper.Concat(valueStrings, COLLECTION_SPLIT, false);
                return true;
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
                value = StringHelper.Concat(valueStrings, COLLECTION_SPLIT, false);
                return true;
            }

            // 无法表达为配置字符串 (原先在这里退回 ToString)
            value = null;
            return false;
        }
        /// <summary>
        /// 字符串配置值转换为 <typeparamref name="T"/> 对象
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
        /// 尝试将字符串配置值转换为 <typeparamref name="T"/> 对象 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="convertResult"></param>
        /// <returns></returns>
        public static bool TryConfigValue2Obj<T>(string str, [NotNullWhen(true)] out T? convertResult)
        {
            object? obj = ConfigValue2Obj(str, typeof(T));
            if (obj == null)
            {
                convertResult = default;
                return false;
            }
            else
            {
                convertResult = (T)obj;
                return true;
            }
        }
        /// <summary>
        /// 字符串配置值转换为对象
        /// </summary>
        /// <param name="str"></param>
        /// <param name="targetType">目标类型</param>
        /// <param name="options">可选参数</param>
        /// <returns>
        /// 转换结果; 转换失败时, 可空类型与引用类型为 <see langword="null"/>, 非可空值类型为该类型的默认值
        /// </returns>
        /// <remarks>
        /// 实际转换见 <see cref="TryConfigValue2Obj(string?, Type, out object?, ConfigValue2ObjOptions?)"/>,
        /// 本方法在转换失败时按目标类型的默认值回退
        /// </remarks>
        public static object? ConfigValue2Obj(string? str, Type targetType, ConfigValue2ObjOptions? options = null)
        {
            return TryConfigValue2Obj(str, targetType, out var converted, options) ? converted : GetDefaultValue(targetType);
        }
        /// <summary>
        /// 尝试将字符串配置值转换为对象
        /// </summary>
        /// <param name="str"></param>
        /// <param name="targetType">目标类型</param>
        /// <param name="converted">
        /// 转换结果; 输入为 <see langword="null"/>, 或按 <paramref name="options"/> 判定为空值时,
        /// 结果为 <see langword="null"/> (空配置值), 此时同样返回 <see langword="true"/>
        /// </param>
        /// <param name="options">可选参数</param>
        /// <returns>是否转换成功; 返回 <see langword="false"/> 时表示该字符串无法转换为目标类型</returns>
        public static bool TryConfigValue2Obj(string? str, Type targetType, out object? converted, ConfigValue2ObjOptions? options = null)
        {
            options ??= ConfigValue2ObjOptions.Default;
            if (options.Value.EmptyValueConvertWay.HasFlag(EmptyValueConvertWays.Empty2NullResult))
            {
                if (string.IsNullOrEmpty(str))
                {
                    converted = null;
                    return true;
                }
            }
            if (options.Value.EmptyValueConvertWay.HasFlag(EmptyValueConvertWays.WhiteSpace2NullResult))
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    converted = null;
                    return true;
                }
            }



            if (str == null)
            {
                converted = null;
                return true;
            }

            Type? nullableTarget = targetType.NullableTarget();
            if (nullableTarget != null)
            {
                targetType = nullableTarget;
            }

            if (targetType == typeof(DBNull))
            {
                converted = DBNull.Value;
                return true;
            }
            else if (targetType.IsEnum)
            {
                object? temp = null;
                if (targetType.IsDefined(typeof(FlagsAttribute), false))
                    temp = EnumHelper.Convert(targetType, str, false);
                else
                    temp = EnumHelper.Convert(targetType, str, true);
                if (temp == null) goto ReturnDefault;
                else
                {
                    converted = temp;
                    return true;
                }
            }
            else if (targetType == typeof(string))
            {
                converted = str;
                return true;
            }
            else if (targetType == typeof(bool))
            {
                if (ValueHelper.TryLooselyParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(int))
            {
                if (int.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(uint))
            {
                if (uint.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(long))
            {
                if (long.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(ulong))
            {
                if (ulong.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(float))
            {
                if (float.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(double))
            {
                if (double.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(sbyte))
            {
                if (sbyte.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(byte))
            {
                if (byte.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(char))
            {
                if (char.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(short))
            {
                if (short.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(ushort))
            {
                if (ushort.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (targetType == typeof(Guid))
            {
                if (Guid.TryParse(str, out var val))
                {
                    converted = val;
                    return true;
                }
                else goto ReturnDefault;
            }
            else if (StringConveyingHelper.ToObjectIfConvertible(targetType, str, out var convertResult))
            {
                converted = convertResult;
                return true;
            }
            else if (targetType.IsEnum)
            {
                var val = EnumHelper.Convert(targetType, str);
                if (val != null)
                {
                    converted = val;
                    return true;
                }
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
                if (output != null)
                {
                    converted = output;
                    return true;
                }
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
                converted = arr;
                return true;
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
                        converted = list;
                        return true;
                    }
                }
            }

        ReturnDefault:
            converted = null;
            return false;
        }
        /// <summary>
        /// 取得转换失败时的回退值: 可空类型与引用类型为 <see langword="null"/>, 非可空值类型为该类型的默认值
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static object? GetDefaultValue(Type targetType)
        {
            if (targetType.NullableTarget() != null || !targetType.IsValueType) return null;
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
