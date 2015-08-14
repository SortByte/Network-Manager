namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    partial class EditItemForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditItemForm));
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxRoute.SuspendLayout();
            this.SuspendLayout();
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
            this.groupBoxRoute.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRoute.Name = "groupBoxRoute";
            this.groupBoxRoute.Size = new System.Drawing.Size(519, 230);
            this.groupBoxRoute.TabIndex = 4;
            this.groupBoxRoute.TabStop = false;
            this.groupBoxRoute.Text = "IPv4 Route";
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
            this.button2.Text = "Save";
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
            // EditItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 256);
            this.Controls.Add(this.groupBoxRoute);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "EditItemForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit Saved Route";
            this.Shown += new System.EventHandler(this.EditItemForm_Shown);
            this.groupBoxRoute.ResumeLayout(false);
            this.groupBoxRoute.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxRoute;
        private System.Windows.Forms.TextBox savedRouteName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button2;
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
        private System.Windows.Forms.ToolTip toolTip1;
    }
}