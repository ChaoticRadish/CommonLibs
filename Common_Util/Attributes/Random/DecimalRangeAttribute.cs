using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 区间
    /// </summary>
    public class DecimalRangeAttribute : Attribute
    {
        public DecimalRangeAttribute(string min, string max)
        {
            if (!decimal.TryParse(min, out decimal dc_min)) dc_min = 0;
            if (!decimal.TryParse(max, out decimal dc_max)) dc_max = 0;
            if (dc_max < dc_min)
            {
                (dc_max, dc_min) = (dc_min, dc_max);
            }
            Min = dc_min;
            Max = dc_max;
        }
        public DecimalRangeAttribute(double min, double max)
        {
            decimal dc_min = (decimal)min;
            decimal dc_max = (decimal)max;
            if (dc_max < dc_min)
            {
                (dc_max, dc_min) = (dc_min, dc_max);
            }
            Min = dc_min;
            Max = dc_max;
        }
        public DecimalRangeAttribute(double length) : this(0, length) { }

        public decimal Min { get; private set; }
        public decimal Max { get; private set; }
    }
}
