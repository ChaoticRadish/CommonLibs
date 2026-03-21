using Common_Util.Attributes.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Reflection
{
    /// <summary>
    /// 类型成员的序号相关帮助类
    /// </summary>
    public static class MemberSequenceHelper
    {
        /// <summary>
        /// 获取标记了 <see cref="SequenceFlagAttribute"/> 的属性，并按顺序增序排序。
        /// </summary>
        /// <remarks>
        /// 不带标记的会忽略
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<SequenceMemberInfo<PropertyInfo>> GetOrderedProperties(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var properties = type.GetProperties(bindingFlags);
            return _filterAndGetSequence(properties).Order(DefaultComparer<PropertyInfo>());
        }

        /// <summary>
        /// 获取标记了 <see cref="SequenceFlagAttribute"/> 的字段，并按顺序增序排序。
        /// </summary>
        /// <remarks>
        /// 不带标记的会忽略
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<SequenceMemberInfo<FieldInfo>> GetOrderedFields(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var fields = type.GetFields(bindingFlags);
            return _filterAndGetSequence(fields).Order(DefaultComparer<FieldInfo>());
        }

        private static IEnumerable<SequenceMemberInfo<TMember>> _filterAndGetSequence<TMember>(IEnumerable<TMember> members)
             where TMember : MemberInfo
        {
            if (members == null) yield break;

            foreach (var member in members)
            {
                if (member.ExistCustomAttribute<SequenceFlagAttribute>(out var attribute))
                {
                    yield return new()
                    {
                        Member = member,
                        Sequence = attribute.Sequence,
                    };
                }
            }
        }


        public readonly struct SequenceMemberInfo<TMember> where TMember : MemberInfo
        {
            /// <summary>
            /// 负数值表示按名称排序 (正序的情况下会固定排在有序号的值之后)
            /// </summary>
            public int? Sequence { get; init; }

            public string MemberName => Member.Name;

            public TMember Member { get; init; }

        }

        public static IComparer<SequenceMemberInfo<TMember>> DefaultComparer<TMember>() where TMember : MemberInfo
        {
            return SequenceMemberInfoComparer<TMember>.Shared;
        }
        private readonly struct SequenceMemberInfoComparer<TMember> : IComparer<SequenceMemberInfo<TMember>>
            where TMember : MemberInfo
        {
            public static SequenceMemberInfoComparer<TMember> Shared { get; } = new();
            public int Compare(SequenceMemberInfo<TMember> x, SequenceMemberInfo<TMember> y)
            {
                if (x.Sequence == null && y.Sequence == null)
                    return Comparer<string>.Default.Compare(x.MemberName, y.MemberName);
                else if (x.Sequence != null && y.Sequence != null)
                {
                    var result = Comparer<int>.Default.Compare(x.Sequence.Value, y.Sequence.Value);
                    if (result != 0) return result;
                    return Comparer<string>.Default.Compare(x.MemberName, y.MemberName);
                }
                else if (x.Sequence != null && y.Sequence == null)
                    return -1;
                else // if (x.Sequence == null && y.Sequence != null)
                    return 1;
            }
        }
    }


}
