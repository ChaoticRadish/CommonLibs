using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Common_Wpf.Controls.FeatureGroup
{
    public class LogFlowBoxModel
    {
        private readonly LogData logData;

        /// <summary>
        /// 仅用于测试
        /// </summary>
        public LogFlowBoxModel()
        {
            logData = new LogData()
            {
                Time = DateTime.Now,
                Category = "Category",
                Level = "Level",
                SubCategory = "SubCategory",
                Message = "测试文本!",
                Exception = null,
            };
        }

        public LogFlowBoxModel(LogData logData)
        {
            this.logData = logData;
        }

        /// <summary>
        /// 自定义显示时间字符串, 如果为null, 将显示以全局默认格式转换后的值
        /// </summary>
        public string? CustomTimeStr { get; set; }
        public string TimeStr { get => CustomTimeStr ?? logData.Time.ToString(Globals.GlobalResources.FullTimeFormat); }


        public string? CustomLevelStr { get; set; }
        public string Level { get => CustomLevelStr ?? logData.Level; }


        public string? CustomCategoryStr { get; set; }
        public string Category { get => CustomCategoryStr ?? logData.Category; }


        public string? CustomSubCategoryStr { get; set; }
        public string SubCategory { get => CustomSubCategoryStr ?? logData.SubCategory; }

        public string? CustomMessage { get; set; }
        public string Message { get => CustomMessage ?? logData.Message; }

        public Exception? Exception { get => logData.Exception; }

        public Brush? CustomBackColor { get; set; }
        public Brush? CustomBorderColor { get; set; }
        public Brush? CustomForeColor { get; set; }
    }
}
