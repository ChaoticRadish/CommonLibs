using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Common_Util.Extensions;
using Common_Wpf.Attributes;

namespace Common_Wpf.Themes
{
    public static class ColorManager
    {
        static ColorManager()
        {
            string fileName = _GetColorFileName(ColorGroupEnum.Default);
            DefaultColorGroupResDic.Source = new Uri(_FullColorFileName(fileName));
            Application.Current.Resources.MergedDictionaries.Add(DefaultColorGroupResDic);

        }

        /// <summary>
        /// 设置当前的颜色组文件为指定的文件名, 其路径固定为根目录下 /Themes/ColorGroups/{输入值}
        /// </summary>
        public static string ColorGroupFileName
        {
            get
            {
                return _colorGroupFileName;
            }
            set
            {
                string fileName = value;
                _colorGroupFileName = fileName;

                if (_colorGroupIndex < 0)
                {
                    _colorGroupIndex = Application.Current.Resources.MergedDictionaries.Count;
                    Application.Current.Resources.MergedDictionaries.Add(ColorGroupResDic);
                }
                ColorGroupResDic.Source = new Uri(_FullColorFileName(fileName));
            }
        }
        private static string _colorGroupFileName = "Default.xaml";

        /// <summary>
        /// 设置当前的颜色组文件为指定枚举对应的文件名, 具体值从枚举的 <see cref="ColorFileNameAttribute"/> 特性获取
        /// </summary>
        public static ColorGroupEnum ColorGroup
        {
            get => _colorGroup;
            set
            {
                _colorGroup = value;

                string fileName = _GetColorFileName(value);

                ColorGroupFileName = fileName;
            }
        }
        private static ColorGroupEnum _colorGroup = ColorGroupEnum.Default;
        private static int _colorGroupIndex = -1;

        private static ResourceDictionary DefaultColorGroupResDic = new ResourceDictionary();
        private static ResourceDictionary ColorGroupResDic = new ResourceDictionary();

        private static string _GetColorFileName(ColorGroupEnum color)
        {
            string fileName = string.Empty;
            Type type = typeof(ColorGroupEnum);
            FieldInfo? field = type.GetField(color.ToString());
            if (field == null)
            {
                throw new Exception("传入了意料之外的值: " + color);
            }

            if (field.ExistCustomAttribute<ColorFileNameAttribute>(out var attr) && attr != null)
            {
                fileName = attr.FileName;
            }

            if (fileName.IsEmpty())
            {
                throw new Exception("颜色组资源字典文件名为空值! ");
            }

            return fileName;
        }
        private static string _FullColorFileName(string fileName) => $"pack://application:,,,/Common_Wpf;component/Themes/ColorGroups/{fileName}";
    }
}
