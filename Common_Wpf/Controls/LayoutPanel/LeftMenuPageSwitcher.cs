using Common_Util.Data.Wrapped;
using Common_Util.Log;
using Common_Util.Module.Command;
using Common_Wpf.CommonViewModel;
using Common_Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common_Wpf.Controls.LayoutPanel
{
    /// <summary>
    /// 左侧有一个可以用来切换页面的菜单的页面切换器
    /// <para>内容为默认显示页面内容</para>
    /// </summary>
    public class LeftMenuPageSwitcher : MbContentControl01
    {
        static LeftMenuPageSwitcher()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LeftMenuPageSwitcher),
                new FrameworkPropertyMetadata(typeof(LeftMenuPageSwitcher)));
        }

        public Guid Id { get; } = Guid.NewGuid();

        public LeftMenuPageSwitcher()
        {
            ViewModel = new()
            {
                RunInUiThread = Dispatcher.Invoke,
                SwitchPageImpl = SwitchPageImpl,
            };
            ViewModel.OnSwitchPage += (item) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (CustomMenuTemplate == null)
                    {
                        var listBox = this.GetFirstChildObject<ListBox>("DefaultMenuListBox");
                        if (listBox != null)
                        {
                            listBox.Focus();
                        }
                    }
                });
            };
            Pages = [];
            Pages.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is PageSwictherItemWithPageType itemWithPageType)
                        {
                            itemWithPageType.InstantiateFunc = (type) =>
                            {
                                if (!type.IsAssignableTo(typeof(Page)))
                                {
                                    throw new InvalidOperationException($"类型不是 {typeof(Page)} 的子类, 类型: {type}");
                                }
                                return Dispatcher.Invoke(() =>
                                {
                                    var obj = Activator.CreateInstance(type) ?? $"实例化失败! 类型: {type}";
                                    return (Page) obj;
                                });
                            };
                        }
                    }
                }

                ViewModel.SetPages(Pages.Where(
#if DEBUG
                    i => i.DebugVisible
#else
                    i => i.ReleaseVisible
#endif
                    ));
            };
            
        }

        private void SwitchPageImpl(PageSwitcherItem? item)
        {
            Page? page = item switch
            {
                PageSwictherItemWithPage iWithPage => iWithPage.Page,
                PageSwictherItemWithPageType iWithPageType => iWithPageType.Page,
                null => null,
                _ => throw new NotImplementedException($"未实现从类型 {item.GetType()} 取得页面"),
            };
            CurrentPage = page;
        }

        public PageSwitcher ViewModel { get; private set; }

        #region 配置



        public SuspendableObservableCollection<PageSwitcherItem> Pages
        {
            get { return (SuspendableObservableCollection<PageSwitcherItem>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Pages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PagesProperty =
            DependencyProperty.Register(
                "Pages",
                typeof(SuspendableObservableCollection<PageSwitcherItem>),
                typeof(LeftMenuPageSwitcher),
                new PropertyMetadata(null));



        #endregion

        #region 当前页面



        public Page? CurrentPage
        {
            get { return (Page?)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(
                "CurrentPage", 
                typeof(Page), 
                typeof(LeftMenuPageSwitcher),
                new PropertyMetadata(null) 
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is LeftMenuPageSwitcher switcher)
                        {
                            if (e.NewValue is Page page)
                            {

                            }
                        }
                    }
                });




        #endregion

        #region 日志



        /// <summary>
        /// 用于测试的日志输出器
        /// </summary>
        public ILevelLogger? TestLogger
        {
            get { return (ILevelLogger?)GetValue(TestLoggerProperty); }
            set { SetValue(TestLoggerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TestLogger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestLoggerProperty =
            DependencyProperty.Register("TestLogger", 
                typeof(ILevelLogger), 
                typeof(LeftMenuPageSwitcher), 
                new PropertyMetadata(null));



        #endregion

        #region 命令
        public ICommand ClearSelectedPageCommand => new SampleCommand(_ => ViewModel.ClearShowing(), _ => true);
        #endregion

        #region 模板



        public ControlTemplate? CustomMenuTemplate
        {
            get { return (ControlTemplate)GetValue(CustomMenuTemplateProperty); }
            set { SetValue(CustomMenuTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomMenuControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomMenuTemplateProperty =
            DependencyProperty.Register(
                "CustomMenuTemplate",
                typeof(ControlTemplate), 
                typeof(LeftMenuPageSwitcher), 
                new PropertyMetadata(null)
                {
                });


        public ControlTemplate? CustomPageContainerTemplate
        {
            get { return (ControlTemplate)GetValue(CustomPageContainerTemplateProperty); }
            set { SetValue(CustomPageContainerTemplateProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CustomPageContainerTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomPageContainerTemplateProperty =
            DependencyProperty.Register(
                "CustomPageContainerTemplate",
                typeof(ControlTemplate),
                typeof(LeftMenuPageSwitcher),
                new PropertyMetadata(null)
                {
                });


        #endregion

        #region 默认模板的配置

        /// <summary>
        /// 默认样式下, 菜单项的最大宽度. 默认值为正无穷
        /// </summary>
        public double DefaultMenuItemMaxWidth
        {
            get { return (double)GetValue(DefaultMenuItemMaxWidthProperty); }
            set { SetValue(DefaultMenuItemMaxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultMenuItemMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultMenuItemMaxWidthProperty =
            DependencyProperty.Register("DefaultMenuItemMaxWidth", typeof(double), typeof(LeftMenuPageSwitcher), new PropertyMetadata(double.PositiveInfinity));


        #endregion
    }
}
