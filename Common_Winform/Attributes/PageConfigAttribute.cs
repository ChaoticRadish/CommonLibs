using Common_Util.Data.Attributes;
using Common_Winform.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Attributes
{
    /// <summary>
    /// 页面配置信息
    /// </summary>
    public class PageConfigAttribute : PageAttribute
    {
        /// <summary>
        /// 是否忽略此页面
        /// </summary>
        public bool Ignore { get; set; }

        #region 索引 / 分组
        /// <summary>
        /// 页面索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 页面所属组
        /// </summary>
        public string? Group { get; set; }
        #endregion


        #region 展示信息
        /// <summary>
        /// 展示名
        /// </summary>
        public string? ShowName { get; set; }

        /// <summary>
        /// 日志名字
        /// </summary>
        public string? LogName { get; set; }

        /// <summary>
        /// 描述性的信息
        /// </summary>
        public string? Desc { get; set; }
        #endregion

        #region 初始状态
        public bool ReadOnly { get; set; } = false;
        public bool Enable { get; set; } = true;
        #endregion


    }
}
