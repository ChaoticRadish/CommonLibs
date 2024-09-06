using Common_Util.Data.Structure.Value;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.AccessControl;

namespace Common_Util.Data.Converter
{
    public class DecFixedPointNumberTypeConverter : StringConveyingConverter<DecFixedPointNumber>
    {
        private static readonly Type[] _allowNumberTypes =
        [
            typeof(int),
            //typeof(long), 暂未实现
            typeof(float),
            typeof(double), 
        ];
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (_allowNumberTypes.Contains(sourceType))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is int i)
            {
                return (DecFixedPointNumber)i;
            }
            else if (value is float f)
            {
                return (DecFixedPointNumber)f;
            }
            else if (value is double d)
            {
                return (DecFixedPointNumber)d;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            if (_allowNumberTypes.Contains(destinationType))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is DecFixedPointNumber num)
            {
                if (destinationType == typeof(int))
                {
                    return (int)num;
                }
                else if (destinationType == typeof(float))
                {
                    return (float)num;
                }
                else if (destinationType == typeof(double))
                {
                    return (double)num;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
