using ChaoticKit.Log;
using ChaoticKit.Module.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ChaoticKit.LibTest.Wpf.TestPages.Ui.Facility
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
        public PaginationControl001ViewModel ViewModel { get; } = new();

        private void PageContentLoad(object? arg)
        {
            if (arg is ChaoticKit.Data.Structure.Pair.IObjectChanged<int> e)
            {
                Logger.Operation.Info($"(同步) 模拟加载: {e.OldOne} => {e.NewOne}");
                Thread.Sleep(2000);
                Logger.Operation.Info($"(同步) 模拟加载结束");
            }
        }

        public ICommand PageContentLoadAsyncCommand { get; }

        private async Task PageContentLoadAsync(object? arg)
        {
            if (arg is ChaoticKit.Data.Structure.Pair.IObjectChanged<int> e)
            {
                Logger.Operation.Info($"(异步) 模拟加载: {e.OldOne} => {e.NewOne}");
                await Task.Delay(2000);
                Logger.Operation.Info($"(异步) 模拟加载结束");
            }
        }

        private void PaginationBox_OnPageChanged(object sender, ChaoticKit.Data.Structure.Pair.IObjectChanged<int> e)
        {
            Logger.Operation.Info($"事件: {e.OldOne} => {e.NewOne}");
        }


    }

    public class PaginationControl001ViewModel : INotifyPropertyChanged
    {
        public int PageCode
        {
            get
            {
                Logger.Operation.Info($"ViewModel Get PageCode: {pageCode}");
                return pageCode;
            }
            set
            {
                Logger.Operation.Info($"ViewModel Set PageCode: {value}");
                pageCode = value;
                TriggerPropertyChanged();
            }
        }
        private int pageCode;

        public int TotalPage
        {
            get
            {
                Logger.Operation.Info($"ViewModel Get TotalPage: {totalPage}");
                return totalPage;
            }
            set
            {
                Logger.Operation.Info($"ViewModel Set TotalPage: {value}");
                totalPage = value;
                TriggerPropertyChanged();
            }
        }
        private int totalPage;

        #region 属性值变化事件
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<string>? TitleChanged;
        protected void TriggerPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
