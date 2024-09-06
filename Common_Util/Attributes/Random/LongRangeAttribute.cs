using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 区间
    /// </summary>
    public class LongRangeAttribute : Attribute
    {
        public LongRangeAttribute(long min, long max)
        {
            if (max < min)
            {
                long temp = min;
                min = max;
                max = temp;
            }
            Min = min;
            Max = max;
        }
        public LongRangeAttribute(long length) : this(0, length) { }

        public long Min { get; private set; }
        public long Max { get; private set; }
    }
}
