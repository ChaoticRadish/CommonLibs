using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Maths
{
    /// <summary>
    /// int 类型的常用计算工具方法
    /// </summary>
    public static class IntCalcUtil
    {
        /// <summary>
        /// 已 10 为底的指数值表
        /// </summary>
        private static readonly int[] _pow10Table =
        [
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000
        ];
        /// <summary>
        /// 已 10 为底的指数允许的最大值 (受int取值范围的限制)
        /// </summary>
        public static readonly int Pow10MaxIndex = _pow10Table.Length - 1;

        /// <summary>
        /// 10 的 x 次方
        /// </summary>
        /// <param name="x">最小值: 0, 如果传入负数, 输出 0. 如果超出允许的范围, 将抛出异常</param>
        /// <returns></returns>
        public static int Pow10(int x)
        {
            if (x < 0) return 0;
            return x <= Pow10MaxIndex ? _pow10Table[x] : throw new Exception($"输入指数 {x} 超出允许的最大值 {Pow10MaxIndex}");
        }
    }
}
