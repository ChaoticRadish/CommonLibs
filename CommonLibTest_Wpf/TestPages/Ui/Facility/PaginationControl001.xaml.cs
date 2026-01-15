using Common_Util.Log;
using Common_Util.Module.Command;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Facility
{
    /// <summary>
    /// PaginationControl001.xaml 的交互逻辑
    /// </summary>
    public partial class PaginationControl001 : Page
    {
        public PaginationControl001()
        {
            InitializeComponent();
            DataContext = this;
            PageContentLoadCommand = new SimpleCommand(PageContentLoad);
            PageContentLoadAsyncCommand = new AsyncCommand((token, arg) => PageContentLoadAsync(arg), (arg) => true);
        }

        public ICommand PageContentLoadCommand { get; }
        private void PageContentLoad(object? arg)
        {
            if (arg is Common_Util.Data.Structure.Pair.IObjectChanged<int> e)
            {
                Logger.Operation.Info($"(同步) 模拟加载: {e.OldOne} => {e.NewOne}");
                Thread.Sleep(2000);
                Logger.Operation.Info($"(同步) 模拟加载结束");
            }
        }

        public ICommand PageContentLoadAsyncCommand { get; }

        private async Task PageContentLoadAsync(object? arg)
        {
            if (arg is Common_Util.Data.Structure.Pair.IObjectChanged<int> e)
            {
                Logger.Operation.Info($"(异步) 模拟加载: {e.OldOne} => {e.NewOne}");
                await Task.Delay(2000);
                Logger.Operation.Info($"(异步) 模拟加载结束");
            }
        }

        private void PaginationBox_OnPageChanged(object sender, Common_Util.Data.Structure.Pair.IObjectChanged<int> e)
        {
            Logger.Operation.Info($"事件: {e.OldOne} => {e.NewOne}");
        }
    }
}
