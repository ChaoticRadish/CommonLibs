using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Converter
{
    /// <summary>
    /// 支持在各个 <see cref="IStringConveying{TSelf}"/> 以及 <see cref="string"/> 之间互转的转换器
    /// </summary>
    /// <typeparam name="T">转换器的处理类型</typeparam>
    public class StringConveyingConverter<T> : TypeConverter
        where T : IStringConveying<T>, new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string)) { return true; } // 可以由字符串转换而来
            else if (StringConveyingHelper.ConvertibleCheck(sourceType)) { return true; }  // 可以由任意其他的 IStringConveying 类型转换而来
            return false;
        }
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                return (T)s;
            }
            else if (StringConveyingHelper.ToStringIfConvertible(value.GetType(), value, out var convertResult))
            {
                return (T)convertResult;
            }
            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            if (destinationType == null) return false;
            if (destinationType == typeof(string)) { return true; } // 可以转换为字符串
            else if (StringConveyingHelper.ConvertibleCheck(destinationType)) { return true; }  // 可以转换为任意其他的字符类型
            return false;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value == null) return null;
            if (value is T t)
            {
                if (destinationType == typeof(string))
                {
                    return (string)t;
                }
                else if (StringConveyingHelper.ConvertibleCheck(destinationType))
                {
                    string str = (string)t;
                    return StringConveyingHelper.FromString(destinationType, str);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
