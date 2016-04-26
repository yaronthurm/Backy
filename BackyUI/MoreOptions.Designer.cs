﻿namespace Backy
{
    partial class MoreOptions
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
            this.btnBrowseFoldersTarget = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.backupTargetView1 = new Backy.BackupTargetView();
            this.btnRunBackupTheBackup = new System.Windows.Forms.Button();
            this.multiStepProgress1 = new Backy.MultiStepProgress();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Backup the Backup";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(149, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please choose the destination";
            // 
            // btnBrowseFoldersTarget
            // 
            this.btnBrowseFoldersTarget.Location = new System.Drawing.Point(167, 35);
            this.btnBrowseFoldersTarget.Name = "btnBrowseFoldersTarget";
            this.btnBrowseFoldersTarget.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersTarget.TabIndex = 23;
            this.btnBrowseFoldersTarget.Text = "...";
            this.btnBrowseFoldersTarget.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersTarget.Click += new System.EventHandler(this.btnBrowseFoldersTarget_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(285, 35);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(257, 108);
            this.richTextBox1.TabIndex = 24;
            this.richTextBox1.Text = "";
            // 
            // backupTargetView1
            // 
            this.backupTargetView1.BackColor = System.Drawing.SystemColors.Control;
            this.backupTargetView1.Location = new System.Drawing.Point(15, 61);
            this.backupTargetView1.Name = "backupTargetView1";
            this.backupTargetView1.Size = new System.Drawing.Size(248, 82);
            this.backupTargetView1.TabIndex = 4;
            // 
            // btnRunBackupTheBackup
            // 
            this.btnRunBackupTheBackup.Enabled = false;
            this.btnRunBackupTheBackup.Location = new System.Drawing.Point(25, 149);
            this.btnRunBackupTheBackup.Name = "btnRunBackupTheBackup";
            this.btnRunBackupTheBackup.Size = new System.Drawing.Size(75, 23);
            this.btnRunBackupTheBackup.TabIndex = 25;
            this.btnRunBackupTheBackup.Text = "Run";
            this.btnRunBackupTheBackup.UseVisualStyleBackColor = true;
            this.btnRunBackupTheBackup.Click += new System.EventHandler(this.btnRunBackupTheBackup_Click);
            // 
            // multiStepProgress1
            // 
            this.multiStepProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.multiStepProgress1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.multiStepProgress1.Location = new System.Drawing.Point(12, 178);
            this.multiStepProgress1.Name = "multiStepProgress1";
            this.multiStepProgress1.Size = new System.Drawing.Size(530, 222);
            this.multiStepProgress1.TabIndex = 26;
            this.multiStepProgress1.TabStop = false;
            // 
            // MoreOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 412);
            this.Controls.Add(this.multiStepProgress1);
            this.Controls.Add(this.btnRunBackupTheBackup);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnBrowseFoldersTarget);
            this.Controls.Add(this.backupTargetView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MoreOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MoreOptions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoreOptions_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private BackupTargetView backupTargetView1;
        private System.Windows.Forms.Button btnBrowseFoldersTarget;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnRunBackupTheBackup;
        private MultiStepProgress multiStepProgress1;
    }
}