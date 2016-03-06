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
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblProgressRenameDetection = new System.Windows.Forms.Label();
            this.progressRenameDetection = new System.Windows.Forms.ProgressBar();
            this.btnView = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSourceFilesScaned = new System.Windows.Forms.Label();
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
            this.txtSource.Text = "D:\\DataFromExternalDrive";
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
            this.progressNewFiles.Location = new System.Drawing.Point(104, 207);
            this.progressNewFiles.Name = "progressNewFiles";
            this.progressNewFiles.Size = new System.Drawing.Size(272, 23);
            this.progressNewFiles.TabIndex = 6;
            // 
            // progressModifiedFiles
            // 
            this.progressModifiedFiles.Location = new System.Drawing.Point(104, 236);
            this.progressModifiedFiles.Name = "progressModifiedFiles";
            this.progressModifiedFiles.Size = new System.Drawing.Size(272, 23);
            this.progressModifiedFiles.TabIndex = 7;
            // 
            // lblProgressNewFiles
            // 
            this.lblProgressNewFiles.AutoSize = true;
            this.lblProgressNewFiles.Location = new System.Drawing.Point(382, 207);
            this.lblProgressNewFiles.Name = "lblProgressNewFiles";
            this.lblProgressNewFiles.Size = new System.Drawing.Size(24, 13);
            this.lblProgressNewFiles.TabIndex = 8;
            this.lblProgressNewFiles.Text = "0/0";
            // 
            // lblProgressModifiedFiles
            // 
            this.lblProgressModifiedFiles.AutoSize = true;
            this.lblProgressModifiedFiles.Location = new System.Drawing.Point(382, 236);
            this.lblProgressModifiedFiles.Name = "lblProgressModifiedFiles";
            this.lblProgressModifiedFiles.Size = new System.Drawing.Size(24, 13);
            this.lblProgressModifiedFiles.TabIndex = 9;
            this.lblProgressModifiedFiles.Text = "0/0";
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 207);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "new files";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 236);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "modified files";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 178);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "rename detection";
            // 
            // lblProgressRenameDetection
            // 
            this.lblProgressRenameDetection.AutoSize = true;
            this.lblProgressRenameDetection.Location = new System.Drawing.Point(382, 178);
            this.lblProgressRenameDetection.Name = "lblProgressRenameDetection";
            this.lblProgressRenameDetection.Size = new System.Drawing.Size(24, 13);
            this.lblProgressRenameDetection.TabIndex = 15;
            this.lblProgressRenameDetection.Text = "0/0";
            // 
            // progressRenameDetection
            // 
            this.progressRenameDetection.Location = new System.Drawing.Point(104, 178);
            this.progressRenameDetection.Name = "progressRenameDetection";
            this.progressRenameDetection.Size = new System.Drawing.Size(272, 23);
            this.progressRenameDetection.TabIndex = 13;
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "source files";
            // 
            // lblSourceFilesScaned
            // 
            this.lblSourceFilesScaned.AutoSize = true;
            this.lblSourceFilesScaned.Location = new System.Drawing.Point(101, 151);
            this.lblSourceFilesScaned.Name = "lblSourceFilesScaned";
            this.lblSourceFilesScaned.Size = new System.Drawing.Size(13, 13);
            this.lblSourceFilesScaned.TabIndex = 20;
            this.lblSourceFilesScaned.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 306);
            this.Controls.Add(this.lblSourceFilesScaned);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblProgressRenameDetection);
            this.Controls.Add(this.progressRenameDetection);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblProgressRenameDetection;
        private System.Windows.Forms.ProgressBar progressRenameDetection;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSourceFilesScaned;
    }
}

