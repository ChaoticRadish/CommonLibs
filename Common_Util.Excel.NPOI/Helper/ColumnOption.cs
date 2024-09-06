using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    /// <summary>
    /// 列配置
    /// </summary>
    public class ColumnOption
    {
        /// <summary>
        /// 列索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 是索引列
        /// </summary>
        public bool IsIndexColumn { get; set; } = false;
        /// <summary>
        /// 索引开始值
        /// </summary>
        public int IndexStartValue { get; set; } = 1;

        /// <summary>
        /// 绑定属性名
        /// </summary>
        public string BindingPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 水平对齐方式
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }
            = HorizontalAlignment.Center;
        /// <summary>
        /// 垂直对齐方式
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }
            = VerticalAlignment.Center;
        /// <summary>
        /// 列宽
        /// </summary>
        public int ColumnWidth { get; set; } = 15;
        /// <summary>
        /// 最大列宽
        /// </summary>
        public int MaxColumnWidh { get; set; } = 55;
        /// <summary>
        /// 自动列宽
        /// </summary>
        public bool AutoColumnWidth { get; set; } = false;
        /// <summary>
        /// 自动换行
        /// </summary>
        public bool WrapText { get; set; } = true;

        /// <summary>
        /// 字体高度, 单位: 点
        /// </summary>
        public float FontHeight { get; set; } = 14;
        /// <summary>
        /// 是粗体
        /// </summary>
        public bool FontIsBold { get; set; } = false;

        /// <summary>
        /// 汇总算法
        /// </summary>
        public SummaryMethodEnum SummaryMethod { get; set; } = SummaryMethodEnum.Empty;
        /// <summary>
        /// 自定义汇总算法
        /// </summary>
        public Func<List<object?>, string>? CustomSummaryMethod { get; set; } = null;
        /// <summary>
        /// 汇总字符串格式, 以 {<see cref="SUMMARY_FLAG_STRING"/>} 代替汇总计算结果
        /// </summary>
        public string SummaryStringFormat { get; set; } = $"{{{SUMMARY_FLAG_STRING}}}";

        public const string SUMMARY_FLAG_STRING = "RESULT";
    }
    /// <summary>
    /// 汇总算法枚举
    /// </summary>
    public enum SummaryMethodEnum
    {
        /// <summary>
        /// 求和
        /// </summary>
        Sum,
        /// <summary>
        /// 最小值
        /// </summary>
        Min,
        /// <summary>
        /// 最大值
        /// </summary>
        Max,
        /// <summary>
        /// 平均值
        /// </summary>
        Average,
        /// <summary>
        /// 中位数
        /// </summary>
        Median,
        /// <summary>
        /// 个数
        /// </summary>
        Count,
        /// <summary>
        /// 分组数量
        /// </summary>
        GroupCount,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom,
        /// <summary>
        /// 空
        /// </summary>
        Empty,
    }
}
