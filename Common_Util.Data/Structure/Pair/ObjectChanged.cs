using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 类型 <typeparamref name="T"/> 的某个东西变更前后分别是什么值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectChanged<T>
    {
        /// <summary>
        /// 变更前
        /// </summary>
        T OldOne { get; }
        /// <summary>
        /// 变更后
        /// </summary>
        T NewOne { get; }
    }
    public struct ObjectChanged<T> : IObjectChanged<T>
    {
        public T OldOne { get; set; }

        public T NewOne { get; set; }

        public ObjectChanged(T oldNew, T newOne)
        {
            OldOne = oldNew;
            NewOne = newOne;
        }

        #region 隐式转换
        public static implicit operator ObjectChanged<T>((T oldOne, T newOne) obj)
        {
            return new ObjectChanged<T>()
            {
                NewOne = obj.newOne,
                OldOne = obj.oldOne,
            };
        }
        #endregion
    }
}
