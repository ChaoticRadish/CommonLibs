using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Common_Wpf.Converter
{

    /// <summary>
    /// 判断一个值是否为空的转换器
    /// <para>null => true</para>
    /// <para>空值(如string.Empty) => true</para>
    /// </summary>
    public class IsEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) 
                return true;
            if (value is string str) 
            {
                return str.IsEmpty();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
