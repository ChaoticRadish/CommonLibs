using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Common_WpfWinformMix.Win32
{
    /// <summary>
    /// win32 窗口的包装类
    /// </summary>
    public class Win32Wrapper : System.Windows.Forms.IWin32Window, System.Windows.Interop.IWin32Window
    {
        public Win32Wrapper(IntPtr handle)
        {
            Handle = handle;
        }

        public Win32Wrapper(System.Windows.Window window)
        {
            Handle = new WindowInteropHelper(window).Handle;
        }

        public Win32Wrapper(System.Windows.Forms.Form form)
        {
            Handle = form.Handle;
        }
        public IntPtr Handle { get; }
    }
}
