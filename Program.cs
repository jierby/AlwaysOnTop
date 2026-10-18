/********************************************
 *	窗口置顶（AlwaysOnTop）
 *	版本说明：	0.8.2 — Windows 11 适配 + Bug 修复 + 全中文界面
 *
 *	0.8.0 新增/修复（详见 changelog.txt）：
 *	 1. 置顶后给窗口加一圈描边，颜色可在设置中指定、可整体关闭。
 *	 2. 修复热键接收窗口缺少窗口类名导致 CreateWindowEx 失败、
 *	    全局快捷键完全无响应的致命问题。
 *	 3. 旧配置里的 Ctrl+Space 与中文输入法冲突，自动迁移；被占用时自动改用备用组合。
 *	 4. 工程改为 SDK 风格 + 微软官方引用程序集包，不再需要 .NET Framework Developer Pack。
 *
 *	重写要点（0.7.0）：
 *	 1. 窗口识别由“标题字符串全进程匹配”改为“直接持有窗口句柄”，
 *	    彻底解决浏览器/编辑器改标题后无法取消置顶的问题。
 *	 2. 修复 GetWindowTitle() 死循环导致 CPU 100%。
 *	 3. 修复“点击窗口置顶”流程缺失（十字准星瞬间被还原、从未等待用户点击）。
 *	 4. 全局键盘钩子改为 RegisterHotKey 注册，去掉系统级键盘钩子带来的延迟与漏键。
 *	 5. 托盘气泡（BalloonTip）在 Win11 上常失效，改为自绘轻提示。
 *	 6. 补齐 64 位 P/Invoke 声明、DPI 感知、单实例、异常日志等基础能力。
 ********************************************/

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AlwaysOnTop
{
    /// <summary>程序信息集中定义，避免版本号散落在多处。</summary>
    internal static class AppInfo
    {
        public const string Name = "窗口置顶";
        public const string EnglishName = "AlwaysOnTop";
        public const string Version = "0.8.2";
        public const string Build = "261018.1510";
    }

    internal static class Program
    {
        private static Mutex singleInstanceMutex;

        [STAThread]
        private static void Main(string[] args)
        {
            // 便捷入口：AlwaysOnTop.exe --settings 直接打开设置窗口，
            // 不用去点托盘图标（也方便排查设置界面问题）。
            if (args != null && args.Length > 0 &&
                string.Equals(args[0], "--settings", StringComparison.OrdinalIgnoreCase))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Classes.FormSettings());
                return;
            }

            // 单实例：旧版本没有做，重复启动会装两份托盘图标、两份钩子
            bool createdNew;
            singleInstanceMutex = new Mutex(true, @"Local\AlwaysOnTop.SingleInstance", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("「" + AppInfo.Name + "」已经在运行了。" + Environment.NewLine +
                        "请在任务栏右下角的通知区域（可能收在「^」里面）查看它的图标。",
                    AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 兜底异常处理：旧版本出错时写日志到程序目录，装在 Program Files 下会因无权限再崩一次
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                Application.Run(new TrayContext());
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
            finally
            {
                GC.KeepAlive(singleInstanceMutex);
            }
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteLog(e.Exception);
        }

        /// <summary>「以管理员身份重启」前先让出单实例锁，否则新进程会以为已有实例在跑。</summary>
        internal static void ReleaseSingleInstance()
        {
            try
            {
                if (singleInstanceMutex != null) singleInstanceMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        /// <summary>提权被用户取消时，把锁抢回来继续运行。</summary>
        internal static void ReacquireSingleInstance()
        {
            try
            {
                if (singleInstanceMutex != null) singleInstanceMutex.WaitOne(0);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLog(e.ExceptionObject as Exception);
        }

        /// <summary>日志写到 %LOCALAPPDATA%\AlwaysOnTop\error.log，普通权限即可写入。</summary>
        internal static void WriteLog(Exception ex)
        {
            if (ex == null) return;
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppInfo.EnglishName);
                Directory.CreateDirectory(dir);

                using (StreamWriter writer = new StreamWriter(Path.Combine(dir, "error.log"), true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + ex.Message);
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine(new string('-', 60));
                }
            }
            catch
            {
                // 日志失败就失败，不能因此再抛异常
            }
        }

        /// <summary>
        /// 启动阶段的诊断日志（快捷键注册结果、句柄等）。
        /// 出问题时用户把 diag.log 发过来就能定位，不必再猜。
        /// </summary>
        internal static void WriteDiagnostic(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppInfo.EnglishName);
                Directory.CreateDirectory(dir);

                using (StreamWriter writer = new StreamWriter(Path.Combine(dir, "diag.log"), true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message);
                }
            }
            catch
            {
            }
        }
    }
}
