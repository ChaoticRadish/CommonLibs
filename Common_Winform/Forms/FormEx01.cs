using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Forms
{
    public class FormEx01 : Form
    {
        [Category("CVII_自定义_参数"), DisplayName("是否启用日志")]
        public bool EnableLog { get; set; }

        public void LogInfo(string info) { LogInfoImpl(info); }
        protected virtual void LogInfoImpl(string info) { }

        public void LogWarning(string warning) { LogWarningImpl(warning); }
        protected virtual void LogWarningImpl(string warning) { }

        public void LogError(string error, Exception? exception = null) { LogErrorImpl(error, exception); }
        protected virtual void LogErrorImpl(string error, Exception? exception = null) { }
    }
}
