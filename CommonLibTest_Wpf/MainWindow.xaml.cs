using Common_Util.Data.Structure.Tree;
using Common_Util.Extensions;
using Common_Util.Module;
using Common_Wpf.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CommonLibTest_Wpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            BuildTree();

            MenuTree.ItemsSource = Children;

        }

        #region 生成子页面菜单的树
        private void BuildTree()
        {

            LayerComponentBaseLong layerComponent = new LayerComponentBaseLong(PageIndexRule);

            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            List<MenuItemModel> menuItems = new List<MenuItemModel>();

            foreach (PropertyInfo property in properties)
            {
                if (property.ExistCustomAttribute<PageConfigAttribute>(out var attribute)
                    && attribute != null)
                {
                    if (property.PropertyType.IsAssignableTo(typeof(Page)))
                    {
                        menuItems.Add(new MenuItemModel()
                        {
                            Index = attribute.Index,
                            Page = (Page?)property.GetValue(this),
                            ShowText = attribute.ShowText,
                            Desc = attribute.Desc,
                            IsPage = true,
                            
                        });
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        var item = new MenuItemModel()
                        {
                            Index = attribute.Index,
                            Page = null,
                            ShowText = attribute.ShowText,
                            Desc = attribute.Desc,
                            IsPage = false,

                        };
                        if (attribute.ShowText.IsEmpty())
                        {
                            item.ShowText = (string?)(property.GetValue(this)) ?? string.Empty;
                        }
                        menuItems.Add(item);
                    }
                }

            }


            foreach (MenuItemModel item in menuItems)
            {
                var result = layerComponent.Analysis((ulong)item.Index);
                if (result)
                {
                    item.SimplifySetLayerData(result.LayerValueMap);
                    item.SimplifyIndex = layerComponent.Create(item.LayerValueDic);
                }
                else
                {
                    throw new Exception("无法将序号转换为层级数据: " + result.FailureInfo);
                }
            }

            GeneralTree<ulong, MenuItemModel?> menuItemTree = new GeneralTree<ulong, MenuItemModel?>();
            menuItemTree.BuildTreeFromLayer<MenuItemModel?>(
                layerInfos: menuItems.Select(item => new KeyValuePair<MenuItemModel?, Dictionary<int, ulong>>(item, item.LayerValueDic)),
                convertFunc: item => item,
                nodeAvailableFunc: item => true);
            menuItemTree.Preorder(
                (node, index) =>
                {
                    if (node.NodeValue != null)
                    {
                        node.NodeValue.Children.Clear();
                        foreach (var children in node.Childrens)
                        {
                            if (children.NodeValue != null)
                            {
                                node.NodeValue.Children.Add(children.NodeValue);
                            }
                        }
                    }
                });

            Children.Clear();
            if (menuItemTree.Root != null)
            {
                foreach (var chilren in menuItemTree.Root.Childrens)
                {
                    if (chilren.NodeValue != null)
                    {
                        Children.Add(chilren.NodeValue);
                    }
                }
            }

        }

        public readonly LayerComponentBaseLong.Rule PageIndexRule = "11223333";

        private class PageConfigAttribute : System.Attribute
        {
            public PageConfigAttribute(int index, string showText = "", string desc = "")
            {
                Index = index;
                ShowText = showText;
                Desc = desc;
            }

            public int Index { get; }
            public string ShowText { get; }
            public string Desc { get; }
        }
        #endregion

        #region 子页面菜单
#pragma warning disable IDE0051 // 删除未使用的私有成员

        // =================== UI ========================
        [PageConfig(0x_01_00_0000)]
        string Page_Ui { get; set; } = "UI";


        // -------------- Base --------------
        [PageConfig(0x_01_01_0000)]
        string Page_Ui_Base { get; set; } = "Base";

        [PageConfig(0x_01_01_0001, "001")]
        TestPages.Ui.Base001 Pages_Ui_Base001 { get; set; } = new();

        [PageConfig(0x_01_01_0002, "002")]
        TestPages.Ui.Base002 Pages_Ui_Base002 { get; set; } = new();

        [PageConfig(0x_01_01_0003, "003")]
        TestPages.Ui.Base003 Pages_Ui_Base003 { get; set; } = new();

        [PageConfig(0x_01_01_0004, "004 Debug Visible")]
        TestPages.Ui.Base004 Pages_Ui_Base004 { get; set; } = new();

        // -------------- 颜色组 --------------
        [PageConfig(0x_01_02_0000)]
        string Page_Ui_ColorGroup { get; set; } = "颜色组";

        [PageConfig(0x_01_02_0001, "001")]
        TestPages.Ui.ColorGroup001 ColorGroup001 { get; set; } = new();


        // -------------- 集合绑定 --------------
        [PageConfig(0x_01_03_0000)]
        string Page_Ui_CollectionBinding { get; set; } = "集合绑定";

        [PageConfig(0x_01_03_0001, "001")]
        TestPages.Ui.CollectionBinding001 Page_Ui_CollectionBinding001 { get; set; } = new();

        [PageConfig(0x_01_03_0002, "002.可挂起集合")]
        TestPages.Ui.CollectionBinding002 Page_Ui_CollectionBinding002 { get; set; } = new();

        // -------------- 资源 --------------
        [PageConfig(0x_01_04_0000)]
        string Page_Ui_Resource { get; set; } = "资源";
        [PageConfig(0x_01_04_0001, "全局资源 001")]
        TestPages.Ui.GlobalRes001 Page_Ui_GlobalRes001 { get; set; } = new();

        // -------------- Label --------------
        [PageConfig(0x_01_05_0000)]
        string Page_Ui_Label { get; set; } = "Label";
        [PageConfig(0x_01_05_0001, "001")]
        TestPages.Ui.Label001 Page_Ui_Label001 { get; set; } = new();

        // -------------- Converter --------------
        [PageConfig(0x_01_06_0000)]
        string Page_Ui_Converter { get; set; } = "Converter";
        [PageConfig(0x_01_06_0001, "001")]
        TestPages.Ui.Converter001 Page_Ui_Converter001 { get; set; } = new();

        // -------------- Grid --------------
        [PageConfig(0x_01_07_0000)]
        string Page_Ui_Grid { get; set; } = "Grid";
        [PageConfig(0x_01_07_0001, "DataGrid 001")]
        TestPages.Ui.Grid.DataGrid001 Page_Ui_DataGrid001 { get; set; } = new();

        // -------------- Inputer --------------
        [PageConfig(0x_01_08_0000)]
        string Page_Ui_Inputer { get; set; } = "输入控件";
        [PageConfig(0x_01_08_0001, "Float 输入001")]
        TestPages.Ui.Inputer.FloatInput001 Page_Ui_FloatInput001 { get; set; } = new();
        [PageConfig(0x_01_08_0002, "Int 输入001")]
        TestPages.Ui.Inputer.IntInputer001 Page_Ui_IntInputer001 { get; set; } = new();
        [PageConfig(0x_01_08_0003, "Double 输入001")]
        TestPages.Ui.Inputer.DoubleInputer001 Page_Ui_DoubleInputer001 { get; set; } = new();

        // -------------- 便利控件 --------------
        [PageConfig(0x_01_09_0000)]
        string Page_Ui_Facility { get; set; } = "便利控件";
        [PageConfig(0x_01_09_0001, "键值对形式的容器.001")]
        TestPages.Ui.Facility.KeyValuePairContainer001 Page_Ui_KeyValuePairContainer001 { get; set; } = new();

        // -------------- 布局/容器 --------------
        [PageConfig(0x_01_0A_0000)]
        string Page_Ui_Layouts { get; set; } = "布局/容器";
        [PageConfig(0x_01_0A_0001, "可收起的容器 001")]
        TestPages.Ui.Layouts.RetractableContainer001 Page_Ui_RetractableContainer001 { get; set; } = new();
        [PageConfig(0x_01_0A_0011, "页面切换器 001")]
        TestPages.Ui.Layouts.PageSwitch001 Page_Ui_PageSwitch001 { get; set; } = new();
        [PageConfig(0x_01_0A_0012, "页面切换器 002 嵌套测试")]
        TestPages.Ui.Layouts.PageSwitch002 Page_Ui_PageSwitch002 { get; set; } = new();
        [PageConfig(0x_01_0A_0020, "Null 值感知内容容器 001")]
        TestPages.Ui.Layouts.NullAware001 Page_Ui_NullAware001 { get; set; } = new();   


        // =================== Log ========================
        [PageConfig(0x_02_00_0000)]
        string Page_Log { get; set; } = "Log";

        // -------------- 日志窗口 --------------
        [PageConfig(0x_02_01_0000)]
        string Page_Log_LogWindow { get; set; } = "日志窗口";

        [PageConfig(0x_02_01_0001, "001")]
        TestPages.Log.LogWindow001 Pages_Log_LogWindow001 { get; set; } = new();


        // -------------- 日志显示控件 --------------
        [PageConfig(0x_02_02_0000)]
        string Page_Log_LogShower { get; set; } = "日志显示控件";

        [PageConfig(0x_02_02_0001, "001")]
        TestPages.Log.LogShower001 Pages_Log_LogShower001 { get; set; } = new();




        // =================== 模块 ========================
        [PageConfig(0x_03_00_0000)]
        string Page_Modules { get; set; } = "模块";

        // -------------- 调度 --------------
        [PageConfig(0x_03_01_0000)]
        string Page_Modules_Scheduling { get; set; } = "调度";

        [PageConfig(0x_03_01_0001, "001.最小时间间隔")]
        TestPages.Modules.Scheduling001 Pages_Modules_Scheduling001 { get; set; } = new();

        // -------------- 调度 --------------
        [PageConfig(0x_03_02_0000)]
        string Page_Modules_Command { get; set; } = "指令";

        [PageConfig(0x_03_02_0001, "001.简单的异步指令")]
        TestPages.Modules.Command001 Pages_Modules_Command001 { get; set; } = new();

        // =================== 字符串 ========================
        [PageConfig(0x_04_00_0000)]
        string Page_String { get; set; } = "字符串";

        // -------------- StringBuilder --------------
        [PageConfig(0x_04_01_0000)]
        string Page_String_Builder { get; set; } = "StringBuilder";

        [PageConfig(0x_04_01_0001, "001.扩展方法 Trim()")]
        TestPages.StringTest.Builder001 Pages_String_Builder001 { get; set; } = new();

        // -------------- 正则表达式 --------------
        [PageConfig(0x_04_02_0000)]
        string Page_String_Regex { get; set; } = "正则表达式";

        [PageConfig(0x_04_02_0001, "001.是否匹配正则表达式")]
        TestPages.StringTest.Regex001 Pages_String_Regex001 { get; set; } = new();

        // -------------- 信息字符串 --------------
        [PageConfig(0x_04_03_0000)]
        string Page_String_InfoString { get; set; } = "信息字符串";

        [PageConfig(0x_04_03_0001, "001.信息字符串转换器测试")]
        TestPages.StringTest.InfoString001 Pages_String_InfoString001 { get; set; } = new();

        // =================== 值 ========================
        [PageConfig(0x_05_00_0000)]
        string Page_Value { get; set; } = "值";

        // -------------- 自定义值类型 --------------
        [PageConfig(0x_05_01_0000)]
        string Page_Value_Custom { get; set; } = "自定义值类型";

        [PageConfig(0x_05_01_0001, "十进制定点数.001.string转换")]
        TestPages.ValueTest.Custom.DecFixedPointNum001 Pages_Value_Custom_DecFixedPointNum001 { get; set; } = new();

        [PageConfig(0x_05_01_0002, "十进制定点数.002.int转换")]
        TestPages.ValueTest.Custom.DecFixedPointNum002 Pages_Value_Custom_DecFixedPointNum002 { get; set; } = new();

        [PageConfig(0x_05_01_0003, "十进制定点数.003.转换器")]
        TestPages.ValueTest.Custom.DecFixedPointNum003 Pages_Value_Custom_DecFixedPointNum003 { get; set; } = new();

        // -------------- Byte 数组 --------------
        [PageConfig(0x_05_02_0000)]
        string Page_Value_ByteArray { get; set; } = "Byte 数组";

        [PageConfig(0x_05_02_0001, "001.Trim")]
        TestPages.ValueTest.ByteArray.ByteArrayTest001 Pages_Value_ByteArray_ByteArrayTest001 { get; set; } = new();

        [PageConfig(0x_05_02_0002, "002.与字符串的转化")]
        TestPages.ValueTest.ByteArray.ByteArrayTest002 Pages_Value_ByteArray_ByteArrayTest002 { get; set; } = new();

        // -------------- 自定义值类型 --------------
        [PageConfig(0x_05_03_0000)]
        string Page_Value_Code { get; set; } = "编码值";

        [PageConfig(0x_05_03_0001, "001.比较")]
        TestPages.ValueTest.Code.LayeringAddressCode001 Pages_Value_ByteArray_LayeringAddressCode001 { get; set; } = new();



#pragma warning restore IDE0051 // 删除未使用的私有成员
        #endregion


        public List<MenuItemModel> Children { get; set; } = new List<MenuItemModel>();

        public class MenuItemModel
        {
            public int Index { get; set; }

            public string ShowText { get; set; } = string.Empty;

            public string Desc { get; set; } = string.Empty;

            public bool IsPage { get; set; }

            public Page? Page { get; set; }

            public List<MenuItemModel> Children { get; set; } = new List<MenuItemModel>();

            /// <summary>
            /// 实际所属层
            /// </summary>
            public int LayerIndex { get; set; }
            /// <summary>
            /// 分析所得的层级数值的字典
            /// </summary>
            public Dictionary<int, ulong> LayerValueDic { get; private set; } = new Dictionary<int, ulong>();
            /// <summary>
            /// 以精简的方式设置层级数值
            /// </summary>
            /// <param name="layerData"></param>
            public void SimplifySetLayerData(Dictionary<int, ulong> layerData)
            {
                LayerValueDic.Clear();
                var layerIndices = layerData.Where(i => i.Value > 0).OrderBy(i => i.Key);
                LayerIndex = layerIndices.Max(i => i.Key);
                int temp = 0;
                foreach (var kvPair in layerIndices)
                {
                    if (kvPair.Key - temp > 1)
                    {
                        break;
                    }
                    LayerValueDic.Add(kvPair.Key, kvPair.Value);
                    temp++;
                }
            }
            /// <summary>
            /// 精简之后的索引, 即去掉多余信息后的索引值, 比如一个索引含有1层, 2层和4层的值, 4层的值是无意义的, 将被精简
            /// </summary>
            public ulong SimplifyIndex { get; set; }
        }

        private void MenuTreeItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == null) return;
            var treeViewItem = VisualUpwardSearch<TreeViewItem>((DependencyObject)e.OriginalSource) as TreeViewItem;
            if (treeViewItem != null && treeViewItem.HasHeader)
            {
                MenuItemModel model = (MenuItemModel)treeViewItem.Header;
                if (model.IsPage)
                {
                    BodyArea.Content = model.Page;

                    treeViewItem.Focus();
                    e.Handled = true;
                }
            }
        }
        private DependencyObject? VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }


        #region 事件重载
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            UiLogHelper.CloseWindow();
        }
        #endregion
    }
}
