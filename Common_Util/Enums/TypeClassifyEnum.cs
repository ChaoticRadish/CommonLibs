
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    /// <summary>
    /// 类型的分类
    /// </summary>
    [Flags]
    public enum TypeClassifyEnum : int
    {
        /// <summary>
        /// 枚举类型
        /// </summary>
        Enum = 0x2,
        /// <summary>
        /// 抽象类型
        /// </summary>
        Abstract = 0x4,
        /// <summary>
        /// 数组类型
        /// </summary>
        Array = 0x08,
        /// <summary>
        /// 值类型
        /// </summary>
        ValueType = 0x10,
        /// <summary>
        /// 泛型类型
        /// </summary>
        GenericType = 0x20,
        /// <summary>
        /// 可以用来构造其他泛型类型的泛型定义
        /// </summary>
        GenericTypeDefinition = 0x40,
        /// <summary>
        /// 接口
        /// </summary>
        Interface = 0x80,
        /// <summary>
        /// 类
        /// </summary>
        Class = 0x100,
        /// <summary>
        /// 所有类型
        /// </summary>
        All = 0x200,

        /// <summary>
        /// 过滤条件, 非嵌套类 (在另一个类型内部定义的类型)
        /// </summary>
        NoNested = 0x2000,
        /// <summary>
        /// 过滤条件, 提供无参构造函数
        /// </summary>
        HasEmptyArgConstructor = 0x4000,
    }
}
