using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Maui.Converter
{
    public class IsEmptyStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return true;
            else if (value is string s)
            {
                return s.IsEmpty();
            }
            else
            {
                throw new NotSupportedException($"不支持的类型: {value.GetType()}");
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
