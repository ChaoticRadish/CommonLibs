using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 汇总信息
    /// </summary>
    public class SummaryAttribute : Attribute
    {
        public SummaryAttribute()
        {

        }
        
        public SummaryAttribute(SummaryMethodEnum summaryMethod)
        {
            SummaryMethod = summaryMethod;
        }


        /// <summary>
        /// 汇总算法
        /// </summary>
        public SummaryMethodEnum SummaryMethod { get; set; } = SummaryMethodEnum.Sum;
        /// <summary>
        /// 自定义汇总算法
        /// </summary>
        public Func<List<object?>, string>? CustomSummaryMethod { get; set; } = null;
        /// <summary>
        /// 汇总字符串格式, 以 {result} 代替汇总计算结果
        /// </summary>
        public string SummaryStringFormat { get; set; } = "{result}";
    }
}
