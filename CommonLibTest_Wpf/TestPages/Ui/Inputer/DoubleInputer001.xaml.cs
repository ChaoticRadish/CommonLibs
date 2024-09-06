using CommonLibTest_Wpf.Models;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Inputer
{
    /// <summary>
    /// DoubleInputer001.xaml 的交互逻辑
    /// </summary>
    public partial class DoubleInputer001 : Page
    {
        public DoubleInputer001()
        {
            InitializeComponent();
            DataContext = Model;
        }

        DoubleInputer001Model Model = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Model.CurrentInput = Common_Util.Random.RandomValueTypeHelper.GetDouble(0, 100);
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
            Model.EmptyValue = 100.01;
        }
    }
    public class DoubleInputer001Model : NotifyTestModelBase
    {
        private double? currentInput;
        private double? emptyValue;

        public double? CurrentInput
        {
            get => currentInput;
            set
            {
                currentInput = value;
                OnPropertyChanged();
            }
        }

        public double? EmptyValue
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
