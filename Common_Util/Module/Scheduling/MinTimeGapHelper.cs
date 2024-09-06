using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Scheduling
{
    /// <summary>
    /// 最小调用时间间隔的帮助类
    /// </summary>
    public class MinTimeGapHelper
    {

        #region 静态
        private static ConcurrentDictionary<string, MinTimeGapHelper> _actions = new();

        static MinTimeGapHelper()
        {
            new Thread(_checkAndRelease) 
            {
                IsBackground = true // 后台线程才会跟着进程结束
            }.Start();
        }
        /// <summary>
        /// 检查是否有需要被清理掉的项
        /// </summary>
        private static void _checkAndRelease() 
        {
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            while (true)
            {
                Thread.Sleep(100);
                if (_actions.Count > 0)
                {
                    var list = _actions.ToList();
                    foreach (var kvp in list)
                    {
                        if (kvp.Value.LongTimeNotDo)
                        {
                            Release(kvp.Key);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 执行特定的方法, 如果指定的时间间隔内已经有调用过, 则延迟一段时间在调用. 
        /// <para>调用时如果已有相同 key 的项, 将覆盖执行内容, 但是时间间隔与超时系数无法被覆盖, 如果需要更改, 需要先调用 <see cref="Release(string)"/> 释放 key </para>
        /// <para>执行内容将在新线程中执行</para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="timeGap">单位: 秒. 因为用到了 Thread.Sleep 精度不高</param>
        /// <param name="overTimeScale">超时系数, 当 timeGap * overTimeScale 的时间没有执行此方法时, 将会释放此. </param>
        public static void Do(string key, Action action, TimeSpan timeGap, double overTimeScale = 10)
        {
            MinTimeGapHelper? helper = null;
            _actions.AddOrUpdate(key, 
                (key) =>
                {
                    helper = new MinTimeGapHelper(timeGap, action, overTimeScale);
                    return helper;
                }, 
                (key, exist) =>
                {
                    exist.Action = action;
                    helper = exist;
                    return exist;
                });
            if (helper == null)
            {
                throw new NullReferenceException("这里不应该是 null : " + nameof(helper));
            }
            helper.Do();
        }
        /// <summary>
        /// 如果指定的 key 已被缓存, 则释放它
        /// </summary>
        /// <param name="key"></param>
        public static void Release(string key)
        {
            _actions.TryRemove(key, out _);
        }
        #endregion

        private MinTimeGapHelper(TimeSpan timeGap, Action action, double overTimeScale) 
        {
            TimeGap = timeGap;
            Action = action;
            OverTimeScale = overTimeScale > 2 ? overTimeScale : 2;
            CreateTime = DateTime.Now;
        }

        private Action Action { get; set; }
        private TimeSpan TimeGap { get; }
        private DateTime CreateTime { get; }
        private DateTime LastRun { get; set; }
        private double OverTimeScale { get; }

        private readonly object _lock = new object();

        private bool _waitingDo { get; set; }

        private void Do()
        {
            if (_waitingDo)
            {
                return;
            }
            lock (_lock)
            {
                if (_waitingDo)
                {
                    return;
                }
                // 等待状态下不允许执行

                DateTime now = DateTime.Now;
                if (now - LastRun > TimeGap)
                {
                    // 不处于锁定状态
                    InvokeAction(now, true);
                }
                else
                {
                    // 处于锁定状态
                    _waitingDo = true;
                    new Thread(WaitNextDo).Start();
                }
            }
        }
        private void InvokeAction(DateTime now, bool newThread)
        {
            LastRun = now;
            _waitingDo = false;
            
            if (newThread)
            {
                new Thread(new ThreadStart(Action)).Start();
            }
            else
            {
                Action.Invoke();
            }

        }
        /// <summary>
        /// 等待下一次可以被执行
        /// </summary>
        private void WaitNextDo()
        {
            double sleepTime = (TimeGap - (DateTime.Now - LastRun)) // 需间隔时间 - 已间隔时间
                                .TotalMilliseconds;
            if (sleepTime > 0)
            {
                Thread.Sleep((int)sleepTime);
            }

            InvokeAction(DateTime.Now, false);
        }

        /// <summary>
        /// 是否已经太长时间没有被执行
        /// </summary>
        private bool LongTimeNotDo
        {
            get
            {
                if (_waitingDo) return false;
                DateTime now = DateTime.Now;
                DateTime check = CreateTime > LastRun ? CreateTime : LastRun;  // 取较大值, 避免在等待执行之前就被清理
                return (now - check) > (TimeGap * OverTimeScale);
            }
        }
    }
}
