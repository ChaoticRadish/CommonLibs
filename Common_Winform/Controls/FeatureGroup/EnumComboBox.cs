using Common_Util;
using Common_Util.Attributes.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Controls.FeatureGroup
{
    /// <summary>
    /// 可初始化选项为枚举类型的ComboBox
    /// </summary>
    [ToolboxItem(true)]
    public partial class EnumComboBox : BaseListComboBox
    {
        public EnumComboBox()
        {
            InitializeComponent();

            DropDownStyle = ComboBoxStyle.DropDownList;
        }


        #region 获取选中项
        /// <summary>
        /// 获取当前选中的枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selected">未选择时返回默认值</param>
        /// <returns></returns>
        public bool GetSelectedEnum<T>(out T? selected) where T : Enum
        {
            bool result = GetSelectedEnum(typeof(T), out object? obj);
            selected = result && obj != null ? (T)obj : default;
            return result;
        }
        /// <summary>
        /// 获取当前选中的枚举
        /// </summary>
        /// <param name="type"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        public bool GetSelectedEnum(Type type, out object? selected)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException($"输入类型 {type} 不是枚举", nameof(type));
            }

            selected = SelectedObj;
            if (selected != null && selected.GetType().Equals(type))
            {
                return true;
            }
            else
            {
                selected = null;
                return false;
            }

        }
        #endregion

        #region 初始化
        /// <summary>
        /// 将枚举类型各值的名字设置为可选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void InitAsName<T>() where T : Enum
        {
            InitAsName(typeof(T));
        }
        public void InitAsName(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException($"输入类型 {type} 不是枚举", nameof(type));
            }

            List<ItemData> datas = new List<ItemData>();
            Array values = Enum.GetValues(type);
            foreach (object obj in values)
            {
                if (obj != null)
                {
                    datas.Add(ItemData.NewItem(obj, Enum.GetName(type, obj) ?? string.Empty));
                }
            }
            SetSelectItems(datas);
            RefreshItems();
        }
        /// <summary>
        /// 将枚举类型各值的描述 (使用 <see cref="DescriptionAttribute"/> 或 <see cref="EnumDescAttribute"/> 标识的描述信息) 设置为可选项, 默认会跳过不含描述的项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ignoreWithoutDesc">是否跳过不含描述的项<, 不跳过时使用字段名字/param>
        public void InitAsDesc<T>(bool ignoreWithoutDesc = true) where T : Enum
        {
            InitAsDesc(typeof(T), ignoreWithoutDesc);
        }
        public void InitAsDesc(Type type, bool ignoreWithoutDesc = true)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException($"输入类型 {type} 不是枚举", nameof(type));
            }

            List<ItemData> datas = new List<ItemData>();
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                var obj = field.GetValue(null);
                if (obj == null) continue;
                var attr = field.GetCustomAttribute<EnumDescAttribute>();
                if (attr != null)
                {
                    datas.Add(ItemData.NewItem(obj, attr.Desc));
                    continue;
                }
                var attr2 = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr2 != null)
                {
                    datas.Add(ItemData.NewItem(obj, attr2.Description));
                    continue;
                }
                if (!ignoreWithoutDesc)
                {
                    datas.Add(ItemData.NewItem(obj, field.Name));
                    continue;
                }
            }
            SetSelectItems(datas);
            RefreshItems();
        }

        #endregion

    }
}
