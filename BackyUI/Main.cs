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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class Main : Form
    {
        private Task _backupTask = Task.FromResult(0);
        private IFileSystem _fileSystem = new OSFileSystem();
        private View _viewForm;
        private BackupTheBackup _backupTheBackupForm = new BackupTheBackup();
        private BackyLogic.Settings _settings = BackyLogic.Settings.Load();
        private CancellationTokenSource _cancelTokenSource;
        private Dictionary<string, ManualResetFileSystemWatcher> _watchers = new Dictionary<string, ManualResetFileSystemWatcher>();
        private BlockingQueue<ManualResetFileSystemWatcher> _changesQueue = new BlockingQueue<ManualResetFileSystemWatcher>();
        private bool _listenToChanges;
        private Task _detectChangesTask = Task.FromResult(0);
        private MachineID CurrentMachineID => new MachineID { Value = _settings.MachineID };
        private DriveWatcher _driveWatcher = new DriveWatcher();

        public Main()
        {
            InitializeComponent();

            this.radManual.CheckedChanged += Radio_CheckedChanged;
            this.radScheduled.CheckedChanged += Radio_CheckedChanged;
            this.radDetection.CheckedChanged += Radio_CheckedChanged;

            _driveWatcher.NewDrivesAdded += (newDrives) =>
            {
                foreach (var drive in newDrives)
                    TestForBackyDriveAndStartBackup(drive);
            };
            _driveWatcher.Start();

            _viewForm = new View(_fileSystem, _settings.Target);

            this.multiStepProgress1.BackColor = SystemColors.Control;
            this.richTextBox1.BackColor = SystemColors.Control;
        }

        private void TestForBackyDriveAndStartBackup(DriveInfo drive)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<DriveInfo>)this.TestForBackyDriveAndStartBackup, drive);
                return;
            }

            var backyFile = new FileInfo(Path.Combine(drive.RootDirectory.FullName, "backy_drive.ini"));
            if (!backyFile.Exists)
                return;
            var pathToBackyFolder = Path.Combine(drive.RootDirectory.FullName, File.ReadAllText(backyFile.FullName));
            if (!Directory.Exists(pathToBackyFolder))
                return;

            _viewForm.NotifyNewDrive(pathToBackyFolder);
            _backupTheBackupForm.ShowAndRun(pathToBackyFolder);
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            this.btnRun.Enabled = this.radManual.Checked;
            this.btnStartStop.Enabled = this.radScheduled.Checked;
            this.numSeconds.Enabled = this.radScheduled.Checked;
            this.btnDetect.Enabled = this.radDetection.Checked;
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();
            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
            this.radScheduled.Enabled = false;
            this.radDetection.Enabled = false;
            this.btnView.Enabled = false;
            this.btnSettings.Enabled = false;

            _backupTask = RunBackupOnAllActiveSources();
            await _backupTask;

            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
            this.radScheduled.Enabled = true;
            this.radDetection.Enabled = true;
            this.btnView.Enabled = true;
            this.btnSettings.Enabled = true;
            this._viewForm.NotifyNewBackup();
        }

        private Task RunBackupOnAllActiveSources()
        {
            if (!Directory.Exists(_settings.Target))
            {
                MessageBox.Show($"Target directory '{_settings.Target}' does not exist. Please select a different target.");
                return Task.FromResult(0);
            }
            var activeSources = _settings.Sources.Where(x => x.Enabled).ToArray();
            foreach (var source in activeSources)
            {
                if (!Directory.Exists(source.Path))
                {
                    MessageBox.Show($"Source directory '{source.Path}' does not exist and was removed from sources list.");
                    _settings.SetSources(_settings.Sources.Where(x => x.Path != source.Path));
                    SaveSettingsAndRefreshView();
                    return Task.FromResult(0);
                }
            }

            _cancelTokenSource = new CancellationTokenSource();
            var backupCommands = activeSources
                .Select(x =>
                new RunBackupCommand(_fileSystem, x.Path, _settings.Target, CurrentMachineID, _cancelTokenSource.Token) { Progress = this.multiStepProgress1 });

            if (!backupCommands.Any())
                return Task.Run(() => this.multiStepProgress1.StartStepWithoutProgress("There are no active sources"));

            return Task.Run(() =>
            {
                foreach (var x in backupCommands)
                {
                    x.Execute();
                    if (x.Failures.Any())
                        MessageBox.Show(x.FailuresPretty, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _cancelTokenSource.Cancel();
            this.btnAbort.Enabled = false;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            _viewForm.Show();
            _viewForm.Focus();
        }

        private void btnStartStop_CheckedChanged(object sender, EventArgs e)
        {
            if (this.btnStartStop.Checked)
            {
                this.radManual.Enabled = false;
                this.radDetection.Enabled = false;
                this.numSeconds.Enabled = false;
                this.numSeconds.Minimum = 0;
                this.numSeconds.Value = (int)this.numSeconds.Value;
                this.numSeconds.Tag = this.numSeconds.Value;
                this.autoRunTimer.Start();
                this.btnStartStop.Text = "Cancel";
            }
            else
            {
                this.radManual.Enabled = true;
                this.radDetection.Enabled = true;
                this.numSeconds.Enabled = true;
                this.numSeconds.Minimum = 10;
                this.numSeconds.Value = (decimal)this.numSeconds.Tag;
                this.autoRunTimer.Stop();
                this.btnStartStop.Text = "Start";
            }
        }

        private async void autoRunTimer_Tick(object sender, EventArgs e)
        {
            this.numSeconds.Value--;
            if (this.numSeconds.Value == 0)
            {
                this.autoRunTimer.Enabled = false;
                this.btnSettings.Enabled = false;
                this.multiStepProgress1.Clear();
                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnStartStop.Enabled = false;

                _backupTask = RunBackupOnAllActiveSources();
                await _backupTask;

                this.btnAbort.Enabled = false;
                this.numSeconds.Value = (decimal)this.numSeconds.Tag;
                this.autoRunTimer.Enabled = true;
                this.btnView.Enabled = true;
                this.btnStartStop.Enabled = true;
                this.btnSettings.Enabled = true;
                this._viewForm.NotifyNewBackup();
            }
        }

        private async void btnDetect_CheckedChanged(object sender, EventArgs e)
        {
            if (this.btnDetect.Checked)
            {
                if (!Directory.Exists(_settings.Target))
                {
                    MessageBox.Show($"Target directory '{_settings.Target}' does not exist. Please select a different target.");
                    return;
                }

                this.multiStepProgress1.Clear();
                if (!NoActiveSource())
                {
                    this.multiStepProgress1.StartStepWithoutProgress("There are no active sources");
                    return;
                }

                this.btnAbort.Enabled = true;
                this.btnDetect.Enabled = false;
                this.radScheduled.Enabled = false;
                this.btnView.Enabled = false;
                this.btnSettings.Enabled = false;
                this.radManual.Enabled = false;
                this.btnDetect.Text = "Stop";
                this.multiStepProgress1.StartStepWithoutProgress("Run full backup once");

                StartListenToFileSystemEvents();

                _backupTask = RunBackupOnAllActiveSources();
                await _backupTask;

                this.btnDetect.Enabled = true;
                this.btnAbort.Enabled = false;
                this.btnView.Enabled = true;
                this._viewForm.NotifyNewBackup();

                _listenToChanges = true;
                _detectChangesTask = this.DetectChanges();
            }
            else
            {
                this.btnDetect.Enabled = false;

                await WaitForAllPendingTasks();
                StopListenToFileSystemEvents();
                this.multiStepProgress1.Clear();
                this.btnDetect.Text = "Run&&Detect";
                this.radScheduled.Enabled = true;
                this.radManual.Enabled = true;
                this.btnDetect.Enabled = true;
                this.btnSettings.Enabled = true;
            }
        }

        private async Task DetectChanges()
        {
            this.multiStepProgress1.StartUnboundedStep("\nListening to changes:", count => TimeSpan.FromSeconds(count).ToString());
            while (_listenToChanges)
            {
                var getChangeOrTimeout = await _changesQueue.DequeueAsync(1000);
                if (getChangeOrTimeout.Timedout)
                {
                    this.multiStepProgress1.Increment();
                    continue;
                }
                var watcher = getChangeOrTimeout.Value;
                if (!_listenToChanges) return;

                await WaitUntilChangeCanBeProcessed(watcher);

                if (!_listenToChanges) return;

                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnDetect.Enabled = false;
                this.btnSettings.Enabled = false;

                watcher.StartListen();

                if (Directory.Exists(_settings.Target) && Directory.Exists(watcher.Path))
                {
                    var backupCommand = new RunBackupCommand(_fileSystem, watcher.Path, _settings.Target, CurrentMachineID, _cancelTokenSource.Token) { Progress = this.multiStepProgress1 };
                    _backupTask = Task.Run(() => backupCommand.Execute());
                    await _backupTask;
                }
                else if (!Directory.Exists(watcher.Path))
                {
                    MessageBox.Show($"Source directory '{watcher.Path}' does not exist");
                }
                else if (!Directory.Exists(_settings.Target))
                {
                    MessageBox.Show($"Target directory '{_settings.Target}' does not exist. Please select a different target.");
                }

                this.btnAbort.Enabled = false;
                this.btnView.Enabled = true;
                this.btnDetect.Enabled = true;
                this._viewForm.NotifyNewBackup();

                this.multiStepProgress1.StartUnboundedStep("\n\nListening to changes:", count => TimeSpan.FromSeconds(count).ToString());
            }
        }

        private async Task WaitUntilChangeCanBeProcessed(ManualResetFileSystemWatcher watcher)
        {
            if (watcher.Age < 10)
            {
                var timeToWait = 10 - watcher.Age;
                this.multiStepProgress1.StartUnboundedStep($"Change was detected in '{watcher.Path}'. Running backup in:");
                while (timeToWait > 0)
                {
                    if (!_listenToChanges) return;
                    this.multiStepProgress1.UpdateProgress(timeToWait);
                    await Task.Delay(1000);
                    timeToWait--;
                }
                this.multiStepProgress1.UpdateProgress(0);
            }
        }

        private void StopListenToFileSystemEvents()
        {
            foreach (var source in _settings.Sources)
                _watchers[source.Path].StopListen();
            _changesQueue.Clear();
        }

        private async Task WaitForAllPendingTasks()
        {
            _listenToChanges = false;
            await Task.WhenAll(_detectChangesTask, _backupTask);
        }

        private void StartListenToFileSystemEvents()
        {
            foreach (var source in _settings.Sources.Where(x => x.Enabled))
                _watchers[source.Path].StartListen();
            _changesQueue.Clear();
        }

        private bool NoActiveSource()
        {
            return _settings.Sources.Any(x => x.Enabled);
        }

        



        private void Main_Load(object sender, EventArgs e)
        {
            PopulateSelectedDirectories();
            Radio_CheckedChanged(null, null);
            PopulateFileSystemWatchers();            
        }

        private void PopulateFileSystemWatchers()
        {
            foreach (var source in _settings.Sources)
            {
                if (_watchers.ContainsKey(source.Path)) continue;

                var watcher = new ManualResetFileSystemWatcher(source.Path);
                watcher.ChangeDetected += Watcher_ChangeDetected;
                _watchers[source.Path] = watcher;
            }
        }

        private void Watcher_ChangeDetected(ManualResetFileSystemWatcher source)
        {
            _changesQueue.Add(source);
        }

        private void PopulateSelectedDirectories()
        {
            this.richTextBox1.Text = "";
            if (_settings.Sources.Any() && !string.IsNullOrEmpty(_settings.Target))
            {
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("Chosen directories:\n");
                this.richTextBox1.SelectionFont = this.Font;
                this.richTextBox1.AppendText(string.Join("", _settings.Sources.Where(x => !x.Enabled).Select(x => x.Path + " (Disabled)\n")));
                this.richTextBox1.AppendText(string.Join("", _settings.Sources.Where(x => x.Enabled).Select(x => x.Path + "\n")));
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("\nTarget:\n");
                this.richTextBox1.SelectionFont = this.Font;
                this.richTextBox1.AppendText(_settings.Target);
                this.btnSettings.Text = "Change...";
            }
            else
            {
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold);
                this.richTextBox1.AppendText("You haven't yet chosen directories to backup.\n");
                this.richTextBox1.AppendText("Press setup to start.");
                this.btnSettings.Text = "Setup...";
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new Settings();
            settingsForm.SetSettigns(_settings);

            var res = settingsForm.ShowDialog();
            if (res == DialogResult.Cancel) return;

            
            _settings.SetSources(settingsForm.GetSelectedSources());
            _settings.SetTarget(settingsForm.GetSelectedTarget());
            SaveSettingsAndRefreshView();
        }

        private void SaveSettingsAndRefreshView()
        {
            _settings.Save();
            PopulateFileSystemWatchers();
            PopulateSelectedDirectories();
            _viewForm.NotifyNewBackup();
        }

        private void linkBackupTheBackup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _backupTheBackupForm.Show();
        }
    }

    public class DriveWatcher
    {
        public event Action<IEnumerable<DriveInfo>> NewDrivesAdded;

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    CheckForChanges();
                    await Task.Delay(2000);
                }
            });
        }


        private List<string> _lastDrivesList = DriveInfo.GetDrives().Where(x => x.IsReady).Select(x => x.Name).ToList();

        private void CheckForChanges()
        {            
            var currentDrives = DriveInfo.GetDrives().Where(x => x.IsReady).Select(x => x.Name).ToList();
            var addedDrives = currentDrives.Except(_lastDrivesList).ToList();
            if (addedDrives.Any())
            {
                Task.Run(() => this.NewDrivesAdded?.Invoke(addedDrives.Select(x => new DriveInfo(x))));
            }
            _lastDrivesList = currentDrives;
            
        }
    }
}
