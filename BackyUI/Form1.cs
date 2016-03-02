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
        public Form1()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var backupCommand = new RunBackupCommand(new FileSystem(), this.txtSource.Text, this.txtTarget.Text);
            backupCommand.OnProgress += BackupCommand_OnProgress;
            Task.Run(() => backupCommand.Execute());
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
        }
    }
}
