using Common_Util.Log;
using Common_Util.Module.Command;
using Common_Wpf.Controls.LayoutPanel;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Layouts
{
    /// <summary>
    /// ActionButton001.xaml 的交互逻辑
    /// </summary>
    public partial class ActionButton001 : Page
    {
        static ActionButton001()
        {
            ActionButtonOptionConverter.Shared.Register<ActionButton001TestButtonOption>(o => new()
            {
                Label = o.Key,
                Command = new SimpleCommand(() =>
                {
                    Logger.Operation.Info($"ActionButton001TestButtonOption: {o.Key}");
                })
            });
        }
        public ActionButton001()
        {
            InitializeComponent();

            TestCommand = new SimpleCommand(() =>
            {
                Logger.Operation.Info("TestCommand");
            });
        }

        public ICommand TestCommand { get; init; }
    }
    public class ActionButton001TestButtonOption : DependencyObject
    {
        public string Key { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"<TestButtonOption:{Key}>";
        }
    }
}
