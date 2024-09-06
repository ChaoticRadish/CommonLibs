using Common_Util.Exceptions.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 当传入 true 时, 将返回两枚举值与运算后的值
        /// <para>注意: 此方法与运算时, 需要装箱拆箱, 会有较大的开销! </para>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enum"></param>
        /// <param name="b"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static TEnum AddFlagWhen<TEnum>(this TEnum @enum, bool b, TEnum add)
            where TEnum : struct, Enum
        {
            return b ? FlagsEnumHelper<TEnum>.BitAnd(@enum, add) : @enum;
        }

        /// <summary>
        /// 连续执行检查和与运算. 根据传入的条件是否为 true, 将当前值与条件对应值执行与运算
        /// <para>注意: 此方法需要每一次与运算, 都需要装箱拆箱, 会有较大的开销! </para>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enum"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public static TEnum AddFlagWhen<TEnum>(this TEnum @enum, params (bool b, TEnum add)[] conditions)
            where TEnum : struct, Enum
        {
            TEnum output = @enum;
            foreach (var (b, add) in conditions)
            {
                if (b)
                {
                    output = FlagsEnumHelper<TEnum>.BitAnd(output, add);
                }
            }
            return output;
        }


        /// <summary>
        /// 不为null时调用 <see cref="Enum.ToString()"/>, 为null时返回输入的默认值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="whenNullDefault"></param>
        /// <returns></returns>
        public static string ToString(this Enum? value, string whenNullDefault)
        {
            return value == null ? whenNullDefault : value.ToString();
        }

        internal static class FlagsEnumHelper<TEnum>
            where TEnum : struct, Enum
        {
            static FlagsEnumHelper()
            {
                type = typeof(TEnum);
                isFlagsEnum = type.ExistCustomAttribute<FlagsAttribute>();
                if (!isFlagsEnum)
                {
                    throw new TypeNotSupportedException(type, "类型不是位标志枚举! ");
                }
                var uType = Enum.GetUnderlyingType(type);
                typeCode = Type.GetTypeCode(uType);
                switch (typeCode)
                {
                    case TypeCode.SByte:
                        break;
                    case TypeCode.Byte:
                        break;
                    case TypeCode.Int16:
                        break;
                    case TypeCode.UInt16:
                        break;
                    case TypeCode.Int32:
                        break;
                    case TypeCode.UInt32:
                        break;
                    case TypeCode.Int64:
                        break;
                    case TypeCode.UInt64:
                        break;
                    default:
                        throw new TypeNotSupportedException(uType, $"枚举类型的底层类型为不受支持的类型类别 {typeCode}");
                }
            }
            private static Type type;
            private static bool isFlagsEnum;

            private static TypeCode typeCode;



            public static TEnum BitAnd(TEnum e1, TEnum e2)
            {
                switch (typeCode)
                {
                    //case TypeCode.Byte:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToByte(e1) | Convert.ToByte(e2));
                    //case TypeCode.SByte:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToSByte(e1) | Convert.ToSByte(e2));
                    //case TypeCode.Int16:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToInt16(e1) | Convert.ToInt16(e2));
                    //case TypeCode.UInt16:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToUInt16(e1) | Convert.ToUInt16(e2));
                    //case TypeCode.Int32:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToUInt32(e1) | Convert.ToUInt32(e2));
                    //case TypeCode.UInt32:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToUInt32(e1) | Convert.ToUInt32(e2));
                    //case TypeCode.Int64:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToInt64(e1) | Convert.ToInt64(e2));
                    //case TypeCode.UInt64:
                    //    return (TEnum)Enum.ToObject(type, Convert.ToUInt64(e1) | Convert.ToUInt64(e2));

                    case TypeCode.Byte:
                        return (TEnum)Enum.ToObject(type, (byte)(object)e1 | (byte)(object)e2);
                    case TypeCode.SByte:
                        return (TEnum)Enum.ToObject(type, (sbyte)(object)e1 | (sbyte)(object)e2);
                    case TypeCode.Int16:
                        return (TEnum)Enum.ToObject(type, (short)(object)e1 | (short)(object)e2);
                    case TypeCode.UInt16:
                        return (TEnum)Enum.ToObject(type, (ushort)(object)e1 | (ushort)(object)e2);
                    case TypeCode.Int32:
                        return (TEnum)Enum.ToObject(type, (int)(object)e1 | (int)(object)e2);
                    case TypeCode.UInt32:
                        return (TEnum)Enum.ToObject(type, (uint)(object)e1 | (uint)(object)e2);
                    case TypeCode.Int64:
                        return (TEnum)Enum.ToObject(type, (long)(object)e1 | (long)(object)e2);
                    case TypeCode.UInt64:
                        return (TEnum)Enum.ToObject(type, (ulong)(object)e1 | (ulong)(object)e2);

                    default:    // 实际不会有这种情况
                        return (TEnum)(object)0;
                }
            }
        }
    }

}
