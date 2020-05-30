namespace Backy
{
    partial class View
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
            this.lblScanned = new System.Windows.Forms.Label();
            this.btnRestoreTo = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radDiff = new System.Windows.Forms.RadioButton();
            this.radState = new System.Windows.Forms.RadioButton();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.comboBackupFolder = new System.Windows.Forms.ComboBox();
            this.numVersion = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.filesPanel1 = new Backy.FilesPanel();
            ((System.ComponentModel.ISupportInitialize)(this.numVersion)).BeginInit();
            this.SuspendLayout();
            // 
            // lblScanned
            // 
            this.lblScanned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.lblScanned.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblScanned.Location = new System.Drawing.Point(0, 127);
            this.lblScanned.Name = "lblScanned";
            this.lblScanned.Size = new System.Drawing.Size(724, 310);
            this.lblScanned.TabIndex = 4;
            this.lblScanned.Text = "Scanned: 0";
            this.lblScanned.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRestoreTo
            // 
            this.btnRestoreTo.Enabled = false;
            this.btnRestoreTo.Location = new System.Drawing.Point(375, 35);
            this.btnRestoreTo.Name = "btnRestoreTo";
            this.btnRestoreTo.Size = new System.Drawing.Size(75, 23);
            this.btnRestoreTo.TabIndex = 5;
            this.btnRestoreTo.Text = "Restore to...";
            this.btnRestoreTo.UseVisualStyleBackColor = true;
            this.btnRestoreTo.Click += new System.EventHandler(this.btnRestoreTo_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(7, 8);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(443, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // radDiff
            // 
            this.radDiff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radDiff.AutoSize = true;
            this.radDiff.Location = new System.Drawing.Point(477, 12);
            this.radDiff.Name = "radDiff";
            this.radDiff.Size = new System.Drawing.Size(41, 17);
            this.radDiff.TabIndex = 7;
            this.radDiff.Text = "Diff";
            this.radDiff.UseVisualStyleBackColor = true;
            // 
            // radState
            // 
            this.radState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radState.AutoSize = true;
            this.radState.Checked = true;
            this.radState.Location = new System.Drawing.Point(524, 12);
            this.radState.Name = "radState";
            this.radState.Size = new System.Drawing.Size(50, 17);
            this.radState.TabIndex = 8;
            this.radState.TabStop = true;
            this.radState.Text = "State";
            this.radState.UseVisualStyleBackColor = true;
            // 
            // lblDateTime
            // 
            this.lblDateTime.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblDateTime.Location = new System.Drawing.Point(86, 66);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(293, 23);
            this.lblDateTime.TabIndex = 9;
            this.lblDateTime.Text = "label1";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDateTime.Visible = false;
            // 
            // comboBackupFolder
            // 
            this.comboBackupFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBackupFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBackupFolder.FormattingEnabled = true;
            this.comboBackupFolder.Location = new System.Drawing.Point(580, 8);
            this.comboBackupFolder.Name = "comboBackupFolder";
            this.comboBackupFolder.Size = new System.Drawing.Size(116, 21);
            this.comboBackupFolder.TabIndex = 10;
            this.comboBackupFolder.SelectedIndexChanged += new System.EventHandler(this.comboBackupFolder_SelectedIndexChanged);
            // 
            // numVersion
            // 
            this.numVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.numVersion.Location = new System.Drawing.Point(111, 38);
            this.numVersion.Name = "numVersion";
            this.numVersion.Size = new System.Drawing.Size(81, 23);
            this.numVersion.TabIndex = 12;
            this.numVersion.ValueChanged += new System.EventHandler(this.numVersion_ValueChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(22, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 23);
            this.label1.TabIndex = 13;
            this.label1.Text = "Version:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // filesPanel1
            // 
            this.filesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesPanel1.BackColor = System.Drawing.Color.Transparent;
            this.filesPanel1.EnableContextMenu = true;
            this.filesPanel1.Enabled = false;
            this.filesPanel1.Location = new System.Drawing.Point(0, 92);
            this.filesPanel1.Name = "filesPanel1";
            this.filesPanel1.Size = new System.Drawing.Size(724, 415);
            this.filesPanel1.TabIndex = 0;
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(724, 507);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numVersion);
            this.Controls.Add(this.comboBackupFolder);
            this.Controls.Add(this.lblDateTime);
            this.Controls.Add(this.radState);
            this.Controls.Add(this.radDiff);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnRestoreTo);
            this.Controls.Add(this.lblScanned);
            this.Controls.Add(this.filesPanel1);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "View";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.View_FormClosing);
            this.Load += new System.EventHandler(this.View_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numVersion)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Backy.FilesPanel filesPanel1;
        private System.Windows.Forms.Label lblScanned;
        private System.Windows.Forms.Button btnRestoreTo;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radDiff;
        private System.Windows.Forms.RadioButton radState;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.ComboBox comboBackupFolder;
        private System.Windows.Forms.NumericUpDown numVersion;
        private System.Windows.Forms.Label label1;
    }
}