using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    /// <summary>
    /// 单个被置顶窗口的描边层。
    ///
    /// 实现要点：
    /// · 用一个小号无边框窗口盖在目标窗口外沿，中间挖空成环形区域（SetWindowRgn + RGN_DIFF），
    ///   于是只有最外面几像素可见，中间完全不影响目标窗口的显示；
    /// · 处理 WM_NCHITTEST 返回 HTTRANSPARENT，鼠标点击、拖动、滚轮全部穿透到目标窗口，
    ///   不会出现「边框上点不动」的问题；
    /// · 用目标窗口句柄作为 z 序锚点（SetWindowPos 的 hWndInsertAfter），
    ///   这样描边紧贴在目标窗口之上，同时不会把其他置顶窗口压在下面。
    /// </summary>
    internal sealed class BorderOverlay : IDisposable
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = -1;

        /// <summary>描边基础厚度（96 DPI 下的像素），高 DPI 时按比例放大。</summary>
        private const double BaseThickness = 2.0;

        private readonly IntPtr target;
        private BorderForm form;
        private Color color = Color.FromArgb(unchecked((int)0xFF1F1F1F));
        private Rectangle appliedBounds = Rectangle.Empty;
        private int appliedThickness = -1;
        private bool shown;

        internal BorderOverlay(IntPtr target)
        {
            this.target = target;
        }

        internal IntPtr Target { get { return target; } }

        /// <summary>目标窗口仍然存在，且仍处于置顶状态。</summary>
        internal bool IsAlive
        {
            get { return Native.IsWindow(target) && WindowTools.IsTopmost(target); }
        }

        internal void SetColor(Color value)
        {
            if (color.ToArgb() == value.ToArgb()) return;
            color = value;
            if (form != null && !form.IsDisposed)
            {
                form.BackColor = value;
                form.Invalidate();
            }
        }

        /// <summary>
        /// 同步位置与可见性。返回 false 表示该描边已经没有意义（窗口关闭 / 不再是置顶），应当移除。
        /// </summary>
        internal bool Sync()
        {
            if (!Native.IsWindow(target)) return false;

            // 窗口被别的途径取消了置顶，描边必须撤掉，否则会有一圈“假置顶”的框悬着
            if (!WindowTools.IsTopmost(target)) return false;

            // 最小化或隐藏时不显示描边
            if (!Native.IsWindowVisible(target) || Native.IsIconic(target))
            {
                Hide();
                return true;
            }

            Rectangle visible;
            if (!WindowTools.GetVisibleBounds(target, out visible)) return true;
            if (visible.Width <= 0 || visible.Height <= 0)
            {
                Hide();
                return true;
            }

            int thickness = CurrentThickness();
            Rectangle outer = new Rectangle(
                visible.Left - thickness,
                visible.Top - thickness,
                visible.Width + thickness * 2,
                visible.Height + thickness * 2);

            EnsureForm();

            bool resized = outer != appliedBounds || thickness != appliedThickness;
            if (resized)
            {
                appliedBounds = outer;
                appliedThickness = thickness;

                // 位置尺寸交给 WinForms 同步，否则 form.Bounds 与实际窗口不一致，
                // 之后 Show() 会把窗口弹回旧位置再被下一帧拉回来，出现闪烁。
                form.Bounds = outer;
                ApplyRingRegion(outer.Width, outer.Height, thickness);
            }

            if (!shown)
            {
                // 必须用 WinForms 自己的 Show()：直接调 ShowWindow 会绕过 Visible 状态管理，
                // 窗口虽然被标成可见，但随后会被 WinForms 的一系列布局动作重新隐藏。
                // BorderForm 已重写 ShowWithoutActivation，Show 不会抢走前台焦点。
                form.Show();
                shown = true;
            }

            // z 序：SetWindowPos 的 hWndInsertAfter 语义是“插到这个窗口之后（更下层）”，
            // 直接传目标窗口只会把描边压到它下面。要贴到目标窗口上方，
            // 得取目标窗口在 z 序里的前一个窗口，把描边插到那里。
            IntPtr above = Native.GetWindow(target, Native.GW_HWNDPREV);
            if (above != form.Handle)
            {
                if (above == IntPtr.Zero) above = Native.HWND_TOPMOST;   // 目标已是最顶层
                Native.SetWindowPos(form.Handle, above, 0, 0, 0, 0,
                    Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            }

            return true;
        }

        private int CurrentThickness()
        {
            int dpi = WindowTools.GetDpi(target);
            int value = (int)Math.Round(BaseThickness * dpi / 96.0);
            return value < 1 ? 1 : value;
        }

        private void EnsureForm()
        {
            if (form != null && !form.IsDisposed) return;

            form = new BorderForm();
            form.BackColor = color;
            // 访问 Handle 会立即创建窗口；未 Show 之前它是隐藏的，由 SetWindowPos + ShowWindow 接管
            IntPtr handle = form.Handle;
            if (handle == IntPtr.Zero)
            {
                // 极少数情况下创建失败，直接放弃这一帧，下一帧重试
                form.Dispose();
                form = null;
            }
        }

        /// <summary>把窗口挖成「外框实心、内部透明」的环形，只需要重绘最外面几像素。</summary>
        private void ApplyRingRegion(int width, int height, int thickness)
        {
            if (form == null || form.IsDisposed) return;
            if (width <= thickness * 2 || height <= thickness * 2) return;

            IntPtr outer = Native.CreateRectRgn(0, 0, width, height);
            IntPtr inner = Native.CreateRectRgn(thickness, thickness, width - thickness, height - thickness);
            if (outer == IntPtr.Zero || inner == IntPtr.Zero)
            {
                if (outer != IntPtr.Zero) Native.DeleteObject(outer);
                if (inner != IntPtr.Zero) Native.DeleteObject(inner);
                return;
            }

            Native.CombineRgn(outer, outer, inner, Native.RGN_DIFF);
            Native.DeleteObject(inner);

            // SetWindowRgn 成功后区域所有权移交系统，不能再删
            Native.SetWindowRgn(form.Handle, outer, true);
        }

        private void Hide()
        {
            if (form == null || form.IsDisposed) return;
            if (!shown) return;
            form.Hide();
            shown = false;
        }

        public void Dispose()
        {
            shown = false;
            if (form != null)
            {
                try { form.Dispose(); }
                catch (Exception ex) { Program.WriteLog(ex); }
                form = null;
            }
        }

        /// <summary>纯色描边专用窗口：不抢焦点、不进 Alt+Tab、不接收任何鼠标输入。</summary>
        private sealed class BorderForm : Form
        {
            internal BorderForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                MinimizeBox = false;
                MaximizeBox = false;
                ControlBox = false;
                Text = string.Empty;
                DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE | Native.WS_EX_TOPMOST;
                    return cp;
                }
            }

            protected override void WndProc(ref Message m)
            {
                // HTTRANSPARENT：让鼠标消息穿过描边，落到下面的目标窗口上
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = new IntPtr(HTTRANSPARENT);
                    return;
                }
                base.WndProc(ref m);
            }
        }
    }

    /// <summary>
    /// 所有描边层的统一管理：一个定时器统一同步，避免每个窗口各开一个定时器。
    /// </summary>
    internal sealed class BorderManager : IDisposable
    {
        private const int SyncIntervalMs = 50;

        private readonly List<BorderOverlay> items = new List<BorderOverlay>();
        private readonly Timer timer;
        private bool enabled;
        private Color color = Color.FromArgb(unchecked((int)0xFF1F1F1F));
        private bool disposed;

        internal BorderManager()
        {
            timer = new Timer();
            timer.Interval = SyncIntervalMs;
            timer.Tick += OnTick;
        }

        internal int Count { get { return items.Count; } }

        /// <summary>应用设置：关闭时立即清空全部描边，改颜色时立即重生。</summary>
        internal void Configure(bool on, Color value)
        {
            enabled = on;
            color = value;

            if (!on)
            {
                Clear();
                return;
            }

            for (int i = 0; i < items.Count; i++) items[i].SetColor(value);
            UpdateTimer();
        }

        internal void Attach(IntPtr hwnd)
        {
            if (disposed || !enabled || hwnd == IntPtr.Zero) return;
            if (Contains(hwnd)) return;

            BorderOverlay overlay = new BorderOverlay(hwnd);
            overlay.SetColor(color);
            items.Add(overlay);

            try { overlay.Sync(); }
            catch (Exception ex) { Program.WriteLog(ex); }

            UpdateTimer();
        }

        internal void Detach(IntPtr hwnd)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].Target != hwnd) continue;
                items[i].Dispose();
                items.RemoveAt(i);
            }
            UpdateTimer();
        }

        internal void Clear()
        {
            for (int i = 0; i < items.Count; i++) items[i].Dispose();
            items.Clear();
            UpdateTimer();
        }

        private bool Contains(IntPtr hwnd)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Target == hwnd) return true;
            }
            return false;
        }

        private void UpdateTimer()
        {
            if (disposed) return;

            if (items.Count == 0)
            {
                timer.Stop();
                return;
            }
            if (!timer.Enabled) timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            bool removed = false;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                bool alive;
                try { alive = items[i].Sync(); }
                catch (Exception ex)
                {
                    Program.WriteLog(ex);
                    alive = false;
                }

                if (alive) continue;
                items[i].Dispose();
                items.RemoveAt(i);
                removed = true;
            }

            if (removed) UpdateTimer();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            timer.Stop();
            timer.Tick -= OnTick;
            timer.Dispose();

            for (int i = 0; i < items.Count; i++) items[i].Dispose();
            items.Clear();
        }
    }
}
