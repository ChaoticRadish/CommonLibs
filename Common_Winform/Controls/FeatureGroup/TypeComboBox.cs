using Common_Util;
using Common_Util.Enums;
using Common_Util.Extensions;
using Common_Util.String;
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

namespace Common_Winform.Controls.FeatureGroup
{
    [ToolboxItem(true)]
    public partial class TypeComboBox : ComboBox
    {
        #region 属性
        /// <summary>
        /// 默认加入 "未选择" 选项
        /// </summary>
        [Category("CVII_自定义_配置"), Description("允许未选择类型")]
        public bool AllowNotSelect { get; set; } = true;
        /// <summary>
        /// 未选择项的文本
        /// </summary>
        [Category("CVII_自定义_配置"), Description("未选择项的文本")]
        public string NotSelecedString { get; set; } = "- 未选择 -";
        /// <summary>
        /// 过滤的类型范围
        /// </summary>
        [Category("CVII_自定义_配置"), Description("过滤的类型范围")]
        public TypeClassifyEnum TypeClassify
        {
            get
            {
                return typeClassify;
            }
            set
            {
                typeClassify = value;
                RefreshItems();
            }
        }
        private TypeClassifyEnum typeClassify = TypeClassifyEnum.All;

        #endregion
        #region 数据
        /// <summary>
        /// 类型项
        /// </summary>
        public Type[] TypeItems { get; set; } = new Type[0];

        /// <summary>
        /// 类型源
        /// </summary>
        public Assembly[] TypeSources { get; set; } = new Assembly[0];

        /// <summary>
        /// 当前选中的类型
        /// </summary>
        public Type? SelectedType
        {
            get
            {
                if (SelectedItem != null && SelectedItem is Item item)
                {
                    return item.Type;
                }
                return null;
            }
        }
        #endregion
        public TypeComboBox()
        {
            InitializeComponent();
            DropDownStyle = ComboBoxStyle.DropDownList;//下拉框样式设置为不能编辑
            
        }

        #region 暴露给外面的控制方法
        /// <summary>
        /// 设置可选类型的数据集
        /// </summary>
        /// <param name="assembly"></param>
        public void SetAssembly(params Assembly[] assembly)
        {
            TypeSources = assembly ?? Array.Empty<Assembly>();
        }
        /// <summary>
        /// 设置可选类型的类型
        /// </summary>
        /// <param name="assembly"></param>
        public void SetType(params Type[] type)
        {
            TypeItems = type ?? Array.Empty<Type>();
        }
        #endregion

        #region 显示
        /// <summary>
        /// 刷新正在显示的
        /// </summary>
        public new void RefreshItems()
        {
            Items.Clear();

            if (AllowNotSelect)
            {
                Items.Add(new Item() { Type = null, ShowText = NotSelecedString.WhenEmptyDefault("- 未选择 -") });
            }

            //EnumHelper.ForEach<TypeClassifyEnum>((e) =>
            //{
            //    if (typeClassify.HasFlag(e))
            //    {
            //        Console.WriteLine($"HasFlag {e} ");
            //    }
            //});

            foreach (Type t in TypeItems)
            {
                if (CheckNeedAdd(t))
                {
                    Items.Add(new Item() { Type = t });
                }
            }

            if (TypeSources == null)
            {
                // throw new Exception("未设定可选类型的范围");
            }
            else
            {
                foreach (var source in TypeSources)
                {
                    Type[] types = source.GetTypes();
                    foreach (Type t in types)
                    {
                        if (CheckNeedAdd(t))
                        {
                            Items.Add(new Item() { Type = t });
                        }
                    }
                }
            }
            if (Items.Count > 0)
                SelectedIndex = 0;
        }
        private bool CheckNeedAdd(Type t)
        {
            if (typeClassify.HasFlag(TypeClassifyEnum.NoNested) && t.IsNested)
            {// 类型是嵌套类, 但是没被允许
                return false;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.HasEmptyArgConstructor) && t.GetConstructor(Type.EmptyTypes) == null)
            {// 需要有无参构造函数, 但是实际没有
                return false;
            }

            if (typeClassify.HasFlag(TypeClassifyEnum.Enum) && t.IsEnum)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.Abstract) && t.IsAbstract)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.Array) && t.IsArray)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.ValueType) && t.IsValueType)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.GenericType) && t.IsGenericType)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.GenericTypeDefinition) && t.IsGenericTypeDefinition)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.Interface) && t.IsInterface)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.Class) && t.IsClass)
            {
                return true;
            }
            else if (typeClassify.HasFlag(TypeClassifyEnum.All))
            {// 所有类型都可以
                return true;
            }
            else
            {
                return false;
            }
        }

        public struct Item
        {
            public Type? Type { get; set; }
            public string ShowText { get; set; }
            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(ShowText) ? (Type == null ? "null" : StringHelper.GetTypeString(Type)) : ShowText;
            }
        }
        #endregion
    }
}
