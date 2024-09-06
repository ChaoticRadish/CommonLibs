using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static Common_Util.Module.LayerComponentBaseLong;

namespace CommonLibTest_Wpf.TestPages.StringTest
{
    /// <summary>
    /// Regex001.xaml 的交互逻辑
    /// </summary>
    public partial class Regex001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public Regex001()
        {
            InitializeComponent();

            DataContext = this;
        }

        public string StringInput
        {
            get => _strInput;
            set
            {
                _strInput = value;
                OnPropertyChanged(nameof(StringInput));
                _do();
            }
        }
        private string _strInput = string.Empty;

        public string RegexInput
        {
            get => _regexInput;
            set
            {
                _regexInput = value;
                OnPropertyChanged(nameof(RegexInput));
                _do();
            }
        }
        private string _regexInput = string.Empty;

        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }
        private string _result = string.Empty;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _do();
        }
        private void _do()
        {
            try
            {
                Regex regex = new Regex(RegexInput);
                bool match = regex.IsMatch(StringInput);

                Result = match ? "正则表达式匹配" : "正则表达式不匹配";
            }
            catch (Exception ex)
            {
                Result = "发生异常: " + ex.Message;
            }
        }
    }
}
