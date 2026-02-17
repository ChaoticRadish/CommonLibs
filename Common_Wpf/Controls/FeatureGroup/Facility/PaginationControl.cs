using Common_Util.Data.Structure.Pair;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Common_Wpf.Controls.FeatureGroup
{
    [TemplatePart(Name = "PART_PageNumberButtonArea", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_PageNumberButtonTemplate", Type = typeof(FrameworkElement))]
    public class PaginationControl : MbContentControl01
    {
        static PaginationControl() 
        {
            
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PaginationControl),
                new FrameworkPropertyMetadata(typeof(PaginationControl)));
        }

        public PaginationControl()
        {
            InternalViewModel = new() { Parent = this };
            InternalViewModel.PropertyChanged += InternalViewModel_PropertyChanged;
        }

        #region UI 控件
        private FrameworkElement? pageNumberButtonArea;
        private FrameworkElement? pageNumberButtonTemplate;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (pageNumberButtonArea != null)
            {
                pageNumberButtonArea.SizeChanged -= PageNumberButtonArea_SizeChanged;
            }
            if (pageNumberButtonTemplate != null)
            {
            }

            pageNumberButtonArea = GetTemplateChild("PART_PageNumberButtonArea") as FrameworkElement;
            pageNumberButtonTemplate = GetTemplateChild("PART_PageNumberButtonTemplate") as FrameworkElement;

            if (pageNumberButtonArea != null)
            {
                pageNumberButtonArea.SizeChanged += PageNumberButtonArea_SizeChanged;
            }
            if (pageNumberButtonTemplate != null)
            {
            }

            UpdatePageNumberButtonAreaActualPixelWidth();
        }

        private void PageNumberButtonArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize != e.PreviousSize)
                UpdatePageNumberButtonAreaActualPixelWidth();
        }
        #endregion

        #region ViewModel
        public PaginationControlViewModel InternalViewModel { get; private init; }
        private void InternalViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PaginationControlViewModel.TotalPages):
                    SetCurrentValue(TotalPageProperty, InternalViewModel.TotalPages);
                    break;
                case nameof(PaginationControlViewModel.PageCode):
                    SetCurrentValue(PageCodeProperty, InternalViewModel.PageCode);
                    break;
            }
        }

        public PaginationControlPageButtonViewModel PageNumberButtonTemplateViewModel
        { 
            get => pageNumberButtonTemplateViewModel;
            private set
            {
                pageNumberButtonTemplateViewModel = value;
                OnPropertyChanged();
            }
        }
        private PaginationControlPageButtonViewModel pageNumberButtonTemplateViewModel = new () { MarkEllipsis = true };
        #endregion

        #region UI 数据
        private const double CalculatePageNumberButtonWidthDefaultValue = 50;
        private const double CalculatePageNumberButtonHorizontalGapDefaultValue = 0;

        /// <summary>
        /// 计算指定 ViewModel 下, 按钮的宽度预期是多少
        /// </summary>
        /// <param name="buttonViewModel"></param>
        /// <returns></returns>
        internal double CalculatePageNumberButtonWidth(PaginationControlPageButtonViewModel buttonViewModel)
        {
            if (pageNumberButtonTemplate == null) return CalculatePageNumberButtonWidthDefaultValue;

            PageNumberButtonTemplateViewModel = buttonViewModel;
            pageNumberButtonTemplate.ApplyTemplate();
            pageNumberButtonTemplate.UpdateLayout();
            pageNumberButtonTemplate.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            // pageNumberButtonTemplate.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            return pageNumberButtonTemplate.ActualWidth;
        }
        /// <summary>
        /// 取得按钮的水平间距预期值
        /// </summary>
        /// <returns></returns>
        internal double CalculatePageNumberButtonHorizontalGap()
        {
            if (pageNumberButtonTemplate == null) return CalculatePageNumberButtonHorizontalGapDefaultValue;

            var margin = pageNumberButtonTemplate.Margin;
            return margin.Left + margin.Right;
        }
        #endregion

        #region UI 设置

        #region 常量
        private readonly static GridLength ZeroGridLength = new GridLength(0);
        #endregion

        #region 初始值
        private readonly static GridLength initValue_RefreshButtonAreaWidth = GridLength.Auto;
        private readonly static GridLength initValue_LeftFixedAreaWidth = GridLength.Auto;
        private readonly static GridLength initValue_PageNumberButtonAreaWidth = new GridLength(1, GridUnitType.Star);
        private readonly static GridLength initValue_RightFixedAreaWidth = GridLength.Auto;
        private readonly static GridLength initValue_PageCountAreaWidth = GridLength.Auto;
        private readonly static GridLength initValue_PageJumpAreaWidth = GridLength.Auto;

        #endregion

        #region 动态可选项数量
        public void UpdatePageNumberButtonCount()
        {
            InternalViewModel.TriggerUpdatePageNumbeOptions();
        }
        #endregion

        /// <summary>
        /// 是否显示刷新按钮
        /// </summary>
        public bool ShowRefreshButton
        {
            get { return (bool)GetValue(ShowRefreshButtonProperty); }
            set { SetValue(ShowRefreshButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowRefreshButtonProperty =
            DependencyProperty.Register(
                nameof(ShowRefreshButton), typeof(bool), 
                typeof(PaginationControl), new PropertyMetadata(true, OnRefreshButtonAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 刷新按钮区域的宽度
        /// </summary>
        public GridLength RefreshButtonAreaWidth
        {
            get { return (GridLength)GetValue(RefreshButtonAreaWidthProperty); }
            set { SetValue(RefreshButtonAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty RefreshButtonAreaWidthProperty =
            DependencyProperty.Register(
                nameof(RefreshButtonAreaWidth), typeof(GridLength), 
                typeof(PaginationControl), new PropertyMetadata(initValue_RefreshButtonAreaWidth, OnRefreshButtonAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 刷新按钮区域的实际宽度
        /// </summary>
        public GridLength RefreshButtonAreaActualWidth
        {
            get => (GridLength)GetValue(RefreshButtonAreaActualWidthProperty);
            private set => SetValue(RefreshButtonAreaActualWidthProperty, value);
        }
        public static readonly DependencyProperty RefreshButtonAreaActualWidthProperty =
            DependencyProperty.Register(
                nameof(RefreshButtonAreaActualWidth), typeof(GridLength),
                typeof(PaginationControl),
                new PropertyMetadata(initValue_RefreshButtonAreaWidth,
                    OnRefreshButtonAreaActualWidthPropertyChanged,
                    CoerceRefreshButtonAreaActualWidth));

        private static void OnRefreshButtonAreaWidthRelatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(RefreshButtonAreaActualWidthProperty);
        }
        private static void OnRefreshButtonAreaActualWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;
            if (control.ShowRefreshButton)
                control.SetValue(RefreshButtonAreaWidthProperty, e.NewValue);
        }
        private static object CoerceRefreshButtonAreaActualWidth(DependencyObject d, object baseValue)
        {
            if (d is not PaginationControl control) return ZeroGridLength;
            if (control.ShowRefreshButton) return control.RefreshButtonAreaWidth;
            else return ZeroGridLength;
        }


        /// <summary>
        /// 是否显示页码按钮
        /// </summary>
        public bool ShowPageNumberButton
        {
            get { return (bool)GetValue(ShowPageNumberButtonProperty); }
            set { SetValue(ShowPageNumberButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowPageNumberButtonProperty =
            DependencyProperty.Register(
                nameof(ShowPageNumberButton), typeof(bool),
                typeof(PaginationControl), new PropertyMetadata(true, OnPageNumberButtonAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码按钮区域的宽度
        /// </summary>
        public GridLength PageNumberButtonAreaWidth
        {
            get { return (GridLength)GetValue(PageNumberButtonAreaWidthProperty); }
            set { SetValue(PageNumberButtonAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty PageNumberButtonAreaWidthProperty =
            DependencyProperty.Register(
                nameof(PageNumberButtonAreaWidth), typeof(GridLength),
                typeof(PaginationControl), new PropertyMetadata(initValue_PageNumberButtonAreaWidth, OnPageNumberButtonAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码按钮区域的实际宽度
        /// </summary>
        public GridLength PageNumberButtonAreaActualWidth
        {
            get => (GridLength)GetValue(PageNumberButtonAreaActualWidthProperty);
            private set => SetValue(PageNumberButtonAreaActualWidthProperty, value);
        }
        public static readonly DependencyProperty PageNumberButtonAreaActualWidthProperty =
            DependencyProperty.Register(
                nameof(PageNumberButtonAreaActualWidth), typeof(GridLength),
                typeof(PaginationControl),
                new PropertyMetadata(initValue_PageNumberButtonAreaWidth,
                    OnPageNumberButtonAreaActualWidthPropertyChanged,
                    CoercePageNumberButtonAreaActualWidth));
        /// <summary>
        /// 页码按钮区域的实际像素宽度
        /// </summary>
        public double PageNumberButtonAreaActualPixelWidth
        {
            get => (double)GetValue(PageNumberButtonAreaActualPixelWidthProperty);
            private set => SetValue(PageNumberButtonAreaActualPixelWidthProperty, value);
        }
        public static readonly DependencyProperty PageNumberButtonAreaActualPixelWidthProperty =
            DependencyProperty.Register(
                nameof(PageNumberButtonAreaActualPixelWidth), typeof(double),
                typeof(PaginationControl),
                new PropertyMetadata((double)0));
        private static void OnPageNumberButtonAreaWidthRelatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(PageNumberButtonAreaActualWidthProperty);
        }
        private static void OnPageNumberButtonAreaActualWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;
            if (control.ShowPageNumberButton)
                control.SetValue(PageNumberButtonAreaWidthProperty, e.NewValue);
        }
        private static object CoercePageNumberButtonAreaActualWidth(DependencyObject d, object baseValue)
        {
            if (d is not PaginationControl control) return ZeroGridLength;
            if (control.ShowPageNumberButton) return control.PageNumberButtonAreaWidth;
            else return ZeroGridLength;
        }
        private void UpdatePageNumberButtonAreaActualPixelWidth() 
        {
            if (pageNumberButtonArea == null) return;

            PageNumberButtonAreaActualPixelWidth = pageNumberButtonArea.ActualWidth;
            UpdatePageNumberButtonCount();
        }

        /// <summary>
        /// 左侧固定区域的宽度
        /// </summary>
        public GridLength LeftFixedAreaWidth
        {
            get { return (GridLength)GetValue(LeftFixedAreaWidthProperty); }
            set { SetValue(LeftFixedAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty LeftFixedAreaWidthProperty =
            DependencyProperty.Register(
                nameof(LeftFixedAreaWidth), typeof(GridLength),
                typeof(PaginationControl), new PropertyMetadata(initValue_LeftFixedAreaWidth));
        /// <summary>
        /// 右侧固定区域的宽度
        /// </summary>
        public GridLength RightFixedAreaWidth
        {
            get { return (GridLength)GetValue(RightFixedAreaWidthProperty); }
            set { SetValue(RightFixedAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty RightFixedAreaWidthProperty =
            DependencyProperty.Register(
                nameof(RightFixedAreaWidth), typeof(GridLength),
                typeof(PaginationControl), new PropertyMetadata(initValue_RightFixedAreaWidth));



        /// <summary>
        /// 是否显示页码数量区域
        /// </summary>
        public bool ShowPageCountArea
        {
            get { return (bool)GetValue(ShowPageCountAreaProperty); }
            set { SetValue(ShowPageCountAreaProperty, value); }
        }
        public static readonly DependencyProperty ShowPageCountAreaProperty =
            DependencyProperty.Register(
                nameof(ShowPageCountArea), typeof(bool),
                typeof(PaginationControl), new PropertyMetadata(true, OnPageCountAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码数量区域的宽度
        /// </summary>
        public GridLength PageCountAreaWidth
        {
            get { return (GridLength)GetValue(PageCountAreaWidthProperty); }
            set { SetValue(PageCountAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty PageCountAreaWidthProperty =
            DependencyProperty.Register(
                nameof(PageCountAreaWidth), typeof(GridLength),
                typeof(PaginationControl), new PropertyMetadata(initValue_PageCountAreaWidth, OnPageCountAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码数量区域的实际宽度
        /// </summary>
        public GridLength PageCountAreaActualWidth
        {
            get => (GridLength)GetValue(PageCountAreaActualWidthProperty);
            private set => SetValue(PageCountAreaActualWidthProperty, value);
        }
        public static readonly DependencyProperty PageCountAreaActualWidthProperty =
            DependencyProperty.Register(
                nameof(PageCountAreaActualWidth), typeof(GridLength),
                typeof(PaginationControl),
                new PropertyMetadata(initValue_PageCountAreaWidth,
                    OnPageCountAreaActualWidthPropertyChanged,
                    CoercePageCountAreaActualWidth));
        private static void OnPageCountAreaWidthRelatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(PageCountAreaActualWidthProperty);
        }
        private static void OnPageCountAreaActualWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;
            if (control.ShowPageNumberButton)
                control.SetValue(PageCountAreaWidthProperty, e.NewValue);
        }
        private static object CoercePageCountAreaActualWidth(DependencyObject d, object baseValue)
        {
            if (d is not PaginationControl control) return ZeroGridLength;
            if (control.ShowPageNumberButton) return control.PageCountAreaWidth;
            else return ZeroGridLength;
        }



        /// <summary>
        /// 是否显示页码跳转区域
        /// </summary>
        public bool ShowPageJumpArea
        {
            get { return (bool)GetValue(ShowPageJumpAreaProperty); }
            set { SetValue(ShowPageJumpAreaProperty, value); }
        }
        public static readonly DependencyProperty ShowPageJumpAreaProperty =
            DependencyProperty.Register(
                nameof(ShowPageJumpArea), typeof(bool),
                typeof(PaginationControl), new PropertyMetadata(true, OnPageJumpAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码跳转区域的宽度
        /// </summary>
        public GridLength PageJumpAreaWidth
        {
            get { return (GridLength)GetValue(PageJumpAreaWidthProperty); }
            set { SetValue(PageJumpAreaWidthProperty, value); }
        }
        public static readonly DependencyProperty PageJumpAreaWidthProperty =
            DependencyProperty.Register(
                nameof(PageJumpAreaWidth), typeof(GridLength),
                typeof(PaginationControl), new PropertyMetadata(initValue_PageJumpAreaWidth, OnPageJumpAreaWidthRelatePropertyChanged));
        /// <summary>
        /// 页码跳转区域的实际宽度
        /// </summary>
        public GridLength PageJumpAreaActualWidth
        {
            get => (GridLength)GetValue(PageJumpAreaActualWidthProperty);
            private set => SetValue(PageJumpAreaActualWidthProperty, value);
        }
        public static readonly DependencyProperty PageJumpAreaActualWidthProperty =
            DependencyProperty.Register(
                nameof(PageJumpAreaActualWidth), typeof(GridLength),
                typeof(PaginationControl),
                new PropertyMetadata(initValue_PageJumpAreaWidth,
                    OnPageJumpAreaActualWidthPropertyChanged,
                    CoercePageJumpAreaActualWidth));
        private static void OnPageJumpAreaWidthRelatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(PageJumpAreaActualWidthProperty);
        }
        private static void OnPageJumpAreaActualWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;
            if (control.ShowPageNumberButton)
                control.SetValue(PageJumpAreaWidthProperty, e.NewValue);
        }
        private static object CoercePageJumpAreaActualWidth(DependencyObject d, object baseValue)
        {
            if (d is not PaginationControl control) return ZeroGridLength;
            if (control.ShowPageNumberButton) return control.PageJumpAreaWidth;
            else return ZeroGridLength;
        }


        #endregion

        #region 指令与事件


        /// <summary>
        /// 页面内容加载指令, 参数类型为 <see cref="IObjectChanged{T}"/> (T 是 <see langword="int"/>, 表示页码)
        /// </summary>
        public ICommand ContentLoadCommand
        {
            get { return (ICommand)GetValue(ContentLoadCommandProperty); }
            set { SetValue(ContentLoadCommandProperty, value); }
        }
        public static readonly DependencyProperty ContentLoadCommandProperty =
            DependencyProperty.Register(nameof(ContentLoadCommand), typeof(ICommand), typeof(PaginationControl), new PropertyMetadata(null));


        /// <summary>
        /// 页面变更事件
        /// </summary>
        public event EventHandler<IObjectChanged<int>>? OnPageChanged;
        


        internal void TriggerPageChange(int oldPageCode, int newPageCode)
        {
            Dispatcher.InvokeAsync(() => OnPageChangeAsync(oldPageCode, newPageCode));
        }
        private readonly SemaphoreSlim pageChangeLocker = new(1, 1);
        private int previousPageCode = -1;
        private async Task OnPageChangeAsync(int oldPageCode, int newPageCode)
        {
            var sw = Stopwatch.StartNew();
            await pageChangeLocker.WaitAsync();
            try
            {
                if (sw.ElapsedMilliseconds > 0 && previousPageCode == newPageCode)
                {
                    // 发生了等待, 且上一次执行的新页面与当前的新页面相同
                    return;
                }

                IObjectChanged<int> changedInfo = new ObjectChanged<int>(oldPageCode, newPageCode);
                SetValue(ContentLoadingPropertyKey, true);
                try
                {
                    var command = ContentLoadCommand;
                    if (command != null)
                    {
                        var commandResult = Common_Util.Module.Command.AsyncCommandHelper.TryGetExecuteAsyncTask(command, changedInfo);
                        if (commandResult == null)
                        {
                            ContentLoadCommand.Execute(changedInfo);
                        }
                        else
                        {
                            dynamic task = commandResult;
                            await task;
                        }
                    }
                }
                finally
                {
                    SetValue(ContentLoadingPropertyKey, false);
                }

                if (OnPageChanged != null)
                {
                    _ = Dispatcher.InvokeAsync(() => OnPageChanged?.Invoke(this, changedInfo));
                }
            }
            finally
            {
                previousPageCode = newPageCode;
                pageChangeLocker.Release();
            }
        }


        #endregion

        #region 状态


        /// <summary>
        /// 当前是否正在加载页面内容
        /// </summary>
        public bool ContentLoading
        {
            get { return (bool)GetValue(ContentLoadingProperty); }
        }
        private static readonly DependencyPropertyKey ContentLoadingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContentLoading), typeof(bool), typeof(PaginationControl), 
                new PropertyMetadata(false));
        public static readonly DependencyProperty ContentLoadingProperty = ContentLoadingPropertyKey.DependencyProperty;


        #endregion

        #region 参数

        /// <summary>
        /// 总页面数量
        /// </summary>
        public int TotalPage
        {
            get { return (int)GetValue(TotalPageProperty); }
            set { SetValue(TotalPageProperty, value); }
        }
        public static readonly DependencyProperty TotalPageProperty =
            DependencyProperty.Register(
                nameof(TotalPage), typeof(int), 
                typeof(PaginationControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTotalPageChanged));
        private static void OnTotalPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;

            if (e.NewValue is int value)
                control.InternalViewModel.TotalPages = value;
        }


        /// <summary>
        /// 当前页码 (从 1 开始)
        /// </summary>
        public int PageCode
        {
            get { return (int)GetValue(PageCodeProperty); }
            set { SetValue(PageCodeProperty, value); }
        }
        public static readonly DependencyProperty PageCodeProperty =
            DependencyProperty.Register(
                nameof(PageCode), typeof(int), 
                typeof(PaginationControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageCodeChanged));
        private static void OnPageCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationControl control) return;

            if (e.NewValue is int value)
                control.InternalViewModel.PageCode = value;
        }



        #endregion
    }

    public class PaginationControlViewModel : INotifyPropertyChanged
    {
        public required PaginationControl Parent { get; init; }

        public PaginationControlViewModel()
        {
            FirstPageCommand = new Common_Util.Module.Command.SimpleCommand(OnFirstPage);
            PreviousPageCommand = new Common_Util.Module.Command.SimpleCommand(OnPreviousPage);
            NextPageCommand = new Common_Util.Module.Command.SimpleCommand(OnNextPage);
            LastPageCommand = new Common_Util.Module.Command.SimpleCommand(OnLastPage);
            RefreshCommand = new Common_Util.Module.Command.SimpleCommand(OnRefresh);
            GoToPageCommand = new Common_Util.Module.Command.SimpleCommand(OnGoToPage);

        }

        #region 属性值变化事件


        public event PropertyChangedEventHandler? PropertyChanged;

        private void TriggerPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region 参数
        /// <summary>
        /// 总页面数量
        /// </summary>
        public int TotalPages
        {
            get => totalPages;
            set
            {
                coreUpdate(PageCode, value, false, false);
            }
        }
        private int totalPages;

        /// <summary>
        /// 当前页码 (从 1 开始)
        /// </summary>
        public int PageCode
        {
            get => pageCode;
            set
            {
                coreUpdate(value, TotalPages, false, false);
            }
        }
        private int pageCode;


        #endregion

        #region 状态
        public bool HasPrevious { get => TotalPages > 0 && PageCode > 1; } 
        public bool HasNext { get => TotalPages > 0 && PageCode < TotalPages; }

        #endregion

        #region UI 输入
        public IReadOnlyList<PaginationControlPageButtonViewModel> PageNumberOptions
        {
            get => pageNumberOptions ?? [];
            private set
            {
                if (value is PaginationControlPageButtonViewModel[] arr)
                    pageNumberOptions = arr;
                else 
                    pageNumberOptions = value.ToArray();
                TriggerPropertyChanged();
            }
        }
        private PaginationControlPageButtonViewModel[]? pageNumberOptions;


        public int? PageJumpInput
        {
            get => pageJumpInput;
            set
            {
                pageJumpInput = value;
                TriggerPropertyChanged();
            }
        }
        private int? pageJumpInput;

        #endregion


        #region 操作
        public void TriggerUpdatePageNumbeOptions()
        {
            coreUpdate(PageCode, TotalPages, false, true);
        }
        public void UpdateCount(int pageCode, int totalPage)
        {
            coreUpdate(pageCode, totalPage, false, false);
        }

        private bool initedCoreUpdate = false;
        private readonly object coreUpdateLocker = new();
        private bool coreUpdating = false;
        private void coreUpdate(int pageCode, int totalPage, bool refresh, bool forceUpdateOptions)
        {
            if (coreUpdating) return;
            
            lock (coreUpdateLocker)
            {
                if (coreUpdating) return;
                coreUpdating = true;

                try 
                {
                    _coreUpdateBody(pageCode, totalPage, refresh, forceUpdateOptions);
                }
                finally
                {
                    coreUpdating = false;
                }
            }
        }
        private void _coreUpdateBody(int pageCode, int totalPage, bool refresh, bool forceUpdateOptions)
        {
            int oldTotalPage = this.totalPages;
            int oldPageCode = this.pageCode;
            // 钳制并记录新值
            int newTotalPage = totalPage >= 0 ? totalPage : 0;
            int newPageCode = pageCode < 0 ? (newTotalPage > 0 ? 1 : 0) : (pageCode > newTotalPage ? newTotalPage : pageCode);
            if (newTotalPage > 0 && newPageCode == 0) newPageCode = 1;

            if (!refresh && !forceUpdateOptions && initedCoreUpdate && (
                oldTotalPage == newTotalPage
                && oldPageCode == newPageCode
                )) return;

            double areaWidth = Parent.PageNumberButtonAreaActualPixelWidth;
            if (areaWidth <= 0)
            {
                // 无效的区域宽度
                _updatePageNumberOptionsWithCount(newPageCode, newTotalPage, 5);
            }
            else
            {
                _updatePageNumberOptionWithAreaSize(newPageCode, newTotalPage);
            }

            this.totalPages = newTotalPage;
            this.pageCode = newPageCode;

            bool totalPageChanged = newTotalPage != oldTotalPage;
            bool pageCodeChanged = newPageCode != oldPageCode;
            if (totalPageChanged) TriggerPropertyChanged(nameof(TotalPages));
            if (pageCodeChanged) TriggerPropertyChanged(nameof(PageCode));
            if (totalPageChanged || pageCodeChanged)
            {
                TriggerPropertyChanged(nameof(HasPrevious));
                TriggerPropertyChanged(nameof(HasNext));
            }
            if (refresh || totalPageChanged || pageCodeChanged || !initedCoreUpdate)
            {
                Parent.TriggerPageChange(oldPageCode, newPageCode);
            }

            initedCoreUpdate = true;
        }
        private void _updatePageNumberOptionsWithCount(int targetPageCode, int targetTotalPage, int targetCount)
        {
            if (targetCount <= 0 || targetPageCode <= 0 || targetTotalPage <= 0) 
            {
                PageNumberOptions = [];
                return;
            }

            List<PaginationControlPageButtonViewModel> options = [];

            int currentCode = targetPageCode;
            options.Add(new() { MarkEllipsis = false, PageCode = currentCode, IsSelected = true });
            int leftValue = currentCode;
            int rightValue = currentCode;
            bool nextLeft = false;
            for (int i = 1; i < targetCount; i++)
            {
                bool leftEnd = leftValue == 1;
                bool rightEnd = rightValue == targetTotalPage;
                if (leftEnd && rightEnd) break;

                if (nextLeft)
                {
                    if (leftEnd) goto AddRight;
                    else goto AddLeft;
                }
                else
                {
                    if (rightEnd) goto AddLeft;
                    else goto AddRight;
                }

            AddLeft:
                leftValue -= 1;
                options.Insert(0, new() { MarkEllipsis = false, PageCode = leftValue });
                goto AddDone;
            AddRight:
                rightValue += 1;
                options.Add(new() { MarkEllipsis = false, PageCode = rightValue });
                goto AddDone;
            AddDone:
                nextLeft = !nextLeft;

            }
            if (options.Count > 0)
            {
                if (options[0].PageCode > 1)
                    options[0].MarkEllipsis = true;
                if (options[^1].PageCode < targetTotalPage)
                    options[^1].MarkEllipsis = true;
            }
            PageNumberOptions = options;
        }
        private void _updatePageNumberOptionWithAreaSize(int targetPageCode, int targetTotalPage)
        {
            if (targetPageCode <= 0)
            {
                PageNumberOptions = [];
                return;
            }

            List<PaginationControlPageButtonViewModel> options = [];


            double areaWidth = Parent.PageNumberButtonAreaActualPixelWidth;
            double usingWidth = 0;
            double horizontalGap = Parent.CalculatePageNumberButtonHorizontalGap();

            PaginationControlPageButtonViewModel item;

            int currentCode = targetPageCode;
            item = new() { MarkEllipsis = false, PageCode = currentCode, IsSelected = true };
            double currentCodeWidth = Parent.CalculatePageNumberButtonWidth(item);
            if (currentCodeWidth > areaWidth)
            {
                PageNumberOptions = [];
                return;
            }
            usingWidth += currentCodeWidth;
            options.Add(item);
            int leftValue = currentCode;
            int rightValue = currentCode;
            bool nextLeft = false;

            bool addLeft;

            while (usingWidth < areaWidth)
            {
                bool leftEnd = leftValue == 1;
                bool rightEnd = rightValue == targetTotalPage;
                if (leftEnd && rightEnd) break;

                if (nextLeft)
                {
                    if (leftEnd) goto AddRight;
                    else goto AddLeft;
                }
                else
                {
                    if (rightEnd) goto AddLeft;
                    else goto AddRight;
                }

            AddLeft:
                leftValue -= 1;
                item = new() { MarkEllipsis = false, PageCode = leftValue };
                addLeft = true;
                goto CheckFull;
            AddRight:
                rightValue += 1;
                item = new() { MarkEllipsis = false, PageCode = rightValue };
                addLeft = false;
                goto CheckFull;
            CheckFull:
                double buttonWidth = Parent.CalculatePageNumberButtonWidth(item);
                double addWidth;
                if (options.Count == 0)
                    addWidth = buttonWidth;
                else
                    addWidth = buttonWidth + horizontalGap;
                if (usingWidth + addWidth > areaWidth) break;   // 预期将超出区域, 跳出循环

                if (addLeft)
                    options.Insert(0, item);
                else
                    options.Add(item);

                usingWidth += addWidth;

                nextLeft = !nextLeft;
            }



            if (options.Count > 0)
            {
                if (options[0].PageCode > 1)
                    options[0].MarkEllipsis = true;
                if (options[^1].PageCode < targetTotalPage)
                    options[^1].MarkEllipsis = true;
            }
            PageNumberOptions = options;
        }

        #endregion

        #region 指令
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand GoToPageCommand { get; }


        private void OnFirstPage() => PageCode = 1;
        private void OnPreviousPage() => PageCode--;
        private void OnNextPage() => PageCode++;
        private void OnLastPage() => PageCode = TotalPages;
        private void OnRefresh() => coreUpdate(PageCode, TotalPages, true, false);
        private void OnGoToPage(object? arg)
        {
            if (arg is int i) PageCode = i;
            else if (arg is string str && int.TryParse(str, out var result)) PageCode = result;
        }
        #endregion

    }
    public class PaginationControlPageButtonViewModel : INotifyPropertyChanged
    {
        #region 属性值变化事件

        public event PropertyChangedEventHandler? PropertyChanged;

        private void TriggerPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// 是否用于标记省略
        /// </summary>
        public bool MarkEllipsis
        {
            get => markEllipsis;
            set
            {
                markEllipsis = value;
                TriggerPropertyChanged();
            }
        }
        private bool markEllipsis;

        /// <summary>
        /// 对应页码
        /// </summary>
        public int PageCode
        {
            get => pageCode;
            set
            {
                pageCode = value;
                TriggerPropertyChanged();
            }
        }
        private int pageCode;

        /// <summary>
        /// 当前是否应处于显示状态
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                TriggerPropertyChanged();
            }
        }
        private bool isSelected;

    }
}
