using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// 添加另一个字典到字典中
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="dic2"></param>
        /// <param name="overwrite">是否覆写同键的值</param>
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dic2, bool overwrite = false)
        {
            if (dic == null) throw new NullReferenceException("需要被写入数据的字典为null");
            if (dic2 == null || dic2.Count == 0) return;
            foreach (var kv in dic2)
            {
                if (dic.ContainsKey(kv.Key))
                {
                    if (overwrite)
                    {
                        dic[kv.Key] = kv.Value;
                    }
                }
                else
                {
                    dic.Add(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// 释放所有项
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        public static void DisposeAll<TKey, TValue>(this IDictionary<TKey, TValue> dic)
            where TValue : IDisposable
        {
            foreach (TValue obj in dic.Values)
            {
                obj?.Dispose();
            }
        }
        /// <summary>
        /// 释放所有项并清空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        public static void DisposeAllAndClear<TKey, TValue>(this IDictionary<TKey, TValue> dic)
            where TValue : IDisposable
        {
            foreach (TValue obj in dic.Values)
            {
                obj?.Dispose();
            }
            dic?.Clear();
        }
        /// <summary>
        /// 遍历所有项, 并使用遍历的元素执行输入方法
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        public static void ForEach<TKey, TValue>(
            this IDictionary<TKey, TValue> dic,
            Action<TKey, TValue> action)
        {
            foreach (TKey key in dic.Keys)
            {
                action.Invoke(key, dic[key]);
            }
        }

        /// <summary>
        /// 尝试添加字典项, 如果已有, 则覆盖
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        /// <summary>
        /// 尝试从字典中获取值, 如果字典不含输入键, 则返回默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue TryGetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue)
        {
            return dic.TryGetValue(key, out TValue? value) ? value : defaultValue;
        }
        /// <summary>
        /// 尝试从字典中获取值, 如果字典不含输入键, 则返回null
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue? TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key) 
        {
            dic.TryGetValue(key, out TValue? value);
            return value;
        }
    }
}
