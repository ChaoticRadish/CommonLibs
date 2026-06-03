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
    /// 取一半数值的单值单向转换器
    /// </summary>
    /// <remarks>
    /// 单向是因为取一半一般会产生精度损失
    /// </remarks>
    public class HalfValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i) return i / 2;
            else if (value is uint ui) return ui / 2;
            else if (value is short s) return s / 2;
            else if (value is ushort us) return us / 2;
            else if (value is byte b) return b / 2;
            else if (value is sbyte sb) return sb / 2;
            else if (value is long l) return l / 2;
            else if (value is ulong ul) return ul / 2;
            else if (value is double db) return db / 2;
            else if (value is float f) return f / 2;
            else if (value is decimal dc) return dc / 2;
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
