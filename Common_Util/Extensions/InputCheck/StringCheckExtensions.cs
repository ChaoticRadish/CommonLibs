using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.InputCheck
{
    public static class StringCheckExtensions
    {
        /// <summary>
        /// 判断 <paramref name="input"/> 的长度是否在 [<paramref name="min"/>, <paramref name="max"/>] 
        /// </summary>
        /// <remarks>
        /// 当 <paramref name="input"/> == <see langword="null"/> 时, 返回 <see langword="false"/>
        /// </remarks>
        /// <param name="input"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsLengthIn(this string? input, int min, int max)
        {
            if (input == null) return false;
            return input.Length >= min && input.Length <= max;
        }
    }
}
