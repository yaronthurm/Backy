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
            this.label3 = new System.Windows.Forms.Label();
            this.autoRunTimer = new System.Windows.Forms.Timer(this.components);
            this.radDetection = new System.Windows.Forms.RadioButton();
            this.numSeconds = new System.Windows.Forms.NumericUpDown();
            this.btnSettings = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnStartStop = new System.Windows.Forms.CheckBox();
            this.multiStepProgress1 = new Backy.MultiStepProgress();
            this.btnDetect = new System.Windows.Forms.CheckBox();
            this.linkMoreOptions = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(98, 119);
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
            this.btnAbort.Location = new System.Drawing.Point(315, 148);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 10;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(315, 116);
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
            this.radManual.Checked = true;
            this.radManual.Location = new System.Drawing.Point(16, 122);
            this.radManual.Name = "radManual";
            this.radManual.Size = new System.Drawing.Size(60, 17);
            this.radManual.TabIndex = 22;
            this.radManual.TabStop = true;
            this.radManual.Text = "Manual";
            this.radManual.UseVisualStyleBackColor = true;
            // 
            // radScheduled
            // 
            this.radScheduled.AutoSize = true;
            this.radScheduled.Location = new System.Drawing.Point(16, 151);
            this.radScheduled.Name = "radScheduled";
            this.radScheduled.Size = new System.Drawing.Size(76, 17);
            this.radScheduled.TabIndex = 23;
            this.radScheduled.Text = "Scheduled";
            this.radScheduled.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(237, 155);
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
            // radDetection
            // 
            this.radDetection.AutoSize = true;
            this.radDetection.Location = new System.Drawing.Point(16, 180);
            this.radDetection.Name = "radDetection";
            this.radDetection.Size = new System.Drawing.Size(78, 17);
            this.radDetection.TabIndex = 27;
            this.radDetection.Text = "On change";
            this.radDetection.UseVisualStyleBackColor = true;
            // 
            // numSeconds
            // 
            this.numSeconds.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::Backy.Properties.Settings.Default, "autoBackupInterval", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numSeconds.Enabled = false;
            this.numSeconds.Location = new System.Drawing.Point(180, 150);
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
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(315, 12);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 33;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(15, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(294, 91);
            this.richTextBox1.TabIndex = 37;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = "";
            // 
            // btnStartStop
            // 
            this.btnStartStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnStartStop.Location = new System.Drawing.Point(98, 148);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 38;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.CheckedChanged += new System.EventHandler(this.btnStartStop_CheckedChanged);
            // 
            // multiStepProgress1
            // 
            this.multiStepProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.multiStepProgress1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.multiStepProgress1.Location = new System.Drawing.Point(0, 216);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(440, 277);
            this.multiStepProgress1.TabIndex = 19;
            this.multiStepProgress1.TabStop = false;
            // 
            // btnDetect
            // 
            this.btnDetect.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnDetect.Location = new System.Drawing.Point(98, 177);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(75, 23);
            this.btnDetect.TabIndex = 39;
            this.btnDetect.Text = "Run&&Detect";
            this.btnDetect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.CheckedChanged += new System.EventHandler(this.btnDetect_CheckedChanged);
            // 
            // linkMoreOptions
            // 
            this.linkMoreOptions.AutoSize = true;
            this.linkMoreOptions.Location = new System.Drawing.Point(315, 55);
            this.linkMoreOptions.Name = "linkMoreOptions";
            this.linkMoreOptions.Size = new System.Drawing.Size(68, 13);
            this.linkMoreOptions.TabIndex = 40;
            this.linkMoreOptions.TabStop = true;
            this.linkMoreOptions.Text = "More options";
            this.linkMoreOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkMoreOptions_LinkClicked);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 493);
            this.Controls.Add(this.linkMoreOptions);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.radDetection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numSeconds);
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
        private System.Windows.Forms.NumericUpDown numSeconds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer autoRunTimer;
        private System.Windows.Forms.RadioButton radDetection;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckBox btnStartStop;
        private System.Windows.Forms.CheckBox btnDetect;
        private System.Windows.Forms.LinkLabel linkMoreOptions;
    }
}

