namespace Common_Winform.Controls.FeatureGroup
{
    partial class LogShower01
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
            OutShowingTimer = new System.Windows.Forms.Timer(components);
            Shower = new RichTextBox();
            SuspendLayout();
            // 
            // OutShowingTimer
            // 
            OutShowingTimer.Enabled = true;
            OutShowingTimer.Tick += OutShowingTimer_Tick;
            // 
            // Shower
            // 
            Shower.Dock = DockStyle.Fill;
            Shower.Location = new Point(0, 0);
            Shower.Name = "Shower";
            Shower.ReadOnly = true;
            Shower.Size = new Size(150, 150);
            Shower.TabIndex = 0;
            Shower.Text = "";
            // 
            // LogShower01
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Shower);
            Name = "LogShower01";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer OutShowingTimer;
        private RichTextBox Shower;
    }
}
