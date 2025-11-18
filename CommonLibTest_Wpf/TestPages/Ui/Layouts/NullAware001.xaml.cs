using System;
using System.Collections.Generic;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Layouts
{
    /// <summary>
    /// NullAware001.xaml 的交互逻辑
    /// </summary>
    public partial class NullAware001 : Page, INotifyPropertyChanged
    {
        public NullAware001()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? MyData
        {
            get => _myData;
            set
            {
                _myData = value;
                OnPropertyChanged(nameof(MyData));
            }
        }
        private string? _myData;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyData = MyData == null ? "测试文本" : null;
        }
    }
}
