using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackyLogic;
using System.IO;

namespace Backy
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void btnBrowseFoldersTarget_Click(object sender, EventArgs e)
        {
            var selectedTarget = UIUtils.ChooseAnyFolder();
            if (selectedTarget == null) return;

            this.backupTargetView1.SetDirectory(selectedTarget);
        }


        public BackyLogic.Settings.Source[] GetSelectedSources()
        {
            var ret = this.addSourcesPanel1.GetSelectedSources();
            return ret;
        }

        public string GetSelectedTarget()
        {
            return this.backupTargetView1.Path;
        }

        internal void SetSettigns(BackyLogic.Settings settings)
        {
            foreach (var source in settings.Sources)
            {
                if (Directory.Exists(source.Path))
                    this.addSourcesPanel1.AddSource(source);
            }
            if (!string.IsNullOrEmpty(settings.Target) && Directory.Exists(settings.Target))
                this.backupTargetView1.SetDirectory(settings.Target);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.backupTargetView1.Path))
            {
                MessageBox.Show("Please select a target directory");
                return;
            }

            if (!this.addSourcesPanel1.GetSelectedSources().Any())
            {
                MessageBox.Show("Please select at least one source directory");
                return;
            }

            if (TargetIsContainedInAnyOfTheSource())
            {
                MessageBox.Show($"Target directory '{this.backupTargetView1.Path}' is contained within source directory '{GetAllSourcesThatContainsTarget().First()}'\nPlease select another target");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private bool TargetIsContainedInAnyOfTheSource()
        {
            var containedIn = GetAllSourcesThatContainsTarget();
            return containedIn.Any();
        }

        private IEnumerable<string> GetAllSourcesThatContainsTarget()
        {
            var ret = new List<string>();
            foreach (var source in this.addSourcesPanel1.GetSelectedSources())
            {
                if (this.backupTargetView1.Path.StartsWith(source.Path, StringComparison.OrdinalIgnoreCase))
                    yield return source.Path;
            }
        }
    }
}
