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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class Main : Form
    {
        private RunBackupCommand _backupCommand;
        private FileSystemWatcher _watcher = new FileSystemWatcher();
        private ManualResetEvent _detectChanges = new ManualResetEvent(false);
        private CountdownCounter _onChangeDetectionCounter = new CountdownCounter(10);
        private IFileSystem _fileSystem;
        private View _viewForm;
        private BackyLogic.Settings _settings = BackyLogic.Settings.Load();
        private CancellationTokenSource _cancelTokenSource;
        

        public Main()
        {
            InitializeComponent();

            this.radManual.CheckedChanged += Radio_CheckedChanged;
            this.radScheduled.CheckedChanged += Radio_CheckedChanged;
            this.radDetection.CheckedChanged += Radio_CheckedChanged;

            _watcher.Changed += (s1, e1) => _detectChanges.Set();
            _watcher.Created += (s1, e1) => _detectChanges.Set();
            _watcher.Deleted += (s1, e1) => _detectChanges.Set();
            _watcher.Renamed += (s1, e1) => _detectChanges.Set();
            _watcher.IncludeSubdirectories = true;

            _fileSystem = new OSFileSystem();
            _viewForm = new View(_fileSystem, _settings);

            this.multiStepProgress1.BackColor = SystemColors.Control;
            this.richTextBox1.BackColor = SystemColors.Control;
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            this.btnRun.Enabled = this.radManual.Checked;
            this.btnStartStop.Enabled = this.radScheduled.Checked;
            this.numSeconds.Enabled = this.radScheduled.Checked;
            this.btnDetect.Enabled = this.radDetection.Checked;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();
            var backupTask = GetTaskForRunningBackupOnAllActiveSources();
            Task.Run(() => backupTask.RunSynchronously()).ContinueWith(x => this.Invoke((Action)this.FinishManualBackupCallback));

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
            this.radScheduled.Enabled = false;
            this.radDetection.Enabled = false;
            this.btnView.Enabled = false;
            this.btnSettings.Enabled = false;
        }

        private void autoRunTimer_Tick(object sender, EventArgs e)
        {
            this.numSeconds.Value--;
            if (this.numSeconds.Value == 0)
            {
                this.autoRunTimer.Enabled = false;
                this.btnSettings.Enabled = false;
                this.multiStepProgress1.Clear();
                var backupTask = GetTaskForRunningBackupOnAllActiveSources();
                Task.Run(() => backupTask.RunSynchronously()).ContinueWith(x => this.Invoke((Action)this.FinishAutoBackupCallback));

                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnStartStop.Enabled = false;
            }
        }

        private Task GetTaskForRunningBackupOnAllActiveSources()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var backupCommands = _settings
                .Sources
                .Where(x => x.Enabled)
                .Select(x =>
                new RunBackupCommand(_fileSystem, x.Path, _settings.Target, _cancelTokenSource.Token) { Progress = this.multiStepProgress1 });

            if (!backupCommands.Any())
                return new Task(() => this.multiStepProgress1.StartStepWithoutProgress("There are no active sources"));

            var tasks = backupCommands.Select(x => new Task(x.Execute)).ToArray();
            var ret = new Task(() =>
            {
                foreach (var task in tasks)
                {
                    task.Start();
                    task.Wait();
                }
            });
            return ret;
        }

        private void FinishManualBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
            this.radScheduled.Enabled = true;
            this.radDetection.Enabled = true;
            this.btnView.Enabled = true;
            this.btnSettings.Enabled = true;
            this._viewForm.NotifyNewBackup();
        }

        private void FinishAutoBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.numSeconds.Value = (decimal)this.numSeconds.Tag;
            this.autoRunTimer.Enabled = true;
            this.btnView.Enabled = true;
            this.btnStartStop.Enabled = true;
            this.btnSettings.Enabled = true;
            this._viewForm.NotifyNewBackup();
        }

        private void FinishDetectionBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this._onChangeDetectionCounter.Reset();
            this.btnView.Enabled = true;
            this.btnDetect.Enabled = true;
            this.btnSettings.Enabled = true;
            Task.Run(() => this.WaitForFileChanges());
            this._viewForm.NotifyNewBackup();
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

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (this.btnStartStop.Text == "Start")
            {
                this.radManual.Enabled = false;
                this.radDetection.Enabled = false;
                this.numSeconds.Enabled = false;
                this.numSeconds.Minimum = 0;
                this.numSeconds.Value = (int)this.numSeconds.Value;
                this.numSeconds.Tag = this.numSeconds.Value;
                this.autoRunTimer.Enabled = true;
                this.btnStartStop.Text = "Cancel";
                Properties.Settings.Default.Save();
            }
            else
            {
                this.radManual.Enabled = true;
                this.radDetection.Enabled = true;
                this.numSeconds.Enabled = true;
                this.numSeconds.Minimum = 10;
                this.numSeconds.Value = (decimal)this.numSeconds.Tag;
                this.autoRunTimer.Enabled = false;
                this.btnStartStop.Text = "Start";
            }
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            if (this.btnDetect.Text == "Detect")
            {
                if (!Directory.Exists(_settings.Sources[0].Path)) return;

                this.radScheduled.Enabled = false;
                this.radManual.Enabled = false;
                this.btnDetect.Text = "Stop";
                this.multiStepProgress1.Clear();

                _watcher.Path = _settings.Sources[0].Path;
                _watcher.EnableRaisingEvents = true;
                this.multiStepProgress1.StartUnboundedStep("Running in:");
                this.multiStepProgress1.UpdateProgress(_onChangeDetectionCounter.CurrentValue);
                this.changeDetectionTimer.Start();
            }
            else
            {
                _watcher.EnableRaisingEvents = false;
                this.changeDetectionTimer.Stop();
                this.multiStepProgress1.Clear();
                this.btnDetect.Text = "Detect";
                this.radScheduled.Enabled = true;
                this.radManual.Enabled = true;
            }
        }

        private void WaitForFileChanges()
        {
            this.Invoke((Action)(() =>
                this.multiStepProgress1.StartUnboundedStep("Listening to changes:", count => TimeSpan.FromSeconds(count).ToString())
            ));

            while (_watcher.EnableRaisingEvents)
            {
                bool isSignaled = _detectChanges.WaitOne(1000);
                if (isSignaled)
                    break;
                else
                    this.Invoke((Action)(() => this.multiStepProgress1.Increment()));
            }
            if (_watcher.EnableRaisingEvents == false)
            {
                this.Invoke((Action)(() => this.multiStepProgress1.StartStepWithoutProgress("Stoped listening")));
                return;
            }
            
            this.Invoke((Action)(() =>
            {
                this.multiStepProgress1.StartUnboundedStep("Change was detected. Running backup in:");
                this.changeDetectionTimer.Enabled = true;
            }));
        }

        private void changeDetectionTimer_Tick(object sender, EventArgs e)
        {
            this._onChangeDetectionCounter.Countdown();
            this.multiStepProgress1.UpdateProgress(_onChangeDetectionCounter.CurrentValue);
            if (this._onChangeDetectionCounter.CurrentValue == 0)
            {
                this.changeDetectionTimer.Stop();
                _backupCommand = new RunBackupCommand(new OSFileSystem(), _settings.Sources[0].Path, _settings.Target);
                _backupCommand.Progress = this.multiStepProgress1;
                _detectChanges.Reset();


                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnDetect.Enabled = false;

                Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishDetectionBackupCallback));
            }
        }


        private void Main_Load(object sender, EventArgs e)
        {
            PopulateSelectedDirectories();
            Radio_CheckedChanged(null, null);
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
            _settings.Save();
            PopulateSelectedDirectories();
            _viewForm.NotifyNewBackup();
        }
    }




    public class CountdownCounter
    {
        public int InitialValue { get; private set; }
        public int CurrentValue { get; private set; }

        public CountdownCounter(int initialValue)
        {
            this.InitialValue = initialValue;
            this.CurrentValue = initialValue;
        }

        public void Countdown()
        {
            this.CurrentValue--;
        }

        public void Reset()
        {
            this.CurrentValue = this.InitialValue;
        }
    }
}
