using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// <para>浮力数据链 (双向链) !线程不安全!</para>
    /// <para>置入数据时, 除了数据体, 还需要传入浮力值(float值)</para>
    /// <para>浮力值越小, 所处位置越接近起点</para>
    /// <para>相同浮力值时, 以置入的先后顺序排序</para>
    /// </summary>
    public class BuoyanceChain<DataType> : ICollection<BuoyanceChainNode<DataType>>
    {
        #region 链节
        /// <summary>
        /// 第一个链节
        /// </summary>
        public BuoyanceChainNode<DataType>? First { get; private set; } = null;
        /// <summary>
        /// 最后一个链环
        /// </summary>
        public BuoyanceChainNode<DataType>? Last { get; private set; } = null;
        /// <summary>
        /// 游标, 上一个添加进去的链节
        /// </summary>
        public BuoyanceChainNode<DataType>? Cursor { get; private set; } = null;
        #endregion

        #region 接口实现 - ICollection
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; private set; } = 0;
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; } = false;
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="item"></param>
        public void Add(BuoyanceChainNode<DataType> item)
        {
            if (IsReadOnly)
            {
                return;
            }
            if (First == null)  // 首个链节不是null, 此时游标和最后一个链节都不会是null
            {
                // 没有链节
                First = item;
                Last = item;

                First.Next = null;
                First.Prev = null;
            }
            else
            {
                // 存在链节
                if (item.Buoyance >= Cursor!.Buoyance)
                {
                    // 要添加的链节的浮力比游标的浮力大或相等
                    if (Cursor.Next == null)
                    {
                        // 游标是最后一个链节
                        Cursor.SetNext(item);
                        Last = item;
                    }
                    else
                    {
                        // 循环, 直到游标已经是最后一个链节或者游标的下一个链节的浮力比要添加的链节的浮力大
                        while (Cursor.Next != null && Cursor.Next.Buoyance <= item.Buoyance)
                        {
                            Cursor = Cursor.Next;
                        }
                        if (Cursor.Next == null)
                        {
                            // 游标是最后一个链节
                            Last = item;
                        }
                        Cursor.SetNext(item);
                    }
                }
                else if (item.Buoyance >= Last!.Buoyance)
                {
                    // 要添加的链节的浮力大于或等于链末的链节的浮力
                    Last.SetNext(item);
                    Last = item;
                }
                else if (item.Buoyance < Cursor!.Buoyance)
                {
                    // 要添加的链节的浮力比游标的浮力小
                    if (Cursor.Prev == null)
                    {
                        // 游标是第一个链节
                        Cursor.SetPrev(item);
                        First = item;
                    }
                    else
                    {
                        // 循环, 直到游标已经是第一个链节或者游标的上一个链节的浮力比要添加的链节的浮力小或相等
                        while (Cursor.Prev != null && Cursor.Prev.Buoyance > item.Buoyance)
                        {
                            Cursor = Cursor.Prev;
                        }
                        if (Cursor.Prev == null)
                        {
                            // 游标是第一个链节
                            First = item;
                        }
                        Cursor.SetPrev(item);
                    }
                }
            }
            // 将添加进去的物品设置为游标
            Cursor = item;
            // 数量加1
            Count++;
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="data"></param>
        /// <param name="buoyance"></param>
        public void Add(DataType data, float buoyance = 0)
        {
            if (IsReadOnly)
            {
                return;
            }
            Add(new BuoyanceChainNode<DataType>(buoyance, data));
        }
        /// <summary>
        /// 清空链
        /// </summary>
        public void Clear()
        {
            if (IsReadOnly)
            {
                return;
            }
            BuoyanceChainNode<DataType>? temp = First;
            while (temp != null)
            {
                BuoyanceChainNode<DataType> tempPrev = temp;
                temp = temp.Next;
                tempPrev.Release();
            }
            First = null;
            Last = null;
            Cursor = null;
            Count = 0;
        }
        public void Clear(bool release)
        {
            if (IsReadOnly)
            {
                return;
            }
            BuoyanceChainNode<DataType>? temp = First;
            while (temp != null)
            {
                BuoyanceChainNode<DataType> tempPrev = temp;
                temp = temp.Next;
                if(release) tempPrev.Release();
            }
            First = null;
            Last = null;
            Cursor = null;
            Count = 0;
        }

        /// <summary>
        /// 弹出链的首个链节, 会将弹出项从列表中移除, 只读时不允许弹出
        /// </summary>
        /// <returns></returns>
        public BuoyanceChainNode<DataType>? Pop()
        {
            if (IsReadOnly)
            {
                return null;
            }
            if (First == null)
            {
                return null;
            }
            BuoyanceChainNode<DataType> output = First;
            First = First.Next;
            if(First == null)
            {
                // 链空了
                Cursor = null;
                Last = null;
            }
            else
            {
                // 链未空
                First.Prev = null;
                if (output == Cursor)
                {
                    // 游标刚好指向了弹出项, 将游标指向第一个链节
                    Cursor = First;
                }
            }
            if (output != null)
            {
                output.Next = null;
                output.Prev = null;
                Count--;
            }
            return output;
        }

        /// <summary>
        /// 判断链中是否存在某一项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(BuoyanceChainNode<DataType> item)
        {
            if(Count > 0)
            {
                BuoyanceChainNode<DataType>? temp = item.Buoyance > Cursor!.Buoyance ? Cursor : First;
                while (temp != null)
                {
                    if (temp.Buoyance > item.Buoyance)
                    {
                        // temp已经移动到浮力高于item的浮力的地方了
                        break;
                    }
                    else
                    {
                        if (temp.Buoyance == item.Buoyance)
                        {
                            // temp浮力等于item的浮力, 此时两项有可能相等
                            if (temp.Data == null)
                            {
                                return item.Data == null;
                            }
                            else if (temp.Data.Equals(item.Data))
                            {
                                // 如果保存的数据相等, 返回true
                                return true;
                            }
                        }
                        /*else
                        {
                            // temp浮力小于item的浮力, 两项不可能相等
                        }*/
                        temp = temp.Next;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 将数据拷贝到传入的数组array中. 如果索引超出了数组的边界, 将不会拷贝. 本方法拷贝链节的引用, 而不是深拷贝.
        /// </summary>
        /// <param name="array">拷贝所得的引用将放入此数组</param>
        /// <param name="arrayIndex">拷贝所得的引用们在数组中的起点, 即偏移量</param>
        public void CopyTo(BuoyanceChainNode<DataType>[] array, int arrayIndex)
        {
            BuoyanceChainNode<DataType>? temp = First;
            while (temp != null && arrayIndex < array.Length)
            {
                array[arrayIndex] = temp;
                temp = temp.Next;
                arrayIndex++;
            }
        }
        /// <summary>
        /// 返回一个循环访问集合的枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<BuoyanceChainNode<DataType>> GetEnumerator()
        {
            BuoyanceChainNode<DataType>? temp = First;
            while (temp != null)
            {
                yield return temp;
                temp = temp.Next;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            BuoyanceChainNode<DataType>? temp = First;
            while (temp != null)
            {
                yield return temp;
                temp = temp.Next;
            }
        }

        /// <summary>
        /// 从链中移除一个特定的链节
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(BuoyanceChainNode<DataType> item)
        {
            if (IsReadOnly)
            {
                return false;
            }
            if (Count > 0)
            {
                BuoyanceChainNode<DataType>? temp = item.Buoyance > Cursor!.Buoyance ? Cursor : First;
                while (temp != null)
                {
                    if (temp.Buoyance > item.Buoyance)
                    {
                        // temp已经移动到浮力高于item的浮力的地方了
                        break;
                    }
                    else
                    {
                        if (temp.Buoyance == item.Buoyance)
                        {
                            // temp浮力等于item的浮力, 此时两项有可能相等
                            if ((temp.Data == null && item.Data == null) || (temp.Data != null && temp.Data.Equals(item.Data)))
                            {
                                // 如果保存的数据相等, 移除这个链节, 并返回true
                                if (Count == 1)
                                {
                                    // 如果只有一个链节
                                    First = null;
                                    Last = null;
                                    Cursor = null;
                                }
                                else
                                {
                                    if (temp.Prev == null)
                                    {
                                        // 是第一个
                                        First = temp.Next;
                                        if (First != null)
                                        {
                                            First.Prev = null;
                                        }
                                    }
                                    else
                                    {
                                        // 不是第一个
                                        temp.Prev.Next = temp.Next;
                                    }
                                    if (temp.Next == null)
                                    {
                                        // 是最后一个
                                        Last = temp.Prev;
                                        if (Last != null)
                                        {
                                            Last.Next = null;
                                        }
                                    }
                                    else
                                    {
                                        // 不是最后一个
                                        temp.Next.Prev = temp.Prev;
                                    }
                                }
                                Count--;
                                return true;
                            }
                        }
                        /*else
                        {
                            // temp浮力小于item的浮力, 两项不可能相等
                        }*/
                        temp = temp.Next;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 从链中移除一个特定的链节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="buoyance"></param>
        /// <returns></returns>
        public bool Remove(DataType data, float buoyance = 0)
        {
            if (IsReadOnly)
            {
                return false;
            }
            return Remove(new BuoyanceChainNode<DataType>(buoyance, data));
        }

        #endregion

        /// <summary>
        /// 获取数据链的数据字符串
        /// </summary>
        /// <param name="dataToStrFunc"></param>
        /// <param name="split"></param>
        /// <param name="nullValueStr">空数据的表示字符串, 仅在传入的转换方法为null时有效</param>
        /// <returns></returns>
        public string ToString(Func<DataType?, string>? dataToStrFunc, string split = ", ", string nullValueStr = "<null>")
        {
            StringBuilder sb = new StringBuilder();
            int total = Count;
            int index = 0;
            foreach (BuoyanceChainNode<DataType> node in this)
            {
                if (dataToStrFunc == null)
                {
                    if (node.Data == null)
                    {
                        sb.Append(nullValueStr);
                    }
                    else
                    {
                        sb.Append(node.Data);
                    }
                }
                else
                {
                    sb.Append(dataToStrFunc(node.Data));
                }
                index++;
                if (index != total)
                {
                    sb.Append(split);
                }
            }
            return sb.ToString();
        }
    }
    /// <summary>
    /// 链节
    /// </summary>
    public class BuoyanceChainNode<DataType>
    {
        public BuoyanceChainNode(float buoyance, DataType data)
        {
            Data = data;
            Buoyance = buoyance;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public DataType Data { get; set; }
        /// <summary>
        /// 浮力
        /// </summary>
        public float Buoyance { get; set; }

        /// <summary>
        /// 上一个链节
        /// </summary>
        public BuoyanceChainNode<DataType>? Next { get; set; }
        /// <summary>
        /// 下一个链接
        /// </summary>
        public BuoyanceChainNode<DataType>? Prev { get; set; }


        #region 控制
        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            Next = null;
            Prev = null;
            if (Data is IDisposable data)
            {
                data.Dispose();
            }
        }
        /// <summary>
        /// 设置链节为本链节的下一个链节
        /// </summary>
        public void SetNext(BuoyanceChainNode<DataType> node)
        {
            // 设当前节点为A, 当前的下一个节点为B(如果有), node为N
            // B的Prev改为N, N的Next改为B
            // A的Next改为N, N的Prev改为A
            if (Next != null)
            {
                Next.Prev = node;
            }
            node.Next = Next;
            Next = node;
            node.Prev = this;
        }
        /// <summary>
        /// 设置链节为本链节的上一个链节
        /// </summary>
        public void SetPrev(BuoyanceChainNode<DataType> node)
        {
            // 设当前节点为B, 当前的上一个节点为A(如果有), node为N
            // A的Next改为N, N的Prev改为A
            // B的Prev改为N, N的Next改为B
            if (Prev != null)
            {
                Prev.Next = node;
            }
            node.Prev = Prev;
            Prev = node;
            node.Next = this;
        }

        #endregion
    }
}
