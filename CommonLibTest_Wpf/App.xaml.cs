using Common_Wpf.Themes;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CommonLibTest_Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ColorManager.ColorGroup = ColorGroupEnum.Default;
        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UiLogHelper.InitWindow();
            UiLogHelper.ShowWindow();

            Common_Util.Log.GlobalLoggerManager.CurrentLogger = UiLogHelper.Instance;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }


    }

}
