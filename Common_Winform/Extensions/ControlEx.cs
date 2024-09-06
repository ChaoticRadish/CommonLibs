using Common_Winform.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Extensions
{
    public static class ControlEx
    {
        /// <summary>
        /// 自动检查是否需要使用BeginInvoke方法
        /// </summary>
        /// <param name="c"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public static void AutoBeginInvoke(this Control c, Delegate method, params object[] args)
        {
            if (c.InvokeRequired)
            {
                c.BeginInvoke(method, args);
            }
            else
            {
                method.DynamicInvoke(args);
            }
        }
        /// <summary>
        /// 自动检查是否需要使用BeginInvoke方法
        /// </summary>
        /// <param name="c"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public static void AutoBeginInvoke(this Control c, Action method)
        {
            if (c == null || c.IsDisposed)
            {
                return;
            }
            object[] args = Array.Empty<object>();
            if (c.InvokeRequired)
            {
                c.BeginInvoke(method, args);
            }
            else
            {
                method.DynamicInvoke(args);
            }
        }

        /// <summary>
        /// 自动检查是否需要使用Invoke方法
        /// </summary>
        /// <param name="c"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public static void AutoInvoke(this Control c, Delegate method, params object[] args)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(method, args);
            }
            else
            {
                method.DynamicInvoke(args);
            }
        }
        /// <summary>
        /// 自动检查是否需要使用Invoke方法
        /// </summary>
        /// <param name="c"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public static void AutoInvoke(this Control c, Action method)
        {
            if (c == null || c.IsDisposed)
            {
                return;
            }
            object[] args = Array.Empty<object>();
            if (c.InvokeRequired)
            {
                c.Invoke(method, args);
            }
            else
            {
                method.DynamicInvoke(args);
            }
        }

        
        /// <summary>
        /// 自动设置是否可用
        /// </summary>
        /// <param name="c"></param>
        /// <param name="b"></param>
        public static void AutoSetEnable(this Control c, bool b)
        {
            if (c.InvokeRequired)
            {
                try
                {
                    c.Invoke(new Action(() => { c.Enabled = b; }));
                }
                catch
                {

                }
            }
            else
            {
                c.Enabled = b;
            }
        }

        /// <summary>
        /// 判断当前环境是否设计器
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public static bool IsDesignMode(this Control _)
        {
            bool returnFlag = false;

#if DEBUG
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                returnFlag = true;
            }
            else if (Process.GetCurrentProcess().ProcessName == "devenv")
            {
                returnFlag = true;
            }
#endif

            return returnFlag;
        }


        /// <summary>
        /// 将控件中的某个点转换为所属窗口(由内到外找到的第一层窗口)中的位置
        /// </summary>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Point LocationOnClient(this Control c, Point p)
        {
            for (; c != null && !typeof(Form).IsAssignableFrom(c.GetType()); c = c.Parent)
            {
                if (c is IControlList controlList)
                {
                    p.X -= controlList.InnerAreaOffset.X;
                    p.Y -= controlList.InnerAreaOffset.Y;
                }
                p.Offset(c.Location);
            }
            return p;
        }


    }
}
