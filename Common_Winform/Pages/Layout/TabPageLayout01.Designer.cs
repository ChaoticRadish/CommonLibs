namespace Common_Winform.Pages.Layout
{
    partial class TabPageLayout01
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
            this.Area_Body = new System.Windows.Forms.Panel();
            this.Area_InnerBody = new System.Windows.Forms.Panel();
            this.Area_TabController = new System.Windows.Forms.Panel();
            this.Area_TabControllerInnerBox = new Common_Winform.Container.ScrollPanel();
            this.Area_Body.SuspendLayout();
            this.Area_TabController.SuspendLayout();
            this.SuspendLayout();
            // 
            // Area_Body
            // 
            this.Area_Body.BackColor = System.Drawing.Color.Silver;
            this.Area_Body.Controls.Add(this.Area_InnerBody);
            this.Area_Body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Area_Body.Location = new System.Drawing.Point(30, 0);
            this.Area_Body.Name = "Area_Body";
            this.Area_Body.Padding = new System.Windows.Forms.Padding(3, 2, 2, 2);
            this.Area_Body.Size = new System.Drawing.Size(581, 408);
            this.Area_Body.TabIndex = 1;
            // 
            // Area_InnerBody
            // 
            this.Area_InnerBody.BackColor = System.Drawing.Color.White;
            this.Area_InnerBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Area_InnerBody.Location = new System.Drawing.Point(3, 2);
            this.Area_InnerBody.Name = "Area_InnerBody";
            this.Area_InnerBody.Size = new System.Drawing.Size(576, 404);
            this.Area_InnerBody.TabIndex = 0;
            // 
            // Area_TabController
            // 
            this.Area_TabController.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Area_TabController.Controls.Add(this.Area_TabControllerInnerBox);
            this.Area_TabController.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_TabController.Location = new System.Drawing.Point(0, 0);
            this.Area_TabController.Name = "Area_TabController";
            this.Area_TabController.Size = new System.Drawing.Size(30, 408);
            this.Area_TabController.TabIndex = 0;
            // 
            // Area_TabControllerInnerBox
            // 
            this.Area_TabControllerInnerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Area_TabControllerInnerBox.Location = new System.Drawing.Point(0, 0);
            this.Area_TabControllerInnerBox.Name = "Area_TabControllerInnerBox";
            this.Area_TabControllerInnerBox.ScrollOrientation = Common_Winform.Enums.OrientationEnum.Vertical;
            this.Area_TabControllerInnerBox.Size = new System.Drawing.Size(30, 408);
            this.Area_TabControllerInnerBox.SizeCalcCorrection = 5;
            this.Area_TabControllerInnerBox.TabIndex = 0;
            // 
            // TabPageLayout01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 408);
            this.Controls.Add(this.Area_Body);
            this.Controls.Add(this.Area_TabController);
            this.Name = "TabPageLayout01";
            this.Area_Body.ResumeLayout(false);
            this.Area_TabController.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        protected Panel Area_Body;
        private Panel Area_InnerBody;
        protected Panel Area_TabController;
        private Container.ScrollPanel Area_TabControllerInnerBox;
    }
}
