using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 以枚举类型为键的Map
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Data"></typeparam>
    public class EnumMap<T, Data> : Dictionary<T, Data>
        where T : Enum
    {
        /// <summary>
        /// 实例化, 并添加枚举的所有值到Map中, 对应的泛型 <see cref="Data"/> 使用 default
        /// </summary>
        public EnumMap() : base(Enum.GetNames(typeof(T)).Length)
        {
            var es = Enum.GetValues(typeof(T));
            foreach (var e in es)
            {
                Add((T)e, default);
            }
        }

        public EnumMap(Func<T, Data> getDefaultValue) : base(Enum.GetNames(typeof(T)).Length)
        {
            var es = Enum.GetValues(typeof(T));
            foreach (var e in es)
            {
                Add((T)e, getDefaultValue.Invoke((T)e));
            }
        }
    }
    public static class EnumMapExtensions
    {
        /// <summary>
        /// Map的值类型是IEnumerable时, 取得所有列表值所包含的所有项的总数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="map"></param>
        /// <returns></returns>
        public static int Total<T, TData>(this EnumMap<T, IEnumerable<TData>> map)
            where T : Enum
        {
            return map.Values.Sum(l => l == null ? 0 : l.Count());
        }
        public static int Total<T, TData>(this EnumMap<T, List<TData>> map)
            where T : Enum
        {
            return map.Values.Sum(l => l == null ? 0 : l.Count);
        }
    }
}
