using Common_Util.Extensions.Boolean;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Common_Wpf.Converter
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
}
