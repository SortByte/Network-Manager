﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinLib.WinAPI;

namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    public partial class EditGroupForm : Form
    {
        private bool rename;
        private TreeView treeView;

        public EditGroupForm(TreeView treeView, bool rename = false)
        {
            InitializeComponent();
            this.rename = rename;
            if (rename)
            {
                Text = "Edit Saved Route Group";
                button1.Text = "Save";
                Config.SavedRouteNode editNode = Global.Config.SavedRoutes.GetSelectedNode(treeView);
                textBox1.Text = editNode.Name;
                checkBox1.Checked = ((Config.SavedRouteGroup)editNode).AutoLoadOnStartup;
            }
            else
            {
                Text = "Create Saved Route Group";
                button1.Text = "Create";
            }
            this.treeView = treeView;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                new BalloonTip("Warning", "Group name can not be empty", textBox1, BalloonTip.ICON.WARNING);
                return;
            }
            Config.SavedRouteGroup group = new Config.SavedRouteGroup();
            group.Name = textBox1.Text;
            group.AutoLoadOnStartup = checkBox1.Checked;
            int result = Global.Config.SavedRoutes.AddNode(treeView, group, rename);
            if (result == 1)
            {
                new BalloonTip("Error", "Invalid destination", button1, BalloonTip.ICON.ERROR);
                return;
            }
            if (result == 2)
            {
                new BalloonTip("Warning", "Group name already used at current destination", textBox1, BalloonTip.ICON.WARNING);
                return;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
