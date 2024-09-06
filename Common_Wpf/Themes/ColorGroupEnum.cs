using Common_Wpf.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Themes
{
    public enum ColorGroupEnum
    {
        /// <summary>
        /// 默认
        /// </summary>
        [ColorFileName("Default.xaml")]
        Default,
        /// <summary>
        /// 圣光
        /// </summary>
        [ColorFileName("Holy.xaml")]
        Holy,
        /// <summary>
        /// 黑色
        /// </summary>
        [ColorFileName("Dark.xaml")]
        Dark,
    }
}
