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
        private string _selectedSourceDirectory = "";
        private IFileSystem _fileSystem;
        private RestoreTo _restorToForm = new RestoreTo();
        private string _initialBackupFolder;
        private string _currentBackupFolder;
        private ConcurrentDictionary<string, StateCalculator> _stateCalculatorPerSource = new ConcurrentDictionary<string, StateCalculator>();
        private ConcurrentDictionary<string, string> _currentDirectoryPerSource = new ConcurrentDictionary<string, string>();

        public View(IFileSystem fileSystem, string backupFolder)
        {
            InitializeComponent();

            _fileSystem = fileSystem;
            this.filesPanel1.AddContextMenuItem("Open", x => Process.Start(x.PhysicalPath));
            this.filesPanel1.AddContextMenuItem("Copy", x => Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { x.PhysicalPath }));
            this.filesPanel1.AddContextMenuItem("Restore", x => RestoreFile(x, _selectedSourceDirectory));

            _initialBackupFolder = backupFolder;
            _currentBackupFolder = backupFolder;
            PopulateBackupFoldersCombo();

            this.radState.CheckedChanged += Rad_CheckedChanged;
            this.radDiff.CheckedChanged += Rad_CheckedChanged;
        }

        private void PopulateBackupFoldersCombo()
        {
            this.comboBackupFolder.Items.Clear();
            this.comboBackupFolder.Items.Add(_initialBackupFolder);
            var otherBackupFolders = DriveInfo.GetDrives().Where(x => x.IsReady)
                .Select(x => new { drive = x, iniFile = new FileInfo(Path.Combine(x.RootDirectory.FullName, "backy_drive.ini")) })
                .Where(x => x.iniFile.Exists)
                .Select(x => Path.Combine(x.drive.RootDirectory.FullName, File.ReadAllText(x.iniFile.FullName)))
                .ToArray();
            this.comboBackupFolder.Items.AddRange(otherBackupFolders);
            this.comboBackupFolder.SelectedIndex = 0;
        }

        private async void Rad_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radDiff.Checked)
                this.filesPanel1.EnableContextMenu = false;
            else
                this.filesPanel1.EnableContextMenu = true;

            this.filesPanel1.SetCurrentDirectoty("");
            var state = GetStateOrDiff((int)this.numVersion.Value);
            this.SetFiles(state.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));
        }

        private StateCalculator CurrentStateCalculator
        {
            get { return _stateCalculatorPerSource[_selectedSourceDirectory]; }
        }

        private State GetStateOrDiff(int? version)
        {
            if (version == null)
                version = CurrentStateCalculator.MaxVersion;
            if (this.radState.Checked)
                return CurrentStateCalculator.GetState(version.Value);
            else if (version > 0)
                return CurrentStateCalculator.GetDiff(version.Value);
            else
                return new State();
        }


        private void ClearView()
        {
            this.filesPanel1.Clear();
            this.numVersion.Visible = false;
            this.ResetScanCount();
        }

        private async Task SetDirectories()
        {
            this.ClearView();

            if (!_stateCalculatorPerSource.ContainsKey(_selectedSourceDirectory))
            {
                this.Enabled = false;
                this.lblDateTime.Visible = false;
                var machineID = _fileSystem.GetTopLevelDirectories(_currentBackupFolder)
                    .Where(x => BackupDirectory.IsBackupDirectory(x, _fileSystem))
                    .Select(x => BackupDirectory.FromPath(x, _fileSystem))
                    .First(x => x.OriginalSource == _selectedSourceDirectory)
                    .MachineID;
                var stateCalculator = new StateCalculator(_fileSystem, _currentBackupFolder, _selectedSourceDirectory, machineID);
                stateCalculator.OnProgress += OnScanProgressHandler;
                _stateCalculatorPerSource[_selectedSourceDirectory] = stateCalculator;
                await Task.Run(() => stateCalculator.GetLastState());
                this.comboBox1.Focus();
                this.Enabled = true;
            }

            var backupState = GetStateOrDiff(null);
            this.lblScanned.Visible = false;
            this.numVersion.Minimum = 1;
            this.numVersion.Maximum = CurrentStateCalculator.MaxVersion;
            this.numVersion.Value = CurrentStateCalculator.MaxVersion;
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));

            this.btnRestoreTo.Enabled = true;
            this.filesPanel1.Enabled = true;
            this.numVersion.Visible = true;
            this.lblDateTime.Visible = true;
        }

        public void NotifyNewBackup()
        {
            if (_isLoaded)
            {
                _stateCalculatorPerSource = new ConcurrentDictionary<string, StateCalculator>();
                PopulateCombo();
            }
        }

        public void NotifyNewDrive(string pathToBackyFolder)
        {
            PopulateBackupFoldersCombo();
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
            this.filesPanel1.PopulateFiles(files, _currentBackupFolder);
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
                BackupPath = _currentBackupFolder,
                OriginalSourcePath = _selectedSourceDirectory
            },
            (int)this.numVersion.Value);

            _restorToForm.Show();
        }

        private void View_Load(object sender, EventArgs e)
        {
            PopulateCombo();
            _isLoaded = true;
        }

        private void PopulateCombo()
        {
            if (!Directory.Exists(_currentBackupFolder))
                return;

            // Reset the combo box.
            // Try to set the last selected item (it may not be there any more).

            var currentlySelectedItem = this.comboBox1.SelectedItem?.ToString();
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(
                _fileSystem.GetTopLevelDirectories(_currentBackupFolder)
                .Where(x => BackupDirectory.IsBackupDirectory(x, _fileSystem))
                .Select(x => BackupDirectory.FromPath(x, _fileSystem).OriginalSource)
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

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentDirectoryPerSource[_selectedSourceDirectory] = this.filesPanel1.GetCurrentDirectory();
            _selectedSourceDirectory = this.comboBox1.SelectedItem.ToString();
            string newSourceDirectory;
            _currentDirectoryPerSource.TryGetValue(_selectedSourceDirectory, out newSourceDirectory);
            this.filesPanel1.SetCurrentDirectoty(newSourceDirectory ?? "");

            await this.SetDirectories();
        }

        private void comboBackupFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            if (_currentBackupFolder == this.comboBackupFolder.SelectedItem?.ToString()) return;
            _selectedSourceDirectory = "";
            _currentBackupFolder = this.comboBackupFolder.SelectedItem?.ToString();
            _stateCalculatorPerSource = new ConcurrentDictionary<string, StateCalculator>();
            _currentDirectoryPerSource = new ConcurrentDictionary<string, string>();
            PopulateCombo();
        }

        private void numVersion_ValueChanged(object sender, EventArgs e)
        {
            var backupState = GetStateOrDiff((int)this.numVersion.Value);
            this.SetFiles(backupState.GetFiles().Select(x => new FileView { PhysicalPath = x.PhysicalPath, LogicalPath = x.RelativeName }));
            var time = CurrentStateCalculator.GetDateByVersion((int)this.numVersion.Value).ToLocalTime();
            this.lblDateTime.Text = $"{time.ToShortTimeString()}   {time.ToLongDateString()}";
        }

        private void btnShallow_Click(object sender, EventArgs e)
        {
            var respond = MessageBox.Show(
                $"Turning a folder into a Shallow Folder removes all files from that folder " +
                "and will only leave a few 'marker files' - files that contain only the names of the " +
                "actual files that were removed.\n\n" +
                "Before proceeding, please make sure you have a copy of that folder elsewhere, " +
                "otherwise, you are risking loosing data that will prevent you from being able to do a complete " +
                "backup in the future.\n\n" +
                $"Are you sure you want to turn the folder of version {this.numVersion.Value} into a Shallow Folder?",
                "Confirm Shallow Folder",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (respond != DialogResult.Yes)
                return;
                       
            try
            {
                this.Enabled = false;
                var machineID = _fileSystem.GetTopLevelDirectories(_currentBackupFolder)
                        .Where(x => BackupDirectory.IsBackupDirectory(x, _fileSystem))
                        .Select(x => BackupDirectory.FromPath(x, _fileSystem))
                        .First(x => x.OriginalSource == _selectedSourceDirectory)
                        .MachineID;
                ShallowFoldersMaker.MakeFolderShallow(_fileSystem, _currentBackupFolder, _selectedSourceDirectory, machineID, (int)this.numVersion.Value);
                MessageBox.Show($"Finished turning folder of version {this.numVersion.Value} into a Shallow Folder?", "Shallow Folder", MessageBoxButtons.OK);
            }
            finally{
                this.Enabled = true;
            }
        }
    } 
}
