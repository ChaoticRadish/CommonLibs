using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 文本键值对
    /// </summary>
    public struct TextPair
    {
        public string Key { get; set; }

        public string Value { get; set; }

        #region 隐式转换
        public static implicit operator TextPair((string, string) pair)
        {
            return new()
            {
                Key = pair.Item1,
                Value = pair.Item2,
            };
        }
        public static implicit operator (string, string)(TextPair pair)
        {
            return (pair.Key, pair.Value);
        }

        public static implicit operator TextPair(KeyValuePair<string, string> pair)
        {
            return new()
            {
                Key = pair.Key,
                Value = pair.Value,
            };
        }
        public static implicit operator KeyValuePair<string, string>(TextPair pair)
        {
            return new(pair.Key, pair.Value);
        }

        #endregion
    }
}
