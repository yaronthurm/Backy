using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class AddBackupSourcesPanel : UserControl
    {
        public AddBackupSourcesPanel()
        {
            InitializeComponent();

            var tmpSourceDirectoryControl = new BackupSourceView();
            this.btnAdd.Size = tmpSourceDirectoryControl.Size;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = false;
            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK) {
                var sourceDirectoryControl = new BackupSourceView();
                sourceDirectoryControl.SetDirectory(folderBrowser.SelectedPath);

                this.flowLayoutPanel1.Controls.Remove(this.btnAdd);
                this.flowLayoutPanel1.Controls.Add(sourceDirectoryControl);
                this.flowLayoutPanel1.Controls.Add(this.btnAdd);
            }
        }
    }
}
