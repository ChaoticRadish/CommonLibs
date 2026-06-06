using ChaoticKit;
using ChaoticKit.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChaoticKit.Wpf.Converter
{
    /// <summary>
    /// 调用帮助方法 <see cref="EnumHelper.GetDesc(Enum)"/>, 将传入枚举值转换为字符串
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumDescConverter : IValueConverter
    {
        /// <summary>
        /// 输入 <see langword="null"/> 值时的描述内容, 如果是空值, 则不生效
        /// </summary>
        public string? NullValueDesc { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum @enum)
                return EnumHelper.GetDesc(@enum);
            else 
            {
                if (value is null && NullValueDesc.IsNotEmpty())
                {
                    return NullValueDesc;
                }
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
