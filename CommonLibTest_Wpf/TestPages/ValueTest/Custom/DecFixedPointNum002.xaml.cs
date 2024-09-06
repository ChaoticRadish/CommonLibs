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
    /// DecFixedPointNum002.xaml 的交互逻辑
    /// </summary>
    public partial class DecFixedPointNum002 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public DecFixedPointNum002()
        {
            InitializeComponent();
            DataContext = this;
        }

        public int? IntSource
        {
            get => intSource;
            set
            {
                intSource = value;
                OnPropertyChanged(nameof(IntSource));
                test();
            }
        }
        private int? intSource;

        public int? PointPosition
        {
            get => pointPosition;
            set
            {
                pointPosition = value;
                OnPropertyChanged(nameof(PointPosition));
                test();
            }
        }
        private int? pointPosition;

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

        private void test()
        {
            转换结果 = string.Empty;

            try
            {
                if (IntSource == null)
                {
                    转换结果 = "输入值为null";
                }
                else
                {
                    var number = DecFixedPointNumber.Convert(IntSource.Value, PointPosition ?? 0);

                    符号 = number.IsZero ? "0" : (number.IsPositive ? "+" : "-");
                    整数部分 = number.IntegerPart.ToHexString();
                    小数部分 = number.DecimalPart.ToHexString();

                    转换结果 = number.ToString();
                }
            }
            catch (Exception ex)
            {
                转换结果 = "发生异常" + ex.ToString();
            }
        }
    }
}
