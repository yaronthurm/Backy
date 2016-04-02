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
        private List<string> _selectedDirectories = new List<string>();

        public AddBackupSourcesPanel()
        {
            InitializeComponent();

            var tmpSourceDirectoryControl = new BackupSourceView();
            this.btnAdd.Size = tmpSourceDirectoryControl.Size;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selectedPath = this.ChooseDirectoryOrNul();
            if (selectedPath == null)
                return;

            if (PathAlreadyAdded(selectedPath)) {
                MessageBox.Show($"Directory '{selectedPath}' was already added");
                return;
            }

            var containedIn = GetContainingPathOrNull(selectedPath);
            if (containedIn != null)
            {
                MessageBox.Show($"Directory '{selectedPath}' is already contained in '{containedIn}'");
                return;
            }

            this.AddNewSourceUIControl(selectedPath);
        }


        private string GetContainingPathOrNull(string selectedPath)
        {
            var ret = _selectedDirectories.FirstOrDefault(x => selectedPath.StartsWith(x));
            return ret;
        }

        private bool PathAlreadyAdded(object selectedPath)
        {
            var ret = _selectedDirectories.Contains(selectedPath);
            return ret;
        }

        private string ChooseDirectoryOrNul()
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = false;
            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
                return folderBrowser.SelectedPath;
            return null;
        }

        private void AddNewSourceUIControl(string selectedPath)
        {
            _selectedDirectories.Add(selectedPath);
            var sourceDirectoryControl = new BackupSourceView();
            sourceDirectoryControl.OnRemoveClick += SourceDirectoryControl_OnRemoveClick;
            sourceDirectoryControl.SetDirectory(selectedPath);

            this.flowLayoutPanel1.Controls.Remove(this.btnAdd);
            this.flowLayoutPanel1.Controls.Add(sourceDirectoryControl);
            this.flowLayoutPanel1.Controls.Add(this.btnAdd);

            UIUtils.ScrollToBottom(this.flowLayoutPanel1);
        }

        private void SourceDirectoryControl_OnRemoveClick(BackupSourceView source)
        {
            this.flowLayoutPanel1.Controls.Remove(source);
            _selectedDirectories.Remove(source.GetPath());
        }
    }
}
