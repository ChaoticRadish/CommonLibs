using Common_Util.Log;
using Common_Wpf.CommonViewModel;
using Common_Wpf.Controls.LayoutPanel;
using Common_Wpf.Extensions;
using Common_Wpf.SettingPackage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Common_Wpf.Controls.FeatureGroup
{
    /// <summary>
    /// 简单的日志显示器, 默认样式下, 会以文字形式显示日志, 可设定日志显示上限, 不同级别对应文本颜色, 是否显示时间信息等
    /// </summary>
    public class SimpleLogShower : MbContentControl01, ILogger
    {
        static SimpleLogShower()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SimpleLogShower),
                new FrameworkPropertyMetadata(typeof(SimpleLogShower)));
        }

        private readonly Guid Id = Guid.NewGuid();

        public SimpleLogShower()
        {
            ViewModel = new()
            {
                RunInUiThread = Dispatcher.Invoke,
                ShowLastData = MoveToEnd,
            };
        }

        public SinglePageLogShower ViewModel { get; set; }

        #region 操作

        public void Log(LogData log)
        {
            ViewModel.Log(log);
        }
        #endregion

        #region 控件日志



        public ILevelLogger? TestLogger
        {
            get { return (ILevelLogger)GetValue(TestLoggerProperty); }
            set { SetValue(TestLoggerProperty, value); }
        }



        // Using a DependencyProperty as the backing store for TestLogger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestLoggerProperty =
            DependencyProperty.Register(
                nameof(TestLogger), typeof(ILevelLogger), typeof(SimpleLogShower), 
                new PropertyMetadata(null));



        #endregion

        #region 移动到底部
        private const string _logListBoxName = "LogShower";
        private void MoveToEnd()
        {
            //string key = $"{nameof(SimpleLogShower)}_{Id}_{nameof(MoveToEnd)}";
            //Common_Util.Module.Scheduling.MinTimeGapHelper.Do(
            //    key, 
            //    () => 
            //    {
            //        Dispatcher.BeginInvoke(() =>
            //        {
            //            MoveToEndImpl();
            //        });
            //    }, 
            //    TimeSpan.FromSeconds(0.05));
            Dispatcher.Invoke(() =>
            {
                MoveToEndImpl();
            });
        }
        private bool isMoving = false;
        private readonly object movingLocker = new();
        private void MoveToEndImpl()
        {
            if (isMoving) return;
            lock (movingLocker)
            {
                if (isMoving) return;

                isMoving = true;

                try
                {
                    var findResult = this.GetFirstChildObject<ListBox>(_logListBoxName);
                    if (findResult != null)
                    {
                        var scrollViewer = findResult.GetFirstChildObject<ScrollViewer>();
                        scrollViewer?.ScrollToBottom();
                    }
                    else
                    {
                        TestLogger?.Warning($"未找到匹配控件: 名字={_logListBoxName}, 类型={typeof(ListBox)}");
                    }
                }
                catch (Exception ex)
                {
                    TestLogger?.Error("移动到当前显示日志的尾部发生异常", ex);
                }
                finally
                {
                    isMoving = false;
                }
            }
            
        }

        #endregion

        #region 配置


        /// <summary>
        /// 文本是否自动换行
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextWrapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(SimpleLogShower), new PropertyMetadata(TextWrapping.NoWrap));




        public BaseBoxSettingCollection LevelSetting
        {
            get { return (BaseBoxSettingCollection)GetValue(LevelSettingProperty); }
            set { SetValue(LevelSettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LevelSetting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LevelSettingProperty =
            DependencyProperty.Register("LevelSetting", typeof(BaseBoxSettingCollection), typeof(SimpleLogShower),
                new PropertyMetadata(new BaseBoxSettingCollection())
                {
                    PropertyChangedCallback = (sender, e) =>
                    {
                        if (sender is SimpleLogShower shower)
                        {
                            BaseBoxSettingCollection? settings = null;
                            if (e.NewValue is BaseBoxSettingCollection _s)
                            {
                                settings = _s;
                            }
                            if (e.NewValue is BaseBoxSetting[] settingArray)
                            {
                                settings = settingArray;
                            }
                            if (settings != null) 
                            {

                                List<BaseBoxSetting> settingList = new(shower.LevelSetting);
                                bool hasDiff = false;
                                foreach (var setting in shower.LevelSetting)
                                {
                                    if (!settings.Any(i => i.Name == setting.Name))
                                    {
                                        settingList.Remove(setting);
                                        hasDiff = true;
                                    }
                                }
                                foreach (var setting in settings)
                                {
                                    if (!settingList.Any(i => i.Name == setting.Name))
                                    {
                                        settingList.Add(setting);
                                        hasDiff = true;
                                    }
                                    else
                                    {
                                        var exist = settingList.First(i => i.Name == setting.Name);
                                        if (exist != setting)
                                        {
                                            int index = settingList.IndexOf(exist);
                                            settingList[index] = setting;
                                            hasDiff = true;
                                        }
                                    }
                                }
                                if (hasDiff)
                                {
                                    shower.SetCurrentValue(LevelSettingProperty, settingList.ToArray());
                                }
                            }
                        }
                    }
                });


        #endregion

        #region 数据模板

        public DataTemplate? LogItemTemplate
        {
            get { return (DataTemplate?)GetValue(LogItemTemplateProperty); }
            set { SetValue(LogItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogItemTemplateProperty =
            DependencyProperty.Register(
                nameof(LogItemTemplate),
                typeof(DataTemplate),
                typeof(SimpleLogShower),
                new PropertyMetadata(null)
                {
                });

        #endregion

        #region 默认数据模板相关配置



        public LogStringBuilderConfig DefaultTemplateLogStringConfig
        {
            get { return (LogStringBuilderConfig)GetValue(DefaultTemplateLogStringConfigProperty); }
            set { SetValue(DefaultTemplateLogStringConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultTemplateLogStringConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTemplateLogStringConfigProperty =
            DependencyProperty.Register(
                "DefaultTemplateLogStringConfig",
                typeof(LogStringBuilderConfig), 
                typeof(SimpleLogShower), 
                new PropertyMetadata(new LogStringBuilderConfig()));



        #endregion

        #region UI 数据
        private double itemContainerWidth = double.PositiveInfinity;
        public double ItemContainerWidth
        {
            get => itemContainerWidth;
            set
            {
                itemContainerWidth = value;
                if (double.IsNaN(itemContainerWidth))
                {
                    itemContainerWidth = double.PositiveInfinity;
                }
                OnPropertyChanged();
            }
        } 
        #endregion
    }

    /// <summary>
    /// <see cref="LogData"/> => <see cref="string"/>
    /// <para>输入参数: [0]日志数据; [1]生成日志数据的配置;</para>
    /// </summary>
    internal class SimpleLogShowerDefaultLogDataConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) throw new Exception("无输入参数! ");
            if (values[0] is not LogData log) return string.Empty;
            LogStringBuilderConfig? config;
            if (values.Length == 0 || values[1] is not LogStringBuilderConfig) 
            {
                config = new();
            }
            else
            {
                config = (LogStringBuilderConfig)values[1];
            }

            LogStringBuilder builder = new LogStringBuilder()
            {
                Config = config
            };
            return builder.Build(log);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
