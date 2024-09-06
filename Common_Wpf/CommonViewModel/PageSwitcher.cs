using Common_Util.Data.Wrapped;
using Common_Util.Module.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common_Wpf.CommonViewModel
{
    /// <summary>
    /// 页面切换器 (单页显示)
    /// </summary>
    public class PageSwitcher : INotifyPropertyChanged
    {
        public PageSwitcher()
        {
            Pages = [];
            Pages.CollectionChanged += Pages_CollectionChanged;
        }


        #region 页面列表

        public SuspendableObservableCollection<PageSwitcherItem> Pages { get; private set; }
        /// <summary>
        /// 集合变化时, 为集合项更新换页命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is PageSwitcherItem switcherItem)
                    {
                        setCommand(switcherItem);
                    }
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in Pages)
                {
                    setCommand(item);
                }
            }
        }
        private void setCommand(PageSwitcherItem? item)
        {
            if (item == null) return;
            item.SwitchPageCommand = GetSwitchCommand(item);
        }

        private readonly object switchCommandLocker = new();
        private bool switchCommandAction = false;
        private ICommand GetSwitchCommand(PageSwitcherItem item)
        {
            return new SampleCommand(
                _ => 
                {
                    lock (switchCommandLocker)
                    {
                        switchCommandAction = true;

                        CurrentSelected = item;
                        SwitchPageImpl(item);
                        OnSwitchPage?.Invoke(item);

                        switchCommandAction = false;
                    }
                },
                _ => true);
        }

        public void SetPages(IEnumerable<PageSwitcherItem> pages)
        {
            RunInUiThread(() =>
            {
                Pages.Clear();
                Pages.AddRange(pages, true);
            });
        }
        #endregion

        #region 当前选择

        public PageSwitcherItem? CurrentSelected
        { 
            get => currentPage; 
            set
            {
                if (!switchCommandAction)
                {
                    SwitchPageImpl(value);
                    OnSwitchPage?.Invoke(value);
                }
                currentPage = value;
                CurrentPageId = value?.Id;
                OnPropertyChanged();
            }
        }
        private PageSwitcherItem? currentPage;

        public long? CurrentPageId 
        {
            get => currentPageId; 
            private set
            {
                long? oldPageId = currentPageId;
                currentPageId = value;
                setPageShowing(oldPageId, false);
                setPageShowing(currentPageId, true);
                OnPropertyChanged();
            }
        }
        private long? currentPageId;

        private void setPageShowing(long? id, bool isShowing)
        {
            if (id != null)
            {
                var prev = Pages.FirstOrDefault(i => i.Id == id);
                if (prev != null)
                {
                    prev.PageShowing = isShowing;
                }
            }
        }


        #endregion

        #region 操作
        public void ClearShowing()
        {
            CurrentSelected = null;
        }
        #endregion

        #region 换页
        /// <summary>
        /// 换页的具体实现, 传入参数为准备切换的目标页面对应的集合项, 如果准备清空显示页面, 可能会传入 null 值
        /// </summary>
        public required Action<PageSwitcherItem?> SwitchPageImpl { get; set; }


        /// <summary>
        /// 将一个操作放在UI线程中运行, 此操作在锁住处理线程的状态下执行, 会造成处理线程阻塞
        /// </summary>
        public required Action<Action> RunInUiThread { get; init; }
        #endregion

        #region 属性变更

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 事件
        public event Action<PageSwitcherItem?>? OnSwitchPage;
        #endregion
    }

    public class PageSwitcherItem : INotifyPropertyChanged
    {
        #region 基础信息

        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Brief { get; set; } = string.Empty;

        #endregion

        #region 显示状态
        public bool PageShowing { get; internal set; }

        /// <summary>
        /// 在 Debug 模式下可见
        /// </summary>
        public bool DebugVisible { get; set; } = true;
        /// <summary>
        /// 在 Release 模式下可见
        /// </summary>
        public bool ReleaseVisible { get; set; } = true;
        #endregion

        #region 换页指令
        public ICommand? SwitchPageCommand { get; internal set; }
        #endregion

        #region 选项 UI 
        /// <summary>
        /// 自定义子项控件, 如果为null, 则使用默认的控件
        /// </summary>
        public ControlTemplate? CustomItemTemplate { get; set; }
        #endregion

        #region 属性变更

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class PageSwictherItemWithPage : PageSwitcherItem
    {
        public required System.Windows.Controls.Page Page { get; set; } 

    }
    public class PageSwictherItemWithPageType : PageSwitcherItem
    {
        /// <summary>
        /// 页面类型
        /// </summary>
        public required Type PageType { get; set; }
        /// <summary>
        /// 将类型实例化为页面的方法
        /// </summary>
        public required Func<Type, System.Windows.Controls.Page> InstantiateFunc { get; set; }

        public System.Windows.Controls.Page Page 
        {
            get
            {
                if (page == null)
                {
                    lock (instantiateLocker)
                    {
                        page ??= InstantiateFunc(PageType);
                    }
                }
                return page;
            }
        }
        private readonly object instantiateLocker = new();
        private System.Windows.Controls.Page? page;
    }
}
