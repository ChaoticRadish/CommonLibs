using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 数字与字符串互相转换的转换器
    /// </summary>
    public class StringToNumberConverter : IValueConverter
    {
        public StringToNumberConverter() : this(false) { }
        public StringToNumberConverter(bool throwExceptionWhenParseFailure)
        {
            ThrowExceptionWhenParseFailure = throwExceptionWhenParseFailure;
        }
        /// <summary>
        /// 转换失败时是否抛出异常
        /// </summary>
        public bool ThrowExceptionWhenParseFailure { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convert(value, targetType);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convert(value, targetType);
        }

        public object? _convert(object value, Type targetType)
        {
            if (targetType == typeof(string)) 
            {
                return value.ToString() ?? string.Empty;
            }
            else
            {
                string str;
                if (value is string s)
                {
                    str = s;
                }
                else
                {
                    str = value.ToString() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(str)) 
                {
                    return Common_Util.ValueHelper.DefaultValue(targetType);
                }
                else if (targetType == typeof(int))
                {
                    if (int.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(uint))
                {
                    if (uint.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(short))
                {
                    if (short.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(ushort))
                {
                    if (ushort.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(long))
                {
                    if (long.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(ulong))
                {
                    if (ulong.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(byte))
                {
                    if (byte.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(sbyte))
                {
                    if (sbyte.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(float))
                {
                    if (float.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else if (targetType == typeof(double))
                {
                    if (double.TryParse(str, out var v))
                    {
                        return v;
                    }
                    else
                    {
                        return _parseFailure(targetType, str);
                    }
                }
                else
                {
                    throw new NotSupportedException($"未受支持的目标类型: {targetType}");
                }
            }
        }

        private object? _parseFailure(Type targetType, string str)
        {
            if (ThrowExceptionWhenParseFailure)
            {
                throw new FormatException($"未能将字符串 \"{str}\" 转换为 {targetType.Name}");
            }
            else
            {
                return Common_Util.ValueHelper.DefaultValue(targetType);
            }
        }
        
    }
}
