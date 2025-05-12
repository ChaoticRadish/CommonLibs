using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Comparer
{
    /// <summary>
    /// 树结构节点包含关系的比较结果, 注释中的 a 为左值, b 为右值
    /// </summary>
    [Flags]
    public enum TreeNodeIncludeCompareResult : byte
    {
        /// <summary>
        /// a 和 b 在同一个分支内
        /// </summary>
        SameFork = 0b0000_0001,
        /// <summary>
        /// a 和 b 不在同一个分支内
        /// </summary>
        DiffFork = 0b0000_0010,

        /// <summary>
        /// a 和 b 的深度值邻接
        /// </summary>
        AdjoinDepth = 0b0000_0100,
        /// <summary>
        /// a 和 b 的深度值不相邻
        /// </summary>
        DistantDepth = 0b0000_1000,
        /// <summary>
        /// a 和 b 处于同一个深度
        /// </summary>
        SameDepth = 0b0001_0000,
        /// <summary>
        /// a 的深度比 b 深
        /// </summary>
        Deeper = 0b0010_0000,
        /// <summary>
        /// a 的深度比 b 浅
        /// </summary>
        Shallower = 0b0100_0000,

        /// <summary>
        /// a 和 b 是同一个位置
        /// </summary>
        SameOne = SameFork | SameDepth,
        /// <summary>
        /// a 在 b 的某个子分支下 (任意深度)
        /// </summary>
        ChildFork = SameFork | Deeper,
        /// <summary>
        /// a 是 b 的直接子节点
        /// </summary>
        AdjoinChild = SameFork | Deeper | AdjoinDepth,
        /// <summary>
        /// a 是 b 的非邻接节点
        /// </summary>
        DistantChild = SameFork | Deeper | DistantDepth,
        /// <summary>
        /// a 是 b 的直接父节点
        /// </summary>
        AdjoinParent = SameFork | Shallower | AdjoinDepth,
        /// <summary>
        /// a 是 b 的非邻接父节点
        /// </summary>
        DistantParent = SameFork | Shallower | DistantDepth,
    }
}
