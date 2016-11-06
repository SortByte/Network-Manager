namespace Network_Manager.Gadget.ControlPanel
{
    partial class ControlPanelForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanelForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.routesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBalancingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetTCPIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iPv4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iPv6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gadgetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.networkMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wiFiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(818, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionsToolStripMenuItem,
            this.routesToolStripMenuItem,
            this.loadBalancingToolStripMenuItem,
            this.resetTCPIPToolStripMenuItem,
            this.networkMapToolStripMenuItem,
            this.wiFiToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // connectionsToolStripMenuItem
            // 
            this.connectionsToolStripMenuItem.Name = "connectionsToolStripMenuItem";
            this.connectionsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.connectionsToolStripMenuItem.Text = "IP sessions";
            this.connectionsToolStripMenuItem.Click += new System.EventHandler(this.ipSessionsToolStripMenuItem_Click);
            // 
            // routesToolStripMenuItem
            // 
            this.routesToolStripMenuItem.Name = "routesToolStripMenuItem";
            this.routesToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.routesToolStripMenuItem.Text = "Routes";
            this.routesToolStripMenuItem.Click += new System.EventHandler(this.routesToolStripMenuItem_Click);
            // 
            // loadBalancingToolStripMenuItem
            // 
            this.loadBalancingToolStripMenuItem.Name = "loadBalancingToolStripMenuItem";
            this.loadBalancingToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.loadBalancingToolStripMenuItem.Text = "Load balancing";
            this.loadBalancingToolStripMenuItem.Click += new System.EventHandler(this.loadBalancingToolStripMenuItem_Click);
            // 
            // resetTCPIPToolStripMenuItem
            // 
            this.resetTCPIPToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iPv4ToolStripMenuItem,
            this.iPv6ToolStripMenuItem});
            this.resetTCPIPToolStripMenuItem.Name = "resetTCPIPToolStripMenuItem";
            this.resetTCPIPToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.resetTCPIPToolStripMenuItem.Text = "Reset TCP/IP";
            // 
            // iPv4ToolStripMenuItem
            // 
            this.iPv4ToolStripMenuItem.Name = "iPv4ToolStripMenuItem";
            this.iPv4ToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.iPv4ToolStripMenuItem.Text = "IPv4";
            this.iPv4ToolStripMenuItem.Click += new System.EventHandler(this.iPv4ToolStripMenuItem_Click);
            // 
            // iPv6ToolStripMenuItem
            // 
            this.iPv6ToolStripMenuItem.Name = "iPv6ToolStripMenuItem";
            this.iPv6ToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.iPv6ToolStripMenuItem.Text = "IPv6";
            this.iPv6ToolStripMenuItem.Click += new System.EventHandler(this.iPv6ToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gadgetToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // gadgetToolStripMenuItem
            // 
            this.gadgetToolStripMenuItem.Name = "gadgetToolStripMenuItem";
            this.gadgetToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.gadgetToolStripMenuItem.Text = "Gadget settings";
            this.gadgetToolStripMenuItem.Click += new System.EventHandler(this.gadgetToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(12, 24);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(109, 44);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(59, 19);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(47, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "IPv6";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "IPv4";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // networkMapToolStripMenuItem
            // 
            this.networkMapToolStripMenuItem.Name = "networkMapToolStripMenuItem";
            this.networkMapToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.networkMapToolStripMenuItem.Text = "Network map";
            this.networkMapToolStripMenuItem.Visible = false;
            // 
            // wiFiToolStripMenuItem
            // 
            this.wiFiToolStripMenuItem.Name = "wiFiToolStripMenuItem";
            this.wiFiToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.wiFiToolStripMenuItem.Text = "WiFi";
            this.wiFiToolStripMenuItem.Visible = false;
            // 
            // ControlPanelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(818, 171);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ControlPanelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Control Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlPanelForm_FormClosing);
            this.Load += new System.EventHandler(this.ControlPanelForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem routesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadBalancingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetTCPIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gadgetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.ToolStripMenuItem iPv4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iPv6ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem networkMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wiFiToolStripMenuItem;
    }
}