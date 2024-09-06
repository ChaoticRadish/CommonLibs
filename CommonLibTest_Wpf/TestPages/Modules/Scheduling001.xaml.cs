using Common_Util.Module.Scheduling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Scheduling001.xaml 的交互逻辑
    /// </summary>
    public partial class Scheduling001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Scheduling001()
        {
            InitializeComponent();
            DataContext = this;
        }



        public ObservableCollection<string> Infos
        {
            get { return (ObservableCollection<string>)GetValue(InfosProperty); }
            set { SetValue(InfosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Infos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfosProperty =
            DependencyProperty.Register("Infos", typeof(ObservableCollection<string>), typeof(Scheduling001), 
                new PropertyMetadata(new ObservableCollection<string>()
                {
                    $"测试调度: {nameof(MinTimeGapHelper)}"
                }));

        public void Do()
        {
            Info("调度! ");
        }
        private void Info(string info)
        {
            this.Dispatcher.Invoke(() =>
            {
                Infos.Add($"{DateTime.Now:G}: {info}");
                OnPropertyChanged(nameof(Infos));
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Info("按下按钮");
            MinTimeGapHelper.Do("KEY", Do, new TimeSpan(0, 0, 5));
        }
    }
}
