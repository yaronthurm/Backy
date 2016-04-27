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
    public partial class MoreOptions : Form
    {
        private OSFileSystem _fileSystem = new OSFileSystem();

        public MoreOptions()
        {
            InitializeComponent();

            this.multiStepProgress1.BackColor = SystemColors.Control;
            this.richTextBox1.BackColor = SystemColors.Control;
        }

        private void btnBrowseFoldersTarget_Click(object sender, EventArgs e)
        {
            var selectedTarget = UIUtils.ChooseAnyFolder();
            if (selectedTarget == null) return;

            this.backupTargetView1.SetDirectory(selectedTarget);

            var settings = BackyLogic.Settings.Load();

            if (!Directory.Exists(settings.Target))
            {
                this.richTextBox1.Clear();
                this.richTextBox1.AppendText($"Cannot calculate diff since source directoty '{settings.Target}' does not exist");
                return;
            }

            var diff = BackupTheBackupDiff.Calculate(settings.Target, selectedTarget, _fileSystem);

            this.richTextBox1.Clear();
            if (diff.Missing.Any())
            {
                foreach (var missing in diff.Missing)
                {
                    this.richTextBox1.SelectionColor = Color.Red;
                    this.richTextBox1.AppendText($"{missing.OriginalSource} is missing\n");
                }
            }

            if (diff.Existing.Any())
            {                
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("Existing directories:\n");
                this.richTextBox1.SelectionFont = this.Font;
                foreach (var existing in diff.Existing)
                {
                    if (existing.MissingDirectories.Any())
                    {
                        this.richTextBox1.SelectionColor = Color.DarkOrange;
                        this.richTextBox1.AppendText($"{existing.Directory.OriginalSource} is missing some versions: {existing.MissingDirectories.ToCommaDelimited()} \n");
                    }
                    else
                    {
                        this.richTextBox1.SelectionColor = Color.Green;
                        this.richTextBox1.AppendText($"{existing.Directory.OriginalSource} is up to date\n");
                    }
                }                
            }

            this.btnRunBackupTheBackup.Enabled = true;
        }

        private void MoreOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private async void btnRunBackupTheBackup_Click(object sender, EventArgs e)
        {
            var settings = BackyLogic.Settings.Load();
            if (!Directory.Exists(settings.Target))
            {
                MessageBox.Show("Source directory does not exist. Cannot proceed");
                return;
            }
            if (!Directory.Exists(this.backupTargetView1.Path))
            {
                MessageBox.Show("Destination directory does not exist. Cannot proceed");
                return;
            }


            var cmd = new BackupTheBackupCommand(_fileSystem, settings.Target, this.backupTargetView1.Path);
            cmd.Progress = this.multiStepProgress1;
            await Task.Run(() => cmd.Execute());            
        }
    }
}
