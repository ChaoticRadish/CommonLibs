using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module
{
    /// <summary>
    /// 默认的简略信息获取器的实现
    /// <para>本实现将根据输入的类型按以下情况生成简略信息字符串</para>
    /// <para>· <see cref="IBriefInfo"/>: 直接返回 <see cref="IBriefInfo.Brief"/> </para>
    /// <para>· 非结构体的值类型或枚举: 将使用<see cref="object.ToString"/>直接转换为字符串</para>
    /// <para>· 字符串: 输出使用双引号包裹的字符串, 长度限制到 <see cref="STRING_LIMIT"/> </para>
    /// <para>· 数组/列表/字典: 以 "{元素名}[{集合大小}]" 的形式生成字符串</para>
    /// <para>· 其他: 以 "{类型名}<{对象HashCode}>" 的形式生成字符串</para>
    /// </summary>
    internal class BriefInfoGetterImplDefault : IBriefInfoGetter
    {
        private Dictionary<Type, TypePropertyInfo> _infoDic = new Dictionary<Type, TypePropertyInfo>();

        public const int STRING_LIMIT = 25;
        private const string NULL_STRING = "<null>";

        public string GetBriefInfo(object obj)
        {
            return _getBriefInfo(obj);
        }

        private readonly object locker_infoType = new object();
        private string _getBriefInfo(object? obj)
        {
            if (obj == null)
            {
                return NULL_STRING;
            }
            Type type = obj.GetType();
            if (!_infoDic.ContainsKey(type))
            {
                lock (locker_infoType)
                {
                    if (!_infoDic.ContainsKey(type))
                    {
                        _infoDic.Add(type, new TypePropertyInfo(type));
                    }
                }
            }
            TypePropertyInfo info = _infoDic[type];

            switch (info.DealType)
            {
                case DealTypeEnum.Other:
                case DealTypeEnum.ToString:
                    return obj.ToString() ?? string.Empty;
                case DealTypeEnum.IsString:
                    return $"\"{((string)obj).Brief(STRING_LIMIT)}\"";
                case DealTypeEnum.List:
                case DealTypeEnum.Array:
                    {
                        IList list = (IList)obj;
                        int count = list.Count;
                        return $"{type.Name}[{count}]";
                    }
                case DealTypeEnum.Dictionary:
                    {
                        IDictionary dic = (IDictionary)obj;
                        int count = dic.Count;
                        return $"{type.Name}[{count}]";
                    }
                case DealTypeEnum.IBriefInfo:
                    return ((IBriefInfo)obj).Brief;

                default:
                    throw new NotSupportedException();
            }

        }
        class TypePropertyInfo
        {
            public TypePropertyInfo(Type type)
            {
                Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (type.IsAssignableTo(typeof(IBriefInfo)))
                {
                    DealType = DealTypeEnum.IBriefInfo;
                }
                if (type.IsPrimitive || type.IsEnum)
                {
                    DealType = DealTypeEnum.ToString;
                }
                else if (type == typeof(string))
                {
                    DealType = DealTypeEnum.IsString;
                }
                else if (type.IsAssignableTo(typeof(IDictionary)))
                {
                    DealType = DealTypeEnum.Dictionary;
                }
                else if (type.IsAssignableTo(typeof(MemberInfo)) || type.IsAssignableTo(typeof(ParameterInfo)))
                {
                    DealType = DealTypeEnum.ToString;
                }
                else if (type.IsArray)
                {
                    DealType = DealTypeEnum.Array;
                }
                else if (type.IsAssignableTo(typeof(IList)))
                {
                    DealType = DealTypeEnum.List;
                }
                else
                {
                    DealType = DealTypeEnum.Other;
                }


            }
            public readonly PropertyInfo[] Properties;


            public readonly DealTypeEnum DealType;

        }
        enum DealTypeEnum
        {
            ToString,
            IsString,
            List,
            Array,
            Dictionary,
            IBriefInfo,
            Other,
        }
    }
}
