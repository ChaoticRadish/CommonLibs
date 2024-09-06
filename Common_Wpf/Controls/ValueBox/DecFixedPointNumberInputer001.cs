using Common_Util.Data.Structure.Value;
using Common_Util.Extensions;
using Common_Util.Interfaces.UI;
using Common_Wpf.Extensions;
using Common_Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Common_Wpf.Controls.ValueBox
{
    public class DecFixedPointNumberInputer001 : MbControl01, IDecFixedPointNumber, ITextShower, IReadOnlySwtich
    {

        static DecFixedPointNumberInputer001()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(typeof(DecFixedPointNumberInputer001)));

            HorizontalAlignmentProperty.OverrideMetadata(
                typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(
                typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }
        public DecFixedPointNumberInputer001() 
        { 
        }

        #region 设置
        /// <summary>
        /// 没有输入或输入空值时自动填入的值
        /// </summary>
        public DecFixedPointNumber? EmptyValue
        {
            get { return (DecFixedPointNumber?)GetValue(EmptyValueProperty); }
            set { SetValue(EmptyValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EmptyValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmptyValueProperty =
            DependencyProperty.Register("EmptyValue", typeof(DecFixedPointNumber?), typeof(DecFixedPointNumberInputer001),
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue != null && inputer.CurrentText.IsEmpty())
                            {
                                inputer.SetCurrentValue(CurrentTextProperty, inputer.EmptyValue.ToString());
                            }

                        }
                    }
                });



        /// <summary>
        /// 整数部分的最大长度
        /// </summary>
        public int? MaxIntegerLength
        {
            get { return (int?)GetValue(MaxIntegerLengthProperty); }
            set { SetValue(MaxIntegerLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxIntegerLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxIntegerLengthProperty =
            DependencyProperty.Register("MaxIntegerLength", typeof(int?), typeof(DecFixedPointNumberInputer001), 
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue is int newValue)
                            {
                                if (newValue < 1)
                                {
                                    inputer.SetCurrentValue(MaxIntegerLengthProperty, 1);
                                    return;
                                }

                                if (inputer.Value != null)
                                {
                                    var currentValue = inputer.Value.Value;
                                    if (!currentValue.LengthSmallThen(inputer.MaxIntegerLength, inputer.MaxDemicalLength))
                                    {
                                        currentValue = currentValue.LimitMaxLength(inputer.MaxIntegerLength, inputer.MaxDemicalLength,
                                            Common_Util.Enums.HeadTailEnum.Head, Common_Util.Enums.HeadTailEnum.Head);
                                        inputer.SetCurrentValue(ValueProperty, currentValue);
                                    }
                                }
                            }
                        }
                    },
                });


        /// <summary>
        /// 小数部分的最大长度
        /// </summary>
        public int? MaxDemicalLength
        {
            get { return (int?)GetValue(MaxDemicalLengthProperty); }
            set { SetValue(MaxDemicalLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxDemicalLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxDemicalLengthProperty =
            DependencyProperty.Register("MaxDemicalLength", typeof(int?), typeof(DecFixedPointNumberInputer001), 
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue is int newValue)
                            {
                                if (newValue < 0)
                                {
                                    inputer.SetCurrentValue(MaxDemicalLengthProperty, 0);
                                    return;
                                }

                                if (inputer.Value != null)
                                {
                                    var currentValue = inputer.Value.Value;
                                    if (!currentValue.LengthSmallThen(inputer.MaxIntegerLength, inputer.MaxDemicalLength))
                                    {
                                        currentValue = currentValue.LimitMaxLength(inputer.MaxIntegerLength, inputer.MaxDemicalLength,
                                            Common_Util.Enums.HeadTailEnum.Head, Common_Util.Enums.HeadTailEnum.Head);
                                        inputer.SetCurrentValue(ValueProperty, currentValue);
                                    }
                                }
                            }
                        }
                    },
                });



        #endregion


        public DecFixedPointNumber? ShowingValue
        {
            get { return _showingValue; }
            set
            {
                SetCurrentValue(CurrentTextProperty, value?.ToString() ?? string.Empty);
            }
        }
        private DecFixedPointNumber? _showingValue;



        /// <summary>
        /// 取得或设置当前输入值
        /// </summary>
        public DecFixedPointNumber? Value
        {
            get { return (DecFixedPointNumber?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DecFixedPointNumber?), typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue == null)
                            {
                                inputer.SetCurrentValue(CurrentTextProperty, string.Empty);
                            }
                            else if (e.NewValue is DecFixedPointNumber num)
                            {
                                if (!num.LengthSmallThen(inputer.MaxIntegerLength, inputer.MaxDemicalLength))
                                {
                                    num = num.LimitMaxLength(inputer.MaxIntegerLength, inputer.MaxDemicalLength, 
                                        Common_Util.Enums.HeadTailEnum.Head, Common_Util.Enums.HeadTailEnum.Head);
                                }
                                inputer._showingValue = num;
                                
                                if (DecFixedPointNumber.TryPasue(inputer.CurrentText, out var current))
                                {
                                    if (current != num)
                                    {
                                        inputer.SetCurrentValue(CurrentTextProperty, num.ToString());
                                    }
                                }
                                else
                                {
                                    inputer.SetCurrentValue(CurrentTextProperty, num.ToString());
                                }
                            }
                            inputer.OnPropertyChanged(nameof(Value));
                        }
                    },
                    DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
                });


        public string? ShowingText
        {
            get { return (string?)GetValue(ShowingTextProperty); }
            set { SetValue(ShowingTextProperty, value ?? string.Empty); }
        }

        // Using a DependencyProperty as the backing store for ShowingText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowingTextProperty =
            DependencyProperty.Register("ShowingText", typeof(string), typeof(DecFixedPointNumberInputer001),
                new PropertyMetadata(string.Empty)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue is string str)
                            {
                                inputer.SetCurrentValue(CurrentTextProperty, str);
                            }
                        }
                    }
                });


        /// <summary>
        /// 当前输入文本
        /// </summary>
        internal string CurrentText
        {
            get { return (string)GetValue(CurrentTextProperty); }
            set
            {
                SetValue(CurrentTextProperty, value);
                OnPropertyChanged(nameof(CurrentText));
            }
        }


        // Using a DependencyProperty as the backing store for CurrentText.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty CurrentTextProperty =
            DependencyProperty.Register("CurrentText", typeof(string), typeof(DecFixedPointNumberInputer001),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is DecFixedPointNumberInputer001 inputer)
                        {
                            if (e.NewValue is string str)
                            {
                                inputer.SetCurrentValue(ShowingTextProperty, str);

                                if (str.IsEmpty())
                                {
                                    inputer.SetCurrentValue(ValueProperty, inputer.EmptyValue);
                                    if (inputer.EmptyValue != null)
                                    {
                                        inputer.SetCurrentValue(CurrentTextProperty, inputer.EmptyValue.ToString());
                                        //inputer.CurrentText = inputer.EmptyValue.Value.ToString();
                                        inputer.SetCurrentValue(ShowingTextProperty, inputer.EmptyValue.ToString());
                                    }
                                    else
                                    {
                                        inputer.SetCurrentValue(ValueProperty, null);
                                    }
                                }
                                else if (str.Trim().Equals("-") || str.Trim().Equals("+") || str.Trim().Equals("."))
                                {
                                }
                                else
                                {

                                    if (DecFixedPointNumber.TryPasue(str, out var temp))
                                    {
                                        if (temp != inputer.Value)
                                        {
                                            inputer.SetCurrentValue(ValueProperty, temp);
                                        }
                                    }
                                    else
                                    {
                                        if (e.OldValue is string old)
                                        {
                                            old = old.Trim();
                                            inputer.SetCurrentValue(CurrentTextProperty, old);
                                            inputer.SetCurrentValue(ShowingTextProperty, old);
                                        }
                                    }
                                }
                            }
                        }
                    },
                    DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
                });



        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(DecFixedPointNumberInputer001), new PropertyMetadata(false));



        #region 重载 (初始化相关)
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var textbox = this.GetFirstChildObject<TextBox>();
            if (textbox == null)
            {
                throw new Exception("找不到文本输入框 (TextBox)");
            }
            textbox.LostFocus += Textbox_LostFocus;
        }

        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            CurrentText = Value?.ToString() ?? string.Empty;
        }
        #endregion
    }
}
