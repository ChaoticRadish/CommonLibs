using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.LinearCollection
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 将原始对象适配为数组类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[] AsArray<T>(this IEnumerable<T> source)
        {
            if (source is T[] arr) return arr;
            return source.ToArray();
        }

    }
}
