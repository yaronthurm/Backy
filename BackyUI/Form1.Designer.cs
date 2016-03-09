namespace Backy
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.btnBrowseFoldersSource = new System.Windows.Forms.Button();
            this.btnBrowseFoldersTarget = new System.Windows.Forms.Button();
            this.multiStepProgress1 = new Backy.MultiStepProgress();
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
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(83, 30);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(272, 20);
            this.txtSource.TabIndex = 2;
            this.txtSource.Text = "D:\\FolderForBackyTesting\\Source";
            // 
            // txtTarget
            // 
            this.txtTarget.Location = new System.Drawing.Point(83, 63);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(272, 20);
            this.txtTarget.TabIndex = 3;
            this.txtTarget.Text = "D:\\FolderForBackyTesting\\Target";
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
            this.btnAbort.Location = new System.Drawing.Point(173, 106);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 10;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(280, 106);
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
            // multiStepProgress1
            // 
            this.multiStepProgress1.BackColor = System.Drawing.Color.Transparent;
            this.multiStepProgress1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.multiStepProgress1.Location = new System.Drawing.Point(0, 150);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(477, 299);
            this.multiStepProgress1.TabIndex = 19;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 449);
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
            this.Name = "Form1";
            this.Text = "Form1";
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
    }
}

