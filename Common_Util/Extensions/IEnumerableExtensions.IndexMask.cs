using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static partial class IEnumerableExtensions
    {
        /// <summary>
        /// 按索引遮罩 <paramref name="mask"/>, 从其低位开始与 <paramref name="values"/> 同时遍历, 按特定规则抛弃一些值
        /// </summary>
        /// <remarks>
        /// 超出 <paramref name="values"/> 的范围时会直接返回; 此方法不修改 <paramref name="values"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="mask"></param>
        /// <param name="filtering">什么遮罩值表示需要被抛弃</param>
        /// <param name="overMask">当遮罩比 <paramref name="values"/> 短时, 需要将超出部分视为什么值</param>
        /// <returns></returns>
        /// <returns>过滤值之后的可枚举对象</returns>
        public static IEnumerable<T> Filtering<T>(this IEnumerable<T> values, IndexMask mask, bool filtering = true, bool overMask = true)
        {
            bool mValue;
            foreach (var (v, m) in (values, mask.All(false).Select(b => (bool?)b)).UntilAllAway())
            {
                if (v == null) yield break;
                if (m == null)
                {
                    if (filtering == overMask) yield break;
                    else mValue = overMask;
                }
                else
                {
                    mValue = m.Value;
                }

                if (mValue != filtering)
                {
                    yield return v;
                }
                
            }
        }

        /// <summary>
        /// 按索引遮罩 <paramref name="mask"/>, 从其低位开始与 <paramref name="values"/> 同时遍历, 按特定规则过滤一些值, 使用 <paramref name="replaceFunc"/> 将未被过滤的值替换为另一个值
        /// </summary>
        /// <remarks>
        /// 超出 <paramref name="values"/> 的范围时会直接返回; 此方法不修改 <paramref name="values"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="mask"></param>
        /// <param name="replaceFunc">传入原有值, 返回新值</param>
        /// <param name="filtering"></param>
        /// <param name="overMask"></param>
        /// <returns>替换值之后的可枚举对象</returns>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> values, IndexMask mask, Func<T, T> replaceFunc, bool filtering = true, bool overMask = true)
        {
            bool mValue;
            foreach (var (v, m) in (values, mask.All(false).Select(b => (bool?)b)).UntilAllAway())
            {
                if (v == null) yield break;
                if (m == null)
                {
                    mValue = overMask;
                }
                else
                {
                    mValue = m.Value;
                }

                if (mValue != filtering)
                {
                    yield return replaceFunc.Invoke(v);
                }
                else
                {
                    yield return v;
                }

            }
        }

        /// <summary>
        /// 按索引遮罩 <paramref name="mask"/>, 从其低位开始与 <paramref name="values"/> 同时遍历, 按特定规则过滤一些值, 将未被过滤的值替换为 <paramref name="replaceValue"/>
        /// </summary>
        /// <remarks>
        /// 超出 <paramref name="values"/> 的范围时会直接返回; 此方法不修改 <paramref name="values"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="mask"></param>
        /// <param name="replaceValue">替换后的新值</param>
        /// <param name="filtering"></param>
        /// <param name="overMask"></param>
        /// <returns>替换值之后的可枚举对象</returns>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> values, IndexMask mask, T replaceValue, bool filtering = true, bool overMask = true)
        {
            bool mValue;
            foreach (var (v, m) in (values, mask.All(false).Select(b => (bool?)b)).UntilAllAway())
            {
                if (v == null) yield break;
                if (m == null)
                {
                    mValue = overMask;
                }
                else
                {
                    mValue = m.Value;
                }

                if (mValue != filtering)
                {
                    yield return replaceValue;
                }
                else
                {
                    yield return v;
                }

            }
        }
    }
}
