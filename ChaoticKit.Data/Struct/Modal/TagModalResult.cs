using ChaoticKit.Data.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Data.Struct.Modal
{
    /// <summary>
    /// 覆盖标签对象的模态结果
    /// </summary>
    public interface ITagModalResult
    {
        /// <summary>
        /// 模态结果的类型
        /// </summary>
        ModalResult Result { get; }
        /// <summary>
        /// 关联标签值
        /// </summary>
        object? Tag { get; }
    }
    /// <summary>
    /// 标签对象为 <typeparamref name="T"/> 的模态结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITagModalResult<T> : ITagModalResult
    {
        /// <summary>
        /// 关联标签值
        /// </summary>
        new T Tag { get; }
        object? ITagModalResult.Tag { get => Tag; }

    }
    public readonly struct TagModalResult : ITagModalResult
    {
        public ModalResult Result { get; init; }
        public object? Tag { get; init; }

        public static readonly TagModalResult None = new() { Result = ModalResult.Chaos, Tag = null };
        public static readonly TagModalResult Ok = new TagModalResult() { Result = ModalResult.Ok, Tag = null };
    }
    public readonly struct TagModalResult<T> : ITagModalResult<T>
    {
        public T Tag { get; init; }

        public ModalResult Result { get; init; }

        public static TagModalResult<T> Ok(T tag) => new() { Result = ModalResult.Ok, Tag = tag };
        public static TagModalResult<T> Cancel(T tag = default!) => new() { Result = ModalResult.Cancel, Tag = tag };
        public static TagModalResult<T> Chaos(T tag = default!) => new() { Result = ModalResult.Cancel, Tag = tag };
    }
}
