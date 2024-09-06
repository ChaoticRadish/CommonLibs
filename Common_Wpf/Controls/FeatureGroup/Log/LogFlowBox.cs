using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Common_Wpf.Controls.FeatureGroup
{
    public class LogFlowBox : Control, INotifyPropertyChanged, ILogger
    {
        static LogFlowBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LogFlowBox), 
                new FrameworkPropertyMetadata(typeof(LogFlowBox)));
        }

        public LogFlowBox()
        {
            _initAndStartTimer();

            DataContext = this;
        }

        #region 配置

        /// <summary>
        /// 日志容量
        /// </summary>
        public int Capacity
        {
            get { return (int)GetValue(CapacityProperty); }
            set { SetValue(CapacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Capacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register("Capacity", typeof(int), typeof(LogFlowBox), new PropertyMetadata(5000));


        public const double DEAL_PERIOD = 0.05;

        /// <summary>
        /// 每个周期 ( <see cref="DEAL_PERIOD"/> 秒) 处理的日志数量
        /// </summary>
        public int DealOneTime
        {
            get { return (int)GetValue(DealOneTimeProperty); }
            set { SetValue(DealOneTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DealOneTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DealOneTimeProperty =
            DependencyProperty.Register("DealOneTime", typeof(int), typeof(LogFlowBox), new PropertyMetadata(100));


        #endregion

        #region 接口实现: INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 接口实现: ILogger

        /// <summary>
        /// 将日志数据记录到
        /// </summary>
        /// <param name="log"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Log(LogData log)
        {
            if (Config.IsNeedOutput(log))
            {
                WaitDealDatas.Enqueue(log);
            }
        }


        #region 日志配置

        public LogOutputConfig<LogConfigItem> Config { get; private set; } = new LogOutputConfig<LogConfigItem>();


        /// <summary>
        /// 是否默认输出所有日志, 最终设置属性 => <see cref="LogOutputConfig{TConfigItem}.AllOutput"/>
        /// </summary>
        public bool AllLogOutput
        {
            get { return (bool)GetValue(AllLogOutputProperty); }
            set { SetValue(AllLogOutputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllLogOutput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllLogOutputProperty =
            DependencyProperty.Register("AllLogOutput", typeof(bool), typeof(LogFlowBox), 
                new PropertyMetadata(true, 
                    propertyChangedCallback: (d, e) =>
                    {
                        if (d is LogFlowBox box)
                        {
                            box.Config.AllOutput = (bool)e.NewValue;
                        }
                    }));


        /// <summary>
        /// 默认配置项, 最终设置属性 => <see cref="LogOutputConfig{TConfigItem}.DefaultConfigItem"/>
        /// </summary>
        public LogConfigItem DefaultConfigItem
        {
            get { return (LogConfigItem)GetValue(DefaultConfigItemProperty); }
            set { SetValue(DefaultConfigItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultConfigItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultConfigItemProperty =
            DependencyProperty.Register("DefaultConfigItem", typeof(LogConfigItem), typeof(LogFlowBox), 
                new PropertyMetadata(null,
                    propertyChangedCallback: (d, e) =>
                    {
                        if (d is LogFlowBox box)
                        {
                            box.Config.DefaultConfigItem = (LogConfigItem)e.NewValue;
                        }
                    }));



        /// <summary>
        /// 日志配置项, 最终设置属性 => <see cref="LogOutputConfig{TConfigItem}.ConfigItems"/>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogConfigCollect ConfigItems
        {
            get { return (LogConfigCollect)GetValue(ConfigItemsProperty); }
            set { SetValue(ConfigItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConfigItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigItemsProperty =
            DependencyProperty.Register("ConfigItems", typeof(LogConfigCollect), typeof(LogFlowBox), 
                new PropertyMetadata(
                    // new LogConfigCollect(),
                    propertyChangedCallback: (d, e) =>
                    {
                        if (d is LogFlowBox box)
                        {
                            box.Config.ConfigItems = ((LogConfigCollect)e.NewValue).ToArray();
                        }
                    }));



        /// <summary>
        /// 忽略配置项, 最终设置属性 => <see cref="LogOutputConfig{TConfigItem}.Ignore"/>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogConfigCollect Ignore
        {
            get { return (LogConfigCollect)GetValue(IgnoreProperty); }
            set { SetValue(IgnoreProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ignore.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IgnoreProperty =
            DependencyProperty.Register("Ignore", typeof(LogConfigCollect), typeof(LogFlowBox), 
                new PropertyMetadata(
                    // new LogConfigCollect(),
                    propertyChangedCallback: (d, e) =>
                    {
                        if (d is LogFlowBox box)
                        {
                            box.Config.Ignore = ((LogConfigCollect)e.NewValue).ToArray();
                        }
                    }));





        #endregion

        #endregion

        #region 数据

        /// <summary>
        /// 缓存数据
        /// </summary>
        private Queue<LogData> WaitDealDatas = new Queue<LogData>();





        internal ObservableCollection<LogFlowBoxModel> ShowingData
        {
            get { return (ObservableCollection<LogFlowBoxModel>)GetValue(ShowingDataProperty); }
            set { SetValue(ShowingDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowingData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowingDataProperty =
            DependencyProperty.Register("ShowingData", typeof(ObservableCollection<LogFlowBoxModel>), 
                typeof(LogFlowBox), 
                new PropertyMetadata(new ObservableCollection<LogFlowBoxModel>()));


        /// <summary>
        /// 移除超出容量的部分
        /// </summary>
        /// <returns>移除数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RemoveOverCapacity()
        {
            int count = 0;
            while (ShowingData.Count > 0 && ShowingData.Count > Capacity)
            {
                ShowingData.RemoveAt(0);
                count++;
            }
            return count;
        }

        /// <summary>
        /// 追加日志内容
        /// </summary>
        /// <param name="log"></param>
        protected virtual void AppendLog(LogData log)
        {
            var config = Config.GetConfigItem(log);
            ShowingData.Add(new LogFlowBoxModel(log)
            {
                CustomBackColor = config?.BackColor,
                CustomForeColor = config?.ForeColor,
                CustomBorderColor = config?.BorderColor,
            });
        }

        /// <summary>
        /// 触发当前显示的数据变更
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void TriggerShowingDataChange()
        {
            OnPropertyChanged(nameof(ShowingData));
        }

        #endregion

        #region 定时器
        private DispatcherTimer _dealTimer = new DispatcherTimer();
        private void _initAndStartTimer()
        {
            _dealTimer.Interval = TimeSpan.FromSeconds(DEAL_PERIOD);
            _dealTimer.Tick += _dealTimer_Tick;
            _dealTimer.Start();
        }

        private readonly object _dealLocker = new object();
        private void _dealTimer_Tick(object? sender, EventArgs e)
        {
            if (!this.IsEnabled) return;
            lock (_dealLocker)
            {
                int currentAll = WaitDealDatas.Count;
                int dealThisTime = currentAll / 10; // 本周期需要输出的日志数量
                if (dealThisTime > DealOneTime)
                {
                    dealThisTime = DealOneTime;
                }
                else if (dealThisTime <= 0)
                {
                    dealThisTime = 1;
                }

                // 更新列表
                Dispatcher.Invoke(() =>
                {
                    int index = 0;
                    int remove = 0;
                    while (index < dealThisTime
                        && WaitDealDatas.TryDequeue(out LogData? logData)
                        && logData != null)
                    {
                        AppendLog(logData);
                        remove += RemoveOverCapacity();

                        index++;
                    }
                    if (index + remove > 0)
                    {
                        TriggerShowingDataChange();
                    }
                });
            }
        }
        #endregion


    }

    public class LogConfigCollect : List<LogConfigItem>, ICollection<LogConfigItem>
    {

    }
    public class LogConfigItem : LogOutputConfigItemBase
    {
        public Brush? BackColor { get; set; }
        //public Brush BackColorBrush 
        //{
        //    get
        //    {
        //        if (_backColorBrush == null)
        //        {
        //            _backColorBrush = new SolidColorBrush(BackColor);
        //        }
        //        return _backColorBrush;
        //    } 
        //}
        //private Brush? _backColorBrush;

        public Brush? BorderColor { get; set; }
        //public Brush BorderColorBrush
        //{
        //    get
        //    {
        //        if (_borderColorBrush == null)
        //        {
        //            _borderColorBrush = new SolidColorBrush(BackColor);
        //        }
        //        return _borderColorBrush;
        //    }
        //}
        //private Brush? _borderColorBrush;

        public Brush? ForeColor { get; set; }
        //public Brush ForeColorBrush
        //{
        //    get
        //    {
        //        if (_foreColorBrush == null)
        //        {
        //            _foreColorBrush = new SolidColorBrush(BackColor);
        //        }
        //        return _foreColorBrush;
        //    }
        //}
        //private Brush? _foreColorBrush;
    }
}
