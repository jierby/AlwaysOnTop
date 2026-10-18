using System;
using System.Windows.Forms;

namespace AlwaysOnTop.Classes
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            strVer.Text = AppInfo.Version;
            strBuild.Text = AppInfo.Build;
        }

        private void strBuild_Click(object sender, EventArgs e)
        {

        }
    }
}
