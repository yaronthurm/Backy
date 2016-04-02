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
            var selectedTarget = UIUtils.ChooseEmptyFolder();
            if (selectedTarget == null) return;

            this.backupTargetView1.SetDirectory(selectedTarget);
        }


        public string[] GetSelectedSources()
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
                this.addSourcesPanel1.AddSource(source);
            }
            if (!string.IsNullOrEmpty(settings.Target))
                this.backupTargetView1.SetDirectory(settings.Target);
        }
    }
}
