using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 根据一个对象是否为 null, 转换为不可见状态: <see cref="Visibility.Collapsed"/> 或 <see cref="Visibility.Hidden"/>
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public NullToVisibilityConverter()
            : this(true)
        {

        }
        public NullToVisibilityConverter(bool collapsewhenInvisible)
            : base()
        {
            CollapseWhenInvisible = collapsewhenInvisible;
        }
        /// <summary>
        /// 不可见时使用 <see cref="Visibility.Collapsed"/>
        /// </summary>
        public bool CollapseWhenInvisible { get; set; }



        /// <summary>
        /// null 时使用的 <see cref="Visibility"/> 值
        /// </summary>
        public Visibility NullVisibility
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


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullVisibility : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
