using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class DateTimeExtensions
    {
        public static string DefaultTimeStringFormat { set; get; } = "yyyy-MM-dd HH:mm:ss:fff";

        public static string ToStringEx(this DateTime d, string defaultOrZeroValue = " -- -- -- -- -- -- ")
        {
            if (d == DateTime.MinValue) return defaultOrZeroValue;
            return d.ToString(DefaultTimeStringFormat);
        }
        public static string ToStringEx(this DateTime? d, string defaultOrZeroValue = " -- -- -- -- -- -- ")
        {
            if (d == null || d.Value == DateTime.MinValue) return defaultOrZeroValue;
            return d.Value.ToString(DefaultTimeStringFormat);
        }

    }
}
