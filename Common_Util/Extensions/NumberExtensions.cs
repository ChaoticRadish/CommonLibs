using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// 如果浮点数的值比检查值小, 则返回null
        /// </summary>
        /// <param name="f"></param>
        /// <param name="checkValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float? IfSmallerThenNull(this float f, float checkValue)
        {
            return f >= checkValue ? f : null;
        }

        /// <summary>
        /// 如果int的值比检查值小, 则返回null
        /// </summary>
        /// <param name="i"></param>
        /// <param name="checkValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? IfSmallerThenNull(this int i, int checkValue)
        {
            return i >= checkValue ? i : null;
        }

        /// <summary>
        /// 如果浮点数的值不大于检查值, 则返回null
        /// </summary>
        /// <param name="f"></param>
        /// <param name="checkValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float? IfNotBiggerThenNull(this float f, float checkValue)
        {
            return f > checkValue ? f : null;
        }

        /// <summary>
        /// 如果int的值不大于检查值, 则返回null
        /// </summary>
        /// <param name="i"></param>
        /// <param name="checkValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? IfNotBiggerThenNull(this int i, int checkValue)
        {
            return i > checkValue ? i : null;
        }
    }
}
