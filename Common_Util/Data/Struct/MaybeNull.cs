using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 接口: 包装一个可能为 <see langword="null"/> 的对象的包装器
    /// </summary>
    /// <remarks>
    /// 如果实现此接口的类型不是结构体, 则在 <see langword="null"/> 时, 表示无值, 即 <see cref="HasValue"/> 为 <see langword="false"/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IMaybeNull<T> 
    {
        /// <summary>
        /// 当前是否拥有值, 即逻辑上不为 <see langword="null"/>
        /// </summary>
        public bool HasValue { get; }
        /// <summary>
        /// 当前保存的值
        /// </summary>
        public T? Value { get; }
    }

    public static class MaybeNullHelper
    {
        /// <summary>
        /// 比较两个包装器是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(IMaybeNull<T>? left, IMaybeNull<T>? right)
        {
            bool hasValueLeft = left?.HasValue ?? false;
            bool hasValueRight = right?.HasValue ?? false;
            if (hasValueLeft && hasValueRight)
            {
                return left!.Value!.Equals(right!.Value);
            }
            else if (!hasValueLeft && !hasValueRight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 由一个可空值创建一个默认实现的包装器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public static MaybeNull<T> From<T>(T? nullable)
            where T : struct
        {
            if (nullable.HasValue)
                return new MaybeNull<T>(nullable.Value);
            else
                return new MaybeNull<T>();
        }
        /// <summary>
        /// 由一个 <see langword="class"/> 类型的值创建一个默认实现的包装器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MaybeNull<T> From<T>(T? value)
            where T : class
        {
            if (value != null)
                return new MaybeNull<T>(value);
            else
                return new MaybeNull<T>();
        }
    }


    /// <summary>
    /// 用于包装一个可能为 <see langword="null"/> 的对象的包装器
    /// </summary>
    /// <remarks>
    /// 是否拥有值由 <see cref="HasValue"/> 属性决定, 而非 <see cref="Value"/> 是否为 <see langword="null"/> 决定.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public readonly struct MaybeNull<T> : IMaybeNull<T> ,IEquatable<IMaybeNull<T>>, IEquatable<MaybeNull<T>>
    {
        #region 构造方法

        /// <summary>
        /// 实例化为表示没有值的包装器
        /// </summary>
        public MaybeNull()
        {
            HasValue = false;
            Value = default;
        }
        /// <summary>
        /// 实例化包装器, 当 <paramref name="value"/> == <see langword="null"/> 时, 表示没有值, 反之表示有值, 其值为 <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        public MaybeNull(T? value)
        {
            HasValue = value != null;
            Value = value;
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 取得一个表示没有值的包装器
        /// </summary>
        public readonly static MaybeNull<T> Null = new();
        #endregion

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue { get; }

        public T? Value { get; }


        #region 相等比较
        /// <summary>
        /// 比较当前包装器与另一个包装器是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(IMaybeNull<T>? other)
        {
            return MaybeNullHelper.Equals(this, other);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(MaybeNull<T> other)
        {
            return MaybeNullHelper.Equals(this, other);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object? obj)
        {
            if (obj is MaybeNull<T> other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public readonly override int GetHashCode()
        {
            return HasValue ? Value!.GetHashCode() : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MaybeNull<T> left, MaybeNull<T> right)
        {
            return left.Equals(right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MaybeNull<T> left, MaybeNull<T> right)
        {
            return !(left == right);
        }
        #endregion

        #region 隐式转换
        public static implicit operator MaybeNull<T>(T? value)
        {
            return new(value);
        }
        public static implicit operator MaybeNull<T>((bool hasValue, T? value) args)
        {
            if (args.hasValue)
                return new(args.value);
            else
                return new();
        }
        #endregion
    }
}
