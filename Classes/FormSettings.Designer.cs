namespace AlwaysOnTop.Classes
{
    partial class FormSettings
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
            this.chkRunAtLogin = new System.Windows.Forms.CheckBox();
            this.chkHotkey = new System.Windows.Forms.CheckBox();
            this.btnSetHotkey = new System.Windows.Forms.Button();
            this.lblHotkey = new System.Windows.Forms.Label();
            this.chkMarker = new System.Windows.Forms.CheckBox();
            this.chkDisableNotify = new System.Windows.Forms.CheckBox();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.btnBorderColor = new System.Windows.Forms.Button();
            this.lblBorderHint = new System.Windows.Forms.Label();
            this.sep = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkRunAtLogin
            // 
            this.chkRunAtLogin.AutoSize = true;
            this.chkRunAtLogin.Location = new System.Drawing.Point(14, 18);
            this.chkRunAtLogin.Name = "chkRunAtLogin";
            this.chkRunAtLogin.Size = new System.Drawing.Size(112, 17);
            this.chkRunAtLogin.TabIndex = 0;
            this.chkRunAtLogin.Text = "开机时自动启动";
            this.chkRunAtLogin.UseVisualStyleBackColor = true;
            this.chkRunAtLogin.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // chkHotkey
            // 
            this.chkHotkey.AutoSize = true;
            this.chkHotkey.Location = new System.Drawing.Point(14, 50);
            this.chkHotkey.Name = "chkHotkey";
            this.chkHotkey.Size = new System.Drawing.Size(110, 17);
            this.chkHotkey.TabIndex = 1;
            this.chkHotkey.Text = "启用全局快捷键";
            this.chkHotkey.UseVisualStyleBackColor = true;
            this.chkHotkey.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // btnSetHotkey
            // 
            this.btnSetHotkey.Location = new System.Drawing.Point(168, 46);
            this.btnSetHotkey.Name = "btnSetHotkey";
            this.btnSetHotkey.Size = new System.Drawing.Size(104, 26);
            this.btnSetHotkey.TabIndex = 2;
            this.btnSetHotkey.Text = "设置快捷键…";
            this.btnSetHotkey.UseVisualStyleBackColor = true;
            this.btnSetHotkey.Click += new System.EventHandler(this.btnSetHotkey_Click);
            // 
            // lblHotkey
            // 
            this.lblHotkey.AutoSize = true;
            this.lblHotkey.ForeColor = System.Drawing.Color.DimGray;
            this.lblHotkey.Location = new System.Drawing.Point(284, 52);
            this.lblHotkey.Name = "lblHotkey";
            this.lblHotkey.Size = new System.Drawing.Size(62, 17);
            this.lblHotkey.TabIndex = 3;
            this.lblHotkey.Text = "Ctrl+Alt+T";
            // 
            // chkMarker
            // 
            this.chkMarker.AutoSize = true;
            this.chkMarker.Location = new System.Drawing.Point(14, 82);
            this.chkMarker.Name = "chkMarker";
            this.chkMarker.Size = new System.Drawing.Size(168, 17);
            this.chkMarker.TabIndex = 4;
            this.chkMarker.Text = "在窗口标题栏显示 [置顶] 标记";
            this.chkMarker.UseVisualStyleBackColor = true;
            this.chkMarker.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(14, 114);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(200, 17);
            this.chkBorder.TabIndex = 5;
            this.chkBorder.Text = "置顶后给窗口加描边，便于一眼识别";
            this.chkBorder.UseVisualStyleBackColor = true;
            this.chkBorder.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // btnBorderColor
            // 
            this.btnBorderColor.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnBorderColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBorderColor.Location = new System.Drawing.Point(34, 142);
            this.btnBorderColor.Name = "btnBorderColor";
            this.btnBorderColor.Size = new System.Drawing.Size(52, 26);
            this.btnBorderColor.TabIndex = 6;
            this.btnBorderColor.Text = "";
            this.btnBorderColor.UseVisualStyleBackColor = false;
            this.btnBorderColor.Click += new System.EventHandler(this.btnBorderColor_Click);
            // 
            // lblBorderHint
            // 
            this.lblBorderHint.AutoSize = true;
            this.lblBorderHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblBorderHint.Location = new System.Drawing.Point(96, 148);
            this.lblBorderHint.Name = "lblBorderHint";
            this.lblBorderHint.Size = new System.Drawing.Size(120, 17);
            this.lblBorderHint.TabIndex = 7;
            this.lblBorderHint.Text = "描边颜色（点击色块更改）";
            // 
            // chkDisableNotify
            // 
            this.chkDisableNotify.AutoSize = true;
            this.chkDisableNotify.Location = new System.Drawing.Point(14, 182);
            this.chkDisableNotify.Name = "chkDisableNotify";
            this.chkDisableNotify.Size = new System.Drawing.Size(140, 17);
            this.chkDisableNotify.TabIndex = 8;
            this.chkDisableNotify.Text = "关闭操作提示（静默模式）";
            this.chkDisableNotify.UseVisualStyleBackColor = true;
            this.chkDisableNotify.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // sep
            // 
            this.sep.AutoSize = false;
            this.sep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.sep.Location = new System.Drawing.Point(14, 214);
            this.sep.Name = "sep";
            this.sep.Size = new System.Drawing.Size(396, 2);
            this.sep.TabIndex = 9;
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.ForeColor = System.Drawing.Color.DimGray;
            this.lblCopyright.Location = new System.Drawing.Point(14, 226);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(230, 17);
            this.lblCopyright.TabIndex = 10;
            this.lblCopyright.Text = "Copyright © 2026 · 保留所有权利";
            // 
            // btnApply
            // 
            this.btnApply.Enabled = false;
            this.btnApply.Location = new System.Drawing.Point(240, 258);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(78, 30);
            this.btnApply.TabIndex = 11;
            this.btnApply.Text = "应用";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(326, 258);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(78, 30);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(424, 302);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.sep);
            this.Controls.Add(this.chkDisableNotify);
            this.Controls.Add(this.lblBorderHint);
            this.Controls.Add(this.btnBorderColor);
            this.Controls.Add(this.chkBorder);
            this.Controls.Add(this.chkMarker);
            this.Controls.Add(this.lblHotkey);
            this.Controls.Add(this.btnSetHotkey);
            this.Controls.Add(this.chkHotkey);
            this.Controls.Add(this.chkRunAtLogin);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = AppIcon.Get();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "窗口置顶 - 设置";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkRunAtLogin;
        private System.Windows.Forms.CheckBox chkHotkey;
        private System.Windows.Forms.Button btnSetHotkey;
        private System.Windows.Forms.Label lblHotkey;
        private System.Windows.Forms.CheckBox chkMarker;
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.Button btnBorderColor;
        private System.Windows.Forms.Label lblBorderHint;
        private System.Windows.Forms.CheckBox chkDisableNotify;
        private System.Windows.Forms.Label sep;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;
    }
}
