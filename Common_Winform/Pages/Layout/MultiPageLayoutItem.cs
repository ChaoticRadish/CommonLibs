using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Pages.Layout
{
    /// <summary>
    /// 多页面布局的子项
    /// </summary>
    public class MultiPageLayoutItem
    {
        public int Index { get; set; }

        /// <summary>
        /// 页面名称, 一般用于标签页或菜单项显示文本
        /// </summary>
        public string? PageName { get; set; }
        /// <summary>
        /// 页面描述, 一般用于鼠标悬停时的提示框
        /// </summary>
        public string? PageDesc { get; set; }

        /// <summary>
        /// 实际页面
        /// </summary>
        public PageBase? Page { get; set; }

    }
}
