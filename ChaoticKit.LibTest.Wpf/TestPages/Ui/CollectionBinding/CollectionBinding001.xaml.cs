using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace ChaoticKit.LibTest.Wpf.TestPages.Ui
{
    /// <summary>
    /// CollectionBinding001.xaml 的交互逻辑
    /// </summary>
    public partial class CollectionBinding001 : Page
    {
        public CollectionBinding001()
        {
            InitializeComponent();

            DataContext = VM;


        }

        public CollectionBinding001ViewModel VM { get; set; } = new();




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VM.AddRandom();
        }
    }

    public class CollectionBinding001ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<CollectionBinding001Model> Models
        {
            get => _Models;
            set
            {
                _Models = value;
                OnPropertyChanged(nameof(Models));
            }
        }
        private ObservableCollection<CollectionBinding001Model> _Models = new ObservableCollection<CollectionBinding001Model>();



        public void AddRandom()
        {
            System.Random random = new System.Random();
            Models.Add(new CollectionBinding001Model()
            {
                A = ChaoticKit.Random.RandomStringHelper.GetRandomEnglishString(3, random),
                B = ChaoticKit.Random.RandomValueTypeHelper.GetInt(0, 100, random),
                C = ChaoticKit.Random.RandomValueTypeHelper.GetFloat(0, 10, random),
            });
        }
    }
    public class CollectionBinding001Model
    {
        public string A { get; set; } = string.Empty;

        public int B { get; set; } = 3;

        public float C { get; set; }

        public List<int> D { get; set; }

    }
}
