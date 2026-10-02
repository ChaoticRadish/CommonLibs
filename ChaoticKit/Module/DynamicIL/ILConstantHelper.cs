using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.DynamicIL
{
    public static class ILConstantHelper
    {
        /// <summary>
        /// 根据 <typeparamref name="TEnum"/> 的底层类型, 在 <paramref name="il"/> 添加加载其常量值的语句
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="il"></param>
        /// <param name="enum"></param>
        public static void Load<TEnum>(ILGenerator il, TEnum @enum)
            where TEnum : Enum
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

            long longValue = Convert.ToInt64(@enum);

            if (underlyingType == typeof(int) || underlyingType == typeof(uint) ||
                underlyingType == typeof(short) || underlyingType == typeof(ushort) ||
                underlyingType == typeof(byte) || underlyingType == typeof(sbyte))
            {
                // 对于 32 位及以下整数，加载后会被自动扩展/截断为目标类型
                int intValue = (int)longValue;

                // 优化：根据数值大小使用短指令
                switch (intValue)
                {
                    case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                    case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                    case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                    case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                    case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                    case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                    case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                    case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                    case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                    case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                    default:
                        // 使用 sbyte 参数以节省空间（如果值在 -128 到 127 之间）
                        if (intValue >= sbyte.MinValue && intValue <= sbyte.MaxValue)
                            il.Emit(OpCodes.Ldc_I4_S, (sbyte)intValue);
                        else
                            il.Emit(OpCodes.Ldc_I4, intValue);
                        break;
                }
            }
            else if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
            {
                // 64 位枚举需要使用 Ldc_I8
                il.Emit(OpCodes.Ldc_I8, longValue);
            }
        }

    }
}
