using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lib.Network;

namespace Network_Manager
{
    public partial class UpdateForm : Form
    {
        public UpdateForm(VersionInfo versionInfo)
        {
            InitializeComponent();
            label1.Text = "New update is avalable.\nYour version is " + versionInfo.MainModule.CurrentVersion.ToString() +
                " and the latest version is " + versionInfo.MainModule.LatestVersion.ToString() + "." +
                "\n\nDo you want to update (size = " + Unit.AutoScale(versionInfo.MainModule.Urls[0].Size, "B") + ")?";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.sortbyte.com/software-programs/networking/network-manager/changelog");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
