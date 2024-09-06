namespace Common_Winform.Controls.FeatureGroup
{
    partial class LogTableGroup
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            PagingBox = new PagingBox();
            ControllerArea = new Panel();
            label2 = new Label();
            Input_Level = new ListComboBox();
            label1 = new Label();
            Input_Type = new ListComboBox();
            CountShower = new Label();
            CheckBox_KeepRefresh = new CheckBox();
            LogShower = new DataGridView();
            TimeColumn = new DataGridViewTextBoxColumn();
            TypeNameShower = new DataGridViewTextBoxColumn();
            LevelNameColumn = new DataGridViewTextBoxColumn();
            MessageColumn = new DataGridViewTextBoxColumn();
            TimerRefreshShowing = new System.Windows.Forms.Timer(components);
            ControllerArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogShower).BeginInit();
            SuspendLayout();
            // 
            // PagingBox
            // 
            PagingBox.CurrentIndex = 0;
            PagingBox.Dock = DockStyle.Bottom;
            PagingBox.Location = new Point(0, 438);
            PagingBox.Margin = new Padding(4);
            PagingBox.Name = "PagingBox";
            PagingBox.PageSize = 100;
            PagingBox.ReadOnly = false;
            PagingBox.Size = new Size(884, 34);
            PagingBox.TabIndex = 0;
            PagingBox.TotalCount = 5000;
            PagingBox.TotalPage = 50;
            PagingBox.UseRefreshButton = false;
            PagingBox.OnPageIndexChanged += PagingBox_OnPageIndexChanged;
            // 
            // ControllerArea
            // 
            ControllerArea.Controls.Add(label2);
            ControllerArea.Controls.Add(Input_Level);
            ControllerArea.Controls.Add(label1);
            ControllerArea.Controls.Add(Input_Type);
            ControllerArea.Controls.Add(CountShower);
            ControllerArea.Controls.Add(CheckBox_KeepRefresh);
            ControllerArea.Dock = DockStyle.Top;
            ControllerArea.Location = new Point(0, 0);
            ControllerArea.Name = "ControllerArea";
            ControllerArea.Size = new Size(884, 30);
            ControllerArea.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(279, 6);
            label2.Name = "label2";
            label2.Size = new Size(32, 17);
            label2.TabIndex = 5;
            label2.Text = "级别";
            // 
            // Input_Level
            // 
            Input_Level.AllowNotSelect = false;
            Input_Level.DropDownStyle = ComboBoxStyle.DropDownList;
            Input_Level.FormattingEnabled = true;
            Input_Level.Location = new Point(317, 3);
            Input_Level.Name = "Input_Level";
            Input_Level.NotSelecedString = "- 未选择 -";
            Input_Level.Size = new Size(121, 25);
            Input_Level.TabIndex = 4;
            Input_Level.SelectedIndexChanged += Input_Level_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(94, 6);
            label1.Name = "label1";
            label1.Size = new Size(32, 17);
            label1.TabIndex = 3;
            label1.Text = "类别";
            // 
            // Input_Type
            // 
            Input_Type.AllowNotSelect = false;
            Input_Type.DropDownStyle = ComboBoxStyle.DropDownList;
            Input_Type.FormattingEnabled = true;
            Input_Type.Location = new Point(132, 3);
            Input_Type.Name = "Input_Type";
            Input_Type.NotSelecedString = "- 未选择 -";
            Input_Type.Size = new Size(121, 25);
            Input_Type.TabIndex = 2;
            Input_Type.SelectedIndexChanged += Input_Type_SelectedIndexChanged;
            // 
            // CountShower
            // 
            CountShower.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            CountShower.Location = new Point(708, 5);
            CountShower.Name = "CountShower";
            CountShower.Size = new Size(173, 20);
            CountShower.TabIndex = 1;
            CountShower.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CheckBox_KeepRefresh
            // 
            CheckBox_KeepRefresh.AutoSize = true;
            CheckBox_KeepRefresh.Checked = true;
            CheckBox_KeepRefresh.CheckState = CheckState.Checked;
            CheckBox_KeepRefresh.Location = new Point(5, 5);
            CheckBox_KeepRefresh.Name = "CheckBox_KeepRefresh";
            CheckBox_KeepRefresh.Size = new Size(75, 21);
            CheckBox_KeepRefresh.TabIndex = 0;
            CheckBox_KeepRefresh.Text = "实时刷新";
            CheckBox_KeepRefresh.UseVisualStyleBackColor = true;
            // 
            // LogShower
            // 
            LogShower.AllowUserToAddRows = false;
            LogShower.AllowUserToDeleteRows = false;
            LogShower.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            LogShower.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LogShower.Columns.AddRange(new DataGridViewColumn[] { TimeColumn, TypeNameShower, LevelNameColumn, MessageColumn });
            LogShower.Dock = DockStyle.Fill;
            LogShower.Location = new Point(0, 30);
            LogShower.Name = "LogShower";
            LogShower.ReadOnly = true;
            LogShower.RowHeadersVisible = false;
            dataGridViewCellStyle5.Padding = new Padding(2);
            LogShower.RowsDefaultCellStyle = dataGridViewCellStyle5;
            LogShower.Size = new Size(884, 408);
            LogShower.TabIndex = 2;
            LogShower.CellDoubleClick += LogShower_CellDoubleClick;
            LogShower.RowsAdded += LogShower_RowsAdded;
            // 
            // TimeColumn
            // 
            TimeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            TimeColumn.DataPropertyName = "Time";
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.Format = "yyyy-MM-dd HH:mm:ss:fff";
            TimeColumn.DefaultCellStyle = dataGridViewCellStyle1;
            TimeColumn.HeaderText = "时间";
            TimeColumn.MinimumWidth = 120;
            TimeColumn.Name = "TimeColumn";
            TimeColumn.ReadOnly = true;
            TimeColumn.Width = 120;
            // 
            // TypeNameShower
            // 
            TypeNameShower.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            TypeNameShower.DataPropertyName = "TypeName";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TypeNameShower.DefaultCellStyle = dataGridViewCellStyle2;
            TypeNameShower.HeaderText = "类别";
            TypeNameShower.MinimumWidth = 60;
            TypeNameShower.Name = "TypeNameShower";
            TypeNameShower.ReadOnly = true;
            TypeNameShower.Width = 60;
            // 
            // LevelNameColumn
            // 
            LevelNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            LevelNameColumn.DataPropertyName = "LevelName";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            LevelNameColumn.DefaultCellStyle = dataGridViewCellStyle3;
            LevelNameColumn.HeaderText = "级别";
            LevelNameColumn.MinimumWidth = 60;
            LevelNameColumn.Name = "LevelNameColumn";
            LevelNameColumn.ReadOnly = true;
            LevelNameColumn.Width = 60;
            // 
            // MessageColumn
            // 
            MessageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            MessageColumn.DataPropertyName = "Message";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            MessageColumn.DefaultCellStyle = dataGridViewCellStyle4;
            MessageColumn.HeaderText = "正文";
            MessageColumn.Name = "MessageColumn";
            MessageColumn.ReadOnly = true;
            // 
            // TimerRefreshShowing
            // 
            TimerRefreshShowing.Enabled = true;
            TimerRefreshShowing.Tick += TimerRefreshShowing_Tick;
            // 
            // LogTableGroup
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LogShower);
            Controls.Add(ControllerArea);
            Controls.Add(PagingBox);
            Name = "LogTableGroup";
            Size = new Size(884, 472);
            ControllerArea.ResumeLayout(false);
            ControllerArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogShower).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PagingBox PagingBox;
        private Panel ControllerArea;
        private DataGridView LogShower;
        private CheckBox CheckBox_KeepRefresh;
        private System.Windows.Forms.Timer TimerRefreshShowing;
        private Label CountShower;
        private Label label1;
        private ListComboBox Input_Type;
        private Label label2;
        private ListComboBox Input_Level;
        private DataGridViewTextBoxColumn TimeColumn;
        private DataGridViewTextBoxColumn TypeNameShower;
        private DataGridViewTextBoxColumn LevelNameColumn;
        private DataGridViewTextBoxColumn MessageColumn;
    }
}
