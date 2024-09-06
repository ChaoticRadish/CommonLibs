using Common_Wpf.Globals;
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
    /// GlobleRes001.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalRes001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public GlobalRes001()
        {
            InitializeComponent();

            DataContext = this;

            FormatStr = GlobalResources.FullTimeFormat;
            TestStr = GlobalResources.TestStr001;
        }



        public string FormatStr
        {
            get { return (string)GetValue(FormatStrProperty); }
            set 
            {
                SetValue(FormatStrProperty, value);
                OnPropertyChanged(nameof(FormatStr));
            }
        }

        // Using a DependencyProperty as the backing store for FormatStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FormatStrProperty =
            DependencyProperty.Register("FormatStr", typeof(string), typeof(GlobalRes001),
                new PropertyMetadata("..."));



        public string TestStr
        {
            get { return (string)GetValue(TestStrProperty); }
            set
            {
                SetValue(TestStrProperty, value);
                OnPropertyChanged(nameof(TestStr));
            }
        }

        // Using a DependencyProperty as the backing store for TestStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestStrProperty =
            DependencyProperty.Register("TestStr", typeof(string), typeof(GlobalRes001),
                new PropertyMetadata("..."));




        private void Button_Switch1_Click(object sender, RoutedEventArgs e)
        {
            GlobalResources.FileNameFormats = "Formats.xaml";
            GlobalResources.ReInit();
            FormatStr = GlobalResources.FullTimeFormat;
        }

        private void Button_Switch2_Click(object sender, RoutedEventArgs e)
        {
            GlobalResources.FileNameFormats = "Formats_test.xaml";
            GlobalResources.ReInit();
            FormatStr = GlobalResources.FullTimeFormat;
        }

        private void Button_Switch3_Click(object sender, RoutedEventArgs e)
        {
            GlobalResources.FileNameTexts = "Texts.xaml";
            GlobalResources.ReInit();
            TestStr = GlobalResources.TestStr001;
        }

        private void Button_Switch4_Click(object sender, RoutedEventArgs e)
        {
            GlobalResources.FileNameTexts = "Texts_test.xaml";
            GlobalResources.ReInit();
            TestStr = GlobalResources.TestStr001;
        }
    }
}
