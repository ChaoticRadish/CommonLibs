using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ByteArrayTest002.xaml 的交互逻辑
    /// </summary>
    public partial class ByteArrayTest002 : Page
    {
        public ByteArrayTest002()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }


        private ByteArrayTest002ViewModel ViewModel;
    }

    public class ByteArrayTest002ViewModel : Models.NotifyTestModelBase
    {

        #region string => byte[]
        private string strToArrInput = string.Empty;
        public string StrToArrInput { get => strToArrInput; set { strToArrInput = value; OnPropertyChanged(); } }

        #endregion

        #region byte[] => string

        public ObservableCollection<byte> ArrToStrInput { get; set; } = [];


        #endregion

    }
}
