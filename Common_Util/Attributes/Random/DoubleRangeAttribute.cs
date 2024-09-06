using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 区间
    /// </summary>
    public class DoubleRangeAttribute : Attribute
    {
        public DoubleRangeAttribute(double min, double max)
        {
            if (max < min)
            {
                double temp = min;
                min = max;
                max = temp;
            }
            Min = min;
            Max = max;
        }
        public DoubleRangeAttribute(double length) : this(0, length) { }

        public double Min { get; private set; }
        public double Max { get; private set; }
    }
}
