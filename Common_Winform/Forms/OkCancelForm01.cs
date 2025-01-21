using Common_Winform.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Forms
{
    /// <summary>
    /// 包含确认和取消按钮的弹窗, 可以在实例化时设定主体显示内容
    /// </summary>
    public partial class OkCancelForm01 : Form
    {
        public OkCancelForm01()
        {
            InitializeComponent();

            ClearBody();
            SetDefault();
        }

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string Title
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// 默认显示内容文本
        /// </summary>
        public string DefaultText 
        { 
            get => 默认显示内容.Text; 
            set
            {
                默认显示内容.Text = value;
            }
        }

        /// <summary>
        /// 设置控件为窗口的主体
        /// </summary>
        public Control? Body 
        {
            get => body;
            set
            {
                body = value;
                if (body == null)
                {
                    ClearBody();
                    SetDefault();
                }
                else
                {
                    ClearBody();
                    SetBody(body);
                }
            }
        }
        private Control? body;
        private void ClearBody()
        {
            显示容器.Controls.Clear();
        }
        private void SetDefault()
        {
            显示容器.Controls.Add(默认显示内容);
        }
        private void SetBody(Control body)
        {
            显示容器.Controls.Add(body);
            body.Dock = DockStyle.None;
            body.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            body.Visible = false;
            body.Show();
        }



    }
}
