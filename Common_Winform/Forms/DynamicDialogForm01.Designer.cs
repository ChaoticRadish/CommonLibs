namespace Common_Winform.Forms
{
    partial class DynamicDialogForm01
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
            显示容器 = new Panel();
            默认显示内容 = new Label();
            按钮流式布局容器 = new FlowLayoutPanel();
            调整尺寸的计时器 = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1.SuspendLayout();
            显示容器.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.11111F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.7777786F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.7777786F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111107F));
            tableLayoutPanel1.Controls.Add(显示容器, 0, 0);
            tableLayoutPanel1.Controls.Add(按钮流式布局容器, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel1.Size = new Size(678, 403);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // 显示容器
            // 
            tableLayoutPanel1.SetColumnSpan(显示容器, 6);
            显示容器.Controls.Add(默认显示内容);
            显示容器.Dock = DockStyle.Fill;
            显示容器.Location = new Point(10, 10);
            显示容器.Margin = new Padding(10);
            显示容器.Name = "显示容器";
            显示容器.Size = new Size(658, 338);
            显示容器.TabIndex = 3;
            // 
            // 默认显示内容
            // 
            默认显示内容.Dock = DockStyle.Fill;
            默认显示内容.Location = new Point(0, 0);
            默认显示内容.Name = "默认显示内容";
            默认显示内容.Size = new Size(658, 338);
            默认显示内容.TabIndex = 0;
            默认显示内容.Text = "默认显示内容";
            默认显示内容.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // 按钮流式布局容器
            // 
            tableLayoutPanel1.SetColumnSpan(按钮流式布局容器, 6);
            按钮流式布局容器.Dock = DockStyle.Fill;
            按钮流式布局容器.FlowDirection = FlowDirection.RightToLeft;
            按钮流式布局容器.Location = new Point(3, 361);
            按钮流式布局容器.Name = "按钮流式布局容器";
            按钮流式布局容器.Size = new Size(672, 39);
            按钮流式布局容器.TabIndex = 4;
            // 
            // DynamicDialogForm01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(678, 403);
            Controls.Add(tableLayoutPanel1);
            Name = "DynamicDialogForm01";
            Text = "DynamicDialogForm01";
            tableLayoutPanel1.ResumeLayout(false);
            显示容器.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button Button_O;
        private Panel 显示容器;
        private Label 默认显示内容;
        private System.Windows.Forms.Timer 调整尺寸的计时器;
        private FlowLayoutPanel 按钮流式布局容器;
    }
}