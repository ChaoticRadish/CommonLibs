using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 转换器: 检查输入值是否为null, 如果为null
    /// <para>注: 默认情况下, 值为 <see cref="DependencyProperty.UnsetValue"/> 则同样视为 null, 可取消此设定</para>
    /// <para>单值转换器规则: </para>
    /// <para>· 如果传入值为null, 则返回 <see cref="DefaultValue"/> </para>
    /// <para>多值转换器规则: 如果传入的第一个值为 null, 则返回传入的第二个值, 即将第二个值视为默认值</para>
    /// <para>· 如果没有传入如何值, 则返回null</para>
    /// <para>· 如果只传入了一个值, 那么无论是否null都会将其返回</para>
    /// <para>· 如果传入超过两个值, 第三个值及后面的其他值都将被忽略</para>
    /// </summary>
    public class IfNullThenDefaultConverter : DependencyObject, IMultiValueConverter, IValueConverter
    {
        /// <summary>
        /// 默认值, 在多值转换器中, 优先度较低
        /// </summary>
        public object? DefaultValue
        {
            get { return (object?)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(object), typeof(IfNullThenDefaultConverter), new PropertyMetadata(null));



        /// <summary>
        /// <see cref="DependencyProperty.UnsetValue"/> 是否视为 null
        /// </summary>
        public bool UnsetValueIsNull
        {
            get { return (bool)GetValue(UnsetValueIsNullProperty); }
            set { SetValue(UnsetValueIsNullProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnsetValueIsNull.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnsetValueIsNullProperty =
            DependencyProperty.Register("UnsetValueIsNull", typeof(bool), typeof(IfNullThenDefaultConverter), new PropertyMetadata(true));



        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            switch (values.Length)
            {
                case 0:
                    return _getNull(null);
                case 1:
                    return _getReturn(values[0], DefaultValue);
                default:
                    return _getReturn(values[0], values[1] ?? DefaultValue);
            }
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _getReturn(value, DefaultValue);
        }

        private object? _getReturn(object? input, object? defaultValue)
        { 
            if (input == null)
            {
                return defaultValue ?? _getNull(input);
            }
            else if (UnsetValueIsNull && input == DependencyProperty.UnsetValue)
            {
                return defaultValue ?? _getNull(input);
            }
            return input;
        }
        private object? _getNull(object? input)
        {
            if (input == null) { return null; }
            if (UnsetValueIsNull && input == DependencyProperty.UnsetValue) { return input; }
            
            return null;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
