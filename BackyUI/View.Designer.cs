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
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.lblScanned = new System.Windows.Forms.Label();
            this.btnRestoreTo = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radDiff = new System.Windows.Forms.RadioButton();
            this.radState = new System.Windows.Forms.RadioButton();
            this.filesPanel1 = new Backy.FilesPanel();
            this.SuspendLayout();
            // 
            // btnPrev
            // 
            this.btnPrev.Enabled = false;
            this.btnPrev.Location = new System.Drawing.Point(130, 35);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 23);
            this.btnPrev.TabIndex = 1;
            this.btnPrev.Text = "<<Prev";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnNext
            // 
            this.btnNext.Enabled = false;
            this.btnNext.Location = new System.Drawing.Point(252, 35);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "Next>>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.Location = new System.Drawing.Point(211, 40);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(35, 13);
            this.lblCurrentVersion.TabIndex = 3;
            this.lblCurrentVersion.Text = "label1";
            this.lblCurrentVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblScanned
            // 
            this.lblScanned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.lblScanned.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblScanned.Location = new System.Drawing.Point(0, 95);
            this.lblScanned.Name = "lblScanned";
            this.lblScanned.Size = new System.Drawing.Size(724, 342);
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
            this.comboBox1.Size = new System.Drawing.Size(385, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // radDiff
            // 
            this.radDiff.AutoSize = true;
            this.radDiff.Location = new System.Drawing.Point(509, 12);
            this.radDiff.Name = "radDiff";
            this.radDiff.Size = new System.Drawing.Size(41, 17);
            this.radDiff.TabIndex = 7;
            this.radDiff.Text = "Diff";
            this.radDiff.UseVisualStyleBackColor = true;
            // 
            // radState
            // 
            this.radState.AutoSize = true;
            this.radState.Checked = true;
            this.radState.Location = new System.Drawing.Point(556, 12);
            this.radState.Name = "radState";
            this.radState.Size = new System.Drawing.Size(50, 17);
            this.radState.TabIndex = 8;
            this.radState.TabStop = true;
            this.radState.Text = "State";
            this.radState.UseVisualStyleBackColor = true;
            // 
            // filesPanel1
            // 
            this.filesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesPanel1.BackColor = System.Drawing.Color.Transparent;
            this.filesPanel1.Enabled = false;
            this.filesPanel1.Location = new System.Drawing.Point(0, 66);
            this.filesPanel1.Name = "filesPanel1";
            this.filesPanel1.Size = new System.Drawing.Size(724, 441);
            this.filesPanel1.TabIndex = 0;
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(724, 507);
            this.Controls.Add(this.radState);
            this.Controls.Add(this.radDiff);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnRestoreTo);
            this.Controls.Add(this.lblScanned);
            this.Controls.Add(this.lblCurrentVersion);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.filesPanel1);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "View";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.View_FormClosing);
            this.Load += new System.EventHandler(this.View_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Backy.FilesPanel filesPanel1;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.Label lblScanned;
        private System.Windows.Forms.Button btnRestoreTo;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radDiff;
        private System.Windows.Forms.RadioButton radState;
    }
}