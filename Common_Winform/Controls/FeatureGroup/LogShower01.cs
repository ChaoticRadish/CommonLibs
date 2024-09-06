using Common_Util.Log;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Controls.FeatureGroup
{
    /// <summary>
    /// 简单的日志显示器, 会以文字形式显示日志, 可设定日志显示上限, 不同级别对应文本颜色, 是否显示时间信息等
    /// <para>显示设置修改后需要调用 <see cref="RefreshAll"/> 来刷新显示, 不然可能只有新增的日志使用了新的设置! </para>
    /// <para>一般用于少量, 单一模块或事务日志的显示</para>
    /// </summary>
    public partial class LogShower01 : UserControl, ILogger
    {
        public LogShower01()
        {
            InitializeComponent();
        }

        #region 显示设置
        [Category("CVII_自定义_配置"), DisplayName("展示时间")]
        public bool ShowTime { get; set; } = true;
        [Category("CVII_自定义_配置"), DisplayName("展示时间格式")]
        public string ShowTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";


        [Category("CVII_自定义_配置"), DisplayName("展示日志分类")]
        public bool ShowCategory { get; set; } = false;
        [Category("CVII_自定义_配置"), DisplayName("展示日志子分类")]
        public bool ShowSubCategory { get; set; } = false;
        [Category("CVII_自定义_配置"), DisplayName("展示日志级别")]
        public bool ShowLevel { get; set; } = false;

        [Category("CVII_自定义_配置"), DisplayName("日志头信息与消息放在同一行")]
        public bool HeadSameLine { get; set; } = false;

        [Category("CVII_自定义_配置"), DisplayName("展示日志数量上限")]
        public int Capacity 
        {
            get => capacity;
            set => capacity = Math.Max(CAPACITY_MIN_VALUE, value);
        }
        private int capacity = 100;

        /// <summary>
        /// 展示日志数量下限值
        /// </summary>
        private const int CAPACITY_MIN_VALUE = 10;


        [Category("CVII_自定义_配置"), DisplayName("单次最大处理日志数量")]
        public int MaxDealCountOneTime
        {
            get => maxDealCountOneTime;
            set => maxDealCountOneTime = Math.Max(1, value);
        }
        private int maxDealCountOneTime = 50;


        [Category("CVII_自定义_配置"), DisplayName("超出展示数量上限后的替换文本")]
        public string OverCapacityText 
        {
            get => overCapacityText;
            set => overCapacityText = string.IsNullOrEmpty(overCapacityText) ? DEFAULT_OVER_CAPACITY_TEXT : overCapacityText;
        } 
        private string overCapacityText = DEFAULT_OVER_CAPACITY_TEXT;
        private const string DEFAULT_OVER_CAPACITY_TEXT = "...";

        #endregion

        #region 颜色映射
        private readonly Dictionary<string, Color> LevelColorDic = [];

        /// <summary>
        /// 设置日志级别对应的颜色
        /// </summary>
        /// <param name="level"></param>
        /// <param name="color"></param>
        public void SetLevelColor(string level, Color color)
        {
            if (!LevelColorDic.TryAdd(level, color))
            {
                LevelColorDic[level] = color;
            }
        }

        private Color GetLevelColor(string level)
        {
            if (LevelColorDic.TryGetValue(level, out var color))
            {
                return color;
            }
            else
            {
                return ForeColor;
            }
        }

        #endregion

        #region 日志输入
        public void Log(LogData log)
        {
            lock (AppendLogLocker)
            {
                AppendLog(log);
            }
        }

        #endregion

        #region 显示控制
        /// <summary>
        /// 刷新当前所有显示内容, 当前显示的日志会被保留
        /// </summary>
        public void RefreshAll()
        {
            ClearShowing(true);
        }
        /// <summary>
        /// 清空当前显示的日志
        /// </summary>
        public void Clear()
        {
            ClearShowing(false);
        }
        #endregion

        #region 日志数据

        private readonly object AppendLogLocker = new object();

        /// <summary>
        /// 等待处理后显示出来的日志数据, 先进(时间较早的)先出
        /// </summary>
        private readonly ConcurrentQueue<LogData> WaitShowQueue = [];


        private void AppendLog(LogData log)
        {
            WaitShowQueue.Enqueue(log);
        }
        private void AppendLog(IEnumerable<LogData> logs)
        {
            foreach (LogData log in logs)
            {
                WaitShowQueue.Enqueue(log);
            }
        }
        /// <summary>
        /// 限制当前等待数量(从队列前方, 也就是出口移除), 调用前确保当前显示数量为空, 否则可能会出现部分日志消失的现象
        /// </summary>
        /// <param name="count"></param>
        private void LimitWaitingDataCount(int count)
        {
            while (WaitShowQueue.Count > count)
            {
                WaitShowQueue.TryDequeue(out _);
            }
        }
        #endregion

        #region 显示数据
        private readonly object ShowingUpdateLocker = new();

        /// <summary>
        /// 正在显示的数据, 顺序: 列表从前往后, 日志时间逐渐加大 (越后越晚)
        /// </summary>
        private readonly List<RowData> ShowingData = []; 

        private void ClearShowing(bool keepOldShowing)
        {
            lock (ShowingUpdateLocker)  // 锁定住, 不会有新的行被添加进来
            {
                if (keepOldShowing)
                {
                    lock (AppendLogLocker)  // 将当前显示的所有数据取出, 并放置到待显示数据的前部
                    {
                        List<LogData> allWaiting = [];
                        while (WaitShowQueue.TryDequeue(out var waiting))
                        {
                            allWaiting.Add(waiting);
                        }

                        AppendLog(ShowingData.Select(i => i.Log));
                        AppendLog(allWaiting);
                        LimitWaitingDataCount(Capacity);
                    }
                }
                ShowingData.Clear();
                Shower.Clear();

                CurrentStartOffSet = 0;
                ExistOverCapacityText = false;
            }

        }
        private void AppendShowing(LogData data)
        {
            string text = GetShowingText(data).Replace("\r\n", "\n");
            RowData rowData = new()
            {
                Start = Shower.TextLength + CurrentStartOffSet,
                Length = text.Length,
                Log = data,
            };

            int start = Shower.TextLength;
            // 写入文本
            Shower.SelectionStart = start;
            Shower.SelectionLength = 0;
            Shower.AppendText(text);

            // 设置颜色
            Shower.SelectionStart = start;
            Shower.SelectionLength = text.Length;
            Shower.SelectionFont = Font;
            Shower.SelectionColor = GetLevelColor(data.Level);

            // 取消选择文本
            Shower.SelectionStart = Shower.TextLength;
            Shower.SelectionLength = 0;

            ShowingData.Add(rowData);
        }

        private void ClearOverCapacity() 
        {
            int needRemoveCount = ShowingData.Count - Capacity;
            if (needRemoveCount > 0)
            {
                RowData[] rowDatas = new RowData[needRemoveCount];
                for (int i = 0; i < needRemoveCount; i++)
                {
                    rowDatas[i] = ShowingData[0];
                    ShowingData.RemoveAt(0);
                }
                int totalLength = rowDatas.Sum(x => x.Length);
                if (ExistOverCapacityText)
                {
                    totalLength += OverCapacityText.Length;
                }

                // 替换文本
                Shower.SelectionStart = 0;
                Shower.SelectionLength = totalLength;
                Shower.SelectedText = OverCapacityText;

                // 替换后的文本设定颜色
                Shower.SelectionStart = 0;
                Shower.SelectionLength = OverCapacityText.Length;
                Shower.SelectionFont = Font;
                Shower.SelectionColor = ForeColor;

                ExistOverCapacityText = true;
                CurrentStartOffSet += totalLength;
            }
        }

        private void ShowEnd()
        {
            Shower.SelectionStart = Shower.TextLength - 1;
            Shower.SelectionLength = 0;
            Shower.ScrollToCaret();
        }

        private struct RowData
        {
            public int Start;
            public int Length;
            public LogData Log;
        }
        private int CurrentStartOffSet = 0;
        private bool ExistOverCapacityText = false;

        private string GetShowingText(LogData log)
        {
            StringBuilder sb = new StringBuilder();

            if (ShowLevel && !string.IsNullOrEmpty(log.Level))
            {
                sb.Append($"<{log.Level}>");
            }
            if (ShowCategory && !string.IsNullOrEmpty(log.Category))
            {
                sb.Append($"[{log.Category}]");
            }
            if (ShowSubCategory && !string.IsNullOrEmpty(log.SubCategory))
            {
                sb.Append($"({log.SubCategory})");
            }
            if (ShowTime)
            {
                sb.Append(' ').Append(log.Time.ToString(ShowTimeFormat));
            }
            sb.Append(':');
            if (HeadSameLine)
            {
                sb.Append(' ');
            }
            else
            {
                sb.AppendLine();
            }
            sb.AppendLine(log.Message);
            if (log.Exception != null)
            {
                sb.AppendLine($"异常: " + log.Exception.ToString());
            }
            if (log.StackFrames != null && log.StackFrames.Length > 0)
            {
                sb.AppendLine("堆栈追踪: ");
                for (int index = 0; index < log.StackFrames.Length; index++)
                {
                    var frame = log.StackFrames[index];
                    sb.AppendLine($"{(index + ".").PadRight(5, ' ')}{frame.GetMethod()?.DeclaringType} :: {(frame.GetMethod())} - {frame.GetFileName()}:{frame.GetFileLineNumber()},{frame.GetFileColumnNumber()}");
                }
            }
            bool endWithNewLine;    // 移除末尾的换行符
            do
            {
                endWithNewLine = false;
                if (sb.Length >= 2 && sb[^2] == '\r' && sb[^1] == '\n')
                {
                    endWithNewLine = true;
                    sb.Remove(sb.Length - 2, 2);
                }
                else if (sb.Length >= 1 && sb[^1] == '\n')
                {
                    endWithNewLine = true;
                    sb.Remove(sb.Length - 1, 1);
                }

            } while (endWithNewLine);
            sb.Replace("\n", "\n◇ ");

            if (ShowingData.Count > 0)
            {
                sb.Insert(0, Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #region 输入 => 显示 的处理




        private void OutShowingTimer_Tick(object sender, EventArgs e)
        {
            SuspendLayout();

            lock (ShowingUpdateLocker)
            {
                int dealCountThisTime = 0;
                if (!WaitShowQueue.IsEmpty)
                {
                    dealCountThisTime = Math.Max(WaitShowQueue.Count / 20, 1);
                }
                dealCountThisTime = Math.Min(dealCountThisTime, MaxDealCountOneTime);
                if (dealCountThisTime > 0)
                {
                    int dealCount = 0;
                    while (dealCount < dealCountThisTime
                        && WaitShowQueue.TryDequeue(out LogData? data))
                    {
                        AppendShowing(data);
                        dealCount++;
                    }

                    ClearOverCapacity();

                    ShowEnd();
                }
            }

            ResumeLayout();
        }
        #endregion

    }
}
