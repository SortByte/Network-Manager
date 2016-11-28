using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinLib.Network;
using WinLib.WinAPI;
using Network_Manager.Jobs.Extensions;

namespace Network_Manager.Gadget.ControlPanel.IPSessions
{
    public partial class IPSessionsForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("IP Sessions");
        private CancellationTokenSource cts = new CancellationTokenSource();
        private List<Iphlpapi.IPSession> sessions = new List<Iphlpapi.IPSession>();
        private ConcurrentDictionary<uint, OwningProcess> processList = new ConcurrentDictionary<uint, OwningProcess>();
        private ConcurrentDictionary<IPAddress, string> DnsRescords = new ConcurrentDictionary<IPAddress, string>();

        public IPSessionsForm()
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
            filterProtocol.SelectedIndex = 2;
            imageList1.Images.Add(WinLib.WinAPI.Shell32.ExtractIcon("shell32.dll", 2, false));
            treeView1.Nodes.Add("root", "All connected processes", 0, 0);
            Show();
        }

        async void UpdateIPSessions()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    Kernel32.GetDeviceNameMap();
                    listView1.BeginUpdate();
                    sessions = Iphlpapi.GetIPSessions();
                    IPAddress ipAddress;
                    // add/update items
                    foreach (Iphlpapi.IPSession session in sessions)
                    {
                        // get process info
                        string filePath = "";
                        int imageIndex = 0;
                        if (processList.ContainsKey(session.OwningPid))
                        {
                            imageIndex = processList[session.OwningPid].ImageListIndex;
                            filePath = processList[session.OwningPid].Path;
                        }
                        else if (processList.Where(i => i.Value.Path == filePath).Count() > 0)
                        {
                            OwningProcess owningProcess = processList.Where(i => i.Value.Path == filePath).First().Value;
                            processList.TryAdd(session.OwningPid, owningProcess);
                            imageIndex = owningProcess.ImageListIndex;
                            filePath = owningProcess.Path;
                        }
                        else
                        {
                            System.Drawing.Icon icon = null;
                            filePath = Psapi.GetProcessFileName(session.OwningPid);
                            if (filePath != "")
                                icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                            if (icon != null)
                            {
                                imageList1.Images.Add(icon);
                                imageIndex = imageList1.Images.Count - 1;
                                OwningProcess owningProcess = new OwningProcess();
                                owningProcess.Path = filePath;
                                owningProcess.ImageListIndex = imageIndex;
                                processList.TryAdd(session.OwningPid, owningProcess);
                            }
                        }
                        // add process in TV
                        if (!treeView1.Nodes[0].Nodes.ContainsKey(Path.GetFileName(filePath) + " (" + session.OwningPid + ")"))
                            treeView1.Nodes[0].Nodes.Add(Path.GetFileName(filePath) + " (" + session.OwningPid + ")",
                                Path.GetFileName(filePath) + " (" + session.OwningPid + ")", imageIndex, imageIndex).Parent.Expand();
                        
                        // filter
                        if (session.SocketID.LocalEP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && comboBox1.SelectedIndex == 1 ||
                            session.SocketID.LocalEP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && comboBox1.SelectedIndex == 0)
                            continue;
                        if (treeView1.SelectedNode != null &&
                            treeView1.SelectedNode.Parent != null)
                            if (session.OwningPid != uint.Parse(Regex.Replace(treeView1.SelectedNode.Text, @"^.*\((\d+)\)$", "$1")))
                                continue;
                        if (filterProtocol.SelectedIndex == 0 && session.SocketID.Protocol != IP.ProtocolFamily.TCP ||
                            filterProtocol.SelectedIndex == 1 && session.SocketID.Protocol != IP.ProtocolFamily.UDP)
                            continue;

                        // update existing items
                        bool found = false;
                        foreach (ListViewItem item in listView1.Items)
                        {
                            // find item
                            if (session.SocketID.Equals(item.Tag))
                            {
                                found = true;
                                item.SubItems[6].Text = session.State;
                                IPEndPoint remoteEP;
                                // resolve IP
                                if (resolveIP.Checked)
                                {
                                    if (((IP.SocketID)item.Tag).Protocol == IP.ProtocolFamily.UDP)
                                    {
                                        if ((remoteEP = UdpDetector.Table.GetRemoteEP(((IP.SocketID)item.Tag).LocalEP)) != null)
                                            if (!DnsRescords.ContainsKey(remoteEP.Address))
                                                ResolveIP(remoteEP.Address);
                                            else if (DnsRescords[remoteEP.Address] != "")
                                                item.SubItems[3].Text = DnsRescords[remoteEP.Address];
                                    }
                                    else if (((IP.SocketID)item.Tag).Protocol == IP.ProtocolFamily.TCP)
                                    {
                                        if (!DnsRescords.ContainsKey(((IP.SocketID)item.Tag).RemoteEP.Address))
                                            ResolveIP(((IP.SocketID)item.Tag).RemoteEP.Address);
                                        else if (DnsRescords[((IP.SocketID)item.Tag).RemoteEP.Address] != "")
                                            item.SubItems[3].Text = DnsRescords[((IP.SocketID)item.Tag).RemoteEP.Address];
                                    }
                                }
                                else
                                {
                                    if (!IPAddress.TryParse(item.SubItems[3].Text, out ipAddress))
                                        item.SubItems[3].Text = ((IP.SocketID)item.Tag).RemoteEP.Address.ToString();
                                }
                                // update remote UDP EP
                                if (((IP.SocketID)item.Tag).Protocol == IP.ProtocolFamily.UDP &&
                                    (item.SubItems[3].Text == "0.0.0.0" || item.SubItems[3].Text == "::" ||
                                    item.SubItems[4].Text == "0") &&
                                    (remoteEP = UdpDetector.Table.GetRemoteEP(((IP.SocketID)item.Tag).LocalEP)) != null)
                                {
                                    item.SubItems[3].Text = remoteEP.Address.ToString();
                                    item.SubItems[4].Text = remoteEP.Port.ToString();
                                }
                                // update bytes
                                if (getBytes.Checked == true)
                                {
                                    ByteCounter.ByteTable.Bytes bytes = ByteCounter.Table.GetBytes((IP.SocketID)item.Tag);
                                    if (bytes.Received > 0 || bytes.Sent > 0)
                                    {
                                        item.SubItems[7].Text = Unit.AutoScale(bytes.Received, "B");
                                        item.SubItems[8].Text = Unit.AutoScale(bytes.Sent, "B");
                                    }
                                    else
                                    {
                                        item.SubItems[7].Text = "";
                                        item.SubItems[8].Text = "";
                                    }
                                }
                            }
                        }

                        if (!found)
                            listView1.Items.Add(new ListViewItem(new string[] { 
                                Path.GetFileName(filePath) + " (" + session.OwningPid + ")",
                                session.SocketID.LocalEP.Address.ToString(),
                                session.SocketID.LocalEP.Port.ToString(),
                                session.SocketID.RemoteEP.Address.ToString(),
                                session.SocketID.RemoteEP.Port.ToString(),
                                session.SocketID.Protocol.ToString(),
                                session.State,
                                "", "" }, imageIndex)).Tag = session.SocketID;
                            
                    }
                    // delete items
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (!sessions.Any((i) => i.SocketID.Equals(item.Tag)) ||
                            item.SubItems[1].Text.Contains(':') && comboBox1.SelectedIndex == 0 ||
                            !item.SubItems[1].Text.Contains(':') && comboBox1.SelectedIndex == 1 ||
                            filterProtocol.SelectedIndex == 0 && item.SubItems[5].Text != "TCP" ||
                            filterProtocol.SelectedIndex == 1 && item.SubItems[5].Text != "UDP")
                        {
                            item.Remove();
                        }
                        else if (treeView1.SelectedNode != null &&
                            treeView1.SelectedNode.Parent != null)
                            if (item.SubItems[0].Text != treeView1.SelectedNode.Text)
                                item.Remove();
                    }
                        
                    foreach (KeyValuePair<uint, OwningProcess> process in processList)
                        if (sessions.Find(i => i.OwningPid == process.Key) == null)
                        {
                            treeView1.Nodes[0].Nodes.RemoveByKey(Path.GetFileName(process.Value.Path) + " (" + process.Key + ")");
                            OwningProcess value;
                            processList.TryRemove(process.Key, out value);
                        }
                            
                    foreach (ColumnHeader column in listView1.Columns)
                        column.Width = -2;
                    listView1.Sort();
                    listView1.EndUpdate();
                    //Unit.Compare("10.5 KB", "10.5 B");
                    await TaskEx.Delay(1000);
                }
            }
            catch (Exception e) { Global.WriteLog(e.ToString()); }
        }

        private void IPSessionsForm_Load(object sender, EventArgs e)
        {
            UpdateIPSessions();
        }

        private void IPSessionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
            Instance = null;
            UdpDetector.Stop();
            ByteCounter.Stop();
            busyForm.Done.SetResult(true);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < listView1.Columns.Count)
                listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
            if (listView1.Sorting == SortOrder.None)
                listView1.Sorting = SortOrder.Ascending;
            else if (listView1.Sorting == SortOrder.Ascending)
                listView1.Sorting = SortOrder.Descending;
            else if (listView1.Sorting == SortOrder.Descending)
                listView1.Sorting = SortOrder.None;
            listView1.Sort();
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
                ListView listView = ((ListViewItem)x).ListView;
                ListViewItem xItem = (ListViewItem)x;
                ListViewItem yItem = (ListViewItem)y;
                if (listView.Sorting == SortOrder.Ascending)
                    if (col == 7 || col == 8)
                        return Unit.Compare(xItem.SubItems[col].Text, yItem.SubItems[col].Text);
                    else
                        return String.Compare(xItem.SubItems[col].Text, yItem.SubItems[col].Text);
                if (listView.Sorting == SortOrder.Descending)
                    if (col == 7 || col == 8)
                        return - Unit.Compare(xItem.SubItems[col].Text, yItem.SubItems[col].Text);
                    else
                        return - String.Compare(xItem.SubItems[col].Text, yItem.SubItems[col].Text);
                return 0;
            }
        }

        class OwningProcess
        {
            public string Path;
            public int ImageListIndex;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e == null || e.Node.Parent == null)
            {
                textBox1.Text = "[Process path]";
                return;
            }
            uint pid = uint.Parse(Regex.Replace(e.Node.Text, @"^.*\((\d+)\)$", "$1"));
            if (processList.ContainsKey(pid))
                textBox1.Text = processList[pid].Path;
            else
                textBox1.Text = "N/A";
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            if (treeView1.SelectedNode == null)
                treeView1_AfterSelect(sender, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int ipVersion = 0;
            if (listView1.SelectedItems.Count == 0)
            {
                new BalloonTip("No selection", "Select an IP session first", listView1, BalloonTip.ICON.INFO, 5000);
                return;
            }
            if (((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ipVersion = 4;
            else if (((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                ipVersion = 6;
            IPEndPoint remoteEP;
            if (((IP.SocketID)listView1.SelectedItems[0].Tag).Protocol == IP.ProtocolFamily.UDP &&
                UdpDetector.Table.GetRemoteEP(((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP) != null)
                remoteEP = UdpDetector.Table.GetRemoteEP(((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP);
            else
                remoteEP = ((IP.SocketID)listView1.SelectedItems[0].Tag).RemoteEP;
            RouteIPForm form = new RouteIPForm(ipVersion, remoteEP.Address.ToString());
            form.ShowDialog();
        }

        private void ResolveIP(IPAddress ip)
        {
            DnsRescords.TryAdd(ip, "");
            if (ip.GetAddressBytes().Max() > 0)
                Dns.BeginGetHostEntry(ip, new AsyncCallback(ResolveIPCallback), ip);
        }

        private void ResolveIPCallback(IAsyncResult asyncResult)
        {
            IPAddress ip = (IPAddress)asyncResult.AsyncState;
            try
            {
                IPHostEntry host = Dns.EndGetHostEntry(asyncResult);
                DnsRescords[ip] = host.HostName;
            }
            catch { }
        }

        private void detectRemoteUdp_CheckedChanged(object sender, EventArgs e)
        {
            if (detectRemoteUdp.Enabled == false)
                return;
            detectRemoteUdp.Enabled = false;
            if (detectRemoteUdp.Checked == true)
            {
                if (!Jobs.Extensions.Dependencies.Check() ||
                    !Jobs.Extensions.UdpDetector.Start())
                    detectRemoteUdp.Checked = false;
            }
            else
            {
                Jobs.Extensions.UdpDetector.Stop();
            }
            detectRemoteUdp.Enabled = true;
        }

        private void getBytes_CheckedChanged(object sender, EventArgs e)
        {
            if (getBytes.Enabled == false)
                return;
            getBytes.Enabled = false;
            if (getBytes.Checked == true)
            {
                if (!Jobs.Extensions.Dependencies.Check() ||
                    !Jobs.Extensions.ByteCounter.Start())
                    getBytes.Checked = false;
            }
            else
            {
                Jobs.Extensions.ByteCounter.Stop();
            }
            getBytes.Enabled = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                new BalloonTip("No selection", "Select an IP session first", listView1, BalloonTip.ICON.INFO, 5000);
                return;
            }
            IPEndPoint remoteEP;
            if (((IP.SocketID)listView1.SelectedItems[0].Tag).Protocol == IP.ProtocolFamily.UDP &&
                UdpDetector.Table.GetRemoteEP(((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP) != null)
                remoteEP = UdpDetector.Table.GetRemoteEP(((IP.SocketID)listView1.SelectedItems[0].Tag).LocalEP);
            else
                remoteEP = ((IP.SocketID)listView1.SelectedItems[0].Tag).RemoteEP;
            Clipboard.SetText(remoteEP.ToString());
        }
    }
}