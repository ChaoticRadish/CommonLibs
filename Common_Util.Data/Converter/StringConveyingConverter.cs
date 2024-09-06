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
    /// 支持在各个 <see cref="IStringConveying"/>(需要有公共无参构造函数) 以及 <see cref="string"/> 之间互转的转换器
    /// </summary>
    /// <typeparam name="T">转换器的处理类型</typeparam>
    public class StringConveyingConverter<T> : TypeConverter
        where T : IStringConveying, new()
    {
        protected readonly static Type IStringConveyingType = typeof(IStringConveying);

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string)) { return true; } // 可以由字符串转换而来
            else if (sourceType.IsAssignableTo(IStringConveyingType)) { return true; }  // 可以由任意其他的 IStringConveying 类型转换而来
            return false;
        }
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                return StringConveyingHelper.FromString<T>(s);
            }
            else if (value is IStringConveying stringConveying)
            {
                string str = stringConveying.ConvertToString();
                return StringConveyingHelper.FromString<T>(str);
            }
            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            if (destinationType == null) return false;
            if (destinationType == typeof(string)) { return true; } // 可以转换为字符串
            else if (destinationType.IsAssignableTo(IStringConveyingType)
                && destinationType.HavePublicEmptyCtor()) { return true; }  // 可以转换为任意其他的字符类型 (需要有无参构造函数)
            return false;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value == null) return null;
            if (value is T t)
            {
                if (destinationType == typeof(string))
                {
                    return t.ConvertToString();
                }
                else if (destinationType.IsAssignableTo(IStringConveyingType)
                    && destinationType.HavePublicEmptyCtor())
                {
                    string str = t.ConvertToString();
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
