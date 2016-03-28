namespace Backy
{
    partial class RestoreTo
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
            this.btnBrowseFoldersRestoreTarget = new System.Windows.Forms.Button();
            this.txtRestoreTarget = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblValidationMessage = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.multiStepProgress1 = new Backy.MultiStepProgress();
            this.btnOpenTarget = new System.Windows.Forms.Button();
            this.lblRestoreVersion = new System.Windows.Forms.Label();
            this.lblRestoreSource = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please choose an empty directory to hold the restored data";
            // 
            // btnBrowseFoldersRestoreTarget
            // 
            this.btnBrowseFoldersRestoreTarget.Location = new System.Drawing.Point(324, 25);
            this.btnBrowseFoldersRestoreTarget.Name = "btnBrowseFoldersRestoreTarget";
            this.btnBrowseFoldersRestoreTarget.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersRestoreTarget.TabIndex = 21;
            this.btnBrowseFoldersRestoreTarget.Text = "...";
            this.btnBrowseFoldersRestoreTarget.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersRestoreTarget.Click += new System.EventHandler(this.btnBrowseFoldersRestoreTarget_Click);
            // 
            // txtRestoreTarget
            // 
            this.txtRestoreTarget.Location = new System.Drawing.Point(12, 27);
            this.txtRestoreTarget.Name = "txtRestoreTarget";
            this.txtRestoreTarget.Size = new System.Drawing.Size(296, 20);
            this.txtRestoreTarget.TabIndex = 22;
            this.txtRestoreTarget.TextChanged += new System.EventHandler(this.txtRestoreTarget_TextChanged);
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(11, 118);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 23;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblValidationMessage
            // 
            this.lblValidationMessage.AutoSize = true;
            this.lblValidationMessage.ForeColor = System.Drawing.Color.Red;
            this.lblValidationMessage.Location = new System.Drawing.Point(12, 50);
            this.lblValidationMessage.Name = "lblValidationMessage";
            this.lblValidationMessage.Size = new System.Drawing.Size(97, 13);
            this.lblValidationMessage.TabIndex = 25;
            this.lblValidationMessage.Text = "validation message";
            this.lblValidationMessage.Visible = false;
            // 
            // btnAbort
            // 
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(102, 118);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 27;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // multiStepProgress1
            // 
            this.multiStepProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.multiStepProgress1.BackColor = System.Drawing.Color.Transparent;
            this.multiStepProgress1.Location = new System.Drawing.Point(0, 160);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(390, 192);
            this.multiStepProgress1.TabIndex = 26;
            // 
            // btnOpenTarget
            // 
            this.btnOpenTarget.Location = new System.Drawing.Point(355, 25);
            this.btnOpenTarget.Name = "btnOpenTarget";
            this.btnOpenTarget.Size = new System.Drawing.Size(25, 23);
            this.btnOpenTarget.TabIndex = 32;
            this.btnOpenTarget.Text = "->";
            this.btnOpenTarget.UseVisualStyleBackColor = true;
            this.btnOpenTarget.Click += new System.EventHandler(this.btnOpenTarget_Click);
            // 
            // lblRestoreVersion
            // 
            this.lblRestoreVersion.AutoSize = true;
            this.lblRestoreVersion.Location = new System.Drawing.Point(12, 94);
            this.lblRestoreVersion.Name = "lblRestoreVersion";
            this.lblRestoreVersion.Size = new System.Drawing.Size(95, 13);
            this.lblRestoreVersion.TabIndex = 33;
            this.lblRestoreVersion.Text = "Restoring version: ";
            // 
            // lblRestoreSource
            // 
            this.lblRestoreSource.AutoSize = true;
            this.lblRestoreSource.Location = new System.Drawing.Point(12, 77);
            this.lblRestoreSource.Name = "lblRestoreSource";
            this.lblRestoreSource.Size = new System.Drawing.Size(82, 13);
            this.lblRestoreSource.TabIndex = 34;
            this.lblRestoreSource.Text = "Restore source:";
            // 
            // RestoreTo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 351);
            this.Controls.Add(this.lblRestoreSource);
            this.Controls.Add(this.lblRestoreVersion);
            this.Controls.Add(this.btnOpenTarget);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.multiStepProgress1);
            this.Controls.Add(this.lblValidationMessage);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.txtRestoreTarget);
            this.Controls.Add(this.btnBrowseFoldersRestoreTarget);
            this.Controls.Add(this.label1);
            this.MaximumSize = new System.Drawing.Size(405, 800);
            this.MinimumSize = new System.Drawing.Size(405, 300);
            this.Name = "RestoreTo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RestoreTo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RestoreTo_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowseFoldersRestoreTarget;
        private System.Windows.Forms.TextBox txtRestoreTarget;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblValidationMessage;
        private MultiStepProgress multiStepProgress1;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnOpenTarget;
        private System.Windows.Forms.Label lblRestoreVersion;
        private System.Windows.Forms.Label lblRestoreSource;
    }
}