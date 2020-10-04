using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinLib.Network;
using WinLib.WinAPI;

namespace Network_Manager.Gadget.ControlPanel.Routes
{
    // TODO: add advanced route options (publish, persistent)
    // IPv4 persistent routes are stored in HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\PersistentRoutes
    // IPv6 persistent routes seems to be stored in HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Nsi\{eb004a01-9b1a-11d4-9123-0050047759bc}\16 (probably safer through netsh)
    public partial class RoutesForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("Routes");
        private CancellationTokenSource cts = new CancellationTokenSource();
        private TreeNode dragOverNode;

        public RoutesForm()
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
            // enable double buffering to stop flickering
            var doubleBufferPropertyInfo = listView1.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(listView1, true, null);

            comboBox1.SelectedIndex = 0;
            Global.Config.SavedRoutes.Populate(treeView1);
            Global.Config.SavedRoutes.NodesChanged += SavedRoutes_NodesChanged;
            Show();
        }

        void SavedRoutes_NodesChanged(object sender, EventArgs e)
        {
            Global.Config.SavedRoutes.Populate(treeView1);
        }

        async void UpdateRoutes(CancellationTokenSource cts)
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    listView1.BeginUpdate();
                    List<Iphlpapi.Route> routes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_UNSPEC);
                    List<Config.SavedRouteItem> savedRoutes = Global.Config.SavedRoutes.GetRoutes(Global.Config.SavedRoutes.Nodes[0]);
                    Config.SavedRouteNode selectedNode = Global.Config.SavedRoutes.GetSelectedNode(treeView1);
                    List<Config.SavedRouteItem> selectedRoutes = Global.Config.SavedRoutes.GetRoutes(selectedNode);
                    // add/update items
                    foreach (Iphlpapi.Route route in routes)
                    {
                        if (route.IPVersion == 4 && comboBox1.SelectedIndex == 1 ||
                            route.IPVersion == 6 && comboBox1.SelectedIndex == 0)
                            continue;
                        // skip disconnected interfaces
                        if (Global.NetworkInterfaces.Values.Where((i) => i.Index == route.InterfaceIndex).Count() == 0 &&
                            route.InterfaceIndex != NetworkInterface.Loopback.Index)
                            continue;
                        // calculate route metric
                        if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                            if (route.InterfaceIndex != NetworkInterface.Loopback.Index)
                                route.Metric += Global.NetworkInterfaces.Values.Where((i) => i.Index == route.InterfaceIndex).FirstOrDefault().InterfaceMetric;
                            else
                                route.Metric += NetworkInterface.Loopback.InterfaceMetric;
                        bool found = false;
                        string ifIndex;
                        if (route.InterfaceIndex == 1)
                            ifIndex = route.InterfaceIndex.ToString() + " (" + NetworkInterface.Loopback.Name + ")";
                        else
                            ifIndex = route.InterfaceIndex.ToString() + " (" + Global.NetworkInterfaces.Values.Where((i) => i.Index == route.InterfaceIndex).FirstOrDefault().Name + ")";
                        // filter routes
                        bool filter = false;
                        if (checkBox1.Checked)
                        {
                            if (selectedRoutes.Find(i =>
                                i.Destination == route.Destination &&
                                i.Prefix == route.Prefix &&
                                i.Gateway == route.Gateway &&
                                (Global.NetworkInterfaces.ContainsKey(i.InterfaceGuid) &&
                                Global.NetworkInterfaces[i.InterfaceGuid].Index == route.InterfaceIndex ||
                                NetworkInterface.Loopback.Guid == i.InterfaceGuid &&
                                NetworkInterface.Loopback.Index == route.InterfaceIndex)) == null)
                                filter = true;
                        }
                        // TODO: update routes with all matching saved routes using Find()
                        // update existing items
                        foreach (ListViewItem item in listView1.Items)
                            if (item.SubItems[0].Text == route.Destination &&
                                item.SubItems[1].Text == route.Prefix &&
                                item.SubItems[2].Text == route.Gateway &&
                                item.SubItems[3].Text == ifIndex)
                            {
                                item.SubItems[4].Text = route.Age.ToString();
                                item.SubItems[5].Text = route.Metric.ToString();
                                Config.SavedRouteItem savedRoute = savedRoutes.Find(i =>
                                    i.Destination == route.Destination &&
                                    i.Prefix == route.Prefix &&
                                    i.Gateway == route.Gateway &&
                                    (Global.NetworkInterfaces.ContainsKey(i.InterfaceGuid) &&
                                    Global.NetworkInterfaces[i.InterfaceGuid].Index == route.InterfaceIndex ||
                                    NetworkInterface.Loopback.Guid == i.InterfaceGuid &&
                                    NetworkInterface.Loopback.Index == route.InterfaceIndex));
                                if (savedRoute != null)
                                    item.SubItems[6].Text = savedRoute.Name;
                                else
                                    item.SubItems[6].Text = "";
                                found = true;
                                break;
                            }
                        
                        if (found || filter)
                            continue;
                        listView1.Items.Add(new ListViewItem(new string[] { route.Destination, route.Prefix, route.Gateway, ifIndex, route.Age.ToString(), route.Metric.ToString(), "" }));
                    }
                    // delete items
                    foreach (ListViewItem item in listView1.Items)
                    {
                        string ifIndex = Regex.Replace(item.SubItems[3].Text, @"^(\d+) .*$", "$1");
                        // filter routes
                        bool filter = false;
                        if (checkBox1.Checked)
                        {
                            if (selectedRoutes.Find(i =>
                                i.Destination == item.SubItems[0].Text &&
                                i.Prefix == item.SubItems[1].Text &&
                                i.Gateway == item.SubItems[2].Text &&
                                (Global.NetworkInterfaces.ContainsKey(i.InterfaceGuid) &&
                                Global.NetworkInterfaces[i.InterfaceGuid].Index.ToString() == ifIndex ||
                                NetworkInterface.Loopback.Guid == i.InterfaceGuid &&
                                NetworkInterface.Loopback.Index.ToString() == ifIndex)) == null)
                                filter = true;
                        }
                        if (routes.Find((i) =>
                            i.Destination == item.SubItems[0].Text &&
                            i.Prefix == item.SubItems[1].Text &&
                            i.Gateway == item.SubItems[2].Text &&
                            i.InterfaceIndex.ToString() == ifIndex) == null ||
                            item.SubItems[0].Text.Contains(':') && comboBox1.SelectedIndex == 0 ||
                            !item.SubItems[0].Text.Contains(':') && comboBox1.SelectedIndex == 1 ||
                            filter)
                            item.Remove();
                    }
                    foreach (ColumnHeader column in listView1.Columns)
                        column.Width = -2;
                    listView1.EndUpdate();
                    await TaskEx.Delay(1000);
                }
            }
            catch { }
        }
        
        private void RoutesForm_Load(object sender, EventArgs e)
        {
            UpdateRoutes(cts);
        }

        private void RoutesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
            Instance = null;
            busyForm.Done.SetResult(true);
            Global.Config.SavedRoutes.NodesChanged -= SavedRoutes_NodesChanged;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                // || Regex.Replace(listView1.SelectedItems[0].SubItems[3].Text, @"^(\d+) .*$", "$1") == "1")
            {
                button7.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
            }
            else
            {
                
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                {
                    List<Iphlpapi.Route> routes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_INET);
                    Iphlpapi.Route route = routes.Find((i) =>
                        i.Destination == listView1.SelectedItems[0].SubItems[0].Text &&
                        i.Prefix == listView1.SelectedItems[0].SubItems[1].Text &&
                        i.Gateway == listView1.SelectedItems[0].SubItems[2].Text &&
                        i.InterfaceIndex.ToString() == Regex.Replace(listView1.SelectedItems[0].SubItems[3].Text, @"^(\d+) .*$", "$1")
                    );
                    if (route != null)
                        if (route.Protocol == Iphlpapi.NL_ROUTE_PROTOCOL.MIB_IPPROTO_LOCAL)
                        {
                            button7.Enabled = false;
                            button8.Enabled = false;
                            button9.Enabled = false;
                            return;
                        }
                }
                button7.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AddRouteForm form = new AddRouteForm(comboBox1.SelectedIndex == 0 ? 4 : 6);
            form.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ushort metric = ushort.Parse(listView1.SelectedItems[0].SubItems[5].Text);
            int ifIndex = int.Parse(Regex.Replace(listView1.SelectedItems[0].SubItems[3].Text, @"^(\d+) .*$", "$1"));
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                if (ifIndex != 1)
                    metric -= Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).FirstOrDefault().InterfaceMetric;
                else
                    metric -= NetworkInterface.Loopback.InterfaceMetric;
            EditRouteForm form = new EditRouteForm(comboBox1.SelectedIndex == 0 ? 4 : 6,
                listView1.SelectedItems[0].SubItems[0].Text,
                listView1.SelectedItems[0].SubItems[1].Text,
                listView1.SelectedItems[0].SubItems[2].Text,
                ifIndex.ToString(),
                metric.ToString());
            form.ShowDialog();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, "Do you want to delete the following route ?\n" +
                "\nDestination: " + listView1.SelectedItems[0].SubItems[0].Text +
                "\nPrefix: " + listView1.SelectedItems[0].SubItems[1].Text +
                "\nGateway: " + listView1.SelectedItems[0].SubItems[2].Text +
                "\nInterface Index: " + listView1.SelectedItems[0].SubItems[3].Text,
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.No)
                return;
            Iphlpapi.DeleteRoute(listView1.SelectedItems[0].SubItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text, listView1.SelectedItems[0].SubItems[2].Text, Regex.Replace(listView1.SelectedItems[0].SubItems[3].Text, @"^(\d+) .*$", "$1"));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ushort metric = ushort.Parse(listView1.SelectedItems[0].SubItems[5].Text);
            int ifIndex = int.Parse(Regex.Replace(listView1.SelectedItems[0].SubItems[3].Text, @"^(\d+) .*$", "$1"));
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                if (ifIndex != 1)
                    metric -= Global.NetworkInterfaces.Values.Where((i) => i.Index == ifIndex).FirstOrDefault().InterfaceMetric;
                else
                    metric -= NetworkInterface.Loopback.InterfaceMetric;
            SaveRouteForm form = new SaveRouteForm(comboBox1.SelectedIndex == 0 ? 4 : 6,
                listView1.SelectedItems[0].SubItems[0].Text,
                listView1.SelectedItems[0].SubItems[1].Text,
                listView1.SelectedItems[0].SubItems[2].Text,
                ifIndex.ToString(),
                metric.ToString());
            form.ShowDialog();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < listView1.Columns.Count - 1)
            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        class ListViewItemComparer : IComparer
        {
            private int col = 0;

            public ListViewItemComparer(int column)
            {
                col = column;
            }
            public int Compare(object x, object y)
            {
                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }
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

        private bool treeView1_SearchNodeRecursively(TreeNodeCollection nodes, TreeNode searchNode)
        {
            foreach (TreeNode node in nodes)
                if (node == searchNode)
                    return true;
                else if (treeView1_SearchNodeRecursively(node.Nodes, searchNode))
                    return true;
            return false;
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                if (treeView1.SelectedNode.Parent == null)
                {
                    button2.Enabled = false;
                    button3.Enabled = false;
                }
                else
                {
                    button2.Enabled = true;
                    button3.Enabled = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SavedRoutes.AddNodeForm form = new SavedRoutes.AddNodeForm(treeView1);
            form.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode.ImageIndex == 0)
            {
                SavedRoutes.EditGroupForm form = new SavedRoutes.EditGroupForm(treeView1, true);
                form.ShowDialog();
            }
            else
            {
                SavedRoutes.EditItemForm form = new SavedRoutes.EditItemForm(treeView1);
                form.ShowDialog();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string message;
            if (treeView1.SelectedNode.ImageIndex == 0)
                message = "Do you want to delete the saved route group \"" + treeView1.SelectedNode.Text + "\" and all of its subitems ?";
            else
                message = "Do you want to delete the saved route \"" + treeView1.SelectedNode.Text + "\" ?";
            DialogResult result = MessageBox.Show(this, message, "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
                Global.Config.SavedRoutes.DeleteNode(treeView1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SavedRoutes.LoadForm form = new SavedRoutes.LoadForm(treeView1);
            form.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SavedRoutes.UnloadForm form = new SavedRoutes.UnloadForm(treeView1);
            form.ShowDialog();
        }


    }
}
