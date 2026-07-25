using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ChaoticKit.Winform.Controls.ValueBox
{
    /// <summary>
    /// 只读复选框：外观与普通 CheckBox 一致，但用户无法更改其勾选状态
    /// </summary>
    [DefaultEvent(nameof(CheckedChanged))]
    public class ReadOnlyCheckBox : CheckBox
    {
        #region 属性
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("是否只读，为 true 时用户无法更改勾选状态")]
        public bool ReadOnly
        {
            get => readOnly;
            set => readOnly = value;
        }
        private bool readOnly = true;
        #endregion

        protected override void OnClick(EventArgs e)
        {
            if (!readOnly)
            {
                base.OnClick(e);
            }
        }
    }
}
