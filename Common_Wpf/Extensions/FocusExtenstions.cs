using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Common_Wpf.Extensions
{
    public static class FocusExtensions
    {
        /// <summary>
        /// 找到第一个文本输入框并设置焦点
        /// </summary>
        /// <param name="d"></param>
        /// <param name="selectAll">是否选择所有文本, 如果为否, 则将光标设置到末尾</param>
        public static void SetFirstTextboxFocus(this DependencyObject d, bool selectAll = false)
        {
            if (d == null) return;
            var textBox = d.GetFirstChildObject<TextBox>();
            if (textBox != null)
            {
                d.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() => {
                        textBox.Focus();
                        if (selectAll)
                        {
                            textBox.SelectAll();
                        }
                        else
                        {
                            textBox.Select(textBox.Text.Length, 0);
                        }
                    }));
            }
        }
    }
}
