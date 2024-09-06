using Common_Util;
using Common_Util.Attributes.General;
using Common_Util.Attributes;
using Common_Util.Extensions;
using Common_Winform.Attributes;
using Common_Winform.Pages;
using Common_Winform.Pages.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common_Winform.Controls.Menus;
using Common_Util.Module;
using Common_Util.Data.Structure.Tree;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Common_Winform.Forms
{
    /// <summary>
    /// 主窗口模板01: 
    /// </summary>
    public partial class MainFormTemplate01 : FormEx01
    {
        public MainFormTemplate01()
        {
            InitializeComponent();

            LogInfo("创建类属性中声明的页面");
            LoadProperty_Page();
            LogInfo("创建类属性中声明的顶部菜单项");
            LoadProperty_TopMenu();
            LogInfo("创建类属性中声明的状态等");
            LoadProperty_StateLight();

        }

        #region 属性
        public bool TopMenuVisable 
        { 
            get => TopMenuArea.Visible;
            set => TopMenuArea.Visible = value;
        }
        #endregion

        #region 子项
        /// <summary>
        /// 所有子页面
        /// </summary>
        private Dictionary<int, PageBase> Pages
        {
            get => AreaBody.Where(i => i != null && i.Page != null).ToDictionary(i => i!.Index, i => i!.Page!); 
        }

        private Dictionary<long, TopMenuItem> TopMenuItems = new Dictionary<long, TopMenuItem>();
        #endregion

        #region 创建类属性中声明的页面
        protected virtual void LoadProperty_Page()
        {
            AreaBody.AddPagesHandle = (box) =>
            {
                PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                int? defaultIndex = null;
                foreach (PropertyInfo property in properties)
                {
                    if (property.SetMethod == null)
                    {
                        continue;
                    }

                    if (property.PropertyType.IsAssignableTo(typeof(PageBase))
                        && property.PropertyType.HavePublicEmptyCtor()
                        && property.ExistCustomAttribute<PageConfigAttribute>(out var attribute)
                        && !attribute!.Ignore)
                    {
                        if (attribute.ShowName.IsEmpty())
                        {
                            if (property.ExistCustomAttribute<ShowInfoAttribute>(out var showInfo))
                            {
                                attribute.ShowName = showInfo!.Name;
                            }
                        }
                        if (attribute.Desc.IsEmpty())
                        {
                            if (property.ExistCustomAttribute<ShowInfoAttribute>(out var showInfo))
                            {
                                attribute.Desc = showInfo!.Desc;
                            }
                        }

                        PageBase page = (PageBase)TypeHelper.InvokePublicNoParamConsturctor(property.PropertyType)!;
                        page.Init(attribute);
                        property.SetValue(this, page);

                        LogInfo($"添加页面: {page.Name.WhenEmptyDefault(property.PropertyType.Name)}");

                        AreaBody.AddPage(new TabPageLayout01Item()
                        {
                            Index = attribute!.Index,
                            Page = page,
                            PageName = attribute.ShowName,
                            PageDesc = attribute.Desc,
                            页面只读 = attribute.ReadOnly,
                            页面禁用 = !attribute.Enable,
                        });

                        if (property.ExistCustomAttribute<DefaultSelectItemAttribute>())
                        {
                            defaultIndex = attribute!.Index;
                        }
                    }

                }
                AreaBody.DefaultSelectIndex = defaultIndex;
            };
            AreaBody.Init();
            AreaBody.BringToFront();
        }
        #endregion

        #region 创建类属性中声明的顶部菜单
        protected LayerComponentBaseLong? TopMenuIndexLayerComponent { get; set; }

        protected virtual void LoadProperty_TopMenu()
        {
            List<TopMenuItem> items = new List<TopMenuItem>();

            Type type = GetType();
            TopMenuConfigAttribute? config;
            if (!type.ExistCustomAttribute(out config))
            {
                LogInfo($"未标记顶部菜单配置, 将不创建顶部菜单项 ({nameof(TopMenuConfigAttribute)})");
                return;
            }
            var rule = config!.Rule;
            TopMenuIndexLayerComponent = new LayerComponentBaseLong(rule);
            if (TopMenuIndexLayerComponent.StartWithLayerId == null)
            {
                throw new Exception("顶部菜单序号规则没有有效层");
            }
            if (TopMenuIndexLayerComponent.StartWithLayerId != 1)
            {
                throw new Exception("顶部菜单序号规则的层编号必须由1开始, 当前开始值: " + TopMenuIndexLayerComponent.StartWithLayerId);
            }
            if (!TopMenuIndexLayerComponent.IsSequence)
            {
                throw new Exception("顶部菜单序号规则的层编号不连续! ");
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (PropertyInfo property in properties)
            {
                if (property.ExistCustomAttribute<TopMenuItemConfigAttribute>(out var attribute)
                    && (property.PropertyType == typeof(string) || property.PropertyType == typeof(TopMenuItem) || property.PropertyType == typeof(ToolStripItem)))
                {

                    TopMenuItem item = new TopMenuItem()
                    {
                        Index = attribute!.Index,
                        RelateProperty = property,
                        Name = property.Name,
                        ShowName = property.Name,
                        Description = string.Empty,
                    };
                    if (property.ExistCustomAttribute<ShowInfoAttribute>(out var showInfo))
                    {
                        item.ShowName = showInfo!.Name;
                        item.Description = showInfo!.Desc ?? string.Empty;
                    }
                    if (property.ExistCustomAttribute<MethodAttribute>(out var methodName) && !methodName!.MethodName.IsEmpty())
                    {
                        var method = type.GetMethod(methodName.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            if (ReflectionHelper.IsMatchAnyReturn(method))
                            {
                                item.OnClick = () => method.Invoke(this, new object[] { });
                            }
                            else if (ReflectionHelper.IsMatchAnyReturn(method, new Type[] { typeof(TopMenuItem) }))
                            {
                                item.OnClick = () => method.Invoke(this, new object[] { item });
                            }
                        }
                    }
                    if (property.PropertyType == typeof(string))
                    {
                        item.ShowName = (property.GetValue(this)?.ToString()).WhenEmptyDefault(item.Name);
                    }

                    if (property.PropertyType == typeof(TopMenuItem))
                    {
                        property.SetValue(this, item);
                    }

                    items.Add(item);
                }
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                if (method.ExistCustomAttribute<TopMenuItemConfigAttribute>(out var attribute)
                    && (ReflectionHelper.IsMatchAnyReturn(method) || ReflectionHelper.IsMatchAnyReturn(method, new Type[] { typeof(TopMenuItem) })))
                {
                    TopMenuItem item = new TopMenuItem()
                    {
                        Index = attribute!.Index,
                        RelateMethod = method,
                        Name = method.Name,
                        ShowName = method.Name,
                        Description = string.Empty,
                    };
                    if (method.ExistCustomAttribute<ShowInfoAttribute>(out var showInfo))
                    {
                        item.ShowName = showInfo!.Name;
                        item.Description = showInfo!.Desc ?? string.Empty;
                    }
                    if (ReflectionHelper.IsMatchAnyReturn(method))
                    {
                        item.OnClick = () => method.Invoke(this, new object[] { });
                    }
                    else if (ReflectionHelper.IsMatchAnyReturn(method, new Type[] { typeof(TopMenuItem) }))
                    {
                        item.OnClick = () => method.Invoke(this, new object[] { item });
                    }

                    items.Add(item);
                }
            }

            foreach (TopMenuItem item in items)
            {
                var result = TopMenuIndexLayerComponent.Analysis(item.Index);
                if (result)
                {
                    item.SimplifySetLayerData(result.LayerValueMap);
                    item.SimplifyIndex = TopMenuIndexLayerComponent.Create(item.LayerValueDic);
                }
                else
                {
                    throw new Exception("无法将序号转换为层级数据: " + result.FailureInfo);
                }
            }
            GeneralTree<ulong, TopMenuItem?> menuItemTree = new GeneralTree<ulong, TopMenuItem?>();
            menuItemTree.BuildTreeFromLayer<TopMenuItem?>(
                layerInfos: items.Select(item => new KeyValuePair<TopMenuItem?, Dictionary<int, ulong>>(item, item.LayerValueDic)),
                convertFunc: item => item,
                nodeAvailableFunc: item => true);
            string treeStr = menuItemTree.GetSimpleTreeString(
                getValueStringFunc: item => item == null ? "<null>" : item.ShowName);
            LogInfo("顶部菜单分析结果, 其结构: \n" + treeStr);
            LogInfo("顶部菜单创建菜单项...");
            menuItemTree.Preorder(
                (node, index) =>
                {
                    if (node.NodeValue == null) return;
                    TopMenuItem item = node.NodeValue;
                    bool isButton = item.IsButton;
                    bool existChild = node.Childrens.Any();
                    if (existChild)
                    {
                        LogInfo($"创建顶部菜单: 创建菜单项 {item.ShowName}");
                        ToolStripMenuItem menuItem = new ToolStripMenuItem();
                        menuItem.DropDown.AutoSize = true;
                        menuItem.Text = item.ShowName;
                        if (item.OnClick != null)
                        {
                            menuItem.Click += (obj, args) => item.OnClick();
                        }
                        item.RelateToolStripItem = menuItem;
                    }
                    else
                    {
                        if (isButton)
                        {
                            LogInfo($"创建顶部菜单: 创建按钮项 {item.ShowName}");
                            ToolStripMenuItem button = new ToolStripMenuItem();
                            button.Text = item.ShowName;
                            if (item.OnClick != null)
                            {
                                button.Click += (obj, args) => item.OnClick();
                            }
                            item.RelateToolStripItem = button;
                        }
                        else
                        {
                            LogInfo($"创建顶部菜单: 创建文本项 {item.ShowName}");
                            ToolStripLabel label = new ToolStripLabel();
                            label.Text = item.ShowName;
                            item.RelateToolStripItem = label;
                        }
                    }
                });
            LogInfo("顶部菜单菜单项排序...");
            menuItemTree.SortChildrenUse(
                (node1, node2) =>
                {
                    return node1.NodeScope > node2.NodeScope ? Common_Util.Enums.CompareResultEnum.Bigger : Common_Util.Enums.CompareResultEnum.Smaller;
                });

            LogInfo("顶部菜单菜单项放入菜单栏...");
            menuItemTree.Preorder(
                (node, index) =>
                {
                    if (node.NodeValue == null) return;
                    TopMenuItem item = node.NodeValue;

                    if (item.RelateToolStripItem != null)
                    {
                        item.RelateToolStripItem.AutoSize = true;

                        if (node.Parent == null || node.Parent.NodeValue == null)
                        {
                            LogInfo($"创建顶部菜单: {item.ShowName} 放入根节点");
                            TopMenuArea.Items.Add(item.RelateToolStripItem);
                        }
                        else if (node.Parent.NodeValue.RelateToolStripItem is ToolStripMenuItem menuItem)
                        {
                            LogInfo($"创建顶部菜单: {item.ShowName} 放入父节点 {menuItem.Text}");
                            menuItem.DropDownItems.Add(item.RelateToolStripItem);
                        }
                        else
                        {
                            throw new Exception("尝试创建顶部菜单节点失败! 没有对应的父节点! ");
                        }
                    }
                    if (item.RelateProperty?.PropertyType == typeof(ToolStripItem))
                    {
                        item.RelateProperty.SetValue(this, item.RelateToolStripItem);
                    }
                });

        }

        #endregion

        #region 创建类属性中声明的状态灯
        protected virtual void LoadProperty_StateLight()
        {

        }

        #endregion
    }
}
