using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    internal enum ToastKind
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// 轻提示（替代托盘 BalloonTip）。
    /// Windows 11 上未打包应用的 BalloonTip 经常不显示，这里自己画一个小卡片，稳定可控。
    /// </summary>
    internal static class Toast
    {
        private static ToastForm current;

        public static void Show(string title, string message)
        {
            Show(title, message, ToastKind.Info);
        }

        public static void Show(string title, string message, ToastKind kind)
        {
            try
            {
                if (current != null && !current.IsDisposed) current.CloseNow();

                ToastForm form = new ToastForm(title, message, kind);
                current = form;
                form.ShowToast();
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
        }

        internal static void Clear(ToastForm form)
        {
            if (ReferenceEquals(current, form)) current = null;
        }

        public static void CloseAll()
        {
            try
            {
                if (current != null && !current.IsDisposed) current.CloseNow();
            }
            catch
            {
            }
            current = null;
        }
    }

    internal sealed class ToastForm : Form
    {
        private readonly Timer closeTimer;
        private bool closing;

        internal ToastForm(string title, string message, ToastKind kind)
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.FromArgb(32, 32, 34);
            ClientSize = new Size(360, 84);
            Font = new Font("Microsoft YaHei UI", 9F);
            Padding = new Padding(0);

            Panel accent = new Panel();
            accent.Dock = DockStyle.Left;
            accent.Width = 4;
            accent.BackColor = AccentColor(kind);
            Controls.Add(accent);

            Label lblTitle = new Label();
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(16, 10);
            lblTitle.Size = new Size(328, 22);
            lblTitle.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Text = title;
            lblTitle.AutoEllipsis = true;
            Controls.Add(lblTitle);

            Label lblMessage = new Label();
            lblMessage.AutoSize = false;
            lblMessage.Location = new Point(16, 34);
            lblMessage.Size = new Size(328, 42);
            lblMessage.ForeColor = Color.FromArgb(198, 198, 200);
            lblMessage.Text = message;
            Controls.Add(lblMessage);

            closeTimer = new Timer();
            closeTimer.Interval = 3200;
            closeTimer.Tick += CloseTimerTick;

            Click += AnyClick;
            accent.Click += AnyClick;
            lblTitle.Click += AnyClick;
            lblMessage.Click += AnyClick;

            ResumeLayout(false);
        }

        /// <summary>不抢焦点，避免提示条把用户正在操作的窗口切走。</summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW（不出现在 Alt+Tab）
                return cp;
            }
        }

        private static Color AccentColor(ToastKind kind)
        {
            switch (kind)
            {
                case ToastKind.Success: return Color.FromArgb(46, 160, 67);
                case ToastKind.Warning: return Color.FromArgb(210, 153, 34);
                case ToastKind.Error: return Color.FromArgb(218, 54, 51);
                default: return Color.FromArgb(56, 139, 253);
            }
        }

        internal void ShowToast()
        {
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Right - Width - 20, area.Bottom - Height - 20);
            Show();
            closeTimer.Start();
        }

        private void CloseTimerTick(object sender, EventArgs e)
        {
            CloseNow();
        }

        private void AnyClick(object sender, EventArgs e)
        {
            CloseNow();
        }

        internal void CloseNow()
        {
            if (closing) return;
            closing = true;
            try
            {
                closeTimer.Stop();
            }
            catch
            {
            }
            Toast.Clear(this);
            try
            {
                if (!IsDisposed) Close();
            }
            catch
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && closeTimer != null)
            {
                closeTimer.Stop();
                closeTimer.Dispose();
            }
            Toast.Clear(this);
            base.Dispose(disposing);
        }
    }
}
