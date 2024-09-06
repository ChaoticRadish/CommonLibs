using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 由组号与ID两个值构成的数据
    /// </summary>
    public struct GroupIdPair : IStringConveying
    {
        #region 数据
        public string Group { get; set; }// = string.Empty;
        public string Id { get; set; }// = string.Empty;
        #endregion

        #region 常量
        /// <summary>
        /// 分割字符
        /// </summary>
        public const char SPLIT_CHAR = ':';
        /// <summary>
        /// 转义字符
        /// </summary>
        public const char ESCAPE_CHAR = '$';
        #endregion

        public void ChangeValue(string value)
        {
            StringBuilder groupBuilder = new StringBuilder();
            StringBuilder idBuilder = new StringBuilder();
            StringBuilder current = groupBuilder;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == ESCAPE_CHAR)
                {
                    if (i + 1 == value.Length)
                    {
                        throw new ArgumentException($"无效的转义: 转义字符后无其他字符");
                    }
                    else
                    {
                        char next = value[i + 1];
                        switch (next)
                        {
                            case SPLIT_CHAR:
                            case ESCAPE_CHAR:
                                current.Append(next);   // 写入转义字符的下一位
                                i++;    // 转义字符不写入, 跳过下一位
                                break;
                            default:
                                throw new ArgumentException($"无效的转义: '{next}'", nameof(value));
                        }
                    }
                }
                else if (c == SPLIT_CHAR)
                {
                    current = idBuilder;
                }
                else
                {
                    current.Append(c);
                }

            }
            Group = groupBuilder.ToString();
            Id = idBuilder.ToString();
        }

        public readonly string ConvertToString()
        {
            return $"{_转义(Group)}{SPLIT_CHAR}{_转义(Id)}";
        }

        public override readonly string ToString()
        {
            return $"{Group}{SPLIT_CHAR}{Id}";
        }

        #region 转义
        private static readonly char[] _需转义字符 = [ESCAPE_CHAR, SPLIT_CHAR]; 
        private static string _转义(string input)
        {
            if (input.IsEmpty()) return input;
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (_需转义字符.Contains(c))
                {
                    sb.Append(ESCAPE_CHAR);   
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
        #endregion

        #region 隐式转换
        public static implicit operator GroupIdPair((string? group, string? id) arg)
        {
            return new GroupIdPair()
            {
                Group = arg.group ?? string.Empty,
                Id = arg.id ?? string.Empty
            };
        }
        public static implicit operator GroupIdPair(KeyValuePair<string, string> pair)
        {
            return new GroupIdPair()
            {
                Group = pair.Key,
                Id = pair.Value
            };
        }
        #endregion
    }
}
