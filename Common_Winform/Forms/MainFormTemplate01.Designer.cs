using Common_Winform.Pages.Layout;

namespace Common_Winform.Forms
{
    partial class MainFormTemplate01
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
            this.AreaBody = new Common_Winform.Pages.Layout.TabPageLayout01();
            this.TopMenuArea = new System.Windows.Forms.MenuStrip();
            this.BottomStatusArea = new System.Windows.Forms.StatusStrip();
            this.SuspendLayout();
            // 
            // AreaBody
            // 
            this.AreaBody.AddPagesHandle = null;
            this.AreaBody.ContainerFont = null;
            this.AreaBody.DefaultSelectIndex = null;
            this.AreaBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AreaBody.Location = new System.Drawing.Point(0, 24);
            this.AreaBody.LogName = "TabPageLayout01";
            this.AreaBody.Name = "AreaBody";
            this.AreaBody.PageIndex = 0;
            this.AreaBody.ReadOnly = false;
            this.AreaBody.Size = new System.Drawing.Size(800, 404);
            this.AreaBody.TabAreaWidth = 30;
            this.AreaBody.TabIndex = 0;
            this.AreaBody.初始化过程参数_标签间距 = 3;
            this.AreaBody.初始化过程参数_标签默认长度 = ((ushort)(100));
            // 
            // TopMenuArea
            // 
            this.TopMenuArea.Location = new System.Drawing.Point(0, 0);
            this.TopMenuArea.Name = "TopMenuArea";
            this.TopMenuArea.Size = new System.Drawing.Size(800, 24);
            this.TopMenuArea.TabIndex = 1;
            this.TopMenuArea.Text = "menuStrip1";
            // 
            // BottomStatusArea
            // 
            this.BottomStatusArea.Location = new System.Drawing.Point(0, 428);
            this.BottomStatusArea.Name = "BottomStatusArea";
            this.BottomStatusArea.Size = new System.Drawing.Size(800, 22);
            this.BottomStatusArea.TabIndex = 2;
            this.BottomStatusArea.Text = "statusStrip1";
            // 
            // MainFormTemplate01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.AreaBody);
            this.Controls.Add(this.BottomStatusArea);
            this.Controls.Add(this.TopMenuArea);
            this.MainMenuStrip = this.TopMenuArea;
            this.Name = "MainFormTemplate01";
            this.Text = "MainFormTemplate01";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private TabPageLayout01 AreaBody;

        #endregion

        private MenuStrip TopMenuArea;
        private StatusStrip BottomStatusArea;
    }
}