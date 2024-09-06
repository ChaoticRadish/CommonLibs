namespace Common_Winform.Forms
{
    partial class ExceptionShowerForm01
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
            label1 = new Label();
            Shower_异常类型 = new TextBox();
            label2 = new Label();
            Shower_HRESULT = new TextBox();
            label3 = new Label();
            Shower_Source = new TextBox();
            label4 = new Label();
            Shower_TargetSite = new TextBox();
            label5 = new Label();
            Shower_HelpLink = new TextBox();
            label6 = new Label();
            Shower_Message = new RichTextBox();
            label7 = new Label();
            Shower_Data = new RichTextBox();
            Shower_StackTrace = new RichTextBox();
            label8 = new Label();
            Shower_InnerException = new TextBox();
            label9 = new Label();
            Button_详情 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new Point(20, 13);
            label1.Name = "label1";
            label1.Size = new Size(104, 22);
            label1.TabIndex = 0;
            label1.Text = "异常类型";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_异常类型
            // 
            Shower_异常类型.Location = new Point(130, 13);
            Shower_异常类型.Name = "Shower_异常类型";
            Shower_异常类型.ReadOnly = true;
            Shower_异常类型.Size = new Size(587, 23);
            Shower_异常类型.TabIndex = 1;
            // 
            // label2
            // 
            label2.Location = new Point(20, 42);
            label2.Name = "label2";
            label2.Size = new Size(104, 22);
            label2.TabIndex = 2;
            label2.Text = "HRESULT";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_HRESULT
            // 
            Shower_HRESULT.Location = new Point(130, 42);
            Shower_HRESULT.Name = "Shower_HRESULT";
            Shower_HRESULT.ReadOnly = true;
            Shower_HRESULT.Size = new Size(140, 23);
            Shower_HRESULT.TabIndex = 3;
            // 
            // label3
            // 
            label3.Location = new Point(276, 42);
            label3.Name = "label3";
            label3.Size = new Size(104, 22);
            label3.TabIndex = 4;
            label3.Text = "来源";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_Source
            // 
            Shower_Source.Location = new Point(386, 42);
            Shower_Source.Name = "Shower_Source";
            Shower_Source.ReadOnly = true;
            Shower_Source.Size = new Size(331, 23);
            Shower_Source.TabIndex = 5;
            // 
            // label4
            // 
            label4.Location = new Point(20, 71);
            label4.Name = "label4";
            label4.Size = new Size(104, 22);
            label4.TabIndex = 6;
            label4.Text = "异常方法";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_TargetSite
            // 
            Shower_TargetSite.Location = new Point(130, 71);
            Shower_TargetSite.Name = "Shower_TargetSite";
            Shower_TargetSite.ReadOnly = true;
            Shower_TargetSite.Size = new Size(587, 23);
            Shower_TargetSite.TabIndex = 7;
            // 
            // label5
            // 
            label5.Location = new Point(20, 100);
            label5.Name = "label5";
            label5.Size = new Size(104, 22);
            label5.TabIndex = 8;
            label5.Text = "帮助链接";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_HelpLink
            // 
            Shower_HelpLink.Location = new Point(130, 100);
            Shower_HelpLink.Name = "Shower_HelpLink";
            Shower_HelpLink.ReadOnly = true;
            Shower_HelpLink.Size = new Size(587, 23);
            Shower_HelpLink.TabIndex = 9;
            // 
            // label6
            // 
            label6.Location = new Point(20, 129);
            label6.Name = "label6";
            label6.Size = new Size(104, 22);
            label6.TabIndex = 10;
            label6.Text = "异常消息";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_Message
            // 
            Shower_Message.Location = new Point(130, 129);
            Shower_Message.Name = "Shower_Message";
            Shower_Message.ReadOnly = true;
            Shower_Message.Size = new Size(394, 67);
            Shower_Message.TabIndex = 11;
            Shower_Message.Text = "";
            // 
            // label7
            // 
            label7.Location = new Point(20, 202);
            label7.Name = "label7";
            label7.Size = new Size(104, 22);
            label7.TabIndex = 12;
            label7.Text = "异常数据";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_Data
            // 
            Shower_Data.Location = new Point(130, 202);
            Shower_Data.Name = "Shower_Data";
            Shower_Data.ReadOnly = true;
            Shower_Data.Size = new Size(394, 67);
            Shower_Data.TabIndex = 13;
            Shower_Data.Text = "";
            // 
            // Shower_StackTrace
            // 
            Shower_StackTrace.Location = new Point(130, 275);
            Shower_StackTrace.Name = "Shower_StackTrace";
            Shower_StackTrace.ReadOnly = true;
            Shower_StackTrace.Size = new Size(587, 142);
            Shower_StackTrace.TabIndex = 14;
            Shower_StackTrace.Text = "";
            // 
            // label8
            // 
            label8.Location = new Point(20, 275);
            label8.Name = "label8";
            label8.Size = new Size(104, 22);
            label8.TabIndex = 15;
            label8.Text = "调用堆栈";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Shower_InnerException
            // 
            Shower_InnerException.Location = new Point(130, 423);
            Shower_InnerException.Name = "Shower_InnerException";
            Shower_InnerException.ReadOnly = true;
            Shower_InnerException.Size = new Size(394, 23);
            Shower_InnerException.TabIndex = 16;
            // 
            // label9
            // 
            label9.Location = new Point(20, 423);
            label9.Name = "label9";
            label9.Size = new Size(104, 22);
            label9.TabIndex = 17;
            label9.Text = "源异常";
            label9.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Button_详情
            // 
            Button_详情.Location = new Point(530, 423);
            Button_详情.Name = "Button_详情";
            Button_详情.Size = new Size(75, 23);
            Button_详情.TabIndex = 18;
            Button_详情.Text = "详情";
            Button_详情.UseVisualStyleBackColor = true;
            Button_详情.Click += Button_详情_Click;
            // 
            // ExceptionShower01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Button_详情);
            Controls.Add(label9);
            Controls.Add(Shower_InnerException);
            Controls.Add(label8);
            Controls.Add(Shower_StackTrace);
            Controls.Add(Shower_Data);
            Controls.Add(label7);
            Controls.Add(Shower_Message);
            Controls.Add(label6);
            Controls.Add(Shower_HelpLink);
            Controls.Add(label5);
            Controls.Add(Shower_TargetSite);
            Controls.Add(label4);
            Controls.Add(Shower_Source);
            Controls.Add(label3);
            Controls.Add(Shower_HRESULT);
            Controls.Add(label2);
            Controls.Add(Shower_异常类型);
            Controls.Add(label1);
            Name = "ExceptionShower01";
            Text = "异常信息";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox Shower_异常类型;
        private Label label2;
        private TextBox Shower_HRESULT;
        private Label label3;
        private TextBox Shower_Source;
        private Label label4;
        private TextBox Shower_TargetSite;
        private Label label5;
        private TextBox Shower_HelpLink;
        private Label label6;
        private RichTextBox Shower_Message;
        private Label label7;
        private RichTextBox Shower_Data;
        private RichTextBox Shower_StackTrace;
        private Label label8;
        private TextBox Shower_InnerException;
        private Label label9;
        private Button Button_详情;
    }
}