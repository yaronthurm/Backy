﻿using BackyLogic;
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
            _viewForm = new View(_fileSystem);
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
            _backupCommand = new RunBackupCommand(_fileSystem, _settings.Sources[0], _settings.Target);
            _backupCommand.Progress = this.multiStepProgress1;

            Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishManualBackupCallback));

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
            this.radScheduled.Enabled = false;
            this.radDetection.Enabled = false;
            this.btnView.Enabled = false;
        }

        private void FinishManualBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
            this.radScheduled.Enabled = true;
            this.radDetection.Enabled = true;
            this.btnView.Enabled = true;
            this._viewForm.NotifyNewBackup();
        }

        private void FinishAutoBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.numSeconds.Value = (decimal)this.numSeconds.Tag;
            this.autoRunTimer.Enabled = true;
            this.btnView.Enabled = true;
            this.btnStartStop.Enabled = true;
            this._viewForm.NotifyNewBackup();
        }

        private void FinishDetectionBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this._onChangeDetectionCounter.Reset();
            this.btnView.Enabled = true;
            this.btnDetect.Enabled = true;
            Task.Run(() => this.WaitForFileChanges());
            this._viewForm.NotifyNewBackup();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _backupCommand.Abort();
            this.btnAbort.Enabled = false;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            _viewForm.SetDirectoriesAndShow(_settings.Target,  _settings.Sources[0]);
        }

        private void txtSource_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void txtTarget_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (this.btnStartStop.Text == "Start")
            {
                this.radManual.Enabled = false;
                this.radDetection.Enabled = false;
                this.numSeconds.Enabled = false;
                this.numSeconds.Minimum = 0;
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

        private void autoRunTimer_Tick(object sender, EventArgs e)
        {
            this.numSeconds.Value--;
            if (this.numSeconds.Value == 0)
            {
                this.autoRunTimer.Enabled = false;
                this.multiStepProgress1.Clear();
                _backupCommand = new RunBackupCommand(new OSFileSystem(), _settings.Sources[0], _settings.Target);
                _backupCommand.Progress = this.multiStepProgress1;
                Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishAutoBackupCallback));

                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnStartStop.Enabled = false;
            }
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            if (this.btnDetect.Text == "Detect")
            {
                if (!Directory.Exists(_settings.Sources[0])) return;

                this.radScheduled.Enabled = false;
                this.radManual.Enabled = false;
                this.btnDetect.Text = "Stop";
                this.multiStepProgress1.Clear();

                _watcher.Path = _settings.Sources[0];
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
                _backupCommand = new RunBackupCommand(new OSFileSystem(), _settings.Sources[0], _settings.Target);
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
            //this.btnDetect_Click(null, null);
            // Step 1: selected sources and target
            if (_settings.Sources.Any() && !string.IsNullOrEmpty(_settings.Target))
            {
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("Chosen directories:\n");
                this.richTextBox1.SelectionFont = this.Font;
                this.richTextBox1.AppendText(string.Join(";\n", _settings.Sources));
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("\nTarget:\n");
                this.richTextBox1.SelectionFont = this.Font;
                this.richTextBox1.AppendText(_settings.Target);
                this.btnSettings.Text = "Change...";
            }
            else
            {
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("You haven't yet chosen directories to backup.\n");
                this.richTextBox1.AppendText("Press setup to start.");
                this.btnSettings.Text = "Setup...";
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new Settings();
            var settings = BackyLogic.Settings.Load();
            settingsForm.SetSettigns(settings);

            var res = settingsForm.ShowDialog();
            if (res == DialogResult.Cancel) return;

            
            settings.SetSources(settingsForm.GetSelectedSources());
            settings.SetTarget(settingsForm.GetSelectedTarget());
            settings.Save();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

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
