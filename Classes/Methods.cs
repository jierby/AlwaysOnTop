using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AlwaysOnTop.Classes
{
    #region 窗口工具
    /// <summary>
    /// 窗口相关操作。
    /// 这里不用“标题字符串”去全进程匹配窗口，而是直接持有窗口句柄(HWND)，
    /// 这样即使标题被程序改动（浏览器、编辑器会不停改标题），也仍然能准确取消置顶。
    /// </summary>
    internal static class WindowTools
    {
        /// <summary>置顶后追加到标题栏的标记（可关闭）。</summary>
        public const string Marker = " [置顶]";

        private static readonly string[] ShellClassNames = new string[]
        {
            "Progman",                  // 桌面
            "WorkerW",                  // 桌面（壁纸层）
            "Shell_TrayWnd",            // 任务栏
            "Shell_SecondaryTrayWnd",   // 副屏任务栏
            "NotifyIconOverflowWindow", // 托盘溢出窗口
            "Windows.UI.Core.CoreWindow",
            "ApplicationManager_DesktopShellWindow"
        };

        /// <summary>安全地读取窗口标题（窗口无标题时返回空串，绝不会卡死）。</summary>
        public static string GetTitle(IntPtr hwnd)
        {
            try
            {
                if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd)) return string.Empty;
                int len = Native.GetWindowTextLength(hwnd);
                if (len <= 0) return string.Empty;
                StringBuilder sb = new StringBuilder(len + 2);
                int copied = Native.GetWindowText(hwnd, sb, sb.Capacity);
                if (copied <= 0) return string.Empty;
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetClassName(IntPtr hwnd)
        {
            try
            {
                StringBuilder sb = new StringBuilder(256);
                Native.GetClassName(hwnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>窗口当前是否处于置顶状态（直接读扩展样式，不依赖标题）。</summary>
        public static bool IsTopmost(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            int exStyle = Native.GetWindowLong(hwnd, Native.GWL_EXSTYLE);
            return (exStyle & Native.WS_EX_TOPMOST) != 0;
        }

        /// <summary>
        /// 取窗口「肉眼可见」的边界。
        /// Windows 10/11 的窗口带一圈不可见的调整边框，GetWindowRect 会多出 7~8 像素，
        /// 直接用会让描边离窗口有明显缝隙，所以优先用 DWM 的扩展边框。
        /// </summary>
        public static bool GetVisibleBounds(IntPtr hwnd, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            if (hwnd == IntPtr.Zero) return false;

            try
            {
                Native.RECT frame;
                int size = Marshal.SizeOf(typeof(Native.RECT));
                if (Native.DwmGetWindowAttribute(hwnd, Native.DWMWA_EXTENDED_FRAME_BOUNDS, out frame, size) == 0
                    && frame.Width > 0 && frame.Height > 0)
                {
                    bounds = new Rectangle(frame.Left, frame.Top, frame.Width, frame.Height);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }

            try
            {
                Native.RECT rect;
                if (Native.GetWindowRect(hwnd, out rect) && rect.Width > 0 && rect.Height > 0)
                {
                    bounds = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }

            return false;
        }

        /// <summary>取窗口所在显示器的 DPI，用于按缩放比例调整描边粗细。</summary>
        public static int GetDpi(IntPtr hwnd)
        {
            try
            {
                uint dpi = Native.GetDpiForWindow(hwnd);
                if (dpi >= 48) return (int)dpi;
            }
            catch (EntryPointNotFoundException)
            {
                // Windows 10 1607 之前的系统没有这个 API，按 96 处理即可
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
            return 96;
        }

        /// <summary>
        /// 设置/取消置顶，并**回读校验**是否真的生效。
        /// 返回 false 基本只有一种原因：目标窗口权限更高（UIPI 拦截），需要以管理员身份运行本程序。
        /// </summary>
        public static bool SetTopmost(IntPtr hwnd, bool onTop)
        {
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd)) return false;

            IntPtr insertAfter = onTop ? Native.HWND_TOPMOST : Native.HWND_NOTOPMOST;
            uint flags = Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE | Native.SWP_NOOWNERZORDER;

            Native.SetWindowPos(hwnd, insertAfter, 0, 0, 0, 0, flags);
            return IsTopmost(hwnd) == onTop;
        }

        /// <summary>按需追加或剥掉标题栏标记。标题被程序改掉时保持沉默，不会写回旧标题。</summary>
        public static void ApplyMarker(IntPtr hwnd, bool on)
        {
            try
            {
                string title = GetTitle(hwnd);
                if (title.Length == 0) return;

                if (on)
                {
                    if (title.EndsWith(Marker, StringComparison.Ordinal)) return;
                    Native.SetWindowText(hwnd, title + Marker);
                }
                else
                {
                    if (!title.EndsWith(Marker, StringComparison.Ordinal)) return;
                    Native.SetWindowText(hwnd, title.Substring(0, title.Length - Marker.Length));
                }
            }
            catch
            {
                // 标题标记只是装饰，失败不影响置顶功能，直接忽略
            }
        }

        /// <summary>取当前前台窗口的顶层句柄；取不到可用窗口时返回 IntPtr.Zero。</summary>
        public static IntPtr GetForegroundAppWindow()
        {
            IntPtr hwnd = Native.GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

            IntPtr root = Native.GetAncestor(hwnd, Native.GA_ROOT);
            if (root != IntPtr.Zero) hwnd = root;

            if (IsShellWindow(hwnd)) return IntPtr.Zero;
            if (IsOwnWindow(hwnd)) return IntPtr.Zero;
            return hwnd;
        }

        /// <summary>判断是否是桌面/任务栏这类系统窗口，这些窗口不应被操作。</summary>
        public static bool IsShellWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return true;
            string cls = GetClassName(hwnd);
            for (int i = 0; i < ShellClassNames.Length; i++)
            {
                if (string.Compare(cls, ShellClassNames[i], StringComparison.OrdinalIgnoreCase) == 0) return true;
            }
            return false;
        }

        /// <summary>判断窗口是否属于本程序自己（避免把自己置顶）。</summary>
        public static bool IsOwnWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            try
            {
                uint pid;
                Native.GetWindowThreadProcessId(hwnd, out pid);
                return pid == (uint)Process.GetCurrentProcess().Id;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>把标题截断成适合放进提示条的长度。</summary>
        public static string ShortenTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return "（无标题窗口）";
            title = title.Replace("\r", " ").Replace("\n", " ").Trim();
            if (title.Length <= 42) return title;
            return title.Substring(0, 42) + "…";
        }

        #region 十字准星光标
        /// <summary>把系统箭头/文本光标临时替换成十字准星。</summary>
        public static void SetPickCursor()
        {
            try
            {
                IntPtr cross = Native.LoadCursor(IntPtr.Zero, new IntPtr(Native.IDC_CROSS));
                if (cross == IntPtr.Zero) return;
                // SetSystemCursor 会销毁传入的句柄，所以每个光标 ID 都要单独 CopyIcon
                Native.SetSystemCursor(Native.CopyIcon(cross), Native.OCR_NORMAL);
                Native.SetSystemCursor(Native.CopyIcon(cross), Native.OCR_IBEAM);
            }
            catch
            {
            }
        }

        /// <summary>从注册表重新加载系统光标方案，恢复默认指针。</summary>
        public static void RestoreCursors()
        {
            try
            {
                Native.SystemParametersInfo(Native.SPI_SETCURSORS, 0, IntPtr.Zero, 0);
            }
            catch
            {
            }
        }
        #endregion
    }
    #endregion

    #region 快捷键文本
    /// <summary>快捷键字符串（如 "Ctrl+Alt+T"）与 RegisterHotKey 参数之间的转换。</summary>
    internal static class HotkeySpec
    {
        public const string Default = "Ctrl+Alt+T";

        /// <summary>
        /// 主快捷键被占用时依次尝试的组合。
        /// 特意不含 Ctrl+Space：中文输入法几乎一定会占用它。
        /// </summary>
        public static readonly string[] FallbackCandidates = new string[]
        {
            "Ctrl+Alt+T",
            "Ctrl+Shift+T",
            "Ctrl+Alt+F12",
            "Ctrl+Shift+F12"
        };

        public static string Format(Keys modifiers, Keys key)
        {
            StringBuilder sb = new StringBuilder();
            if ((modifiers & Keys.Control) == Keys.Control) sb.Append("Ctrl+");
            if ((modifiers & Keys.Alt) == Keys.Alt) sb.Append("Alt+");
            if ((modifiers & Keys.Shift) == Keys.Shift) sb.Append("Shift+");
            sb.Append(KeyName(key));
            return sb.ToString();
        }

        public static string KeyName(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
                return ((char)('0' + (key - Keys.D0))).ToString();
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                return "NumPad" + ((int)(key - Keys.NumPad0)).ToString();
            return key.ToString();
        }

        public static bool IsModifierKey(Keys key)
        {
            if (key == Keys.Control || key == Keys.LControlKey || key == Keys.RControlKey) return true;
            if (key == Keys.Shift || key == Keys.LShiftKey || key == Keys.RShiftKey) return true;
            if (key == Keys.Alt || key == Keys.LMenu || key == Keys.RMenu) return true;
            if (key == Keys.LWin || key == Keys.RWin) return true;
            if (key == Keys.None) return true;
            return false;
        }

        /// <summary>把 "Ctrl+Alt+T" 解析成 RegisterHotKey 需要的修饰位与虚拟键码。</summary>
        public static bool TryParse(string spec, out uint modifiers, out uint virtualKey)
        {
            modifiers = 0;
            virtualKey = 0;
            if (string.IsNullOrEmpty(spec)) return false;

            string[] parts = spec.Split('+');
            if (parts.Length < 2) return false;

            Keys key = Keys.None;

            for (int i = 0; i < parts.Length; i++)
            {
                string token = parts[i].Trim();
                if (token.Length == 0) continue;

                string low = token.ToLowerInvariant();
                if (low == "ctrl" || low == "control")
                {
                    modifiers |= Native.MOD_CONTROL;
                }
                else if (low == "alt")
                {
                    modifiers |= Native.MOD_ALT;
                }
                else if (low == "shift")
                {
                    modifiers |= Native.MOD_SHIFT;
                }
                else if (low == "win" || low == "lwin" || low == "rwin")
                {
                    modifiers |= Native.MOD_WIN;
                }
                else
                {
                    key = ParseKey(token);
                }
            }

            if (modifiers == 0) return false;
            if (key == Keys.None || IsModifierKey(key)) return false;

            virtualKey = (uint)key;
            return true;
        }

        private static Keys ParseKey(string token)
        {
            // 数字键
            if (token.Length == 1 && token[0] >= '0' && token[0] <= '9')
                return (Keys)((int)Keys.D0 + (token[0] - '0'));

            // 小键盘
            if (token.Length == 7 && token.Substring(0, 6).ToLowerInvariant() == "numpad")
            {
                char c = token[6];
                if (c >= '0' && c <= '9') return (Keys)((int)Keys.NumPad0 + (c - '0'));
            }

            // 单个字母
            if (token.Length == 1)
            {
                char c = char.ToUpperInvariant(token[0]);
                if (c >= 'A' && c <= 'Z') return (Keys)((int)Keys.A + (c - 'A'));
            }

            try
            {
                return (Keys)Enum.Parse(typeof(Keys), token, true);
            }
            catch
            {
                return Keys.None;
            }
        }
    }
    #endregion

    #region 设置
    /// <summary>注册表设置读写（路径 HKCU\SOFTWARE\AlwaysOnTop，升级后旧配置不会丢）。</summary>
    internal sealed class AppSettings
    {
        public const string RegistryPath = @"SOFTWARE\AlwaysOnTop";
        public const string RunRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "AlwaysOnTop";

        /// <summary>描边默认颜色：深灰黑，在浅色窗口上清晰、在深色窗口上也不刺眼。</summary>
        public const int DefaultBorderColor = unchecked((int)0xFF1F1F1F);

        public bool RunAtLogin;
        public bool UseHotkey;
        public string Hotkey = HotkeySpec.Default;
        public bool DisableNotify;
        public bool ShowMarker;
        public bool ShowBorder = true;
        public int BorderColor = DefaultBorderColor;

        public static AppSettings Load()
        {
            AppSettings s = new AppSettings();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key == null) return s;

                    s.RunAtLogin = ReadInt(key, "Run at Login", 0) == 1;
                    s.UseHotkey = ReadInt(key, "Use Hot Key", 1) == 1;
                    s.DisableNotify = ReadInt(key, "Disable Balloon Notify", 0) == 1;
                    s.ShowMarker = ReadInt(key, "Show Topmost Marker", 0) == 1;
                    s.ShowBorder = ReadInt(key, "Show Topmost Border", 1) == 1;
                    s.BorderColor = ReadInt(key, "Topmost Border Color", DefaultBorderColor);

                    string hk = ReadString(key, "Hotkey", HotkeySpec.Default);
                    s.Hotkey = string.IsNullOrEmpty(hk) ? HotkeySpec.Default : hk;

                    // 旧版本的默认值是 Ctrl+Space，在中文系统里基本都被输入法占着，
                    // 老配置迁移过来后热键会静默失效，这里直接换成新默认值并写回。
                    if (string.Equals(s.Hotkey, "Ctrl+Space", StringComparison.OrdinalIgnoreCase))
                    {
                        s.Hotkey = HotkeySpec.Default;
                        key.SetValue("Hotkey", s.Hotkey, RegistryValueKind.String);
                    }

                    key.SetValue("Build", AppInfo.Build, RegistryValueKind.String);
                    key.SetValue("Installation Path", Application.ExecutablePath, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
            return s;
        }

        public void Save()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key == null) return;

                    key.SetValue("Run at Login", RunAtLogin ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Use Hot Key", UseHotkey ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Hotkey", Hotkey == null ? string.Empty : Hotkey, RegistryValueKind.String);
                    key.SetValue("Disable Balloon Notify", DisableNotify ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Show Topmost Marker", ShowMarker ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Show Topmost Border", ShowBorder ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Topmost Border Color", BorderColor, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
                throw;
            }
        }

        /// <summary>写入/删除开机自启动项（路径带引号，避免装在含空格的目录里起不来）。</summary>
        public static void ApplyRunAtLogin(bool enabled)
        {
            using (RegistryKey run = Registry.CurrentUser.CreateSubKey(RunRegistryPath))
            {
                if (run == null) return;

                if (enabled)
                    run.SetValue(RunValueName, "\"" + Application.ExecutablePath + "\"", RegistryValueKind.String);
                else if (run.GetValue(RunValueName) != null)
                    run.DeleteValue(RunValueName, false);
            }
        }

        private static int ReadInt(RegistryKey key, string name, int defaultValue)
        {
            object value = key.GetValue(name);
            if (value == null) return defaultValue;
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static string ReadString(RegistryKey key, string name, string defaultValue)
        {
            object value = key.GetValue(name);
            if (value == null) return defaultValue;
            string text = value as string;
            return text == null ? defaultValue : text;
        }
    }
    #endregion
}
