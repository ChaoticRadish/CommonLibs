using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Maui.Converter
{
    /// <summary>
    /// 判断输入值是否全为 true 的转换器
    /// <para>如果没有任何输入, 则返回 true </para>
    /// <para>如果有输入, 且任意值不为 bool 类型或不为 true 值, 则返回 false</para>
    /// </summary>
    public class AllTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return true;
            foreach (object value in values)
            {
                if (value is bool b && b)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
