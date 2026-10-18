using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    public partial class FormSettings : Form
    {
        private AppSettings settings;
        private Color borderColor = Color.FromArgb(AppSettings.DefaultBorderColor);

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            settings = AppSettings.Load();

            chkRunAtLogin.Checked = settings.RunAtLogin;
            chkHotkey.Checked = settings.UseHotkey;
            chkMarker.Checked = settings.ShowMarker;
            chkBorder.Checked = settings.ShowBorder;
            chkDisableNotify.Checked = settings.DisableNotify;
            lblHotkey.Text = settings.Hotkey;

            borderColor = Color.FromArgb(settings.BorderColor);
            ApplyColorPreview();

            // 必须是最后一步：初始化过程中触发的 CheckedChanged 不该把“应用”点亮
            btnApply.Enabled = false;
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
        }

        private void btnSetHotkey_Click(object sender, EventArgs e)
        {
            using (FormSetHotkey form = new FormSetHotkey(settings.Hotkey))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                if (string.IsNullOrEmpty(form.SelectedHotkey)) return;

                settings.Hotkey = form.SelectedHotkey;
                lblHotkey.Text = settings.Hotkey;
                chkHotkey.Checked = true;      // 会顺带点亮“应用”
                btnApply.Enabled = true;
            }
        }

        /// <summary>把选中的颜色显示成色块；深色底色配浅色描线，保证色块本身看得见。</summary>
        private void ApplyColorPreview()
        {
            btnBorderColor.BackColor = borderColor;
            btnBorderColor.FlatAppearance.BorderColor = IsDark(borderColor)
                ? Color.FromArgb(200, 200, 200)
                : Color.FromArgb(90, 90, 90);
        }

        private static bool IsDark(Color color)
        {
            int brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
            return brightness < 128;
        }

        private void btnBorderColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.FullOpen = true;                                  // 直接展开自定义颜色
                dialog.AnyColor = true;
                dialog.Color = borderColor;
                dialog.CustomColors = new int[] { AppSettings.DefaultBorderColor };

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                borderColor = dialog.Color;
                ApplyColorPreview();
                btnApply.Enabled = true;
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                settings.RunAtLogin = chkRunAtLogin.Checked;
                settings.UseHotkey = chkHotkey.Checked;
                settings.ShowMarker = chkMarker.Checked;
                settings.ShowBorder = chkBorder.Checked;
                settings.BorderColor = borderColor.ToArgb();
                settings.DisableNotify = chkDisableNotify.Checked;

                settings.Save();
                AppSettings.ApplyRunAtLogin(settings.RunAtLogin);

                btnApply.Enabled = false;
                Toast.Show("设置已保存", "描边颜色、快捷键等改动已立即生效，无需重启程序。", ToastKind.Success);
            }
            catch (Exception ex)
            {
                Program.WriteLog(ex);
                MessageBox.Show("保存设置时出错：" + Environment.NewLine + ex.Message,
                    "设置", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
