using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class RestoreTo : Form
    {
        public RestoreTo()
        {
            InitializeComponent();
        }

        private void btnBrowseFoldersRestoreTarget_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = true;
            folderBrowser.SelectedPath = this.txtRestoreTarget.Text;
            var result = folderBrowser.ShowDialog();
            this.txtRestoreTarget.Text = folderBrowser.SelectedPath;
            Properties.Settings.Default.Save();
        }
    }
}
