using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 可以添加特定的某个类型项的某个东西
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAddable<T> 
    {
        void Add(T item);
    }
    /// <summary>
    /// 可以添加任意对象的某个东西
    /// </summary>
    public interface IAddable : IAddable<object?>
    {

    }
}
