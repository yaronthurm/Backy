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
        private StateCalculator _state;
        private string _selectedSourceDirectory;
        private IFileSystem _fileSystem;
        private RestoreTo _restorToForm = new RestoreTo();
        private BackyLogic.Settings _setting;


        public View(IFileSystem fileSystem, BackyLogic.Settings setting)
        {
            InitializeComponent();

            _fileSystem = fileSystem;
            this.filesPanel1.AddContextMenuItem("Open", x => Process.Start(x.PhysicalPath));
            this.filesPanel1.AddContextMenuItem("Copy", x => Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { x.PhysicalPath }));
            this.filesPanel1.AddContextMenuItem("Restore", x => RestoreFile(x, _selectedSourceDirectory));

            _setting = setting;
        }




        public async Task SetDirectories()
        {
            this.filesPanel1.Clear();
            this.lblCurrentVersion.Visible = false;
            this.ResetScanCount();

            _state = new StateCalculator(_fileSystem, _setting.Target, _selectedSourceDirectory);
            _state.OnProgress += OnScanProgressHandler;
            var backupState = await Task.Run(() => _state.GetLastState());
            this.lblScanned.Visible = false;
            this.lblCurrentVersion.Text = _state.MaxVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            this.btnPrev.Enabled = _state.MaxVersion > 1;
            this.btnNext.Enabled = false;
            this.btnRestoreTo.Enabled = true;
            this.filesPanel1.Enabled = true;
            this.lblCurrentVersion.Visible = true;
        }

        public void NotifyNewBackup()
        {
            PopulateCombo();
        }


        private void ResetScanCount()
        {
            this.lblScanned.Tag = 0;
            this.lblScanned.Visible = true;
        }

        private void OnScanProgressHandler()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)OnScanProgressHandler);
                return;
            }

            int current = (int)this.lblScanned.Tag;
            current++;
            this.lblScanned.Tag = current;
            if (current % 100 == 0)
                this.lblScanned.Text = $"Files scanned: {current}";
        }

        private void SetFiles(IEnumerable<FileView> files)
        {
            this.filesPanel1.PopulateFiles(files, _setting.Target);
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

        private static void RestoreFile(FileView x, string sourceRootDirectory)
        {
            var restorePath = Path.Combine(sourceRootDirectory, x.LogicalPath);
            var restoreDir = Path.GetDirectoryName(restorePath);
            if (!Directory.Exists(restoreDir))
                Directory.CreateDirectory(restoreDir);
            File.Copy(x.PhysicalPath, restorePath, true);
        }

        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void btnRestoreTo_Click(object sender, EventArgs e)
        {
            _restorToForm.SetRestoreData(new CloneSource
            {
                BackupPath = _setting.Target,
                OriginalSourcePath = _selectedSourceDirectory
            },
            int.Parse(this.lblCurrentVersion.Text));

            _restorToForm.Show();
        }

        private void View_Load(object sender, EventArgs e)
        {
            PopulateCombo();
        }

        private void PopulateCombo()
        {
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(_setting.Sources.Select(x => x.Path).ToArray());
            this.comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedSourceDirectory = this.comboBox1.SelectedItem.ToString();
            this.SetDirectories();
        }
    }
}
