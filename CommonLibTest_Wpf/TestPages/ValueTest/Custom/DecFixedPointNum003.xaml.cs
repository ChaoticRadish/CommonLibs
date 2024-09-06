using Common_Util.Data.Structure.Value;
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

namespace CommonLibTest_Wpf.TestPages.ValueTest.Custom
{
    /// <summary>
    /// DecFixedPointNum003.xaml 的交互逻辑
    /// </summary>
    public partial class DecFixedPointNum003 : Page
    {
        public DecFixedPointNum003()
        {
            InitializeComponent();
            DataContext = new DecFixedPointNum003Model();
        }
    }
    public class DecFixedPointNum003Model : NotifyTestModelBase
    {
        public string 当前输入String 
        {
            get => _当前输入String;
            set
            {
                _当前输入String = value;
                OnPropertyChanged();
            }
        }
        private string _当前输入String = string.Empty;

        public int 当前输入Int
        {
            get => _当前输入Int;
            set
            {
                _当前输入Int = value;
                OnPropertyChanged();
            }
        }
        private int _当前输入Int = 0;

        public float 当前输入Float
        {
            get => _当前输入Float;
            set
            {
                _当前输入Float = value;
                OnPropertyChanged();
            }
        }
        private float _当前输入Float = 0;

        public double 当前输入Double
        {
            get => _当前输入Double;
            set
            {
                _当前输入Double = value;
                OnPropertyChanged();
            }
        }
        private double _当前输入Double = 0;
    }
}
