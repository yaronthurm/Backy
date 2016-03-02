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
            this.progressNewFiles = new System.Windows.Forms.ProgressBar();
            this.progressModifiedFiles = new System.Windows.Forms.ProgressBar();
            this.lblProgressNewFiles = new System.Windows.Forms.Label();
            this.lblProgressModifiedFiles = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
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
            // progressNewFiles
            // 
            this.progressNewFiles.Location = new System.Drawing.Point(83, 135);
            this.progressNewFiles.Name = "progressNewFiles";
            this.progressNewFiles.Size = new System.Drawing.Size(272, 23);
            this.progressNewFiles.TabIndex = 6;
            // 
            // progressModifiedFiles
            // 
            this.progressModifiedFiles.Location = new System.Drawing.Point(83, 164);
            this.progressModifiedFiles.Name = "progressModifiedFiles";
            this.progressModifiedFiles.Size = new System.Drawing.Size(272, 23);
            this.progressModifiedFiles.TabIndex = 7;
            // 
            // lblProgressNewFiles
            // 
            this.lblProgressNewFiles.AutoSize = true;
            this.lblProgressNewFiles.Location = new System.Drawing.Point(361, 135);
            this.lblProgressNewFiles.Name = "lblProgressNewFiles";
            this.lblProgressNewFiles.Size = new System.Drawing.Size(48, 13);
            this.lblProgressNewFiles.TabIndex = 8;
            this.lblProgressNewFiles.Text = "new files";
            // 
            // lblProgressModifiedFiles
            // 
            this.lblProgressModifiedFiles.AutoSize = true;
            this.lblProgressModifiedFiles.Location = new System.Drawing.Point(361, 164);
            this.lblProgressModifiedFiles.Name = "lblProgressModifiedFiles";
            this.lblProgressModifiedFiles.Size = new System.Drawing.Size(67, 13);
            this.lblProgressModifiedFiles.TabIndex = 9;
            this.lblProgressModifiedFiles.Text = "modified files";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 258);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.lblProgressModifiedFiles);
            this.Controls.Add(this.lblProgressNewFiles);
            this.Controls.Add(this.progressModifiedFiles);
            this.Controls.Add(this.progressNewFiles);
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
        private System.Windows.Forms.ProgressBar progressNewFiles;
        private System.Windows.Forms.ProgressBar progressModifiedFiles;
        private System.Windows.Forms.Label lblProgressNewFiles;
        private System.Windows.Forms.Label lblProgressModifiedFiles;
        private System.Windows.Forms.Button btnAbort;
    }
}

