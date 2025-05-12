using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Owner
{
    /// <summary>
    /// 有父代的东西 (如一个拥有父节点的节点)
    /// </summary>
    /// <typeparam name="T">附带类型</typeparam>
    public interface IParentOwner<T>
    {
        T? Parent { get; }
    }
    /// <summary>
    /// 有相同类型的父代的东西
    /// </summary>
    /// <typeparam name="TSame"></typeparam>
    public interface ISameTypeParentOwner<TSame> : IParentOwner<TSame>
        where TSame : IParentOwner<TSame>
    {
        new TSame? Parent { get; }
    }
}
