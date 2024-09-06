using Common_Util.Extensions;
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

namespace CommonLibTest_Wpf.TestPages.Modules
{
    /// <summary>
    /// Command001.xaml 的交互逻辑
    /// </summary>
    public partial class Command001 : Page
    {
        public Command001()
        {
            InitializeComponent();

            DataContext = new Command001ViewModel();
        }

        
    }

    public class Command001ViewModel
    {
        public ICommand C1 
        {
            get
            {
                var temp = new AsyncCommand(
                    (token, obj) =>
                    {
                        token.Register(() => Logger.Def.Info("c1 触发取消!!!"));
                        return Task.Run(() => c1Impl(token, obj), token);
                    },
                    (obj) => true);
                C2 = temp.CancelCommand;
                return temp;
            }
        }

        public ICommand? C2 { get; private set; }


        public ICommand C3
        {
            get
            {
                var temp = new AsyncCommand(
                    (token, obj) =>
                    {
                        token.Register(() => Logger.Def.Info("c3 触发取消!!!"));
                        return Task.Run(() => c3Impl(token, obj), token);
                    },
                    (obj) => true);
                C4 = temp.CancelCommand;
                return temp;
            }
        }

        public ICommand? C4 { get; private set; }

        private async Task c1Impl(CancellationToken token, object? obj)
        {
            try
            {
                Logger.Def.Info("c1Impl: 开始");
                Logger.Def.Info(obj?.FullInfoString() ?? "<null>");
                await Task.Delay(5000, token);
                Logger.Def.Info("c1Impl: 结束");
            }
            catch (TaskCanceledException ex)
            {
                Logger.Def.Error("c1Impl: 取消执行! ", ex);
            }
        }
        private async Task c3Impl(CancellationToken token, object? obj)
        {
            Logger.Def.Info("c3Impl: 开始");
            Logger.Def.Info(obj?.FullInfoString() ?? "<null>");
            for (int i = 0; i < 10; i++)
            {
                Logger.Def.Info("c3Impl: " + i);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
            Logger.Def.Info("c3Impl: 结束");
        }
    }
}
