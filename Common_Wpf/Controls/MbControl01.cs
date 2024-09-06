using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common_Wpf.Controls
{
    /// <summary>
    /// Control 改进版01, 实现了一些接口, 增加了一些属性, UI需另外再实现
    /// <para>· 实现 <see cref="INotifyPropertyChanged"/> <see cref="INotifyPropertyChanging"/> 接口</para>
    /// <para>· 增加属性: 边框0 边框1</para>
    /// </summary>
    public abstract class MbControl01 : Control, INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region 通知
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        #endregion

        #region 背景


        public Brush? CustomBackground
        {
            get { return (Brush?)GetValue(CustomBackgroundProperty); }
            set { SetValue(CustomBackgroundProperty, value); OnPropertyChanged(nameof(CustomBackground)); }
        }

        // Using a DependencyProperty as the backing store for CustomBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBackgroundProperty =
            DependencyProperty.Register("CustomBackground", typeof(Brush), typeof(MbControl01), new PropertyMetadata(null));


        #endregion

        #region 字体


        public Brush? CustomForeground
        {
            get { return (Brush?)GetValue(CustomForegroundProperty); }
            set { SetValue(CustomForegroundProperty, value); OnPropertyChanged(nameof(CustomForeground)); }
        }

        // Using a DependencyProperty as the backing store for CustomForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomForegroundProperty =
            DependencyProperty.Register("CustomForeground", typeof(Brush), typeof(MbControl01), new PropertyMetadata(null));


        #endregion

        #region 边框


        public Brush? CustomBorderColor0
        {
            get { return (Brush?)GetValue(CustomBorderColor0Property); }
            set { SetValue(CustomBorderColor0Property, value); OnPropertyChanged(nameof(CustomBorderColor0)); }
        }

        // Using a DependencyProperty as the backing store for CustomBorderColor0.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBorderColor0Property =
            DependencyProperty.Register("CustomBorderColor0", typeof(Brush), typeof(MbControl01), new PropertyMetadata(null));



        public Brush BorderColor0
        {
            get { return (Brush)GetValue(BorderColor0Property); }
            set { SetValue(BorderColor0Property, value); OnPropertyChanged(nameof(BorderColor0)); }
        }

        // Using a DependencyProperty as the backing store for BorderColor0.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColor0Property =
            DependencyProperty.Register("BorderColor0", typeof(Brush), typeof(MbControl01));

        public Thickness BorderWidth0
        {
            get { return (Thickness)GetValue(BorderWidth0Property); }
            set { SetValue(BorderWidth0Property, value); OnPropertyChanged(nameof(BorderWidth0)); }
        }

        // Using a DependencyProperty as the backing store for BorderWidth0.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderWidth0Property =
            DependencyProperty.Register("BorderWidth0", typeof(Thickness), typeof(MbControl01));





        public Brush? CustomBorderColor1
        {
            get { return (Brush?)GetValue(CustomBorderColor1Property); }
            set { SetValue(CustomBorderColor1Property, value); OnPropertyChanged(nameof(CustomBorderColor1)); }
        }

        // Using a DependencyProperty as the backing store for CustomBorderColor1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBorderColor1Property =
            DependencyProperty.Register("CustomBorderColor1", typeof(Brush), typeof(MbControl01), new PropertyMetadata(null));


        public Brush BorderColor1
        {
            get { return (Brush)GetValue(BorderColor1Property); }
            set { SetValue(BorderColor1Property, value); OnPropertyChanged(nameof(BorderColor1)); }
        }

        // Using a DependencyProperty as the backing store for BorderColor1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColor1Property =
            DependencyProperty.Register("BorderColor1", typeof(Brush), typeof(MbControl01));

        public Thickness BorderWidth1
        {
            get { return (Thickness)GetValue(BorderWidth1Property); }
            set { SetValue(BorderWidth1Property, value); OnPropertyChanged(nameof(BorderWidth1)); }
        }

        // Using a DependencyProperty as the backing store for BorderWidth1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderWidth1Property =
            DependencyProperty.Register("BorderWidth1", typeof(Thickness), typeof(MbControl01));

        #endregion
    }
}
