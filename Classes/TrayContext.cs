using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using AlwaysOnTop.Classes;

namespace AlwaysOnTop
{
    /// <summary>
    /// 托盘主逻辑。
    /// </summary>
    internal sealed class TrayContext : ApplicationContext
    {
        private readonly NotifyIcon tray = new NotifyIcon();
        private readonly ContextMenuStrip menu = new ContextMenuStrip();
        private readonly HotkeyWindow hotkeyWindow = new HotkeyWindow();
        private readonly Control marshaller = new Control();     // 用于把后台线程结果切回 UI 线程
        private readonly MousePicker picker;
        private readonly BorderManager borders = new BorderManager();
        private readonly List<IntPtr> pinnedWindows = new List<IntPtr>();

        private AppSettings settings;
        private ToolStripMenuItem adminItem;
        private bool handOff;
        private bool cleanedUp;
        private bool hotkeyRetried;

        public TrayContext()
        {
            IntPtr dummy = marshaller.Handle;   // 触发句柄创建，BeginInvoke 才有落脚点

            picker = new MousePicker(marshaller);
            settings = AppSettings.Load();
            borders.Configure(settings.ShowBorder, Color.FromArgb(settings.BorderColor));

            BuildTray();
            ApplyHotkey(true);

            hotkeyWindow.HotkeyPressed += HotkeyPressed;

            Application.ApplicationExit += OnApplicationExit;
        }

        #region 托盘
        private void BuildTray()
        {
            try
            {
                Stream iconStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("AlwaysOnTop.icon.ico");
                tray.Icon = iconStream != null ? new Icon(iconStream) : SystemIcons.Application;
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
                tray.Icon = SystemIcons.Application;
            }

            tray.Text = AppInfo.Name + "（" + AppInfo.EnglishName + "）";

            menu.ShowImageMargin = false;
            menu.Font = new Font("Microsoft YaHei UI", 9F);

            menu.Items.Add(CreateItem("选择窗口置顶 / 取消置顶…", PickClicked));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(CreateItem("设置…", SettingsClicked));
            menu.Items.Add(CreateItem("帮助…", HelpClicked));
            menu.Items.Add(CreateItem("关于…", AboutClicked));
            menu.Items.Add(new ToolStripSeparator());

            adminItem = CreateItem("以管理员身份重启", ElevateClicked);
            adminItem.Visible = !IsElevated();
            menu.Items.Add(adminItem);

            menu.Items.Add(CreateItem("退出", ExitClicked));

            tray.ContextMenuStrip = menu;
            tray.MouseClick += TrayMouseClick;
            tray.Visible = true;
        }

        private static ToolStripMenuItem CreateItem(string text, EventHandler handler)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Click += handler;
            return item;
        }

        private void TrayMouseClick(object sender, MouseEventArgs e)
        {
            // 左键与右键行为一致：都弹出菜单
            if (e.Button != MouseButtons.Left) return;
            ShowMenu();
        }

        private void ShowMenu()
        {
            if (adminItem != null) adminItem.Visible = !IsElevated();
            try
            {
                menu.Show(Cursor.Position);
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }
        }
        #endregion

        #region 置顶
        private void PickClicked(object sender, EventArgs e)
        {
            if (picker.IsPicking) return;

            Notify("请选择目标窗口", "把鼠标移到目标窗口上单击即可；按 Esc 或点右键取消。", ToastKind.Info);
            picker.Start(Picked, PickCancelled);
        }

        private void Picked(IntPtr hwnd)
        {
            Toggle(hwnd);
        }

        private void PickCancelled()
        {
            // 用户主动取消，无需提示
        }

        private void HotkeyPressed(object sender, EventArgs e)
        {
            IntPtr hwnd = WindowTools.GetForegroundAppWindow();
            if (hwnd == IntPtr.Zero)
            {
                Notify("没有可操作的窗口", "当前前台窗口是桌面或任务栏。", ToastKind.Warning);
                return;
            }
            Toggle(hwnd);
        }

        /// <summary>核心：按句柄切换置顶状态。</summary>
        private void Toggle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd)) return;
            if (WindowTools.IsOwnWindow(hwnd)) return;

            if (WindowTools.IsShellWindow(hwnd))
            {
                Notify("已忽略该窗口", "桌面、任务栏等系统窗口不支持置顶。", ToastKind.Warning);
                return;
            }

            bool wasTopmost = WindowTools.IsTopmost(hwnd);
            string title = WindowTools.ShortenTitle(WindowTools.GetTitle(hwnd));

            if (!WindowTools.SetTopmost(hwnd, !wasTopmost))
            {
                Notify("无法修改置顶状态",
                    "该窗口可能以管理员权限运行。请从托盘菜单选择「以管理员身份重启」后再试。",
                    ToastKind.Error);
                return;
            }

            if (settings.ShowMarker) WindowTools.ApplyMarker(hwnd, !wasTopmost);

            if (wasTopmost)
            {
                pinnedWindows.Remove(hwnd);
                borders.Detach(hwnd);
                Notify("已取消置顶", title, ToastKind.Info);
            }
            else
            {
                if (!pinnedWindows.Contains(hwnd)) pinnedWindows.Add(hwnd);
                borders.Attach(hwnd);
                Notify("已置顶", title + (settings.ShowBorder ? "（窗口已加描边）" : string.Empty), ToastKind.Success);
            }
        }

        /// <summary>退出时把所有被本程序置顶的窗口还原，避免关掉工具后窗口“永远悬在最上层”。</summary>
        private void RestorePinnedWindows()
        {
            for (int i = pinnedWindows.Count - 1; i >= 0; i--)
            {
                IntPtr hwnd = pinnedWindows[i];
                try
                {
                    if (!Native.IsWindow(hwnd)) continue;
                    WindowTools.SetTopmost(hwnd, false);
                    if (settings.ShowMarker) WindowTools.ApplyMarker(hwnd, false);
                }
                catch (Exception ex)
                {
                    Program.WriteLog(ex);
                }
            }
            pinnedWindows.Clear();
            borders.Clear();
        }
        #endregion

        #region 快捷键
        /// <summary>
        /// 注册全局快捷键。
        ///
        /// <summary>旧版本默认值是 Ctrl+Space，中文系统里几乎一定被输入法占用，
        /// 老配置迁移过来后热键会静默失效——用户按了没反应，程序也不知道。
        /// 所以这里的策略是：注册失败先延迟重试一次（应对提权重启时新旧进程抢热键），
        /// 仍失败就自动换一个可用组合并写回设置，保证快捷键一定能用。
        /// </summary>
        private void ApplyHotkey(bool quiet)
        {
            hotkeyWindow.Unregister();

            if (!settings.UseHotkey)
            {
                Program.WriteDiagnostic("全局快捷键未启用");
                return;
            }

            if (hotkeyWindow.Handle == IntPtr.Zero)
            {
                Program.WriteDiagnostic("热键接收窗口创建失败（Handle=0），快捷键不可用");
                if (!quiet) Notify("快捷键不可用", "程序内部窗口创建失败，请重启程序后再试。", ToastKind.Error);
                return;
            }

            string spec = settings.Hotkey;
            uint modifiers, virtualKey;

            if (HotkeySpec.TryParse(spec, out modifiers, out virtualKey) && hotkeyWindow.Register(modifiers, virtualKey))
            {
                Program.WriteDiagnostic("快捷键注册成功：" + spec + "（hwnd=" + hotkeyWindow.Handle + "）");
                return;
            }

            Program.WriteDiagnostic("快捷键注册失败：" + spec);

            // 「以管理员身份重启」时新旧进程会短暂共存，旧进程还占着热键，给一次延迟重试
            if (quiet && !hotkeyRetried)
            {
                hotkeyRetried = true;
                System.Windows.Forms.Timer retry = new System.Windows.Forms.Timer();
                retry.Interval = 1200;
                retry.Tick += delegate
                {
                    retry.Stop();
                    retry.Dispose();
                    settings = AppSettings.Load();
                    ApplyHotkey(false);
                };
                retry.Start();
                return;
            }

            string picked;
            if (TryFallbackHotkey(spec, out picked))
            {
                settings.Hotkey = picked;
                try { settings.Save(); }
                catch (Exception ex) { Program.WriteLog(ex); }

                Program.WriteDiagnostic("快捷键已自动改用：" + picked);
                Notify("快捷键已自动调整",
                    "「" + spec + "」被其他程序或输入法占用，已改用「" + picked + "」，可在设置中修改。",
                    ToastKind.Warning);
                return;
            }

            Program.WriteDiagnostic("所有候选快捷键均被占用");
            Notify("快捷键注册失败",
                "「" + spec + "」可能已被其他程序占用，请在设置中换一个组合。",
                ToastKind.Warning);
        }

        /// <summary>依次尝试候选组合，返回第一个注册成功的。</summary>
        private bool TryFallbackHotkey(string exclude, out string picked)
        {
            picked = null;

            string[] candidates = HotkeySpec.FallbackCandidates;
            for (int i = 0; i < candidates.Length; i++)
            {
                if (string.Equals(candidates[i], exclude, StringComparison.OrdinalIgnoreCase)) continue;

                uint modifiers, virtualKey;
                if (!HotkeySpec.TryParse(candidates[i], out modifiers, out virtualKey)) continue;
                if (!hotkeyWindow.Register(modifiers, virtualKey)) continue;

                picked = candidates[i];
                return true;
            }
            return false;
        }
        #endregion

        #region 菜单命令
        private void SettingsClicked(object sender, EventArgs e)
        {
            using (FormSettings form = new FormSettings())
            {
                form.ShowDialog();
            }

            // 设置改完立即生效，不需要重启程序
            settings = AppSettings.Load();
            ApplyHotkey(false);
            borders.Configure(settings.ShowBorder, Color.FromArgb(settings.BorderColor));
        }

        private void HelpClicked(object sender, EventArgs e)
        {
            using (FormHelp form = new FormHelp())
            {
                form.ShowDialog();
            }
        }

        private void AboutClicked(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout())
            {
                form.ShowDialog();
            }
        }

        private void ElevateClicked(object sender, EventArgs e)
        {
            // 先让出单实例锁，否则提权后的新进程会被判定为“已在运行”而直接退出
            Program.ReleaseSingleInstance();

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Application.ExecutablePath;
                info.UseShellExecute = true;
                info.Verb = "runas";     // 触发 UAC
                Process.Start(info);

                handOff = true;          // 交接给新进程，退出时不要还原已置顶的窗口
                ExitThread();
            }
            catch (Exception ex)
            {
                Program.ReacquireSingleInstance();
                Program.WriteLog(ex);
                Notify("未能以管理员身份重启", "可能是你在 UAC 提示中取消了操作。", ToastKind.Warning);
            }
        }

        private void ExitClicked(object sender, EventArgs e)
        {
            ExitThread();
        }

        private static bool IsElevated()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        private void Notify(string title, string message, ToastKind kind)
        {
            if (settings != null && settings.DisableNotify) return;
            Toast.Show(title, message, kind);
        }

        /// <summary>退出统一收尾。可重入：ExitThreadCore 与 ApplicationExit 都会调它。</summary>
        private void Cleanup()
        {
            if (cleanedUp) return;
            cleanedUp = true;

            if (!handOff)
            {
                try { RestorePinnedWindows(); } catch (Exception ex) { Program.WriteLog(ex); }
            }
            try { Toast.CloseAll(); } catch { }
            try { borders.Dispose(); } catch (Exception ex) { Program.WriteLog(ex); }
            try { picker.Dispose(); } catch (Exception ex) { Program.WriteLog(ex); }
            try { hotkeyWindow.Cleanup(); } catch (Exception ex) { Program.WriteLog(ex); }
            try { tray.Visible = false; tray.Dispose(); } catch (Exception ex) { Program.WriteLog(ex); }
            try { menu.Dispose(); } catch (Exception ex) { Program.WriteLog(ex); }
            try { marshaller.Dispose(); } catch (Exception ex) { Program.WriteLog(ex); }
        }

        protected override void ExitThreadCore()
        {
            Cleanup();
            base.ExitThreadCore();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Cleanup();
        }
    }
}
