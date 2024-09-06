using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Data.Property
{
    /// <summary>
    /// 作用域
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// 检查是否交错
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool CheckInterlace(IScope other);

        /// <summary>
        /// 检查是否相同作用域
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool CheckSame(IScope other);
    }
}
