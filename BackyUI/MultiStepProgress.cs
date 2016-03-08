using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Backy
{
    public partial class MultiStepProgress : UserControl, BackyLogic.IMultiStepProgress
    {
        public MultiStepProgress()
        {
            InitializeComponent();
        }


        private List<string> _stepsText = new List<string>();

        public void ReportNewStep(string text)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<string>)this.ReportNewStep, text);
                return;
            }

            this.tableLayoutPanel1.Controls.Add(new Label {
                Text = text,
                AutoSize = true,
                AutoEllipsis = false,

            });
        }

        public void UpdateStep(string text)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<string>)this.UpdateStep, text);
                return;
            }

            var lbl = this.tableLayoutPanel1.Controls[this.tableLayoutPanel1.Controls.Count - 1];
            lbl.Text = text;
        }
    }
}