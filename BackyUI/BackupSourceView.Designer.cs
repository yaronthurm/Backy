namespace Backy
{
    partial class BackupSourceView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkEnabled = new Backy.ResizeableCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(3, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(70, 70);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lblPath
            // 
            this.lblPath.BackColor = System.Drawing.Color.Transparent;
            this.lblPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPath.Location = new System.Drawing.Point(77, 0);
            this.lblPath.MaximumSize = new System.Drawing.Size(200, 100);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(156, 69);
            this.lblPath.TabIndex = 2;
            this.lblPath.Text = "Name";
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(235, 23);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(20, 20);
            this.btnOpen.TabIndex = 3;
            this.btnOpen.Text = ">";
            this.toolTip1.SetToolTip(this.btnOpen, "Open");
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(235, 3);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(20, 20);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "X";
            this.toolTip1.SetToolTip(this.btnRemove, "Remove");
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // chkEnabled
            // 
            this.chkEnabled.Checked = true;
            this.chkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnabled.Location = new System.Drawing.Point(236, 46);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(20, 20);
            this.chkEnabled.TabIndex = 5;
            this.chkEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.chkEnabled, "Enable/Disable");
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
            // 
            // BackupSourceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.pictureBox1);
            this.Name = "BackupSourceView";
            this.Size = new System.Drawing.Size(258, 71);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ToolTip toolTip1;
        private ResizeableCheckBox chkEnabled;
    }
}
