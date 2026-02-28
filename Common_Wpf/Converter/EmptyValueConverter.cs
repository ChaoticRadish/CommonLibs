using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using Common_Util.Extensions;

namespace Common_Wpf.Converter
{
    public abstract class EmptyValueConverterBase<T>(T emptyTo, T notEmptyTo) : IValueConverter
    {
        public T EmptyTo { get; set; } = emptyTo;
        public T NotEmptyTo { get; set; } = notEmptyTo;

        /// <summary>
        /// 判断转换器的输入值是否为空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(object? value) 
        {
            if (value == null) return true;
            if (value is string str)
            {
                return str.IsEmpty();
            }
            return false;
        }

        public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsEmpty(value) ? EmptyTo : NotEmptyTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 判断一个值是否为空, 根据是否为空转换为 <see cref="Visibility"/> 的转换器
    /// </summary>
    /// <remarks>
    /// 默认为: <br/>
    /// 空 => <see cref="Visibility.Collapsed"/> <br/>
    /// 非空 => <see cref="Visibility.Visible"/>
    /// </remarks>
    public class EmptyToVisibilityConverter() : EmptyValueConverterBase<Visibility>(Visibility.Collapsed, Visibility.Visible)
    {
    }

    /// <summary>
    /// 判断一个值是否为空的转换器
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> => <see langword="true"/> <br/>
    /// 空值 (如 <see cref="string.Empty"/>) => <see langword="true"/>
    /// </remarks>
    public class IsEmptyConverter() : EmptyValueConverterBase<bool>(true, false)
    {
    }


    /// <summary>
    /// 判断一个值是否不为空的转换器
    /// <see langword="null"/> => <see langword="false"/> <br/>
    /// 空值 (如 <see cref="string.Empty"/>) => <see langword="false"/>
    /// </summary>
    public class IsNotEmptyConverter() : EmptyValueConverterBase<bool>(false, true)
    {
    }
}
