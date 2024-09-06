using CommonLibTest_Wpf.Models;
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
    /// KeyValuePairContainer001.xaml 的交互逻辑
    /// </summary>
    public partial class KeyValuePairContainer001 : Page
    {
        public KeyValuePairContainer001()
        {
            InitializeComponent();
            DataContext = Model;
        }

        KeyValuePairContainer001Model Model = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Model.生成随机值();
        }
    }
    public class KeyValuePairContainer001Model : NotifyTestModelBase
    {
        private string key = "Key值";
        private string value = "Value值";

        public string Key
        {
            get => key;
            set
            {
                key = value;
                OnPropertyChanged();
            }
        }
        public string Value
        {
            get => value;
            set 
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        public void 生成随机值() 
        {
            Key = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(8);
            Value = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(16);
        }
    }
}
