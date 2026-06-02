using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Extensions.NumberParsing
{
    public static class ObjectNumberExtensions
    {
        public static double? AsDouble(this object? obj)
        {
            if (obj == null) return null;
            if (obj is double d) return d;
            if (obj is float f) return f;
            if (obj is decimal dc) return (double)dc;
            if (obj is int i) return i;
            if (obj is uint ui) return ui;
            if (obj is byte b) return b;
            if (obj is sbyte sb) return sb;
            if (obj is short s) return s;
            if (obj is ushort us) return us;
            if (obj is long l) return l;
            if (obj is ulong ul) return ul;
            if (obj is string str && double.TryParse(str, out d)) return d;
            return null;
        }
        public static double AsDouble(this object? obj, double defaultValue)
            => AsDouble(obj) ?? defaultValue;
    }
}
