using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Pages.Layout
{
    public interface IMultiPageLayout<ItemType> : IList<ItemType?>
        where ItemType : MultiPageLayoutItem, new()
    {
        /// <summary>
        /// 默认选择的页面索引
        /// </summary>
        int? DefaultSelectIndex { get; set; }

        /// <summary>
        /// 添加页面
        /// </summary>
        /// <param name="item"></param>
        void AddPage(ItemType item);

        /// <summary>
        /// 展示指定索引的页面
        /// </summary>
        /// <param name="pageIndex"></param>
        void ShowPage(int pageIndex);

        /// <summary>
        /// 获取指定索引的页面
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        PageBase? GetPage(int pageIndex);

    }
    public static class IMultiPageLayoutEx
    {
        public static void AddPage<ItemType>(this IMultiPageLayout<ItemType> layout, PageBase page)
            where ItemType : MultiPageLayoutItem, new()
        {
            layout.AddPage(new ItemType()
            {
                Index = page.PageIndex,
                Page = page,
                PageDesc = page.Name,
                PageName = page.Name,
            });
        }
        public static void AddPage<ItemType>(this IMultiPageLayout<ItemType> layout, int index, PageBase page)
            where ItemType : MultiPageLayoutItem, new()
        {
            page.PageIndex = index;
            layout.AddPage(new ItemType()
            {
                Index = page.PageIndex,
                Page = page,
                PageDesc = page.Name,
                PageName = page.Name,
            });
        }
        public static void AddPage<ItemType>(this IMultiPageLayout<ItemType> layout, int index, string? name, PageBase page)
            where ItemType : MultiPageLayoutItem, new()
        {
            page.PageIndex = index;
            page.Name = name;
            layout.AddPage(new ItemType()
            {
                Index = page.PageIndex,
                Page = page,
                PageDesc = name,
                PageName = name,
            });
        }
    }
}
