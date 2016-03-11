using BackyLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            this.btnRun.Enabled = this.radManual.Checked;
            this.btnStartStop.Enabled = this.radScheduled.Checked;
            this.numSeconds.Enabled = this.radScheduled.Checked;
            this.btnDetect.Enabled = this.radDetection.Checked;
            this.numDetectionAggregationTime.Enabled = this.radDetection.Checked;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();
            _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
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
            this.btnView.Enabled = true;
        }

        private void FinishAutoBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.numSeconds.Value = (decimal)this.numSeconds.Tag;
            this.autoRunTimer.Enabled = true;
            this.btnView.Enabled = true;
            this.btnStartStop.Enabled = true;
        }

        private void FinishDetectionBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.numDetectionAggregationTime.Value = (decimal)this.numDetectionAggregationTime.Tag;
            this.btnView.Enabled = true;
            this.btnDetect.Enabled = true;
            Task.Run(() => this.WaitForFileChenges());
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _backupCommand.Abort();
            this.btnAbort.Enabled = false;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            var viewForm = new View(new FileSystem(), this.txtTarget.Text, this.txtSource.Text);
            viewForm.Show();
        }

        private void btnBrowseFoldersSource_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = false;
            folderBrowser.SelectedPath = this.txtSource.Text;
            var result = folderBrowser.ShowDialog();
            this.txtSource.Text = folderBrowser.SelectedPath;
            Properties.Settings.Default.Save();
        }

        private void btnBrowseFoldersTarget_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = true;
            folderBrowser.SelectedPath = this.txtTarget.Text;
            var result = folderBrowser.ShowDialog();
            this.txtTarget.Text = folderBrowser.SelectedPath;
            Properties.Settings.Default.Save();
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
                _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
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
                if (!Directory.Exists(this.txtSource.Text)) return;

                this.radScheduled.Enabled = false;
                this.radManual.Enabled = false;
                this.numDetectionAggregationTime.Enabled = false;
                this.numDetectionAggregationTime.Minimum = 0;
                this.numDetectionAggregationTime.Tag = this.numDetectionAggregationTime.Value;
                this.btnDetect.Text = "Stop";

                _watcher.Path = this.txtSource.Text;
                _watcher.EnableRaisingEvents = true;
                Task.Run(() => this.WaitForFileChenges());
            }
            else
            {
                _watcher.EnableRaisingEvents = false;
                this.changeDetectionTimer.Enabled = false;
                this.btnDetect.Text = "Detect";
                this.radScheduled.Enabled = true;
                this.radManual.Enabled = true;
                this.numDetectionAggregationTime.Enabled = true;
                this.numDetectionAggregationTime.Minimum = 10;
                this.numDetectionAggregationTime.Value = (decimal)this.numDetectionAggregationTime.Tag;
            }
        }

        private void WaitForFileChenges()
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
                this.multiStepProgress1.StartStepWithoutProgress("Change was detected");
                this.changeDetectionTimer.Enabled = true;
            }));
        }

        private void changeDetectionTimer_Tick(object sender, EventArgs e)
        {
            this.numDetectionAggregationTime.Value--;
            if (this.numDetectionAggregationTime.Value == 0)
            {
                this.changeDetectionTimer.Enabled = false;
                _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
                _backupCommand.Progress = this.multiStepProgress1;
                _detectChanges.Reset();


                this.btnAbort.Enabled = true;
                this.btnView.Enabled = false;
                this.btnDetect.Enabled = false;

                Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishDetectionBackupCallback));
            }
        }
    }
}
