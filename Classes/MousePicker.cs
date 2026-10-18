using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    /// <summary>
    /// 十字准星“点选窗口”拾取器。
    ///
    /// 旧版本的置顶菜单点下去只会读一次当前前台窗口（而此时前台通常是任务栏），
    /// 所谓“点击你想置顶的窗口”这一步从未真正实现：光标刚改成十字就被还原了。
    /// 这里用低级鼠标钩子实现真正的拾取：左键点谁就操作谁，右键/Esc 取消。
    /// </summary>
    internal sealed class MousePicker : IDisposable
    {
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #region Win32
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int VK_ESCAPE = 0x1B;
        #endregion

        private readonly Control marshaller;

        private IntPtr hookHandle = IntPtr.Zero;
        private LowLevelMouseProc hookProc;      // 必须长期持有，否则委托会被 GC 回收导致钩子回调崩溃
        private Action<IntPtr> onPicked;
        private Action onCancelled;
        private Timer escapeTimer;
        private IntPtr pendingWindow = IntPtr.Zero;

        public MousePicker(Control marshaller)
        {
            this.marshaller = marshaller;
        }

        public bool IsPicking
        {
            get { return hookHandle != IntPtr.Zero; }
        }

        /// <summary>进入拾取状态。picked 会带回被点中窗口的顶层句柄。</summary>
        public void Start(Action<IntPtr> picked, Action cancelled)
        {
            if (IsPicking) return;

            onPicked = picked;
            onCancelled = cancelled;

            WindowTools.SetPickCursor();

            hookProc = new LowLevelMouseProc(HookCallback);
            hookHandle = SetWindowsHookEx(WH_MOUSE_LL, hookProc, GetModuleHandle(null), 0);

            if (hookHandle == IntPtr.Zero)
            {
                WindowTools.RestoreCursors();
                RaiseCancelled();
                return;
            }

            escapeTimer = new Timer();
            escapeTimer.Interval = 120;
            escapeTimer.Tick += EscapeTimerTick;
            escapeTimer.Start();
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int message = wParam.ToInt32();

                if (message == WM_LBUTTONDOWN)
                {
                    IntPtr target = IntPtr.Zero;
                    try
                    {
                        Native.MSLLHOOKSTRUCT info =
                            (Native.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.MSLLHOOKSTRUCT));

                        target = Native.WindowFromPoint(info.pt);
                        if (target != IntPtr.Zero)
                        {
                            IntPtr root = Native.GetAncestor(target, Native.GA_ROOT);
                            if (root != IntPtr.Zero) target = root;
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.WriteLog(ex);
                    }

                    StopPicking();

                    if (target != IntPtr.Zero)
                    {
                        pendingWindow = target;
                        // 钩子回调里不直接弹窗、改窗口，交给下一次消息循环处理，避免重入
                        try
                        {
                            marshaller.BeginInvoke(new MethodInvoker(DispatchPick));
                        }
                        catch (Exception ex)
                        {
                            Program.WriteLog(ex);
                            pendingWindow = IntPtr.Zero;
                        }
                    }
                    else
                    {
                        RaiseCancelled();
                    }

                    return new IntPtr(1); // 吞掉这次按下，避免点击穿透到目标窗口
                }

                if (message == WM_RBUTTONDOWN || message == WM_MBUTTONDOWN)
                {
                    StopPicking();
                    RaiseCancelled();
                    return new IntPtr(1);
                }
            }

            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private void EscapeTimerTick(object sender, EventArgs e)
        {
            if ((Native.GetAsyncKeyState(VK_ESCAPE) & 0x8000) == 0) return;
            StopPicking();
            RaiseCancelled();
        }

        private void DispatchPick()
        {
            Action<IntPtr> callback = onPicked;
            IntPtr target = pendingWindow;
            pendingWindow = IntPtr.Zero;
            if (callback == null) return;
            try
            {
                callback(target);
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
        }

        private void RaiseCancelled()
        {
            Action callback = onCancelled;
            if (callback == null) return;
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
        }

        private void StopPicking()
        {
            if (escapeTimer != null)
            {
                try
                {
                    escapeTimer.Stop();
                    escapeTimer.Dispose();
                }
                catch
                {
                }
                escapeTimer = null;
            }

            if (hookHandle != IntPtr.Zero)
            {
                try
                {
                    UnhookWindowsHookEx(hookHandle);
                }
                catch
                {
                }
                hookHandle = IntPtr.Zero;
            }

            WindowTools.RestoreCursors();
        }

        public void Dispose()
        {
            StopPicking();
            onPicked = null;
            onCancelled = null;
        }
    }
}
