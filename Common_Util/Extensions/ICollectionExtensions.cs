using Common_Util.Data.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ICollectionExtensions
    {
        #region 委托声明
        /// <summary>
        /// 获取集合中指定索引的子项的方法
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="collection"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public delegate TItem GetItemFromIndexHandler<TItem, TCollection>(TCollection collection, int index) where TCollection : ICollection<TItem>;
        /// <summary>
        /// 基于索引值交换集合中子项位置的处理实现
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="collection"></param>
        /// <param name="indexA">准备交换位置的子项的其一</param>
        /// <param name="indexB">准备交换位置的子项的其二</param>
        public delegate void SwapBaseIndexHandler<TItem, TCollection>(TCollection collection, int indexA, int indexB) where TCollection : ICollection<TItem>;


        #endregion

        /// <summary>
        /// 往集合中添加枚举的所有可能的值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="collection"></param>
        public static void AddEnums<TEnum>(this ICollection<TEnum> collection)
            where TEnum : Enum
        {
            EnumHelper.ForEach<TEnum>(collection.Add);
        }



        #region 排序

        /// <summary>
        /// 以交换位置的方式实现的排序. 
        /// </summary>
        /// <remarks>
        /// 排序具有破坏性, 将改变原有结构, 而不是得到一个新的序列
        /// </remarks>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="collection"></param>
        /// <param name="comparer">取得比对键值后, 比较其大小顺序的比较器, 如果为 <see langword="null"/> 将使用 <see cref="Comparer{T}.Default"/></param>
        /// <param name="desc">是否按降序排列</param>
        /// <param name="getItemFunc">获取子项的方法</param>
        /// <param name="swapFunc">交换子项位置的方法</param>
        public static void SwapWaySort<TItem, TCollection>(
            this TCollection collection,
            IComparer<TItem>? comparer,
            bool desc,
            GetItemFromIndexHandler<TItem, TCollection> getItemFunc,
            SwapBaseIndexHandler<TItem, TCollection> swapFunc)
            where TCollection : ICollection<TItem>
        {
            SwapWaySort(collection, _self, comparer, desc, getItemFunc, swapFunc);
        }
        /// <summary>
        /// 以交换位置的方式实现的排序. 
        /// </summary>
        /// <remarks>
        /// 排序具有破坏性, 将改变原有结构, 而不是得到一个新的序列
        /// </remarks>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="collection"></param>
        /// <param name="keySelector"></param>
        /// <param name="comparer">取得比对键值后, 比较其大小顺序的比较器, 如果为 <see langword="null"/> 将使用 <see cref="Comparer{T}.Default"/></param>
        /// <param name="desc">是否按降序排列</param>
        /// <param name="getItemFunc">获取子项的方法</param>
        /// <param name="swapFunc">交换子项位置的方法</param>
        public static void SwapWaySort<TItem, TKey, TCollection>(
            this TCollection collection,
            Func<TItem, TKey> keySelector,
            IComparer<TKey>? comparer, 
            bool desc,
            GetItemFromIndexHandler<TItem, TCollection> getItemFunc,
            SwapBaseIndexHandler<TItem, TCollection> swapFunc)
            where TCollection : ICollection<TItem>
        {
            var t0 = _toString(collection);
            int[] currentIndex_old2New = new int[collection.Count]; // index: 集合项的原始位置; value: 集合项的当前位置
            int[] currentIndex_new2Old = new int[collection.Count];
            List<(TItem item, int oldIndex)> input = new(collection.Count); // item: 集合项; oldIndex: 原始位置
            for (int i = 0; i < collection.Count; i++)
            {
                input.Add((getItemFunc(collection, i), i));
                currentIndex_old2New[i] = i;
                currentIndex_new2Old[i] = i;
            }
            input.Sort(new CompaperWrapper<(TItem item, int oldIndex), TKey>((obj) => keySelector(obj.item), desc, comparer));
            //var t1 = _toString(input);
            //var t2 = _toString(currentIndex_old2New);
            //var t3 = _toString(currentIndex_new2Old);
            //var t4 = _toString(collection);
            for (int i = 0; i < input.Count; i++)
            {
                var item = input[i];
                int currentInNew = currentIndex_old2New[item.oldIndex]; // 旧索引在当前状态中的位置
                int readySwap = currentIndex_new2Old[i];    // 将要被换位的项, 在初始状态下的位置, 即旧索引
                if (i != currentInNew)
                {
                    swapFunc(collection, i, currentInNew);
                    (currentIndex_old2New[item.oldIndex], currentIndex_old2New[readySwap]) = (currentIndex_old2New[readySwap], currentIndex_old2New[item.oldIndex]);
                    (currentIndex_new2Old[i], currentIndex_new2Old[currentInNew]) = (currentIndex_new2Old[currentInNew], currentIndex_new2Old[i]);
                }

                //t1 = _toString(input);
                //t2 = _toString(currentIndex_old2New);
                //t3 = _toString(currentIndex_new2Old);
                //t4 = _toString(collection);
            }
        }
        #endregion


        #region 私有

        private static T _self<T>(T obj) => obj;

        #endregion

        #region 测试


        private static string _toString<T>(IEnumerable<T> vs)
        {
            return Common_Util.String.StringHelper.Concat(vs.Select((v, i) => $"[{i}]{v}").ToList(), ", ");
        }
        #endregion
    }
}
