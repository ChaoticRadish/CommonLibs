using Common_Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 调用帮助方法 <see cref="EnumHelper.GetDesc(Enum)"/>, 将传入枚举值转换为字符串
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EnumHelper.GetDesc((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
