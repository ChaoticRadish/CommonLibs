using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common_Wpf.Converter
{
    /// <summary>
    /// 将 byte[] 转换为十六进制显示的字符串的转换器
    /// <para>支持逆向转换, 会将每2个字符 (排除 0~9 a~f 外其他字符后的剩余字符串) 判断为一个byte</para>
    /// </summary>
    public class ByteArrayToHexStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return convert(value, targetType);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return convert(value, targetType);
        }

        private object? convert(object value, Type targetType)
        {
            if (targetType == null) return null;

            if (targetType == typeof(string))
            {
                if (value is byte b)
                {
                    return bs2Str([b]);
                }
                else if (value is byte[] bs)
                {
                    return bs2Str(bs);
                }
            }
            else if (targetType.IsAssignableTo(typeof(IEnumerable)))
            {
                List<byte>? bs = null;
                if (value is string s)
                {
                    bs = str2Bs(s);
                }
                bs ??= [];
                if (targetType == typeof(IEnumerable))
                {
                    return bs;
                }
                else if (targetType == typeof(IEnumerable<byte>))
                {
                    return bs;
                }
                else if (targetType == typeof(byte[]))
                {
                    return bs.ToArray();
                }
                else if (targetType == typeof(IList<byte>))
                {
                    return bs;
                }
            }
            // 这种情况下, 使用输入参数返回默认对应类型
            else if (targetType == typeof(object))
            {
                if (value is byte b)
                {
                    return bs2Str([b]);
                }
                else if(value is byte[] bs)
                {
                    return bs2Str(bs);
                }
                else if (value is IEnumerable<byte> enumerable)
                {
                    return bs2Str(enumerable.ToArray());
                }
                else if (value is string str)
                {
                    return str2Bs(str);
                }
            }
            if (value == null) return null;
            throw new NotImplementedException($"未受支持的输入值: {(value == null ? "<null>" : value.GetType().Name)}, 目标类型: {targetType.Name}");
        }

        private string bs2Str(byte[] bs)
        {
            return bs == null ? string.Empty : bs.ToHexString();
        }
        private List<byte> str2Bs(string s)
        {
            if (s == null || s.Length == 0) { return []; }

            List<byte> bList = new(s.Length / 2 + 1);

            StringBuilder sb = new StringBuilder(2);
            foreach (char c in s)
            {
                if ((c >= '0' && c <= '9')
                    || (c >= 'a' && c <= 'f')
                    || (c >= 'A' && c <= 'F'))
                {
                    sb.Append(c);
                    if (sb.Length == 2)
                    {
                        bList.Add(System.Convert.ToByte(sb.ToString(), 16));
                        sb.Clear();
                    }
                }
                else if (sb.Length > 0 && (c == ' ' || c == '\n' || c == '\r' || c == '\0'))
                {
                    bList.Add(System.Convert.ToByte(sb.ToString(), 16));
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
            {
                bList.Add(System.Convert.ToByte(sb.ToString(), 16));
            }

            return bList;
        }
    }
}
