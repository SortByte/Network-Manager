using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinLib.Network;
using WinLib.WinAPI;


namespace Network_Manager.Gadget.ControlPanel.LoadBalancing
{
    public partial class AdvancedForm : Form
    {
        public AdvancedForm()
        {
            InitializeComponent();
            tapIPAddress.Text = Global.Config.LoadBalancer.IPv4LocalAddresses.First().Address;
            tapSubnet.Text = Global.Config.LoadBalancer.IPv4LocalAddresses.First().Subnet;
            tapGateway.Text = Global.Config.LoadBalancer.IPv4GatewayAddresses.First().Address;
            tapDnsServer.Text = Global.Config.LoadBalancer.IPv4DnsAddresses.First();
            checkBox1.Checked = Global.Config.LoadBalancer.ShowTrayTipsWarnings;
        }

        private void apply_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigs())
                return;
            Global.Config.LoadBalancer.IPv4LocalAddresses.Clear();
            Global.Config.LoadBalancer.IPv4LocalAddresses.Add(new NetworkInterface.IPHostAddress(tapIPAddress.Text, tapSubnet.Text));
            Global.Config.LoadBalancer.IPv4GatewayAddresses.Clear();
            Global.Config.LoadBalancer.IPv4GatewayAddresses.Add(new NetworkInterface.IPGatewayAddress(tapGateway.Text, 1));
            Global.Config.LoadBalancer.IPv4DnsAddresses.Clear();
            Global.Config.LoadBalancer.IPv4DnsAddresses.Add(tapDnsServer.Text);
            Global.Config.Save();
            MessageBox.Show("Settings will apply next time you start the Load Balancer.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void routingTable_Click(object sender, EventArgs e)
        {
            RoutingTableForm form = new RoutingTableForm();
        }

        private bool ValidateConfigs()
        {
            string ip = tapIPAddress.Text;
            string subnet = tapSubnet.Text;
            string gateway = tapGateway.Text;
            string dns = tapDnsServer.Text;
            if (Regex.IsMatch(tapSubnet.Text, @"^\s*$"))
                tapSubnet.Text = "255.255.255.0";
            if (!IP.ValidateIPv4Mask(ref subnet))
            {
                new BalloonTip("Warning", "Invalid IPv4 subnet mask", tapSubnet, BalloonTip.ICON.WARNING);
                return false;
            }
            tapSubnet.Text = subnet;
            if (!IP.ValidateIPv4(ref ip, subnet))
            {
                new BalloonTip("Warning", "Invalid IPv4 address", tapIPAddress, BalloonTip.ICON.WARNING);
                return false;
            }
            tapIPAddress.Text = ip;
            if (!IP.ValidateIPv4(ref gateway))
            {
                new BalloonTip("Warning", "Invalid IPv4 address", tapGateway, BalloonTip.ICON.WARNING);
                return false;
            }
            tapGateway.Text = gateway;
            if (!IP.CheckIfSameNetwork(ip, gateway, subnet))
            {
                new BalloonTip("Warning", "The gateway IPv4 address is not in the same natework", tapGateway, BalloonTip.ICON.WARNING);
                return false;
            }
            if (IP.CheckIfSameNetwork(ip, gateway, "255.255.255.255"))
            {
                new BalloonTip("Warning", "Default gateway can't be the same as the local address.", tapGateway, BalloonTip.ICON.WARNING);
                return false;
            }
            if (!IP.ValidateIPv4(ref dns))
            {
                new BalloonTip("Warning", "Invalid IPv4 address", tapDnsServer, BalloonTip.ICON.WARNING);
                return false;
            }
            tapDnsServer.Text = dns;
            return true;
        }
    }
}
