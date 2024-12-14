using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class IEnumerableExtensions
    {

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
        /// 遍历传入集合的遍历器, 对其中的每一项都执行指定的操作. 
        /// </summary>
        /// <remarks>
        /// 此方法用于链式调用时, 中途需要执行集合项某个方法的场景
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> Invoke<T>(this IEnumerable<T> enumerable, Action<T> action) 
        { 
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }
        #endregion

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

        /// <summary>
        /// 比较两个集合内的元素能否一一对应. 此比较与顺序无关, 类似于连连看.
        /// </summary>
        /// <remarks>
        /// 当前实现会将集合转换为数组来实现, 数据量较大时可能会有性能问题
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


            Dictionary<int, List<T>> group1 = [];
            int arr1NullCount = 0;

            Dictionary<int, List<T>> group2 = [];
            int arr2NullCount = 0;

            static void tidyHashCode(T[] arr, Dictionary<int, List<T>> groupContainer, ref int count, IEqualityComparer<T> equalityComparer)
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

            List<int> ignoreIndexMark = []; // 比较两组数据中相同 HashCode 的项的过程中, 标记后者中已有对应关系的项
            foreach (int hashCode in group1.Keys)
            {
                var list1 = group1[hashCode];
                if (group2.TryGetValue(hashCode, out var list2))
                {
                    if (list1.Count != list2.Count)
                    {
                        // 同一个 HashCode, 两者拥有数量不同
                        return false;
                    }
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
                else
                {
                    // 存在任意 HashCode, 只有前者拥有, 后者没有
                    return false;
                }
            }

            return true;
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
