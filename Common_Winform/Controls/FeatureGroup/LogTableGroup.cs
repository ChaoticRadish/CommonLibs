using Common_Util.Extensions;
using Common_Util.GDI.Extensions;
using Common_Util.Log;
using Common_Winform.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Controls.FeatureGroup
{
    public partial class LogTableGroup : UserControl, ILogger
    {
        public LogTableGroup()
        {
            InitializeComponent();

            ShowTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";

            LogShower.AutoGenerateColumns = false;
            LogShower.DataSource = ShowingData;

            Input_Level.AllowNotSelect = true;
            Input_Level.SetSelectItemsFunc(() =>
            {
                return LevelConfigDic.Select(i => BaseListComboBox.ItemData.NewItem(i.Value, $"{i.Value.Name.WhenEmptyDefault(i.Value.Code)}")).ToList();
            });
            Input_Type.AllowNotSelect = true;
            Input_Type.SetSelectItemsFunc(() =>
            {
                return TypeConfigDic.Select(i => BaseListComboBox.ItemData.NewItem(i.Value, $"{i.Value.Name.WhenEmptyDefault(i.Value.Code)}")).ToList();
            });
        }

        #region 其他配置
        [Category("CVII_自定义_配置"), DisplayName("展示时间格式")]
        public string ShowTimeFormat { get => TimeColumn.DefaultCellStyle.Format; set => TimeColumn.DefaultCellStyle.Format = value; }


        [Category("CVII_自定义_配置"), DisplayName("实时刷新")]
        public bool KeepRefresh { get => CheckBox_KeepRefresh.Checked; set => CheckBox_KeepRefresh.Checked = value; }

        [Category("CVII_自定义_配置"), DisplayName("控制区可见")]
        public bool ControllerVisible { get => ControllerArea.Visible; set => ControllerArea.Visible = value; }


        [Category("CVII_自定义_配置"), DisplayName("级别检查"), Description("如果传入未配置的级别, 将不加入数据列表")]
        public bool LevelCheck { get; set; } = true;

        [Category("CVII_自定义_配置"), DisplayName("类别检查"), Description("如果传入未配置的类别, 将不加入数据列表")]
        public bool TypeCheck { get; set; } = true;

        [Category("CVII_自定义_配置"), DisplayName("最大日志数")]
        public int MaxLog { get; set; } = 10000;

        [Category("CVII_自定义_配置"), DisplayName("单页显示数量")]
        public int PageSize { get => PagingBox.PageSize; set => PagingBox.PageSize = value; }
        #endregion

        #region 状态

        [Category("CVII_自定义_状态"), DisplayName("对级别过滤展示")]
        public bool FilterLevel { get => LevelConfigDic != null && LevelConfigDic.Count > 0 && LevelConfigDic.Any(i => !i.Value.Show); }
        [Category("CVII_自定义_状态"), DisplayName("对类型过滤展示")]
        public bool FilterType { get => TypeConfigDic != null && TypeConfigDic.Count > 0 && TypeConfigDic.Any(i => !i.Value.Show); }
        #endregion

        #region 级别类别配置
        public readonly Dictionary<string, LevelConfig> LevelConfigDic = new Dictionary<string, LevelConfig>();
        public readonly Dictionary<string, TypeConfig> TypeConfigDic = new Dictionary<string, TypeConfig>();
        /// <summary>
        /// 展示范围是否变更
        /// </summary>
        private bool ShowRangeDirty { get; set; } = true;

        public class LevelConfig
        {
            public LevelConfig(string code, string? name = null)
            {
                Code = code;
                Name = name ?? code;
            }

            public string Code { get; set; }
            public string Name { get; set; }
            public bool Show { get; set; }

            public Color? ForeColor { get; set; }

            public Color? BackColor { get; set; }

        }

        public class TypeConfig
        {
            public TypeConfig(string code, string? name = null)
            {
                Code = code;
                Name = name ?? code;
            }

            public string Code { get; set; }
            public string Name { get; set; }
            public bool Show { get; set; }
        }

        public void SetLevel(Enum code, string? name = null, bool show = true, Color? foreColor = null, Color? backColor = null)
        {
            SetLevel(code.ToString(), name, show, foreColor, backColor);
        }
        public void SetLevel(string code, string? name = null, bool show = true, Color? foreColor = null, Color? backColor = null)
        {
            LevelConfig config;
            if (LevelConfigDic.ContainsKey(code))
            {
                config = LevelConfigDic[code];
            }
            else
            {
                config = new LevelConfig(code, name);
                LevelConfigDic.Add(code, config);
            }
            config.Name = name ?? code;
            config.Show = show;
            config.ForeColor = foreColor;
            config.BackColor = backColor;

            Input_Level.RefreshItems();
            ShowRangeDirty = true;
        }
        public void SetType(Enum code, string? name = null, bool show = true)
        {
            SetType(code.ToString(), name, show);
        }
        public void SetType(string code, string? name = null, bool show = true)
        {
            TypeConfig config;
            if (TypeConfigDic.ContainsKey(code))
            {
                config = TypeConfigDic[code];
            }
            else
            {
                config = new TypeConfig(code, name);
                TypeConfigDic.Add(code, config);
            }
            config.Name = name ?? code;
            config.Show = show;

            Input_Type.RefreshItems();
            ShowRangeDirty = true;
        }


        private void Input_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Input_Type.SelectedObj != null && Input_Type.SelectedObj is TypeConfig config)
            {
                TypeConfigDic.ForEach((k, v) => v.Show = v.Code == config.Code);
            }
            else
            {
                TypeConfigDic.ForEach((k, v) => v.Show = true);
            }

            ShowRangeDirty = true;
        }
        private void Input_Level_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Input_Level.SelectedObj != null && Input_Level.SelectedObj is LevelConfig config)
            {
                LevelConfigDic.ForEach((k, v) => v.Show = v.Code == config.Code);
            }
            else
            {
                LevelConfigDic.ForEach((k, v) => v.Show = true);
            }

            ShowRangeDirty = true;
        }
        #endregion

        #region 数据
        private readonly Queue<LogItem> WaitDeal = new Queue<LogItem>();
        private readonly Queue<LogItem> Logs = new Queue<LogItem>();
        /// <summary>
        /// 过滤后的日志, 应当被显示出来的数据, 重设配置之后需要重新从 <see cref="Logs"/> 中查询
        /// </summary>
        private readonly Queue<LogItem> LogsAfterFilter = new Queue<LogItem>();
        private readonly BindingList<LogItem> ShowingData = new BindingList<LogItem>();
        public class LogItem
        {
            /// <summary>
            /// 级别编码
            /// </summary>
            public string? Level { get; set; }
            public string? LevelName { get; set; }
            /// <summary>
            /// 类别编码
            /// </summary>
            public string? Type { get; set; }
            public string? TypeName { get; set; }
            public string? Message { get; set; }
            public DateTime Time { get; set; }

            public Exception? Exception { get; set; }

            public override string ToString()
            {
                string str;
                if (Message == null) str = "";
                else
                {
                    str = Message.Length > 100 ? Message.Substring(0, 100 - 3) + "..." : Message;
                }
                return $"[{Time:yyyy-MM-dd HH:mm:ss:fff}] {TypeName} {LevelName} {str}";
            }
        }
        #endregion

        #region 操作

        public void Log(LogData log)
        {
            Log(log.Level, log.Category, $"{log.SubCategory} > {log.Message}", log.Time, log.Exception);
        }
        public void Log(string level, string type, string message, DateTime time, Exception? ex = null)
        {
            LogItem item = new LogItem()
            {
                Level = level,
                Type = type,
                Message = message,
                Time = time,
                Exception = ex
            };

            WaitDeal.Enqueue(item);


        }
        #endregion

        #region 表格
        private void LogShower_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i < 0 || i >= LogShower.RowCount)
                {
                    continue;
                }
                var row = LogShower.Rows[i];
                //DataGridViewCellStyle baseStyle = row.InheritedStyle;
                if (row.DataBoundItem is LogItem item)
                {
                    if (LevelConfigDic.ContainsKey(item.Level ?? ""))
                    {
                        var levelConfig = LevelConfigDic[item.Level ?? ""];
                        if (levelConfig.ForeColor != null)
                        {
                            row.DefaultCellStyle.ForeColor = levelConfig.ForeColor.Value;
                            row.DefaultCellStyle.SelectionForeColor = levelConfig.ForeColor.Value.Multi(0.5, 0.5, 0.5);
                        }
                        if (levelConfig.BackColor != null)
                        {
                            row.DefaultCellStyle.BackColor = levelConfig.BackColor.Value;
                            row.DefaultCellStyle.SelectionBackColor = levelConfig.BackColor.Value.Multi(0.5, 0.5, 0.5);
                        }
                    }
                }
            }
        }

        private void LogShower_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var i = e.RowIndex;
            if (i >= 0 && i < ShowingData.Count)
            {
                LogItem item = ShowingData[i];
                if (item != null && item.Exception != null)
                {
                    ExceptionShowerForm01 exceptionShower = new ExceptionShowerForm01()
                    {
                        Exception = item.Exception,
                        Text = $"[{item.Time:yyyy-MM-dd HH:mm:ss}]({item.TypeName})({item.LevelName}) 日志包含的异常 ::: {item.Message.Brief()}",
                    };
                    exceptionShower.Show();
                }
            }
        }
        #endregion


        #region 刷新数据显示
        private readonly object ShowingUpdateLocker = new object();
        /// <summary>
        /// 自动定时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerRefreshShowing_Tick(object sender, EventArgs e)
        {
            SuspendLayout();

            lock (ShowingUpdateLocker)
            {
                if (ShowRangeDirty)
                {
                    _reShowAllLog();
                    ShowRangeDirty = false;
                }
                _showNewLogs();

                UpdateCountShower();
            }

            ResumeLayout();
        }
        private void _reShowAllLog()
        {
            bool filterLevel = FilterLevel;
            bool filterType = FilterType;

            IEnumerable<LogItem> range = Logs.ToList();
            if (filterLevel)
            {
                var levels = LevelConfigDic!.Values.Where(i => i.Show).Select(i => i.Code).ToArray();
                range = range.Where(i => levels.Contains(i.Level));
            }
            if (filterType)
            {
                var types = TypeConfigDic!.Values.Where(i => i.Show).Select(i => i.Code).ToArray();
                range = range.Where(i => types.Contains(i.Type));
            }

            LogsAfterFilter.Clear();
            foreach (var item in range)
            {
                LogsAfterFilter.Enqueue(item);
            }
            PagingBox.CurrentIndex = 1;
        }
        private void _showNewLogs()
        {
            int addShow = 0;
            while (WaitDeal.TryDequeue(out LogItem? item))
            {
                if (item == null) continue;
                LevelConfig? levelConfig = LevelConfigDic.ContainsKey(item.Level ?? "") ? LevelConfigDic[item.Level ?? ""] : null;
                if (LevelCheck && levelConfig == null)
                {
                    continue;
                }
                TypeConfig? typeConfig = TypeConfigDic.ContainsKey(item.Type ?? "") ? TypeConfigDic[item.Type ?? ""] : null;
                if (TypeCheck && typeConfig == null)
                {
                    continue;
                }
                item.LevelName = levelConfig?.Name ?? item.Level;
                item.TypeName = typeConfig?.Name ?? item.Type;

                bool filter;
                #region 过滤检查
                //bool filterLevel;
                //if (levelConfig != null)
                //{
                //    filterLevel = !levelConfig.Show;
                //}
                //else
                //{
                //    filterLevel = FilterLevel;
                //}
                //bool filterType;
                //if (typeConfig != null)
                //{
                //    filterType = !typeConfig.Show;
                //}
                //else
                //{
                //    filterType = FilterType;
                //}
                //filter = filterLevel || filterType;
                filter = (levelConfig != null ? !levelConfig.Show : FilterLevel) || (typeConfig != null ? !typeConfig.Show : FilterType);
                #endregion

                Logs.Enqueue(item);
                if (!filter)
                {
                    LogsAfterFilter.Enqueue(item);
                    addShow++;
                }
            }
            if (KeepRefresh && addShow > 0)
            {
                PushOn(addShow);
            }
            while (Logs.Count > MaxLog)
            {
                Logs.Dequeue();
            }
            while (LogsAfterFilter.Count > MaxLog)
            {
                LogsAfterFilter.Dequeue();
            }
            if (KeepRefresh)
            {
                PagingBox.TotalCount = LogsAfterFilter.Count;
                if (ShowingData.Count > 0 && addShow > 0)
                {
                    LogShower.FirstDisplayedScrollingRowIndex = ShowingData.Count - 1;
                }
            }
        }

        /// <summary>
        /// 手动换页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        private void PagingBox_OnPageIndexChanged(int pageIndex, int pageSize)
        {
            SuspendLayout();

            lock (ShowingUpdateLocker)
            {
                ShowingData.Clear();
                var data = LogsAfterFilter.SkipLast(pageSize * (pageIndex - 1)).TakeLast(pageSize).ToList();
                data.ForEach(i => ShowingData.Add(i));
            }

            ResumeLayout();
        }

        /// <summary>
        /// 将当前显示内容向前推荐
        /// </summary>
        /// <param name="count"></param>
        private void PushOn(int count)
        {
            IEnumerable<LogItem> log = LogsAfterFilter.SkipLast(PagingBox.PageSize * (PagingBox.CurrentIndex - 1)).TakeLast(count);//.Reverse();
            foreach (LogItem logItem in log)
            {
                ShowingData.Add(logItem);
            }
            while (ShowingData.Count > PagingBox.PageSize)
            {
                ShowingData.RemoveAt(0);
            }
        }

        /// <summary>
        /// 更新数量统计显示
        /// </summary>
        private void UpdateCountShower()
        {
            CountShower.Text = $"{LogsAfterFilter.Count} / {Logs.Count} / {MaxLog}";
        }


        #endregion

    }
}
