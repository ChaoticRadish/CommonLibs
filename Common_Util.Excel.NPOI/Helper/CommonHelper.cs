using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    public class CommonHelper
    {
        /// <summary>
        /// 取得默认的过滤器
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultFilter()
        {
            return "xlsx(2007或其后版本)|*.xlsx|xls(2003版本)|*.xls";
        }
        /// <summary>
        /// 取得xls的过滤器
        /// </summary>
        /// <returns></returns>
        public static string GetXlsFilter()
        {
            return "xls(2003版本)|*.xls";
        }
        /// <summary>
        /// 取得xlsx的过滤器
        /// </summary>
        /// <returns></returns>
        public static string GetXlsxFilter()
        {
            return "xlsx(2007或其后版本)|*.xlsx";
        }
    }
}
