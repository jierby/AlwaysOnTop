using System;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private void FormHelp_Load(object sender, EventArgs e)
        {
        }

        private void btnRevertArrow_Click(object sender, EventArgs e)
        {
            WindowTools.RestoreCursors();
            MessageBox.Show("鼠标指针已恢复为系统默认样式。" + Environment.NewLine +
                    "如果界面上的指针仍是十字形，把鼠标移到别的窗口上再移回来即可。",
                "还原鼠标指针", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblStep5_Click(object sender, EventArgs e)
        {

        }
    }
}
