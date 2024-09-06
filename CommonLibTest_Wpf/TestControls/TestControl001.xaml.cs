using CommonLibTest_Wpf.Models;
using CommonLibTest_Wpf.TestPages.Ui;
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

namespace CommonLibTest_Wpf.TestControls
{
    /// <summary>
    /// TestControl001.xaml 的交互逻辑
    /// </summary>
    public partial class TestControl001 : UserControl
    {
        public TestControl001()
        {
            InitializeComponent();
        }


        public ObservableCollection<ITestModel> TestModels
        {
            get { return (ObservableCollection<ITestModel>)GetValue(TestModelsProperty); }
            set { SetValue(TestModelsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TestModels.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestModelsProperty =
            DependencyProperty.Register("TestModels", typeof(ObservableCollection<ITestModel>), typeof(TestControl001), new PropertyMetadata(new ObservableCollection<ITestModel>()) 
            { 
                PropertyChangedCallback = (s, e) =>
                {

                }
            });

    }
}
