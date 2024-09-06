using Common_Util.Extensions;
using Common_Util.Log;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Grid
{
    /// <summary>
    /// Grid001.xaml 的交互逻辑
    /// </summary>
    public partial class DataGrid001 : Page
    {
        Collection<TestModel001> list = new();

        public DataGrid001()
        {
            InitializeComponent();
            list.AddRandom(30);
            DataContext = list;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            list.AddRandom();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            list.AddRandom(10);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag == null)
                {
                    Logger.Operation.Info($"点击按钮列的按钮, Tag为null");
                }
                else
                {
                    Logger.Operation.Info($"点击按钮列的按钮, Tag类型: {btn.Tag.GetType().Name}");
                    Logger.Operation.Info($"Tag值: \n{btn.Tag.FullInfoString()}");
                }
            }
        }
    }
}
