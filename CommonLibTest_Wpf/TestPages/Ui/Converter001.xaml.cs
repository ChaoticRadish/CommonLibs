using Common_Wpf.Themes;
using System;
using System.Collections.Generic;
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
    /// Converter001.xaml 的交互逻辑
    /// </summary>
    public partial class Converter001 : Page
    {
        public Converter001()
        {
            InitializeComponent();

            DataContext = this;
        }



        public SolidColorBrush? Color
        {
            get { return (SolidColorBrush?)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(Converter001), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Color = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Color = System.Windows.Media.Brushes.White;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Color = System.Windows.Media.Brushes.Red;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Color = System.Windows.Media.Brushes.Blue;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Holy;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Default;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ColorManager.ColorGroup = ColorGroupEnum.Dark;
        }
    }
}
