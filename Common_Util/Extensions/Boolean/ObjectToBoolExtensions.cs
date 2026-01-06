using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.Boolean
{
    public static class ObjectToBoolExtensions
    {
        /// <summary>
        /// 将 <paramref name="obj"/> 适配为 <see langword="bool"/> 
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> 会视为 <see langword="false"/>. <br/>
        /// 数值类型正数会视为 <see langword="true"/>, 零或负数会视为 <see langword="false"/>
        /// </remarks>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool AsBool(this object? obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            if (obj is string str) return ValueHelper.IsTrueString(str);
            if (obj is sbyte _sbyte) return _sbyte > 0;
            if (obj is byte _byte) return _byte > 0;
            if (obj is int _int) return _int > 0;
            if (obj is uint _uint) return _uint > 0;
            if (obj is short _short) return _short > 0;
            if (obj is ushort _ushort) return _ushort > 0;
            if (obj is long _long) return _long > 0;
            if (obj is ulong _ulong) return _ulong > 0;
            if (obj is float _float) return _float > 0;
            if (obj is double _double) return _double > 0;
            return false;
        }
    }
}
