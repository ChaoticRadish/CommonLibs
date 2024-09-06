using Common_Util.Extensions;
using Common_Wpf.SettingPackage;
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
    /// 从 <see cref="BaseBoxSetting"/> 取得特定的属性值
    /// <para>输入参数: [0]:设定结构体或其集合; [1]:想要查找的名称; [2]:属性名, 为空时表示返回设定结构体本身; [3]:默认值</para>
    /// </summary>
    public class BaseBoxSettingGetValueConverter : IMultiValueConverter
    {
        private static BaseBoxSetting? TryFindSetting(object[] values)
        {
            if (values[0] is BaseBoxSetting setting)
            {
                return setting;
            }
            else 
            {
                string? targetName = (values.Length < 2 || values[1] is not string) ? null : (string)values[1];
                if (values[0] is Array array)
                {
                    foreach (var item in array)
                    {
                        if (item is BaseBoxSetting s && (targetName == null || targetName == s.Name)) return s;
                    }
                }
                else if (values[0] is IEnumerable<BaseBoxSetting> enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is BaseBoxSetting s && (targetName == null || targetName == s.Name)) return s;
                    }
                }
            }
            return null;
        }

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1) throw new Exception("传入参数数量不足! ");
            var setting = TryFindSetting(values);
            string propertyName = (values.Length < 3 || values[2] is not string) ? string.Empty : (string)values[2];
            object? defaultValue = values.Length < 4 ? null : values[3];

            if (setting == null) return defaultValue;
            else if (propertyName.IsEmpty()) return setting;
            else
            {
                Type type = typeof(BaseBoxSetting);
                var property = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception($"找不到属性 {propertyName} ");
                else
                {
                    return property.GetValue(setting) ?? defaultValue;
                }

            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
