namespace Network_Manager.Jobs.Extensions
{
    partial class Dependencies
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dependencies));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.labelVC2010Installed = new System.Windows.Forms.Label();
            this.labelWinPcapInstalled = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(389, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "You are missing one or more dependencies required to perform this action.\r\nBellow" +
    " is the list of required packages / libraries. Install the ones that are missing" +
    " !";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(223, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Microsoft Visual C++ 2010 x86 Redistributable";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "WinPcap 4.1.3";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(471, 68);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Download && Install";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(471, 97);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(106, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Download && Install";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // labelVC2010Installed
            // 
            this.labelVC2010Installed.AutoSize = true;
            this.labelVC2010Installed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVC2010Installed.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelVC2010Installed.Location = new System.Drawing.Point(262, 73);
            this.labelVC2010Installed.Name = "labelVC2010Installed";
            this.labelVC2010Installed.Size = new System.Drawing.Size(76, 13);
            this.labelVC2010Installed.TabIndex = 5;
            this.labelVC2010Installed.Text = "Checking ...";
            // 
            // labelWinPcapInstalled
            // 
            this.labelWinPcapInstalled.AutoSize = true;
            this.labelWinPcapInstalled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWinPcapInstalled.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelWinPcapInstalled.Location = new System.Drawing.Point(262, 102);
            this.labelWinPcapInstalled.Name = "labelWinPcapInstalled";
            this.labelWinPcapInstalled.Size = new System.Drawing.Size(76, 13);
            this.labelWinPcapInstalled.TabIndex = 6;
            this.labelWinPcapInstalled.Text = "Checking ...";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(265, 147);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "Refresh";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Dependencies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 182);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.labelWinPcapInstalled);
            this.Controls.Add(this.labelVC2010Installed);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Dependencies";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dependencies";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label labelVC2010Installed;
        private System.Windows.Forms.Label labelWinPcapInstalled;
        private System.Windows.Forms.Button button3;
    }
}