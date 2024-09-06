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
using System.Windows.Shapes;

namespace CommonLibTest_Wpf
{
    /// <summary>
    /// LogViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerWindow : Window
    {
        public LogViewerWindow()
        {
            InitializeComponent();
        }

        #region 日志流部分
        public void Log(LogData log)
        {
            LogBox.Log(log);
        }
        #endregion

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }
    }
}
