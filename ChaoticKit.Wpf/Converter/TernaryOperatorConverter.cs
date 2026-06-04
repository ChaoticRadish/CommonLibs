using ChaoticKit.Extensions;
using ChaoticKit.Extensions.Boolean;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ChaoticKit.Wpf.Converter
{
    public class TernaryOperatorConverter : MarkupExtension, IValueConverter
    {
        public object? TrueValue { get; set; }

        public object? FalseValue { get; set; }

        private static TernaryOperatorConverter Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }
        private static TernaryOperatorConverter? _instance;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return TrueValue == null && FalseValue == null ? Instance : this;
        }
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool condition = value.AsBool();

                return condition ? TrueValue : FalseValue;
            }
            catch
            {
                return FalseValue; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 三目运算的多值绑定的转换器
    /// </summary>
    /// <remarks>
    /// 值[0] 判断条件 <br/>
    /// 值[1] <see langword="true"/> 值对应的值 <br/>
    /// 值[2] <see langword="false"/> 值对应的值 <br/>
    /// </remarks>
    public class TernaryOperatorMultiConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return DependencyProperty.UnsetValue;

            var condition = values.GetOrDefault(0).AsBool();
            var trueValue = values.GetOrDefault(1);
            var falseValue = values.GetOrDefault(2);
            return condition ? trueValue : falseValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
