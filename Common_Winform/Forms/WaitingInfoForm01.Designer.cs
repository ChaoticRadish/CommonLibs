namespace Common_Winform.Forms
{
    partial class WaitingInfoForm01
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            WaitingBox = new Common_Winform.Controls.FeatureGroup.WaitingInfoBox();
            CancelWaitingButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // WaitingBox
            // 
            WaitingBox.BorderStyle = BorderStyle.FixedSingle;
            WaitingBox.CirculCount = 0;
            WaitingBox.Dock = DockStyle.Fill;
            WaitingBox.Info = "等待信息";
            WaitingBox.Location = new Point(74, 4);
            WaitingBox.Margin = new Padding(4, 4, 4, 4);
            WaitingBox.Name = "WaitingBox";
            WaitingBox.Size = new Size(207, 59);
            WaitingBox.TabIndex = 0;
            // 
            // CancelWaitingButton
            // 
            CancelWaitingButton.Anchor = AnchorStyles.None;
            CancelWaitingButton.Location = new Point(140, 70);
            CancelWaitingButton.Name = "CancelWaitingButton";
            CancelWaitingButton.Size = new Size(75, 22);
            CancelWaitingButton.TabIndex = 1;
            CancelWaitingButton.Text = "取消等待";
            CancelWaitingButton.UseVisualStyleBackColor = true;
            CancelWaitingButton.Click += CancelButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19.8053017F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60.38939F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19.8053017F));
            tableLayoutPanel1.Controls.Add(WaitingBox, 1, 0);
            tableLayoutPanel1.Controls.Add(CancelWaitingButton, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 71.42857F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 28.5714283F));
            tableLayoutPanel1.Size = new Size(357, 95);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // WaitingInfoForm01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(357, 95);
            Controls.Add(tableLayoutPanel1);
            Name = "WaitingInfoForm01";
            Text = "等待中...";
            TopMost = true;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Common_Winform.Controls.FeatureGroup.WaitingInfoBox WaitingBox;
        private Button CancelWaitingButton;
        private TableLayoutPanel tableLayoutPanel1;
    }
}