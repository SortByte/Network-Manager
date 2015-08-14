using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lib.Network;
using Lib.WinAPI;
using Lib.Forms;
using Network_Manager.Jobs;

namespace Network_Manager.Gadget.ControlPanel.LoadBalancing
{
    public partial class LoadBalancingForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("Load Balancer");
        private EventHandler statusEventHandler = null;

        public LoadBalancingForm()
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
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
            {
                int oldHeight = groupBox1.Height;
                // TAP LB
                CheckBox checkBox1 = new CheckBox();
                checkBox1.Tag = nic.Guid;
                checkBox1.Text = nic.Name;
                checkBox1.Enabled = false;
                checkBox1.Width = tabControl1.Width - 40;
                tapInterfaces.Controls.Add(checkBox1);
                if (nic.IPv4Gateway.Count > 0 &&
                    !Jobs.Extensions.LoadBalancer.TapInterface.IsTap(nic.Guid))
                {
                    EventHandler handler = null;
                    handler = new EventHandler((s, e) =>
                    {
                        nic.DefaultIPv4GatewayChecked -= handler;
                        if (nic.DefaultIPv4GatewayMac != null)
                        {
                            Invoke(new Action(() => { checkBox1.Enabled = true; }));
                            if (!Global.Config.LoadBalancer.ExcludedInterfacesForTap.Contains(nic.Guid))
                                Invoke(new Action(() => { checkBox1.Checked = true; }));
                        }
                    });
                    nic.DefaultIPv4GatewayChecked += handler;
                    nic.CheckDefaultIPv4Gateway();
                }
                
                // Windows LB
                CheckBox checkBox2 = new CheckBox();
                checkBox2.Tag = nic.Guid;
                checkBox2.Text = nic.Name;
                checkBox2.Width = tabControl1.Width - 40;
                windowsInterfaces.Controls.Add(checkBox2);
                if (!Global.Config.LoadBalancer.ExcludedInterfacesForWindows.Contains(nic.Guid))
                    checkBox2.Checked = true;
                tabControl1.Height += groupBox1.Height - oldHeight;
            }
            new TextBoxMask(textBox1, TextBoxMask.Mask.Numeric);
            new TextBoxMask(textBox2, TextBoxMask.Mask.Numeric);
            textBox1.Text = "10";
            textBox2.Text = "10";
            // update status
            status.Text = Jobs.Extensions.LoadBalancer.Status.Message;
            status.ForeColor = Jobs.Extensions.LoadBalancer.Status.Color;
            statusEventHandler = new EventHandler((s, e) =>
            {
                Invoke(new Action(UpdateControls));
            });
            Jobs.Extensions.LoadBalancer.Status.Changed += statusEventHandler;
            Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.sortbyte.com/software-programs/networking/network-manager/kb/1003");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.sortbyte.com/software-programs/networking/network-manager/kb/1001");
        }

        private void LoadBalancerForm_Shown(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            Jobs.Extensions.LoadBalancer.TapInterface.Check();
            status.Text = Jobs.Extensions.LoadBalancer.Status.Message;
            status.ForeColor = Jobs.Extensions.LoadBalancer.Status.Color;
            if (Jobs.Extensions.LoadBalancer.TapInterface.Guid != null)
            {
                installTapDriver.Enabled = false;
                uninstallTapDriver.Enabled = true;
                if (Jobs.Extensions.LoadBalancer.Status.State == Jobs.Extensions.LoadBalancer.State.Stopped ||
                    Jobs.Extensions.LoadBalancer.Status.State == Jobs.Extensions.LoadBalancer.State.Running ||
                    Jobs.Extensions.LoadBalancer.Status.State == Jobs.Extensions.LoadBalancer.State.Failed)
                {
                    startLoadBalancer.Enabled = true;
                }
                else
                {
                    startLoadBalancer.Enabled = false;
                }
                if (Jobs.Extensions.LoadBalancer.TapInterface.Connected)
                    startLoadBalancer.Text = "Stop";
                else
                    startLoadBalancer.Text = "Start";
            }
            else
            {
                startLoadBalancer.Text = "Start";
                startLoadBalancer.Enabled = false;
                installTapDriver.Enabled = true;
                uninstallTapDriver.Enabled = false;
            }
        }

        private void startStopLoadBalancer_Click(object sender, EventArgs e)
        {
            if (Jobs.Extensions.LoadBalancer.TapInterface.Connected)
            {
                Jobs.Extensions.LoadBalancer.Stop();
            }
            else
            {
                Global.Config.LoadBalancer.ExcludedInterfacesForTap.Clear();
                List<NetworkInterface> loadBalancingInterfaces = new List<NetworkInterface>();
                foreach (Control control in tapInterfaces.Controls)
                    if (!((CheckBox)control).Checked)
                        Global.Config.LoadBalancer.ExcludedInterfacesForTap.Add((string)((CheckBox)control).Tag);
                    else
                        loadBalancingInterfaces.Add(Global.NetworkInterfaces[(string)((CheckBox)control).Tag]);
                Global.Config.Save();
                Jobs.Extensions.LoadBalancer.Start(loadBalancingInterfaces);
            }
            UpdateControls();
        }

        private void LoadBalancerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            Jobs.Extensions.LoadBalancer.Status.Changed -= statusEventHandler;
            busyForm.Done.SetResult(true);
        }

        private void installTap_Click(object sender, EventArgs e)
        {
            if (Jobs.Extensions.LoadBalancer.TapInterface.Install())
                UpdateControls();
        }

        private void uninstallTap_Click(object sender, EventArgs e)
        {
            if (Jobs.Extensions.LoadBalancer.TapInterface.Uninstall())
                UpdateControls();
        }

        private void advanced_Click(object sender, EventArgs e)
        {
            AdvancedForm form = new AdvancedForm();
            form.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadingForm splash = new LoadingForm("Initializing ...");
            Global.Config.LoadBalancer.ExcludedInterfacesForWindows.Clear();
            List<NetworkInterface> loadBalancingInterfaces = new List<NetworkInterface>();
            foreach (Control control in windowsInterfaces.Controls)
                if (!((CheckBox)control).Checked)
                    Global.Config.LoadBalancer.ExcludedInterfacesForWindows.Add((string)((CheckBox)control).Tag);
                else
                    loadBalancingInterfaces.Add(Global.NetworkInterfaces[(string)((CheckBox)control).Tag]);
            Global.Config.Save();
            // Configure high metrics on interfaces that are not used
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                if (!loadBalancingInterfaces.Any(i => i.Guid == nic.Guid) &&
                    (nic.IPv4Gateway.Count > 0 || nic.IPv6Gateway.Count > 0))
                {
                    splash.UpdateStatus("Configuring " + nic.Name + " ...");
                    nic.SetInterfaceMetric("4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                        nic.EditIPv4Gateway(ip.Address, "4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                        nic.EditIPv6Gateway(ip.Address, "4000");    
                }
            // Configure the used interfaces with the specified metrics
            foreach (NetworkInterface nic in loadBalancingInterfaces)
            {
                splash.UpdateStatus("Configuring " + nic.Name + " ...");
                nic.SetInterfaceMetric(textBox1.Text);
                foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                    nic.EditIPv4Gateway(ip.Address, textBox2.Text);
                foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                    nic.EditIPv6Gateway(ip.Address, textBox2.Text);
            }
            splash.Stop();
            Program.Refresh();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Regex.IsMatch(textBox1.Text, @"^\s*$"))
            {
                textBox1.Text = "0";
                return;
            }
            if (Regex.IsMatch(textBox2.Text, @"^\s*$"))
            {
                textBox2.Text = "0";
                return;
            }
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                textBox3.Text = textBox2.Text;
            else
                textBox3.Text = (int.Parse(textBox1.Text) + int.Parse(textBox2.Text)).ToString();
        }
    }
}
