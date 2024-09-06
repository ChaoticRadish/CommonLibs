using Common_Wpf.Themes;
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

namespace CommonLibTest_Wpf.TestPages.Ui
{
    /// <summary>
    /// ColorGroup001.xaml 的交互逻辑
    /// </summary>
    public partial class ColorGroup001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public ColorGroup001()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_SwitchDark_Click(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Dark;
        }

        private void Button_SwitchHoly_Click(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Holy;
        }

        private void Button_SwitchDefault_Click(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Default;
        }
        private void Button_SwitchTest_Click(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroupFileName = "Test.xaml";
        }

        public int Temp
        {
            get => CurrentValue;
        }

        public int CurrentValue
        {
            get { return (int)GetValue(CurrentValueProperty); }
            set 
            { 
                SetValue(CurrentValueProperty, value);
                OnPropertyChanged(nameof(CurrentValue));
                OnPropertyChanged(nameof(Temp));
            }
        }

        // Using a DependencyProperty as the backing store for CurrentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", 
                typeof(int), typeof(ColorGroup001), new PropertyMetadata(50));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentValue *= 2;
            if (CurrentValue <= 0)
            {
                CurrentValue = 2;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentValue /= 2;
            if (CurrentValue <= 0)
            {
                CurrentValue = 2;
            }
        }

    }
}
