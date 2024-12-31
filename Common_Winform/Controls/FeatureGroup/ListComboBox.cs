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
    [ToolboxItem(true)]
    public partial class ListComboBox : BaseListComboBox
    {
        public ListComboBox()
        {
            InitializeComponent();
        }

        #region 暴露基类方法
        /// <summary>
        /// 设置获取可选数据的方法
        /// </summary>
        /// <param name="funcCaller"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public override ListComboBox SetSelectItemsFunc(Func<List<ItemData>> func)
        {
            base.SetSelectItemsFunc(func);
            return this;
        }
        /// <summary>
        /// 设置获取可选数据的方法
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public override BaseListComboBox SetSelectItemsFunc(Func<List<KeyValuePair<object, string>>> func)
        {
            base.SetSelectItemsFunc(func);
            return this;
        }
        /// <summary>
        /// 设置可选数据
        /// </summary>
        /// <param name="items">可选项列表</param>
        /// <returns></returns>
        public override BaseListComboBox SetSelectItems(List<ItemData> items)
        {
            base.SetSelectItems(items);
            return this;
        }
        /// <summary>
        /// 设置可选数据
        /// </summary>
        /// <param name="items">可选项列表</param>
        /// <returns></returns>
        public override BaseListComboBox SetSelectItems(List<KeyValuePair<object, string>> items)
        {
            base.SetSelectItems(items);
            return this;
        }
        #endregion
    }
}
