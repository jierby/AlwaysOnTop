using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace AlwaysOnTop.Classes
{
    /// <summary>
    /// 统一提供程序图标。
    /// 说明：原工程把图标以 System.Drawing.Icon 对象序列化进各窗体的 .resx，
    /// 这种二进制资源在新的 MSBuild 资源生成器下需要额外运行时程序集；
    /// 改为直接从嵌入资源里的 icon.ico 加载，零依赖、体积更小。
    /// </summary>
    internal static class AppIcon
    {
        private const string ResourceName = "AlwaysOnTop.icon.ico";
        private static Icon cached;

        public static Icon Get()
        {
            if (cached != null) return cached;
            try
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
                if (stream != null)
                {
                    using (stream)
                    {
                        cached = new Icon(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
            }

            if (cached == null) cached = SystemIcons.Application;
            return cached;
        }
    }
}
