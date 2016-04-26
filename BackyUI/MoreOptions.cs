using BackyLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            
            var diff = BackupTheBackupDiff.Calculate(settings.Target, selectedTarget, _fileSystem);

            this.richTextBox1.Clear();
            if (diff.Missing.Any())
            {
                this.richTextBox1.SelectionFont = new Font(this.Font, FontStyle.Bold | FontStyle.Underline);
                this.richTextBox1.AppendText("Missing directories:\n");
                this.richTextBox1.SelectionFont = this.Font;
                this.richTextBox1.AppendText(string.Join("", diff.Missing.Select(x => x.OriginalSource + "\n")));
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
                        this.richTextBox1.AppendText($"{existing.Directory.OriginalSource} is missing: {existing.MissingDirectories.ToCommaDelimited()} \n");
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
            var cmd = new BackupTheBackupCommand(_fileSystem, settings.Target, this.backupTargetView1.Path);
            cmd.Progress = this.multiStepProgress1;
            await Task.Run(() => cmd.Execute());            
        }
    }
}
