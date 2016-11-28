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
using WinLib.Forms;
using WinLib.WinAPI;

namespace Network_Manager.Gadget.ControlPanel.IPSessions
{
    public partial class RouteIPForm : Form
    {
        private string lastIPv4Gateway = "0.0.0.0";
        private string lastIPv6Gateway = "::";
        private int lastGatewayMode = 1;
        private int ipVersion;
        private TreeNode dragOverNode;

        public RouteIPForm(int ipVersion, string destination)
        {
            InitializeComponent();
            this.ipVersion = ipVersion;
            if (ipVersion == 4)
            {
                Text = "Add IPv4 Route";
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
                Text = "Add IPv6 Route";
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
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                routeMetric.Text = "1";
            }
            else
            {
                toolTip1.SetToolTip(routeMetric, "Route metric offset. The actual route metric is the summation of interface metric");
                routeMetric.Text = "0";
            }
            // load route
            routeDestination.Text = destination;

            checkBox1.Checked = false;

            // load saved routes
            Global.Config.SavedRoutes.Populate(treeView1);
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
            if (checkBox1.Checked)
            {
                if (savedRouteName.Text == "")
                {
                    new BalloonTip("Warning", "Route name can't be empty", savedRouteName, BalloonTip.ICON.WARNING);
                    return false;
                }
                if (treeView1.SelectedNode == null)
                {
                    new BalloonTip("Information", "Select the destination where to save the route", treeView1, BalloonTip.ICON.INFO);
                    return false;
                }
            }
            return true;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                groupBox2.Visible = true;
                button1.Location = new Point(button1.Location.X, button1.Location.Y + groupBox2.Height);
                Height += groupBox2.Height;
            }
            else
            {
                groupBox2.Visible = false;
                button1.Location = new Point(button1.Location.X, button1.Location.Y - groupBox2.Height);
                Height -= groupBox2.Height;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                createSavedRouteGroup.Enabled = false;
                renameSavedRouteGroup.Enabled = false;
                deleteSavedRouteGroup.Enabled = false;
            }
            else if (e.Node.ImageIndex == 0) // group
            {
                createSavedRouteGroup.Enabled = true;
                if (e.Node.Parent == null) // root
                {
                    renameSavedRouteGroup.Enabled = false;
                    deleteSavedRouteGroup.Enabled = false;
                }
                else
                {
                    renameSavedRouteGroup.Enabled = true;
                    deleteSavedRouteGroup.Enabled = true;
                }
            }
            else // item
            {
                createSavedRouteGroup.Enabled = true;
                renameSavedRouteGroup.Enabled = false;
                deleteSavedRouteGroup.Enabled = false;
            }
        }

        private void createSavedRouteGroup_Click(object sender, EventArgs e)
        {
            Routes.SavedRoutes.EditGroupForm form = new Routes.SavedRoutes.EditGroupForm(treeView1);
            form.ShowDialog();
        }

        private void renameSavedRouteGroup_Click(object sender, EventArgs e)
        {
            Routes.SavedRoutes.EditGroupForm form = new Routes.SavedRoutes.EditGroupForm(treeView1, true);
            form.ShowDialog();
        }

        private void deleteSavedRouteGroup_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, "Do you want to delete the saved route group \"" + treeView1.SelectedNode.Text + "\" and all of its subitems ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
                Global.Config.SavedRoutes.DeleteNode(treeView1);
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            if (treeView1.SelectedNode == null)
                treeView1_AfterSelect(sender, null);
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            Refresh();
            TreeNode movingNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            if (movingNode == null || dragOverNode == null)
                return;
            if (dragOverNode.ImageIndex == 1)
                dragOverNode = dragOverNode.Parent;
            if (dragOverNode == movingNode)
            {
                new BalloonTip("Warning", "The chosen destination is the same as the source", treeView1, BalloonTip.ICON.WARNING);
                return;
            }
            if (treeView1_SearchNodeRecursively(movingNode.Nodes, dragOverNode))
            {
                new BalloonTip("Warning", "Can't move a group into one of its childs", treeView1, BalloonTip.ICON.WARNING);
                return;
            }
            if (dragOverNode == movingNode.Parent)
            {
                new BalloonTip("Warning", "The chosen destination is the same as before", treeView1, BalloonTip.ICON.WARNING);
                return;
            }
            int result = Global.Config.SavedRoutes.MoveNode(treeView1, movingNode, dragOverNode);
            if (result == 1)
            {
                new BalloonTip("Error", "Invalid source or destination", treeView1, BalloonTip.ICON.ERROR);
                return;
            }
            if (result == 2)
            {
                new BalloonTip("Warning", "Source name already used at destiation", treeView1, BalloonTip.ICON.WARNING);
                return;
            }
        }

        private bool treeView1_SearchNodeRecursively(TreeNodeCollection nodes, TreeNode searchNode)
        {
            foreach (TreeNode node in nodes)
                if (node == searchNode)
                    return true;
                else if (treeView1_SearchNodeRecursively(node.Nodes, searchNode))
                    return true;
            return false;
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            TreeNode hoveredNode = treeView1.GetNodeAt(treeView1.PointToClient(Cursor.Position));
            if (hoveredNode != null && hoveredNode != dragOverNode)
            {
                dragOverNode = hoveredNode;
                Graphics g = treeView1.CreateGraphics();
                int rightEdge = dragOverNode.Bounds.Right;
                Point[] triangleArrow = new Point[3] {
                    new Point(rightEdge, dragOverNode.Bounds.Top + dragOverNode.Bounds.Height / 2),
                    new Point(rightEdge + 4, dragOverNode.Bounds.Top),
                    new Point(rightEdge + 4, dragOverNode.Bounds.Bottom)
                };
                Refresh();
                g.FillPolygon(System.Drawing.Brushes.Black, triangleArrow);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateRoute())
                return;
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
            // save route
            if (checkBox1.Checked)
            {
                Config.SavedRouteItem savedRoute = new Config.SavedRouteItem();
                savedRoute.Name = savedRouteName.Text;
                savedRoute.Destination = routeDestination.Text;
                savedRoute.Prefix = routePrefix.Text;
                savedRoute.Gateway = routeGateway.Text;
                if (ifIndex == 1)
                    savedRoute.InterfaceGuid = NetworkInterface.Loopback.Guid;
                else
                    savedRoute.InterfaceGuid = Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).FirstOrDefault().Guid;
                savedRoute.Metric = ushort.Parse(routeMetric.Text);
                savedRoute.IPVersion = ipVersion;
                int result = Global.Config.SavedRoutes.AddNode(treeView1, savedRoute);
                if (result == 1)
                {
                    new BalloonTip("Error", "Invalid destination", button1, BalloonTip.ICON.ERROR);
                    return;
                }
                if (result == 2)
                {
                    DialogResult dialogResult = MessageBox.Show("Route name already used at current destination.\nDo you want to overwrite?", "Overwrite confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (dialogResult == DialogResult.No)
                        return;
                    if (treeView1.SelectedNode.ImageIndex == (int)Config.SavedRouteNode.ImageIndex.Group)
                        treeView1.SelectedNode = treeView1.SelectedNode.Nodes.Find(savedRouteName.Text, false).First();
                    else if (treeView1.SelectedNode.Name != savedRouteName.Text)
                        treeView1.SelectedNode = treeView1.SelectedNode.Parent.Nodes.Find(savedRouteName.Text, false).First();
                    Global.Config.SavedRoutes.DeleteNode(treeView1);
                    Global.Config.SavedRoutes.AddNode(treeView1, savedRoute);
                }
            }
            Close();
        }

        private void RouteIPForm_SizeChanged(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            Location = new Point(workingArea.Left + workingArea.Width / 2 - Width / 2,
                workingArea.Top + workingArea.Height / 2 - Height / 2);
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null &&
                treeView1.SelectedNode.ImageIndex == (int)Config.SavedRouteNode.ImageIndex.Item)
            {
                savedRouteName.Text = treeView1.SelectedNode.Text;
            }
        }
    }
}