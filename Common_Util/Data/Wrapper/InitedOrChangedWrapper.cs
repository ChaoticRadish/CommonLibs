using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper
{
    /// <summary>
    /// 初始化或变更的包装器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct InitedOrChangedWrapper<T>
    {
        /// <summary>
        /// 包装的值
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// 等值比较器
        /// </summary>
        public IEqualityComparer<T>? Comparer { get; init; }
        /// <summary>
        /// 当前是否已经初始化值
        /// </summary>
        public bool Inited { get; private set; }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        /// <returns>如果初始化或变更了值, 则返回 <see langword="true"/></returns>
        public bool Set(T value)
        {
            IEqualityComparer<T> comparer = Comparer ?? EqualityComparer<T>.Default;
            T oldValue = Value;
            Value = value;
            if (!Inited || !comparer.Equals(oldValue, value))
            {
                Inited = true;
                return true;
            }
            else return false;
        }
        /// <summary>
        /// 保持当前值不变, 仅修改 <see cref="Inited"/> 为 <see langword="false"/>
        /// </summary>
        /// <returns></returns>
        public void Reset()
        {
            Inited = false;
        }
        /// <summary>
        /// 修改当前值为 <paramref name="value"/>, 并修改 <see cref="Inited"/> 为 <see langword="false"/>
        /// </summary>
        /// <returns></returns>
        public void Reset(T value)
        {
            Inited = false;
            Value = value;
        }

        public static implicit operator T(InitedOrChangedWrapper<T> wrapper)
        {
            return wrapper.Value;
        }
    }
}
