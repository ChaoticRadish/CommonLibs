using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    /// <summary>
    /// 图片文件信息
    /// </summary>
    public class ImageFileInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public required string Path { get; set; }
        /// <summary>
        /// 是否存在
        /// </summary>
        public required bool Exist { get; set; }
        /// <summary>
        /// 图片窄边限制, 图片宽度和长度中较短的一边, 尺寸不应该超过这个数值, 如果为零, 不做限制
        /// </summary>
        public int ImageSizeLimit
        {
            get; set;
        } = 0;
    }
}
