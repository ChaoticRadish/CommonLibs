using ChaoticKit.LibTest.Wpf.Models;
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

namespace ChaoticKit.LibTest.Wpf.TestPages.Ui.Inputer
{
    /// <summary>
    /// DecimalInputer001.xaml 的交互逻辑
    /// </summary>
    public partial class DecimalInputer001 : Page
    {
        public DecimalInputer001()
        {
            InitializeComponent();
            DataContext = Model;
        }
        DecimalInputer001Model Model = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Model.CurrentInput = (decimal)ChaoticKit.Random.RandomValueTypeHelper.GetDouble(0, 100);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Model.EmptyValue = Model.CurrentInput;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Model.EmptyValue = null;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Model.EmptyValue = 100.01m;
        }

    }

    public class DecimalInputer001Model : NotifyTestModelBase
    {
        private decimal? currentInput;
        private decimal? emptyValue;

        public decimal? CurrentInput
        {
            get => currentInput;
            set
            {
                currentInput = value;
                OnPropertyChanged();
            }
        }

        public decimal? EmptyValue
        {
            get => emptyValue;
            set
            {
                emptyValue = value;
                OnPropertyChanged();
            }
        }


    }
}
