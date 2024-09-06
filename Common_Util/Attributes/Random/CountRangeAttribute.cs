using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 数量区间
    /// </summary>
    public class CountRangeAttribute : Attribute
    {
        /// <summary>
        /// 范围区间: [min, max) 输入值会钳制为正数
        /// </summary>
        /// <param name="count"></param>
        public CountRangeAttribute(int min, int max)
        {
            min = min >= 0 ? min : 0;
            max = max >= 0 ? max : 0;
            if (max < min)
            {
                (max, min) = (min, max);
            }
            Min = min;
            Max = max;
        }
        /// <summary>
        /// 范围区间: [0, count)
        /// </summary>
        /// <param name="count"></param>
        public CountRangeAttribute(int count) : this(0, count) { }

        public int Min { get; private set; }
        public int Max { get; private set; }
    }
}
