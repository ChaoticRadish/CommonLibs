using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ValueTypeExtensions
    {
        /// <summary>
        /// 限制并返回输入值到区间 [left, right] 间
        /// </summary>
        /// <param name="value"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int Range(this int value, int left, int right = int.MaxValue)
        {
            if (value < left) return left;
            else if (value > right) return right;
            else return value;
        }

        /// <summary>
        /// 将double值转换为普通的字符串 (会避免转换成科学计数法)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToNormalString(this double value)
        {
            return value.ToString("0.###############"); // double值只能保存15位数(整数+小数总共15位), 所以15个'#'就够了
        }

        /// <summary>
        /// 根据bool值返回字符串
        /// </summary>
        /// <param name="b"></param>
        /// <param name="trueStr"></param>
        /// <param name="falseStr"></param>
        /// <returns></returns>
        public static string? ToString(this bool b, string? trueStr, string? falseStr = null)
        {
            return b ? trueStr : falseStr;
        }
    }
}
