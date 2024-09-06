using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 用于判断对象是否可以赋值到目标类型的转换器
    /// <para>传入对象可以被赋值到指定类型时, 返回true, 反之返回false</para>
    /// <para>传入对象或类型为null时, 返回false</para>
    /// </summary>
    public class IsAssignableToConverter : IMultiValueConverter
    {
        /// <summary>
        /// 传入参数: [0]:需要检查的对象, [1]:目标类型
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1) throw new Exception("输入参数不足! ");
            Type? objType = values[0]?.GetType();
            Type? inputType = values.Length >= 2 && values[1] is Type type ? type : null;
            if (objType == null || inputType == null) return false;
            return objType.IsAssignableTo(inputType);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
