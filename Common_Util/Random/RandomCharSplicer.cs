using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common_Util.Random
{
    /// <summary>
    /// 随机字符拼接器
    /// </summary>
    public class RandomCharSplicer
    {
        /// <summary>
        /// 从指定的字符集合及随机种子构建
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="seed">种子</param>
        public RandomCharSplicer(IEnumerable<char> chars, int seed) : this(chars, new System.Random(seed)) { }
        /// <summary>
        /// 从指定的字符集合及随机对象构建
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="random"></param>
        public RandomCharSplicer(IEnumerable<char> chars, System.Random? random = null)
        {
            CharList = chars == null ? new List<char>() : chars.ToList();
            Random = random ?? new System.Random();
        }

        #region 参数
        public System.Random Random { get; private set; }
        public List<char> CharList { get; private set; }
        #endregion

        #region 构建字符串
        public string Get(int length)
        {
            if (length <= 0 || CharList.Count == 0) { return ""; }
            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                builder.Append(GetRandomItem(CharList));
            }

            return builder.ToString();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 在列表中随机取一项, 不检查是否为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public T GetRandomItem<T>(List<T> source)
        {
            return source[Random.Next(0, source.Count)];
        }
        #endregion
    }
}
