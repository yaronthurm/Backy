namespace Backy
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.radManual = new System.Windows.Forms.RadioButton();
            this.radScheduled = new System.Windows.Forms.RadioButton();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.autoRunTimer = new System.Windows.Forms.Timer(this.components);
            this.btnDetect = new System.Windows.Forms.Button();
            this.radDetection = new System.Windows.Forms.RadioButton();
            this.numSeconds = new System.Windows.Forms.NumericUpDown();
            this.changeDetectionTimer = new System.Windows.Forms.Timer(this.components);
            this.multiStepProgress1 = new Backy.MultiStepProgress();
            this.btnSettings = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(94, 96);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 4;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(311, 133);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 10;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(311, 96);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 23);
            this.btnView.TabIndex = 18;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // radManual
            // 
            this.radManual.AutoSize = true;
            this.radManual.Location = new System.Drawing.Point(12, 99);
            this.radManual.Name = "radManual";
            this.radManual.Size = new System.Drawing.Size(60, 17);
            this.radManual.TabIndex = 22;
            this.radManual.Text = "Manual";
            this.radManual.UseVisualStyleBackColor = true;
            // 
            // radScheduled
            // 
            this.radScheduled.AutoSize = true;
            this.radScheduled.Location = new System.Drawing.Point(12, 128);
            this.radScheduled.Name = "radScheduled";
            this.radScheduled.Size = new System.Drawing.Size(76, 17);
            this.radScheduled.TabIndex = 23;
            this.radScheduled.Text = "Scheduled";
            this.radScheduled.UseVisualStyleBackColor = true;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Enabled = false;
            this.btnStartStop.Location = new System.Drawing.Point(94, 125);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 24;
            this.btnStartStop.Tag = "1";
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(233, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "[Sec]";
            // 
            // autoRunTimer
            // 
            this.autoRunTimer.Interval = 1000;
            this.autoRunTimer.Tick += new System.EventHandler(this.autoRunTimer_Tick);
            // 
            // btnDetect
            // 
            this.btnDetect.Location = new System.Drawing.Point(94, 154);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(75, 23);
            this.btnDetect.TabIndex = 28;
            this.btnDetect.Tag = "1";
            this.btnDetect.Text = "Detect";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // radDetection
            // 
            this.radDetection.AutoSize = true;
            this.radDetection.Checked = true;
            this.radDetection.Location = new System.Drawing.Point(12, 157);
            this.radDetection.Name = "radDetection";
            this.radDetection.Size = new System.Drawing.Size(78, 17);
            this.radDetection.TabIndex = 27;
            this.radDetection.TabStop = true;
            this.radDetection.Text = "On change";
            this.radDetection.UseVisualStyleBackColor = true;
            // 
            // numSeconds
            // 
            this.numSeconds.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::Backy.Properties.Settings.Default, "autoBackupInterval", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numSeconds.Enabled = false;
            this.numSeconds.Location = new System.Drawing.Point(176, 127);
            this.numSeconds.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numSeconds.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numSeconds.Name = "numSeconds";
            this.numSeconds.Size = new System.Drawing.Size(51, 20);
            this.numSeconds.TabIndex = 25;
            this.numSeconds.Value = global::Backy.Properties.Settings.Default.autoBackupInterval;
            // 
            // changeDetectionTimer
            // 
            this.changeDetectionTimer.Interval = 1000;
            this.changeDetectionTimer.Tick += new System.EventHandler(this.changeDetectionTimer_Tick);
            // 
            // multiStepProgress1
            // 
            this.multiStepProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.multiStepProgress1.BackColor = System.Drawing.Color.Transparent;
            this.multiStepProgress1.Location = new System.Drawing.Point(0, 193);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(440, 300);
            this.multiStepProgress1.TabIndex = 19;
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(94, 25);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 33;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 493);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.radDetection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numSeconds);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.radScheduled);
            this.Controls.Add(this.radManual);
            this.Controls.Add(this.multiStepProgress1);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnRun);
            this.MaximumSize = new System.Drawing.Size(456, 800);
            this.MinimumSize = new System.Drawing.Size(456, 488);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backup";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnView;
        private MultiStepProgress multiStepProgress1;
        private System.Windows.Forms.RadioButton radManual;
        private System.Windows.Forms.RadioButton radScheduled;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.NumericUpDown numSeconds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer autoRunTimer;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.RadioButton radDetection;
        private System.Windows.Forms.Timer changeDetectionTimer;
        private System.Windows.Forms.Button btnSettings;
    }
}

