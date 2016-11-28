using System;
using System.Collections;
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

namespace Network_Manager.Gadget.ControlPanel.Routes.SavedRoutes
{
    public partial class LoadForm : Form
    {
        private string lastIPv4Gateway = "0.0.0.0";
        private string lastIPv6Gateway = "::";
        private int lastIPv4GatewayMode = 3;
        private int lastIPv6GatewayMode = 3;
        private TreeView treeView;
        private int loadIPv4 = 0;
        private int loadIPv6 = 0;
        private int NotConnectedIPv4Interfaces = 0;
        private int NotConnectedIPv6Interfaces = 0;
        private int NotActiveIPv4Routes = 0;
        private int NotActiveIPv6Routes = 0;
        private List<Config.SavedRouteItem> loadRoutes;
        private List<Iphlpapi.Route> activeRoutes;


        public LoadForm(TreeView treeView)
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
            Config.SavedRouteNode loadNode = Global.Config.SavedRoutes.GetSelectedNode(treeView);
            loadRoutes = Global.Config.SavedRoutes.GetRoutes(loadNode);
            activeRoutes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_UNSPEC);
            foreach (Config.SavedRouteItem item in loadRoutes)
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
                            loadIPv4++;
                            if (Global.NetworkInterfaces[item.InterfaceGuid].IPv4Address.Count > 0)
                                interfaceIndex += Global.NetworkInterfaces[item.InterfaceGuid].IPv4Address[0].Address;
                            else
                                interfaceIndex += "0.0.0.0";
                        }
                        else
                        {
                            loadIPv6++;
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
                            loadIPv4++;
                            interfaceIndex += "127.0.0.1)";
                        }
                        else
                        {
                            loadIPv6++;
                            interfaceIndex += "::1)";
                        }
                    }
                    else
                    {
                        if (item.IPVersion == 4)
                        {
                            loadIPv4++;
                            NotConnectedIPv4Interfaces++;
                        }
                        else
                        {
                            loadIPv6++;
                            NotConnectedIPv6Interfaces++;
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
                        loadIPv4++;
                        NotActiveIPv4Routes++;
                        NotConnectedIPv4Interfaces++;
                    }
                    else
                    {
                        loadIPv6++;
                        NotActiveIPv6Routes++;
                        NotConnectedIPv6Interfaces++;
                    }

                }
                ListViewItem lvItem = new ListViewItem(new string[] {
                    item.Destination,
                    item.Prefix,
                    item.Gateway,
                    interfaceIndex,
                    item.Metric.ToString(),
                    item.Name,
                    status
                });
                lvItem.Tag = item;
                listView1.Items.Add(lvItem).Checked = true;
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
                if (loadIPv4 > 0 && NotConnectedIPv4Interfaces > 0)
                {
                    tabControl1.TabPages[0].Enabled = true;
                    button1.Enabled = true;
                    if (loadIPv6 == 0 || NotConnectedIPv6Interfaces == 0)
                        tabControl1.SelectTab(0);
                }
                else
                {
                    tabControl1.TabPages[0].Enabled = false;
                }
                if (loadIPv6 > 0 && NotConnectedIPv6Interfaces > 0)
                {
                    tabControl1.TabPages[1].Enabled = true;
                    button1.Enabled = true;
                    if (loadIPv4 == 0 || NotConnectedIPv4Interfaces == 0)
                        tabControl1.SelectTab(1);
                }
                else
                    tabControl1.TabPages[1].Enabled = false;
            }
            else
            // overwrite
            {

                if (loadIPv4 > 0)
                    tabControl1.TabPages[0].Enabled = true;
                else
                    tabControl1.TabPages[0].Enabled = false;
                if (loadIPv6 > 0)
                    tabControl1.TabPages[1].Enabled = true;
                else
                    tabControl1.TabPages[1].Enabled = false;
            }
            if (loadIPv4 == 0 && loadIPv6 == 0)
            {
                tabControl1.TabPages[0].Enabled = false;
                tabControl1.TabPages[1].Enabled = false;
                button1.Enabled = false;
            }
            else
                button1.Enabled = true;
            if (tabControl1.TabPages[0].Enabled || tabControl1.TabPages[1].Enabled)
            {
                updateSavedRoutesCheckBox.Checked = true;
                updateSavedRoutesCheckBox.Enabled = true;
            }
            else
            {
                updateSavedRoutesCheckBox.Checked = false;
                updateSavedRoutesCheckBox.Enabled = false;
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

            if (e.Item.SubItems[3].Text == "Not connected")
                if (e.Item.Checked)
                    if (loadRoutes[e.Item.Index].IPVersion == 4)
                        NotConnectedIPv4Interfaces++;
                    else
                        NotConnectedIPv6Interfaces++;
                else
                    if (loadRoutes[e.Item.Index].IPVersion == 4)
                        NotConnectedIPv4Interfaces--;
                    else
                        NotConnectedIPv6Interfaces--;
            if (e.Item.Checked)
                if (loadRoutes[e.Item.Index].IPVersion == 4)
                    loadIPv4++;
                else
                    loadIPv6++;
            else
                if (loadRoutes[e.Item.Index].IPVersion == 4)
                    loadIPv4--;
                else
                    loadIPv6--;
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

        private bool ValidateConfigs()
        {
            IPAddress ipAddress = new IPAddress(0);
            if (loadIPv4 > 0 && NotConnectedIPv4Interfaces > 0 && defaultInterfaceMode.SelectedIndex == 0 ||
                loadIPv4 > 0 && defaultInterfaceMode.SelectedIndex == 1)
            {
                if (defaultIPv4Interface.SelectedIndex == -1)
                {
                    tabControl1.SelectTab(0);
                    new BalloonTip("Information", "Select the default IPv4 interface through which to route", defaultIPv4Interface, BalloonTip.ICON.INFO);
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
            if (loadIPv6 > 0 && NotConnectedIPv6Interfaces > 0 && defaultInterfaceMode.SelectedIndex == 0 ||
                loadIPv6 > 0 && defaultInterfaceMode.SelectedIndex == 1)
            {
                if (defaultIPv6Interface.SelectedIndex == -1)
                {
                    tabControl1.SelectTab(1);
                    new BalloonTip("Information", "Select the default IPv6 interface through which to route", defaultIPv6Interface, BalloonTip.ICON.INFO);
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigs())
                return;
            IPAddress ipAddress = new IPAddress(0);
            for (int j = 0; j < loadRoutes.Count; j++)
                if (listView1.Items[j].Checked &&
                    ValidateRoute(ref loadRoutes[j].Destination, ref loadRoutes[j].Prefix, ref loadRoutes[j].Gateway, loadRoutes[j].IPVersion))
                {
                    Config.SavedRouteItem savedRoute = loadRoutes[j];
                    int ifIndex = 0;
                    if (Global.NetworkInterfaces.ContainsKey(savedRoute.InterfaceGuid))
                        ifIndex = Global.NetworkInterfaces[savedRoute.InterfaceGuid].Index;
                    else if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                        NetworkInterface.Loopback.Guid == savedRoute.InterfaceGuid)
                        ifIndex = 1;
                    // load defaults
                    if (defaultInterfaceMode.SelectedIndex == 0 && ifIndex == 0 ||
                        defaultInterfaceMode.SelectedIndex == 1)
                    {
                        if (loadRoutes[j].IPVersion == 4)
                        {
                            ifIndex = int.Parse(Regex.Replace(defaultIPv4Interface.Text, @"^(\d+) .*$", "$1"));
                            if (defaultIPv4GatewayMode.SelectedIndex != 3)
                                loadRoutes[j].Gateway = defaultIPv4Gateway.Text;
                        }
                        else
                        {
                            ifIndex = int.Parse(Regex.Replace(defaultIPv6Interface.Text, @"^(\d+) .*$", "$1"));
                            if (defaultIPv6GatewayMode.SelectedIndex != 3)
                                loadRoutes[j].Gateway = defaultIPv6Gateway.Text;
                        }
                    }
                    // if on-link set type to direct for XP
                    Iphlpapi.MIB_IPFORWARD_TYPE type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_INDIRECT;
                    NetworkInterface nic;
                    if (ifIndex == 1)
                        nic = NetworkInterface.Loopback;
                    else
                        nic = Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).First();
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    {

                        if (savedRoute.IPVersion == 4)
                        {
                            if (nic.IPv4Address.Where((i) => i.Address == savedRoute.Gateway).Count() > 0)
                                type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                        }
                        else
                        {
                            if (nic.IPv6Address.All.Where((i) => i.Address == savedRoute.Gateway).Count() > 0)
                                type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                        }
                        if (savedRoute.Gateway == "0.0.0.0" || savedRoute.Gateway == "::")
                            type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                    }
                    // correction for Vista->XP transitioned saved route
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    {
                        if (IPAddress.TryParse(savedRoute.Gateway, out ipAddress))
                            if (IPAddress.Parse(savedRoute.Gateway).GetAddressBytes().Max() == 0)
                                if (savedRoute.IPVersion == 4)
                                    savedRoute.Gateway = nic.IPv4Address.First().Address;
                                else
                                    savedRoute.Gateway = nic.IPv6Address.All.First().Address;
                        if (savedRoute.Metric == 0)
                            savedRoute.Metric = 1;
                    }
                    Iphlpapi.DeleteRoute(savedRoute.Destination, savedRoute.Prefix, savedRoute.Gateway, ifIndex.ToString());
                    Iphlpapi.AddRoute(savedRoute.Destination, savedRoute.Prefix, savedRoute.Gateway, ifIndex.ToString(), savedRoute.Metric.ToString(), type);
                    if (updateSavedRoutesCheckBox.Checked == true)
                    {
                        // TODO: update saved route with new interface + gw
                    }
                }
                else
                {
                    MessageBox.Show("Could not load route:\nDestination: " + loadRoutes[j].Destination + "\nPrefix: " + loadRoutes[j].Prefix + "\nGateway: " + loadRoutes[j].Gateway + "\nIP version: " + loadRoutes[j].IPVersion, "Invalid route", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            Close();
        }
    }
}
