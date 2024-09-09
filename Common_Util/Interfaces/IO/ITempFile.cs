using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.IO
{
    /// <summary>
    /// 临时文件接口
    /// </summary>
    /// <remarks>
    /// 需实现 <see cref="IDisposable"/>, 以支持使用 <see langword="using"/> 在离开作用域后自动调用 <see cref="IDisposable.Dispose"/> 释放该临时文件 <br/>
    /// 释放即永久性的丢弃该临时文件
    /// </remarks>
    public interface ITempFile : IDisposable
    {
        /// <summary>
        /// 临时文件对应的文件实际绝对路径
        /// </summary>
        string Path { get; }
    }
    /// <summary>
    /// 临时文件管理器接口
    /// </summary>
    /// <remarks>
    /// 需实现 <see cref="IDisposable"/>, 以支持使用 <see langword="using"/> 在离开作用域后自动调用 <see cref="IDisposable.Dispose"/> 释放该管理器 <br/>
    /// 释放即永久性的丢弃该管理器创建的所有临时文件
    /// </remarks>
    public interface ITempFileManager : IDisposable
    {
        /// <summary>
        /// 创建并返回一个新的临时文件
        /// </summary>
        /// <returns></returns>
        ITempFile NewOne();
    }
    /// <summary>
    /// 用于创建特定类型临时文件工厂接口
    /// </summary>
    public interface ITempFileManager<TTempFile> : ITempFileManager
        where TTempFile : ITempFile
    {
        /// <summary>
        /// 创建并返回一个新特定类型的临时文件
        /// </summary>
        /// <returns></returns>
        new TTempFile NewOne();
    }
}
