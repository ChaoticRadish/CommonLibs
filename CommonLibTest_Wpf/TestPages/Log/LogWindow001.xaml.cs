using Common_Util.Log;
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

namespace CommonLibTest_Wpf.TestPages.Log
{
    /// <summary>
    /// LogWindow001.xaml 的交互逻辑
    /// </summary>
    public partial class LogWindow001 : Page
    {
        public LogWindow001()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Def.Info("Button_Click");
            for (int i = 0; i < 100; i++)
            {
                Logger.Def.Info(i.ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Logger.Def.Error("Button_Click_1 Def Error !!!");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Logger.Def.Error("Button_Click_1 Def Error !!! Button_Click_1 Def Error !!! Button_Click_1 Def Error !!! Button_Click_1 Def Error !!! ");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Logger.Def.Error("Button_Click_1 Def Error !", new Exception("异常测试", new Exception("内部异常asdasidjoaisjd")));

        }
    }
}
