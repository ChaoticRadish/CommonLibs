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
    public abstract class ExtremeValueConverterBase<T> : IMultiValueConverter
    {
        /// <summary>
        /// 缺省值, 没有输入有效值时取此值
        /// </summary>
        /// <remarks>
        /// 如果为 <see langword="null"/>, 则在没有输入有效值时取 <see cref="DependencyProperty.UnsetValue"/>
        /// </remarks>
        public T? DefaultValue { get; set; }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class MinDoubleConverter : ExtremeValueConverterBase<double?>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValues = values.Where(v => v is double).Select(v => (double)v).ToArray();
            if (doubleValues.Length == 0) 
                return DefaultValue ?? DependencyProperty.UnsetValue;
            else return doubleValues.Min();

        }
    }
    public class MaxDoubleConverter : ExtremeValueConverterBase<double?>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValues = values.Where(v => v is double).Select(v => (double)v).ToArray();
            if (doubleValues.Length == 0)
                return DefaultValue ?? DependencyProperty.UnsetValue;
            else return doubleValues.Max();

        }
    }
}
