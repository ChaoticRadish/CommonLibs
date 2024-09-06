using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 会发生无序集合变化的东西
    /// <para>可能的变化只有增加, 移除两种, 如果发生替换, 则表现为同时发生了增加与移除</para>
    /// </summary>
    public interface IUnorderedCollectionChanged
    {
        event EventHandler<UnorderedCollectionChangedEventArgs> UnorderedCollectionChanged;  
    }

    /// <summary>
    /// 会发生无序集合变化的东西 (泛型版本)
    /// <para>可能的变化只有增加, 移除两种, 如果发生替换, 则表现为同时发生了增加与移除</para>
    /// </summary>
    public interface IUnorderedCollectionChanged<T>
    {
        event EventHandler<UnorderedCollectionChangedEventArgs<T>> UnorderedCollectionChanged;
    }

    public interface IUnorderedCollectionChangedEventArgs
    {
        IList? AddItems { get; }

        IList? RemoveItems { get; }
    }

    public static class UnorderedCollectionChangedEventArgsHelper
    {
        public static UnorderedCollectionChangedEventArgs AddItems(params object?[] items)
        {
            return new UnorderedCollectionChangedEventArgs() { AddItems = items, RemoveItems = null };
        }
        public static UnorderedCollectionChangedEventArgs RemoveItems(params object?[] items)
        {
            return new UnorderedCollectionChangedEventArgs() { AddItems = null, RemoveItems = items };
        }
        public static UnorderedCollectionChangedEventArgs ReplaceItem(object? item)
        {
            return new UnorderedCollectionChangedEventArgs() { AddItems = new object?[] { item }, RemoveItems = new object?[] { item }, };
        }
        public static UnorderedCollectionChangedEventArgs Changed(IList? add, IList? remove)
        {
            return new UnorderedCollectionChangedEventArgs() { AddItems = add, RemoveItems = remove, };
        }


        public static UnorderedCollectionChangedEventArgs<T> AddItems<T>(params T[] items)
        {
            return new UnorderedCollectionChangedEventArgs<T>() { AddItems = items, RemoveItems = null };
        }
        public static UnorderedCollectionChangedEventArgs<T> RemoveItems<T>(params T[] items)
        {
            return new UnorderedCollectionChangedEventArgs<T>() { AddItems = null, RemoveItems = items };
        }
        public static UnorderedCollectionChangedEventArgs<T> ReplaceItem<T>(T item)
        {
            return new UnorderedCollectionChangedEventArgs<T>() { AddItems = [item], RemoveItems = [item], };
        }

        public static UnorderedCollectionChangedEventArgs<T> Changed<T>(IList<T>? add, IList<T>? remove)
        {
            return new UnorderedCollectionChangedEventArgs<T>() { AddItems = add, RemoveItems = remove, };
        }
    }

    public class UnorderedCollectionChangedEventArgs : IUnorderedCollectionChangedEventArgs
    {
        public required IList? AddItems { get; set; }
        public IEnumerable GetAddItems() => AddItems == null ? Enumerable.Empty<object>() : AddItems;

        public required IList? RemoveItems { get; set; }
        public IEnumerable GetRemoveItems() => RemoveItems == null ? Enumerable.Empty<object>() : RemoveItems;

        /// <summary>
        /// 变化项数量 (新增项数量 + 移除项数量)
        /// </summary>
        public int Count { get => AddCount + RemoveCount; }

        public int AddCount { get => AddItems?.Count ?? 0; }

        public int RemoveCount { get => RemoveItems?.Count ?? 0; }
    }
    public class UnorderedCollectionChangedEventArgs<T> : IUnorderedCollectionChangedEventArgs
    {
        public required IList<T>? AddItems { get; set; }
        public IEnumerable<T> GetAddItems() => AddItems ?? [];

        public required IList<T>? RemoveItems { get; set; }
        public IEnumerable<T> GetRemoveItems() => RemoveItems ?? [];

        /// <summary>
        /// 变化项数量 (新增项数量 + 移除项数量)
        /// </summary>
        public int Count { get => AddCount + RemoveCount; }

        public int AddCount { get => AddItems?.Count ?? 0; }

        public int RemoveCount { get => RemoveItems?.Count ?? 0; }

        IList? IUnorderedCollectionChangedEventArgs.AddItems => AddItems == null ? null : (IList)AddItems;
        IList? IUnorderedCollectionChangedEventArgs.RemoveItems => RemoveItems == null ? null : (IList)RemoveItems;
    }
}
