using CommonLibTest_Wpf.Models;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Inputer
{
    /// <summary>
    /// FloatInput001.xaml 的交互逻辑
    /// </summary>
    public partial class FloatInput001 : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public FloatInput001()
        {
            InitializeComponent();
            DataList.AddRandom(5);
            DataContext = this;
        }

        public float? CurrentInput
        {
            get => _currentInput;
            set
            {
                _currentInput = value;
                CurrentToInt = (int?)value ?? 0;
                OnPropertyChanged(nameof(CurrentInput));
            }
        }
        private float? _currentInput;


        public int CurrentToInt
        {
            get => _currentToInt;
            set
            {
                _currentToInt = value;
                OnPropertyChanged(nameof(CurrentToInt));
            }
        }
        private int _currentToInt;

        public float? EmptyValue
        {
            get => _emptyValue;
            set
            {
                _emptyValue = value;
                OnPropertyChanged(nameof(EmptyValue));
            }
        }
        private float? _emptyValue;
        public bool ReadOnly
        {
            get => readOnly;
            set 
            {
                readOnly = value;
                OnPropertyChanged(nameof(ReadOnly));
            }
        }
        private bool readOnly;

        public string TestStr
        {
            get => _testStr;
            set
            {
                _testStr = value;
                OnPropertyChanged(nameof(TestStr));
            }
        }
        private string _testStr = string.Empty;

        public Collection<TestModel001> DataList { get; set; } = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentInput = Common_Util.Random.RandomValueTypeHelper.GetFloat(0, 100);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button btn)
            {
                if (btn.DataContext is TestModel001 test)
                {
                    test.StrB = "123.456";
                }
            }
        }
    }
}
