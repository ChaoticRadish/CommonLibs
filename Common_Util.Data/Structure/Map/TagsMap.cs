using Common_Util.Interfaces.Owner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 可以使用标签检索的Map
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class TagsMap<Key, Value> : Dictionary<Key, TagsMapItem<Key, Value>>
        where Key : notnull
    {


        #region 控制
        /// <summary>
        /// 将键值对以指定标签保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tags"></param>
        public void Add(Key key, Value value, params string[] tags)
        {
            Add(key, new TagsMapItem<Key, Value>()
            {
                Key = key,
                Value = value,
                Tags = tags ?? (Array.Empty<string>())
            });
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="tags">标签</param>
        public void UpdateTags(Key key, params string[] tags)
        {
            if (ContainsKey(key))
            {
                this[key].Tags = tags ?? (Array.Empty<string>());
            }
        }

        /// <summary>
        /// 清空Map
        /// </summary>
        public new void Clear()
        {
            base.Clear();
        }
        #endregion

        #region 索引器 (将调用搜索方法)
        public List<Value?> this[params string[] tags]
        {
            get
            {
                if (tags == null || tags.Length == 0)
                {
                    return GetAll();
                }
                else
                {
                    return GetExistTag(tags);
                }
            }
        }
        #endregion

        #region 搜索
        /// <summary>
        /// 取得所有值
        /// </summary>
        /// <returns></returns>
        public List<Value?> GetAll()
        {
            List<Value?> output = new();
            foreach(TagsMapItem<Key, Value> item in Values)
            {
                output.Add(item.Value);
            }
            return output;
        }
        /// <summary>
        /// 取得所有没有标签的值
        /// </summary>
        /// <returns></returns>
        public List<Value?> GetAllNoTag()
        {
            List<Value?> output = new List<Value?>();
            foreach (TagsMapItem<Key, Value> item in Values)
            {
                if (item.Tags.Length == 0)
                {
                    output.Add(item.Value);
                }
            }
            return output;
        }
        /// <summary>
        /// 取得存在传入标签的值 (只要有交集即可)
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public List<Value?> GetExistTag(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return GetAll();
            }
            List<Value?> output = new List<Value?>();
            foreach (TagsMapItem<Key, Value> item in Values)
            {
                bool existSame = false;
                foreach (string itag in tags)
                {
                    foreach (string etag in item.Tags)
                    {
                        if(itag == etag)
                        {
                            existSame = true;
                            break;
                        }
                    }
                    if (existSame) break;
                }
                if (existSame)
                {
                    output.Add(item.Value);
                }
            }
            return output;
        }
        /// <summary>
        /// 取得匹配传入标签的值 (需要包含传入所有标签)
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public List<Value?> GetMatchingTag(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return GetAll();
            }
            List<Value?> output = new List<Value?>();
            foreach (TagsMapItem<Key, Value> item in Values)
            {
                bool existSame = true;
                foreach (string itag in tags)
                {
                    bool existSame_t = false;
                    foreach (string etag in item.Tags)
                    {
                        if (itag == etag)
                        {
                            existSame_t = true;
                            break;
                        }
                    }
                    if (!existSame_t)
                    {// 输入的一个标签 在已有标签列表中 没有匹配项
                        existSame = false;
                        break;
                    }
                }
                if (existSame)
                {
                    output.Add(item.Value);
                }
            }
            return output;
        }

        #endregion

    }

    /// <summary>
    /// 可以使用标签检索的Map的值项
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class TagsMapItem<TKey, TValue> : ITagsOwner
        where TKey : notnull
    {
        public TagsMapItem()
        {
            Key = default!;
            Value = default;
            Tags = Array.Empty<string>();
        }

        /// <summary>
        /// 键
        /// </summary>
        public TKey Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public TValue? Value { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string[] Tags { get; set; }

        IEnumerable<string?> ITagsOwner.Tags => Tags;
    }
}
