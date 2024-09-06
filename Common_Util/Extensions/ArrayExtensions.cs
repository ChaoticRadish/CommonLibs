using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// 判断数组是否为null或空(长度0)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsEmpty(this Array? array) 
        {
            return array == null || array.Length == 0;
        }
    }
}
