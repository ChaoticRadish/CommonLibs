using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data
{
    public static class GuidHelper
    {
        /// <summary>
        /// 取得一个 <see cref="GuidBuilder"/>
        /// </summary>
        /// <returns></returns>
        public static GuidBuilder Builder() => new();

        /// <summary>
        /// <see cref="Guid"/> 构建器, 用于创建非标准的 <see cref="Guid"/>, 一般用于不与外部系统交互, 仅需要其唯一性的场景
        /// </summary>
        /// <remarks>
        /// 提供了一些方法来追加 <see langword="byte"/> 到 <see cref="Bytes"/>, 追加完成后调用 <see cref="Build"/> 方法来生成 <see cref="Guid"/> <br/>
        /// 追加达到上限 16 <see langword="byte"/> 后不会继续添加, 也不会抛出异常
        /// </remarks>
        public struct GuidBuilder
        {
            /// <summary>
            /// 构建 <see cref="Guid"/> 所需字节数组的大小
            /// </summary>
            private const int Size = 16;
            /// <summary>
            /// 用于最终生成 <see cref="Guid"/> 的 <see langword="byte"/> 数组
            /// </summary>
            private unsafe fixed byte InnerBytes[Size];
            private unsafe void SetByte(int index, byte b)
            {
                InnerBytes[index] = b;
            }
            /// <summary>
            /// 返回内含数组中 <paramref name="index"/> 处的字节
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public readonly byte this[int index]
            {
                get
                {
                    unsafe
                    {
                        return InnerBytes[index];
                    }
                }
            }
            /// <summary>
            /// 取得内含的字节数组, 此操作会拷贝得到一个新的数组
            /// </summary>
            /// <returns></returns>
            public readonly byte[] GetBytes()
            {
                byte[] bs = new byte[Size];
                unsafe
                {
                    for (int i = 0; i < Size; i++)
                    {
                        var b = InnerBytes[i];
                    }
                }
                return bs;
            }

            /// <summary>
            /// 添加 <see langword="byte"/> 到 <see cref="Bytes"/> 中的过程中的当前位置
            /// </summary>
            public int CurrentIndex { get; private set; } 
            private readonly int NeedAddCount(int inputLength)
            {
                return inputLength + CurrentIndex > Size ? Size - CurrentIndex : inputLength;
            }


            /// <summary>
            /// 添加输入字节到当前索引处, 并推进索引位置
            /// </summary>
            /// <param name="b"></param>
            public void Add(byte b)
            {
                if (CurrentIndex >= Size) return;
                SetByte(CurrentIndex, b);
                CurrentIndex++;
            }
            /// <summary>
            /// 添加输入字节到当前索引处, 并推进索引位置
            /// </summary>
            /// <param name="bs"></param>
            public void Add(params byte[] bs)
            {
                if (CurrentIndex >= Size) return;
                if (bs == null || bs.Length == 0) return;
                int addCount = NeedAddCount(bs.Length);
                for (int i = 0; i < addCount; i++)
                {
                    SetByte(CurrentIndex, bs[i]);
                    CurrentIndex++;
                }
            }
            /// <summary>
            /// 将输入值拆分为字节后, 添加到当前索引处, 并推进索引位置
            /// </summary>
            /// <param name="i"></param>
            /// <param name="bigEndian">是否使用大端序</param>
            public void Add(int i, bool bigEndian = true)
            {
                int addCount = NeedAddCount(4);
                if (bigEndian)
                {
                    for (int index = 0; index < addCount; index++)
                    {
                        SetByte(CurrentIndex, (byte)(i >> ((4 - 1 - index) * 8)));
                        CurrentIndex++;
                    }
                }
                else
                {
                    for (int index = 0; index < addCount; index++)
                    {
                        SetByte(CurrentIndex, (byte)(i >> (index * 8)));
                        CurrentIndex++;
                    }
                }
            }
            /// <summary>
            /// 将输入值拆分为字节后, 添加到当前索引处, 并推进索引位置
            /// </summary>
            /// <param name="l"></param>
            /// <param name="bigEndian">是否使用大端序</param>
            public void Add(long l, bool bigEndian = true)
            {
                int addCount = NeedAddCount(8);
                if (bigEndian)
                {
                    for (int index = 0; index < addCount; index++)
                    {
                        SetByte(CurrentIndex, (byte)(l >> ((8 - 1 - index) * 8)));
                        CurrentIndex++;
                    }
                }
                else
                {
                    for (int index = 0; index < addCount; index++)
                    {
                        SetByte(CurrentIndex, (byte)(l >> (index * 8)));
                        CurrentIndex++;
                    }
                }
            }
            /// <summary>
            /// 从当前索引处开始添加 <paramref name="byteCount"/> 个随机字节, 同时会推进当前索引位置
            /// </summary>
            /// <param name="byteCount"></param>
            /// <param name="random"></param>
            public void AddRandom(int byteCount, System.Random? random = null)
            {
                if (byteCount <= 0) return;
                int addCount = NeedAddCount(byteCount);
                random ??= System.Random.Shared;
                for (int i = 0; i < addCount; i++)
                {
                    SetByte(CurrentIndex, (byte)random.Next(byte.MaxValue + 1));
                    CurrentIndex++;
                }
            }
            /// <summary>
            /// 从当前索引处开始添加随机字节, 直到达到需求的上限值, 同时会推进当前索引位置
            /// </summary>
            /// <param name="random"></param>
            public void AddRandom(System.Random? random = null)
            {
                int addCount = Size - CurrentIndex;
                if (addCount <= 0) return;
                random ??= System.Random.Shared;
                for (int i = 0; i < addCount; i++)
                {
                    SetByte(CurrentIndex, (byte)random.Next(byte.MaxValue + 1));
                    CurrentIndex++;
                }
            }

            /// <summary>
            /// 重置输入状态
            /// </summary>
            public void Reset()
            {
                CurrentIndex = 0;
                unsafe
                {
                    for (int i = 0; i < Size; i++)
                    {
                        InnerBytes[i] = 0;
                    }
                }
            }


            /// <summary>
            /// 使用当前已有的字节数组创建一个 <see cref="Guid"/>
            /// </summary>
            /// <returns></returns>
            public readonly Guid Build()
            {
                Guid guid;
                unsafe
                {
                    fixed (byte* ptr = InnerBytes)
                    {
                        guid = new(new ReadOnlySpan<byte>(ptr, Size));
                    }     
                }
                return guid;
            }

            #region 遍历
            /// <summary>
            /// 取得对所有字节的迭代器
            /// </summary>
            /// <returns></returns>
            public readonly IEnumerable<byte> AllBytes()
            {
                for (int i = 0; i < Size; i++)
                {
                    yield return this[i];
                }
            }

            #endregion

        }
    }
}
