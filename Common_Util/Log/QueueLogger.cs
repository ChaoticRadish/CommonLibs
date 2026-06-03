using Common_Util.Module.Concurrency;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common_Util.Log
{

    /// <summary>
    /// 使用一个队列处理输入的日志信息的日志输出器
    /// </summary>
    public abstract class QueueLogger : ILogger
    {

        public QueueLogger()
        {
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
            start();
        }
        public QueueLogger(CancellationToken cancellationToken)
        {
            _token = cancellationToken;
            start();
        }
        private void start()
        {
            Task.Factory.StartNew(loop, TaskCreationOptions.LongRunning);
        }

        #region 实际输出日志的循环
        private BlockingCollection<LogData> _queue = new();

        private ManualResetEventSlim _pauseSignal = new(true);

        private CancellationTokenSource? _cts;
        private CancellationToken _token;

        private AsyncCounter _counter = new();

        private void loop()
        {
            Thread.CurrentThread.Name = $"{GetType().Name}-output_loop";

            try
            {
                while (!_token.IsCancellationRequested)
                {
                    // _pauseSignal.Wait(_token);
                    if (_queue.TryTake(out LogData? data, Timeout.Infinite, _token))
                    {
                        _pauseSignal.Wait(_token);
                        Output(data);
                    }
                    _counter.Decrement();
                }
                _counter.Set(0);
            }
            catch (OperationCanceledException)
            {
                _counter.Set(0);
            }
        }
        #endregion

        #region 输入
        public void Log(LogData log)
        {
            _queue.Add(log);
            _counter.Increment();
        }
        #endregion

        #region 控制
        /// <summary>
        /// 暂停日志输出
        /// </summary>
        public void Pause()
        {
            _pauseSignal.Reset();
        }
        /// <summary>
        /// 恢复日志输出
        /// </summary>
        public void Continue()
        {
            _pauseSignal.Set();
        }
        /// <summary>
        /// 中止日志输出, 无法使用 <see cref="Continue"/> 恢复
        /// </summary>
        /// <remarks>
        /// 如果实例化时传入了 <see cref="CancellationToken"/>, 则无法使用此方法来中止输出, 需要通过其源来中止
        /// </remarks>
        public void Stop()
        {
            _cts?.Cancel();
        }

        /// <summary>
        /// 等待直到队列为空
        /// </summary>
        /// <remarks>
        /// 如果已停止, 将直接返回
        /// </remarks>
        public void WaitUntilEmpty()
        {
            try
            {
                _counter.WaitNonpositive(-1, _token);
            }
            catch (OperationCanceledException)
            {

            }
        }
        /// <summary>
        /// 等待直到队列为空或取消
        /// </summary>
        /// <param name="timeout">等待超时时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns>如果超时/取消等待/已停止, 则返回 <see langword="false"/></returns>
        public bool WaitUntilEmpty(int timeout, CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _token);
            var usingCancellationToken = linkedCts.Token;
            try
            {
                return _counter.WaitNonpositive(timeout, usingCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
        /// <summary>
        /// 异步等待队列为空或取消
        /// </summary>
        /// <param name="timeout">等待超时时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns>如果超时/取消等待/已停止, 则返回 <see langword="false"/></returns>
        public async Task<bool> WaitUntilEmptyAsync(int timeout = Timeout.Infinite, CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _token);
            var usingCancellationToken = linkedCts.Token;
            try
            {
                return await _counter.WaitNonpositiveAsync(timeout, usingCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        #endregion

        #region 输出

        protected abstract void Output(LogData log);
        #endregion
    }
}
