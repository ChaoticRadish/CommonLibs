using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _queue = new ConcurrentQueue<LogData>();
            Task.Factory.StartNew(loop, TaskCreationOptions.LongRunning);
        }

        #region 实际输出日志的循环
        private ConcurrentQueue<LogData> _queue;

        private void loop()
        {
            Thread.CurrentThread.Name = $"{GetType().Name}-write_loop";
            while (true)
            {
                while (_queue.TryDequeue(out LogData? data))
                {
                    if (data != null)
                    {
                        Output(data);
                    }
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 输入
        public void Log(LogData log)
        {
            _queue.Enqueue(log);
        }
        #endregion

        #region 输出

        protected abstract void Output(LogData log);
        #endregion
    }
}
