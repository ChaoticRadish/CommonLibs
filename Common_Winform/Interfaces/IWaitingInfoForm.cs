using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Interfaces
{
    public interface IWaitingInfoForm
    {
        /// <summary>
        /// 需要显示的等待信息
        /// </summary>
        string Info { get; set; }
    }
}
