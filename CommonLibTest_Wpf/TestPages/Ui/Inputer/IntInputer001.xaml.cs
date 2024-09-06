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

namespace CommonLibTest_Wpf.TestPages.Ui.Inputer
{
    /// <summary>
    /// IntInputer001.xaml 的交互逻辑
    /// </summary>
    public partial class IntInputer001 : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public IntInputer001()
        {
            InitializeComponent();

            DataContext = this;
        }

        public int? Input
        {
            get => input;
            set
            {
                input = value;
                OnPropertyChanged(nameof(Input));
                OnPropertyChanged(nameof(CurrentInput));
            }
        }
        private int? input;

        public int? EmptyValue
        {
            get => emptyValue;
            set
            {
                emptyValue = value;
                OnPropertyChanged(nameof(EmptyValue));
                OnPropertyChanged(nameof(CurrentEmptyValue));
            }
        }
        private int? emptyValue;

        public string CurrentInput { get => Input?.ToString() ?? "null"; }
        public string CurrentEmptyValue { get => EmptyValue?.ToString() ?? "null"; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EmptyValue = Input;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Input = new Random().Next(-1000, 1000);
        }
    }
}
