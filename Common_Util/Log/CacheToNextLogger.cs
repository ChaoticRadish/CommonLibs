using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 将日志数据缓存起来, 在有另外的日志输出器被设置为 <see cref="GlobalLoggerManager.CurrentLogger"/> 时输出过去的日志输出器
    /// <para>注: 如果缓存的日志数据较多, 可能会导致更换输出器的过程耗时较长! </para>
    /// </summary>
    internal class CacheToNextLogger : CacheLogger, ILogger
    {
        public CacheToNextLogger()
        {
            GlobalLoggerManager.OnSetLogger += GlobalLoggerManager_OnSetLogger;
            GlobalLoggerManager.OnClearLogger += GlobalLoggerManager_OnClearLogger;
        }
        /// <summary>
        /// 当前是否已经绑定 <see cref="GlobalLoggerManager.AfterSetLogger"/> 事件
        /// </summary>
        public bool IsBindingEvent { get; private set; } = false;

        private void GlobalLoggerManager_AfterSetLogger(ILogger newLogger)
        {
            if (newLogger != this)
            {
                OutputLog(newLogger);
            }
        }

        private void GlobalLoggerManager_OnSetLogger(ILogger newLogger)
        {
            if (newLogger == this)
            {
                if (!IsBindingEvent)
                {
                    // 自身被设置为日志输出器时, 监听是否有其他日志输出器被设置为日志输出器
                    GlobalLoggerManager.AfterSetLogger += GlobalLoggerManager_AfterSetLogger;
                    IsBindingEvent = true;
                }
            }
            else
            {
                if (IsBindingEvent)
                {
                    // 已经替换为其他输出器了
                    GlobalLoggerManager.AfterSetLogger -= GlobalLoggerManager_AfterSetLogger;
                    IsBindingEvent = false;
                }
            }
        }
        private void GlobalLoggerManager_OnClearLogger(ILogger obj)
        {
            OutputLog(obj);
            if (IsBindingEvent)
            {
                GlobalLoggerManager.AfterSetLogger -= GlobalLoggerManager_AfterSetLogger;
                IsBindingEvent = false;
            }
        }



    }
}
