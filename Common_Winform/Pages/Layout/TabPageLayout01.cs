using Common_Winform.Controls;
using Common_Winform.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Pages.Layout
{
    /// <summary>
    /// 只有一层的分页标签布局, 仅支持正数作为页面索引
    /// </summary>
    [ToolboxItem(true)]
    public partial class TabPageLayout01 : TabPageLayout01Base
    {
        public TabPageLayout01()
        {
            InitializeComponent();
        }

        #region 自动生成子控件
        /// <summary>
        /// 标签页切换区域的标签控件
        /// </summary>
        private Dictionary<int, LabelSimpleState01> Labels = new Dictionary<int, LabelSimpleState01>();
        #endregion

        #region 参数
        [Category("CVII_自定义_参数"), DisplayName("标签区宽度")]
        public int TabAreaWidth
        {
            get => Area_TabController.Width;
            set
            {
                SuspendLayout();
                Area_TabController.Width = value;
                foreach (LabelSimpleState01 label in Labels.Values)
                {
                    label.Width = Area_TabControllerInnerBox.Width;
                    label.Invalidate();
                }
                ResumeLayout();
                //Invalidate();
            }
        }
        #endregion

        #region 静态参数
        private readonly static string 无名称状态下默认标签名 = "无名标签页";
        #endregion

        #region 初始化过程参数
        [Category("CVII_自定义_初始化过程参数"), DisplayName("标签间距")]
        public int 初始化过程参数_标签间距 { get; set; } = 3;

        [Category("CVII_自定义_初始化过程参数"), DisplayName("标签默认长度")]
        public ushort 初始化过程参数_标签默认长度 { get; set; } = 100;


        #endregion


        public override void InitBody()
        {
            int 无名标签页总数 = 0;

            int 标签计数 = 0;
            int 总长度计数_不含间隙 = 0;
            int[] indexArr = Items.Keys.ToArray();
            foreach (var index in indexArr)
            {
                TabPageLayout01Item? item = Items[index];
                if (item == null) continue;

                string 显示文本 = item.PageName ?? item.PageDesc ?? $"{无名称状态下默认标签名} {++无名标签页总数}";
                StringBuilder 显示文本处理 = new StringBuilder();
                foreach (char c in 显示文本)
                {
                    if (c >= '0' && c <= '9' || c == '.')
                    {
                        显示文本处理.Append(c);
                    }
                    else if (c == ' ' || c == '\n')
                    {
                        continue;
                    }
                    else
                    {
                        显示文本处理.Append(c).AppendLine();
                    }
                }
                显示文本 = 显示文本处理.ToString();
                int y = 总长度计数_不含间隙 + 标签计数 * 初始化过程参数_标签间距;
                int 标签宽度 = Area_TabController.Width;
                LabelSimpleState01 label = new()
                {
                    ShowingText = 显示文本,
                    State = item.页面初始状态,
                    Width = 标签宽度,
                    Height = item.标签长度 > 0 ? item.标签长度 : 初始化过程参数_标签默认长度,
                    Location = new Point(0, y),
                    Font = item.专用字体 ?? Font,
                    Tag = index,
                };
                label.Click += Label_Click;
                Area_TabControllerInnerBox.Controls.Add(label);
                Labels.Add(index, label);

                总长度计数_不含间隙 += label.Height;
                标签计数++;
            }
        }

        #region 页面展示
        public override void ShowPage(int pageIndex)
        {
            RemoveShowPage();
            if (pageIndex <= 0) return;
            TabPageLayout01Item? item = this[pageIndex];
            if (item == null || item.Page == null) return;
            SetCheckLabel(item.Index);
            if (item.Page != null)
            {
                LabelSimpleState01 label = Labels[pageIndex];
                switch (label.State)
                {
                    case SimpleStateEnum.ReadOnly:
                        item.Page.ReadOnly = true;
                        break;
                    case SimpleStateEnum.Disable:
                        item.Page.Enabled = false;
                        break;
                }
                SetShowPage(item.Page);
            }

        }
        /// <summary>
        /// 移除页面展示
        /// </summary>
        private void RemoveShowPage()
        {
            Area_InnerBody.Controls.Clear();
        }
        /// <summary>
        /// 设置当前展示的页面
        /// </summary>
        /// <param name="page"></param>
        private void SetShowPage(PageBase page)
        {
            page.Dock = DockStyle.Fill;
            Area_InnerBody.Controls.Add(page);
            page.BringToFront();
        }


        #endregion

        #region 标签按钮事件
        /// <summary>
        /// 设置选中了指定的标签
        /// </summary>
        /// <param name="index"></param>
        private void SetCheckLabel(int index)
        {
            int[] indies = Labels.Keys.ToArray();
            foreach (int i in indies)
            {
                LabelSimpleState01 label = Labels[i];
                if (label.State == SimpleStateEnum.Disable || label.State == SimpleStateEnum.ReadOnly)
                {
                    continue;
                }
                else if ((int)label.Tag == index)
                {
                    label.State = SimpleStateEnum.Checked;
                }
                else
                {
                    label.State = SimpleStateEnum.UnChecked;
                }
            }
        }
        private void Label_Click(object? sender, EventArgs e)
        {
            // 获取对应页面
            if (sender == null) return;
            LabelSimpleState01 label = (LabelSimpleState01)sender;
            int index = (int)label.Tag;
            TabPageLayout01Item? item = this[index];
            if (item == null) return;

            SetCheckLabel(index);
            switch (label.State)
            {
                case SimpleStateEnum.Checked:
                    ShowPage(index);
                    break;
                case SimpleStateEnum.UnChecked:
                    ShowPage(index);
                    break;
                case SimpleStateEnum.ReadOnly:
                    ShowPage(index);
                    break;
                case SimpleStateEnum.Disable:
                    // 什么都不做
                    break;
            }
        }


        #endregion

    }


    public class TabPageLayout01Item : MultiPageLayoutItem
    {
        public bool 页面只读 { get; set; }
        
        public bool 页面禁用 { get; set; }

        public int 标签长度 { get; set; }

        public Font? 专用字体 { get; set; }

        public SimpleStateEnum 页面初始状态 
        {
            get 
            {
                if (页面禁用) return SimpleStateEnum.Disable;
                else if (页面只读) return SimpleStateEnum.ReadOnly;
                else return SimpleStateEnum.UnChecked;
            }
        }
    }
}
