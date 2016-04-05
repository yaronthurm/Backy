namespace Backy
{
    partial class Settings
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.backupTargetView1 = new Backy.BackupTargetView();
            this.addSourcesPanel1 = new Backy.AddBackupSourcesPanel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(290, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please select the directories you would like to be backed up";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 302);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(208, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please select a directory as the destination";
            // 
            // btnBrowseFoldersTarget
            // 
            this.btnBrowseFoldersTarget.Location = new System.Drawing.Point(226, 297);
            this.btnBrowseFoldersTarget.Name = "btnBrowseFoldersTarget";
            this.btnBrowseFoldersTarget.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersTarget.TabIndex = 22;
            this.btnBrowseFoldersTarget.Text = "...";
            this.btnBrowseFoldersTarget.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersTarget.Click += new System.EventHandler(this.btnBrowseFoldersTarget_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(471, 452);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(562, 452);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // backupTargetView1
            // 
            this.backupTargetView1.BackColor = System.Drawing.SystemColors.Control;
            this.backupTargetView1.Location = new System.Drawing.Point(15, 318);
            this.backupTargetView1.Name = "backupTargetView1";
            this.backupTargetView1.Size = new System.Drawing.Size(248, 82);
            this.backupTargetView1.TabIndex = 3;
            // 
            // addSourcesPanel1
            // 
            this.addSourcesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addSourcesPanel1.BackColor = System.Drawing.Color.White;
            this.addSourcesPanel1.Location = new System.Drawing.Point(12, 36);
            this.addSourcesPanel1.Name = "addSourcesPanel1";
            this.addSourcesPanel1.Size = new System.Drawing.Size(630, 237);
            this.addSourcesPanel1.TabIndex = 0;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 481);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowseFoldersTarget);
            this.Controls.Add(this.backupTargetView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addSourcesPanel1);
            this.MaximumSize = new System.Drawing.Size(670, 520);
            this.MinimumSize = new System.Drawing.Size(670, 520);
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AddBackupSourcesPanel addSourcesPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private BackupTargetView backupTargetView1;
        private System.Windows.Forms.Button btnBrowseFoldersTarget;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}