﻿namespace Common_Winform.Controls.FeatureGroup
{
    partial class WaitingInfoBox
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
            this.components = new System.ComponentModel.Container();
            this.InnerTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // InnerTimer
            // 
            this.InnerTimer.Enabled = true;
            this.InnerTimer.Interval = 200;
            this.InnerTimer.Tick += new System.EventHandler(this.InnerTimer_Tick);
            // 
            // WaitingInfoBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Name = "WaitingInfoBox";
            this.Size = new System.Drawing.Size(345, 76);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer InnerTimer;
    }
}
