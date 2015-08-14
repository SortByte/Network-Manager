namespace Network_Manager.Gadget.ControlPanel.IPSessions
{
    partial class IPSessionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IPSessionsForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.detectRemoteUdp = new System.Windows.Forms.CheckBox();
            this.resolveIP = new System.Windows.Forms.CheckBox();
            this.getBytes = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.filterProtocol = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(12, 12);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(281, 437);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(299, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(613, 463);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Process";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Local Address";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Local Port";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Remote Address";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Remote Port";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Protocol";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "State";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Received Bytes";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Sent Bytes";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(12, 455);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(281, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "[Process path]";
            // 
            // detectRemoteUdp
            // 
            this.detectRemoteUdp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.detectRemoteUdp.AutoSize = true;
            this.detectRemoteUdp.Location = new System.Drawing.Point(12, 481);
            this.detectRemoteUdp.Name = "detectRemoteUdp";
            this.detectRemoteUdp.Size = new System.Drawing.Size(184, 17);
            this.detectRemoteUdp.TabIndex = 3;
            this.detectRemoteUdp.Text = "Detect remote UDP IPs and ports";
            this.toolTip1.SetToolTip(this.detectRemoteUdp, "It works only for the IP sessions that are active, on which packets are being exc" +
        "hanged");
            this.detectRemoteUdp.UseVisualStyleBackColor = true;
            this.detectRemoteUdp.CheckedChanged += new System.EventHandler(this.detectRemoteUdp_CheckedChanged);
            // 
            // resolveIP
            // 
            this.resolveIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resolveIP.AutoSize = true;
            this.resolveIP.Location = new System.Drawing.Point(12, 504);
            this.resolveIP.Name = "resolveIP";
            this.resolveIP.Size = new System.Drawing.Size(78, 17);
            this.resolveIP.TabIndex = 4;
            this.resolveIP.Text = "Resolve IP";
            this.toolTip1.SetToolTip(this.resolveIP, "It resolves remote IP addresses into one of their corresponding DNS (reverse DNS " +
        "lookup),\r\nif they have one");
            this.resolveIP.UseVisualStyleBackColor = true;
            // 
            // getBytes
            // 
            this.getBytes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.getBytes.AutoSize = true;
            this.getBytes.Location = new System.Drawing.Point(96, 504);
            this.getBytes.Name = "getBytes";
            this.getBytes.Size = new System.Drawing.Size(71, 17);
            this.getBytes.TabIndex = 5;
            this.getBytes.Text = "Get bytes";
            this.toolTip1.SetToolTip(this.getBytes, "It retieves the amount of bytes transfered by each IP session");
            this.getBytes.UseVisualStyleBackColor = true;
            this.getBytes.CheckedChanged += new System.EventHandler(this.getBytes_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(299, 481);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Route remote IP";
            this.toolTip1.SetToolTip(this.button1, "Create a new route using the selected IP session\'s remote address as the destinat" +
        "ion");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(409, 481);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Copy remote IP end point";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "IPv4 Sessions Only",
            "IPv6 Sessions Only",
            "IPv4 & IPv6 Sessions"});
            this.comboBox1.Location = new System.Drawing.Point(638, 483);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(157, 21);
            this.comboBox1.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(598, 486);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Show";
            // 
            // filterProtocol
            // 
            this.filterProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.filterProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterProtocol.FormattingEnabled = true;
            this.filterProtocol.Items.AddRange(new object[] {
            "TCP Only",
            "UDP Only",
            "TCP & UDP"});
            this.filterProtocol.Location = new System.Drawing.Point(801, 483);
            this.filterProtocol.Name = "filterProtocol";
            this.filterProtocol.Size = new System.Drawing.Size(111, 21);
            this.filterProtocol.TabIndex = 10;
            // 
            // IPSessionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 533);
            this.Controls.Add(this.filterProtocol);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.getBytes);
            this.Controls.Add(this.resolveIP);
            this.Controls.Add(this.detectRemoteUdp);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.treeView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IPSessionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IP Sessions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IPSessionsForm_FormClosing);
            this.Load += new System.EventHandler(this.IPSessionsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox detectRemoteUdp;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox resolveIP;
        private System.Windows.Forms.CheckBox getBytes;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ComboBox filterProtocol;
    }
}