using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChaoticKit.Wpf.Converter
{
    public class DecimalPrecisionConverter : IValueConverter
    {
        /// <summary>
        /// 精度, 即保留几位小数
        /// </summary>
        public byte DecimalPlaces { get; set; } = 2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float f)
                return f.ToString($"0.{new string('#', DecimalPlaces)}") + '%';
            if (value is double d)
                return d.ToString($"0.{new string('#', DecimalPlaces)}") + '%';
            if (value is decimal dc)
                return dc.ToString($"0.{new string('#', DecimalPlaces)}") + '%';
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
