﻿using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        private string group;
        private string id;

        public string Group { readonly get => group ?? string.Empty; set => group = value; }// = string.Empty;
        public string Id { readonly get => id ?? string.Empty; set => id = value; }// = string.Empty;
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

        /// <summary>
        /// 将此数据转换为字符串, 不会对其中的特殊字符作转义处理
        /// </summary>
        /// <remarks>
        /// 通过此方法取得的字符串可能无法转换取得与当前对象等价的对象, 如果需要取得作转义处理得到的字符串, 需调用 <see cref="ConvertToString"/>
        /// </remarks>
        /// <returns></returns>
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

        #region 相等比较
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is GroupIdPair other)
            {
                return this.Group == other.Group && this.Id == other.Id;
            }
            else if (obj is string str)
            {
                return str.Equals(this.ConvertToString());
            }
            return base.Equals(obj);
        }
        public readonly override int GetHashCode()
        {
            return (((Group?.GetHashCode() ?? 0) & 0b1111_1111_1111_1111) << 16) | ((Id?.GetHashCode() ?? 0) & 0b1111_1111_1111_1111);
        }

        public static bool operator ==(GroupIdPair left, GroupIdPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GroupIdPair left, GroupIdPair right)
        {
            return !(left == right);
        }
        #endregion
    }
}
