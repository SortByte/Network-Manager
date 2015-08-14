namespace Network_Manager.Gadget.Settings
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.autoStartup = new System.Windows.Forms.CheckBox();
            this.checkForUpdates = new System.Windows.Forms.CheckBox();
            this.alwaysOnTop = new System.Windows.Forms.CheckBox();
            this.autoDetectInterfaces = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.maxHorizontalSlots = new System.Windows.Forms.ComboBox();
            this.maxVerticalSlots = new System.Windows.Forms.ComboBox();
            this.maxTotalSlots = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.visibleInterfaces = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.graphTimeSpan = new System.Windows.Forms.ComboBox();
            this.apply = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.visibleInterfaces.SuspendLayout();
            this.SuspendLayout();
            // 
            // autoStartup
            // 
            this.autoStartup.AutoSize = true;
            this.autoStartup.Location = new System.Drawing.Point(12, 12);
            this.autoStartup.Name = "autoStartup";
            this.autoStartup.Size = new System.Drawing.Size(144, 17);
            this.autoStartup.TabIndex = 0;
            this.autoStartup.Text = "Launch at system startup";
            this.autoStartup.UseVisualStyleBackColor = true;
            // 
            // checkForUpdates
            // 
            this.checkForUpdates.AutoSize = true;
            this.checkForUpdates.Location = new System.Drawing.Point(12, 35);
            this.checkForUpdates.Name = "checkForUpdates";
            this.checkForUpdates.Size = new System.Drawing.Size(163, 17);
            this.checkForUpdates.TabIndex = 1;
            this.checkForUpdates.Text = "Check for updates on startup";
            this.checkForUpdates.UseVisualStyleBackColor = true;
            // 
            // alwaysOnTop
            // 
            this.alwaysOnTop.AutoSize = true;
            this.alwaysOnTop.Location = new System.Drawing.Point(12, 58);
            this.alwaysOnTop.Name = "alwaysOnTop";
            this.alwaysOnTop.Size = new System.Drawing.Size(92, 17);
            this.alwaysOnTop.TabIndex = 2;
            this.alwaysOnTop.Text = "Always on top";
            this.alwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // autoDetectInterfaces
            // 
            this.autoDetectInterfaces.AutoSize = true;
            this.autoDetectInterfaces.Location = new System.Drawing.Point(12, 81);
            this.autoDetectInterfaces.Name = "autoDetectInterfaces";
            this.autoDetectInterfaces.Size = new System.Drawing.Size(132, 17);
            this.autoDetectInterfaces.TabIndex = 3;
            this.autoDetectInterfaces.Text = "Auto-Detect interfaces";
            this.toolTip.SetToolTip(this.autoDetectInterfaces, "It will automatically refresh the gadget when interface changes are detected");
            this.autoDetectInterfaces.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Graph timespan:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.maxHorizontalSlots);
            this.groupBox1.Controls.Add(this.maxVerticalSlots);
            this.groupBox1.Controls.Add(this.maxTotalSlots);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 134);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(241, 88);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Maximum number of gadget slots";
            this.toolTip.SetToolTip(this.groupBox1, "Maximum number of interfaces to show in the gadget window");
            // 
            // maxHorizontalSlots
            // 
            this.maxHorizontalSlots.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxHorizontalSlots.FormattingEnabled = true;
            this.maxHorizontalSlots.Items.AddRange(new object[] {
            "1",
            "2"});
            this.maxHorizontalSlots.Location = new System.Drawing.Point(66, 50);
            this.maxHorizontalSlots.Name = "maxHorizontalSlots";
            this.maxHorizontalSlots.Size = new System.Drawing.Size(39, 21);
            this.maxHorizontalSlots.TabIndex = 5;
            this.maxHorizontalSlots.SelectedIndexChanged += new System.EventHandler(this.maxHorizontalSlots_SelectedIndexChanged);
            // 
            // maxVerticalSlots
            // 
            this.maxVerticalSlots.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxVerticalSlots.FormattingEnabled = true;
            this.maxVerticalSlots.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.maxVerticalSlots.Location = new System.Drawing.Point(66, 25);
            this.maxVerticalSlots.Name = "maxVerticalSlots";
            this.maxVerticalSlots.Size = new System.Drawing.Size(39, 21);
            this.maxVerticalSlots.TabIndex = 4;
            this.maxVerticalSlots.SelectedIndexChanged += new System.EventHandler(this.maxVerticalSlots_SelectedIndexChanged);
            // 
            // maxTotalSlots
            // 
            this.maxTotalSlots.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxTotalSlots.Enabled = false;
            this.maxTotalSlots.Location = new System.Drawing.Point(190, 50);
            this.maxTotalSlots.Name = "maxTotalSlots";
            this.maxTotalSlots.Size = new System.Drawing.Size(28, 20);
            this.maxTotalSlots.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(187, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Total";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Horizontal";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Vertical";
            // 
            // visibleInterfaces
            // 
            this.visibleInterfaces.AutoSize = true;
            this.visibleInterfaces.Controls.Add(this.flowLayoutPanel1);
            this.visibleInterfaces.Location = new System.Drawing.Point(12, 228);
            this.visibleInterfaces.Margin = new System.Windows.Forms.Padding(3, 3, 12, 50);
            this.visibleInterfaces.Name = "visibleInterfaces";
            this.visibleInterfaces.Size = new System.Drawing.Size(241, 53);
            this.visibleInterfaces.TabIndex = 7;
            this.visibleInterfaces.TabStop = false;
            this.visibleInterfaces.Text = "Visible interfaces in gadget";
            this.toolTip.SetToolTip(this.visibleInterfaces, "Checked interfaces will be shown in the gadget window if slots are vailable");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(9, 23);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(218, 11);
            this.flowLayoutPanel1.TabIndex = 10;
            // 
            // graphTimeSpan
            // 
            this.graphTimeSpan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.graphTimeSpan.FormattingEnabled = true;
            this.graphTimeSpan.Items.AddRange(new object[] {
            "20",
            "40",
            "60",
            "80",
            "100",
            "120",
            "140"});
            this.graphTimeSpan.Location = new System.Drawing.Point(102, 107);
            this.graphTimeSpan.Name = "graphTimeSpan";
            this.graphTimeSpan.Size = new System.Drawing.Size(42, 21);
            this.graphTimeSpan.TabIndex = 5;
            // 
            // apply
            // 
            this.apply.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.apply.Location = new System.Drawing.Point(97, 287);
            this.apply.Name = "apply";
            this.apply.Size = new System.Drawing.Size(75, 23);
            this.apply.TabIndex = 8;
            this.apply.Text = "Apply";
            this.apply.UseVisualStyleBackColor = true;
            this.apply.Click += new System.EventHandler(this.apply_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(263, 322);
            this.Controls.Add(this.apply);
            this.Controls.Add(this.visibleInterfaces);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.graphTimeSpan);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.autoDetectInterfaces);
            this.Controls.Add(this.alwaysOnTop);
            this.Controls.Add(this.checkForUpdates);
            this.Controls.Add(this.autoStartup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.visibleInterfaces.ResumeLayout(false);
            this.visibleInterfaces.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox autoStartup;
        private System.Windows.Forms.CheckBox checkForUpdates;
        private System.Windows.Forms.CheckBox alwaysOnTop;
        private System.Windows.Forms.CheckBox autoDetectInterfaces;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ComboBox graphTimeSpan;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox maxVerticalSlots;
        private System.Windows.Forms.TextBox maxTotalSlots;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox maxHorizontalSlots;
        private System.Windows.Forms.GroupBox visibleInterfaces;
        private System.Windows.Forms.Button apply;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}