using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Maths
{
    /// <summary>
    /// 近似比较的帮助类
    /// </summary>
    public static class ApproximateHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(float x, float y)
        {
            return MathF.Abs(x - y) <= 1E-06f;
        }
    }
}
