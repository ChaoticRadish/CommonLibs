using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using Common_Util.Extensions.Boolean;

namespace Common_Wpf.Converter
{
    public class BoolToVisibilityConverterBase
    {
        public BoolToVisibilityConverterBase()
            : this(true)
        {

        }
        public BoolToVisibilityConverterBase(bool collapsewhenInvisible)
            : base()
        {
            CollapseWhenInvisible = collapsewhenInvisible;
        }

        /// <summary>
        /// 不可见时使用 <see cref="Visibility.Collapsed"/>
        /// </summary>
        public bool CollapseWhenInvisible { get; set; }

        /// <summary>
        /// false 时使用的 <see cref="Visibility"/> 值
        /// </summary>
        public Visibility FalseVisibility
        {
            get
            {
                if (CollapseWhenInvisible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }

        }

        protected static bool Convert(object obj)
        {
            return obj.AsBool();
        }
    }

    /// <summary>
    /// 将 <see langword="bool"/> 根据值转换为 <see cref="Visibility"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : BoolToVisibilityConverterBase, IValueConverter
    {
        public BoolToVisibilityConverter()
            : base(true)
        {
        }
        public BoolToVisibilityConverter(bool collapsewhenInvisible)
            : base(collapsewhenInvisible)
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = Convert(value);
            return b ? Visibility.Visible : FalseVisibility;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return ((Visibility)value == Visibility.Visible);
        }
    }

    /// <summary>
    /// 判断所有 <see langword="bool"/> 是否均为 <see langword="true"/> 转换为 <see cref="Visibility"/>
    /// </summary>
    public class MultiAndBoolToVisibilityConverter : BoolToVisibilityConverterBase, IMultiValueConverter
    {
        public MultiAndBoolToVisibilityConverter()
            : base(true)
        {
        }
        public MultiAndBoolToVisibilityConverter(bool collapsewhenInvisible)
            : base(collapsewhenInvisible)
        {
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return Visibility.Visible;
            bool b = values.All(Convert);
            return b ? Visibility.Visible : FalseVisibility;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"不支持将 {nameof(Visibility)} 转换为 {nameof(Boolean)} 数组");
        }
    }
    public class MultiOrBoolToVisibilityConverter : BoolToVisibilityConverterBase, IMultiValueConverter
    {
        public MultiOrBoolToVisibilityConverter()
            : base(true)
        {
        }
        public MultiOrBoolToVisibilityConverter(bool collapsewhenInvisible)
            : base(collapsewhenInvisible)
        {
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return Visibility.Visible;
            bool b = values.Any(Convert);
            return b ? Visibility.Visible : FalseVisibility;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"不支持将 {nameof(Visibility)} 转换为 {nameof(Boolean)} 数组");
        }
    }

}
