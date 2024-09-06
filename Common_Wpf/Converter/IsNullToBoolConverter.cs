using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 判断传入值是否为null, 并相应返回true或false的多值转换器
    /// </summary>
    public class IsNullToBoolConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入值: (必填)1. 需要检查是否为null的值; (非必填)2. 如果是null, 返回什么值, 默认情况下, not null => 返回true, null => 返回false
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return false;
            object? input = values[0];

            bool nullToFalse = true;

            if (values.Length > 1)
            {
                object? v1 = values[1];
                if (v1 != null)
                {
                    var converter = TypeDescriptor.GetConverter(v1.GetType());
                    if (converter != null && converter.CanConvertTo(typeof(bool)))
                    {
                        var convertResult = converter.ConvertTo(v1, typeof(bool));
                        if (convertResult is bool b) // b: null的情况下, 需要返回的值
                        {
                            nullToFalse = b == false;
                        }
                    }
                }
            }

            if (nullToFalse)
            {
                return input != null;
            }
            else
            {
                return input == null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
