using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lib.WinAPI;
using Lib.Network;
using Lib.Extensions;

namespace Network_Manager.Gadget.ControlPanel.ConfigureInterface
{
    public partial class SaveProfileForm : Form
    {
        public string ProfileName;
        public Config.InterfaceProfile.LoadingMode LoadMode;
        public List<NetworkInterface.IPHostAddress> IPv4Address = new List<NetworkInterface.IPHostAddress>();
        public List<NetworkInterface.IPGatewayAddress> IPv4Gateway = new List<NetworkInterface.IPGatewayAddress>();
        public List<string> IPv4DnsServer = new List<string>();
        public NetworkInterface.Dhcp DhcpEnabled = NetworkInterface.Dhcp.Unchanged;
        public NetworkInterface.Netbios NetbiosEnabled = NetworkInterface.Netbios.Unchanged;
        public int IPv4Mtu = -1;
        public List<NetworkInterface.IPHostAddress> IPv6Address = new List<NetworkInterface.IPHostAddress>();
        public List<NetworkInterface.IPGatewayAddress> IPv6Gateway = new List<NetworkInterface.IPGatewayAddress>();
        public List<string> IPv6DnsServer = new List<string>();
        public NetworkInterface.RouterDiscovery RouterDiscovery = NetworkInterface.RouterDiscovery.Unchanged;
        public int IPv6Mtu = -1;
        public int InterfaceMetric = -1;

        public SaveProfileForm(string name,
            bool ipv4Enabled,
            List<NetworkInterface.IPHostAddress> ipv4Address,
            List<NetworkInterface.IPGatewayAddress> ipv4Gateway,
            List<string> ipv4DnsServer,
            bool dhcpIP,
            bool dhcpDns,
            bool netbios,
            int ipv4Mtu,
            bool ipv6Enabled,
            List<NetworkInterface.IPHostAddress> ipv6Address,
            List<NetworkInterface.IPGatewayAddress> ipv6Gateway,
            List<string> ipv6DnsServer,
            bool routerDiscovery,
            int ipv6Mtu,
            int interfaceMetric)
        {
            InitializeComponent();
            textBox1.Text = name;
            if (ipv4Enabled)
            {
                TreeNode ipv4Node = treeView1.Nodes.Add("IPv4");
                ipv4Node.Checked = true;
                if (!dhcpIP && ipv4Address.Count > 0)
                {
                    TreeNode node = ipv4Node.Nodes.Add("Local Address & Subnet Mask");
                    foreach (NetworkInterface.IPHostAddress ip in ipv4Address)
                    {
                        TreeNode item = node.Nodes.Add("ipv4LocalAddress", ip.Address + " - " + ip.Subnet);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (!dhcpIP && ipv4Gateway.Count > 0)
                {
                    TreeNode node = ipv4Node.Nodes.Add("Gateway Address & Metric");
                    foreach (NetworkInterface.IPGatewayAddress ip in ipv4Gateway)
                    {
                        TreeNode item = node.Nodes.Add("ipv4GatewayAddress", ip.Address + " - " + ip.GatewayMetric);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (!dhcpDns && ipv4DnsServer.Count > 0)
                {
                    TreeNode node = ipv4Node.Nodes.Add("DNS Server");
                    foreach (string ip in ipv4DnsServer)
                    {
                        TreeNode item = node.Nodes.Add("ipv4DnsAddress", ip);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                {
                    TreeNode item = ipv4Node.Nodes.Add("dhcp", "DHCP: " + ((NetworkInterface.Dhcp)(Convert.ToInt32(dhcpIP) + Convert.ToInt32(dhcpDns))).GetDescription());
                    item.Checked = true;
                    item.Tag = (NetworkInterface.Dhcp)(Convert.ToInt32(dhcpIP) + Convert.ToInt32(dhcpDns));
                }
                {
                    TreeNode item = ipv4Node.Nodes.Add("netbios", "NetBIOS: " + ((NetworkInterface.Netbios)(netbios ? 1 : 2)).GetDescription());
                    item.Checked = true;
                    item.Tag = (NetworkInterface.Netbios)(netbios ? 1 : 2);
                }
                {
                    TreeNode item = ipv4Node.Nodes.Add("ipv4Mtu", "MTU: " + ipv4Mtu);
                    item.Checked = true;
                    item.Tag = ipv4Mtu;
                }
            }
            if (ipv6Enabled)
            {
                TreeNode ipv6Node = treeView1.Nodes.Add("IPv6");
                ipv6Node.Checked = true;
                if (ipv6Address.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("Local Address & Subnet Prefix Length");
                    foreach (NetworkInterface.IPHostAddress ip in ipv6Address)
                    {
                        TreeNode item = node.Nodes.Add("ipv6LocalAddress", ip.Address + " - " + ip.Subnet);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (ipv6Gateway.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("Gateway Address & Metric");
                    foreach (NetworkInterface.IPGatewayAddress ip in ipv6Gateway)
                    {
                        TreeNode item = node.Nodes.Add("ipv6GatewayAddress", ip.Address + " - " + ip.GatewayMetric);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (ipv6DnsServer.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("DNS Server");
                    foreach (string ip in ipv6DnsServer)
                    {
                        TreeNode item = node.Nodes.Add("ipv6DnsAddress", ip);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                {
                    TreeNode item = ipv6Node.Nodes.Add("ipv6RouterDiscovery", "Router Discovery: " + ((NetworkInterface.RouterDiscovery)Convert.ToInt32(routerDiscovery)).GetDescription());
                    item.Checked = true;
                    item.Tag = (NetworkInterface.RouterDiscovery)Convert.ToInt32(routerDiscovery);
                }
                {
                    TreeNode item = ipv6Node.Nodes.Add("ipv6Mtu", "MTU: " + ipv6Mtu);
                    item.Checked = true;
                    item.Tag = ipv6Mtu;
                }
            }
            {
                TreeNode item = treeView1.Nodes.Add("interfaceMetric", "Interface Metric: " + interfaceMetric);
                item.Checked = true;
                item.Tag = interfaceMetric;
            }
            treeView1.ExpandAll();
            loadMode.SelectedIndex = 0;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                new BalloonTip("Warning", "Profile name can't be empty", textBox1, BalloonTip.ICON.WARNING);
                return;
            }
            DialogResult result;
            if (Global.Config.InterfaceProfiles.Where((i) => i.Name == textBox1.Text).Count() > 0)
            {
                result = MessageBox.Show(this, "There is already a profile with this name \"" + textBox1.Text + "\".\nDo you want to overwrite it?", "Profile duplicate", MessageBoxButtons.YesNo);
                if (result.CompareTo(DialogResult.No) == 0)
                    return;
            }
            ProfileName = textBox1.Text;
            LoadMode = (Config.InterfaceProfile.LoadingMode)loadMode.SelectedIndex;
            TreeNode[] nodes = treeView1.Nodes.Find("ipv4LocalAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv4Address.Add((NetworkInterface.IPHostAddress)node.Tag);
            nodes = treeView1.Nodes.Find("ipv4GatewayAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv4Gateway.Add((NetworkInterface.IPGatewayAddress)node.Tag);
            nodes = treeView1.Nodes.Find("ipv4DnsAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv4DnsServer.Add((string)node.Tag);
            nodes = treeView1.Nodes.Find("dhcp", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    DhcpEnabled = (NetworkInterface.Dhcp)node.Tag;
            nodes = treeView1.Nodes.Find("netbios", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    NetbiosEnabled = (NetworkInterface.Netbios)node.Tag;
            nodes = treeView1.Nodes.Find("ipv4Mtu", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv4Mtu = (int)node.Tag;
            nodes = treeView1.Nodes.Find("ipv6LocalAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv6Address.Add((NetworkInterface.IPHostAddress)node.Tag);
            nodes = treeView1.Nodes.Find("ipv6GatewayAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv6Gateway.Add((NetworkInterface.IPGatewayAddress)node.Tag);
            nodes = treeView1.Nodes.Find("ipv6DnsAddress", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv6DnsServer.Add((string)node.Tag);
            nodes = treeView1.Nodes.Find("ipv6RouterDiscovery", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    RouterDiscovery = (NetworkInterface.RouterDiscovery)node.Tag;
            nodes = treeView1.Nodes.Find("ipv6Mtu", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    IPv6Mtu = (int)node.Tag;
            nodes = treeView1.Nodes.Find("interfaceMetric", true);
            foreach (TreeNode node in nodes)
                if (node.Checked)
                    InterfaceMetric = (int)node.Tag;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            updateChildNodes(e.Node, e.Node.Checked);
            if (e.Node.Parent != null)
            {
                foreach (TreeNode node in e.Node.Parent.Nodes)
                {
                    if (node.Checked)
                    {
                        if (node.Parent.Checked != true)
                            node.Parent.Checked = true;
                        return;
                    }
                }
                if (e.Node.Parent.Checked != false)
                    e.Node.Parent.Checked = false;
            }
        }

        private void updateChildNodes(TreeNode node, bool state)
        {
            if (state)
                foreach (TreeNode item in node.Nodes)
                    if (item.Checked)
                        return;
            foreach (TreeNode item in node.Nodes)
            {
                if (item.Checked != state)
                    item.Checked = state;
                if (item.Nodes.Count > 0)
                    updateChildNodes(item, state);
            }
        }
    }
}
