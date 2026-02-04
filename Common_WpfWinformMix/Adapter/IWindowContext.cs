using Common_WpfWinformMix.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_WpfWinformMix.Adapter
{
    public interface IWindowContext<TCapabilities> : IWin32Window
    {
        // /// <summary>
        // /// 窗口句柄
        // /// </summary>
        // IntPtr Handle { get; }

        /// <summary>
        /// 提供的功能
        /// </summary>
        TCapabilities Capability { get; }
    }

    public class WpfWindowContext<TCapabilities>(System.Windows.Window window, TCapabilities capabilities) : IWindowContext<TCapabilities>
    {
        private readonly System.Windows.Window window = window;
        private readonly Win32Wrapper wrapper = new(window);
        public nint Handle => wrapper.Handle;
        public TCapabilities Capability { get; } = capabilities;
    }
    public class WinformWindowContext<TCapabilities>(System.Windows.Forms.Form form, TCapabilities capabilities) : IWindowContext<TCapabilities>
    {
        private readonly System.Windows.Forms.Form form = form;
        private readonly Win32Wrapper wrapper = new(form);
        public nint Handle => wrapper.Handle;
        public TCapabilities Capability { get; } = capabilities;
    }

}
