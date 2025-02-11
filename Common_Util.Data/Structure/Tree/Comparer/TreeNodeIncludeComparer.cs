using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Comparer
{

    /// <summary>
    /// 树结构节点包含关系的比较器
    /// </summary>
    /// <typeparam name="TValue">节点值</typeparam>
    public interface ITreeNodeIncludeComparer<TValue>
    {
        /// <summary>
        /// 判断 <paramref name="left"/> 和 <paramref name="right"/> 是否在同一个分支里
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        bool SameForkCheck(TValue? left, TValue? right);
        /// <summary>
        /// 计算 <paramref name="left"/> 和 <paramref name="right"/> 的深度差值
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>
        /// 正数: <paramref name="left"/> 深于 <paramref name="right"/>, 值为 1 时表示相邻 <br/>
        /// 负数: <paramref name="left"/> 浅于 <paramref name="right"/>, 值为 -1 时表示相邻 <br/>
        /// 0: <paramref name="left"/> 和 <paramref name="right"/> 深度相同
        /// </returns>
        int DepthDifference(TValue? left, TValue? right);
    }
    /// <summary>
    /// 对 <see cref="ITreeNodeIncludeComparer{TValue}"/> 的包装类
    /// </summary>
    /// <typeparam name="TOutter">包装后的传入值类型</typeparam>
    /// <typeparam name="TInner">被包装的比较器 <see cref="Comparer"/> 所需值类型</typeparam>
    internal readonly struct TreeNodeIncludeComparerWrapper<TOutter, TInner> : ITreeNodeIncludeComparer<TOutter>
    {
        /// <summary>
        /// 转换传入值为 <see cref="Comparer"/> 需要的值的方法
        /// </summary>
        public required Func<TOutter?, TInner?> GetValue { get; init; }
        /// <summary>
        /// 被包装的比较器
        /// </summary>
        public required ITreeNodeIncludeComparer<TInner> Comparer { get; init; }

        public int DepthDifference(TOutter? left, TOutter? right)
        {
            return Comparer.DepthDifference(GetValue(left), GetValue(right));
        }

        public bool SameForkCheck(TOutter? left, TOutter? right)
        {
            return Comparer.SameForkCheck(GetValue(left), GetValue(right));
        }
    }
    internal readonly struct TreeNodeIncludeComparerWrapperToDepthComparer<TValue> : IComparer<TValue>
    {
        /// <summary>
        /// 被包装的比较器
        /// </summary>
        public required ITreeNodeIncludeComparer<TValue> Comparer { get; init; }

        public int Compare(TValue? x, TValue? y)
        {
            var diff = Comparer.DepthDifference(x, y);
            if (diff > 0) return 1;
            else if (diff < 0) return -1;
            else return 0;
        }
    }

    public static class ITreeNodeIncludeComparerExtensions
    {
        #region 包装

        /// <summary>
        /// 包装传入的比较器 <paramref name="comparer"/>, 为其加上一层外壳, 以允许传入另一种类型的值
        /// </summary>
        /// <typeparam name="TOutter">新的传入类型</typeparam>
        /// <typeparam name="TInner">被包装的比较器所需的类型</typeparam>
        /// <param name="comparer"></param>
        /// <param name="getValueFunc">如何从新的传入值中, 取得被包装的比较器需要的值</param>
        /// <returns></returns>
        public static ITreeNodeIncludeComparer<TOutter> Shell<TOutter, TInner>(this ITreeNodeIncludeComparer<TInner> comparer, Func<TOutter?, TInner?> getValueFunc)
        {
            return new TreeNodeIncludeComparerWrapper<TOutter, TInner>()
            {
                Comparer = comparer,
                GetValue = getValueFunc
            };
        }
        /// <summary>
        /// 将传入的比较器 <paramref name="comparer"/> 包装为深度比较器
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IComparer<TValue> DepthComparer<TValue>(this ITreeNodeIncludeComparer<TValue> comparer)
        {
            return new TreeNodeIncludeComparerWrapperToDepthComparer<TValue>() { Comparer = comparer };
        }

        #endregion

        /// <summary>
        /// 使用比较器 <paramref name="comparer"/> 比较传入的两个值 <paramref name="left"/> <paramref name="right"/> 的包含关系
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="comparer"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static TreeNodeIncludeCompareResult IncludeComparer<TValue>(this ITreeNodeIncludeComparer<TValue> comparer, TValue left, TValue right)
        {
            bool sameFork = comparer.SameForkCheck(left, right);
            int depthDiff = comparer.DepthDifference(left, right);
            TreeNodeIncludeCompareResult result = 0;
            result |= sameFork ? TreeNodeIncludeCompareResult.SameFork : TreeNodeIncludeCompareResult.DiffFork;
            if (depthDiff == 0)
            {
                result |= TreeNodeIncludeCompareResult.SameDepth;
            }
            else 
            {
                result |= depthDiff == 1 || depthDiff == -1 ? TreeNodeIncludeCompareResult.AdjoinDepth : TreeNodeIncludeCompareResult.DistantDepth;
                result |= depthDiff > 0 ? TreeNodeIncludeCompareResult.Deeper : TreeNodeIncludeCompareResult.Shallower;
            }
            return result;
        }

    }
}
