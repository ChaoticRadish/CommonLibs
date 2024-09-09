using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 可以通过某一 <see cref="TValue"/> 具体值创建根节点的树结构
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface ICanCreateRootNodeFrom<TValue> : ITree<TValue>
    {
        /// <summary>
        /// 使用传入的值创建根节点
        /// </summary>
        /// <param name="value"></param>
        void CreateRootNode(TValue value);
    }

    /// <summary>
    /// 拥有一个特定类型的根节点
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public interface IHasRootNodeAs<TNode> : ITree
    {
        TNode? Root { get; }
    }
}
