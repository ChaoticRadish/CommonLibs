using Common_Util.Extensions;
using Common_Util.Module.Command;
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

namespace CommonLibTest_Wpf.TestPages.StringTest
{
    /// <summary>
    /// InfoString001.xaml 的交互逻辑
    /// </summary>
    public partial class InfoString001 : Page
    {
        public InfoString001()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }


        private InfoString001ViewModel ViewModel;
    }

    public class InfoString001ViewModel : Models.NotifyTestModelBase
    {
        public ObservableCollection<Models.TestModel001> TestItems { get; set; } = [];

        public ICommand CreateRandomsCommand => new SampleCommand(_ => CreateRandoms(), _ => true);
        private void CreateRandoms() 
        {
            TestItems.Clear();

            var list = Common_Util.Random.RandomObjectHelper.GetList<Models.TestModel001>(null, 10, 20);
            if (list == null) return;
            foreach (var item in list)
            {
                TestItems.Add(item);
            }

        }

        public ICommand ClickListItemCommand => new SampleCommand(ClickListItem, _ => true);

        private void ClickListItem(object? obj)
        {
            Showing = null;
            ShowingInfoString = string.Empty;
            if (obj == null) return;
            if (obj is Models.TestModel001 testMode)
            {
                Showing = testMode;
                ShowInfoString(testMode);
            }

        }

        private Models.TestModel001? showing;
        public Models.TestModel001? Showing { get => showing; set { showing = value; OnPropertyChanged(); } }

        private void ShowInfoString(object? obj)
        {
            ShowingInfoString = obj == null ? "<null>" : obj.FullInfoString();
        }

        private string showingInfoString = string.Empty;
        public string ShowingInfoString { get => showingInfoString; set { showingInfoString = value; OnPropertyChanged(); } }

    }
}
