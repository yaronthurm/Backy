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
    public partial class RestoreTo : Form
    {
        private CloneSource _cloneSource;
        private int _version;
        private CloneBackupCommand _cloneCommand;

        public RestoreTo()
        {
            InitializeComponent();
        }

        public void SetRestoreData(CloneSource cloneSource, int version)
        {
            _cloneSource = cloneSource;
            _version = version;
            this.lblRestoreVersion.Text = $"Restore version: { _version }";
            this.lblRestoreSource.Text = $"Restore source: { cloneSource.BackupPath }";
            this.txtRestoreTarget.Text = "";
            this.multiStepProgress1.Clear();
        }

        private void btnBrowseFoldersRestoreTarget_Click(object sender, EventArgs e)
        {
            this.txtRestoreTarget.Text = UIUtils.ChooseEmptyFolder();
        }

        private void txtRestoreTarget_TextChanged(object sender, EventArgs e)
        {
            this.multiStepProgress1.Clear();
            ValidateRestoreDirectory();
        }

        private void ValidateRestoreDirectory()
        {
            if (this.txtRestoreTarget.Text == "")
                return;

            // Make sure the directory exists
            if (!Directory.Exists(this.txtRestoreTarget.Text))
            {
                this.SetValidationError("Directory does not exist");
                return;
            }

            // Make sure the directory is empty
            if (Directory.EnumerateFiles(this.txtRestoreTarget.Text, "*", SearchOption.AllDirectories).Any())
            {
                this.SetValidationError("Directory is not empty");
                return;
            }

            this.lblValidationMessage.Visible = false;
            this.btnRun.Enabled = true;
        }

        private void SetValidationError(string message)
        {
            this.lblValidationMessage.Text = message;
            this.lblValidationMessage.Visible = true;
            this.btnRun.Enabled = false;
        }

        private void RestoreTo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (this.btnAbort.Enabled)
                    _cloneCommand.Abort();
                this.Hide();
                e.Cancel = true;
            }
        }

        
        private void btnRun_Click(object sender, EventArgs e)
        {
            _cloneCommand = new CloneBackupCommand(new OSFileSystem(), this._cloneSource, this.txtRestoreTarget.Text, this._version);
            _cloneCommand.Progress = this.multiStepProgress1;

            Task.Run(() => _cloneCommand.Execute()).ContinueWith(x => this.Invoke((Action)this.FinishRestoreCallback));

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
        }

        private void FinishRestoreCallback()
        {
            this.btnAbort.Enabled = false;
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _cloneCommand.Abort();
        }

        private void btnOpenTarget_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(this.txtRestoreTarget.Text))
                Process.Start(this.txtRestoreTarget.Text);
        }
    }
}
