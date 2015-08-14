using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Network_Manager.Gadget.About
{
    public partial class AboutForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("About");
        public AboutForm()
        {
            if (Instance != null)
            {
                Instance.WindowState = FormWindowState.Normal;
                Instance.Activate();
                return;
            }
            Instance = this;
            Global.BusyForms.Enqueue(busyForm);
            InitializeComponent();
            label1.Text = "Network Manager v" + Global.VersionInfo.MainModule.CurrentVersion;
            Show();
        }

        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            busyForm.Done.SetResult(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Jobs.Extensions.LoadBalancer.TapInterface.PutUp();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new LicenseForm().ShowDialog();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SortByte/Network-Manager");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.sortbyte.com/software-programs/networking/network-manager");
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.sortbyte.com");
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SortByte/Network-Manager/issues");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.sortbyte.com");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.sortbyte.com");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.winpcap.org");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://pcapdot.net");
        }
    }
}
