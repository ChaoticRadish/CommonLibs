using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common_Util.Interfaces.Behavior;

namespace Common_Util.Extensions.Behavior
{
    /// <summary>
    /// <see cref="ISortable{T}"/> 的扩展方法
    /// </summary>
    public static class ISortableExtensions
    {
        /// <summary>
        /// 使用 <see cref="Comparer{T}.Default"/> <b>升序</b> 执行排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortable"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static void Sort<T>(this ISortable<T> sortable)
        {
            sortable.Sort(Comparer<T>.Default, false);
        }
        /// <summary>
        /// 使用 <see cref="Comparer{T}.Default"/> <b>降序</b> 执行排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortable"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SortDesc<T>(this ISortable<T> sortable)
        {
            sortable.Sort(Comparer<T>.Default, true);
        }
    }
}
