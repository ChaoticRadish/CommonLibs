using Common_Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Common_Wpf.Converter
{
    public class BrushAlphaConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入参数: [0]画刷; [1]新透明度
        /// <para>目前仅支持固定颜色的画刷</para>
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) throw new Exception("传入参数不足! ");
            Brush? brush = values[0] as Brush;
            if (brush == null) return null;
            if (values[1] is byte b) { }
            else if (values[1] is string str && byte.TryParse(str, out b)) { }
            else if (byte.TryParse(values[1].ToString(), out b)) { }
            else
            {
                throw new Exception("参数[1] 无法转换为 byte 值! ");
            }
            byte alpha = b;
            if (brush is SolidColorBrush solidColorBrush)
            {
                return new SolidColorBrush(solidColorBrush.Color.Copy(alpha));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
