using Common_Winform.Interfaces;
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
    public partial class WaitingInfoForm01 : Form, IWaitingInfoForm
    {
        public WaitingInfoForm01()
        {
            InitializeComponent();
            WaitingBox.BorderStyle = BorderStyle.None;
        }

        #region 属性
        public string Info
        {
            get => WaitingBox.Info ?? string.Empty;
            set => WaitingBox.Info = value;
        }
        #endregion

        /// <summary>
        /// 点击按钮点击事件
        /// </summary>
        public event EventHandler? OnCancelButtonClick;


        private void CancelButton_Click(object sender, EventArgs e)
        {
            OnCancelButtonClick?.Invoke(this, e);
        }
    }
}
