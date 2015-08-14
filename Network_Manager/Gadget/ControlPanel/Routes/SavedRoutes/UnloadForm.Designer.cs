namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    partial class UnloadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnloadForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.defaultInterfaceMode = new System.Windows.Forms.ComboBox();
            this.defaultIPv4Configurations = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.defaultIPv4GatewayMode = new System.Windows.Forms.ComboBox();
            this.defaultIPv4Gateway = new System.Windows.Forms.ComboBox();
            this.defaultIPv4Interface = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.defaultIPv6GatewayMode = new System.Windows.Forms.ComboBox();
            this.defaultIPv6Gateway = new System.Windows.Forms.ComboBox();
            this.defaultIPv6Interface = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.defaultIPv4Configurations.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(674, 219);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Destination";
            this.columnHeader1.Width = 72;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Prefix";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Gateway";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Interface Index";
            this.columnHeader4.Width = 114;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Metric";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Route Name";
            this.columnHeader6.Width = 102;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Status";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(304, 421);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Unload";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // defaultInterfaceMode
            // 
            this.defaultInterfaceMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.defaultInterfaceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultInterfaceMode.FormattingEnabled = true;
            this.defaultInterfaceMode.Items.AddRange(new object[] {
            "If route is not found or has has a different interface, use:",
            "Override all selected routes with:"});
            this.defaultInterfaceMode.Location = new System.Drawing.Point(12, 237);
            this.defaultInterfaceMode.Name = "defaultInterfaceMode";
            this.defaultInterfaceMode.Size = new System.Drawing.Size(347, 21);
            this.defaultInterfaceMode.TabIndex = 2;
            this.defaultInterfaceMode.SelectedIndexChanged += new System.EventHandler(this.defaultInterfaceMode_SelectedIndexChanged);
            // 
            // defaultIPv4Configurations
            // 
            this.defaultIPv4Configurations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv4Configurations.Controls.Add(this.tabControl1);
            this.defaultIPv4Configurations.Location = new System.Drawing.Point(12, 264);
            this.defaultIPv4Configurations.Name = "defaultIPv4Configurations";
            this.defaultIPv4Configurations.Size = new System.Drawing.Size(674, 151);
            this.defaultIPv4Configurations.TabIndex = 6;
            this.defaultIPv4Configurations.TabStop = false;
            this.defaultIPv4Configurations.Text = "Default Configurations";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(6, 19);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(662, 125);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.defaultIPv4GatewayMode);
            this.tabPage1.Controls.Add(this.defaultIPv4Gateway);
            this.tabPage1.Controls.Add(this.defaultIPv4Interface);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(654, 99);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "IPv4";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // defaultIPv4GatewayMode
            // 
            this.defaultIPv4GatewayMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv4GatewayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv4GatewayMode.FormattingEnabled = true;
            this.defaultIPv4GatewayMode.Items.AddRange(new object[] {
            "Manual",
            "Use interface default gateway",
            "No gateway (direct link)",
            "Use loaded route gateway regardless"});
            this.defaultIPv4GatewayMode.Location = new System.Drawing.Point(412, 53);
            this.defaultIPv4GatewayMode.Name = "defaultIPv4GatewayMode";
            this.defaultIPv4GatewayMode.Size = new System.Drawing.Size(213, 21);
            this.defaultIPv4GatewayMode.TabIndex = 9;
            this.defaultIPv4GatewayMode.SelectedIndexChanged += new System.EventHandler(this.defaultIPv4GatewayMode_SelectedIndexChanged);
            // 
            // defaultIPv4Gateway
            // 
            this.defaultIPv4Gateway.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv4Gateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv4Gateway.FormattingEnabled = true;
            this.defaultIPv4Gateway.Location = new System.Drawing.Point(72, 53);
            this.defaultIPv4Gateway.Name = "defaultIPv4Gateway";
            this.defaultIPv4Gateway.Size = new System.Drawing.Size(334, 21);
            this.defaultIPv4Gateway.TabIndex = 8;
            // 
            // defaultIPv4Interface
            // 
            this.defaultIPv4Interface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv4Interface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv4Interface.FormattingEnabled = true;
            this.defaultIPv4Interface.Location = new System.Drawing.Point(72, 24);
            this.defaultIPv4Interface.Name = "defaultIPv4Interface";
            this.defaultIPv4Interface.Size = new System.Drawing.Size(553, 21);
            this.defaultIPv4Interface.TabIndex = 7;
            this.defaultIPv4Interface.SelectedIndexChanged += new System.EventHandler(this.defaultIPv4Interface_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Gateway:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Interface:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.defaultIPv6GatewayMode);
            this.tabPage2.Controls.Add(this.defaultIPv6Gateway);
            this.tabPage2.Controls.Add(this.defaultIPv6Interface);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(654, 99);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "IPv6";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // defaultIPv6GatewayMode
            // 
            this.defaultIPv6GatewayMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv6GatewayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv6GatewayMode.FormattingEnabled = true;
            this.defaultIPv6GatewayMode.Items.AddRange(new object[] {
            "Manual",
            "Use interface default gateway",
            "No gateway (direct link)",
            "Use loaded route gateway regardless"});
            this.defaultIPv6GatewayMode.Location = new System.Drawing.Point(412, 53);
            this.defaultIPv6GatewayMode.Name = "defaultIPv6GatewayMode";
            this.defaultIPv6GatewayMode.Size = new System.Drawing.Size(213, 21);
            this.defaultIPv6GatewayMode.TabIndex = 9;
            this.defaultIPv6GatewayMode.SelectedIndexChanged += new System.EventHandler(this.defaultIPv6GatewayMode_SelectedIndexChanged);
            // 
            // defaultIPv6Gateway
            // 
            this.defaultIPv6Gateway.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv6Gateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv6Gateway.FormattingEnabled = true;
            this.defaultIPv6Gateway.Location = new System.Drawing.Point(72, 53);
            this.defaultIPv6Gateway.Name = "defaultIPv6Gateway";
            this.defaultIPv6Gateway.Size = new System.Drawing.Size(334, 21);
            this.defaultIPv6Gateway.TabIndex = 8;
            // 
            // defaultIPv6Interface
            // 
            this.defaultIPv6Interface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultIPv6Interface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultIPv6Interface.FormattingEnabled = true;
            this.defaultIPv6Interface.Location = new System.Drawing.Point(72, 24);
            this.defaultIPv6Interface.Name = "defaultIPv6Interface";
            this.defaultIPv6Interface.Size = new System.Drawing.Size(553, 21);
            this.defaultIPv6Interface.TabIndex = 7;
            this.defaultIPv6Interface.SelectedIndexChanged += new System.EventHandler(this.defaultIPv6Interface_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Gateway:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Interface:";
            // 
            // UnloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 456);
            this.Controls.Add(this.defaultIPv4Configurations);
            this.Controls.Add(this.defaultInterfaceMode);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UnloadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Unload Routes";
            this.defaultIPv4Configurations.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ComboBox defaultInterfaceMode;
        private System.Windows.Forms.GroupBox defaultIPv4Configurations;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ComboBox defaultIPv4GatewayMode;
        private System.Windows.Forms.ComboBox defaultIPv4Gateway;
        private System.Windows.Forms.ComboBox defaultIPv4Interface;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox defaultIPv6GatewayMode;
        private System.Windows.Forms.ComboBox defaultIPv6Gateway;
        private System.Windows.Forms.ComboBox defaultIPv6Interface;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}