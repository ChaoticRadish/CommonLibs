using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common_Wpf.Converter
{

    public class DateTimeToTextConverter : IValueConverter
    {
        public DateTimeToTextConverter() { }

        public static string DefaultFormat { get; } = "yyyy-MM-dd HH:mm:ss";
        public string Format 
        {
            get;
            set;
        } = DefaultFormat;

        /// <summary>
        /// 当值为 <see langword="null"/> 时返回的文本字符串。
        /// </summary>
        public string TextWhenNull { get; } = string.Empty;

        /// <summary>
        /// 当值不是 <see cref="DateTime"/> 类型时返回的文本字符串。
        /// </summary>
        /// <remarks>
        /// 如果为 <see langword="null"/>，则使用值的字符串表示形式。
        /// </remarks>
        public string? TextIfNotDateTime { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return TextWhenNull;
            if (value is DateTime dt) return dt.ToString(Format);
            return TextIfNotDateTime.WhenEmptyDefault(value?.ToString() ?? string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
