using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.IO
{
    public static class FileSizeHelper
    {
        /// <summary>
        /// 取得尺寸字符串
        /// </summary>
        /// <param name="size"></param>
        /// <param name="reserved">保留小数位数</param>
        /// <returns></returns>
        public static string GetSizeString(long size, int reserved = 2)
        {
            int unitIndex = 0;  // 单位索引
            double valueThis = size;    // 当前单位下的数值
            while (valueThis > 1024)
            {// 满1024
                valueThis /= 1024;
                unitIndex++;    // 单位索引增加
            }
            string output = $"{valueThis.ToString($"f{(reserved >= 0 ? reserved : 0)}")} {UnitsOfMeasure[unitIndex]}";
            return output;
        }
        /// <summary>
        /// 存储单位, 从小到大
        /// </summary>
        public static string[] UnitsOfMeasure = new string[]
        {
            "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "BB", "NB", "DB"
        };
    }
}
