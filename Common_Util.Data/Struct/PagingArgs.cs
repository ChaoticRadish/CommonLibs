using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 分页查询时的页码参数
    /// </summary>
    public struct PagingArgs
    {
        public PagingArgs(int pageIndex, int pageSize) 
        {
            this.pageIndex = 0;
            this.pageSize = 1;

            PageIndex = pageIndex;
            PageSize = pageSize;
        }
        /// <summary>
        /// 页面索引, 从0开始计算
        /// </summary>
        public int PageIndex
        {
            get => Common_Util.Input.ValueRevision.ToRange(ref pageIndex, 0);
            set => pageIndex = Common_Util.Input.ValueRevision.ToRange(value, 0);
        }
        private int pageIndex;
        /// <summary>
        /// 页码, 从1开始记算
        /// </summary>
        public int PageCode
        {
            get => PageIndex + 1;
            set => PageIndex = value - 1;
        }

        /// <summary>
        /// 页面大小/容量, 最小值: 1
        /// </summary>
        public int PageSize
        {
            get => Common_Util.Input.ValueRevision.ToRange(ref pageSize, 1);
            set => pageSize = Common_Util.Input.ValueRevision.ToRange(value, 1);
        }
        private int pageSize;

        /// <summary>
        /// 目标页码的开始索引
        /// </summary>
        public int PageStart 
        {
            get
            {
                return PageIndex * PageSize;
            } 
        }

    }
}
