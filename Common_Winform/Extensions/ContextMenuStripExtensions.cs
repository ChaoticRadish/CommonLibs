using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Extensions
{
    public static class ContextMenuStripExtensions
    {
        /// <summary>
        /// 遍历上下文条状菜单中的所有子项
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public static IEnumerable<ToolStripItem> AllItem(this ContextMenuStrip menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is ToolStripItem toolStripItem)
                {
                    yield return toolStripItem;
                }
            }
        }

        /// <summary>
        /// 遍历上下文条状菜单中的所有类型为 <typeparamref name="TItem"/> 的子项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="menu"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> AllItem<TItem>(this ContextMenuStrip menu) 
            where TItem : ToolStripItem
        {
            foreach (var item in menu.Items)
            {
                if (item is TItem tItem)
                {
                    yield return tItem;
                }
            }
        }
    }
}
