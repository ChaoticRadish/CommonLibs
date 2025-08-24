using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Extensions.MultiThread;
using Common_Winform.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
            object[] args = [];
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
        /// 尝试执行传入方法, 如果 <paramref name="func"/> 返回了空值, 视为执行失败
        /// </summary>
        /// <param name="c"></param>
        /// <param name="func"></param>
        /// <param name="allowNull">如果执行结果是 <see langword="null"/>, 是否视为成功结果</param>
        /// <returns></returns>
        public static OperationResultEx<T> TryAutoInvoke<T>(this Control c, Func<T> func, bool allowNull = false)
        {
            ArgumentNullException.ThrowIfNull(c);
            if (c.IsDisposed)
            {
                return "控件已释放";
            }
            object[] args = [];
            object? result;
            try
            {
                if (c.InvokeRequired)
                {
                    result = c.Invoke(func, args);
                }
                else
                {
                    result = func.DynamicInvoke(args);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
            if (result == null)
            {
                if (allowNull)
                {
                    return OperationResultEx<T>.Success(default);
                }
                else
                {
                    return "执行结果为 null! ";
                }
            }
            else if (result is T t)
            {
                return t;
            }
            else
            {
                return $"执行结果类型不是预期类型 {typeof(T)}";
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


        /// <summary>
        /// 获取数据 (广义上的数据, 也可以是操作结果) 然后根据结果执行相应的方法
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="c"></param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task GetDataThenInvokeAsync<TArg, TData>(
            this Control c, GetDataThenInvokeArgs<TArg, TData> args, CancellationToken cancellationToken)
        {
            try
            {
                if (args.Locker != null)
                {
                    await args.Locker.TryWaitOneAsync(cancellationToken: cancellationToken);
                    args.Locker.Reset();
                }

                TArg startArg;
                var getArgResult = c.TryAutoInvoke(args.GetArgFunc);
                OperationFailureException.ThrowIfFailure(getArgResult);
                startArg = getArgResult.Data ?? throw new InvalidOperationException("取得获取数据的参数时, 取得空值! ");

                OperationResultEx<TData> getDataResult;
                try
                {
                    var result = await Task.Run(() => args.GetDataFunc(startArg, cancellationToken));
                    getDataResult = (result, null);
                }
                catch (Exception ex)
                {
                    getDataResult = ex;
                }
                args.GotDataDone?.Invoke(startArg, getDataResult);

                TData? data;
                if (getDataResult)
                {
                    data = getDataResult.Data ?? args.DefaultData ?? throw new InvalidOperationException("获取数据取得空值! ");
                }
                else
                {
                    data = default;
                }

                TArg checkArg;
                getArgResult = c.TryAutoInvoke(args.GetArgFunc);
                OperationFailureException.ThrowIfFailure(getArgResult);
                checkArg = getArgResult.Data ?? throw new InvalidOperationException("重新取得获取数据的参数时, 取得空值! ");

                c.AutoInvoke(() =>
                {
                    bool checkPass = (args.ArgComparer ?? EqualityComparer<TArg>.Default).Equals(startArg, checkArg);
                    if (!checkPass)
                    {
                        args.GotDataButArgChange?.Invoke((startArg, checkArg), getDataResult);
                    }
                    else if (getDataResult)
                    {
                        args.GetDataSuccess?.Invoke(startArg, data!);
                    }
                    else
                    {
                        args.GetDataFailure?.Invoke(startArg, getDataResult);
                    }
                });
            }
            finally
            {
                if (args.Locker != null)
                {
                    args.Locker.Set();
                }
            }
        }

        public struct GetDataThenInvokeArgs<TArg, TData>
        {
            /// <summary>
            /// 锁定器, 如果不为空, 则将在开始执行时等待放行信号, 结束时 (无论成功或失败) 调用它的 <see cref="EventWaitHandle.Set"/>
            /// </summary>
            public AutoResetEvent? Locker { get; init; }


            /// <summary>
            /// 校对参数是否一致的比较器
            /// </summary>
            public IEqualityComparer<TArg> ArgComparer { get; init; }

            /// <summary>
            /// 取得获取数据的过程所需的参数的方法, 返回结果不能是 <see langword="null"/>, 在获取数据前后分别会执行一次, 以确认查询过程中参数是否发生了变更, 在 UI 线程中运行
            /// </summary>
            public required Func<TArg> GetArgFunc { get; init; }
            /// <summary>
            /// 传入获取数据的参数, 返回数据, 返回结果不能是 <see langword="null"/>, 在非 UI 线程中运行
            /// </summary>
            public required Func<TArg, CancellationToken, Task<OperationResult<TData>>> GetDataFunc { get; init; }

            /// <summary>
            /// 获取数据结束 (无论成功与否, 参数是否发生变化), 在 UI 线程中运行
            /// </summary>
            public Action<TArg, OperationResultEx<TData>>? GotDataDone { get; init; }

            /// <summary>
            /// 获取数据结束后, 校对参数发现参数发生了变更
            /// </summary>
            public Action<(TArg oldOne, TArg newOne), OperationResultEx<TData>>? GotDataButArgChange { get; init; }

            /// <summary>
            /// 获取数据成功, 且参数未变更
            /// </summary>
            public required Action<TArg, TData> GetDataSuccess { get; init; }
            /// <summary>
            /// 获取数据失败, 且参数未变更
            /// </summary>
            public Action<TArg, OperationResultEx<TData>>? GetDataFailure { get; init; }

            /// <summary>
            /// 如果获取数据后, 数据为 <see langword="null"/> 值, 则赋予此值
            /// </summary>
            public TData? DefaultData { get; init; }

        }


        #region 显示
        /// <summary>
        /// 取得当前 DPI 缩放比例
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static float CurrentDpiScale(this Control control)
        {
            int currentDpi = control.DeviceDpi;
            float dpiScale = currentDpi / 96f; // 计算 DPI 缩放比例
            return dpiScale;
        }
        #endregion
    }
}
