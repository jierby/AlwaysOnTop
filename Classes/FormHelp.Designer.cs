namespace AlwaysOnTop.Classes
{
    partial class FormHelp
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormHelp));
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.lblStep2 = new System.Windows.Forms.Label();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.lblStep4 = new System.Windows.Forms.Label();
            this.lblStep5 = new System.Windows.Forms.Label();
            this.lblStep6 = new System.Windows.Forms.Label();
            this.sep = new System.Windows.Forms.Label();
            this.btnRevertArrow = new System.Windows.Forms.Button();
            this.lblRevertHint = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.Location = new System.Drawing.Point(0, 24);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(705, 51);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "使用说明";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStep1
            // 
            this.lblStep1.AutoSize = true;
            this.lblStep1.Location = new System.Drawing.Point(24, 93);
            this.lblStep1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(529, 24);
            this.lblStep1.TabIndex = 1;
            this.lblStep1.Text = "1. 单击任务栏右下角的「窗口置顶」图标（可能收在「^」里）。";
            // 
            // lblStep2
            // 
            this.lblStep2.AutoSize = true;
            this.lblStep2.Location = new System.Drawing.Point(24, 129);
            this.lblStep2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep2.Name = "lblStep2";
            this.lblStep2.Size = new System.Drawing.Size(333, 24);
            this.lblStep2.TabIndex = 2;
            this.lblStep2.Text = "2. 选择「选择窗口置顶 / 取消置顶…」。";
            // 
            // lblStep3
            // 
            this.lblStep3.AutoSize = true;
            this.lblStep3.Location = new System.Drawing.Point(24, 165);
            this.lblStep3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(408, 24);
            this.lblStep3.TabIndex = 3;
            this.lblStep3.Text = "3. 鼠标变成十字准星后，点击目标窗口即可置顶。";
            // 
            // lblStep4
            // 
            this.lblStep4.AutoSize = true;
            this.lblStep4.Location = new System.Drawing.Point(24, 201);
            this.lblStep4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep4.Name = "lblStep4";
            this.lblStep4.Size = new System.Drawing.Size(570, 24);
            this.lblStep4.TabIndex = 4;
            this.lblStep4.Text = "4. 置顶成功的窗口四周会出现一圈描边（颜色可在「设置」中修改）。";
            // 
            // lblStep5
            // 
            this.lblStep5.AutoSize = true;
            this.lblStep5.Location = new System.Drawing.Point(24, 237);
            this.lblStep5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep5.Name = "lblStep5";
            this.lblStep5.Size = new System.Drawing.Size(559, 24);
            this.lblStep5.TabIndex = 5;
            this.lblStep5.Text = "5. 也可以按快捷键（默认 Ctrl+Alt+T）切换当前活动窗口是否置顶。";
            this.lblStep5.Click += new System.EventHandler(this.lblStep5_Click);
            // 
            // lblStep6
            // 
            this.lblStep6.AutoSize = true;
            this.lblStep6.Location = new System.Drawing.Point(24, 273);
            this.lblStep6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStep6.Name = "lblStep6";
            this.lblStep6.Size = new System.Drawing.Size(588, 24);
            this.lblStep6.TabIndex = 6;
            this.lblStep6.Text = "6. 目标窗口若以管理员权限运行，请从托盘菜单「以管理员身份重启」。";
            // 
            // sep
            // 
            this.sep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.sep.Location = new System.Drawing.Point(24, 318);
            this.sep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sep.Name = "sep";
            this.sep.Size = new System.Drawing.Size(657, 3);
            this.sep.TabIndex = 7;
            // 
            // btnRevertArrow
            // 
            this.btnRevertArrow.Location = new System.Drawing.Point(24, 342);
            this.btnRevertArrow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRevertArrow.Name = "btnRevertArrow";
            this.btnRevertArrow.Size = new System.Drawing.Size(210, 45);
            this.btnRevertArrow.TabIndex = 8;
            this.btnRevertArrow.Text = "还原鼠标指针";
            this.btnRevertArrow.UseVisualStyleBackColor = true;
            this.btnRevertArrow.Click += new System.EventHandler(this.btnRevertArrow_Click);
            // 
            // lblRevertHint
            // 
            this.lblRevertHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblRevertHint.Location = new System.Drawing.Point(252, 342);
            this.lblRevertHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRevertHint.Name = "lblRevertHint";
            this.lblRevertHint.Size = new System.Drawing.Size(429, 60);
            this.lblRevertHint.TabIndex = 9;
            this.lblRevertHint.Text = "程序异常退出后指针卡在十字形？点左边按钮恢复。";
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.ForeColor = System.Drawing.Color.DimGray;
            this.lblCopyright.Location = new System.Drawing.Point(24, 423);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(290, 24);
            this.lblCopyright.TabIndex = 10;
            this.lblCopyright.Text = "Copyright © 2026 · 保留所有权利";
            // 
            // FormHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(705, 468);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lblRevertHint);
            this.Controls.Add(this.btnRevertArrow);
            this.Controls.Add(this.sep);
            this.Controls.Add(this.lblStep6);
            this.Controls.Add(this.lblStep5);
            this.Controls.Add(this.lblStep4);
            this.Controls.Add(this.lblStep3);
            this.Controls.Add(this.lblStep2);
            this.Controls.Add(this.lblStep1);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = AppIcon.Get();
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormHelp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "窗口置顶 - 帮助";
            this.Load += new System.EventHandler(this.FormHelp_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.Label lblStep2;
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.Label lblStep4;
        private System.Windows.Forms.Label lblStep5;
        private System.Windows.Forms.Label lblStep6;
        private System.Windows.Forms.Label sep;
        private System.Windows.Forms.Button btnRevertArrow;
        private System.Windows.Forms.Label lblRevertHint;
        private System.Windows.Forms.Label lblCopyright;
    }
}
