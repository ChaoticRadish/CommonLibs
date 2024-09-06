using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 类型未被支持
    /// </summary>
    public class TypeNotSupportedException : Exception
    {

        public TypeNotSupportedException(Type errorType, string notSuppertedInfo) 
            : base($"类型 [{errorType.Name}] 不受支持: {notSuppertedInfo}")
        {
            ErrorType = errorType;
            NotSuppertedInfo = notSuppertedInfo;
        }

        /// <summary>
        /// 存在不支持的情况的类型
        /// </summary>
        public Type ErrorType { get; }

        /// <summary>
        /// 不支持的相关信息
        /// </summary>
        public string NotSuppertedInfo { get; }
    }
}
