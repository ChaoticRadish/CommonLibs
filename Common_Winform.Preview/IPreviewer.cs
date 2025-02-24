using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Preview
{
    /// <summary>
    /// 文件预览器接口
    /// </summary>
    public interface IPreviewer
    {
        /// <summary>
        /// 当前展示的文件名 (完整路径)
        /// </summary>
        string? FileName { get; }

        /// <summary>
        /// 判断传入文件名是否可以展示
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool CanShow(string fileName);

        /// <summary>
        /// 展示指定 PDF 文件
        /// </summary>
        /// <param name="fileName"></param>
        void Show(string fileName);

        /// <summary>
        /// 清空当前展示的预览内容
        /// </summary>
        void Clear();
    }
    /// <summary>
    /// 分页文件预览器接口
    /// </summary>
    public interface IPagingFilePreviewer : IPreviewer
    {
        /// <summary>
        /// 提供移动到顶部 (首页) 功能 
        /// </summary>
        bool SupplyMoveToTop { get; }

        /// <summary>
        /// 提供移动到底部 (尾页) 功能
        /// </summary>
        bool SupplyMoveToBottom { get; }

        /// <summary>
        /// 提供移动到指定页码功能
        /// </summary>
        bool SupplyMoveToPage { get; }

        /// <summary>
        /// 提供获取当前页码功能
        /// </summary>
        bool SupplyGetPageCode { get; }

        /// <summary>
        /// 提供获取总页码功能
        /// </summary>
        bool SupplyGetTotalPage { get; }

        /// <summary>
        /// 移动到顶部 (首页) 
        /// </summary>
        void MoveToTop();
        /// <summary>
        /// 提供移动到底部 (尾页) 
        /// </summary>
        void MoveToBottom();
        /// <summary>
        /// 移动到指定页码
        /// </summary>
        /// <param name="page"></param>
        void MoveToPage(int page);
        /// <summary>
        /// 获取当前页码
        /// </summary>
        /// <returns></returns>
        int GetPageCode();
        /// <summary>
        /// 获取总页码
        /// </summary>
        /// <returns></returns>
        int GetTotalPage(); 
    }
}
