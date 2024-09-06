using Common_Util.Extensions;
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

namespace CommonLibTest_Wpf.TestPages.ValueTest.ByteArray
{
    /// <summary>
    /// ByteArrayTest001.xaml 的交互逻辑
    /// </summary>
    public partial class ByteArrayTest001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public ByteArrayTest001()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void test()
        {
            try
            {
                执行结果 = string.Empty;

                byte[] bs = 输入值.Where(i => i >= '0' && i <= '9')
                                .Select(i => (byte)(i - '0')).ToArray();

                输入值字符串 = bs.ToHexString();

                Trim结果 = bs.Trim(0, 5, 9).ToHexString();

                TrimStart结果 = bs.TrimStart(0, 5, 9).ToHexString();

                TrimEnd结果 = bs.TrimEnd(0, 5, 9).ToHexString();


                执行结果 = "执行完成";
            }
            catch (Exception ex)
            {
                执行结果 = "发生异常: " + ex.Message;
            }
        }

        public string 输入值
        {
            get => _输入值;
            set
            {
                _输入值 = value;
                OnPropertyChanged(nameof(输入值));
                test();
            }
        }
        private string _输入值 = string.Empty;

        public string 输入值字符串
        {
            get => _输入值字符串;
            set
            {
                _输入值字符串 = value;
                OnPropertyChanged(nameof(输入值字符串));
            }
        }
        private string _输入值字符串 = string.Empty;

        public string Trim结果
        {
            get => _Trim结果;
            set
            {
                _Trim结果 = value;
                OnPropertyChanged(nameof(Trim结果));
            }
        }
        private string _Trim结果 = string.Empty;

        public string TrimStart结果
        {
            get => _TrimStart结果;
            set
            {
                _TrimStart结果 = value;
                OnPropertyChanged(nameof(TrimStart结果));
            }
        }
        private string _TrimStart结果 = string.Empty;

        public string TrimEnd结果
        {
            get => _TrimEnd结果;
            set
            {
                _TrimEnd结果 = value;
                OnPropertyChanged(nameof(TrimEnd结果));
            }
        }
        private string _TrimEnd结果 = string.Empty;

        public string 执行结果
        {
            get => _执行结果;
            set
            {
                _执行结果 = value;
                OnPropertyChanged(nameof(执行结果));
            }
        }
        private string _执行结果 = string.Empty;
    }
}
