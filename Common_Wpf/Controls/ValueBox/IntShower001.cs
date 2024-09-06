using Common_Util.Extensions;
using Common_Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace Common_Wpf.Controls.ValueBox
{
    /// <summary>
    /// Int值的显示控件001
    /// </summary>
    public class IntShower001 : MbControl01, IIntShower, ITextShower
    {

        static IntShower001()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntShower001), new FrameworkPropertyMetadata(typeof(IntShower001)));
            
            HorizontalAlignmentProperty.OverrideMetadata(
                typeof(IntShower001), 
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(
                typeof(IntShower001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(IntShower001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center)); 
            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(IntShower001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }
        public IntShower001()
        {
            //this.DataContext = this;
        }




        public string HeadStr
        {
            get { return (string)GetValue(HeadStrProperty); }
            set { SetValue(HeadStrProperty, value); OnPropertyChanged(nameof(HeadStr)); }
        }

        // Using a DependencyProperty as the backing store for HeadStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadStrProperty =
            DependencyProperty.Register("HeadStr", typeof(string), typeof(IntShower001), 
                new PropertyMetadata(string.Empty,
                    propertyChangedCallback: PropertyValueChangeCallback_TriggerTextChange));





        public string SplitStr
        {
            get { return (string)GetValue(SplitStrProperty); }
            set { SetValue(SplitStrProperty, value); OnPropertyChanged(nameof(SplitStr)); }
        }

        // Using a DependencyProperty as the backing store for SplitStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SplitStrProperty =
            DependencyProperty.Register("SplitStr", typeof(string), typeof(IntShower001),
                new PropertyMetadata(": ",
                    propertyChangedCallback: PropertyValueChangeCallback_TriggerTextChange));




        public string EmptyStr
        {
            get { return (string)GetValue(EmptyStrProperty); }
            set { SetValue(EmptyStrProperty, value); OnPropertyChanged(nameof(EmptyStr)); }
        }

        // Using a DependencyProperty as the backing store for EmptyStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmptyStrProperty =
            DependencyProperty.Register("EmptyStr", typeof(string), typeof(IntShower001),
                new PropertyMetadata(string.Empty,
                    propertyChangedCallback: PropertyValueChangeCallback_TriggerTextChange));






        /// <summary>
        /// 当前值
        /// </summary>
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set
            { 
                SetValue(ValueProperty, value);
                OnPropertyChanged(nameof(Value));
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(IntShower001),
                new PropertyMetadata(propertyChangedCallback: PropertyValueChangeCallback_TriggerTextChange));



        public int? ShowingValue { get => Value; set => Value = value; }


        private static void PropertyValueChangeCallback_TriggerTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            if (d is IntShower001 shower)
            {
                shower.TriggerTextChange();
            }
        }
        private void TriggerTextChange()
        {
            OnPropertyChanged(nameof(CurrentText));
        }
        public string CurrentText 
        {
            get 
            {
                if (HeadStr.IsEmpty())
                {
                    return Value?.ToString() ?? EmptyStr;
                }
                else
                {
                    return $"{HeadStr}{SplitStr}{Value?.ToString() ?? EmptyStr}";
                }
            }
        }

        public string? ShowingText
        {
            get => CurrentText;
            set => throw new NotSupportedException("不允许直接修改显示文本");
        }

    }
}
