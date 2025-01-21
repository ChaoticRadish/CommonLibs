using Common_Util.Data.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static partial class IEnumerableExtensions
    {


        #region T : any
        /// <summary>
        /// 调用集合内的所有实现 <see cref="IDisposable"/> 项的 <see cref="IDisposable.Dispose"/> 方法, 释放所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void TryDisposeAll<T>(this IEnumerable<T> array)
        {
            foreach (T t in array)
            {
                if (t is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        #region 无序比较
        /// <summary>
        /// 比较两个集合内的元素能否一一对应. 此比较与顺序无关, 类似于连连看.
        /// </summary>
        /// <remarks>
        /// 该实现会采用 <paramref name="equalityComparer"/> 的 <see cref="IEqualityComparer{T}.GetHashCode(T)"/> 方法获取哈希码, 而不是通过 <see cref="object.GetHashCode"/> 方法来获取 <br/>
        /// 当 <paramref name="thisOne"/> 和 <paramref name="otherOne"/> 拥有元素值相等但是数量不相等的元素时, 将 <br/>
        /// 注: 该实现会将集合转换为数组来实现, 数据量较大时可能会有性能问题! <br/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisOne"></param>
        /// <param name="otherOne"></param>
        /// <returns></returns>
        public static bool DisorderEquals<T>(this IEnumerable<T> thisOne, IEnumerable<T> otherOne, IEqualityComparer<T>? equalityComparer = null)
        {
            var arr1 = thisOne.ToArray();
            var arr2 = otherOne.ToArray();

            if (arr1.Length != arr2.Length) return false;
            if (arr1.Length == 0) return true;

            equalityComparer ??= EqualityComparer<T>.Default;

            SortedDictionary<int, List<T>> group1 = [];
            int arr1NullCount = 0;

            SortedDictionary<int, List<T>> group2 = [];
            int arr2NullCount = 0;

            static void tidyHashCode(T[] arr, SortedDictionary<int, List<T>> groupContainer, ref int count, IEqualityComparer<T> equalityComparer)
            {
                foreach (T? item in arr)
                {
                    if (item is null) count++;
                    else
                    {
                        int hashCode = equalityComparer.GetHashCode(item);
                        if (groupContainer.TryGetOrAdd(hashCode, static () => [], out var list))
                        {
                            list.Add(item);
                        }
                        else
                        {
                            throw new InvalidOperationException($"未能添加新项 (hashCode: {hashCode}) 到字典中");
                        }
                    }
                }
            }
            tidyHashCode(arr1, group1, ref arr1NullCount, equalityComparer);
            tidyHashCode(arr2, group2, ref arr2NullCount, equalityComparer);

            if (arr1NullCount != arr2NullCount) return false;
            if (group1.Count != group2.Count) return false;
            foreach (var (kvp1, kvp2) in (group1, group2).UntilAnyAway())   // 两者数量相等, 会同时结束
            {
                if (kvp1.Key != kvp2.Key) return false;                 // 顺序排列的字典, 相同位置元素的哈希码不等时, 肯定会出现无法对应上的情况
                if (kvp1.Value.Count != kvp2.Value.Count) return false; // 同时, 相同哈希码的元素数量不等时, 也会出现无法对应上的情况
            }

            List<int> ignoreIndexMark = []; // 比较两组数据中相同哈希码的项的过程中, 标记后者中已有对应关系的项
            foreach (int hashCode in group1.Keys)
            {
                var list1 = group1[hashCode];
                var list2 = group2[hashCode];

                if (list1.Count == 1)
                {
                    if (equalityComparer.Equals(list1[0], list2[0]))
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }

                // 两组数据均存在多个相同 HashCode 的项, 比较两组分别拥有的能否一一对应消除
                ignoreIndexMark.Clear();

                foreach (T item1 in list1)
                {
                    bool foundFlag = false;
                    for (int index = 0; index < list2.Count; index++)
                    {
                        if (ignoreIndexMark.Contains(index)) continue;  // 忽略已有对应关系的项

                        T item2 = list2[index];
                        if (equalityComparer.Equals(item1, item2))
                        {
                            ignoreIndexMark.Add(index);
                            foundFlag = true;
                            break;
                        }
                    }
                    if (!foundFlag) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较并排除两个集合内能够一一对应的元素, 最终返回两组元素中相斥的部分. 此比较与顺序无关, 类似于连连看.
        /// </summary>
        /// <remarks>
        /// 该实现会采用 <paramref name="equalityComparer"/> 的 <see cref="IEqualityComparer{T}.GetHashCode(T)"/> 方法获取哈希码, 而不是通过 <see cref="object.GetHashCode"/> 方法来获取 <br/>
        /// 当两个集合中有相等的元素 A, <paramref name="thisOne"/> 有 2 个, <paramref name="otherOne"/> 有 1 个, 此时排除一一对应元素后, <paramref name="thisOne"/> 还会剩余 1 个, 而 <paramref name="otherOne"/> 就没有了 <br/>
        /// 注: 该实现会将集合转换为数组来实现, 数据量较大时可能会有性能问题! <br/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisOne"></param>
        /// <param name="otherOne"></param>
        /// <param name="equalityComparer"></param>
        /// <returns></returns>
        public static (IEnumerable<T> ThisOneExclusion, IEnumerable<T> OtherOneExclusion) ExcludeDisorderEquals<T>(
            this IEnumerable<T> thisOne, IEnumerable<T> otherOne, 
            IEqualityComparer<T>? equalityComparer = null)
        {
            var arr1 = thisOne.ToArray();
            var arr2 = otherOne.ToArray();

            // 任意一方的长度为 0 时, 不用作排除操作
            if (arr1.Length == 0 || arr2.Length == 0)
            {
                return (arr1, arr2);
            }

            equalityComparer ??= EqualityComparer<T>.Default;

            // null 值元素
            IndexMask nullIndexMask1 = new(arr1.Length, false);
            int nullCount1 = 0;
            IndexMask nullIndexMask2 = new(arr2.Length, false);
            int nullCount2 = 0;

            SortedDictionary<int, List<(T item, int index)>> group1 = [];
            SortedDictionary<int, List<(T item, int index)>> group2 = [];
            static void tidyHashCode(T[] arr, IndexMask nullIndexMaskArr, SortedDictionary<int, List<(T item, int index)>> groupContainer, ref int nullCount, IEqualityComparer<T> equalityComparer)
            {
                int index = 0;
                foreach (T? item in arr)
                {
                    if (item is null) 
                    {
                        nullIndexMaskArr[index] = true;
                        nullCount++;
                    }
                    else
                    {
                        int hashCode = equalityComparer.GetHashCode(item);
                        if (groupContainer.TryGetOrAdd(hashCode, static () => [], out var list))
                        {
                            list.Add((item, index));
                        }
                        else
                        {
                            throw new InvalidOperationException($"未能添加新项 (hashCode: {hashCode}) 到字典中");
                        }
                    }
                    index++;
                }
            }

            tidyHashCode(arr1, nullIndexMask1, group1, ref nullCount1, equalityComparer);
            tidyHashCode(arr2, nullIndexMask2, group2, ref nullCount2, equalityComparer);

            // 已被排除项
            IndexMask excluedIndexMask1;
            IndexMask excluedIndexMask2;
            // 初始排除掉能够匹配上的 null 值
            if (nullCount1 == nullCount2)
            {
                excluedIndexMask1 = nullIndexMask1;
                excluedIndexMask2 = nullIndexMask2;
            }
            else
            {
                int minNullCount = Math.Min(nullCount1, nullCount2);
                int counter1 = 0;
                int counter2 = 0;
                excluedIndexMask1 = new(nullIndexMask1.All().Select(i =>
                {
                    if (!i) return false;
                    else return counter1++ < minNullCount;
                }), arr1.Length);
                excluedIndexMask2 = new(nullIndexMask2.All().Select(i =>
                {
                    if (!i) return false;
                    else return counter2++ < minNullCount;
                }), arr2.Length);
            }

            foreach (var (kvp1, kvp2) in group1.Join(group2, g1 => g1.Key, g2 => g2.Key, (k1, k2) => (k1, k2)))
            {
                // true => 已被消除
                IndexMask kvp1IndexMask = new(kvp1.Value.Count, false);
                IndexMask kvp2IndexMask = new(kvp2.Value.Count, false);

                foreach (var (i1, v1) in kvp1.Value.WithIndex())
                {
                    int foundInSub2 = -1;
                    int foundInInput2 = -1;
                    foreach (var (i2, v2) in kvp2.Value.WithIndex().Filtering(kvp2IndexMask))
                    {
                        if (equalityComparer.Equals(v1.item, v2.item))
                        {
                            foundInSub2 = i2;
                            foundInInput2 = v2.index;
                            break;
                        }
                    }
                    if (foundInSub2 >= 0)
                    {
                        // 标记已匹配上
                        kvp1IndexMask[i1] = true;
                        kvp2IndexMask[foundInSub2] = true;
                        excluedIndexMask1[v1.index] = true;
                        excluedIndexMask2[foundInInput2] = true;
                    }
                }
            }

            return (arr1.Filtering(excluedIndexMask1), arr2.Filtering(excluedIndexMask2));
        }
        #endregion


        #region 遍历
        /// <summary>
        /// 取得顺序遍历传入集合的遍历器, 同时附带遍历索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="startIndex">起始索引</param>
        /// <returns></returns>
        public static IEnumerable<(int index, T obj)> WithIndex<T>(this IEnumerable<T> enumerable, int startIndex = 0)
        {
            foreach (var item in enumerable)
            {
                yield return (startIndex, item);
                startIndex++;
            }
        }
        /// <summary>
        /// 对 <paramref name="enumerable"/> 中的每一项都执行指定的操作. 
        /// </summary>
        /// <remarks>
        /// 此方法用于链式调用时, 中途需要执行集合项某个方法的场景 <br/>
        /// 由于 <see cref="IEnumerable{T}"/> 是延迟执行的, 如果需要立即全部执行, 需要调用 <see cref="Finish{T}(IEnumerable{T})"/> 或其他遍历操作以完成执行
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="before">遍历过程中, <see langword="yield"/> <see langword="return"/> 之前对子项作一些操作</param>
        /// <param name="after">遍历过程中, <see langword="yield"/> <see langword="return"/> 之后对子项作一些操作</param>
        /// <returns></returns>
        public static IEnumerable<T> Invoke<T>(this IEnumerable<T> enumerable, Action<T>? before = null, Action<T>? after = null)
        {
            foreach (var item in enumerable)
            {
                before?.Invoke(item);
                yield return item;
                after?.Invoke(item);
            }
        }
        /// <summary>
        /// 遍历传入集合的遍历器, 但是什么都不做.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        public static void Finish<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                _ = item;
            }
        }


        /// <summary>
        /// 遍历两个可枚举的对象, 直到都结束. 其中一方结束后, 如果另一方未结束, 会取得 <see langword="null"/> 值
        /// </summary>
        /// <remarks>如果传入了不可为空的类型, 取得的值将会是 <see langword="default"/></remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<(T1?, T2?)> UntilAllAway<T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) obj)
        {
            IEnumerator<T1> e1 = obj.Item1.GetEnumerator();
            IEnumerator<T2> e2 = obj.Item2.GetEnumerator();
            bool e1End = false;
            bool e2End = false;
            do
            {
                T1? t1 = default;
                T2? t2 = default;
                if (!e1End && e1.MoveNext())
                {
                    t1 = e1.Current;
                }
                else { e1End = true; }
                if (!e2End && e2.MoveNext())
                {
                    t2 = e2.Current;
                }
                else { e2End = true; }

                if (e1End && e2End) yield break;
                else yield return (t1, t2);

            } while (!e1End || !e2End);
        }

        /// <summary>
        /// 遍历两个可枚举的对象, 同时附带从 0 起的索引值, 直到都结束. 其中一方结束后, 如果另一方未结束, 会取得 -1 与 <see langword="null"/> 值
        /// </summary>
        /// <remarks>如果传入了不可为空的类型, 取得的值将会是 <see langword="default"/></remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<((int Index, T1? Value), (int Index, T2? Value))> UntilAllAwayWithIndex<T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) obj)
        {
            IEnumerator<T1> e1 = obj.Item1.GetEnumerator();
            IEnumerator<T2> e2 = obj.Item2.GetEnumerator();
            bool e1End = false;
            int index1 = 0;
            bool e2End = false;
            int index2 = 0;
            do
            {
                T1? t1 = default;
                T2? t2 = default;
                if (!e1End && e1.MoveNext())
                {
                    t1 = e1.Current;
                    index1++;
                }
                else
                {
                    e1End = true;
                    index1 = -1;
                }
                if (!e2End && e2.MoveNext())
                {
                    t2 = e2.Current;
                    index2++;
                }
                else 
                {
                    e2End = true;
                    index2 = -1;
                }

                if (e1End && e2End) yield break;
                else yield return ((index1, t1), (index2, t2));

            } while (!e1End || !e2End);
        }

        /// <summary>
        /// 遍历两个可枚举的对象, 直到任意一方结束. 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2)> UntilAnyAway<T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) obj)
        {
            IEnumerator<T1> e1 = obj.Item1.GetEnumerator();
            IEnumerator<T2> e2 = obj.Item2.GetEnumerator();
            while (true)
            {
                if (e1.MoveNext() && e2.MoveNext())
                {
                    yield return (e1.Current, e2.Current);
                }
                else yield break;
            }
        }


        #endregion

        #endregion



        #region T : IDisposable
        /// <summary>
        /// 调用集合内的所有项的<see cref="IDisposable.Dispose"/>方法, 释放所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void DisposeAll<T>(this IEnumerable<T> array)
            where T : IDisposable
        {
            foreach (T t in array) t?.Dispose();
        }
        #endregion


        #region T : FileInfo
        /// <summary>
        /// 过滤以获取文件扩展名与输入的任意一项匹配的 <see cref="FileInfo"/>
        /// </summary>
        /// <remarks>
        /// 比较时将忽略大小写
        /// </remarks>
        /// <param name="files"></param>
        /// <param name="matchItems">匹配项的集合, 需要输入想要的扩展名 (不需要在前方加 '.', 如果有, 将会被移除), 例如: "txt"</param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> MatchSuffix(this IEnumerable<FileInfo> files, params string[] matchItems)
        {
            var _mItems = matchItems.Select(i => '.' + i.TrimStart('.')).Distinct().ToArray();
            if (_mItems.Length == 0) yield break;

            foreach (FileInfo file in files)
            {
                if (_mItems.Any(i => i.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase)))
                {
                    yield return file;
                }
            }
        } 

        #endregion

    }
}
