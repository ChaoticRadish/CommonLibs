using Common_Util.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct.Modal
{
    public interface ITagModalResult
    {
        /// <summary>
        /// 模态结果的类型
        /// </summary>
        ModalResult Result { get; }
        /// <summary>
        /// 关联标签值
        /// </summary>
        object? Tag { get; }
    }

    public readonly struct TagModalResult : ITagModalResult
    {
        public ModalResult Result { get; init; }
        public object? Tag { get; init; }

    }
}
