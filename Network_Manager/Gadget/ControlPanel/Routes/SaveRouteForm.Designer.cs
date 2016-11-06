namespace Network_Manager.Gadget.ControlPanel.Routes
{
    partial class SaveRouteForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveRouteForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.routeGateway = new System.Windows.Forms.ComboBox();
            this.routeInterface = new System.Windows.Forms.ComboBox();
            this.routeMetric = new System.Windows.Forms.TextBox();
            this.routePrefix = new System.Windows.Forms.TextBox();
            this.routeDestination = new System.Windows.Forms.TextBox();
            this.routeGatewayMode = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.savedRouteName = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.deleteSavedRouteGroup = new System.Windows.Forms.Button();
            this.renameSavedRouteGroup = new System.Windows.Forms.Button();
            this.createSavedRouteGroup = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.routeGateway);
            this.groupBox1.Controls.Add(this.routeInterface);
            this.groupBox1.Controls.Add(this.routeMetric);
            this.groupBox1.Controls.Add(this.routePrefix);
            this.groupBox1.Controls.Add(this.routeDestination);
            this.groupBox1.Controls.Add(this.routeGatewayMode);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(519, 168);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IPv4 Route";
            // 
            // routeGateway
            // 
            this.routeGateway.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routeGateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeGateway.FormattingEnabled = true;
            this.routeGateway.Location = new System.Drawing.Point(85, 78);
            this.routeGateway.Name = "routeGateway";
            this.routeGateway.Size = new System.Drawing.Size(192, 21);
            this.routeGateway.TabIndex = 11;
            // 
            // routeInterface
            // 
            this.routeInterface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routeInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeInterface.FormattingEnabled = true;
            this.routeInterface.Location = new System.Drawing.Point(85, 105);
            this.routeInterface.Name = "routeInterface";
            this.routeInterface.Size = new System.Drawing.Size(412, 21);
            this.routeInterface.TabIndex = 10;
            this.routeInterface.SelectedIndexChanged += new System.EventHandler(this.routeInterface_SelectedIndexChanged);
            // 
            // routeMetric
            // 
            this.routeMetric.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routeMetric.Location = new System.Drawing.Point(85, 132);
            this.routeMetric.MaxLength = 4;
            this.routeMetric.Name = "routeMetric";
            this.routeMetric.Size = new System.Drawing.Size(192, 20);
            this.routeMetric.TabIndex = 9;
            this.routeMetric.Text = "0";
            // 
            // routePrefix
            // 
            this.routePrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routePrefix.Location = new System.Drawing.Point(85, 53);
            this.routePrefix.Name = "routePrefix";
            this.routePrefix.Size = new System.Drawing.Size(192, 20);
            this.routePrefix.TabIndex = 7;
            // 
            // routeDestination
            // 
            this.routeDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routeDestination.Location = new System.Drawing.Point(85, 27);
            this.routeDestination.Name = "routeDestination";
            this.routeDestination.Size = new System.Drawing.Size(192, 20);
            this.routeDestination.TabIndex = 6;
            // 
            // routeGatewayMode
            // 
            this.routeGatewayMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.routeGatewayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeGatewayMode.FormattingEnabled = true;
            this.routeGatewayMode.Items.AddRange(new object[] {
            "Manual",
            "Use interface default gateway",
            "No gateway (direct link)"});
            this.routeGatewayMode.Location = new System.Drawing.Point(283, 78);
            this.routeGatewayMode.Name = "routeGatewayMode";
            this.routeGatewayMode.Size = new System.Drawing.Size(214, 21);
            this.routeGatewayMode.TabIndex = 5;
            this.routeGatewayMode.SelectedIndexChanged += new System.EventHandler(this.routeGatewayMode_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Metric:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Interface:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Gateway:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Subnet:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Destination:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.savedRouteName);
            this.groupBox2.Controls.Add(this.treeView1);
            this.groupBox2.Controls.Add(this.deleteSavedRouteGroup);
            this.groupBox2.Controls.Add(this.renameSavedRouteGroup);
            this.groupBox2.Controls.Add(this.createSavedRouteGroup);
            this.groupBox2.Location = new System.Drawing.Point(12, 186);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(519, 238);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Saving location";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 198);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Route name:";
            // 
            // savedRouteName
            // 
            this.savedRouteName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.savedRouteName.Location = new System.Drawing.Point(97, 195);
            this.savedRouteName.Name = "savedRouteName";
            this.savedRouteName.Size = new System.Drawing.Size(416, 20);
            this.savedRouteName.TabIndex = 4;
            // 
            // treeView1
            // 
            this.treeView1.AllowDrop = true;
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(97, 19);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(416, 170);
            this.treeView1.TabIndex = 3;
            this.treeView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView1_ItemDrag);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
            this.treeView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView1_DragEnter);
            this.treeView1.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            // 
            // deleteSavedRouteGroup
            // 
            this.deleteSavedRouteGroup.Enabled = false;
            this.deleteSavedRouteGroup.Location = new System.Drawing.Point(6, 77);
            this.deleteSavedRouteGroup.Name = "deleteSavedRouteGroup";
            this.deleteSavedRouteGroup.Size = new System.Drawing.Size(85, 23);
            this.deleteSavedRouteGroup.TabIndex = 2;
            this.deleteSavedRouteGroup.Text = "Delete group";
            this.deleteSavedRouteGroup.UseVisualStyleBackColor = true;
            this.deleteSavedRouteGroup.Click += new System.EventHandler(this.deleteSavedRouteGroup_Click);
            // 
            // renameSavedRouteGroup
            // 
            this.renameSavedRouteGroup.Enabled = false;
            this.renameSavedRouteGroup.Location = new System.Drawing.Point(6, 48);
            this.renameSavedRouteGroup.Name = "renameSavedRouteGroup";
            this.renameSavedRouteGroup.Size = new System.Drawing.Size(85, 23);
            this.renameSavedRouteGroup.TabIndex = 1;
            this.renameSavedRouteGroup.Text = "Rename group";
            this.renameSavedRouteGroup.UseVisualStyleBackColor = true;
            this.renameSavedRouteGroup.Click += new System.EventHandler(this.renameSavedRouteGroup_Click);
            // 
            // createSavedRouteGroup
            // 
            this.createSavedRouteGroup.Enabled = false;
            this.createSavedRouteGroup.Location = new System.Drawing.Point(6, 19);
            this.createSavedRouteGroup.Name = "createSavedRouteGroup";
            this.createSavedRouteGroup.Size = new System.Drawing.Size(85, 23);
            this.createSavedRouteGroup.TabIndex = 0;
            this.createSavedRouteGroup.Text = "Create group";
            this.createSavedRouteGroup.UseVisualStyleBackColor = true;
            this.createSavedRouteGroup.Click += new System.EventHandler(this.createSavedRouteGroup_Click);
            // 
            // button4
            // 
            this.button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button4.Location = new System.Drawing.Point(230, 430);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "Save";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // SaveRouteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 465);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SaveRouteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Save Route";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox routeGateway;
        private System.Windows.Forms.ComboBox routeInterface;
        private System.Windows.Forms.TextBox routeMetric;
        private System.Windows.Forms.TextBox routePrefix;
        private System.Windows.Forms.TextBox routeDestination;
        private System.Windows.Forms.ComboBox routeGatewayMode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button deleteSavedRouteGroup;
        private System.Windows.Forms.Button renameSavedRouteGroup;
        private System.Windows.Forms.Button createSavedRouteGroup;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox savedRouteName;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}