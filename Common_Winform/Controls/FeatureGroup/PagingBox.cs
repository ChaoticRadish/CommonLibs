using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Controls.FeatureGroup
{
    /// <summary>
    /// 分页控件
    /// </summary>
    [DefaultEvent(nameof(OnPageIndexChanged))]
    public partial class PagingBox : UserControl
    {
        public PagingBox()
        {
            InitializeComponent();

            _synchronizationContext = SynchronizationContext.Current;
        }

        #region 上下文
        private readonly SynchronizationContext? _synchronizationContext;
        private void UiSyncPost(Action action)
        {
            if (_synchronizationContext == null) action();
            else _synchronizationContext.Post(_ => action(), null);
        }
        #endregion

        #region 属性

        [Category("CVII_自定义_参数"), DisplayName("是否只读")]
        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                if (readOnly != value)
                {
                    readOnly = value;
                    // 变更按钮状态

                    FirstButton.Enabled = !readOnly;
                    PrevButton.Enabled = !readOnly;
                    NextButton.Enabled = !readOnly;
                    LastButton.Enabled = !readOnly;
                    JumpButton.Enabled = !readOnly;
                    PageInput.ReadOnly = readOnly;

                    foreach (PageButton button in pageButtons)
                    {
                        button.Button.Enabled = !readOnly;
                    }
                }
            }
        }
        private bool readOnly = false;

        /// <summary>
        /// 页面容量
        /// </summary>
        [Category("CVII_自定义_参数"), DisplayName("页面容量")]
        public int PageSize
        {
            get => pageSize;
            set
            {
                pageSize = value;
            }
        }
        private int pageSize = 20;

        /// <summary>
        /// 总数量
        /// </summary>
        [Category("CVII_自定义_参数"), DisplayName("当前总数量")]
        public int TotalCount
        {
            get => totalCount;
            set
            {
                totalCount = value;
                UpdatePageButtons();
                if (CurrentIndex > TotalPage)
                {
                    SetShowingCurrentIndex(TotalPage);
                }
                else if (currentIndex < 1)
                {
                    SetShowingCurrentIndex(1);
                }
            }
        }
        private int totalCount = 100;

        /// <summary>
        /// 当前页码, 赋值时就会触发
        /// </summary>
        [Category("CVII_自定义_参数"), DisplayName("当前页码")]
        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                int old = currentIndex;
                currentIndex = value;
                if (currentIndex <= 0)
                {
                    currentIndex = 1;
                }
                if (currentIndex > TotalPage)
                {
                    currentIndex = TotalPage;
                }


                UpdatePageButtons();

                try
                {
                    if (!settingShowingCurrentIndex)
                    {
                        UiSyncPost(() => OnPageIndexChanged?.Invoke(currentIndex, pageSize));
                    }
                    UpdatePageInputUi(currentIndex);
                }
                catch
                {
                    currentIndex = old;
                    UpdatePageInputUi(currentIndex);

                    throw;
                }
            }
        }
        private int currentIndex;

        [Category("CVII_自定义_参数"), DisplayName("总页数")]
        public int TotalPage
        {
            get => totalPage;
            set
            {
                totalPage = value;
                TotalPageShower.Text = totalPage.ToString();
                PageInput.MaxValue = totalPage;
            }
        }
        private int totalPage;


        [Category("CVII_自定义_参数"), DisplayName("是否使用刷新按钮")]
        public bool UseRefreshButton
        {
            get => useRefreshButton;
            set
            {
                useRefreshButton = value;
                if (!IsHandleCreated)
                {
                    return;
                }
                UpdateState_UseRefreshButton();
            }
        }
        private bool useRefreshButton;
        #endregion

        #region 重载
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitSwitchUseRefreshButtonNeedData();
            UpdateState_UseRefreshButton();
        }
        #endregion

        #region 事件
        public delegate void OnPageIndexChangedDelegate(int pageIndex, int pageSize);
        /// <summary>
        /// 页码更新事件
        /// </summary>
        [Category("CVII_自定义"), DisplayName("页码变更事件")]
        public event OnPageIndexChangedDelegate? OnPageIndexChanged;
        #endregion

        #region 按钮

        private void FirstButton_Click(object sender, EventArgs e)
        {
            CurrentIndex = 1;
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            CurrentIndex--;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            CurrentIndex++;
        }

        private void LastButton_Click(object sender, EventArgs e)
        {
            CurrentIndex = TotalPage;
        }

        private void JumpButton_Click(object sender, EventArgs e)
        {
            CurrentIndex = PageInput.Value;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            CurrentIndex = currentIndex;
        }

        private void PageInput_ValueChanged(Common_Winform.Controls.ValueBox.IntegerBox inputBox, int oldValue, int newValue)
        {
            if (!updatingPageInputUi)
            {
                CurrentIndex = newValue;
            }
        }
        #endregion

        #region 页码按钮
        class PageButton
        {
            public PageButton(Button baseButton)
            {
                Button = baseButton;
            }

            public Button Button { get; set; }

            public int PageIndex { get; set; }

            public bool IsCurrentPageIndex
            {
                get => isCurrentPageIndex;
                set
                {
                    isCurrentPageIndex = value;
                    if (isCurrentPageIndex)
                    {
                        Button.BackColor = Color.White;
                    }
                    else
                    {
                        Button.BackColor = Color.FromArgb(225, 225, 225);
                    }
                }
            }
            private bool isCurrentPageIndex;

            public bool Visible { get => Button.Visible; set => Button.Visible = value; }
        }
        private int PageButtonWidth = 40;
        private int PageButtonGap = 3;

        private List<PageButton> pageButtons = new List<PageButton>();


        private void UpdatePageButtons()
        {
            SuspendLayout();

            TotalPage = (TotalCount - 1) / PageSize + 1;

            int canShowButtonCount = (PageButtonArea.Width - PageButtonGap) / (PageButtonGap + PageButtonWidth);
            if (canShowButtonCount % 2 == 0)
            {
                canShowButtonCount -= 1;
            }
            if (canShowButtonCount < 3)
            {
                canShowButtonCount = 3;
            }
            int needShowStart = CurrentIndex - canShowButtonCount / 2;
            int startOffset = needShowStart <= 0 ? (1 - needShowStart) : 0;
            int needShowEnd = CurrentIndex + canShowButtonCount / 2 + startOffset;
            if (needShowEnd > TotalPage)
            {
                needShowEnd = TotalPage;
            }
            int needShowCount = needShowEnd - (needShowStart + startOffset) + 1;

            while (pageButtons.Count < needShowCount)
            {
                PageButton button = new PageButton(new Button()
                {
                    Visible = false,
                    Width = PageButtonWidth,
                    Height = PageButtonInnerArea.Height,
                });
                button.Button.Click += (sender, arg) =>
                {
                    if (button.PageIndex > 0)
                    {
                        CurrentIndex = button.PageIndex;
                    }
                };
                button.IsCurrentPageIndex = false;
                pageButtons.Add(button);
                PageButtonInnerArea.Controls.Add(button.Button);
            }
            int totalWidth = 0;
            for (int i = 0; i < pageButtons.Count; i++)
            {
                PageButton button = pageButtons[i];

                button.PageIndex = i + needShowStart + startOffset;
                button.Button.Text = (i + needShowStart + startOffset).ToString();

                if ((i == 0 && button.PageIndex != 1 && startOffset == 0)
                    || (button.PageIndex == needShowEnd && needShowEnd != TotalPage))
                {
                    button.PageIndex = -1;
                    button.Button.Text = "...";
                }

                button.Visible = i < needShowCount;
                if (!button.Visible) continue;

                button.IsCurrentPageIndex = i + needShowStart + startOffset == CurrentIndex;

                button.Button.Size = new Size(PageButtonWidth, PageButtonInnerArea.Height);
                button.Button.Location = new Point(totalWidth, 0);

                totalWidth += button.Button.Width + PageButtonGap;
            }
            PageButtonInnerArea.Width = totalWidth;
            PageButtonInnerArea.Location = new Point((PageButtonArea.Width - totalWidth) / 2, PageButtonInnerArea.Location.Y);

            ResumeLayout();
        }


        #endregion

        #region 重载
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!IsHandleCreated) return;
            UpdatePageButtons();
        }
        #endregion

        #region 是否使用刷新按钮
        private int _refreshButtonAreaInitWidth;
        private int _alwaysShowAreaInitWidth;
        /// <summary>
        /// 初始化切换使用刷新按钮时所需的数据
        /// </summary>
        private void InitSwitchUseRefreshButtonNeedData()
        {
            _refreshButtonAreaInitWidth = Area_RefreshButton.Width;
            _alwaysShowAreaInitWidth = Area_AlwayShow.Width;
        }
        private void UpdateState_UseRefreshButton()
        {
            if (!IsHandleCreated) return;

            RefreshButton.Enabled = useRefreshButton;
            Area_RefreshButton.Visible = useRefreshButton;
            if (useRefreshButton)
            {
                Area_Right.Width = _refreshButtonAreaInitWidth + _alwaysShowAreaInitWidth;
            }
            else
            {
                Area_Right.Width = _alwaysShowAreaInitWidth;
            }
        }
        #endregion

        #region UI 更新
        /// <summary>
        /// 仅更新页码输入框显示的数值
        /// </summary>
        /// <param name="value"></param>
        private void UpdatePageInputUi(int value)
        {
            updatingPageInputUi = true;
            PageInput.Value = value;
            updatingPageInputUi = false;
        }
        private bool updatingPageInputUi = false;

        /// <summary>
        /// 仅更新当前显示的当前页码, 而不触发 <see cref="OnPageIndexChanged"/> 事件
        /// </summary>
        /// <param name="value"></param>
        public void SetShowingCurrentIndex(int value)
        {
            settingShowingCurrentIndex = true;
            CurrentIndex = value;
            settingShowingCurrentIndex = false;
        }
        private bool settingShowingCurrentIndex = false;
        #endregion
    }
}
