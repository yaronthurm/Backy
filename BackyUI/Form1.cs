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
            _backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
            _backupCommand.OnProgress += BackupCommand_OnProgress;
            Task.Run(() => _backupCommand.Execute());

            this.btnAbort.Enabled = true;
            this.btnRun.Enabled = false;
        }

        private void BackupCommand_OnProgress(BackyProgress obj)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<BackyProgress>)this.BackupCommand_OnProgress, obj);
                return;
            }

            this.progressNewFiles.Maximum = obj.NewFilesTotal;
            this.progressNewFiles.Value = obj.NewFilesFinished;
            this.lblProgressNewFiles.Text = $"{ obj.NewFilesFinished }/{ obj.NewFilesTotal   }";

            this.progressModifiedFiles.Maximum = obj.ModifiedFilesTotal;
            this.progressModifiedFiles.Value = obj.ModifiedFilesFinished;
            this.lblProgressModifiedFiles.Text = $"{ obj.ModifiedFilesFinished }/{ obj.ModifiedFilesTotal   }";

            this.progressRenameDetection.Maximum = obj.RenameDetectionTotal;
            this.progressRenameDetection.Value = obj.RenameDetectionFinish;
            this.lblProgressRenameDetection.Text = $"{ obj.RenameDetectionFinish }/{ obj.RenameDetectionTotal   }";

            this.lblSourceFilesScanned.Text = obj.SourceFileScanned.ToString();
            this.lblTargetFilesScanned.Text = obj.TargetFileScanned.ToString();

            if (obj.Done())
            {
                this.btnRun.Enabled = true;
                this.btnAbort.Enabled = false;
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _backupCommand.Abort();
            this.btnAbort.Enabled = false;
            this.btnRun.Enabled = true;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            var viewForm = new Backy.View(new FileSystem(), this.txtTarget.Text);
            viewForm.Show();
        }
    }
}
