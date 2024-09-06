using Common_Wpf.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Wpf.Globals
{
    /// <summary>
    /// 通用资源管理类, 管理 Common_Wpf 内需要被用到的通用资源
    /// </summary>
    public static class GlobalResources
    {
        static GlobalResources()
        {
            _initDefaultResDic();
        }

        #region 格式字符串
        /// <summary>
        /// 通用的完整时间字符串
        /// </summary>
        public static string FullTimeFormat { get => TryGetStaticResource(nameof(FullTimeFormat), "yyyy-MM-dd HH:mm:ss:fff"); }

        /// <summary>
        /// 精确到秒的完整时间字符串
        /// </summary>
        public static string FullTimeFormat001 { get => TryGetStaticResource(nameof(FullTimeFormat001), "yyyy-MM-dd HH:mm:ss:fff"); }

        /// <summary>
        /// 日期格式字符串
        /// </summary>
        public static string DateFormat000 { get => TryGetStaticResource(nameof(DateFormat000), "yyyy-MM-dd"); }

        #endregion

        #region 文本
        /// <summary>
        /// 测试字符串
        /// </summary>
        public static string TestStr001 { get => TryGetStaticResource(nameof(TestStr001), nameof(GlobalResources)); }
        #endregion

        #region 获取静态资源的方法
        /// <summary>
        /// 尝试获取值, 并将其转换为指定的类型, 如果没有获取到或者不是指定的类型, 将返回输入的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static T TryGetStaticResource<T>(string key, T defaultValue)
        {
            _checkInit();
            if (Application.Current.Resources.Contains(key))
            {
                object exist = Application.Current.Resources[key];
                if (exist is T tValue)
                {
                    return tValue;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region 需加载文件的配置
        /// <summary>
        /// 资源字典文件名: 格式数据
        /// </summary>
        public static string FileNameFormats { get; set; } = DEFAULT_FILENAME_FORMATS;
        public const string DEFAULT_FILENAME_FORMATS = "Formats.xaml";
        /// <summary>
        /// 资源字典文件名: 文本
        /// </summary>
        public static string FileNameTexts { get; set; } = DEFAULT_FILENAME_TEXTS;
        public const string DEFAULT_FILENAME_TEXTS = "Texts.xaml";
         
        private static void _initDefaultResDic()
        {
            _defaultResDic_Formats.Source = new Uri(_fullFileName(DEFAULT_FILENAME_FORMATS));
            Application.Current.Resources.MergedDictionaries.Add(_defaultResDic_Formats);

            _defaultResDic_Texts.Source = new Uri(_fullFileName(DEFAULT_FILENAME_TEXTS));
            Application.Current.Resources.MergedDictionaries.Add(_defaultResDic_Texts);

        }

        private static ResourceDictionary _defaultResDic_Formats = new ResourceDictionary();
        private static ResourceDictionary _defaultResDic_Texts = new ResourceDictionary();

        private static ResourceDictionary _resDic_Formats = new ResourceDictionary();
        private static ResourceDictionary _resDic_Texts = new ResourceDictionary();

        private static int _resDicIndex_Formats = -1;
        private static int _resDicIndex_Texts = -1;

        private static string _fullFileName(string fileName) => $"pack://application:,,,/Common_Wpf;component/Globals/{fileName}";
        #endregion

        #region 初始化
        private static bool _isInited = false;

        private static readonly object _initLocker = new object();


        private static void _init()
        {
            if (_resDicIndex_Formats < 0)
            {
                _resDicIndex_Formats = Application.Current.Resources.MergedDictionaries.Count;
                Application.Current.Resources.MergedDictionaries.Add(_resDic_Formats);
            }
            _resDic_Formats.Source = new Uri(_fullFileName(FileNameFormats));

            if (_resDicIndex_Texts < 0)
            {
                _resDicIndex_Texts = Application.Current.Resources.MergedDictionaries.Count;
                Application.Current.Resources.MergedDictionaries.Add(_resDic_Texts);
            }
            _resDic_Texts.Source = new Uri(_fullFileName(FileNameTexts));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _checkInit()
        {
            if (_isInited)
            {
                return;
            }
            lock (_initLocker)
            {
                if (!_isInited)
                {
                    _init();
                    _isInited = true;
                }
            }

        }

        /// <summary>
        /// 检查是否已经初始化, 如果未初始化, 则初始化
        /// </summary>
        public static void CheckInit()
        {
            _checkInit();
        }
        /// <summary>
        /// 重新初始化
        /// </summary>
        public static void ReInit()
        {
            _isInited = false;
            _checkInit();
        }
        #endregion
    }
}
