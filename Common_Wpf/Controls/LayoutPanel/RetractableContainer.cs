using Common_Util;
using Common_Util.Attributes.General;
using Common_Util.Data.Enums;
using Common_Util.Log;
using Common_Util.Module.Command;
using Common_Wpf.Controls.FeatureGroup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Common_Wpf.Controls.LayoutPanel
{
    /// <summary>
    /// 带有可收起区域的容器, 内容默认放在主体区域, 可收起区域需设置 <see cref="RetractableAreaContent"/>. 
    /// <para>主体区域宽度为1*, 可收起区域宽度不支持auto</para>
    /// </summary>
    public class RetractableContainer : MbContentControl01
    {
        static RetractableContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RetractableContainer),
                new FrameworkPropertyMetadata(typeof(RetractableContainer)));
        }

        public RetractableContainer()
        {
            State.PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(UiState.RetractScale):
                        SetCurrentValue(RetractScaleProperty, State.RetractScale);
                        break;
                }
                OnPropertyChanged(nameof(State));
            };
        }

        


        #region 配置


        /// <summary>
        /// 可收起区域的展开宽度
        /// </summary>
        public GridLength RetractableAreaWidth
        {
            get { return (GridLength)GetValue(RetractableAreaWidthProperty); }
            set { SetValue(RetractableAreaWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RetractableAreaWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RetractableAreaWidthProperty =
            DependencyProperty.Register(
                nameof(RetractableAreaWidth), 
                typeof(GridLength), 
                typeof(RetractableContainer), 
                new PropertyMetadata(new GridLength(1, GridUnitType.Star))
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is RetractableContainer container)
                        {
                            if (e.NewValue is GridLength gridLength)
                            {
                                if (gridLength.IsAuto)
                                {
                                    throw new ValidationException($"可收起区域的宽度不支持 auto ! ");
                                }
                            }
                        }
                    }
                });




        public FourWayEnum RetractableAreaLocation 
        {
            get { return (FourWayEnum)GetValue(RetractableAreaLocationProperty); }
            set { SetValue(RetractableAreaLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RetractableAreaLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RetractableAreaLocationProperty =
            DependencyProperty.Register(
                nameof(RetractableAreaLocation), 
                typeof(FourWayEnum), 
                typeof(RetractableContainer),
                new PropertyMetadata(FourWayEnum.Left)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is RetractableContainer container)
                        {
                            if (e.NewValue is FourWayEnum location)
                            {
                            }
                        }
                    }
                });



        /// <summary>
        /// 展开/收起动画的耗时 (秒)
        /// </summary>
        public double RetractableUsingTime
        {
            get { return (double)GetValue(RetractableUsingTimeProperty); }
            set { SetValue(RetractableUsingTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RetractableUsingTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RetractableUsingTimeProperty =
            DependencyProperty.Register(nameof(RetractableUsingTime), typeof(double), typeof(RetractableContainer), new PropertyMetadata((double)1));





        #endregion

        #region 状态数据

        public UiState State { get; } = new();

        public class UiState : INotifyPropertyChanged
        {

            public SwitchState01Enum RetractState
            {
                get 
                {
                    if (retractScale == 0)
                    {
                        return SwitchState01Enum.Close;
                    }
                    else if (retractScale == 1)
                    {
                        return SwitchState01Enum.Open;
                    }
                    else
                    {
                        return SwitchState01Enum.Changing;
                    }
                }
            }

            public double RetractScale
            {
                get { return retractScale; }
                set 
                {
                    var oldState = RetractState;
                    retractScale = Math.Clamp(value, 0, 1);
                    if (RetractState != oldState)
                    {
                        OnPropertyChanged(nameof(RetractState));
                    }
                    OnPropertyChanged();
                }
            }
            private double retractScale = 1;






            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        /// <summary>
        /// 可收起区域当前的展开比例
        /// </summary>
        internal double RetractScale
        {
            get { return (double)GetValue(RetractScaleProperty); }
            set { SetValue(RetractScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RetractScale.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty RetractScaleProperty =
            DependencyProperty.Register("RetractScale", typeof(double), typeof(RetractableContainer), new PropertyMetadata((double)1)
            {
                PropertyChangedCallback = (s, e) =>
                {
                    if (s is RetractableContainer container)
                    {
                        if (e.NewValue is double d && d != container.State.RetractScale)
                        {
                            container.State.RetractScale = d;
                        }
                    }
                }
            });

        #endregion

        #region 日志



        /// <summary>
        /// 用于测试的日志输出器
        /// </summary>
        public ILevelLogger? TestLogger
        {
            get { return (ILevelLogger?)GetValue(TestLoggerProperty); }
            set { SetValue(TestLoggerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TestLogger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestLoggerProperty =
            DependencyProperty.Register("TestLogger", typeof(ILevelLogger), typeof(RetractableContainer), new PropertyMetadata(null));



        #endregion

        #region 指令
        public ICommand RetractCommand => new SampleCommand(_ => Retract(), _ => true);
        public ICommand ExpandCommand => new SampleCommand(_ => Expand(), _ => true);
        #endregion

        #region 操作
        /// <summary>
        /// 收起可收起区域
        /// </summary>
        public void Retract()
        {
            if (State.RetractState != SwitchState01Enum.Open)
            {
                TestLogger?.Debug($"未执行收起, 当前状态: {State.RetractState.GetDesc()}");
                return;
            }
            TestLogger?.Debug("收起可收起区域");
            DoubleAnimation doubleAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = new(TimeSpan.FromSeconds(RetractableUsingTime))
            };
            this.BeginAnimation(RetractScaleProperty, doubleAnimation);
        }
        /// <summary>
        /// 展开可收起区域
        /// </summary>
        public void Expand()
        {
            if (State.RetractState != SwitchState01Enum.Close)
            {
                TestLogger?.Debug($"未执行展开, 当前状态: {State.RetractState.GetDesc()}");
                return;
            }
            TestLogger?.Debug("展开可收起区域");
            DoubleAnimation doubleAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new(TimeSpan.FromSeconds(RetractableUsingTime))
            };
            this.BeginAnimation(RetractScaleProperty, doubleAnimation);
        }
        #endregion

        #region 内容模板

        /// <summary>
        /// 可收起区域的模板
        /// </summary>
        public ContentControl? RetractableAreaContent
        {
            get { return (ContentControl?)GetValue(RetractableAreaContentProperty); }
            set { SetValue(RetractableAreaContentProperty, value); }
        }


        // Using a DependencyProperty as the backing store for RetractableAreaContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RetractableAreaContentProperty =
            DependencyProperty.Register(
                nameof(RetractableAreaContent), 
                typeof(ContentControl), 
                typeof(RetractableContainer), 
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                    }
                });





        /// <summary>
        /// 收起按钮的模板
        /// </summary>
        public ControlTemplate? RetractButtonTemplate
        {
            get { return (ControlTemplate?)GetValue(RetractButtonTemplateProperty); }
            set { SetValue(RetractButtonTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RetractButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RetractButtonTemplateProperty =
            DependencyProperty.Register(
                nameof(RetractButtonTemplate), 
                typeof(ControlTemplate), 
                typeof(RetractableContainer),
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is RetractableContainer container)
                        {
                            if (e.NewValue is ControlTemplate template)
                            {
                                var obj = template.LoadContent();
                                if (obj is FrameworkElement element)
                                {
                                    element.DataContext = container;
                                }
                            }
                        }
                    }
                });






        #endregion

        #region 区域枚举
        public enum AreaEnum : int
        {
            RetractableArea = 0,
            ButtonArea = 1,
            BodyArea = 2,
        }
        #endregion


    }

    internal static class RetractableContainerHelper
    {
        public static (RetractableContainer.UiState state, int index, FourWayEnum areaLocation, GridLength settingWidth) _convertInputValue_state_index_areaLocation_settingWidth(object[] values)
        {
            if (values.Length < 4) { throw new Exception("传入参数不足! "); }
            // if (values[0] is not UiState state) throw new Exception($"参数[0]不是 {typeof(UiState)}");
            if (values[0] is RetractableContainer.UiState state) { }
            else
            {
                state = new() { RetractScale = 1 };
            }
            // if (values[1] is not int index) throw new Exception($"参数[1]不是 {typeof(int)}");
            if (values[1] is int index) { }
            else if (values[1] is string indexStr && int.TryParse(indexStr, out index)) { }
            else throw new Exception($"参数[1]不是 {typeof(int)}");
            // if (values[2] is not FourWayEnum areaLocatiion) throw new Exception($"参数[2]不是 {typeof(FourWayEnum)}");
            if (values[2] is FourWayEnum areaLocation) { }
            else areaLocation = FourWayEnum.Left;
            if (values[3] is not GridLength settingWidth) throw new Exception($"参数[3]不是 {typeof(GridLength)}");
            return (state, index, areaLocation, settingWidth);
        }
        public static FourWayEnum _convertInputValue_areaLocation(object value)
        {
            if (value is FourWayEnum areaLocatiion) return areaLocatiion;
            else return FourWayEnum.Left;
        }

        public static (RetractableContainer.AreaEnum area, FourWayEnum areaLocation) _convertInputValue_area_areaLocation(object[] values)
        {
            if (values.Length < 2) { throw new Exception("传入参数不足! "); }
            if (values[0] is RetractableContainer.AreaEnum area) { }
            else if (values[0] is string areaString && Common_Util.EnumHelper.TryConvert(areaString, out area)) { }
            else throw new Exception($"参数[0]不是 {typeof(RetractableContainer.AreaEnum)}");
            if (values[1] is FourWayEnum areaLocation) { }
            else areaLocation = FourWayEnum.Left;
            return (area, areaLocation);
        }
    }

    internal class RetractableContainerGridCalcRowSpanConverter : IValueConverter
    {
        /// <summary>
        /// 需传入参数: 当前可收起区域所在的位置
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return RetractableContainerHelper._convertInputValue_areaLocation(value) switch
            {
                FourWayEnum.Up => 1,
                FourWayEnum.Down => 1,
                FourWayEnum.Left => 3,
                FourWayEnum.Right => 3,
                _ => throw new ValidationException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class RetractableContainerGridCalcColumnSpanConverter : IValueConverter
    {
        /// <summary>
        /// 需传入参数: 当前可收起区域所在的位置
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return RetractableContainerHelper._convertInputValue_areaLocation(value) switch
            {
                FourWayEnum.Up => 3,
                FourWayEnum.Down => 3,
                FourWayEnum.Left => 1,
                FourWayEnum.Right => 1,
                _ => throw new ValidationException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    internal class RetractableContainerGridCalcRowHeightConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入 [0]:当前Ui状态; [1]:行索引; [2]:当前可收起区域所在的位置; [3]:当前设定的可收起区域的宽度
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var (state, index, areaLocatiion, settingWidth) = RetractableContainerHelper._convertInputValue_state_index_areaLocation_settingWidth(values);

            int retractableIndex;
            switch (areaLocatiion)
            {
                case FourWayEnum.Up:
                    retractableIndex = 0;
                    break;
                case FourWayEnum.Down:
                    retractableIndex = 2;
                    break;
                case FourWayEnum.Left:
                case FourWayEnum.Right:
                    return new GridLength(1, GridUnitType.Star);
                default:
                    throw new ValidationException();
            }
            if (index == 1)  // 按钮
            {
                return GridLength.Auto;
            }
            if (index == retractableIndex)   // 可收起区域
            {
                return new GridLength(state.RetractScale * settingWidth.Value, settingWidth.GridUnitType);
            }
            else    // 主体区域
            {
                return new GridLength(1, GridUnitType.Star);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class RetractableContainerGridCalcColumnWidthConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入 [0]:当前Ui状态; [1]:列索引; [2]:当前可收起区域所在的位置; [3]:当前设定的可收起区域的宽度
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var (state, index, areaLocatiion, settingWidth) = RetractableContainerHelper._convertInputValue_state_index_areaLocation_settingWidth(values);

            int retractableIndex;
            switch (areaLocatiion)
            {
                case FourWayEnum.Left:
                    retractableIndex = 0;
                    break;
                case FourWayEnum.Right:
                    retractableIndex = 2;
                    break;
                case FourWayEnum.Up:
                case FourWayEnum.Down:
                    return new GridLength(1, GridUnitType.Star);
                default:
                    throw new ValidationException();
            }
            if (index == 1)  // 按钮
            {
                return GridLength.Auto;
            }
            if (index == retractableIndex)   // 可收起区域
            {
                return new GridLength(state.RetractScale * settingWidth.Value, settingWidth.GridUnitType);
            }
            else    // 主体区域
            {
                return new GridLength(1, GridUnitType.Star);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    internal class RetractableContainerGridCalcRowIndexConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入 [0]:区域; [1]:当前可收起区域所在的位置;
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var (area, areaLocatiion) = RetractableContainerHelper._convertInputValue_area_areaLocation(values);

            switch (areaLocatiion)
            {
                case FourWayEnum.Up:
                    return area switch
                    {
                        RetractableContainer.AreaEnum.RetractableArea => 0,
                        RetractableContainer.AreaEnum.ButtonArea => 1,
                        RetractableContainer.AreaEnum.BodyArea => 2,
                        _ => throw new ValidationException()
                    };
                case FourWayEnum.Down:
                    return area switch
                    {
                        RetractableContainer.AreaEnum.RetractableArea => 2,
                        RetractableContainer.AreaEnum.ButtonArea => 1,
                        RetractableContainer.AreaEnum.BodyArea => 0,
                        _ => throw new ValidationException()
                    };
                case FourWayEnum.Left:
                case FourWayEnum.Right:
                    return 0;
                default:
                    throw new ValidationException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class RetractableContainerGridCalcColumnIndexConverter : IMultiValueConverter
    {
        /// <summary>
        /// 需传入 [0]:区域; [1]:当前可收起区域所在的位置;
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var (area, areaLocatiion) = RetractableContainerHelper._convertInputValue_area_areaLocation(values);

            switch (areaLocatiion)
            {
                case FourWayEnum.Left:
                    return area switch
                    {
                        RetractableContainer.AreaEnum.RetractableArea => 0,
                        RetractableContainer.AreaEnum.ButtonArea => 1,
                        RetractableContainer.AreaEnum.BodyArea => 2,
                        _ => throw new ValidationException()
                    };
                case FourWayEnum.Right:
                    return area switch
                    {
                        RetractableContainer.AreaEnum.RetractableArea => 2,
                        RetractableContainer.AreaEnum.ButtonArea => 1,
                        RetractableContainer.AreaEnum.BodyArea => 0,
                        _ => throw new ValidationException()
                    };
                case FourWayEnum.Up:
                case FourWayEnum.Down:
                    return 0;
                default:
                    throw new ValidationException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
