﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 不可能分支异常
    /// </summary>
    /// <remarks>
    /// 代码走到了一个理论上不可能发生的分支
    /// </remarks>
    public sealed class ImpossibleForkException : Exception
    {
        private const string fixedString = "代码走到了一个理论上不可能发生的分支! ";

        public ImpossibleForkException() : base(fixedString) { }

        /// <summary>
        /// 实例化时, 附带额外的文本信息, 一般用来描述为什么是不可能发生的分支
        /// </summary>
        /// <param name="message"></param>
        public ImpossibleForkException(string message) : base($"{fixedString}{message}") { }

        /// <summary>
        /// 传入的 <paramref name="obj"/> 在调用的此刻不可能为空, 如果它现在是空的, 则抛出异常
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramName"></param>
        /// <exception cref="ImpossibleForkException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImpossibleNull([NotNull]object? obj, [CallerArgumentExpression(nameof(obj))] string paramName = "")
        {
            if (obj == null) throw new ImpossibleForkException($"传入对象 {paramName} 在此刻不可能为空, 然而却是空的! ");
        }
    }
}
