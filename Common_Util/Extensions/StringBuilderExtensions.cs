using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class StringBuilderExtensions
    {
        static StringBuilderExtensions()
        {
            DefaultNeedRemoveChars = new char[33];
            for (int i = 0; i < 33; i++)
            {
                DefaultNeedRemoveChars[i] = (char)i;
            }
        }
        /// <summary>
        /// 默认 <see cref="Trim(StringBuilder)"/> 需要移除的字符, 默认值为 ascii 小于33 的所有值
        /// </summary>
        public static char[] DefaultNeedRemoveChars { get; private set; }



        /// <summary>
        /// 往 StringBuilder 后添加键值对
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="split">分隔字符串</param>
        /// <param name="newLine">是否添加新行</param>
        /// <returns>返回传入的 StringBuilder</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendKeyValuePair(this StringBuilder builder, string key, string value, string split = ": ", bool newLine = true)
        {
            builder.Append(key).Append(split).Append(value);
            if (newLine)
            {
                builder.AppendLine();
            }
            return builder;
        }
        /// <summary>
        /// 往 StringBuilder 后添加键值对
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="split"></param>
        /// <param name="newLine"></param>
        /// <returns>返回传入的 StringBuilder</returns>
        public static StringBuilder AppendKeyValuePair(this StringBuilder builder, string key, object value, string split = ": ", bool newLine = true)
        {
            builder.Append(key).Append(split).Append(value);
            if (newLine)
            {
                builder.AppendLine();
            }
            return builder;
        }

        /// <summary>
        /// 剔除首尾的空白字符 (数组 <see cref="DefaultNeedRemoveChars"/> 内)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static StringBuilder Trim(this StringBuilder builder)
        {
            return Trim(builder, DefaultNeedRemoveChars);
        }
        /// <summary>
        /// 剔除首尾的在输入字符串内的字符
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="needRemoveChars"></param>
        /// <returns></returns>
        public static StringBuilder Trim(this StringBuilder builder, string needRemoveChars)
        {
            return Trim(builder, needRemoveChars.GroupBy(i => i).Select(i => i.Key).ToArray());
        }
        /// <summary>
        /// 剔除首尾的在输入数组内的字符
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="needRemoveChars"></param>
        /// <returns></returns>
        public static StringBuilder Trim(this StringBuilder builder, params char[] needRemoveChars)
        {
            if (needRemoveChars == null || needRemoveChars.Length == 0) return builder;

            int index = 0;
            while (builder.Length > index && needRemoveChars.Contains(builder[index]))
            {
                index++;
            }
            if (index > 0)
            {
                builder.Remove(0, index);
            }
            index = builder.Length;
            while (index > 0 && needRemoveChars.Contains(builder[index - 1]))
            {
                index--;
            }
            if (index < builder.Length)
            {
                builder.Remove(index, builder.Length - index);
            }

            return builder;
        }



        /// <summary>
        /// 往 StringBuilder 后追加一个东西, 并用输入的一左一右一对字符串包裹起来
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="sth">追加的东西</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>返回传入的 StringBuilder</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWrap(this StringBuilder builder, object? sth, string left, string right)
        {
            builder.Append(left).Append(sth).Append(right);
            return builder;
        }
        /// <summary>
        /// 往 StringBuilder 后追加一个东西, 并用输入的一左一右一对字符串包裹起来
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="sth">追加的东西</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>返回传入的 StringBuilder</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWrap(this StringBuilder builder, object? sth, char left, char right)
        {
            builder.Append(left).Append(sth).Append(right);
            return builder;
        }
        /// <summary>
        /// 往 StringBuilder 后追加一个东西, 并用输入的字符串包裹起来
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="sth"></param>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWrap(this StringBuilder builder, object? sth, string wrapper)
        {
            builder.Append(wrapper).Append(sth).Append(wrapper);
            return builder;
        }
        /// <summary>
        /// 往 StringBuilder 后追加一个东西, 并用输入的字符串包裹起来
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="sth"></param>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWrap(this StringBuilder builder, object? sth, char wrapper)
        {
            builder.Append(wrapper).Append(sth).Append(wrapper);
            return builder;
        }

        /// <summary>
        /// 当传入的 <see langword="bool"/> 为 <see langword="true"/> 时, 调用 <see cref="StringBuilder.Append(object?)"/> 追加传入的对象
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="condition"></param>
        /// <param name="sth"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWhen(this StringBuilder builder, bool condition, object? sth)
        {
            if (condition) { builder.Append(sth); }
            return builder;
        }
        /// <summary>
        /// 当传入的 <see langword="bool"/> 为 <see langword="true"/> 时, 调用 <see cref="StringBuilder.Append(object?)"/> 依次追加传入的对象
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="condition"></param>
        /// <param name="sth"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendWhen(this StringBuilder builder, bool condition, params object?[] sths)
        {
            if (sths.Length > 0 && condition) 
            {
                foreach (var sth in sths)
                {
                    builder.Append(sth);
                }
            }
            return builder;
        }
    }
}
