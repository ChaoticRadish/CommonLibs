using Common_Util.Extensions;
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

namespace CommonLibTest_Wpf.TestPages.StringTest
{
    /// <summary>
    /// Builder001.xaml 的交互逻辑
    /// </summary>
    public partial class Builder001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Builder001()
        {
            InitializeComponent();
            DataContext = this;
        }



        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); OnPropertyChanged(nameof(Input)); }
        }

        // Using a DependencyProperty as the backing store for Input.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(string), typeof(Builder001), new PropertyMetadata(string.Empty));




        public string Output01
        {
            get { return (string)GetValue(Output01Property); }
            set { SetValue(Output01Property, value); OnPropertyChanged(nameof(Output01)); }
        }

        // Using a DependencyProperty as the backing store for Output01.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Output01Property =
            DependencyProperty.Register("Output01", typeof(string), typeof(Builder001), new PropertyMetadata(string.Empty));




        public string Output02
        {
            get { return (string)GetValue(Output02Property); }
            set { SetValue(Output02Property, value); OnPropertyChanged(nameof(Output02)); }
        }

        // Using a DependencyProperty as the backing store for Output02.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Output02Property =
            DependencyProperty.Register("Output02", typeof(string), typeof(Builder001), new PropertyMetadata(string.Empty));

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            test();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            test();
        }
        private void test()
        {
            StringBuilder sb1 = new StringBuilder(Input);
            Output01 = sb1.Trim().ToString();
            StringBuilder sb2 = new StringBuilder(Input);
            Output02 = sb2.Trim(" .abcd").ToString();
        }
    }
}
