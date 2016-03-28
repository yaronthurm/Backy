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
            this.btnBrowseFoldersRestoreTarget.Location = new System.Drawing.Point(300, 24);
            this.btnBrowseFoldersRestoreTarget.Name = "btnBrowseFoldersRestoreTarget";
            this.btnBrowseFoldersRestoreTarget.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseFoldersRestoreTarget.TabIndex = 21;
            this.btnBrowseFoldersRestoreTarget.Text = "...";
            this.btnBrowseFoldersRestoreTarget.UseVisualStyleBackColor = true;
            this.btnBrowseFoldersRestoreTarget.Click += new System.EventHandler(this.btnBrowseFoldersRestoreTarget_Click);
            // 
            // txtRestoreTarget
            // 
            this.txtRestoreTarget.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Backy.Properties.Settings.Default, "SourceFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtRestoreTarget.Location = new System.Drawing.Point(12, 27);
            this.txtRestoreTarget.Name = "txtRestoreTarget";
            this.txtRestoreTarget.Size = new System.Drawing.Size(281, 20);
            this.txtRestoreTarget.TabIndex = 22;
            this.txtRestoreTarget.Text = global::Backy.Properties.Settings.Default.SourceFolder;
            // 
            // RestoreTo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 261);
            this.Controls.Add(this.txtRestoreTarget);
            this.Controls.Add(this.btnBrowseFoldersRestoreTarget);
            this.Controls.Add(this.label1);
            this.Name = "RestoreTo";
            this.Text = "RestoreTo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowseFoldersRestoreTarget;
        private System.Windows.Forms.TextBox txtRestoreTarget;
    }
}