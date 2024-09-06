using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 可被排序的某个东西, 其子项为泛型参数 <see cref="T"/>
    /// </summary>
    /// <remarks>
    /// 排序具有破坏性, 将改变原有结构, 而不是得到一个新的序列
    /// </remarks>
    public interface ISortable<T>
    {
        /// <summary>
        /// 使用传入的比较器执行排序
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="desc">是否采用降序排序</param>
        void Sort(IComparer<T> comparer, bool desc);
    }
}
