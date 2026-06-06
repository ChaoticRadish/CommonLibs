using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Maths
{
    public static class MathHelper
    {
        #region 模运算
        /// <summary>
        /// 计算数学意义上的模运算。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="modulus"></param>
        /// <returns>始终在 [0, <paramref name="modulus"/>) 范围内，支持负数输入。</returns>
        public static int Modulo(int value, int modulus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            return ModuleUnchecked(value, modulus);
        }
        /// <summary>
        /// 计算数学意义上的模运算。
        /// </summary>
        /// <returns>始终在 [0, <paramref name="modulus"/>) 范围内，支持负数输入。</returns>
        public static int ModuleUnchecked(int value, int modulus)
        {
            int r = value % modulus;
            return r < 0 ? r + modulus : r;
        }
        /// <summary>
        /// 计算两个整数的和并对指定值取模。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public static int AddModulo(int a, int b, int modulus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            return AddModuloUnchecked(ModuleUnchecked(a, modulus), ModuleUnchecked(b, modulus), modulus);
        }
        /// <summary>
        /// 计算两个整数的和并对指定值取模。
        /// </summary>
        /// <remarks>
        /// 不包含输入检查，需确保输入值 <paramref name="a"/> 和 <paramref name="b"/> 均在 [0, <paramref name="modulus"/>) 范围内。
        /// </remarks>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public static int AddModuloUnchecked(int a, int b, int modulus)
        {
            int temp = modulus - b;
            if (a >= temp)
                return a - temp;
            else return a + b;
        }


        #endregion

        /// <summary>
        /// 将输入的值压缩至[0, 1], 值越大, 输出越接近0
        /// </summary>
        /// <param name="value"></param>
        /// <param name="baseValue">底数</param>
        /// <returns></returns>
        public static float Zip(float value, float baseValue = 2)
        {
            if (baseValue == 0)
            {
                return 0;
            }
            else
            {
                return (float)Math.Pow(1 / baseValue, value);
            }
        }
        #region 最小值
        /// <summary>
        /// 返回 <paramref name="inputs"/> 中的最小值, 如果 <paramref name="inputs"/> 是空的, 则返回 0
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static float Min(params float[] inputs)
        {
            float output = 0;
            if (inputs != null && inputs.Length > 0)
            {
                output = inputs[0];
                foreach (float input in inputs)
                {
                    if (output > input)
                    {
                        output = input;
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// 返回 <paramref name="inputs"/> 中的最小值, 如果 <paramref name="inputs"/> 是空的, 则返回 0
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static uint Min(params uint[] inputs)
        {
            uint output = 0;
            if (inputs != null && inputs.Length > 0)
            {
                output = inputs[0];
                foreach (uint input in inputs)
                {
                    if (output > input)
                    {
                        output = input;
                    }
                }
            }
            return output;

        }

        #endregion
        #region 最大值

        /// <summary>
        /// 返回 <paramref name="inputs"/> 中的最大值, 如果 <paramref name="inputs"/> 是空的, 则返回 0
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static uint Max(params uint[] inputs)
        {
            uint output = 0;
            if (inputs != null && inputs.Length > 0)
            {
                output = inputs[0];
                foreach (uint input in inputs)
                {
                    if (output < input)
                    {
                        output = input;
                    }
                }
            }
            return output;

        }
        #endregion
    }
}
