using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Scheduling
{
    /// <summary>
    /// 等待的帮助类
    /// </summary>
    /// <remarks>
    /// 涉及到时间的调度偶尔会需要等待一定时间, 这个类实现了一些等待方法方便使用
    /// </remarks>
    public static class WaitHelper
    {
        /// <summary>
        /// 忙等 <paramref name="waitTime"/> ms 时间, 剩余等待时间较长的情况下会使用 <see cref="Thread.Sleep"/> 休眠 1ms 来降低对 CPU 的占用
        /// </summary>
        /// <remarks>
        /// 此方法执行过程中会将线程优先级提升至 <see cref="ThreadPriority.Highest"/>, 并在退出时复原
        /// </remarks>
        /// <param name="waitTime">等待时间, 单位: 毫秒. 该值为 0 时将直接返回不作等待</param>
        /// <param name="cancellationToken">取消 token, 取消时将在下一次检查是否等待完成时结束等待</param>
        public static void BusyWait(double waitTime, CancellationToken? cancellationToken = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(waitTime);
            if (waitTime == 0) return;

            ThreadPriority priority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            try
            {
                long needWait = (long)(waitTime * 10000);    // 单位: tick
                Stopwatch stopwatch = Stopwatch.StartNew();
                long start = stopwatch.ElapsedTicks;
                while ((stopwatch.ElapsedTicks - start) < needWait)
                {
                    if (cancellationToken?.IsCancellationRequested == true) break;

                    if (needWait - (stopwatch.ElapsedTicks - start) > 18 * 10000L)
                    {
                        Thread.Sleep(1);
                    }
                }

            }
            finally
            {
                Thread.CurrentThread.Priority = priority;
            }

        }
    }
}
