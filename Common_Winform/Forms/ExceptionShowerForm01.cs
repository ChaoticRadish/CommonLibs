using Common_Util.Extensions;
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
    public partial class ExceptionShowerForm01 : Form
    {
        public ExceptionShowerForm01()
        {
            InitializeComponent();

            if (DesignMode)
            {
                _exception = new Exception("测试");
                RefreshShowing();
            }
            else
            {
                ClearShowing();
            }
        }

        public Exception? Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
                if (_exception == null)
                {
                    ClearShowing();
                }
                else
                {
                    RefreshShowing();
                }
            }
        }
        private Exception? _exception;

        private void ClearShowing()
        {

        }
        private void RefreshShowing()
        {
            if (_exception == null)
            {
                throw new NullReferenceException("异常为空");
            }
            Shower_异常类型.Text = typeof(Exception).FullName;
            Shower_HRESULT.Text = $"0x{_exception.HResult:X8}";
            Shower_HelpLink.Text = _exception.HelpLink;
            Shower_Message.Text = _exception.Message;
            Shower_Source.Text = _exception.Source;
            Shower_StackTrace.Text = _exception.StackTrace;
            Shower_TargetSite.Text = _exception.TargetSite?.ToString() ?? "<null>";
            Shower_Data.Text = _exception.Data.FullInfoString();
            Shower_InnerException.Text = _exception.InnerException?.Message;
            Button_详情.Enabled = _exception.InnerException != null;
        }

        private void Button_详情_Click(object sender, EventArgs e)
        {
            if (_exception == null || _exception.InnerException == null) return;
            ExceptionShowerForm01 form = new ExceptionShowerForm01()
            {
                Exception = _exception,
                Text = "源异常信息",
            };
            form.ShowDialog();
        }
    }
}
