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
    /// 可换行Label 001
    /// <para>默认自动换行</para>
    /// </summary>
    public class WrapLabel001 : MbControl01, ITextShower
    {
        static WrapLabel001()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WrapLabel001), new FrameworkPropertyMetadata(typeof(WrapLabel001)));

            HorizontalAlignmentProperty.OverrideMetadata(
                typeof(WrapLabel001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(
                typeof(WrapLabel001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(WrapLabel001),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(WrapLabel001),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }




        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set 
            {
                SetValue(TextWrappingProperty, value);
                OnPropertyChanged(nameof(TextWrapping));
            }
        }

        // Using a DependencyProperty as the backing store for TextWrapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", 
                typeof(TextWrapping), 
                typeof(WrapLabel001), 
                new PropertyMetadata(TextWrapping.Wrap));




        public string? ShowingText
        {
            get { return (string?)GetValue(ShowingTextProperty); }
            set
            {
                SetValue(ShowingTextProperty, value);
                OnPropertyChanged(nameof(ShowingText));
            }
        }

        // Using a DependencyProperty as the backing store for ShowingText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowingTextProperty =
            DependencyProperty.Register("ShowingText", typeof(string), typeof(WrapLabel001), new PropertyMetadata(null));


    }
}
