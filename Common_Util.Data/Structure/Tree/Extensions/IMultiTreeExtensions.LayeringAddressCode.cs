using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Extensions
{
    public static partial class IMultiTreeExtensions
    {
        /* IMultiTree<ILayeringAddressCode<TValue>> 扩展
         * 
         */

        /// <summary>
        /// 检查结果枚举: 对节点值类型为 <see cref="ILayeringAddressCode{TValue}"/> 的多叉树分支 (可以是整个树, 也可以是某个节点下的所有节点) 的检查结果
        /// <para>其值为 0 (<see cref="Full"/>) 时表示分支是完整的没有问题的, 非 0 时有一定的缺陷, 按具体指判断具体是什么情况</para>
        /// </summary>
        [Flags]
        public enum CodeTreeForkCheckResultEnum : int
        {
            /// <summary>
            /// 完整的分支
            /// </summary>
            Full = 0,
            /// <summary>
            /// 缺失层, 出现某个节点, 其节点值不是任意子节点节点值的最小范围编码
            /// </summary>
            MissingLayer = 0b0001,
            /// <summary>
            /// 树结构出现错误, 如某个节点的节点值, 不属于其父节点节点值的范围, 或者一个项编码的节点缺拥有子节点, 等情况
            /// </summary>
            ErrorStruct = 0b0010,
            /// <summary>
            /// 同一层级出现节点值等价的子节点
            /// </summary>
            RepeatNodeValue = 0b0100,
        }

        /// <summary>
        /// 检查节点及其所有子项 (也包含次级子项) 所构成的分支是否完整 
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="node"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static CodeTreeForkCheckResultEnum CheckCodeTreeFork<TLayer>(
            this IMultiTreeNode<ILayeringAddressCode<TLayer>> node,
            IEqualityComparer<TLayer>? comparer = null)
        {
            return CheckCodeTreeFork<TLayer, ILayeringAddressCode<TLayer>>(node, comparer);
        }

        /// <summary>
        /// 检查节点及其所有子项 (也包含次级子项) 所构成的分支是否完整 
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static CodeTreeForkCheckResultEnum CheckCodeTreeFork<TLayer, TCode>(
            this IMultiTreeNode<TCode> node,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            comparer ??= EqualityComparer<TLayer>.Default;

            CodeTreeForkCheckResultEnum output = CodeTreeForkCheckResultEnum.Full;

            bool existMissingLayer = false;
            bool existErrorStrcut = false;
            bool existRepeatNodeValue = false;

            List<TCode> tempCodes = new(); 
            foreach (var item in node.IndexPreorder())
            {
                tempCodes.Add(item.NodeValue);

                if (item.ParentIndex >= 0)
                {
                    var parent = tempCodes[item.ParentIndex];

                    if (!existMissingLayer)
                    {
                        if (!parent.PathEquals(item.NodeValue.PreRangePath()))
                        {
                            existMissingLayer = true;
                        }
                    }
                    if (!existErrorStrcut)
                    {
                        if (!parent.IsRange)
                        {
                            existErrorStrcut = true;
                        }
                        else if (!item.NodeValue.IsIn(parent))
                        {
                            existErrorStrcut = true;
                        }
                    }
                }
                else
                {
                    // 只有根节点 (起始节点) 没有父节点, 这种情况下忽略
                }

                if (!existRepeatNodeValue && item.Node.Childrens.Any())
                {
                    int childCount = item.Node.Childrens.Count();
                    int distinctCount = item.Node.Childrens.Select(child => child.NodeValue.LayerValues[^1]).Distinct(comparer).Count();
                    if (childCount != distinctCount)
                    {
                        existRepeatNodeValue = true;
                    }
                }

                // 出现了所有异常情况
                if (existMissingLayer 
                    & existMissingLayer
                    & existRepeatNodeValue)
                {
                    break;
                }
            }


            output = output.AddFlagWhen(
                (existMissingLayer, CodeTreeForkCheckResultEnum.MissingLayer),
                (existErrorStrcut, CodeTreeForkCheckResultEnum.ErrorStruct),
                (existRepeatNodeValue, CodeTreeForkCheckResultEnum.RepeatNodeValue));

            return output;
        }
    }
}
