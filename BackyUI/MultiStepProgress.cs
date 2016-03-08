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
        private enum Bounding {  None, Bounded, Unbounded};
        private Bounding _bounding;
        private string _text;
        private int _maxValue;

        public MultiStepProgress()
        {
            InitializeComponent();
        }
    

        
        public void StartUnboundedStep(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action<string>)this.StartUnboundedStep, text);
                return;
            }

            _bounding = Bounding.Unbounded;
            _text = text;
            AddProgressLabel();
        }

        public void StartBoundedStep(string text, int maxValue)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<string, int>)this.StartBoundedStep, text, maxValue);
                return;
            }

            _bounding = Bounding.Bounded;
            _maxValue = maxValue;
            _text = text;
            AddProgressLabel();
        }

        public void StartStepWithoutProgress(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action<string>)this.StartStepWithoutProgress, text);
                return;
            }

            _bounding = Bounding.None;
            _text = text;
            AddProgressLabel();
        }

        public void UpdateProgress(int currentValue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action<int>)this.UpdateProgress, currentValue);
                return;
            }

            this.SetText(currentValue);
        }



        private void AddProgressLabel()
        {
            var lbl = new Label
            {
                AutoSize = false,
                Height = 15,
                Width = this.flowLayoutPanel1.Width,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            this.flowLayoutPanel1.Controls.Add(lbl);

            this.SetText(0);
        }

        private void SetText(int currentValue)
        {
            var lbl = this.GetLastControl();
            if (_bounding == Bounding.Bounded)
                lbl.Text = $"{ _text} {currentValue}/{ _maxValue}";
            else if (_bounding == Bounding.Unbounded)
                lbl.Text = $"{ _text} {currentValue}";
            else
                lbl.Text = _text;
        }


        private void UpdateStepProgress(int finished, int total)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<int, int>)this.UpdateStepProgress, finished, total);
                return;
            }

            var lastControl = GetLastControl();
            if (lastControl is Label)
            {
                this.flowLayoutPanel1.Controls.Add(new ProgressBar
                {
                    Width = (int)(this.flowLayoutPanel1.Width * 0.9),
                    Height = 15,
                    Maximum = total,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                });
            }

            ProgressBar bar = GetLastControl() as ProgressBar;
            bar.Value = finished;
        }


        private Control GetLastControl()
        {
            var ret = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 1];
            return ret;
        }

        

        

        private Label GetProgressLabel()
        {
            var ret = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 1];
            return ret as Label;
        }

        
    }
}