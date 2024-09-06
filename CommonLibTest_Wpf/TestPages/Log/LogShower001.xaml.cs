using Common_Util.Log;
using Common_Util.Random;
using Common_Wpf.Controls.FeatureGroup;
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
    /// LogShower001.xaml 的交互逻辑
    /// </summary>
    public partial class LogShower001 : Page
    {
        public LogShower001()
        {
            InitializeComponent();

            //var loggers = new ILogger[] { simpleLogShower1 };
            var loggers = new ILogger[] { simpleLogShower1, simpleLogShower2 };
            logger = LevelLoggerHelper.LogTo(loggers, "控件", "SimpleLogShower");
        }

        private ILevelLogger logger;

        private int Index;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(10);
                    int index = Interlocked.Increment(ref Index);
                    int random = RandomValueTypeHelper.GetInt(0, 5);
                    string str = $"{index}. {RandomStringHelper.GetRandomUpperEnglishString(10)}";
                    Logger.Def.Info($"输出日志: {str}");
                    switch (random)
                    {
                        case 0:
                            logger.Info(str);
                            break;
                        case 1:
                            logger.Debug(str);
                            break;
                        case 2:
                            logger.Warning(str);
                            break;
                        case 3:
                            logger.Error(str);
                            break;
                        case 4:
                            logger.Fatal(str);
                            break;
                    }
                }
            });
        }
    }
}
