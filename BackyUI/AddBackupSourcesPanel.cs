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
                    "Choose OK to replace the contained directories with the selected directory.",
                    "",
                     MessageBoxButtons.OKCancel
                    );
                if (msgRes == DialogResult.OK)
                    ReplaceContainedDirectoriesWithContainingDirectory(selectedPath);
                return;
            }

            this.AddNewSourceUIControl(selectedPath);
        }

        public void AddSource(string source)
        {
            this.AddNewSourceUIControl(source);
        }

        public string[] GetSelectedSources()
        {
            var ret = this.GetAllBackupSourceControls().Select(x => x.Path).ToArray();
            return ret;
        }

        private void ReplaceContainedDirectoriesWithContainingDirectory(string selectedPath)
        {
            var directoriesToRemove = GetAllContainedPaths(selectedPath);
            foreach (var directoryToRemvoe in directoriesToRemove)
            {
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
                if (backupControl != null && backupControl.Path == path)
                {
                    return backupControl;
                }
            }
            throw new ApplicationException("Count not find UI control for path: " + path);
        }

        private string[] GetAllContainedPaths(string selectedPath)
        {
            // Get all existing paths that are contained in the selected path
            // e.g. c:\users\yaron\music is contained inside c:\users\yaron

            var ret = this.GetAllBackupSourceControls()
                .Select(x => x.Path)
                .Where(x => x.StartsWith(selectedPath)).ToArray();
            return ret;
        }

        private IEnumerable<BackupSourceView> GetAllBackupSourceControls()
        {
            foreach (var ctrl in this.flowLayoutPanel1.Controls)
                if (ctrl is BackupSourceView)
                    yield return (BackupSourceView)ctrl;
        }

        private bool PathContainsAnAlreadyExistingPath(string selectedPath)
        {
            var ret = this.GetAllBackupSourceControls()
                .Any(x => x.Path.StartsWith(selectedPath));
            return ret;
        }

        private bool PathIsContainedInAnExistingPath(string selectedPath)
        {
            var ret = this.GetAllBackupSourceControls()
                .Any(x => selectedPath.StartsWith(x.Path));
            return ret;
        }

        private string GetContainingPath(string selectedPath)
        {
            var ret = this.GetAllBackupSourceControls()
                .Select(x => x.Path)
                .First(x => selectedPath.StartsWith(x));
            return ret;
        }

        private bool PathAlreadyAdded(string selectedPath)
        {
            var ret = this.GetAllBackupSourceControls().Any(x => x.Path == selectedPath);
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
        }
    }
}
