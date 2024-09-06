using Common_Winform.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Win32
{
    /// <summary>
    /// 窗口创建事件钩子的帮助类
    /// <para>需设置为静态对象, 防止被回收导致事件无法触发. 或调用静态方法<see cref="Hook(EventHandler{FormCreateEventArgs})"/>, 创建对象到内部的静态变量上. </para>
    /// </summary>
    public class FormCreateHookHelper
    {
        #region win32 api
        internal enum SetWinEventHookFlags
        {
            WINEVENT_INCONTEXT = 4,
            WINEVENT_OUTOFCONTEXT = 0,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_SKIPOWNTHREAD = 1
        }
        /// <summary>
        /// <para>应用程序定义的回调 (或挂钩) 函数，系统调用该函数以响应辅助对象生成的事件</para>
        /// <para>文档: https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nc-winuser-wineventproc</para>
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="iEvent"></param>
        /// <param name="hWnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        internal delegate void WinEventProc(IntPtr hWinEventHook, EventEnum iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);


        /// <summary>
        /// <para>为一系列事件设置事件挂钩函数。</para>
        /// <para>文档: https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwineventhook</para>
        /// </summary>
        /// <param name="eventMin">指定挂钩函数处理的事件范围中最低事件值的事件 常量 。 此参数可以设置为 EVENT_MIN ，以指示可能的最低事件值。</param>
        /// <param name="eventMax">指定由挂钩函数处理的事件范围中最高事件值的事件常量。 此参数可以设置为 EVENT_MAX ，以指示可能的最高事件值。</param>
        /// <param name="hmodWinEventProc"></param>
        /// <param name="lpfnWinEventProc"></param>
        /// <param name="idProcess"></param>
        /// <param name="idThread"></param>
        /// <param name="dwflags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(
            int eventMin, 
            int eventMax, 
            IntPtr hmodWinEventProc,
            WinEventProc lpfnWinEventProc, 
            int idProcess, int idThread,
            SetWinEventHookFlags dwflags);

        /// <summary>
        /// <para>Windows API函数，它的作用是获取已加载到当前进程地址空间的指定模块的句柄。</para>
        /// </summary>
        /// <param name="lpModuleName">宽字符格式</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        /// <summary>
        /// <para>Windows API函数，检索指定模块的模块句柄。 模块必须已由调用进程加载。</para>
        /// <para>文档: https://learn.microsoft.com/zh-cn/windows/win32/api/libloaderapi/nf-libloaderapi-getmodulehandlea</para>
        /// </summary>
        /// <param name="lpModuleName">ANSI 格式</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);

        #endregion

        #region 钩子
        /// <summary>
        /// 是否已经设置上
        /// </summary>
        public bool HookDone { get => PHook != IntPtr.Zero; }
        /// <summary>
        /// 设置上的钩子 (指针)
        /// </summary>
        public IntPtr PHook { get; private set; } = IntPtr.Zero;
        /// <summary>
        /// 钩子对应的回调方法
        /// </summary>
        internal WinEventProc? Listener;

        /// <summary>
        /// 设置上钩子
        /// </summary>
        /// <returns></returns>
        public bool Hook()
        {
            if (!HookDone)
            {
                Listener = new WinEventProc(proc);

                IntPtr hInstance = GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName ?? string.Empty);

                PHook = SetWinEventHook(1, 0x7fffffff, IntPtr.Zero, Listener, Process.GetCurrentProcess().Id, 0, SetWinEventHookFlags.WINEVENT_OUTOFCONTEXT);
                var i = Marshal.GetLastWin32Error();

            }
            return HookDone;
        }

        private void proc(IntPtr hWinEventHook, EventEnum iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            var control = Control.FromHandle(hWnd);
            if (control is Form form)
            {
                switch (iEvent)
                {
                    case EventEnum.EVENT_OBJECT_CREATE:
                        form.AutoBeginInvoke(() =>
                        {
                            OnFormCreate?.Invoke(this, new FormCreateEventArgs(form));
                        });
                        break;
                }
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 事件参数
        /// </summary>
        public class FormCreateEventArgs : EventArgs
        {
            /// <summary>
            /// 触发事件的窗口
            /// </summary>
            public Form Form;

            public FormCreateEventArgs(Form f)
            {
                this.Form = f;
            }
        }
        public event EventHandler<FormCreateEventArgs>? OnFormCreate;
        #endregion

        #region 静态
        private static FormCreateHookHelper? Instance;
        /// <summary>
        /// 创建一个静态的帮助类对象, 存在内部的静态变量上, 再将事件关联到其上. 如果已经创建, 则将事件添加到对象上, 并检查钩子是否已经设置上
        /// </summary>
        /// <param name="onFormCreate"></param>
        /// <returns></returns>
        public static bool Hook(EventHandler<FormCreateEventArgs> onFormCreate)
        {
            Instance ??= new FormCreateHookHelper();
            Instance.OnFormCreate += onFormCreate;
            return Instance.HookDone || Instance.Hook();
        }

        #endregion
    }
}
