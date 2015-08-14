namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    partial class AddNodeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddNodeForm));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxRoute = new System.Windows.Forms.GroupBox();
            this.savedRouteName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBoxGroup = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxRoute.SuspendLayout();
            this.groupBoxGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Group",
            "IPv4 Route",
            "IPv6 Route"});
            this.comboBox1.Location = new System.Drawing.Point(136, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Saved route node type:";
            // 
            // groupBoxRoute
            // 
            this.groupBoxRoute.Controls.Add(this.savedRouteName);
            this.groupBoxRoute.Controls.Add(this.label8);
            this.groupBoxRoute.Controls.Add(this.button2);
            this.groupBoxRoute.Controls.Add(this.routeGateway);
            this.groupBoxRoute.Controls.Add(this.routeInterface);
            this.groupBoxRoute.Controls.Add(this.routeMetric);
            this.groupBoxRoute.Controls.Add(this.routePrefix);
            this.groupBoxRoute.Controls.Add(this.routeDestination);
            this.groupBoxRoute.Controls.Add(this.routeGatewayMode);
            this.groupBoxRoute.Controls.Add(this.label6);
            this.groupBoxRoute.Controls.Add(this.label5);
            this.groupBoxRoute.Controls.Add(this.label4);
            this.groupBoxRoute.Controls.Add(this.label3);
            this.groupBoxRoute.Controls.Add(this.label2);
            this.groupBoxRoute.Location = new System.Drawing.Point(15, 39);
            this.groupBoxRoute.Name = "groupBoxRoute";
            this.groupBoxRoute.Size = new System.Drawing.Size(519, 230);
            this.groupBoxRoute.TabIndex = 3;
            this.groupBoxRoute.TabStop = false;
            this.groupBoxRoute.Text = "IPv4 Route";
            this.groupBoxRoute.Visible = false;
            // 
            // savedRouteName
            // 
            this.savedRouteName.Location = new System.Drawing.Point(87, 29);
            this.savedRouteName.Name = "savedRouteName";
            this.savedRouteName.Size = new System.Drawing.Size(192, 20);
            this.savedRouteName.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 32);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Route name:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(222, 201);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Add";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // routeGateway
            // 
            this.routeGateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeGateway.FormattingEnabled = true;
            this.routeGateway.Location = new System.Drawing.Point(87, 106);
            this.routeGateway.Name = "routeGateway";
            this.routeGateway.Size = new System.Drawing.Size(192, 21);
            this.routeGateway.TabIndex = 11;
            // 
            // routeInterface
            // 
            this.routeInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeInterface.FormattingEnabled = true;
            this.routeInterface.Location = new System.Drawing.Point(87, 133);
            this.routeInterface.Name = "routeInterface";
            this.routeInterface.Size = new System.Drawing.Size(412, 21);
            this.routeInterface.TabIndex = 10;
            this.routeInterface.SelectedIndexChanged += new System.EventHandler(this.routeInterface_SelectedIndexChanged);
            // 
            // routeMetric
            // 
            this.routeMetric.Location = new System.Drawing.Point(87, 160);
            this.routeMetric.MaxLength = 4;
            this.routeMetric.Name = "routeMetric";
            this.routeMetric.Size = new System.Drawing.Size(192, 20);
            this.routeMetric.TabIndex = 9;
            this.routeMetric.Text = "0";
            // 
            // routePrefix
            // 
            this.routePrefix.Location = new System.Drawing.Point(87, 81);
            this.routePrefix.Name = "routePrefix";
            this.routePrefix.Size = new System.Drawing.Size(192, 20);
            this.routePrefix.TabIndex = 7;
            // 
            // routeDestination
            // 
            this.routeDestination.Location = new System.Drawing.Point(87, 55);
            this.routeDestination.Name = "routeDestination";
            this.routeDestination.Size = new System.Drawing.Size(192, 20);
            this.routeDestination.TabIndex = 6;
            // 
            // routeGatewayMode
            // 
            this.routeGatewayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routeGatewayMode.FormattingEnabled = true;
            this.routeGatewayMode.Items.AddRange(new object[] {
            "Manual",
            "Use interface default gateway",
            "No gateway (direct link)"});
            this.routeGatewayMode.Location = new System.Drawing.Point(285, 106);
            this.routeGatewayMode.Name = "routeGatewayMode";
            this.routeGatewayMode.Size = new System.Drawing.Size(214, 21);
            this.routeGatewayMode.TabIndex = 5;
            this.routeGatewayMode.SelectedIndexChanged += new System.EventHandler(this.routeGatewayMode_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 163);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Metric:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Interface:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Gateway:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Subnet:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Destination:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(121, 29);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(231, 20);
            this.textBox1.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 32);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Group name:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(148, 67);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBoxGroup
            // 
            this.groupBoxGroup.Controls.Add(this.label7);
            this.groupBoxGroup.Controls.Add(this.button1);
            this.groupBoxGroup.Controls.Add(this.textBox1);
            this.groupBoxGroup.Location = new System.Drawing.Point(15, 39);
            this.groupBoxGroup.Name = "groupBoxGroup";
            this.groupBoxGroup.Size = new System.Drawing.Size(410, 105);
            this.groupBoxGroup.TabIndex = 7;
            this.groupBoxGroup.TabStop = false;
            this.groupBoxGroup.Text = "Group";
            this.groupBoxGroup.Visible = false;
            // 
            // AddNodeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(681, 319);
            this.Controls.Add(this.groupBoxGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.groupBoxRoute);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AddNodeForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Saved Route Node";
            this.SizeChanged += new System.EventHandler(this.AddNodeForm_SizeChanged);
            this.groupBoxRoute.ResumeLayout(false);
            this.groupBoxRoute.PerformLayout();
            this.groupBoxGroup.ResumeLayout(false);
            this.groupBoxGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxRoute;
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
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBoxGroup;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox savedRouteName;
        private System.Windows.Forms.Label label8;
    }
}