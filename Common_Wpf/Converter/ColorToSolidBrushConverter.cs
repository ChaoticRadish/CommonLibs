using Common_Util.Data.Structure.Pair;
using Common_Util.Extensions;
using Common_Util.Extensions.NumberParsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Common_Wpf.Converter
{
    public abstract class ColorToSolidBrushConverterBase
    {
        protected Color? ConvertColor(object? obj)
        {
            if (obj is RgbaColorB color)
                return Color.FromArgb(color.A, color.R, color.G, color.B);
            return null;
        }
    }
    public class ColorToSolidBrushConverter : ColorToSolidBrushConverterBase, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = ConvertColor(value);
            if (color != null) return new SolidColorBrush(color.Value);
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ColorOpacityToSolidBrushConverter : ColorToSolidBrushConverterBase, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var color = ConvertColor(values.GetOrDefault(0));
            if (color == null) 
            {
                return values.GetOrDefault(2, DependencyProperty.UnsetValue);
            }
            var opacityObj = values.GetOrDefault(1, 1d);
            var opacity = opacityObj.AsDouble(1d);
            return new SolidColorBrush(color.Value)
            {
                Opacity = opacity,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
