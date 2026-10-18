using System;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    public partial class FormSetHotkey : Form
    {
        private readonly string currentSpec;
        private bool closing;

        /// <summary>用户最终选定的组合键；为空表示尚未录入。</summary>
        public string SelectedHotkey { get; private set; }

        public FormSetHotkey()
            : this(HotkeySpec.Default)
        {
        }

        public FormSetHotkey(string current)
        {
            InitializeComponent();
            currentSpec = string.IsNullOrEmpty(current) ? HotkeySpec.Default : current;
            SelectedHotkey = currentSpec;
        }

        private void FormSetHotkey_Load(object sender, EventArgs e)
        {
            txtHotkey.Text = SelectedHotkey;
            btnOK.Enabled = !string.IsNullOrEmpty(SelectedHotkey);
        }

        /// <summary>
        /// 用 ProcessCmdKey 捕获组合键。
        /// 旧版本靠 KeyUp + 一堆 if 黑名单，Tab/Enter 收不到、修饰键组合也判不准。
        /// ProcessCmdKey 在对话框默认键处理之前触发，Tab、Enter、方向键都能拿到。
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;

            if (key == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            if (key == Keys.Back || key == Keys.Delete)
            {
                ClearSelection();
                return true;
            }

            if (HotkeySpec.IsModifierKey(key))
            {
                lblMsg.Text = "请继续按下主键，例如 T、F5 或数字键";
                return true;
            }

            Keys modifiers = Control.ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift);
            if (modifiers == Keys.None)
            {
                lblMsg.Text = "必须包含 Ctrl / Alt / Shift 中的至少一个";
                return true;
            }

            string spec = HotkeySpec.Format(modifiers, key);
            txtHotkey.Text = spec;
            SelectedHotkey = spec;
            lblMsg.Text = "";
            btnOK.Enabled = true;
            return true;
        }

        private void ClearSelection()
        {
            txtHotkey.Text = "";
            SelectedHotkey = "";
            lblMsg.Text = "已清除，请重新按下组合键";
            btnOK.Enabled = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearSelection();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedHotkey))
            {
                lblMsg.Text = "请先按下想要的组合键";
                return;
            }

            uint modifiers, virtualKey;
            if (!HotkeySpec.TryParse(SelectedHotkey, out modifiers, out virtualKey))
            {
                lblMsg.Text = "这个组合不受支持，请换一个";
                return;
            }

            // 和当前已注册的快捷键相同就不用试了（否则会被自己占用，误判为冲突）
            bool sameAsCurrent = string.Compare(SelectedHotkey, currentSpec,
                StringComparison.OrdinalIgnoreCase) == 0;

            if (!sameAsCurrent && !CanRegister(modifiers, virtualKey))
            {
                lblMsg.Text = "「" + SelectedHotkey + "」已被其他程序占用，请换一个";
                return;
            }

            DialogResult = DialogResult.OK;
            closing = true;
            Close();
        }

        /// <summary>用一个临时隐藏窗口试注册，确认组合键没被别人占用。</summary>
        private static bool CanRegister(uint modifiers, uint virtualKey)
        {
            HotkeyWindow probe = new HotkeyWindow();
            try
            {
                return probe.Register(modifiers, virtualKey);
            }
            finally
            {
                probe.Cleanup();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (closing) return;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtHotkey_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
