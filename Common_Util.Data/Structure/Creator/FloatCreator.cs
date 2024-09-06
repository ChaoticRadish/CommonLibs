using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Creator
{
    /// <summary>
    /// float值的生成器
    /// </summary>
    public class FloatCreator
    {
        public static FloatCreator Creator { get; } = new FloatCreator();

        /// <summary>
        /// 最大值
        /// </summary>
        protected float max;
        /// <summary>
        /// 最小值
        /// </summary>
        protected float min;
        /// <summary>
        /// 取值宽度 (包含最大最小值)
        /// </summary>
        protected double width;

        /// <summary>
        /// 上一个数字
        /// </summary>
        public float LastNumber
        {
            get
            {
                return lastNumber;
            }
            set
            {
                if (value <= max && value >= min)
                {
                    lastNumber = value;
                }
            }
        }
        private float lastNumber;

        public FloatCreator() : this(float.MaxValue, float.MinValue)
        {

        }
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="min">允许的最小值</param>
        /// <param name="max">允许的最大值</param>
        public FloatCreator(float min, float max)
        {
            if (min == max)
            {
                if (min == int.MinValue)
                {
                    max++;
                }
                else if (min == int.MaxValue)
                {
                    min--;
                }
                else
                {
                    max++;
                }
            }
            if (min > max)
            {
                lastNumber = max;
                max = min;
                min = lastNumber;
            }
            this.min = min;
            this.max = max;
            this.width = (double)max - min;
            this.lastNumber = min;
        }

        /// <summary>
        /// 将上一次的数字增加, 并返回增加后的数值 (如果增加后超出了范围, 将返回最小值处(输入正)或最大值处(输入负))
        /// </summary>
        /// <param name="value">要增加的量</param>
        /// <returns></returns>
        public float AddReturnNew(float value = 1)
        {
            #region 草稿
            // 假设取值宽度为3, LastNumber == max
            // 初始 ..v
            // 设 value = 5  | 设 value = -5
            // 加1  v..      | 减1  .v.
            // 加1  .v.      | 减1  v..
            // 加1  ..v      | 减1  ..v
            // 加1  v..      | 减1  .v.
            // 加1  .v.      | 减1  v..
            // 实际上移动2个位置, 即移动 value % width;
            #endregion
            if (value == 0)
            {
                // 什么都不做
            }
            else
            {
                // 取余
                value = (float)(value % width);
                if (value > 0)
                {
                    float dist = max - lastNumber;
                    if (dist < value)
                    { // 增加后会超出值域
                        lastNumber = min + (value - dist);
                    }
                    else
                    { // 不会超出, 直接增加
                        lastNumber += value;
                    }
                }
                else
                {
                    value *= -1;    // 值由负转为正
                    float dist = lastNumber - min;
                    if (dist < value)
                    { // 增加后会超出值域
                        lastNumber = max - (value - dist);
                    }
                    else
                    { // 不会超出, 直接减少
                        lastNumber -= value;
                    }
                }
            }
            return lastNumber;
        }
    }
}
