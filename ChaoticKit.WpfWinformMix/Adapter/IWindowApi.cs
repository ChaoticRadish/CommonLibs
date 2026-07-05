using ChaoticKit.Data.Enums;
using ChaoticKit.WpfWinformMix.Win32;
using ChaoticKit.WpfWinformMix.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.WpfWinformMix.Adapter
{
    public interface IWindowApi 
    {
        void Show() => Show(null);
        void Show(object? owner);
        ModalResult ShowDialog() => ShowDialog(null);
        ModalResult ShowDialog(object? owner);
    }

    public interface IWindowApiContext : IWindowContext<IWindowApi> { }

    public static class WindowApiContext
    {
        public static IWindowApiContext Create(System.Windows.Window window)
            => new WpfWindowApiContext(window);
        public static IWindowApiContext Create(System.Windows.Forms.Form form)
            => new WinformWindowApiContext(form);

        public static Func<System.Windows.Window, IWindowApiContext> CreateWpfContext { get; set; } = DefaultCreateWpfContext;
        private static IWindowApiContext DefaultCreateWpfContext(System.Windows.Window window) => new WpfWindowApiContext(window);


        public static Func<System.Windows.Forms.Form, IWindowApiContext> CreateWinformContext { get; set; } = DefaultCreateWinformContext;
        private static IWindowApiContext DefaultCreateWinformContext(System.Windows.Forms.Form window) => new WinformWindowApiContext(window);
    }

    internal readonly struct WpfWindowApiContext(System.Windows.Window window) : IWindowApiContext
    {
        private readonly System.Windows.Window window = window;
        private readonly Win32Wrapper wrapper = new(window);

        public nint Handle => wrapper.Handle;

        public IWindowApi Capability { get; } = new WpfWindowApiImpl(window);
    }
    /// <summary>
    /// <see cref="IWindowApi"/> 接口在 Wpf 窗口下的默认实现
    /// </summary>
    /// <param name="window"></param>
    public class WpfWindowApiImpl(System.Windows.Window window) : IWindowApi
    {
        public System.Windows.Window Window { get; } = window;

        public virtual void Show(object? owner)
        {
            if (owner is System.Windows.Window other)
            {
                Window.Owner = other;
                Window.Show();
            }
            else if (owner is System.Windows.Forms.Form form)
            {
                Window.Show(form);
            }
            else if (owner is IWin32Window win32Window)
                Window.Show(win32Window);
            else
                Window.Show();
        }

        public virtual ModalResult ShowDialog(object? owner)
        {
            bool? result;
            if (owner is System.Windows.Window other)
            {
                Window.Owner = other;
                result = Window.ShowDialog();
            }
            else if (owner is System.Windows.Forms.Form form)
            {
                result = Window.ShowDialog(form);
            }
            else if (owner is IWin32Window win32Window)
                result = Window.ShowDialog(win32Window);
            else
                result = Window.ShowDialog();
            return Wpf.UnifiedConverter.ToModalResult(result);
        }
    }


    internal readonly struct WinformWindowApiContext(System.Windows.Forms.Form form) : IWindowApiContext
    {
        private readonly System.Windows.Forms.Form form = form;
        private readonly Win32Wrapper wrapper = new(form);

        public nint Handle => wrapper.Handle;

        public IWindowApi Capability { get; } = new WinformWindowApiImpl(form);

    }
    /// <summary>
    /// <see cref="IWindowApi"/> 接口在 Winform 窗口下的默认实现
    /// </summary>
    /// <param name="form"></param>
    public class WinformWindowApiImpl(System.Windows.Forms.Form form) : IWindowApi
    {
        public Form Form { get; } = form;

        public virtual void Show(object? owner)
        {
            if (owner is System.Windows.Window window)
            {
                Win32Wrapper wrapper = new(window);
                Form.Show(wrapper);
            }
            else if (owner is System.Windows.Forms.Form other)
                Form.Show(other);
            else if (owner is IWin32Window win32Window)
                Form.Show(win32Window);
            else
                Form.Show();
        }

        public virtual ModalResult ShowDialog(object? owner)
        {
            DialogResult result;
            if (owner is System.Windows.Window window)
            {
                Win32Wrapper wrapper = new(window);
                result = Form.ShowDialog(wrapper);
            }
            else if (owner is System.Windows.Forms.Form other)
                result = Form.ShowDialog(other);
            else if (owner is IWin32Window win32Window)
                result = Form.ShowDialog(win32Window);
            else
                result = Form.ShowDialog();
            return Winform.UnifiedConverter.ToModalResult(result);
        }
    }

}
