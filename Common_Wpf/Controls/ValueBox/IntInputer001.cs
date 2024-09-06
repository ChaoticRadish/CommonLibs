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
    public class IntInputer001 : MbControl01, IIntShower, ITextShower, IReadOnlySwtich
    {
        static IntInputer001()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(IntInputer001), new FrameworkPropertyMetadata(typeof(IntInputer001)));

            HorizontalAlignmentProperty.OverrideMetadata(
                typeof(IntInputer001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(
                typeof(IntInputer001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(IntInputer001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(IntInputer001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }
        public IntInputer001()
        {
        }


        /// <summary>
        /// 没有输入或输入空值时自动填入的值
        /// </summary>
        public int? EmptyValue
        {
            get { return (int?)GetValue(EmptyValueProperty); }
            set { SetValue(EmptyValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EmptyValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmptyValueProperty =
            DependencyProperty.Register("EmptyValue", typeof(int?), typeof(IntInputer001),
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is IntInputer001 inputer)
                        {
                            if (e.NewValue != null && inputer.CurrentText.IsEmpty())
                            {
                                inputer.SetValue(CurrentTextProperty, inputer.EmptyValue.ToString());
                            }

                        }
                    }
                });

        public int? ShowingValue
        {
            get { return _showingValue; }
            set
            {
                SetCurrentValue(CurrentTextProperty, value?.ToString() ?? string.Empty);
            }
        }
        private int? _showingValue;


        /// <summary>
        /// 取得或设置当前输入值
        /// </summary>
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(IntInputer001),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is IntInputer001 inputer)
                        {
                            if (e.NewValue == null)
                            {
                                inputer.SetCurrentValue(CurrentTextProperty, string.Empty);
                            }
                            else if (e.NewValue is int v)
                            {
                                inputer.SetCurrentValue(CurrentTextProperty, v.ToString());
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
            DependencyProperty.Register("ShowingText", typeof(string), typeof(IntInputer001),
                new PropertyMetadata(string.Empty)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is IntInputer001 inputer)
                        {
                            if (e.NewValue is string str)
                            {
                                inputer.SetValue(CurrentTextProperty, str);
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
            DependencyProperty.Register("CurrentText", typeof(string), typeof(IntInputer001),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is IntInputer001 inputer)
                        {
                            if (e.NewValue is string str)
                            {
                                inputer.SetCurrentValue(ShowingTextProperty, str);

                                int temp;
                                if (str.IsEmpty())
                                {
                                    inputer._showingValue = inputer.EmptyValue;
                                    inputer.SetCurrentValue(ValueProperty, inputer.EmptyValue);
                                    if (inputer.EmptyValue != null)
                                    {
                                        inputer.SetCurrentValue(CurrentTextProperty, inputer.EmptyValue.ToString());
                                        inputer.SetCurrentValue(ShowingTextProperty, inputer.EmptyValue.ToString());
                                    }
                                    else
                                    {
                                        inputer.SetCurrentValue(ValueProperty, null);
                                    }
                                }
                                else if (str.Trim().Equals("-") || str.Trim().Equals("+"))
                                {
                                }
                                else if (int.TryParse(str, out temp))
                                {
                                    inputer._showingValue = temp;
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
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(IntInputer001), new PropertyMetadata(false));


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
