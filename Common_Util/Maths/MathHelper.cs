using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Maths
{
    public static class MathHelper
    {
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
    }
}
