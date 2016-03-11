using BackyLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class Main : Form
    {
        private RunBackupCommand _backupCommand;

        public Main()
        {
            InitializeComponent();

            this.radManual.CheckedChanged += Radio_CheckedChanged;
            this.radAuto.CheckedChanged += Radio_CheckedChanged;
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            this.btnRun.Enabled = this.radManual.Checked;
            this.btnStartStop.Enabled = this.radAuto.Checked;
            this.numSeconds.Enabled = this.radAuto.Checked;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();
            _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
            _backupCommand.Progress = this.multiStepProgress1;

            Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishManualBackupCallback));

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
            this.radAuto.Enabled = false;
            this.btnView.Enabled = false;
        }

        private void FinishManualBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
            this.radAuto.Enabled = true;
            this.btnView.Enabled = true;
        }

        private void FinishAutoBackupCallback()
        {
            this.btnAbort.Enabled = false;
            this.radManual.Enabled = true;
            this.numSeconds.Value = (decimal)this.numSeconds.Tag;
            this.autoRunTimer.Enabled = true;
            this.btnView.Enabled = true;
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
            }
        }
    }
}
