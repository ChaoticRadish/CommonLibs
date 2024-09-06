using Common_Util.Data.Structure.Value;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CommonLibTest_Wpf.TestPages.ValueTest.Custom
{
    /// <summary>
    /// DecFixedPointNum001.xaml 的交互逻辑
    /// </summary>
    public partial class DecFixedPointNum001 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public DecFixedPointNum001()
        {
            InitializeComponent();

            DataContext = this;
        }

        public DecFixedPointNumber? 结构体值 
        {
            get => _结构体值;
            set
            {
                _结构体值 = value;
                OnPropertyChanged(nameof(结构体值));
                test2();
            }
        }
        private DecFixedPointNumber? _结构体值;

        public DecFixedPointNumber? 空输入值
        {
            get => _空输入值;
            set
            {
                _空输入值 = value;
                OnPropertyChanged(nameof(空输入值));
                OnPropertyChanged(nameof(当前空输入值));
            }
        }
        private DecFixedPointNumber? _空输入值;


        public int? 最大整数长度
        {
            get => _最大整数长度;
            set
            {
                _最大整数长度 = value;
                OnPropertyChanged(nameof(最大整数长度));
            }
        }
        private int? _最大整数长度;
        public int? 最大小数长度
        {
            get => _最大小数长度;
            set
            {
                _最大小数长度 = value;
                OnPropertyChanged(nameof(最大小数长度));
            }
        }
        private int? _最大小数长度;

        public int? 最大整数长度输入 
        {
            get => _最大整数长度输入;
            set
            {
                最大整数长度 = value;
                _最大整数长度输入 = value;
                OnPropertyChanged(nameof(最大整数长度输入));
            }
        }
        private int? _最大整数长度输入;
        public int? 最大小数长度输入
        {
            get => _最大小数长度输入;
            set
            {
                最大小数长度 = value;
                _最大小数长度输入 = value;
                OnPropertyChanged(nameof(最大小数长度输入));
            }
        }
        private int? _最大小数长度输入;

        public string 当前空输入值
        {
            get => _空输入值?.ToString() ?? "null";
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
        public string 转换结果
        {
            get => _转换结果;
            set
            {
                _转换结果 = value;
                OnPropertyChanged(nameof(转换结果));
            }
        }
        private string _转换结果 = string.Empty;

        public string 整数部分
        {
            get => _整数部分;
            set
            {
                _整数部分 = value;
                OnPropertyChanged(nameof(整数部分));
            }
        }
        private string _整数部分 = string.Empty;
        public string 小数部分
        {
            get => _小数部分;
            set
            {
                _小数部分 = value;
                OnPropertyChanged(nameof(小数部分));
            }
        }
        private string _小数部分 = string.Empty;
        public string 符号
        {
            get => _符号;
            set
            {
                _符号 = value;
                OnPropertyChanged(nameof(符号));
            }
        }
        private string _符号 = string.Empty;
        public string 转换为字符串
        {
            get => _转换为字符串;
            set
            {
                _转换为字符串 = value;
                OnPropertyChanged(nameof(转换为字符串));
            }
        }
        private string _转换为字符串 = string.Empty;

        private void test()
        {
            try
            {
                转换结果 = string.Empty;
                DecFixedPointNumber number = new DecFixedPointNumber();
                number.ChangeValue(输入值);

                转换结果 = "转换完成";

                符号 = number.IsZero ? "0" : (number.IsPositive ? "+" : "-");
                整数部分 = number.IntegerPart.ToHexString();
                小数部分 = number.DecimalPart.ToHexString();

                _结构体值 = number;
                OnPropertyChanged(nameof(结构体值));

                转换为字符串 = number.ToString();


            }
            catch (Exception ex)
            {
                转换结果 = "发生异常: " + ex.ToString();
            }
        }

        private void test2()
        {
            转换结果 = string.Empty;

            if (结构体值 == null)
            {
                转换结果 = "结构体输入框: null";
                
                符号 = string.Empty;
                整数部分 = string.Empty;
                小数部分 = string.Empty;
            }
            else
            {
                转换结果 = "结构体输入框: " + 结构体值.Value;

                var number = 结构体值.Value;
                符号 = number.IsZero ? "0" : (number.IsPositive ? "+" : "-");
                整数部分 = number.IntegerPart.ToHexString();
                小数部分 = number.DecimalPart.ToHexString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            空输入值 = 结构体值;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            空输入值 = new DecFixedPointNumber(true, [1, 0, 0], [0, 1]);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            空输入值 = null;
        }
    }
}
