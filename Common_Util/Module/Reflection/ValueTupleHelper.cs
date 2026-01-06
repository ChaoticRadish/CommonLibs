using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Reflection
{
    /// <summary>
    /// 元组相关的帮助类
    /// </summary>
    public static class ValueTupleHelper
    {
        /// <summary>
        /// 将输入的类型按顺序组合成元组
        /// </summary>
        /// <remarks>
        /// <paramref name="types"/> 内只有一个类型时, 会返回 <see cref="ValueTuple{T1}"/> 而不是唯一的这个类型
        /// </remarks>
        /// <param name="types"></param>
        /// <returns></returns>
        public static Type MakeValueTupleType(IEnumerable<Type> types)
        {
            Type[] typeArr = types.ToArray();
            return makeValueTupleType(typeArr);
        }
        private static Type makeValueTupleType(ReadOnlySpan<Type> types)
        {
            if (types.Length == 0)
                throw new Common_Util.Exceptions.General.ImplementationException("实现异常, 待整理为元组类型的类型列表为空");
            if (types.Length <= 7)
            {
                Type define = types.Length switch
                {
                    1 => typeof(ValueTuple<>),
                    2 => typeof(ValueTuple<,>),
                    3 => typeof(ValueTuple<,,>),
                    4 => typeof(ValueTuple<,,,>),
                    5 => typeof(ValueTuple<,,,,>),
                    6 => typeof(ValueTuple<,,,,,>),
                    7 => typeof(ValueTuple<,,,,,,>),
                    _ => throw new Common_Util.Exceptions.General.ImpossibleForkException()
                };
                return define.MakeGenericType(types.ToArray());
            }
            else
            {
                // 元组第 8 个元素必须是 TRest, 也就是就算刚好 8 元素, 也需要包裹成 ValueTuple
                Type define = typeof(ValueTuple<,,,,,,,>);
                Type[] gArgs = new Type[8];
                for (int i = 0; i < 7; i++)
                {
                    gArgs[i] = types[i];
                }
                gArgs[7] = makeValueTupleType(types[7..]);
                return define.MakeGenericType(gArgs);
            }
        }

    }
}
