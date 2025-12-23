using Common_Util.Attributes.General;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util
{
    public static class EnumHelper
    {
        /// <summary>
        /// 取得枚举值的描述, 为空时返回枚举值的名字
        /// </summary>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(enumObj))]
        public static string? GetDesc(this Enum enumObj)
        {
            if (enumObj == null) return null;
            Type type = enumObj.GetType();
            string enumName = Enum.GetName(type, enumObj)!;
            if (enumName == null) return enumObj.ToString();
            FieldInfo field = type.GetField(enumName!)!;
            Attributes.General.EnumDescAttribute? enumDesc
                = field.GetCustomAttribute<Attributes.General.EnumDescAttribute>();
            return enumDesc == null ? enumName : enumDesc.Desc;
        }

        /// <summary>
        /// 取得枚举值的描述, 为空时返回枚举值的名字
        /// </summary>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static string? GetDesc(object enumObj, Type type)
        {
            if (!type.IsEnum) return null;
            string? enumName = Enum.GetName(type, enumObj);
            if (enumName == null) return null;
            FieldInfo? field = type.GetField(enumName);
            if (field == null) return null; 
            EnumDescAttribute? enumDesc
                = field.GetCustomAttribute<EnumDescAttribute>();
            return enumDesc == null ? enumName : enumDesc.Desc;
        }

        /// <summary>
        /// 获取枚举的自定义名字
        /// </summary>
        /// <param name="enumObj"></param>
        /// <param name="getFieldName">如果没有设置自定义名字, 是否获取枚举字段名</param>
        /// <returns></returns>
        public static string? GetCustomName(this Enum enumObj, bool getFieldName = true)
        {
            if (enumObj == null) return null;
            Type type = enumObj.GetType();
            string enumName = Enum.GetName(type, enumObj)!;
            FieldInfo field = type.GetField(enumName!)!;
            Attributes.General.NameAttribute? nameAttr
                = field.GetCustomAttribute<Attributes.General.NameAttribute>();
            if (nameAttr != null)
            {
                return nameAttr.Name;
            }
            else
            {
                return getFieldName ? enumName : null;
            }
        }

        /// <summary>
        /// 遍历输入的枚举类型中的每一个枚举值
        /// </summary>
        /// <remarks>
        /// 如果枚举类型包含了多个同数值的枚举值, 遍历过程中可能会出现重复! 
        /// </remarks>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TEnum> All<TEnum>() where TEnum : Enum
        {
            foreach (var enumObj in Enum.GetValues(typeof(TEnum)))
            {
                yield return (TEnum)enumObj;
            }
        }
        /// <summary>
        /// 遍历输入的枚举类型中的每一个枚举值对应的 <see cref="FieldInfo"/>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> AllFields<TEnum>() where TEnum : Enum
        {
            Type type = typeof(TEnum);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                yield return field;
            }
        }
        /// <summary>
        /// 遍历输入的枚举类型中的每一个枚举值对应的名字
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<string> AllNames<TEnum>() where TEnum : Enum
        {
            return AllFields<TEnum>().Select(i => i.Name);
        }
        /// <summary>
        /// 遍历输入的枚举类型中的每一个枚举值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="action"></param>
        public static void ForEach<TEnum>(Action<TEnum> action) where TEnum : Enum
        {
            if (action == null) return;
            foreach (var enumObj in Enum.GetValues(typeof(TEnum)))
            {
                action?.Invoke((TEnum)enumObj);
            }
        }
        /// <summary>
        /// 遍历输入的枚举类型中的每一个枚举值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="action"></param>
        public static void ForEach(Type type, Action<object> action)
        {
            if (action == null) return;
            if (!type.IsEnum) return;
            foreach (var enumObj in Enum.GetValues(type))
            {
                action?.Invoke(enumObj);
            }
        }


        /// <summary>
        /// 将字符串转换为枚举 (优先通过枚举名, 不匹配再通过描述)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T? Convert<T>(string str)
            where T : Enum
        {
            Type type = typeof(T);
            object? result = Convert(type, str);
            return result == null ? default : (T)result;
        }
        /// <summary>
        /// 尝试将字符串转换为枚举 (优先通过枚举名, 不匹配再通过描述)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryConvert<T>(string str, out T? result)
            where T : Enum
        {
            Type type = typeof(T);
            object? resultObj = Convert(type, str);
            if (resultObj == null)
            {
                result = default;
                return false;
            }
            else
            {
                result = (T)resultObj;
                return true;
            }
        }
        /// <summary>
        /// 将字符串转换为枚举 (优先通过枚举名, 不匹配再通过描述), 转换失败时返回输入的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ConvertOrDefault<T>(string str, T defaultValue)
            where T : Enum
        {
            Type type = typeof(T);
            object? result = Convert(type, str);
            return result == null ? defaultValue : (T)result;
        }
        /// <summary>
        /// 将字符串转换为枚举 (优先通过枚举名, 不匹配再通过描述)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>null时表示转换失败</returns>
        public static object? Convert(Type type, string str)
        {
            string[] names = Enum.GetNames(type);

            List<FieldInfo> fields = new List<FieldInfo>();
            // 优先通过名字来判断
            foreach (string name in names)
            {
                FieldInfo field = type.GetField(name)!;
                fields.Add(field);
                if (name.Equals(str))
                {
                    return field.GetValue(null);
                }
            }
            // 没有匹配名字的再通过枚举来判断
            foreach (FieldInfo field in fields)
            {
                EnumDescAttribute? enumDesc = field.GetCustomAttribute<EnumDescAttribute>();
                if (enumDesc != null && enumDesc.Desc.Equals(str))
                {
                    return field.GetValue(null);
                }

            }
            // 通过数值判断
            if (Enum.GetUnderlyingType(type) == typeof(int))
            {
                if (int.TryParse(str, out int val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(uint))
            {
                if (uint.TryParse(str, out uint val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(short))
            {
                if (short.TryParse(str, out short val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(ushort))
            {
                if (ushort.TryParse(str, out ushort val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(long))
            {
                if (long.TryParse(str, out long val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(ulong))
            {
                if (long.TryParse(str, out long val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }
            else if (Enum.GetUnderlyingType(type) == typeof(byte))
            {
                if (byte.TryParse(str, out byte val))
                {
                    if (Enum.IsDefined(type, val))
                    {
                        return val;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 尝试获取目标枚举类型中, 名字与输入字符串完全匹配的项
        /// </summary>
        /// <remarks>
        /// 如果存在与找到的匹配项数值相同的枚举值, 可能会返回数值相等的其他项! 
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static bool TryGetMatchName<TEnum>(string str, [NotNullWhen(true)] out TEnum? @enum) where TEnum : Enum
        {
            foreach (var enumField in AllFields<TEnum>()) 
            {
                if (enumField.Name.Equals(str))
                {
                    @enum = (TEnum)enumField.GetValue(null)!;
                    return true;
                }
            }
            @enum = default;
            return false;
        }
    }
}
