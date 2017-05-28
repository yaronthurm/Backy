namespace Backy
{
    partial class PanelTest
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
            this.button1 = new System.Windows.Forms.Button();
            this.numItems = new System.Windows.Forms.NumericUpDown();
            this.loadTime = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.flowPanelTest1 = new Backy.FlowPanelTest();
            ((System.ComponentModel.ISupportInitialize)(this.numItems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadTime)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numItems
            // 
            this.numItems.Location = new System.Drawing.Point(135, 33);
            this.numItems.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numItems.Name = "numItems";
            this.numItems.Size = new System.Drawing.Size(120, 20);
            this.numItems.TabIndex = 2;
            this.numItems.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // loadTime
            // 
            this.loadTime.Location = new System.Drawing.Point(391, 33);
            this.loadTime.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.loadTime.Name = "loadTime";
            this.loadTime.Size = new System.Drawing.Size(120, 20);
            this.loadTime.TabIndex = 3;
            this.loadTime.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "num of items";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(388, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "load time [ms]";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(547, 35);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(32, 13);
            this.lblTime.TabIndex = 6;
            this.lblTime.Text = "(time)";
            // 
            // flowPanelTest1
            // 
            this.flowPanelTest1.Location = new System.Drawing.Point(12, 70);
            this.flowPanelTest1.Name = "flowPanelTest1";
            this.flowPanelTest1.Size = new System.Drawing.Size(845, 422);
            this.flowPanelTest1.TabIndex = 0;
            // 
            // PanelTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 504);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.loadTime);
            this.Controls.Add(this.numItems);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.flowPanelTest1);
            this.Name = "PanelTest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PanelTest";
            ((System.ComponentModel.ISupportInitialize)(this.numItems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FlowPanelTest flowPanelTest1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numItems;
        private System.Windows.Forms.NumericUpDown loadTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTime;
    }
}