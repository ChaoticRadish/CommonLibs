using Common_Util.Data.Structure.Map;
using Common_Util.Extensions;
using Common_Util.Interfaces.Owner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Extensions
{
    public static class ITagsOwnerExtensions
    {

        #region 搜索方法

        /// <summary>
        /// 查询所有没有标签的项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="ignoreNull">判断是否没标签时, 是否忽略掉 null 标签 (所有标签都是 null 标签时, 将被判断为没有标签)</param>
        /// <returns></returns>
        public static IEnumerable<TItem> QueryAllNoTag<TItem>(this IEnumerable<TItem> items, bool ignoreNull = true)
            where TItem : ITagsOwner
        {
            foreach (TItem owner in items)
            {
                if (!owner.Tags.Any())
                {
                    yield return owner;
                }
                else
                {
                    if (ignoreNull && owner.Tags.All(i => i == null))
                    {
                        // 忽略 null 标签的项时, 如果都是 null 标签, 则将其返回
                        yield return owner;
                    }
                }
            }
        }

        /// <summary>
        /// 查询所拥有的标签存在任意传入标签的项 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="tags">传入用于判定的标签</param>
        /// <returns></returns>
        public static IEnumerable<TItem> QueryExistTag<TItem>(this IEnumerable<TItem> items, params string[] tags)
            where TItem : ITagsOwner
        {
            if (tags == null || tags.Length == 0) 
                yield break;
            foreach (TItem owner in items)
            {
                if (owner.Tags.Any(i => tags.Contains(i)))
                {
                    yield return owner;
                }
            }
        }

        /// <summary>
        /// 查询所拥有的标签包含所有传入标签的项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="tags">传入用于判断的标签</param>
        /// <returns></returns>
        public static IEnumerable<TItem> QueryMatchingTag<TItem>(this IEnumerable<TItem> items, params string[] tags)
            where TItem : ITagsOwner
        {
            if (tags == null || tags.Length == 0)
            {
                foreach (TItem owner in items)
                {
                    yield return owner;
                }
            }
            else
            {
                foreach (TItem owner in items)
                {
                    if (tags.All(i => owner.Tags.Contains(i)))
                    {
                        yield return owner;
                    }
                }
            }

        }

        /// <summary>
        /// 查询所拥有的标签符合传入条件的项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> QueryWhere<TItem>(this IEnumerable<TItem> items, Func<IEnumerable<string?>, bool> where)  
            where TItem : ITagsOwner
        {
            if (where == null) yield break;
            foreach (TItem item in items)
            {
                if (where(item.Tags))
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region 转换
        /// <summary>
        /// 将可枚举对象转换为 <see cref="TagsMap{Key, Value}"/>
        /// <para>因 <see cref="TagsMap{Key, Value}"/> 不支持 null 标签, 故转换时 null 标签将被筛选掉</para>
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="getKeyFunc"></param>
        /// <returns></returns>
        public static TagsMap<Tkey, TItem> AsTagsMap<Tkey, TItem>(this IEnumerable<TItem> items, Func<TItem, Tkey> getKeyFunc)
            where Tkey : notnull
            where TItem: ITagsOwner
        {
            TagsMap<Tkey, TItem> output = new();
            foreach (TItem item in items)
            {
                output.Add(getKeyFunc(item), item, item.Tags.FilterNull().Select(i => i!).ToArray());
            }
            return output;
        }
        #endregion
    }
}
