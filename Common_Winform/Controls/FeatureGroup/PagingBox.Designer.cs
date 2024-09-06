namespace Common_Winform.Controls.FeatureGroup
{
    partial class PagingBox
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            FirstButton = new Button();
            LastButton = new Button();
            NextButton = new Button();
            PrevButton = new Button();
            PageInput = new ValueBox.IntegerBox();
            JumpButton = new Button();
            TotalPageShower = new TextBox();
            label2 = new Label();
            Area_AlwayShow = new Panel();
            Area_PageIndexBox = new TableLayoutPanel();
            Area_PageIndex = new Panel();
            Area_Right = new Panel();
            Area_RefreshButton = new Panel();
            RefreshButton = new Button();
            PageButtonInnerArea = new Panel();
            PageButtonArea = new TableLayoutPanel();
            Area_Left = new Panel();
            Area_AlwayShow.SuspendLayout();
            Area_PageIndexBox.SuspendLayout();
            Area_PageIndex.SuspendLayout();
            Area_Right.SuspendLayout();
            Area_RefreshButton.SuspendLayout();
            PageButtonArea.SuspendLayout();
            Area_Left.SuspendLayout();
            SuspendLayout();
            // 
            // FirstButton
            // 
            FirstButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            FirstButton.Location = new Point(4, 3);
            FirstButton.Margin = new Padding(4);
            FirstButton.Name = "FirstButton";
            FirstButton.Size = new Size(71, 41);
            FirstButton.TabIndex = 14;
            FirstButton.Text = "首页";
            FirstButton.UseVisualStyleBackColor = true;
            FirstButton.Click += FirstButton_Click;
            // 
            // LastButton
            // 
            LastButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LastButton.Location = new Point(103, 3);
            LastButton.Margin = new Padding(4);
            LastButton.Name = "LastButton";
            LastButton.Size = new Size(65, 41);
            LastButton.TabIndex = 13;
            LastButton.Text = "尾页";
            LastButton.UseVisualStyleBackColor = true;
            LastButton.Click += LastButton_Click;
            // 
            // NextButton
            // 
            NextButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            NextButton.Location = new Point(8, 3);
            NextButton.Margin = new Padding(4);
            NextButton.Name = "NextButton";
            NextButton.Size = new Size(88, 41);
            NextButton.TabIndex = 12;
            NextButton.Text = "下一页";
            NextButton.UseVisualStyleBackColor = true;
            NextButton.Click += NextButton_Click;
            // 
            // PrevButton
            // 
            PrevButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            PrevButton.Location = new Point(80, 3);
            PrevButton.Margin = new Padding(4);
            PrevButton.Name = "PrevButton";
            PrevButton.Size = new Size(88, 41);
            PrevButton.TabIndex = 11;
            PrevButton.Text = "上一页";
            PrevButton.UseVisualStyleBackColor = true;
            PrevButton.Click += PrevButton_Click;
            // 
            // PageInput
            // 
            PageInput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            PageInput.DefaultValue = 1;
            PageInput.Location = new Point(6, 2);
            PageInput.Margin = new Padding(4);
            PageInput.MaxValue = null;
            PageInput.MinValue = null;
            PageInput.Name = "PageInput";
            PageInput.Range = ValueBox.NumberRangeEnum.Positive;
            PageInput.Size = new Size(50, 23);
            PageInput.TabIndex = 15;
            PageInput.Text = "1";
            PageInput.Value = 1;
            PageInput.ValueChanged += PageInput_ValueChanged;
            // 
            // JumpButton
            // 
            JumpButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            JumpButton.Location = new Point(317, 3);
            JumpButton.Margin = new Padding(4);
            JumpButton.Name = "JumpButton";
            JumpButton.Size = new Size(65, 41);
            JumpButton.TabIndex = 16;
            JumpButton.Text = "跳转";
            JumpButton.UseVisualStyleBackColor = true;
            JumpButton.Click += JumpButton_Click;
            // 
            // TotalPageShower
            // 
            TotalPageShower.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            TotalPageShower.Location = new Point(82, 2);
            TotalPageShower.Margin = new Padding(4);
            TotalPageShower.Name = "TotalPageShower";
            TotalPageShower.ReadOnly = true;
            TotalPageShower.Size = new Size(53, 23);
            TotalPageShower.TabIndex = 18;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(63, 5);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(13, 17);
            label2.TabIndex = 17;
            label2.Text = "/";
            // 
            // Area_AlwayShow
            // 
            Area_AlwayShow.Controls.Add(JumpButton);
            Area_AlwayShow.Controls.Add(Area_PageIndexBox);
            Area_AlwayShow.Controls.Add(NextButton);
            Area_AlwayShow.Controls.Add(LastButton);
            Area_AlwayShow.Dock = DockStyle.Fill;
            Area_AlwayShow.Location = new Point(0, 0);
            Area_AlwayShow.Name = "Area_AlwayShow";
            Area_AlwayShow.Size = new Size(389, 47);
            Area_AlwayShow.TabIndex = 20;
            // 
            // Area_PageIndexBox
            // 
            Area_PageIndexBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            Area_PageIndexBox.ColumnCount = 1;
            Area_PageIndexBox.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Area_PageIndexBox.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            Area_PageIndexBox.Controls.Add(Area_PageIndex, 0, 0);
            Area_PageIndexBox.Location = new Point(167, 0);
            Area_PageIndexBox.Margin = new Padding(0);
            Area_PageIndexBox.Name = "Area_PageIndexBox";
            Area_PageIndexBox.RowCount = 1;
            Area_PageIndexBox.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Area_PageIndexBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            Area_PageIndexBox.Size = new Size(150, 47);
            Area_PageIndexBox.TabIndex = 1;
            // 
            // Area_PageIndex
            // 
            Area_PageIndex.Anchor = AnchorStyles.None;
            Area_PageIndex.Controls.Add(PageInput);
            Area_PageIndex.Controls.Add(label2);
            Area_PageIndex.Controls.Add(TotalPageShower);
            Area_PageIndex.Location = new Point(3, 10);
            Area_PageIndex.Margin = new Padding(0);
            Area_PageIndex.Name = "Area_PageIndex";
            Area_PageIndex.Size = new Size(143, 26);
            Area_PageIndex.TabIndex = 19;
            // 
            // Area_Right
            // 
            Area_Right.Controls.Add(Area_AlwayShow);
            Area_Right.Controls.Add(Area_RefreshButton);
            Area_Right.Dock = DockStyle.Right;
            Area_Right.Location = new Point(551, 0);
            Area_Right.Name = "Area_Right";
            Area_Right.Size = new Size(484, 47);
            Area_Right.TabIndex = 21;
            // 
            // Area_RefreshButton
            // 
            Area_RefreshButton.Controls.Add(RefreshButton);
            Area_RefreshButton.Dock = DockStyle.Right;
            Area_RefreshButton.Location = new Point(389, 0);
            Area_RefreshButton.Name = "Area_RefreshButton";
            Area_RefreshButton.Size = new Size(95, 47);
            Area_RefreshButton.TabIndex = 21;
            // 
            // RefreshButton
            // 
            RefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            RefreshButton.Location = new Point(0, 3);
            RefreshButton.Margin = new Padding(4);
            RefreshButton.Name = "RefreshButton";
            RefreshButton.Size = new Size(92, 41);
            RefreshButton.TabIndex = 12;
            RefreshButton.Text = "刷新";
            RefreshButton.UseVisualStyleBackColor = true;
            // 
            // PageButtonInnerArea
            // 
            PageButtonInnerArea.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            PageButtonInnerArea.Location = new Point(89, 4);
            PageButtonInnerArea.Margin = new Padding(4);
            PageButtonInnerArea.Name = "PageButtonInnerArea";
            PageButtonInnerArea.Size = new Size(198, 39);
            PageButtonInnerArea.TabIndex = 0;
            // 
            // PageButtonArea
            // 
            PageButtonArea.ColumnCount = 1;
            PageButtonArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            PageButtonArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            PageButtonArea.Controls.Add(PageButtonInnerArea, 0, 0);
            PageButtonArea.Dock = DockStyle.Fill;
            PageButtonArea.Location = new Point(174, 0);
            PageButtonArea.Name = "PageButtonArea";
            PageButtonArea.RowCount = 1;
            PageButtonArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            PageButtonArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            PageButtonArea.Size = new Size(377, 47);
            PageButtonArea.TabIndex = 22;
            // 
            // Area_Left
            // 
            Area_Left.Controls.Add(PrevButton);
            Area_Left.Controls.Add(FirstButton);
            Area_Left.Dock = DockStyle.Left;
            Area_Left.Location = new Point(0, 0);
            Area_Left.Name = "Area_Left";
            Area_Left.Size = new Size(174, 47);
            Area_Left.TabIndex = 23;
            // 
            // PagingBox
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(PageButtonArea);
            Controls.Add(Area_Left);
            Controls.Add(Area_Right);
            Margin = new Padding(4);
            Name = "PagingBox";
            Size = new Size(1035, 47);
            Area_AlwayShow.ResumeLayout(false);
            Area_PageIndexBox.ResumeLayout(false);
            Area_PageIndex.ResumeLayout(false);
            Area_PageIndex.PerformLayout();
            Area_Right.ResumeLayout(false);
            Area_RefreshButton.ResumeLayout(false);
            PageButtonArea.ResumeLayout(false);
            Area_Left.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button FirstButton;
        private System.Windows.Forms.Button LastButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button PrevButton;
        private ValueBox.IntegerBox PageInput;
        private System.Windows.Forms.Button JumpButton;
        private System.Windows.Forms.TextBox TotalPageShower;
        private System.Windows.Forms.Label label2;
        private Panel Area_AlwayShow;
        private Panel Area_Right;
        private Panel Area_RefreshButton;
        private Button RefreshButton;
        private Panel Area_PageIndex;
        private TableLayoutPanel Area_PageIndexBox;
        private Panel PageButtonInnerArea;
        private TableLayoutPanel PageButtonArea;
        private Panel Area_Left;
    }
}
