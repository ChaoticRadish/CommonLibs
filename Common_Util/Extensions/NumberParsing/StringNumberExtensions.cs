using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.NumberParsing
{
    /// <summary>
    /// 包含字符串与数值互相转换的扩展方法
    /// </summary>
    public static class StringNumberExtensions
    {
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


        /// <summary>
        /// 将文本转换为int值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue">转换失败时使用的默认值</param>
        /// <returns></returns>
        public static int ToInt(this string? input, int defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (int.TryParse(input, out int output))
                {
                    return output;
                }
            }
            return defaultValue;
        }
    }
}
