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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.btnBrowseFoldersSource = new System.Windows.Forms.Button();
            this.btnBrowseFoldersTarget = new System.Windows.Forms.Button();
            this.radManual = new System.Windows.Forms.RadioButton();
            this.radAuto = new System.Windows.Forms.RadioButton();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.autoRunTimer = new System.Windows.Forms.Timer(this.components);
            this.numSeconds = new System.Windows.Forms.NumericUpDown();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.multiStepProgress1 = new Backy.MultiStepProgress();
            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Target";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(83, 106);
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
            this.btnAbort.Location = new System.Drawing.Point(311, 143);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 10;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(311, 106);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 23);
            this.btnView.TabIndex = 18;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // btnBrowseFoldersSource
            // 
            this.btnBrowseFoldersSource.Location = new System.Drawing.Point(361, 28);
            this.btnBrowseFoldersSource.Name = "btnBrowseFoldersSource";
            this.btnBrowseFoldersSource.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersSource.TabIndex = 20;
            this.btnBrowseFoldersSource.Text = "...";
            this.btnBrowseFoldersSource.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersSource.Click += new System.EventHandler(this.btnBrowseFoldersSource_Click);
            // 
            // btnBrowseFoldersTarget
            // 
            this.btnBrowseFoldersTarget.Location = new System.Drawing.Point(361, 61);
            this.btnBrowseFoldersTarget.Name = "btnBrowseFoldersTarget";
            this.btnBrowseFoldersTarget.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersTarget.TabIndex = 21;
            this.btnBrowseFoldersTarget.Text = "...";
            this.btnBrowseFoldersTarget.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersTarget.Click += new System.EventHandler(this.btnBrowseFoldersTarget_Click);
            // 
            // radManual
            // 
            this.radManual.AutoSize = true;
            this.radManual.Checked = true;
            this.radManual.Location = new System.Drawing.Point(12, 109);
            this.radManual.Name = "radManual";
            this.radManual.Size = new System.Drawing.Size(60, 17);
            this.radManual.TabIndex = 22;
            this.radManual.TabStop = true;
            this.radManual.Text = "Manual";
            this.radManual.UseVisualStyleBackColor = true;
            // 
            // radAuto
            // 
            this.radAuto.AutoSize = true;
            this.radAuto.Location = new System.Drawing.Point(12, 147);
            this.radAuto.Name = "radAuto";
            this.radAuto.Size = new System.Drawing.Size(47, 17);
            this.radAuto.TabIndex = 23;
            this.radAuto.Text = "Auto";
            this.radAuto.UseVisualStyleBackColor = true;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Enabled = false;
            this.btnStartStop.Location = new System.Drawing.Point(83, 144);
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
            this.label3.Location = new System.Drawing.Point(230, 154);
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
            // numSeconds
            // 
            this.numSeconds.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::Backy.Properties.Settings.Default, "autoBackupInterval", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numSeconds.Enabled = false;
            this.numSeconds.Location = new System.Drawing.Point(173, 146);
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
            // txtTarget
            // 
            this.txtTarget.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Backy.Properties.Settings.Default, "TargetFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtTarget.Location = new System.Drawing.Point(83, 63);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(272, 20);
            this.txtTarget.TabIndex = 3;
            this.txtTarget.Text = global::Backy.Properties.Settings.Default.TargetFolder;
            this.txtTarget.Validated += new System.EventHandler(this.txtTarget_Validated);
            // 
            // txtSource
            // 
            this.txtSource.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Backy.Properties.Settings.Default, "SourceFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtSource.Location = new System.Drawing.Point(83, 30);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(272, 20);
            this.txtSource.TabIndex = 2;
            this.txtSource.Text = global::Backy.Properties.Settings.Default.SourceFolder;
            this.txtSource.Validated += new System.EventHandler(this.txtSource_Validated);
            // 
            // multiStepProgress1
            // 
            this.multiStepProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.multiStepProgress1.BackColor = System.Drawing.Color.Transparent;
            this.multiStepProgress1.Location = new System.Drawing.Point(0, 195);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(440, 298);
            this.multiStepProgress1.TabIndex = 19;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 493);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numSeconds);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.radAuto);
            this.Controls.Add(this.radManual);
            this.Controls.Add(this.btnBrowseFoldersTarget);
            this.Controls.Add(this.btnBrowseFoldersSource);
            this.Controls.Add(this.multiStepProgress1);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.txtTarget);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximumSize = new System.Drawing.Size(456, 800);
            this.MinimumSize = new System.Drawing.Size(456, 488);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backup";
            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnView;
        private MultiStepProgress multiStepProgress1;
        private System.Windows.Forms.Button btnBrowseFoldersSource;
        private System.Windows.Forms.Button btnBrowseFoldersTarget;
        private System.Windows.Forms.RadioButton radManual;
        private System.Windows.Forms.RadioButton radAuto;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.NumericUpDown numSeconds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer autoRunTimer;
    }
}

