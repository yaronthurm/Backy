using BackyLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class View : Form
    {
        TransientState _state;
        string _targetRootDirectory;
        string _sourceRootDirectory;

        public View(IFileSystem fileSystem, string targetRootDirectory, string sourceRootDirectory)
        {
            InitializeComponent();

            _targetRootDirectory = targetRootDirectory;
            _sourceRootDirectory = sourceRootDirectory;
           _state = new TransientState(fileSystem, _targetRootDirectory);
        }

        private void SetFiles(IEnumerable<FileView> files)
        {
            this.filesPanel1.PopulateFiles(files, _targetRootDirectory);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            this.btnNext.Enabled = true;

            var currentVersion = int.Parse(this.lblCurrentVersion.Text);
            currentVersion--;

            var backupState = _state.GetState(currentVersion);
            this.lblCurrentVersion.Text = currentVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            if (currentVersion == 1)
                this.btnPrev.Enabled = false;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.btnPrev.Enabled = true;

            var currentVersion = int.Parse(this.lblCurrentVersion.Text);
            currentVersion++;

            var backupState = _state.GetState(currentVersion);
            this.lblCurrentVersion.Text = currentVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            if (currentVersion == _state.MaxVersion)
                this.btnNext.Enabled = false;
        }

        private void View_Load(object sender, EventArgs e)
        {
            this.filesPanel1.AddContextMenuItem("Open", x => Process.Start(x.PhysicalPath));
            this.filesPanel1.AddContextMenuItem("Copy", x => Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { x.PhysicalPath }));
            this.filesPanel1.AddContextMenuItem("Restore", x => RestoreFile(x, _sourceRootDirectory));

            var backupState = _state.GetLastState();
            this.lblCurrentVersion.Text = _state.MaxVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            this.btnPrev.Enabled = _state.MaxVersion > 1;
            this.btnNext.Enabled = false;
        }

        private static void RestoreFile(FileView x, string sourceRootDirectory)
        {
            var restorePath = Path.Combine(sourceRootDirectory, x.LogicalPath);
            var restoreDir = Path.GetDirectoryName(restorePath);
            if (!Directory.Exists(restoreDir))
                Directory.CreateDirectory(restoreDir);
            File.Copy(x.PhysicalPath, restorePath, true);
        }
    }
}
