using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Backy
{
    public partial class MultiStepProgress : UserControl, BackyLogic.IMultiStepProgress
    {
        private enum Bounding {  None, Bounded, Unbounded};
        private Bounding _bounding;
        private string _text;
        private int _maxValue;
        private int _currentValue;
        private Func<int, string> _projection;

        public MultiStepProgress()
        {
            InitializeComponent();
        }
    

        public void Clear()
        {
            this.flowLayoutPanel1.Controls.Clear();
        }

        public void StartUnboundedStep(string text, Func<int, string> projection = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action<string, Func<int, string>>)this.StartUnboundedStep, text, projection);
                return;
            }

            _bounding = Bounding.Unbounded;
            _text = text;
            _currentValue = 0;
            _projection = projection;
            AddProgressLabel();
        }

        public void StartBoundedStep(string text, int maxValue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action<string, int>)this.StartBoundedStep, text, maxValue);
                return;
            }

            _bounding = Bounding.Bounded;
            _maxValue = maxValue;
            _text = text;
            _currentValue = 0;
            AddProgressLabel();
            AddProgressBar();
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
            _currentValue = 0;
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
            if (_bounding == Bounding.Bounded)
                this.UpdateProgressBarValue(currentValue);
        }

        public void Increment()
        {
            _currentValue++;
            this.UpdateProgress(_currentValue);
        }

        private void AddProgressBar()
        {
            this.flowLayoutPanel1.Controls.Add(new ProgressBar
            {
                Width = (int)(this.flowLayoutPanel1.Width * 0.9),
                Maximum = _maxValue,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            });

            UIUtils.ScrollToBottom(this.flowLayoutPanel1);
        }

        private void AddProgressLabel()
        {
            var lbl = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(0, 0, 0, 2)
            };
            this.flowLayoutPanel1.Controls.Add(lbl);

            this.SetText(0);

            // We want to preserve the label height (to preserve new lines)
            // but also set the width to the full width of the container (so that new labels will be in a new line)
            var h = lbl.Height;
            lbl.AutoSize = false;
            lbl.Height = h;
            lbl.Width = (int)(this.flowLayoutPanel1.Width * 0.9);

            UIUtils.ScrollToBottom(this.flowLayoutPanel1);
        }

        private void SetText(int currentValue)
        {
            var valueAsString = currentValue.ToString();
            if (_projection != null)
                valueAsString = _projection(currentValue);
            var lbl = this.GetLastLabel();
            if (_bounding == Bounding.Bounded)
                lbl.Text = $"{ _text} {currentValue}/{ _maxValue}";
            else if (_bounding == Bounding.Unbounded)
                lbl.Text = $"{ _text} {valueAsString}";
            else
                lbl.Text = _text;
        }

        private Label GetLastLabel()
        {
            Label ret = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 1] as Label;
            if (ret == null)
                ret = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 2] as Label;
            return ret;
        }

        private void UpdateProgressBarValue(int value)
        {
            if (_bounding == Bounding.Bounded)
            {
                var progressBar = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 1] as ProgressBar;
                progressBar.Value = value;
            }
        }

    }
}