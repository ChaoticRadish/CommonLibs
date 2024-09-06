namespace Common_Winform.Forms
{
    partial class AskOkCancelForm01
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
            Shower_主要信息 = new RichTextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            Button_Cancel = new Button();
            Button_Ok = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // Shower_主要信息
            // 
            Shower_主要信息.BorderStyle = BorderStyle.None;
            tableLayoutPanel1.SetColumnSpan(Shower_主要信息, 2);
            Shower_主要信息.Dock = DockStyle.Fill;
            Shower_主要信息.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            Shower_主要信息.Location = new Point(10, 10);
            Shower_主要信息.Margin = new Padding(10);
            Shower_主要信息.Name = "Shower_主要信息";
            Shower_主要信息.ReadOnly = true;
            Shower_主要信息.Size = new Size(370, 142);
            Shower_主要信息.TabIndex = 0;
            Shower_主要信息.Text = "";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(Button_Cancel, 1, 1);
            tableLayoutPanel1.Controls.Add(Shower_主要信息, 0, 0);
            tableLayoutPanel1.Controls.Add(Button_Ok, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(390, 203);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // Button_Cancel
            // 
            Button_Cancel.Anchor = AnchorStyles.None;
            Button_Cancel.DialogResult = DialogResult.Cancel;
            Button_Cancel.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            Button_Cancel.Location = new Point(242, 167);
            Button_Cancel.Name = "Button_Cancel";
            Button_Cancel.Size = new Size(100, 30);
            Button_Cancel.TabIndex = 2;
            Button_Cancel.Text = "取消";
            Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // Button_Ok
            // 
            Button_Ok.Anchor = AnchorStyles.None;
            Button_Ok.DialogResult = DialogResult.OK;
            Button_Ok.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            Button_Ok.Location = new Point(47, 167);
            Button_Ok.Name = "Button_Ok";
            Button_Ok.Size = new Size(100, 30);
            Button_Ok.TabIndex = 1;
            Button_Ok.Text = "确定";
            Button_Ok.UseVisualStyleBackColor = true;
            // 
            // AskOkCancelForm01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 203);
            Controls.Add(tableLayoutPanel1);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Name = "AskOkCancelForm01";
            StartPosition = FormStartPosition.CenterParent;
            Text = "提问窗口";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox Shower_主要信息;
        private TableLayoutPanel tableLayoutPanel1;
        private Button Button_Ok;
        private Button Button_Cancel;
    }
}