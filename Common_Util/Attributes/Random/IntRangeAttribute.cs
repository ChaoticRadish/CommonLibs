using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 区间
    /// </summary>
    public class IntRangeAttribute : Attribute
    {
        public IntRangeAttribute(int min, int max)
        {
            if (max < min)
            {
                int temp = min;
                min = max;
                max = temp;
            }
            Min = min;
            Max = max;
        }
        public IntRangeAttribute(int length) : this(0, length) { }

        public int Min { get; private set; }
        public int Max { get; private set; }
    }
}
