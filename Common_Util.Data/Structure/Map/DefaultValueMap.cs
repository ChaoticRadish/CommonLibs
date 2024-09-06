using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 不存在指定键时返回默认值的Map
    /// </summary>
    public class DefaultValueMap<Key, Value> : Dictionary<Key, Value>
        where Key : notnull
    {
        public DefaultValueMap(Value defaultValue) 
        {
            DefaultValue = defaultValue;
        }

        #region 数据
        /// <summary>
        /// 不存在指定键时的返回值
        /// </summary>
        public Value DefaultValue { get; private set; }

        #endregion

        #region 索引器
        /// <summary>
        /// 取得或设置一个指定的键的元素
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new Value this[Key key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                else
                {
                    return DefaultValue;
                }
            }
            set
            {
                if (ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
        #endregion
    }
}
