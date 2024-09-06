using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class IListExtensions
    {
        #region 随机
        /// <summary>
        /// 获取列表内的随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Random<T>(this IList<T> list, System.Random? random = null)
        {
            ArgumentNullException.ThrowIfNull(list);
            if (list.Count == 0) throw new ArgumentException("列表中不包含任何元素", nameof(list));
            random ??= new System.Random();
            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// 尝试从列表中随机取其中一项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item">被随机获取到的项</param>
        /// <param name="exclude">随机取值时, 需要排除的值</param>
        /// <param name="comparer">排除值时, 比较两值是否等价的比较器</param>
        /// <param name="random">随机数生成器, 用于生成随机的索引</param>
        /// <returns>获取成功时, 将返回 true</returns>
        public static bool TryGetRandom<T>(this IList<T> list, out T item, IEnumerable<T>? exclude = null, IEqualityComparer<T>? comparer = null, System.Random? random = null)
        {
            IList<T> range = exclude == null ? list : list.Except(exclude, comparer).ToArray();
            if (range.Count == 0)
            {
                item = default!;    // 这里标记了 '!', 但是实际可能是 null 的, 如 T is string? 时
                return false;
            }
            else
            {
                item = range.Random(random);
                return true;
            }
        }

        /// <summary>
        /// 随机移除列表中的某一项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="random"></param>
        /// <returns>被移除的项. </returns>
        public static T RandomRemove<T>(this IList<T> list, System.Random? random = null)
        {
            if (list.Count == 0) throw new ArgumentException("列表为空! ", nameof(list));
            random ??= new System.Random();
            int index = random.Next(list.Count);
            T output = list[index];
            list.RemoveAt(index);
            return output;
        }

        /// <summary>
        /// 随机替换列表中的某一项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="replace">取得替换数据的方法, 输入值: 随机取得的数据, 输出值: 需要被替换的数据</param>
        /// <param name="random"></param>
        public static void RandomReplace<T>(this IList<T> list, Func<T, T> replace, System.Random? random = null)
        {
            ArgumentNullException.ThrowIfNull(list);
            if (list.Count == 0) throw new ArgumentException("列表中不包含任何元素", nameof(list));
            random ??= new System.Random();
            int index = random.Next(list.Count);
            T item = list[index];
            T newObj = replace.Invoke(item);
            list.RemoveAt(index);
            list.Insert(index, newObj);
        }

        /// <summary>
        /// 随机对列表中的某一项做输入的操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">需要执行的操作, 输入值: 随机取得的数据</param>
        /// <param name="random"></param>
        public static void RandomDo<T>(this IList<T> list, Action<T> action, System.Random? random = null)
        {
            ArgumentNullException.ThrowIfNull(list);
            if (list.Count == 0) throw new ArgumentException("列表中不包含任何元素", nameof(list));
            random ??= new System.Random();
            int index = random.Next(list.Count);
            T item = list[index];
            action.Invoke(item);
        }

        #endregion

        /// <summary>
        /// 将新项添加到列表尾部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IList<T> Append<T>(this IList<T> list, T item)
        {
            list.Add(item); return list;
        }

        /// <summary>
        /// 创建一定数量的新项, 追加到列表尾部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <param name="getObjFunc">创建新项的方法, 传入计数索引</param>
        /// <returns></returns>
        public static IList<T> Append<T>(this IList<T> list, int count, Func<int, T> getObjFunc)
        {
            for (int i = 0; i < count; i++)
            {
                list.Append(getObjFunc(i));
            }
            return list;
        }

        /// <summary>
        /// 将新项添加到列表顶部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IList<T> InsertHead<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
            return list;
        }

        /// <summary>
        /// 添加默认元素到列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        public static void AddDefault<T>(this IList<T> list, int count)
        {
            if (count <= 0 || list == null) return;

            for (int i = 0; i < count; i++)
            {
                list.Add(default!);
            }
        }

        /// <summary>
        /// 创建输入类型的一个列表, 并把输入对象放进去
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static IList? CreateList(Type listType, params object[] objs)
        {
            IList? list = null;
            if (listType.IsArray)
            {
                Type? elementType = listType.GetElementType();
                if (elementType != null)
                {
                    Array array = Array.CreateInstance(elementType, objs.Length);
                    for (int i = 0; i < objs.Length; i++)
                    {
                        array.SetValue(objs[i], i);
                    }
                    list = array;
                }
            }
            else
            {
                list = (IList?)Activator.CreateInstance(listType);
                if (list != null)
                {
                    Type[] genericArguments = listType.GetGenericArguments();
                    if (genericArguments.Length > 0)
                    {
                        for (int i = 0; i < objs.Length; i++)
                        {
                            list.Add(objs[i]);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 释放所有项并清空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        public static void DisposeAllAndClear<T>(this IList<T> objs)
            where T : IDisposable
        {
            foreach (T obj in objs)
            {
                obj.Dispose();
            }
            objs.Clear();
        }

        /// <summary>
        /// 检查列表内的元素是否顺序排列, 忽略相邻值相等的情况
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="compareFunc">比较两个值的方法, 如果第一个参数大于第二个参数, 需要返回true</param>
        /// <param name="asc">是否升序, 即较小的值放在前面</param>
        public static bool CheckSort<T>(this IList<T?> list, Func<T?, T?, bool> compareFunc, bool asc = true)
        {
            if (asc)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    if (compareFunc.Invoke(list[i], list[i + 1]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    if (compareFunc.Invoke(list[i], list[i - 1]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 检查列表内的元素是否顺序排列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="compareFunc">比较两个值大小的方法</param>
        /// <param name="asc">是否升序, 即较小的值放在前面</param>
        /// <param name="ignoreEquel">是否忽略相等情况, 如果为false, 则需要整个列表各个相邻项均不相等</param>
        public static bool CheckSort<T>(this IList<T?> list, Func<T?, T?, Common_Util.Enums.CompareResultEnum> compareFunc, bool asc = true, bool ignoreEquel = true)
        {
            Enums.CompareResultEnum result;
            for (int i = 0; i < list.Count - 1; i++)
            {
                result = compareFunc.Invoke(list[i], list[i + 1]);
                if (ignoreEquel && result == Enums.CompareResultEnum.Equel)
                {
                    return false;
                }
                else if (asc ? result == Enums.CompareResultEnum.Bigger : result == Enums.CompareResultEnum.Smaller)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 对列表内的元素做冒泡排序, 此方法会改变列表内各元素的位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="compareFunc">比较两个值的方法, 如果第一个参数大于第二个参数, 需要返回true</param>
        /// <param name="asc">是否升序, 即较小的值放在前面</param>
        public static void BubbleSort<T>(this IList<T> list, Func<T, T, bool> compareFunc, bool asc = true)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int a = 0; a < list.Count - 1 - i; a++)
                {
                    if (asc == compareFunc(list[a], list[a + 1]))
                    {
                        (list[a], list[a + 1]) = (list[a + 1], list[a]);
                    }
                }
            }
        }

    }
}
