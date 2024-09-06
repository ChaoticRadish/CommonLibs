using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Maui.Converter
{
    /// <summary>
    /// 判断输入值是否存在任意一项为 true
    /// <para>如果没有任何输入, 则返回 false </para>
    /// <para>如果有输入, 不存在任意输入值是 bool 类型且值为 true, 则返回 false</para>
    /// </summary>
    public class AnyTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return false;
            foreach (object value in values)
            {
                if (value is bool b && b)
                {
                    return true;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
