using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Common_Util.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 获取原字符串重复指定次数之后的字符串
        /// </summary>
        /// <param name="origin">需要被重复的字符串</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Repeat(this string origin, int count)
        {
            if (count <= 0) return string.Empty;
            StringBuilder stringBuilder= new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                stringBuilder.Append(origin);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 如果字符串长度大于输入值, 将其截取到输入值的长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string? Limit(this string? str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (str.Length > maxLength)
            {
                return str[..maxLength];
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 省略字符串超过输入长度的部分, 最终长度(包含省略字符)不会超过输入长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <param name="ellipsis">省略字符</param>
        /// <returns></returns>
        public static string? Brief(this string? str, int maxLength = 50, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (maxLength > ellipsis.Length + 1 && str.Length > maxLength)
            {
                return string.Concat(str.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 判断字符串是否为null或空字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty([NotNullWhen(false)] this string? value)
        {
            return string.IsNullOrEmpty(value);
        }
        /// <summary>
        /// 判断字符串是否不为null或空字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty([NotNullWhen(true)] this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 检查字符串是不是null或者空字符串, 如果是则不通过, 返回false
        /// </summary>
        /// <param name="value"></param>
        /// <param name="throwException">在字符串null或者空字符串时, 是否抛出异常</param>
        /// <param name="varName">变量名</param>
        /// <returns>检查通过时返回true</returns>
        public static bool CheckEmpty([NotNullWhen(true)] this string? value, bool throwException = false, string? varName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (throwException)
                {
                    string exStr = string.IsNullOrEmpty(varName) ? "字符串值是空的" : $"字符串变量 {varName} 的值是空的";
                    throw new ArgumentException(exStr, varName);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 当当前字符串为null或空值时, 返回指定的默认值, 否则返回原值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string WhenEmptyDefault(this string? str, string defaultValue)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }
            else
            {
                return str;
            }
        }
        /// <summary>
        /// 当当前字符串为null或空值或仅由空白字符组成时, 返回指定的默认值, 否则返回原值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string WhenWhiteSpaceDefault(this string? str, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultValue;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 将以16进制格式表示的字符串转换为byte[] (会移除字符串中的空格)
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToHexByte(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                hexString += " ";
            }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        #region
        /// <summary>
        /// 过滤 null 值
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static IEnumerable<string> FilterNull(this IEnumerable<string?> strs)
        {
            foreach (string? str in strs)
            {
                if (str != null)
                {
                    yield return str;
                }
            }
        }
        #endregion

        #region IP地址判断
        /// <summary>
        /// 检查是否 IPv4 的正则表达式
        /// </summary>
        public const string IPV4_REGEX_STR = @"(?:(?:1[0-9][0-9]\\.)|(?:2[0-4][0-9]\\.)|(?:25[0-5]\\.)|(?:[1-9][0-9]\\.)|(?:[0-9]\\.)){3}(?:(?:1[0-9][0-9])|(?:2[0-4][0-9])|(?:25[0-5])|(?:[1-9][0-9])|(?:[0-9]))";
        public static Regex IPv4Regex { get; private set; } = new System.Text.RegularExpressions.Regex(IPV4_REGEX_STR);

        /// <summary>
        /// 检查是否 IPv6 的正则表达式
        /// </summary>
        public const string IPV6_REGEX_STR = @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";
        public static Regex IPv6Regex { get; private set; } = new System.Text.RegularExpressions.Regex(IPV6_REGEX_STR);

        /// <summary>
        /// 判断字符串是否IPv4字符串
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPv4([NotNullWhen(true)] this string? ip)
        {
            if (ip == null) return false;
            return IPv4Regex.Match(ip).Success;
        }
        /// <summary>
        /// 判断字符串是否IPv6字符串
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPv6([NotNullWhen(true)] this string? ip)
        {
            if (ip == null) return false;
            return IPv6Regex.Match(ip).Success;
        }
        #endregion

        #region 数值
        private static readonly string _format_fixed_point = "0." + "#".Repeat(339);
        /// <summary>
        /// 使用非科学计数法将一个 float 转换为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NoScientificNotationString(this float value)
        {
            return value.ToString(_format_fixed_point);
        }
        /// <summary>
        /// 使用非科学计数法将一个 double 转换为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NoScientificNotationString(this double value)
        {
            return value.ToString(_format_fixed_point);
        }
        #endregion
    }
}
