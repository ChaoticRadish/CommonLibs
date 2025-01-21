using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Forms
{
    /// <summary>
    /// 询问 "确定" 或 "取消" 的窗口. 显示内容为设定的文本
    /// </summary>
    public partial class AskOkCancelForm01 : Form, IAskOkCancelForm
    {
        public AskOkCancelForm01()
        {
            InitializeComponent();
        }

        public string Title { get => Text; set => Text = value; }

        public string ShowingText { get => Shower_主要信息.Text; set => Shower_主要信息.Text = value; }

    }
}
