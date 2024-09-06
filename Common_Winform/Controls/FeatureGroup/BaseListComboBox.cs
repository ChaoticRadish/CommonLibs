using Common_Util.Extensions;
using Common_Util.Interfaces.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Controls.FeatureGroup
{
    [ToolboxItem(false)]
    public partial class BaseListComboBox : ComboBox
    {
        public BaseListComboBox()
        {
            InitializeComponent();

            DropDownStyle = ComboBoxStyle.DropDownList;
        }
        #region 属性
        /// <summary>
        /// 未选择项的文本
        /// </summary>
        [Category("CVII_自定义_配置"), Description("未选择项的文本")]
        public string NotSelecedString { get; set; } = "- 未选择 -";
        /// <summary>
        /// 是否允许未选择项
        /// </summary>
        [Category("CVII_自定义_配置"), Description("允许未选择类型")]
        public bool AllowNotSelect { get; set; }


        /// <summary>
        /// 当前选中的字符串
        /// </summary>
        [Category("CVII_自定义_状态"), Description("当前选中的字符串")]
        public string? SelectedString
        {
            get
            {
                if (SelectedItem == null) return null;
                ItemData seletecd = (ItemData)SelectedItem;
                return !seletecd.IsNotSelectedItem ? seletecd.Str : null;
            }
        }
        /// <summary>
        /// 当前选中的对象
        /// </summary>
        [Category("CVII_自定义_状态"), Description("当前选中的对象")]
        public object? SelectedObj
        {
            get
            {
                if (SelectedItem == null) return null;
                ItemData seletecd = (ItemData)SelectedItem;
                return !seletecd.IsNotSelectedItem ? seletecd.RelateObj : null;
            }
        }
        #endregion

        #region 配置
        /// <summary>
        /// 可选项列表, 最优先使用
        /// </summary>
        private List<ItemData>? SelectItems;
        /// <summary>
        /// 获取可选项列表的方法, 在无固定的可选项列表时, 优先使用
        /// </summary>
        private Func<List<ItemData>>? GetSelectItemsFunc;
        /// <summary>
        /// 获取可选项列表的方法 (键值对)
        /// </summary>
        private Func<List<KeyValuePair<object, string>>>? GetSelectItemsKeyValuePairFunc;
        /// <summary>
        /// 设置获取可选数据的方法
        /// </summary>
        /// <param name="funcCaller"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal BaseListComboBox SetSelectItemsFunc(Func<List<ItemData>> func)
        {
            GetSelectItemsFunc = func;
            return this;
        }
        /// <summary>
        /// 设置获取可选数据的方法
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        internal BaseListComboBox SetSelectItemsFunc(Func<List<KeyValuePair<object, string>>> func)
        {
            GetSelectItemsKeyValuePairFunc = func;
            return this;
        }
        /// <summary>
        /// 设置可选数据
        /// </summary>
        /// <param name="items">可选项列表</param>
        /// <returns></returns>
        internal BaseListComboBox SetSelectItems(List<ItemData> items)
        {
            SelectItems = items;
            return this;
        }
        /// <summary>
        /// 设置可选数据
        /// </summary>
        /// <param name="items">可选项列表</param>
        /// <returns></returns>
        internal BaseListComboBox SetSelectItems(List<KeyValuePair<object, string>> items)
        {
            SelectItems = items.Select(i => ItemData.NewItem(i.Key, i.Value)).ToList();
            return this;
        }
        ///// <summary>
        ///// 设置是否允许未选择项
        ///// </summary>
        ///// <param name="b"></param>
        ///// <returns></returns>
        //public ListComboBox SetAllowNotSelected(bool b = true)
        //{
        //    AllowNotSelected = b;

        //    return this;
        //}

        #endregion

        #region 控制
        /// <summary>
        /// 刷新可选项
        /// </summary>
        public new void RefreshItems()
        {
            if (SelectItems != null)
            {
                ResetItems(SelectItems);
            }
            else if (GetSelectItemsFunc != null)
            {
                ResetItems(GetSelectItemsFunc.Invoke());
            }
            else if (GetSelectItemsKeyValuePairFunc != null)
            {
                ResetItems(GetSelectItemsKeyValuePairFunc.Invoke().Select(i => ItemData.NewItem(i.Key, i.Value)).ToList());
            }
            else
            {
                ResetItems(null);
            }
        }
        /// <summary>
        /// 尝试选择关联对象与输入对象相等的可选项
        /// </summary>
        /// <param name="obj"></param>
        public void TrySelect(object obj)
        {
            if (obj == null) return;
            ItemData? same = null;
            foreach (ItemData item in Items)
            {
                if (item.RelateObj != null && item.RelateObj.Equals(obj))
                {
                    same = item;
                    break;
                }
            }
            if (same != null)
            {
                SelectedItem = same.Value;
            }
        }
        /// <summary>
        /// 选择 "未选择" 项, 如果没有未选择项, 则清除选择
        /// </summary>
        public void ClearSelected()
        {
            ItemData? temp = null;
            foreach (ItemData item in Items)
            {
                if (item.IsNotSelectedItem)
                {
                    temp = item;
                    break;
                }
            }
            if (temp != null)
            {
                SelectedItem = temp;
            }
            else
            {
                SelectedIndex = -1;
            }
        }

        #endregion


        /// <summary>
        /// 重设可选项
        /// </summary>
        /// <param name="items">可选项的枚举对象及其对应的字符串，注意不能重复</param>
        /// <param name="noSelectedItem"></param>
        private void ResetItems(List<ItemData>? items)
        {
            Items.Clear();
            if (items == null || items.Count == 0) return;
            if (AllowNotSelect)
            {
                items.Insert(0, ItemData.NotSelectedItem(NotSelecedString.WhenEmptyDefault("- 未选择 -")));
            }
            // 去重后添加进去
            Items.AddRange(
                items.Distinct()
                    .Select(i => (object)i)
                    .ToArray());
            if (Items.Count > 0)
                SelectedIndex = 0;
        }

        public struct ItemData
        {
            /// <summary>
            /// 是否未选择项
            /// </summary>
            public bool IsNotSelectedItem { get; set; }
            /// <summary>
            /// 需要显示出来的文本
            /// </summary>
            public string Str { get; set; }
            /// <summary>
            /// 关联对象
            /// </summary>
            public object? RelateObj { get; set; }

            public override string ToString()
            {
                return Str;
            }


            public static bool operator ==(ItemData left, ItemData right)
            {
                if (left.IsNotSelectedItem && right.IsNotSelectedItem)
                {
                    return true;
                }
                return left.RelateObj == right.RelateObj;
            }

            public static bool operator !=(ItemData left, ItemData right)
            {
                if (left.IsNotSelectedItem && right.IsNotSelectedItem)
                {
                    return left.RelateObj != right.RelateObj;
                }
                else
                {
                    return left.IsNotSelectedItem != right.IsNotSelectedItem;
                }
            }
            public override bool Equals(object? obj)
            {
                if (obj != null && obj is ItemData iObj)
                {
                    if (IsNotSelectedItem && iObj.IsNotSelectedItem)
                    {
                        return true;
                    }
                    return RelateObj == iObj.RelateObj;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Str.GetHashCode();
            }

            /// <summary>
            /// 使用输入的对象与字符串, 创建一个可选项
            /// </summary>
            /// <param name="relateObj">关联到选项上的对象</param>
            /// <param name="str"></param>
            /// <returns></returns>
            public static ItemData NewItem(object relateObj, string str)
            {
                return new ItemData()
                {
                    RelateObj = relateObj,
                    Str = str,
                    IsNotSelectedItem = false,
                };
            }
            public static ItemData NotSelectedItem(string text) => new ItemData()
            {
                Str = text,
                RelateObj = null,
                IsNotSelectedItem = true,
            };

        }
    }
}
