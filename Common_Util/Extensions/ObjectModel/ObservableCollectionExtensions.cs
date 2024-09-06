using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.ObjectModel
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// 交换集合中两个索引对应子项的位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="indexA"></param>
        /// <param name="indexB"></param>
        public static void Swap<T>(this ObservableCollection<T> collection, int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= collection.Count)
            {
                throw new IndexOutOfRangeException($"{nameof(indexA)} 不在集合的索引范围内");
            }
            if (indexB < 0 || indexB >= collection.Count)
            {
                throw new IndexOutOfRangeException($"{nameof(indexB)} 不在集合的索引范围内");
            }
            swap(collection, indexA, indexB);
        }
        /// <summary>
        /// 交换集合中两个索引对应子项的位置的具体实现, 不对输入参数作检查
        /// </summary>
        /// <returns>实际是否发生了交换</returns>
        private static bool swap<T>(ObservableCollection<T> collection, int indexA, int indexB)
        {
            if (indexA == indexB)
            {
                return false;
            }
            Common_Util.Maths.CompareHelper.JudgeBigger(ref indexA, ref indexB);    // 执行后, A < B
            collection.Move(indexA, indexB);
            collection.Move(indexB - 1, indexA);
            return true;
        }


        /// <summary>
        /// 使用传入的比较器对集合作排序, 如果比较器为 <see langword="null"/> 则使用默认比较器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        /// <param name="desc"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this ObservableCollection<T> collection, IComparer<T>? comparer = null, bool desc = false)
        {
            collection.SwapWaySort(
                comparer, desc, 
                static (c, i) => c[i], 
                static (c, iA, iB) => swap(c, iA, iB));
        }

        /// <summary>
        /// 使用传入的比较器, 根据指定的键值, 对集合作排序, 如果比较器为 <see langword="null"/> 则使用默认比较器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="collection"></param>
        /// <param name="keySelector"></param>
        /// <param name="comparer"></param>
        /// <param name="desc"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TKey>(this ObservableCollection<T> collection, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null, bool desc = false)
        {
            collection.SwapWaySort(
                keySelector, comparer, desc, 
                static (c, i) => c[i],
                static (c, iA, iB) => swap(c, iA, iB));
        }




    }
}
