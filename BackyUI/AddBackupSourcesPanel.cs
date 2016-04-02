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

            if (PathIsContainedInAnExistingPath(selectedPath))
            {
                var containedIn = GetContainingPath(selectedPath);
                MessageBox.Show($"Directory '{selectedPath}' is already contained in '{containedIn}'");
                return;
            }

            if (PathContainsAnAlreadyExistingPath(selectedPath))
            {
                string[] containing = GetAllContainedPaths(selectedPath);
                var msgRes = MessageBox.Show(
                    $"Directory '{selectedPath}' contains the following selected directories:\n" +
                    $"{string.Join(Environment.NewLine, containing)}\n" +
                    "Choose ok to replace the contained directories with the selected directory or cancle.",
                    "",
                     MessageBoxButtons.OKCancel
                    );
                if (msgRes == DialogResult.OK)
                    ReplaceContainedDirectoriesWithContainingDirectory(selectedPath);
                return;
            }

            this.AddNewSourceUIControl(selectedPath);
        }

        private void ReplaceContainedDirectoriesWithContainingDirectory(string selectedPath)
        {
            var directoriesToRemove = GetAllContainedPaths(selectedPath);
            foreach (var directoryToRemvoe in directoriesToRemove)
            {
                _selectedDirectories.Remove(directoryToRemvoe);
                var uiToRemove = GetUIControlByPath(directoryToRemvoe);
                this.flowLayoutPanel1.Controls.Remove(uiToRemove);
            }
            this.AddNewSourceUIControl(selectedPath);
        }

        private BackupSourceView GetUIControlByPath(string path)
        {
            foreach (var control in this.flowLayoutPanel1.Controls)
            {
                var backupControl = control as BackupSourceView;
                if (backupControl != null && backupControl.GetPath() == path)
                {
                    return backupControl;
                }
            }
            throw new ApplicationException("Count not find UI control for path: " + path);
        }

        private string[] GetAllContainedPaths(string selectedPath)
        {
            var ret = _selectedDirectories.Where(x => x.StartsWith(selectedPath)).ToArray();
            return ret;
        }

        private bool PathContainsAnAlreadyExistingPath(string selectedPath)
        {
            var ret = _selectedDirectories.Any(x => x.StartsWith(selectedPath));
            return ret;
        }

        private bool PathIsContainedInAnExistingPath(string selectedPath)
        {
            var ret = _selectedDirectories.Any(x => selectedPath.StartsWith(x));
            return ret;
        }

        private string GetContainingPath(string selectedPath)
        {
            var ret = _selectedDirectories.First(x => selectedPath.StartsWith(x));
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
