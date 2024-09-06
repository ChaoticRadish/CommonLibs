using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value
{
    /// <summary>
    /// 将 <see cref="TCode"/> 视为 <see cref="ILayeringAddressCode{TLayer}"/> 采用基类 <see cref="LayeringAddressCodeComparer{TLayer}"/> 的比较方法比较
    /// </summary>
    /// <typeparam name="TLayer"></typeparam>
    /// <typeparam name="TCode"></typeparam>
    /// <param name="comparer">用于比较层值的比较器, 输入 <see langword="null"/> 时将采用 <see cref="Comparer{T}.Default"/> </param>
    public class LayeringAddressCodeComparer<TLayer, TCode>(IComparer<TLayer>? comparer = null) : LayeringAddressCodeComparer<TLayer>(comparer), IComparer<TCode>
        where TCode : ILayeringAddressCode<TLayer>
    {
        public int Compare(TCode? x, TCode? y)
        {
            return base.Compare(x, y);
        }
    }

    /// <summary>
    /// <see cref="ILayeringAddressCode{TLayer}"/> 比较器, 使用传入的层值比较器实现逐层比较
    /// </summary>
    /// <remarks>
    /// 1. <see cref="ILayeringAddressCode{TLayer}.IsRange"/> => <see langword="true"/> &gt; <see langword="false"/> <br/>
    /// 2. <see langword="null"/> &lt; <see langword="not null"/> <br/>
    /// 3. 由浅到深, 逐层使用 <see cref="comparer"/> 比较相同层级下的层值, 如果相等, 则比较下一级, 否则以此结果作为最终比较结果
    /// </remarks>
    /// <typeparam name="TLayer"></typeparam>
    /// <param name="comparer">用于比较层值的比较器, 输入 <see langword="null"/> 时将采用 <see cref="Comparer{T}.Default"/> </param>
    public class LayeringAddressCodeComparer<TLayer>(IComparer<TLayer>? comparer = null) : IComparer<ILayeringAddressCode<TLayer>>
    {
        private readonly IComparer<TLayer> comparer = comparer ?? Comparer<TLayer>.Default;

        public int Compare(ILayeringAddressCode<TLayer>? x, ILayeringAddressCode<TLayer>? y)
        {
            if (x == null && y == null) return 0;
            else if (x == null && y != null) return -1;
            else if (x != null && y == null) return 1;
            else
            {
                int min = Math.Min(x!.LayerCount, y!.LayerCount);
                for (int i = 0; i < min; i++)
                {
                    TLayer vx = x.LayerValues[i];
                    TLayer vy = y.LayerValues[i];
                    int result = comparer.Compare(vx, vy);
                    if (result != 0) return result;
                }

                if (x.LayerCount < y.LayerCount) return -1;
                else if (x.LayerCount == y.LayerCount)
                {
                    if (x.IsRange == y.IsRange) return 0;
                    else return x.IsRange ? 1 : -1;
                }
                else return 1;
            }
        }
    }
}
