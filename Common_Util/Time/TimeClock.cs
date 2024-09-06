using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Time
{
    /// <summary>
    /// 差不多就是个计时器啦, 主要用途是调用Update获取时间间隔, 记得Start它
    /// </summary>
    public class TimeClock : IClock
    {
        /// <summary>
        /// 计时器的状态
        /// </summary>
        public enum State
        {
            /// <summary>
            /// 等待启动
            /// </summary>
            Waiting,
            /// <summary>
            /// 计时中
            /// </summary>
            Running,
            /// <summary>
            /// 暂停
            /// </summary>
            Pause,
            /// <summary>
            /// 停止
            /// </summary>
            Stoped,
        }
        /// <summary>
        /// 计时器状态
        /// </summary>
        public State ClockState { get; private set; } = State.Waiting;

        /// <summary>
        /// 计时器的名字
        /// </summary>
        public string Name { get; set; } = "计时器";

        /// <summary>
        /// 计时器模式枚举
        /// </summary>
        public enum TimerMode
        {
            /// <summary>
            /// 时间自增 (计算系统时间)
            /// </summary>
            Normal,
            /// <summary>
            /// 时间手动传入时间变化量 (使用者传入时间变化量)
            /// </summary>
            Manual,
            /// <summary>
            /// 倒计时, 时间手动传入时间变化量 (使用者传入时间变化量)
            /// </summary>
            ManualCountdown,
        }
        /// <summary>
        /// 模式
        /// </summary>
        public TimerMode Mode { get; private set; }

        /// <summary>
        /// 码表
        /// </summary>
        private readonly Stopwatch _stopwatch;
        /// <summary>
        /// 内部计时
        /// </summary>
        private float _timer = 0;

        /// <summary>
        /// 实例化一个时间自增模式(计算系统时间)的计时器
        /// </summary>
        public TimeClock() : this(TimerMode.Normal)
        {

        }
        public TimeClock(TimerMode mode)
        {
            _stopwatch = new Stopwatch();
            Mode = mode;
        }

        private double _lastUpdate;

        /// <summary>
        /// 停下时的值 (如果运行中, 则返回当前值)
        /// </summary>
        public double TimeWhenStop
        {
            get
            {
                if (IsRunning)
                {
                    return ElapseTime;
                }
                else
                {
                    return _TimeWhenStop;
                }
            }
            protected set
            {
                _TimeWhenStop = value;
            }
        }
        private double _TimeWhenStop = -1;

        /// <summary>
        /// 计时中 ? 
        /// </summary>
        public bool IsRunning { get { return ClockState == State.Running; } }
        /// <summary>
        /// 停止状态 ?
        /// </summary>
        public bool IsStoped { get { return ClockState == State.Stoped; } }

        /// <summary>
        /// 暂停中 ?
        /// </summary>
        public bool Pausing { get { return ClockState == State.Pause; } }

        /// <summary>
        /// 开始运行
        /// </summary>
        /// <param name="autoStopTime">自动停止时间, 如果输入了负数, 将不会在本方法内设置自动停止</param>
        public void Start(float autoStopTime = -1)
        {
            // 不处于等待状态无法启动
            if (ClockState != State.Waiting) return;

            if (autoStopTime >= 0)
            {
                AutoStop = autoStopTime;
            }
            switch (Mode)
            {
                case TimerMode.Normal:
                    _stopwatch.Start();
                    _lastUpdate = 0;
                    break;
                case TimerMode.Manual:
                    _timer = 0;
                    _lastUpdate = 0;
                    break;
                case TimerMode.ManualCountdown:
                    _timer = autoStopTime;
                    _lastUpdate = autoStopTime;
                    break;
            }

            ClockState = State.Running;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            // 只有计时中状态下可以暂停
            if (ClockState != State.Running) return;
            switch (Mode)
            {
                case TimerMode.Normal:
                    _stopwatch.Stop();
                    break;
                case TimerMode.Manual:
                    break;
                case TimerMode.ManualCountdown:
                    break;
            }
            ClockState = State.Pause;
        }
        /// <summary>
        /// 继续
        /// </summary>
        public void Continue()
        {
            // 只有计时中状态下可以暂停
            if (ClockState != State.Pause) return;
            switch (Mode)
            {
                case TimerMode.Normal:
                    _stopwatch.Start();
                    break;
                case TimerMode.Manual:
                    break;
                case TimerMode.ManualCountdown:
                    break;
            }
            ClockState = State.Running;
        }


        /// <summary>
        /// 停下 (可以调用Restart以继续)
        /// </summary>
        public void Stop()
        {
            // 不处于计时状态时无法停止
            if (ClockState != State.Running) return;

            _TimeWhenStop = ElapseTime;
            switch (Mode)
            {
                case TimerMode.Normal:
                    _stopwatch.Stop();
                    break;
                case TimerMode.Manual:
                    break;
                case TimerMode.ManualCountdown:
                    break;
            }

            ClockState = State.Stoped;
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        /// <param name="autoStart">在等待启动状态下是否自动启动</param>
        public void Restart(bool autoStart = false)
        {
            if (ClockState == State.Waiting && autoStart)
            {// 等待启动状态下, 并且要求自动启动
                Start();
                return;
            }
            // 不处于停止状态无法重新启动
            if (ClockState != State.Stoped) return;

            switch (Mode)
            {
                case TimerMode.Normal:
                    _stopwatch.Restart();
                    break;
                case TimerMode.Manual:
                    _timer = 0;
                    break;
                case TimerMode.ManualCountdown:
                    _timer = autoStopTime;
                    break;
            }
            ClockState = State.Running;
        }



        /// <summary>
        /// 设置自动停止的时间 (如果time值小于零, 则关闭自动停止 (手动模式限定
        /// </summary>
        public float AutoStop
        {
            get
            {
                return autoStopTime;
            }
            set
            {
                autoStopTime = value;
                if (value >= 0)
                {
                    wantAutoStop = true;
                }
                else
                {
                    wantAutoStop = false;
                }
            }
        }
        /// <summary>
        /// 想要自动停止
        /// </summary>
        private bool wantAutoStop = false;
        private float autoStopTime;
        /// <summary>
        /// 手动模式下自动停止委托
        /// </summary>
        /// <param name="time"></param>
        public delegate void AutoStopDelegate(float time);
        /// <summary>
        /// 手动模式下自动停止事件
        /// </summary>
        public event AutoStopDelegate AutoStopEvent;

        /// <summary>
        /// 返回上一次调用到现在(第一次调用时是开始到现在)的时间间隔 单位: 秒
        /// </summary>
        /// <returns></returns>
        public double Update()
        {
            double now = ElapseTime;
            // 间隔时间
            double updateTime = now - _lastUpdate;
            _lastUpdate = now;
            return updateTime;
        }
        /// <summary>
        /// 将时间更新到计时器
        /// </summary>
        /// <param name="deltaTime">时间变化量(秒)</param>
        /// <returns>返回累计的总时间</returns>
        public float Update(float deltaTime)
        {
            if (ClockState == State.Running)
            {
                switch (Mode)
                {
                    case TimerMode.Manual:
                        _timer += deltaTime;
                        if (_timer < 0)
                        {
                            _timer = 0;
                        }
                        if (wantAutoStop && _timer > autoStopTime)
                        {
                            AutoStopEvent?.Invoke(_timer);
                            Stop();
                        }
                        break;
                    case TimerMode.ManualCountdown:
                        _timer -= deltaTime;
                        if (_timer <= 0 && wantAutoStop)
                        {
                            _timer = 0;
                            AutoStopEvent?.Invoke(_timer);
                            Stop();
                        }
                        break;
                }
            }
            return _timer;
        }
        /// <summary>
        /// 将时间更新到计时器
        /// </summary>
        /// <param name="timeSpan">时间变化量(秒)</param>
        /// <returns>返回累计的总时间</returns>
        public float Update(TimeSpan timeSpan)
        {
            return Update((float)timeSpan.TotalSeconds);
        }
        /// <summary>
        /// 返回上一次调用到现在(第一次调用时是开始到现在)的时间间隔 单位: 毫秒
        /// </summary>
        /// <returns></returns>
        public double UpdateMilliSecond()
        {
            double now = ElapseTime;
            //间隔时间
            double updateTime = now - _lastUpdate;
            _lastUpdate = now;
            return updateTime * 1000;
        }

        /// <summary>
        /// 设置当前时间 (主动模式下有效)
        /// </summary>
        /// <param name="time"></param>
        public void SetTimeNow(float time)
        {
            if (wantAutoStop && time > autoStopTime)
            {
                time = autoStopTime;
            }
            else if (time < 0)
            {
                time = 0;
            }
            _timer = time;
        }

        /// <summary>
        /// 获取当前实例运行到现在的总时间 (秒)
        /// </summary>
        public double ElapseTime
        {
            get
            {
                switch (Mode)
                {
                    case TimerMode.Normal:
                        return _stopwatch.ElapsedMilliseconds * 0.001;
                    case TimerMode.Manual:
                    case TimerMode.ManualCountdown:
                        return _timer;
                }
                return _stopwatch.ElapsedMilliseconds * 0.001;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("计时器[").Append(Name).Append("]: ").Append(ElapseTime).Append(" 秒");
            return stringBuilder.ToString();
        }
    }
}
