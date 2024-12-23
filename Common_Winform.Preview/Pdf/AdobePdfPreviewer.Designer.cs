namespace Common_Winform.Preview.Pdf
{
    partial class AdobePdfPreviewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdobePdfPreviewer));
            axAcropdf = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)axAcropdf).BeginInit();
            SuspendLayout();
            // 
            // axAcropdf
            // 
            axAcropdf.Dock = DockStyle.Fill;
            axAcropdf.Enabled = true;
            axAcropdf.Location = new Point(0, 0);
            axAcropdf.Name = "axAcropdf";
            axAcropdf.OcxState = (AxHost.State)resources.GetObject("axAcropdf.OcxState");
            axAcropdf.Size = new Size(150, 150);
            axAcropdf.TabIndex = 0;
            // 
            // AdobePdfPreviewer
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(axAcropdf);
            Name = "AdobePdfPreviewer";
            ((System.ComponentModel.ISupportInitialize)axAcropdf).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private AxAcroPDFLib.AxAcroPDF axAcropdf;
    }
}
