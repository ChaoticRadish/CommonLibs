using Common_Util.Data.Enums;
using Common_Util.Log;
using Common_Util.Module.Command;
using Common_Util.Random;
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

namespace CommonLibTest_Wpf.TestPages.Ui.Layouts
{
    /// <summary>
    /// RetractableContainer001.xaml 的交互逻辑
    /// </summary>
    public partial class RetractableContainer001 : Page
    {
        public RetractableContainer001()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private RetractableContainer001ViewModel ViewModel = new();


        private void Button_Click_0(object sender, RoutedEventArgs e)
        {
            ViewModel.RetractableAreaLocation = FourWayEnum.Up;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewModel.RetractableAreaLocation = FourWayEnum.Down;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ViewModel.RetractableAreaLocation = FourWayEnum.Left;
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ViewModel.RetractableAreaLocation = FourWayEnum.Right;
        }
    }

    public class RetractableContainer001ViewModel : NotifyTestModelBase
    {
        public FourWayEnum RetractableAreaLocation
        {
            get => retractableAreaLocation;
            set { retractableAreaLocation = value; OnPropertyChanged(); }
        }
        private FourWayEnum retractableAreaLocation = FourWayEnum.Up;


        public GridLength RetractableAreaWidth
        {
            get => retractableAreaWidth;
            set { retractableAreaWidth = value; OnPropertyChanged(); }
        }
        private GridLength retractableAreaWidth = new GridLength(0.5, GridUnitType.Star);

        public ILevelLogger TestLogger { get; } = LevelLoggerHelper.EnumLog(Logger.Control);

        public double RetractUsingTime
        {
            get => retractUsingTime;
            set { retractUsingTime = value; OnPropertyChanged(); }
        }
        private double retractUsingTime = 0.5;


        public RetractableContainer001RetractableAreaModel RAModel { get; } = new();

        public RetractableContainer001BodyAreaModel BAModel { get; } = new();
    }

    public class RetractableContainer001RetractableAreaModel : NotifyTestModelBase
    {
        public int ValueA { get => valueA; set { valueA = value; OnPropertyChanged(); } }
        private int valueA;
        public string ValueB { get => valueB; set { valueB = value; OnPropertyChanged(); } }
        private string valueB = string.Empty;
    }
    public class RetractableContainer001BodyAreaModel : NotifyTestModelBase
    {
        public int ValueA { get => valueA; set { valueA = value; OnPropertyChanged(); } }
        private int valueA;
        public string ValueB { get => valueB; set { valueB = value; OnPropertyChanged(); } }
        private string valueB = string.Empty;

        public double ValueC { get => valueC; set { valueC = value; OnPropertyChanged(); } }
        private double valueC;

        public ICommand ValueCCommand => new SampleCommand((obj) => _valueCCommand(), _ => true);
        private void _valueCCommand()
        {
            ValueC = RandomValueTypeHelper.GetDouble(0, 100);
        }
    }
}
