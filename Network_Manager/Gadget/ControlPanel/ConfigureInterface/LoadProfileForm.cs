using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Lib.Network;
using Lib.Extensions;

namespace Network_Manager.Gadget.ControlPanel.ConfigureInterface
{
    public partial class LoadProfileForm : Form
    {
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

        public LoadProfileForm(string profileName)
        {
            InitializeComponent();
            foreach (Config.InterfaceProfile profile in Global.Config.InterfaceProfiles)
                comboBox1.Items.Add(profile.Name);
            comboBox1.SelectedItem = profileName;
            treeView1.CheckBoxes = true;
        }

        private void Load_Click(object sender, EventArgs e)
        {
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
            //foreach (ListViewItem item in listView1.Items)
            //{
            //    if (item.SubItems[0].Text != "")
            //        property = item.SubItems[0].Text;
            //    if (!item.Checked)
            //        continue;
            //    if (property == "IPv4 Address & Mask")
            //        IPv4Address.Add(new string[] { item.SubItems[1].Text, item.SubItems[2].Text });
            //    if (property == "IPv4 Gateway & Metric")
            //        IPv4Gateway.Add(new string[] { item.SubItems[1].Text, item.SubItems[2].Text });
            //    if (property == "IPv4 DNS Server")
            //        IPv4DnsServer.Add( item.SubItems[1].Text );
            //    if (property == "DHCP Enabled")
            //        if (item.SubItems[1].Text == "Disabled")
            //            DhcpEnabled = NetworkInterface.Dhcp.Disabled;
            //        else if (item.SubItems[1].Text == "IP only")
            //            DhcpEnabled = NetworkInterface.Dhcp.IPOnly;
            //        else if (item.SubItems[1].Text == "IP & DNS")
            //            DhcpEnabled = NetworkInterface.Dhcp.IPnDns;
            //    if (property == "NetBIOS over TCP/IP")
            //        NetbiosEnabled = (NetworkInterface.Netbios)Enum.Parse(typeof(NetworkInterface.Netbios), item.SubItems[1].Text);
            //    if (property == "IPv4 MTU")
            //        IPv4Mtu = int.Parse(item.SubItems[1].Text);
            //    if (property == "IPv6 Address & Mask")
            //        IPv6Address.Add(new string[] { item.SubItems[1].Text, item.SubItems[2].Text });
            //    if (property == "IPv6 Gateway & Metric")
            //        IPv6Gateway.Add(new string[] { item.SubItems[1].Text, item.SubItems[2].Text });
            //    if (property == "IPv6 DNS Server")
            //        IPv6DnsServer.Add(item.SubItems[1].Text);
            //    if (property == "Router Discovery")
            //        RouterDiscovery = (NetworkInterface.RouterDiscovery)Enum.Parse(typeof(NetworkInterface.RouterDiscovery), item.SubItems[1].Text);
            //    if (property == "IPv6 MTU")
            //        IPv6Mtu = int.Parse(item.SubItems[1].Text);
            //    if (property == "Interface Metric")
            //        InterfaceMetric = int.Parse(item.SubItems[1].Text);
            //}
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            Config.InterfaceProfile profile = Global.Config.InterfaceProfiles.Where((i) => i.Name == comboBox1.Text).FirstOrDefault();
            if (profile.IPv4LocalAddresses.Count > 0 ||
                profile.IPv4GatewayAddresses.Count > 0 ||
                profile.IPv4DnsAddresses.Count > 0 ||
                profile.IPv4Mtu > -1 ||
                profile.DhcpEnabled != NetworkInterface.Dhcp.Unchanged ||
                profile.NetbiosEnabled != NetworkInterface.Netbios.Unchanged)
            {
                TreeNode ipv4Node = treeView1.Nodes.Add("IPv4");
                ipv4Node.Checked = true;
                if (profile.DhcpEnabled < Lib.Network.NetworkInterface.Dhcp.IPOnly)
                    if (profile.IPv4LocalAddresses.Count > 0)
                    {
                        TreeNode node = ipv4Node.Nodes.Add("Local Address & Subnet Mask");
                        node.Checked = true;
                        foreach (NetworkInterface.IPHostAddress ip in profile.IPv4LocalAddresses)
                        {
                            TreeNode item = node.Nodes.Add("ipv4LocalAddress", ip.Address + " - " + ip.Subnet);
                            item.Checked = true;
                            item.Tag = ip;
                        }
                            
                    }
                if (profile.DhcpEnabled < Lib.Network.NetworkInterface.Dhcp.IPOnly)
                    if (profile.IPv4GatewayAddresses.Count > 0)
                    {
                        TreeNode node = ipv4Node.Nodes.Add("Gateway Address & Metric");
                        node.Checked = true;
                        foreach (NetworkInterface.IPGatewayAddress ip in profile.IPv4GatewayAddresses)
                        {
                            TreeNode item = node.Nodes.Add("ipv4GatewayAddress", ip.Address + " - " + ip.GatewayMetric);
                            item.Checked = true;
                            item.Tag = ip;
                        }
                            
                    }
                if (profile.DhcpEnabled != Lib.Network.NetworkInterface.Dhcp.IPnDns)
                    if (profile.IPv4DnsAddresses.Count > 0)
                    {
                        TreeNode node = ipv4Node.Nodes.Add("DNS Server");
                        node.Checked = true;
                        foreach (string ip in profile.IPv4DnsAddresses)
                        {
                            TreeNode item = node.Nodes.Add("ipv4DnsAddress", ip);
                            item.Checked = true;
                            item.Tag = ip;
                        }
                    }
                if (profile.DhcpEnabled != NetworkInterface.Dhcp.Unchanged)
                {
                    TreeNode item = ipv4Node.Nodes.Add("dhcp", "DHCP: " + profile.DhcpEnabled.GetDescription());
                    item.Checked = true;
                    item.Tag = profile.DhcpEnabled;
                }
                if (profile.NetbiosEnabled != NetworkInterface.Netbios.Unchanged)
                {
                    TreeNode item = ipv4Node.Nodes.Add("netbios", "NetBIOS: " + profile.NetbiosEnabled.GetDescription());
                    item.Checked = true;
                    item.Tag = profile.NetbiosEnabled;
                }
                if (profile.IPv4Mtu > -1)
                {
                    TreeNode item = ipv4Node.Nodes.Add("ipv4Mtu", "MTU: " + profile.IPv4Mtu);
                    item.Checked = true;
                    item.Tag = profile.IPv4Mtu;
                }
            }
            if (profile.IPv6LocalAddresses.Count > 0 ||
                profile.IPv6GatewayAddresses.Count > 0 ||
                profile.IPv6DnsAddresses.Count > 0 ||
                profile.IPv6Mtu > -1 ||
                profile.IPv6RouterDiscoveryEnabled != NetworkInterface.RouterDiscovery.Unchanged)
            {
                TreeNode ipv6Node = treeView1.Nodes.Add("IPv6");
                ipv6Node.Checked = true;
                if (profile.IPv6LocalAddresses.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("Local Address & Subnet Prefix Length");
                    node.Checked = true;
                    foreach (NetworkInterface.IPHostAddress ip in profile.IPv6LocalAddresses)
                    {
                        TreeNode item = node.Nodes.Add("ipv6LocalAddress", ip.Address + " - " + ip.Subnet);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (profile.IPv6GatewayAddresses.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("Gateway Address & Metric");
                    node.Checked = true;
                    foreach (NetworkInterface.IPGatewayAddress ip in profile.IPv6GatewayAddresses)
                    {
                        TreeNode item = node.Nodes.Add("ipv6GatewayAddress", ip.Address + " - " + ip.GatewayMetric);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (profile.IPv6DnsAddresses.Count > 0)
                {
                    TreeNode node = ipv6Node.Nodes.Add("DNS Server");
                    node.Checked = true;
                    foreach (string ip in profile.IPv6DnsAddresses)
                    {
                        TreeNode item = node.Nodes.Add("ipv6DnsAddress", ip);
                        item.Checked = true;
                        item.Tag = ip;
                    }
                }
                if (profile.IPv6RouterDiscoveryEnabled != NetworkInterface.RouterDiscovery.Unchanged)
                {
                    TreeNode item = ipv6Node.Nodes.Add("ipv6RouterDiscovery", "Router Discovery: " + profile.IPv6RouterDiscoveryEnabled.GetDescription());
                    item.Checked = true;
                    item.Tag = profile.IPv6RouterDiscoveryEnabled;
                }   
                if (profile.IPv6Mtu > -1)
                {
                    TreeNode item = ipv6Node.Nodes.Add("ipv6Mtu", "MTU: " + profile.IPv6Mtu);
                    item.Checked = true;
                    item.Tag = profile.IPv6Mtu;
                }   
            }
            if (profile.InterfaceMetric > -1)
            {
                TreeNode item = treeView1.Nodes.Add("interfaceMetric", "Interface Metric: " + profile.InterfaceMetric);
                item.Checked = true;
                item.Tag = profile.InterfaceMetric;
            }
            treeView1.ExpandAll();
            //if (profile.DhcpEnabled < Lib.Network.NetworkInterface.Dhcp.IPOnly)
            //    for (int i = 0; i < profile.IPv4Address.Count; i++)
            //        listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv4 Address & Mask" : "", profile.IPv4Address[i].Address, profile.IPv4Address[i].Subnet }));
            //if (profile.DhcpEnabled < Lib.Network.NetworkInterface.Dhcp.IPOnly)
            //    for (int i = 0; i < profile.IPv4Gateway.Count; i++)
            //        listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv4 Gateway & Metric" : "", profile.IPv4Gateway[i].Address, profile.IPv4Gateway[i].GatewayMetric.ToString() }));
            //if (profile.DhcpEnabled != Lib.Network.NetworkInterface.Dhcp.IPnDns)
            //    for (int i = 0; i < profile.IPv4DnsServer.Count; i++)
            //        listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv4 DNS Server" : "", profile.IPv4DnsServer[i] }));
            //if (profile.DhcpEnabled == NetworkInterface.Dhcp.Disabled)
            //    listView1.Items.Add(new ListViewItem(new string[] { "DHCP Enabled", "Disabled" }));
            //else if (profile.DhcpEnabled == NetworkInterface.Dhcp.IPOnly)
            //    listView1.Items.Add(new ListViewItem(new string[] { "DHCP Enabled", "IP only" }));
            //else if (profile.DhcpEnabled == NetworkInterface.Dhcp.IPnDns)
            //    listView1.Items.Add(new ListViewItem(new string[] { "DHCP Enabled", "IP & DNS" }));
            //if (profile.NetbiosEnabled != NetworkInterface.Netbios.Unchanged)
            //    listView1.Items.Add(new ListViewItem(new string[] { "NetBIOS over TCP/IP", profile.NetbiosEnabled.ToString() }));
            //if (profile.IPv4Mtu > -1)
            //    listView1.Items.Add(new ListViewItem(new string[] { "IPv4 MTU", profile.IPv4Mtu.ToString() }));
            //for (int i = 0; i < profile.IPv6Address.Count; i++)
            //    listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv6 Address & Mask" : "", profile.IPv6Address[i].Address, profile.IPv6Address[i].Subnet }));
            //for (int i = 0; i < profile.IPv6Gateway.Count; i++)
            //    listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv6 Gateway & Metric" : "", profile.IPv6Gateway[i].Address, profile.IPv6Gateway[i].GatewayMetric.ToString() }));
            //for (int i = 0; i < profile.IPv6DnsServer.Count; i++)
            //    listView1.Items.Add(new ListViewItem(new string[] { i == 0 ? "IPv6 DNS Server" : "", profile.IPv6DnsServer[i] }));
            //if (profile.IPv6RouterDiscoveryEnabled != NetworkInterface.RouterDiscovery.Unchanged)
            //    listView1.Items.Add(new ListViewItem(new string[] { "Router Discovery", profile.NetbiosEnabled.ToString() }));
            //if (profile.IPv4Mtu > -1)
            //    listView1.Items.Add(new ListViewItem(new string[] { "IPv6 MTU", profile.IPv6Mtu.ToString() }));
            //if (profile.InterfaceMetric > -1)
            //    listView1.Items.Add(new ListViewItem(new string[] { "Interface Metric", profile.InterfaceMetric.ToString() }));
            //foreach (ListViewItem item in listView1.Items)
            //    item.Checked = true;
            ////listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //foreach (ColumnHeader column in listView1.Columns)
            //    column.Width = -2;
            loadMode.SelectedIndex = (int)profile.LoadMode;
        }

        //private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        //{
        //    TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
        //    e.DrawBackground();
        //    if (e.ColumnIndex == 0)
        //    {
        //        Point location = new Point(e.Bounds.Left + 4, e.Bounds.Top + e.Bounds.Height / 2 - 6);
        //        CheckBoxRenderer.DrawCheckBox(e.Graphics, location, topCheckBoxState);
        //    }
        //    e.DrawText(flags);
        //}

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
