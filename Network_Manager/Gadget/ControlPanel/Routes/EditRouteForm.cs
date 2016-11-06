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
using Lib.Network;
using Lib.Forms;
using Lib.WinAPI;

namespace Network_Manager.Gadget.ControlPanel.Routes
{
    public partial class EditRouteForm : Form
    {
        private string lastIPv4Gateway = "0.0.0.0";
        private string lastIPv6Gateway = "::";
        private int lastGatewayMode = 1;
        private int ipVersion;
        private string destination;
        private string prefix;
        private string gateway;
        private string interfaceIndex;
        private TreeView treeView;
        private Config.SavedRouteItem route;

        public EditRouteForm(int ipVersion, string destination, string prefix, string gateway, string interfaceIndex, string metric)
        {
            InitializeComponent();
            this.ipVersion = ipVersion;
            this.destination = destination;
            this.prefix = prefix;
            this.gateway = gateway;
            this.interfaceIndex = interfaceIndex;
            // Check if route is saved
            route = new Config.SavedRouteItem();
            route.IPVersion = ipVersion;
            route.Destination = destination;
            route.Prefix = prefix;
            route.Gateway = gateway;
            route.InterfaceGuid = Global.NetworkInterfaces.First(i => i.Value.Index == int.Parse(interfaceIndex)).Value.Guid;
            treeView = Global.Config.SavedRoutes.Find(route);
            if (treeView.SelectedNode != treeView.Nodes[0])
            {
                updateSavedRouteCheckBox.Checked = true;
                updateSavedRouteCheckBox.Enabled = true;
            }
            // Populate IPv4 or IPv6
            if (ipVersion == 4)
            {
                Text = "Edit IPv4 Route";
                groupBox1.Text = "IPv4 Route";
                routeDestination.Text = "0.0.0.0";
                routePrefix.Text = "255.255.255.255";
                toolTip1.SetToolTip(routePrefix, "Subnet IP mask or prefix length");
                foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                    if (nic.IPv4Enabled)
                        routeInterface.Items.Add(nic.Index.ToString() + " (" + nic.Name + " - " + (nic.IPv4Address.Count > 0 ? nic.IPv4Address.FirstOrDefault().Address : "0.0.0.0") + ")");
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                    NetworkInterface.Loopback.IPv4Enabled)
                    routeInterface.Items.Add("1 (" + NetworkInterface.Loopback.Name + " - 127.0.0.1)");
            }
            else
            {
                Text = "Edit IPv6 Route";
                groupBox1.Text = "IPv6 Route";
                routeDestination.Text = "::";
                routePrefix.Text = "128";
                toolTip1.SetToolTip(routePrefix, "Subnet prefix length, a value between 0 and 128 (inclusive)");
                foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                    if (nic.IPv6Enabled)
                        routeInterface.Items.Add(nic.Index.ToString() + " (" + nic.Name + " - " + (nic.IPv6Address.All.Count > 0 ? nic.IPv6Address.All.FirstOrDefault().Address : "::") + ")");
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                    NetworkInterface.Loopback.IPv6Enabled)
                    routeInterface.Items.Add("1 (" + NetworkInterface.Loopback.Name + " - ::1)");
            }
            routeGatewayMode.SelectedIndex = 1;
            new TextBoxMask(routeMetric, TextBoxMask.Mask.Numeric);
            // load route
            routeDestination.Text = destination;
            routePrefix.Text = prefix;
            routeGatewayMode.SelectedIndex = 0;
            routeGateway.Text = gateway;
            for (int i = 0; i < routeInterface.Items.Count; i++)
                if (Regex.IsMatch(routeInterface.Items[i].ToString(), @"^" + interfaceIndex + @" .*$"))
                    routeInterface.SelectedIndex = i;
            routeMetric.Text = metric;
        }

        private void routeGatewayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // manual
            if (routeGatewayMode.SelectedIndex == 0)
            {
                routeGateway.Items.Clear();
                routeGateway.DropDownStyle = ComboBoxStyle.Simple;
                routeGateway.Enabled = true;
                if (lastGatewayMode != 0)
                    if (ipVersion == 4)
                        routeGateway.Text = lastIPv4Gateway;
                    else
                        routeGateway.Text = lastIPv6Gateway;
                lastGatewayMode = 0;
            }
            // interface default gateway
            if (routeGatewayMode.SelectedIndex == 1)
            {
                if (lastGatewayMode == 0)
                    if (ipVersion == 4)
                        lastIPv4Gateway = routeGateway.Text;
                    else
                        lastIPv6Gateway = routeGateway.Text;
                lastGatewayMode = 1;
                routeGateway.Items.Clear();
                routeGateway.DropDownStyle = ComboBoxStyle.DropDownList;
                if (routeInterface.SelectedIndex == -1)
                {
                    return;
                }
                int ifIndex = int.Parse(Regex.Replace(routeInterface.Text, @"^(\d+) .*$", "$1"));
                if (ipVersion == 4)
                {
                    if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Gateway.Count > 0)
                            foreach (NetworkInterface.IPGatewayAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Gateway)
                                routeGateway.Items.Add(ip.Address);
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                        routeGateway.Items.Add("0.0.0.0");
                }
                else
                {
                    if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Gateway.Count > 0)
                            foreach (NetworkInterface.IPGatewayAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Gateway)
                                routeGateway.Items.Add(ip.Address);
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                        routeGateway.Items.Add("::");
                }
                if (routeGateway.Items.Count > 0)
                    routeGateway.SelectedIndex = 0;
                
            }
            // no gateway
            if (routeGatewayMode.SelectedIndex == 2)
            {
                if (lastGatewayMode == 0)
                    if (ipVersion == 4)
                        lastIPv4Gateway = routeGateway.Text;
                    else
                        lastIPv6Gateway = routeGateway.Text;
                lastGatewayMode = 2;
                routeGateway.Items.Clear();
                routeGateway.DropDownStyle = ComboBoxStyle.DropDownList;
                if (routeInterface.SelectedIndex == -1)
                    return;
                int ifIndex = int.Parse(Regex.Replace(routeInterface.Text, @"^(\d+) .*$", "$1"));
                if (ipVersion == 4)
                {
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    {
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                            if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Address.Count > 0)
                                foreach (NetworkInterface.IPHostAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Address)
                                    routeGateway.Items.Add(ip.Address);
                    }
                    else
                        routeGateway.Items.Add("0.0.0.0");
                }
                else
                {
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    {
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                            if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Address.All.Count > 0)
                                foreach (NetworkInterface.IPHostAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Address.All)
                                    routeGateway.Items.Add(ip.Address);
                    }
                    else
                        routeGateway.Items.Add("::");
                }
                if (routeGateway.Items.Count > 0)
                    routeGateway.SelectedIndex = 0;
            }
        }

        private void routeInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            routeGatewayMode_SelectedIndexChanged(sender, e);
        }

        private bool ValidateRoute()
        {
            IPAddress ipAddress = new IPAddress(0);
            string destination = routeDestination.Text;
            string prefix = routePrefix.Text;
            string gateway = routeGateway.Text;
            if (ipVersion == 4)
            {
                if (prefix == "")
                    prefix = "255.255.255.255";
                if (gateway == "")
                    gateway = "0.0.0.0";
                if (destination == "" ||
                    !IPAddress.TryParse(destination, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0 && IP.CheckIfSameNetwork(destination, "240.0.0.0", "240.0.0.0"))
                {
                    new BalloonTip("Warning", "Invalid IPv4 address", routeDestination, BalloonTip.ICON.WARNING);
                    return false;
                }
                if (!IP.ValidateIPv4Mask(ref prefix))
                {
                    new BalloonTip("Warning", "Invalid IPv4 subnet mask", routePrefix, BalloonTip.ICON.WARNING);
                    return false;
                }
                routeDestination.Text = IP.GetNetwork(destination, prefix);
                routePrefix.Text = prefix;
                if (!IPAddress.TryParse(gateway, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 && IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.0.0.0") && !IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.255.255.255") ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0 && IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.255.255.255") ||
                    IP.CheckIfSameNetwork(gateway, "224.0.0.0", "224.0.0.0") ||
                    IP.CheckIfSameNetwork(gateway, "240.0.0.0", "240.0.0.0"))
                {
                    new BalloonTip("Warning", "Invalid IPv4 gateway address", routeGateway, BalloonTip.ICON.WARNING);
                    return false;
                }
                routeGateway.Text = gateway;
            }
            else
            {
                if (prefix == "")
                    prefix = "128";
                if (gateway == "")
                    gateway = "::";
                if (destination == "" || !IPAddress.TryParse(destination, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    new BalloonTip("Warning", "Invalid IPv6 address", routeDestination, BalloonTip.ICON.WARNING);
                    return false;
                }
                if (!IP.ValidateIPv6Prefix(prefix))
                {
                    new BalloonTip("Warning", "Invalid IPv6 subnet prefix length. Value must between 0 and 128 (inclusive)", routePrefix, BalloonTip.ICON.WARNING);
                    return false;
                }
                routeDestination.Text = IP.GetNetwork(destination, prefix);
                routePrefix.Text = prefix;
                if (!IP.ValidateIPv6(ref gateway))
                // && !Regex.IsMatch(gateway, @"^(::1|::)$") && IPAddress.TryParse(gateway, out ipAddress) && ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    new BalloonTip("Warning", "Invalid IPv6 address", routeGateway, BalloonTip.ICON.WARNING);
                    return false;
                }
                routeGateway.Text = gateway;
            }
            if (routeInterface.SelectedIndex == -1)
            {
                new BalloonTip("Information", "Select the interface through which to route", routeInterface, BalloonTip.ICON.INFO);
                return false;
            }
            if (routeMetric.Text == "")
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                    routeMetric.Text = "0";
                else
                    routeMetric.Text = "1";
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                if (ushort.Parse(routeMetric.Text) == 0)
                {
                    new BalloonTip("Warning", "Route metric must be a value between 1 and 9999 (inclusive)", routeMetric, BalloonTip.ICON.WARNING);
                    return false;
                }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateRoute())
                return;
            Iphlpapi.DeleteRoute(destination, prefix, gateway, interfaceIndex);
            Iphlpapi.MIB_IPFORWARD_TYPE type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_INDIRECT;
            int ifIndex = int.Parse(Regex.Replace(routeInterface.Text, @"^(\d+) .*$", "$1"));
            // if on-link set type to direct for XP
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                NetworkInterface nic;
                if (ifIndex == 1)
                    nic = NetworkInterface.Loopback;
                else
                    nic = Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First();
                if (ipVersion == 4)
                {
                    if (nic.IPv4Address.Where((i) => i.Address == routeGateway.Text).Count() > 0)
                        type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                    //if (nic.Index == 1 && IP.CheckIfSameNetwork("127.0.0.1", routeGateway.Text, "255.0.0.0"))
                    //    type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                }
                else
                {
                    if (nic.IPv6Address.All.Where((i) => i.Address == routeGateway.Text).Count() > 0)
                        type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                    //if (nic.Index == 1 && IP.CheckIfSameNetwork("::1", routeGateway.Text, "128"))
                    //    type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                }
                if (routeGateway.Text == "0.0.0.0" || routeGateway.Text == "::")
                    type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
            }
            Iphlpapi.AddRoute(routeDestination.Text, routePrefix.Text, routeGateway.Text, ifIndex.ToString(), routeMetric.Text, type);
            if (updateSavedRouteCheckBox.Checked)
            {
                route.Destination = routeDestination.Text;
                route.Prefix = routePrefix.Text;
                route.Gateway = routeGateway.Text;
                if (ifIndex == 1)
                    route.InterfaceGuid = NetworkInterface.Loopback.Guid;
                else
                    route.InterfaceGuid = Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).FirstOrDefault().Guid;
                route.Metric = ushort.Parse(routeMetric.Text);
                Global.Config.SavedRoutes.AddNode(treeView, route, true);
            }
                
            Close();
        }
    }
}
