using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    public static class StringConvertHelper
    {
        private readonly static List<string> TrueStrs = new List<string>()
        {
            "是", "ture", "对", "yes", "t", "1"
        };
        private readonly static List<string> FalseStrs = new List<string>()
        {
            "否", "flase", "不对", "no", "f", "0"
        };

        /// <summary>
        /// 尝试将输入的字符串转换为布尔值
        /// </summary>
        /// <param name="str"></param>
        /// <returns>转换失败时返回null</returns>
        public static bool? ConvertBool(string str)
        {
            if (bool.TryParse(str, out bool v))
            {
                return v;
            }
            else
            {
                str = str.ToLower().Trim();
                if (TrueStrs.Contains(str))
                {
                    return true;
                }
                else if (FalseStrs.Contains(str))
                {
                    return false;
                }
            }
            return null;
        }
        /// <summary>
        /// 尝试将输入的字符串转换为指定类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object? Convert(string? str, Type targetType)
        {
            return Convert(str, targetType, out _);
        }
        /// <summary>
        /// 尝试将输入的字符串转换为指定类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="targetType"></param>
        /// <param name="isSuccess">转换是否成功</param>
        /// <returns></returns>
        public static object? Convert(string? str, Type targetType, out bool isSuccess)
        {
            if (str == null)
            {
                isSuccess = true;
                return null;
            }
            if (targetType == typeof(string))
            {
                isSuccess = true;
                return str;
            }
            else if (typeof(IStringConveying).IsAssignableFrom(targetType))
            {
                if (StringConveyingHelper.TryFromString(targetType, str, out var obj))
                {
                    isSuccess = true;
                    return obj;
                }
            }
            else if (targetType.IsEnum)
            {
                object? obj = EnumHelper.Convert(targetType, str);
                if (obj != null)
                {
                    isSuccess = true;
                    return obj;
                }
            }
            else if (targetType == typeof(bool))
            {
                if (bool.TryParse(str, out bool v))
                {
                    isSuccess = true;
                    return v;
                }
                else
                {
                    str = str.ToLower().Trim();
                    if (TrueStrs.Contains(str))
                    {
                        isSuccess = true;
                        return true;
                    }
                    else if (FalseStrs.Contains(str))
                    {
                        isSuccess = true;
                        return false;
                    }
                }
            }
            else if (targetType == typeof(char))
            {
                if (char.TryParse(str, out char v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(byte))
            {
                if (byte.TryParse(str, out byte v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(short))
            {
                if (short.TryParse(str, out short v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(ushort))
            {
                if (ushort.TryParse(str, out ushort v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(sbyte))
            {
                if (sbyte.TryParse(str, out sbyte v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(int))
            {
                if (int.TryParse(str, out int v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(uint))
            {
                if (uint.TryParse(str, out uint v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(long))
            {
                if (long.TryParse(str, out long v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(ulong))
            {
                if (ulong.TryParse(str, out ulong v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(float))
            {
                if (float.TryParse(str, out float v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(double))
            {
                if (double.TryParse(str, out double v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(str, out decimal v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(str, out DateTime v))
                {
                    isSuccess = true;
                    return v;
                }
            }
            else if (targetType.IsEnumerable() || targetType.IsList())
            {
                object? listObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType.GenericTypeArguments[0]));
                IList list = (IList)listObj!;
                if (list == null)
                {
                    throw new Exception($"创建列表实例失败! ({targetType.FullName})");
                }
                if (!string.IsNullOrEmpty(str))
                {
                    // 取得数据类型
                    Type objType = targetType.GenericTypeArguments[0];
                    IEnumerable<object?> objs = str.Split(';', '；', '|', '丨').Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)).Select(i => Convert(i, objType));
                    foreach (object? obj in objs)
                    {
                        list.Add(obj);
                    }
                }
                isSuccess = true;
                return list;
            }
            isSuccess = false;
            return null;
        }

    }
}
