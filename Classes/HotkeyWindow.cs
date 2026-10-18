using System;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    /// <summary>
    /// 接收 WM_HOTKEY 的隐藏窗口。
    /// 旧版本用 WH_KEYBOARD_LL 全局键盘钩子自己判键，问题有三：
    ///   1) 给系统里每一次按键都加了延迟；
    ///   2) 打开 CapsLock 时组合键失效（键码里混进了 CapsLock 位）；
    ///   3) Win+字母 组合永远匹配不上。
    /// 改用 RegisterHotKey 后由系统负责匹配，既准确又不影响打字手感。
    /// </summary>
    internal sealed class HotkeyWindow : NativeWindow
    {
        private const int HotkeyId = 0x4A0F;

        private bool registered;

        public event EventHandler HotkeyPressed;

        public HotkeyWindow()
        {
            CreateParams cp = new CreateParams();
            // 必须指定窗口类名：CreateParams.ClassName 为 null 时 CreateWindowEx 直接失败，
            // Handle 变成 IntPtr.Zero，RegisterHotKey 随即退化成“线程级”注册，
            // WM_HOTKEY 被投递到没有窗口过程的线程队列里——按快捷键毫无反应。
            cp.ClassName = "STATIC";
            cp.Caption = "AlwaysOnTop_HotkeyReceiver";
            cp.Style = unchecked((int)0x80000000);   // WS_POPUP
            cp.ExStyle = Native.WS_EX_TOOLWINDOW;    // 不进 Alt+Tab
            cp.X = -32000;
            cp.Y = -32000;
            cp.Width = 1;
            cp.Height = 1;
            CreateHandle(cp);

            if (Handle == IntPtr.Zero)
            {
                Program.WriteLog(new InvalidOperationException("热键接收窗口创建失败，全局快捷键不可用。"));
            }
        }

        public bool IsRegistered
        {
            get { return registered; }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            {
                EventHandler handler = HotkeyPressed;
                if (handler != null)
                {
                    try
                    {
                        handler(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        Program.WriteLog(ex);
                    }
                }
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>注册热键；组合已被其他程序占用时返回 false。</summary>
        public bool Register(uint modifiers, uint virtualKey)
        {
            Unregister();

            if (Handle == IntPtr.Zero)
            {
                registered = false;
                return false;
            }

            registered = Native.RegisterHotKey(Handle, HotkeyId, modifiers | Native.MOD_NOREPEAT, virtualKey);
            return registered;
        }

        public void Unregister()
        {
            if (!registered) return;
            try
            {
                Native.UnregisterHotKey(Handle, HotkeyId);
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
            registered = false;
        }

        /// <summary>释放热键并销毁隐藏窗口。</summary>
        public void Cleanup()
        {
            Unregister();
            if (Handle != IntPtr.Zero)
            {
                try
                {
                    DestroyHandle();
                }
                catch (Exception ex)
                {
                    Program.WriteLog(ex);
                }
            }
        }
    }
}
