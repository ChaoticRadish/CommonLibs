using Common_Util.Extensions;
using Common_Util.Log;
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

namespace CommonLibTest_Wpf.TestPages.Ui
{
    /// <summary>
    /// CollectionBinding002.xaml 的交互逻辑
    /// </summary>
    public partial class CollectionBinding002 : Page
    {
        public CollectionBinding002()
        {
            InitializeComponent();

            model.List.CollectionChanged += (o, e) => Logger.Def.Info($"CollectionChanged: {e.Action}");
            model.List.SuspendUpdated += (o, e) => Logger.Def.Info("SuspendUpdated");
            model.List.ResumeUpdated += (o, e) => Logger.Def.Info($"ResumeUpdated");

            model.List.Add(new());
            this.DataContext = model;
        }

        private TestModel003 model = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Def.Info("model.List.Add(new());");
            model.List.Add(new());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Logger.Def.Info("model.List.AddRange(new(), new(), new());");
            model.List.AddRange(new(), new(), new());
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Logger.Def.Info("model.List.AddRange([new(), new(), new()], true);");
            model.List.AddRange([new(), new(), new()], true);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Logger.Def.Info("model.List.AddRange([new(), new(), new()], false);");
            model.List.AddRange([new(), new(), new()], false);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            model.List.SuspendUpdate();
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            model.List.ResumeUpdate();
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                var obj = model.List.RandomRemove();
                Logger.Def.Info("随机移除值: " + obj.ToString());
            }
            catch (Exception ex)
            {
                Logger.Def.Error("随机移除异常! ", ex);
            }

        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            try
            {
                model.List.RandomReplace((obj) =>
                {
                    TestModel002 newOne = new();
                    Logger.Def.Info($"随机替换值: {obj} => {newOne}");
                    return newOne;
                });
            }
            catch (Exception ex)
            {
                Logger.Def.Error("随机替换异常! ", ex);
            }
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                model.List.RandomDo((obj) =>
                {
                    Logger.Def.Info($"随机执行: ({obj}).A += 随机执行! ");
                    obj.A += "随机执行! ";
                });
            }
            catch (Exception ex)
            {
                Logger.Def.Error("随机执行异常! ", ex);
            }
        }
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            model.List.TriggerCollectionChanged();
        }
    }
}
