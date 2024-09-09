using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 树类型的接口
    /// </summary>
    public interface ITree
    {
    }
    /// <summary>
    /// 用于描述特定值类型构成树结构的接口
    /// </summary>
    /// <remarks>
    /// 根据实现, 这个值类型可以被包含在每个节点, 也可以仅被叶子节点包含, 或其他情况, 这需要根据具体实现决定
    /// </remarks>
    /// <typeparam name="TValue"></typeparam>
    public interface ITree<TValue> : ITree
    {
    }
}
