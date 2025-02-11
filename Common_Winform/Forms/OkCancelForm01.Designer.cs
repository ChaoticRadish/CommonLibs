namespace Common_Winform.Forms
{
    partial class OkCancelForm01
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            Button_Cancel = new Button();
            Button_Ok = new Button();
            显示容器 = new Panel();
            默认显示内容 = new Label();
            调整尺寸的计时器 = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1.SuspendLayout();
            显示容器.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.7777786F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.7777786F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.Controls.Add(Button_Cancel, 4, 1);
            tableLayoutPanel1.Controls.Add(Button_Ok, 1, 1);
            tableLayoutPanel1.Controls.Add(显示容器, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(588, 267);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // Button_Cancel
            // 
            Button_Cancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Button_Cancel.DialogResult = DialogResult.Cancel;
            Button_Cancel.Font = new Font("Microsoft YaHei UI", 11F);
            Button_Cancel.Location = new Point(370, 225);
            Button_Cancel.Margin = new Padding(12, 3, 12, 3);
            Button_Cancel.Name = "Button_Cancel";
            Button_Cancel.Size = new Size(139, 30);
            Button_Cancel.TabIndex = 2;
            Button_Cancel.Text = "取消";
            Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // Button_Ok
            // 
            Button_Ok.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Button_Ok.DialogResult = DialogResult.OK;
            Button_Ok.Font = new Font("Microsoft YaHei UI", 11F);
            Button_Ok.Location = new Point(77, 225);
            Button_Ok.Margin = new Padding(12, 3, 12, 3);
            Button_Ok.Name = "Button_Ok";
            Button_Ok.Size = new Size(139, 30);
            Button_Ok.TabIndex = 1;
            Button_Ok.Text = "确定";
            Button_Ok.UseVisualStyleBackColor = true;
            // 
            // 显示容器
            // 
            tableLayoutPanel1.SetColumnSpan(显示容器, 6);
            显示容器.Controls.Add(默认显示内容);
            显示容器.Location = new Point(10, 10);
            显示容器.Margin = new Padding(10);
            显示容器.Name = "显示容器";
            显示容器.Size = new Size(568, 193);
            显示容器.TabIndex = 3;
            // 
            // 默认显示内容
            // 
            默认显示内容.Dock = DockStyle.Fill;
            默认显示内容.Location = new Point(0, 0);
            默认显示内容.Name = "默认显示内容";
            默认显示内容.Size = new Size(568, 193);
            默认显示内容.TabIndex = 0;
            默认显示内容.Text = "默认显示内容";
            默认显示内容.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // OkCancelForm01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(588, 267);
            Controls.Add(tableLayoutPanel1);
            Name = "OkCancelForm01";
            Text = "OkCancelForm01";
            tableLayoutPanel1.ResumeLayout(false);
            显示容器.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button Button_Cancel;
        private Button Button_Ok;
        private Panel 显示容器;
        private Label 默认显示内容;
        private System.Windows.Forms.Timer 调整尺寸的计时器;
    }
}