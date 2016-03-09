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
    public partial class Form1 : Form
    {
        private RunBackupCommand _backupCommand;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();

            _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
            _backupCommand.Progress = this.multiStepProgress1;

            Task.Run(() => _backupCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishBackupCallbak));

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
        }

        private void FinishBackupCallbak()
        {
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _backupCommand.Abort();
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
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
        }

        private void btnBrowseFoldersTarget_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = true;
            folderBrowser.SelectedPath = this.txtTarget.Text;
            var result = folderBrowser.ShowDialog();
            this.txtTarget.Text = folderBrowser.SelectedPath;
        }
    }
}
