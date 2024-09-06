using Common_Wpf.SettingPackage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Converter
{
    public class BaseBoxSettingCollectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(BaseBoxSettingCollection)
                || sourceType == typeof(BaseBoxSetting[]);
        }
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            BaseBoxSettingCollection? collection = null;
            if (value is BaseBoxSettingCollection _c)
            {
                collection = _c;
            }
            if (value is BaseBoxSetting[] _a)
            {
                collection = _a;
            }
            return collection;
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            return destinationType == typeof(BaseBoxSettingCollection)
                || destinationType == typeof(BaseBoxSetting[]);
        }
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            BaseBoxSettingCollection? collection = null;
            if (value is BaseBoxSettingCollection _c)
            {
                collection = _c;
            }
            if (value is BaseBoxSetting[] _a)
            {
                collection = _a;
            }

            if (destinationType == typeof(BaseBoxSettingCollection))
            {
                return collection;
            }
            else if (destinationType == typeof(BaseBoxSetting[]))
            {
                return collection?.Array;
            }
            else
            {
                return null;
            }
        }
    }
}
