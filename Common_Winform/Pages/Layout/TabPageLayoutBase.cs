using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Pages.Layout
{
    public class TabPageLayoutBase<ItemType> : PageBase, ITabPageLayout<ItemType>
        where ItemType : MultiPageLayoutItem, new()
    {
        #region 状态
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        [Category("CVII_自定义_状态"), DisplayName("是否已经初始化")]
        public bool IsInited { get; private set; } = false;

        
        #endregion
        #region 参数
        /// <summary>
        /// 初始化完成后默认选择页面索引
        /// </summary>
        [Category("CVII_自定义_初始化过程参数"), DisplayName("初始化完成后默认选择页面索引")]
        public int? DefaultSelectIndex { get; set; }

        #endregion


        #region 初始化
        public void Init()
        {
            if (IsInited)
            {
                // 只给初始化一次
                return;
            }

            SuspendLayout();
            AddPages();
            AddPagesHandle?.Invoke(this);
            InitBody();
            ResumeLayout();
            IsInited = true;
        }

        /// <summary>
        /// 内部方法, 提供给子类添加页面
        /// </summary>
        protected virtual void AddPages() { }
        public virtual void InitBody() { }

        public virtual void ShowPage(int pageIndex) { }

        public PageBase? GetPage(int pageIndex)
        {
            return this[pageIndex]?.Page;
        }

        public void AddPage(ItemType item)
        {
            if (IsInited) return;

            int index = item.Index;
            if (Items.ContainsKey(index))
            {
                Items[index] = item;
            }
            else
            {
                Items.Add(index, item);
            }
        }

        /// <summary>
        /// 初始化过程中, 从对象外部添加页面的方法
        /// </summary>
        public Action<TabPageLayoutBase<ItemType>>? AddPagesHandle { get; set; }

        #endregion


        #region 页面数据
        public virtual ItemType? this[int index]
        {
            get
            {
                if (Items.TryGetValue(index, out ItemType? item))
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (IsInited)
                {
                    // 已经初始化的话就不给加了
                    return;
                }
                //MultiPageLayoutItem item = new()
                //{
                //    Index = index,
                //    Page = value,
                //    PageName = value?.Text,
                //};
                if (Items.ContainsKey(index))
                {
                    Items[index] = value;
                }
                else
                {
                    Items.Add(index, value);
                }
            }
        }

        protected Dictionary<int, ItemType?> Items = new();
        public int Count => Items.Count;
        public bool IsReadOnly => IsInited;


        public int IndexOf(ItemType? item)
        {
            int[] keys = Items.Keys.ToArray();
            foreach (int key in keys)
            {
                if (Items[key] == item)
                {
                    return key;
                }
            }
            return -1;
        }
        public void Insert(int index, ItemType? item)
        {
            this[index] = item;
        }
        public void RemoveAt(int index)
        {
            if (IsInited) return;
            if (Items.ContainsKey(index))
            {
                Items.Remove(index);
            }
        }
        public void Add(ItemType? item)
        {
            this[Items.Keys.Max() + 1] = item;
        }
        public void Clear()
        {
            if (IsInited) return;
            Items.Clear();
        }
        public bool Contains(ItemType? item)
        {
            int[] keys = Items.Keys.ToArray();
            foreach (int key in keys)
            {
                if (Items[key] == item)
                {
                    return true;
                }
            }
            return false;
        }
        public void CopyTo(ItemType?[] array, int arrayIndex)
        {
            if (array == null) return;
            PageBase?[] pages = Items.Values.Select(i => i?.Page).ToArray();
            pages.CopyTo(array, arrayIndex);
        }
        public bool Remove(ItemType? item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            if (Items.ContainsKey(index))
            {
                return Items.Remove(index);
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<ItemType?> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }


        #endregion

        #region 重载
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsInited && DefaultSelectIndex != null)
            {
                ShowPage(DefaultSelectIndex.Value);
            }
        }
        #endregion








    }
}
