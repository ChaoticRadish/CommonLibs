using Common_Util.Attributes.General;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf
{
    enum Logger
    {
        [Logger("Def")]
        Def,

        [Logger("操作")]
        Operation,


        [Logger("控件")]
        Control,
    }
    class UiLogHelper : QueueLogger
    {
        #region 单例
        private static readonly object locker = new();
        public static UiLogHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        _instance ??= new UiLogHelper();
                    }
                }
                return _instance;
            }
        }
        private static UiLogHelper? _instance;

        #endregion


        #region 输出
        private static readonly object locker_output = new object();
        protected override void Output(LogData log)
        {
            if (_window == null)
            {
                cache.Enqueue(log);
            }
            else
            {
                lock (locker_output)
                {
                    _window.Log(log);
                }
            }
        }
        private static Queue<LogData> cache = new Queue<LogData>();


        #endregion

        #region 窗口
        private static LogViewerWindow? _window;

        public static void InitWindow()
        {
            _window = new LogViewerWindow();

            lock (locker_output)
            {
                while (cache.TryDequeue(out var log))
                {
                    _window.Log(log);
                }
            }
        }
        public static void ShowWindow()
        {
            _window?.Show();
        }
        public static void CloseWindow()
        {
            _window?.Close();
        }
        #endregion
    }
}
