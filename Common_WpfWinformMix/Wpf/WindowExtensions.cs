using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_WpfWinformMix.Wpf
{
    public static class WindowExtensions
    {
        /// <summary>
        /// 在 Win32 窗口 <paramref name="winform"/> 上显示 WPF 的模态窗口 <paramref name="window"/> 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="winform">Win32 窗口, 如果是 <see langword="null"/>, 则直接使用 <see cref="System.Windows.Window.ShowDialog"/></param>
        /// <returns></returns>
        public static bool? ShowDialog(this System.Windows.Window window, IWin32Window? winform)
        {
            if (winform != null)
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(window);
                helper.Owner = winform.Handle;
            }
            return window.ShowDialog();
        }

        /// <summary>
        /// 在 Win32 窗口 <paramref name="win32Window"/> 上显示 WPF 的窗口 <paramref name="window"/> 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="win32Window">Win32 窗口, 如果是 <see langword="null"/>, 则直接使用 <see cref="System.Windows.Window.Show"/></param>
        /// <returns></returns>
        public static void Show(this System.Windows.Window window, IWin32Window? win32Window)
        {
            if (win32Window != null)
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(window);
                helper.Owner = win32Window.Handle;
            }
            window.Show();
        }
    }
}
