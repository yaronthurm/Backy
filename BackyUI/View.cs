using BackyLogic;
using System;
using System.Collections.Concurrent;
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
        private bool _isLoaded;
        private StateCalculator _stateCalculator;
        private string _selectedSourceDirectory = "";
        private IFileSystem _fileSystem;
        private RestoreTo _restorToForm = new RestoreTo();
        private BackyLogic.Settings _setting;
        private ConcurrentDictionary<string, StateCalculator> _statePerSource = new ConcurrentDictionary<string, StateCalculator>();
        private Dictionary<string, string> _currentDirectoryPerSource = new Dictionary<string, string>();

        public View(IFileSystem fileSystem, BackyLogic.Settings setting)
        {
            InitializeComponent();

            _fileSystem = fileSystem;
            this.filesPanel1.AddContextMenuItem("Open", x => Process.Start(x.PhysicalPath));
            this.filesPanel1.AddContextMenuItem("Copy", x => Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { x.PhysicalPath }));
            this.filesPanel1.AddContextMenuItem("Restore", x => RestoreFile(x, _selectedSourceDirectory));

            _setting = setting;
        }


        private void ClearView()
        {
            this.filesPanel1.Clear();
            this.lblCurrentVersion.Visible = false;
            this.ResetScanCount();
        }

        private async Task SetDirectories()
        {
            this.ClearView();

            if (!_statePerSource.ContainsKey(_selectedSourceDirectory))
            {
                _stateCalculator = new StateCalculator(_fileSystem, _setting.Target, _selectedSourceDirectory);
                _stateCalculator.OnProgress += OnScanProgressHandler;
                await Task.Run(() => _stateCalculator.GetLastState());
                _statePerSource[_selectedSourceDirectory] = _stateCalculator;
            }
            _stateCalculator = _statePerSource[_selectedSourceDirectory];

            var backupState = _stateCalculator.GetLastState();
            this.lblScanned.Visible = false;
            this.lblCurrentVersion.Text = _stateCalculator.MaxVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            this.btnPrev.Enabled = _stateCalculator.MaxVersion > 1;
            this.btnNext.Enabled = false;
            this.btnRestoreTo.Enabled = true;
            this.filesPanel1.Enabled = true;
            this.lblCurrentVersion.Visible = true;
        }

        public void NotifyNewBackup()
        {
            if (_isLoaded)
            {
                _statePerSource = new ConcurrentDictionary<string, StateCalculator>();
                PopulateCombo();
            }
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

            var backupState = _stateCalculator.GetState(currentVersion);
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

            var backupState = _stateCalculator.GetState(currentVersion);
            this.lblCurrentVersion.Text = currentVersion.ToString();
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            if (currentVersion == _stateCalculator.MaxVersion)
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
            _isLoaded = true;
        }

        private void PopulateCombo()
        {
            // Reset the combo box.
            // Try to set the last selected item (it may not be there any more).

            var currentlySelectedItem = this.comboBox1.SelectedItem?.ToString();
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(
                _setting
                .Sources
                .Select(x => x.Path)
                .Where(x => StateCalculator.IsTargetForSourceExist(x, _setting.Target, _fileSystem))
                .ToArray());
            for (int i = 0; i < this.comboBox1.Items.Count; i++)
            {
                if (this.comboBox1.Items[i].ToString() == currentlySelectedItem)
                {
                    this.comboBox1.SelectedIndex = i;
                    break;
                }
            }
            if (this.comboBox1.Items.Count == 0)
                this.ClearView();
            else if (this.comboBox1.SelectedIndex == -1)
                this.comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentDirectoryPerSource[_selectedSourceDirectory] = this.filesPanel1.GetCurrentDirectory();
            _selectedSourceDirectory = this.comboBox1.SelectedItem.ToString();
            string newSourceDirectory;
            _currentDirectoryPerSource.TryGetValue(_selectedSourceDirectory, out newSourceDirectory);
            this.filesPanel1.SetCurrentDirectoty(newSourceDirectory ?? "");

            this.SetDirectories();
        }


    }
}
