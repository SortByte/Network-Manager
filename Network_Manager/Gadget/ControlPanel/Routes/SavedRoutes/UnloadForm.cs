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
using Lib.WinAPI;

namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    public partial class UnloadForm : Form
    {
        private string lastIPv4Gateway = "0.0.0.0";
        private string lastIPv6Gateway = "::";
        private int lastIPv4GatewayMode = 3;
        private int lastIPv6GatewayMode = 3;
        private TreeView treeView;
        private int unloadIPv4 = 0;
        private int unloadIPv6 = 0;
        private int NotActiveIPv4Routes = 0;
        private int NotActiveIPv6Routes = 0;
        private List<Config.SavedRouteItem> unloadRoutes;
        private List<Iphlpapi.Route> activeRoutes;

        public UnloadForm(TreeView treeView)
        {
            InitializeComponent();
            this.treeView = treeView;
            // resize
            Size minimumSize = ClientSize;
            Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
            int titleBarHeight = screenRectangle.Top - Top;
            int borderThickness = screenRectangle.Left - Left;
            Rectangle workingArea = Screen.GetWorkingArea(this);
            Size clientSize = new Size();
            if (minimumSize.Width > workingArea.Width - 2 * borderThickness)
                clientSize.Width = workingArea.Width - 2 * borderThickness;
            else
                clientSize.Width = minimumSize.Width;
            if (minimumSize.Height > workingArea.Height - titleBarHeight - borderThickness)
                clientSize.Height = workingArea.Height - titleBarHeight - borderThickness;
            else
                clientSize.Height = minimumSize.Height;
            AutoScrollMinSize = new System.Drawing.Size(minimumSize.Width, minimumSize.Height);
            ClientSize = new Size(clientSize.Width, clientSize.Height);
            // load routes
            IPAddress ipAddress = new IPAddress(0);
            Config.SavedRouteNode unloadNode = Global.Config.SavedRoutes.GetNode(treeView);
            unloadRoutes = Global.Config.SavedRoutes.GetRoutes(unloadNode);
            activeRoutes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_UNSPEC);
            foreach (Config.SavedRouteItem item in unloadRoutes)
            {
                string interfaceIndex = "Not connected";
                int ifIndex = 0;
                string status = "Not loaded";
                if (ValidateRoute(ref item.Destination, ref item.Prefix, ref item.Gateway, item.IPVersion))
                {
                    if (Global.NetworkInterfaces.ContainsKey(item.InterfaceGuid) &&
                        (item.IPVersion == 4 && Global.NetworkInterfaces[item.InterfaceGuid].IPv4Enabled ||
                        item.IPVersion == 6 && Global.NetworkInterfaces[item.InterfaceGuid].IPv6Enabled))
                    {
                        interfaceIndex = Global.NetworkInterfaces[item.InterfaceGuid].Index.ToString() +
                        " (" + Global.NetworkInterfaces[item.InterfaceGuid].Name + " - ";
                        ifIndex = Global.NetworkInterfaces[item.InterfaceGuid].Index;
                        if (item.IPVersion == 4)
                        {
                            unloadIPv4++;
                            if (Global.NetworkInterfaces[item.InterfaceGuid].IPv4Address.Count > 0)
                                interfaceIndex += Global.NetworkInterfaces[item.InterfaceGuid].IPv4Address[0].Address;
                            else
                                interfaceIndex += "0.0.0.0";
                        }
                        else
                        {
                            unloadIPv6++;
                            if (Global.NetworkInterfaces[item.InterfaceGuid].IPv6Address.All.Count > 0)
                                interfaceIndex += Global.NetworkInterfaces[item.InterfaceGuid].IPv6Address.All[0].Address;
                            else
                                interfaceIndex += "::";
                        }
                        interfaceIndex += ")";
                    }
                    else if (NetworkInterface.Loopback.Guid == item.InterfaceGuid &&
                        Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                        (item.IPVersion == 4 && NetworkInterface.Loopback.IPv4Enabled ||
                        item.IPVersion == 6 && NetworkInterface.Loopback.IPv6Enabled))
                    {
                        interfaceIndex = "1 (" + NetworkInterface.Loopback.Name + " - ";
                        ifIndex = 1;
                        if (item.IPVersion == 4)
                        {
                            unloadIPv4++;
                            interfaceIndex += "127.0.0.1)";
                        }
                        else
                        {
                            unloadIPv6++;
                            interfaceIndex += "::1)";
                        }
                    }
                    else
                    {
                        if (item.IPVersion == 4)
                        {
                            unloadIPv4++;
                        }
                        else
                        {
                            unloadIPv6++;
                        }
                    }
                    if (activeRoutes.Find(i =>
                        i.IPVersion == item.IPVersion &&
                        IPAddress.Parse(i.Destination).Equals(IPAddress.Parse(item.Destination)) &&
                        (i.IPVersion == 4 ? IPAddress.Parse(i.Prefix).Equals(IPAddress.Parse(item.Prefix)) : int.Parse(i.Prefix) == int.Parse(item.Prefix)) &&
                        IPAddress.Parse(i.Gateway).Equals(IPAddress.Parse(item.Gateway)) &&
                        i.InterfaceIndex == ifIndex) != null)
                    {
                        status = "Active";
                    }
                    else if (activeRoutes.Find(i =>
                        i.IPVersion == item.IPVersion &&
                        IPAddress.Parse(i.Destination).Equals(IPAddress.Parse(item.Destination)) &&
                        (i.IPVersion == 4 ? IPAddress.Parse(i.Prefix).Equals(IPAddress.Parse(item.Prefix)) : int.Parse(i.Prefix) == int.Parse(item.Prefix)) &&
                        IPAddress.Parse(i.Gateway).Equals(IPAddress.Parse(item.Gateway))) != null)
                    {
                        status = "Active on a different interface";
                        if (item.IPVersion == 4)
                            NotActiveIPv4Routes++;
                        else
                            NotActiveIPv6Routes++;
                    }
                    else if (activeRoutes.Find(i =>
                        i.IPVersion == item.IPVersion &&
                        IPAddress.Parse(i.Destination).Equals(IPAddress.Parse(item.Destination)) &&
                        (i.IPVersion == 4 ? IPAddress.Parse(i.Prefix).Equals(IPAddress.Parse(item.Prefix)) : int.Parse(i.Prefix) == int.Parse(item.Prefix))
                        ) != null)
                    {
                        status = "Active with a different gateway";
                        if (item.IPVersion == 4)
                            NotActiveIPv4Routes++;
                        else
                            NotActiveIPv6Routes++;
                    }
                    else
                    {
                        if (item.IPVersion == 4)
                            NotActiveIPv4Routes++;
                        else
                            NotActiveIPv6Routes++;
                    }
                }
                else
                {
                    if (item.IPVersion == 4)
                    {
                         unloadIPv4++;
                         NotActiveIPv4Routes++;
                    }
                    else
                    {
                        unloadIPv6++;
                        NotActiveIPv6Routes++;
                    }
                        
                }
                listView1.Items.Add(new ListViewItem(new string[] {
                    item.Destination,
                    item.Prefix,
                    item.Gateway,
                    interfaceIndex,
                    item.Metric.ToString(),
                    item.Name,
                    status
                })).Checked = true;
            }
            foreach (ColumnHeader column in listView1.Columns)
                column.Width = -2;
            // load configs
            defaultInterfaceMode.SelectedIndex = 0;
            defaultIPv4GatewayMode.SelectedIndex = 3;
            defaultIPv6GatewayMode.SelectedIndex = 3;

            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                if (nic.IPv4Enabled)
                    defaultIPv4Interface.Items.Add(nic.Index.ToString() + " (" + nic.Name + " - " +
                        (nic.IPv4Address.Count > 0 ? nic.IPv4Address.FirstOrDefault().Address : "0.0.0.0"));
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                if (NetworkInterface.Loopback.IPv4Enabled)
                    defaultIPv4Interface.Items.Add("1 (" + NetworkInterface.Loopback.Name + " - 127.0.0.1)");

            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                if (nic.IPv6Enabled)
                    defaultIPv6Interface.Items.Add(nic.Index.ToString() + " (" + nic.Name + " - " +
                        (nic.IPv6Address.All.Count > 0 ? nic.IPv6Address.All.FirstOrDefault().Address : "0.0.0.0"));
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                if (NetworkInterface.Loopback.IPv6Enabled)
                    defaultIPv6Interface.Items.Add("1 (" + NetworkInterface.Loopback.Name + " - ::1)");
        }

        private void defaultInterfaceMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (defaultInterfaceMode.SelectedIndex == 0)
            {
                if (unloadIPv4 > 0 && NotActiveIPv4Routes > 0)
                {
                    tabControl1.TabPages[0].Enabled = true;
                    button1.Enabled = true;
                    if (unloadIPv6 == 0 || NotActiveIPv6Routes == 0)
                        tabControl1.SelectTab(0);
                }
                else
                {
                    tabControl1.TabPages[0].Enabled = false;
                }
                if (unloadIPv6 > 0 && NotActiveIPv6Routes > 0)
                {
                    tabControl1.TabPages[1].Enabled = true;
                    button1.Enabled = true;
                    if (unloadIPv4 == 0 || NotActiveIPv4Routes == 0)
                        tabControl1.SelectTab(1);
                }
                else
                    tabControl1.TabPages[1].Enabled = false;
            }
            else
            {

                if (unloadIPv4 > 0)
                    tabControl1.TabPages[0].Enabled = true;
                else
                    tabControl1.TabPages[0].Enabled = false;
                if (unloadIPv6 > 0)
                    tabControl1.TabPages[1].Enabled = true;
                else
                    tabControl1.TabPages[1].Enabled = false;
            }
            if (unloadIPv4 == 0 && unloadIPv6 == 0)
            {
                tabControl1.TabPages[0].Enabled = false;
                tabControl1.TabPages[1].Enabled = false;
                button1.Enabled = false;
            }
            else
                button1.Enabled = true;
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

            if (e.Item.SubItems[6].Text != "Active")
                if (e.Item.Checked)
                    if (unloadRoutes[e.Item.Index].IPVersion == 4)
                        NotActiveIPv4Routes++;
                    else
                        NotActiveIPv6Routes++;
                else
                    if (unloadRoutes[e.Item.Index].IPVersion == 4)
                        NotActiveIPv4Routes--;
                    else
                        NotActiveIPv6Routes--;
            if (e.Item.Checked)
                if (unloadRoutes[e.Item.Index].IPVersion == 4)
                    unloadIPv4++;
                else
                    unloadIPv6++;
            else
                if (unloadRoutes[e.Item.Index].IPVersion == 4)
                    unloadIPv4--;
                else
                    unloadIPv6--;
            defaultInterfaceMode_SelectedIndexChanged(sender, null);
        }

        private void defaultIPv4GatewayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // manual
            if (defaultIPv4GatewayMode.SelectedIndex == 0)
            {
                defaultIPv4Gateway.Items.Clear();
                defaultIPv4Gateway.DropDownStyle = ComboBoxStyle.Simple;
                defaultIPv4Gateway.Enabled = true;
                if (lastIPv4GatewayMode != 0)
                    defaultIPv4Gateway.Text = lastIPv4Gateway;
                lastIPv4GatewayMode = 0;
            }
            // interface default gateway
            if (defaultIPv4GatewayMode.SelectedIndex == 1)
            {
                if (lastIPv4GatewayMode == 0)
                    lastIPv4Gateway = defaultIPv4Gateway.Text;
                lastIPv4GatewayMode = 1;
                defaultIPv4Gateway.Items.Clear();
                defaultIPv4Gateway.DropDownStyle = ComboBoxStyle.DropDownList;
                defaultIPv4Gateway.Enabled = true;
                if (defaultIPv4Interface.SelectedIndex == -1)
                {
                    return;
                }
                int ifIndex = int.Parse(Regex.Replace(defaultIPv4Interface.Text, @"^(\d+) .*$", "$1"));
                if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                    if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Gateway.Count > 0)
                        foreach (NetworkInterface.IPGatewayAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Gateway)
                            defaultIPv4Gateway.Items.Add(ip.Address);
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                    defaultIPv4Gateway.Items.Add("0.0.0.0");
                if (defaultIPv4Gateway.Items.Count > 0)
                    defaultIPv4Gateway.SelectedIndex = 0;
            }
            // no gateway
            if (defaultIPv4GatewayMode.SelectedIndex == 2)
            {
                if (lastIPv4GatewayMode == 0)
                    lastIPv4Gateway = defaultIPv4Gateway.Text;
                lastIPv4GatewayMode = 2;
                defaultIPv4Gateway.Items.Clear();
                defaultIPv4Gateway.DropDownStyle = ComboBoxStyle.DropDownList;
                defaultIPv4Gateway.Enabled = true;
                if (defaultIPv4Interface.SelectedIndex == -1)
                    return;
                int ifIndex = int.Parse(Regex.Replace(defaultIPv4Interface.Text, @"^(\d+) .*$", "$1"));
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                {
                    if (defaultIPv4Interface.SelectedIndex != defaultIPv4Interface.Items.Count - 1)
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Address.Count > 0)
                            foreach (NetworkInterface.IPHostAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv4Address)
                                defaultIPv4Gateway.Items.Add(ip.Address);
                }
                else
                    defaultIPv4Gateway.Items.Add("0.0.0.0");
                if (defaultIPv4Gateway.Items.Count > 0)
                    defaultIPv4Gateway.SelectedIndex = 0;
            }
            // loaded route regardless
            if (defaultIPv4GatewayMode.SelectedIndex == 3)
            {
                if (lastIPv4GatewayMode == 0)
                    lastIPv4Gateway = defaultIPv4Gateway.Text;
                defaultIPv4Gateway.SelectedIndex = -1;
                defaultIPv4Gateway.Items.Clear();
                defaultIPv4Gateway.DropDownStyle = ComboBoxStyle.Simple;
                defaultIPv4Gateway.Enabled = false;
                lastIPv4GatewayMode = 3;
            }
        }

        private void defaultIPv6GatewayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // manual
            if (defaultIPv6GatewayMode.SelectedIndex == 0)
            {
                defaultIPv6Gateway.Items.Clear();
                defaultIPv6Gateway.DropDownStyle = ComboBoxStyle.Simple;
                defaultIPv6Gateway.Enabled = true;
                if (lastIPv6GatewayMode != 0)
                    defaultIPv6Gateway.Text = lastIPv6Gateway;
                lastIPv6GatewayMode = 0;
            }
            // interface default gateway
            if (defaultIPv6GatewayMode.SelectedIndex == 1)
            {
                if (lastIPv6GatewayMode == 0)
                    lastIPv6Gateway = defaultIPv6Gateway.Text;
                lastIPv6GatewayMode = 1;
                defaultIPv6Gateway.Items.Clear();
                defaultIPv6Gateway.DropDownStyle = ComboBoxStyle.DropDownList;
                defaultIPv6Gateway.Enabled = true;
                if (defaultIPv6Interface.SelectedIndex == -1)
                {
                    return;
                }
                int ifIndex = int.Parse(Regex.Replace(defaultIPv6Interface.Text, @"^(\d+) .*$", "$1"));
                if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                    if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Gateway.Count > 0)
                        foreach (NetworkInterface.IPGatewayAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Gateway)
                            defaultIPv6Gateway.Items.Add(ip.Address);
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                    defaultIPv6Gateway.Items.Add("::");
                if (defaultIPv6Gateway.Items.Count > 0)
                    defaultIPv6Gateway.SelectedIndex = 0;
            }
            // no gateway
            if (defaultIPv6GatewayMode.SelectedIndex == 2)
            {
                if (lastIPv6GatewayMode == 0)
                    lastIPv6Gateway = defaultIPv6Gateway.Text;
                lastIPv6GatewayMode = 2;
                defaultIPv6Gateway.Items.Clear();
                defaultIPv6Gateway.DropDownStyle = ComboBoxStyle.DropDownList;
                defaultIPv6Gateway.Enabled = true;
                if (defaultIPv6Interface.SelectedIndex == -1)
                    return;
                int ifIndex = int.Parse(Regex.Replace(defaultIPv6Interface.Text, @"^(\d+) .*$", "$1"));
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                {
                    if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).Count() > 0)
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Address.All.Count > 0)
                            foreach (NetworkInterface.IPHostAddress ip in Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First().IPv6Address.All)
                                defaultIPv6Gateway.Items.Add(ip.Address);
                }
                else
                    defaultIPv6Gateway.Items.Add("::");
                if (defaultIPv6Gateway.Items.Count > 0)
                    defaultIPv6Gateway.SelectedIndex = 0;
            }
            // loaded route regardless
            if (defaultIPv6GatewayMode.SelectedIndex == 3)
            {
                if (lastIPv6GatewayMode == 0)
                    lastIPv6Gateway = defaultIPv6Gateway.Text;
                defaultIPv6Gateway.SelectedIndex = -1;
                defaultIPv6Gateway.Items.Clear();
                defaultIPv6Gateway.DropDownStyle = ComboBoxStyle.Simple;
                defaultIPv6Gateway.Enabled = false;
                lastIPv6GatewayMode = 3;
            }
        }

        private void defaultIPv4Interface_SelectedIndexChanged(object sender, EventArgs e)
        {
            defaultIPv4GatewayMode_SelectedIndexChanged(sender, e);
        }

        private void defaultIPv6Interface_SelectedIndexChanged(object sender, EventArgs e)
        {
            defaultIPv6GatewayMode_SelectedIndexChanged(sender, e);
        }

        private bool ValidateRoute(ref string destination, ref string prefix, ref string gateway, int ipVersion)
        {
            IPAddress ipAddress = new IPAddress(0);
            if (ipVersion == 4)
            {
                if (prefix == "")
                    return false;
                if (gateway == "")
                    return false;
                if (destination == "" ||
                    !IPAddress.TryParse(destination, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0 && IP.CheckIfSameNetwork(destination, "240.0.0.0", "240.0.0.0"))
                    return false;
                if (!IP.ValidateIPv4Mask(ref prefix))
                    return false;
                destination = IP.GetNetwork(destination, prefix);
                if (!IPAddress.TryParse(gateway, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 && IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.0.0.0") && !IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.255.255.255") ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0 && IP.CheckIfSameNetwork(gateway, "0.0.0.0", "255.255.255.255") ||
                    IP.CheckIfSameNetwork(gateway, "224.0.0.0", "224.0.0.0") ||
                    IP.CheckIfSameNetwork(gateway, "240.0.0.0", "240.0.0.0"))
                    return false;
            }
            else
            {
                if (prefix == "")
                    return false;
                if (gateway == "")
                    return false;
                if (destination == "" || !IPAddress.TryParse(destination, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    return false;
                if (!IP.ValidateIPv6Prefix(ref prefix))
                    return false;
                destination = IP.GetNetwork(destination, prefix);
                if (!IP.ValidateIPv6(ref gateway))
                    return false;
            }
            return true;
        }

        private bool ValidateConfigs()
        {
            IPAddress ipAddress = new IPAddress(0);
            if (unloadIPv4 > 0 && NotActiveIPv4Routes > 0 && defaultInterfaceMode.SelectedIndex == 0 ||
                unloadIPv4 > 0 && defaultInterfaceMode.SelectedIndex == 1)
            {
                if (defaultIPv4Interface.SelectedIndex == -1)
                {
                    tabControl1.SelectTab(0);
                    new BalloonTip("Information", "Select the route's default IPv4 interface", defaultIPv4Interface, BalloonTip.ICON.INFO);
                    return false;
                }
                if (defaultIPv4GatewayMode.SelectedIndex != 3)
                {
                    string ipv4Gateway = defaultIPv4Gateway.Text;
                    if (ipv4Gateway == "")
                        ipv4Gateway = "0.0.0.0";
                    if (!IPAddress.TryParse(ipv4Gateway, out ipAddress) ||
                    ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 && IP.CheckIfSameNetwork(ipv4Gateway, "0.0.0.0", "255.0.0.0") && !IP.CheckIfSameNetwork(ipv4Gateway, "0.0.0.0", "255.255.255.255") ||
                    Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0 && IP.CheckIfSameNetwork(ipv4Gateway, "0.0.0.0", "255.255.255.255") ||
                    IP.CheckIfSameNetwork(ipv4Gateway, "224.0.0.0", "224.0.0.0") ||
                    IP.CheckIfSameNetwork(ipv4Gateway, "240.0.0.0", "240.0.0.0"))
                    {
                        tabControl1.SelectTab(0);
                        new BalloonTip("Warning", "Invalid IPv4 gateway address", defaultIPv4Gateway, BalloonTip.ICON.WARNING);
                        return false;
                    }
                    defaultIPv4Gateway.Text = ipv4Gateway;
                }
            }
            if (unloadIPv6 > 0 && NotActiveIPv6Routes > 0 && defaultInterfaceMode.SelectedIndex == 0 ||
                unloadIPv6 > 0 && defaultInterfaceMode.SelectedIndex == 1)
            {
                if (defaultIPv6Interface.SelectedIndex == -1)
                {
                    tabControl1.SelectTab(1);
                    new BalloonTip("Information", "Select the route's default IPv6 interface", defaultIPv6Interface, BalloonTip.ICON.INFO);
                    return false;
                }
                if (defaultIPv6GatewayMode.SelectedIndex != 3)
                {
                    string ipv6Gateway = defaultIPv6Gateway.Text;
                    if (ipv6Gateway == "")
                        ipv6Gateway = "0.0.0.0";
                    if (!IP.ValidateIPv6(ref ipv6Gateway))
                    // && !Regex.IsMatch(gateway, @"^(::1|::)$") && IPAddress.TryParse(gateway, out ipAddress) && ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        tabControl1.SelectTab(1);
                        new BalloonTip("Warning", "Invalid IPv6 gateway address", defaultIPv6Gateway, BalloonTip.ICON.WARNING);
                        return false;
                    }
                    defaultIPv6Gateway.Text = ipv6Gateway;
                }
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigs())
                return;
            IPAddress ipAddress = new IPAddress(0);
            for (int j = 0; j < unloadRoutes.Count; j++)
                if (listView1.Items[j].Checked &&
                    ValidateRoute(ref unloadRoutes[j].Destination, ref unloadRoutes[j].Prefix, ref unloadRoutes[j].Gateway, unloadRoutes[j].IPVersion))
                {
                    Config.SavedRouteItem savedRoute = unloadRoutes[j];
                    int ifIndex = 0;
                    if (Global.NetworkInterfaces.ContainsKey(savedRoute.InterfaceGuid))
                        ifIndex = Global.NetworkInterfaces[savedRoute.InterfaceGuid].Index;
                    else if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                        NetworkInterface.Loopback.Guid == savedRoute.InterfaceGuid)
                        ifIndex = 1;
                    // load defaults
                    if (defaultInterfaceMode.SelectedIndex == 0 && listView1.Items[j].SubItems[6].Text != "Active" ||
                        defaultInterfaceMode.SelectedIndex == 1)
                    {
                        if (unloadRoutes[j].IPVersion == 4)
                        {
                            ifIndex = int.Parse(Regex.Replace(defaultIPv4Interface.Text, @"^(\d+) .*$", "$1"));
                            if (defaultIPv4GatewayMode.SelectedIndex != 3)
                                unloadRoutes[j].Gateway = defaultIPv4Gateway.Text;
                        }
                        else
                        {
                            ifIndex = int.Parse(Regex.Replace(defaultIPv6Interface.Text, @"^(\d+) .*$", "$1"));
                            if (defaultIPv6GatewayMode.SelectedIndex != 3)
                                unloadRoutes[j].Gateway = defaultIPv6Gateway.Text;
                        }
                    }
                    // unload route
                    Iphlpapi.DeleteRoute(unloadRoutes[j].Destination, unloadRoutes[j].Prefix, unloadRoutes[j].Gateway, ifIndex.ToString());
                }
            Close();
        }
    }
}
