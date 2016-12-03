using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WinLib.Network;
using WinLib.WinAPI;
using WinLib.Forms;
using WinLib.Sync;
using WinLib.Extensions;

namespace Network_Manager.Gadget.ControlPanel
{
    // TODO: add disable/enable Teredo option
    // TODO: add IP forwarding
    // TODO: add WLAN configs, info and ICS
    public partial class ControlPanelForm : Form
    {
        public static Form Instance = null;
        private MouseEventArgs lastMouseEvent = new MouseEventArgs(MouseButtons.None, 0, -1, 0, 0);
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Global.BusyForm busyForm = new Global.BusyForm("Control Panel");
        private List<Task> tasks = new List<Task>();
        private int graphTimeSpan = Global.Config.Gadget.GraphTimeSpan;
        private Chart hoveredGraph = null;
        private Dictionary<NetworkInterface, EventHandler<TextEventArgs>> publicIPv4Subscriptions = new Dictionary<NetworkInterface, EventHandler<TextEventArgs>>();
        private Dictionary<NetworkInterface, EventHandler<TextEventArgs>> publicIPv6Subscriptions = new Dictionary<NetworkInterface, EventHandler<TextEventArgs>>();
        private List<EventHandler<TextEventArgs>> internetInterfaceSubscriptions = new List<EventHandler<TextEventArgs>>();

        public ControlPanelForm()
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
            int interfaceHeight = 260;
            int interfaceWidth = 750;
            // resize
            Size minimumSize = new Size(interfaceWidth + 26, (interfaceHeight + 6) * Global.NetworkInterfaces.Count + 78);
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
            
            // populate
            int i = 0;
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
            {
                GroupBox groupBox = new GroupBox();
                groupBox.Name = "interface" + nic.Guid;
                groupBox.Tag = nic.Guid;
                groupBox.Width = interfaceWidth;
                groupBox.Height = interfaceHeight;
                groupBox.Location = new Point(13, 68 + interfaceHeight * i);
                groupBox.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right);
                groupBox.MinimumSize = new System.Drawing.Size(interfaceWidth, interfaceHeight);
                groupBox.Controls.Add(CreateLabel(nic.Name, 10, 15, 340, true, "name"));
                groupBox.Controls["name"].Anchor = AnchorStyles.Left;
                if (!Global.Config.Gadget.HiddenInterfaces.Contains(nic.Guid))
                    groupBox.Controls["name"].Font = new System.Drawing.Font(DefaultFont, FontStyle.Bold);
                if (Global.InternetInterface != null)
                    if (nic.Guid == Global.InternetInterface)
                    {
                        groupBox.Controls["name"].ForeColor = Color.Blue;
                        toolTip1.SetToolTip(groupBox.Controls["name"], "Internet (remote network connections) goes through this interface");
                    }
                groupBox.Controls.Add(CreateListView("ipProperties"));
                Button button = new Button();
                button.Name = "interfaceTools";
                button.Text = "Interface tools";
                button.Location = new Point(360, 15);
                button.Anchor = AnchorStyles.Right;
                button.Width = 100;
                button.Height = 20;
                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                ToolStripItem toolStripItem;
                toolStripItem = contextMenuStrip.Items.Add("Configure");
                toolStripItem.Click += new EventHandler((s, e) => { new ConfigureInterface.ConfigureInterfaceForm(nic.Guid); });
                toolStripItem = contextMenuStrip.Items.Add("Make primary");
                toolStripItem.Click += new EventHandler((s, e) => {
                    DialogResult result = MessageBox.Show(
                        "This will configure all network interfaces so that \"" + nic.Name + "\" has the smallest metrics values of all, which will cause remote connectios to go through this interface when there are multiple NICs with a default gateway.\n\nDo you want to continue ?",
                        "Make an interface primary",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        MakeInterfacePrimary(nic.Guid); 
                });
                toolStripItem.MouseHover += new EventHandler((s, e) =>
                {
                    new BalloonTip(
                        "Make an interface primary",
                        "This will configure all network interfaces so that \"" + nic.Name + "\" has the smallest metrics values of all, which will cause remote connectios to go through this interface when there are multiple NICs with a default gateway."
                        , button, BalloonTip.ICON.INFO, 100000, false, false,
                        (short)(button.PointToScreen(Point.Empty).X + button.Width / 2),
                        (short)(button.PointToScreen(Point.Empty).Y + button.Height * 2.5));
                });
                toolStripItem.MouseLeave += new EventHandler((s, e) => { BalloonTip.CloseAll(); });

                // TODO: add speed, lattency test and network map
                //toolStripItem = contextMenuStrip.Items.Add("Test performance");
                //toolStripItem.Click += new EventHandler((s,e) => new InterfacePerformance.InterfacePerformanceForm(nic));
                //contextMenuStrip.Items.Add("Test lattency").Enabled = false;
                button.Click += new EventHandler((s, e) => { contextMenuStrip.Show((Control)s, 0, ((Control)s).Height); });
                groupBox.Controls.Add(button);
                groupBox.Controls.Add(CreateLabel("Adapter GUID:", 360, 40, 120));
                groupBox.Controls.Add(CreateLabel("Description:", 360, 55, 120));
                groupBox.Controls.Add(CreateLabel("Type:", 360, 70, 120));
                groupBox.Controls.Add(CreateLabel("Interface Index:", 360, 85, 120));
                groupBox.Controls.Add(CreateLabel("MAC Address", 360, 100, 120));
                groupBox.Controls.Add(CreateLabel("Interface Metric:", 360, 115, 120));
                groupBox.Controls.Add(CreateLabel("Lowest IPv4 metrics", 360, 145, 120, false, "lowestMetrics"));
                groupBox.Controls["lowestMetrics"].Font = new System.Drawing.Font(DefaultFont, FontStyle.Underline);
                groupBox.Controls.Add(CreateLabel("Gateway Metric:", 360, 160, 120));
                groupBox.Controls.Add(CreateLabel("Route Metric:", 360, 175, 120));
                groupBox.Controls.Add(CreateLabel("Public IPv4:", 360, 220, 120));
                groupBox.Controls.Add(CreateLabel("Public IPv6:", 360, 235, 120));
                groupBox.Controls.Add(CreateLabel(nic.Guid, 480, 40, 265, true));
                groupBox.Controls.Add(CreateLabel(nic.Description, 480, 55, 265, true));
                groupBox.Controls.Add(CreateLabel(nic.Type.GetDescription(), 480, 70, 265, true));
                groupBox.Controls.Add(CreateLabel(nic.Index.ToString(), 480, 85, 120, true));
                groupBox.Controls.Add(CreateLabel(nic.Mac, 480, 100, 120, true));
                groupBox.Controls.Add(CreateLabel(nic.InterfaceMetric.ToString(), 480, 115, 120, true));
                groupBox.Controls.Add(CreateLabel(nic.IPv4Gateway.Count != 0 ? nic.IPv4Gateway.Min(x => x.GatewayMetric).ToString() : "", 480, 160, 120, true));
                groupBox.Controls.Add(CreateLabel(nic.IPv4Gateway.Count != 0 ? nic.IPv4Gateway.Min(x => x.GatewayMetric + nic.InterfaceMetric).ToString() : "", 480, 175, 120, true));
                groupBox.Controls.Add(CreateLabel(nic.PublicIPv4, 480, 220, 265, true, "publicIPv4"));
                groupBox.Controls.Add(CreateLabel(nic.PublicIPv6, 480, 235, 265, true, "publicIPv6"));
                groupBox.Controls.Add(CreateLabel("Download:", 600, 85, 65, false, "", Color.FromArgb(0x9b, 0x87, 0x0c)));
                groupBox.Controls.Add(CreateLabel("0 B/s", 600, 100, 65, false, "downByte", Color.FromArgb(0x9b, 0x87, 0x0c)));
                groupBox.Controls.Add(CreateLabel("0 b/s", 600, 115, 65, false, "downBit", Color.FromArgb(0x9b, 0x87, 0x0c)));
                groupBox.Controls.Add(CreateLabel(Unit.AutoScale(nic.IPv4BytesReceived, "B"), 600, 205, 65, false, "totalDown", Color.FromArgb(0x9b, 0x87, 0x0c)));
                groupBox.Controls.Add(CreateLabel("Upload:", 665, 85, 65, false, "", Color.Green, ContentAlignment.MiddleRight));
                groupBox.Controls.Add(CreateLabel("0 B/s", 665, 100, 65, false, "upByte", Color.Green, ContentAlignment.MiddleRight));
                groupBox.Controls.Add(CreateLabel("0 b/s", 665, 115, 65, false, "upBit", Color.Green, ContentAlignment.MiddleRight));
                groupBox.Controls.Add(CreateLabel(Unit.AutoScale(nic.IPv4BytesSent, "B"), 665, 205, 65, false, "totalUp", Color.Green, ContentAlignment.MiddleRight));
                groupBox.Controls.Add(CreateLabel("Total:", 650, 190, 65));
                groupBox.Controls.Add(CreateGraph("graph", 600, 145));
                // interface event subscriptions
                EventHandler<TextEventArgs> handler = new EventHandler<TextEventArgs>((s, e) =>
                {
                    try
                    {
                        Invoke(new Action(() =>
                        {
                            if (nic.Guid == ((WinLib.Network.NetworkInterface)s).Guid)
                                groupBox.Controls["publicIPv4"].Text = e.Text;
                        }));
                    }
                    catch { }
                });
                nic.PublicIPv4Changed += handler;
                publicIPv4Subscriptions.Add(nic, handler);

                handler = new EventHandler<TextEventArgs>((s, e) =>
                {
                    try
                    {
                        Invoke(new Action(() =>
                        {
                            if (nic.Guid == ((WinLib.Network.NetworkInterface)s).Guid)
                                groupBox.Controls["publicIPv6"].Text = e.Text;
                        }));
                    }
                    catch { }
                });
                nic.PublicIPv6Changed += handler;
                publicIPv6Subscriptions.Add(nic, handler);

                handler = new EventHandler<TextEventArgs>((s, e) =>
                {
                    try
                    {
                        Invoke(new Action(() =>
                        {
                            if (nic.IPv4Address.Any(a => a.Address == e.Text))
                            {
                                groupBox.Controls["name"].ForeColor = Color.Aqua;
                                toolTip1.SetToolTip(groupBox.Controls["name"], "Internet (remote network connections) goes through this interface");
                                Global.InternetInterface = nic.Guid;
                            }
                            else
                            {
                                groupBox.Controls["name"].ForeColor = Color.Black;
                                toolTip1.SetToolTip(groupBox.Controls["name"], "");
                            }
                        }));
                    }
                    catch { }
                });
                WinLib.Network.NetworkInterface.InternetInterfaceChanged += handler;
                internetInterfaceSubscriptions.Add(handler);

                Controls.Add(groupBox);
                i++;
            }
            ClientSize = new Size(ClientSize.Width + 70, ClientSize.Height);
            Location = new Point(workingArea.Left + workingArea.Width / 2 - Width / 2, workingArea.Top + workingArea.Height / 2 - Height / 2);
            Show();
        }

        /// <summary>
        /// HACK: metrics are sometimes ignored or the effect is delayed
        /// </summary>
        /// <param name="guid"></param>
        void MakeInterfacePrimary(string guid)
        {
            LoadingForm splash = new LoadingForm("Retrieving interfaces ...");
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
            {
                splash.UpdateStatus("Configuring interface \"" + nic.Name + "\" ...");
                if (nic.Guid == guid)
                {
                    nic.SetInterfaceMetric("1");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                        nic.EditIPv4Gateway(ip.Address, "1");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                        nic.EditIPv4Gateway(ip.Address, "1");
                    // on Windows 8.1 occasionaly breaks routing until restart or ??? internal Windows stuff
                    //foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                    //    Iphlpapi.EditRoute("0.0.0.0", "0.0.0.0", ip.Address, nic.Index.ToString(), "0");
                    //foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                    //    Iphlpapi.EditRoute("::", "0", ip.Address, nic.Index.ToString(), "0");
                }
                else
                {
                    nic.SetInterfaceMetric("4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                        nic.EditIPv4Gateway(ip.Address, "4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                        nic.EditIPv4Gateway(ip.Address, "4000");
                    // on Windows 8.1 occasionaly breaks routing until restart or ??? internal Windows stuff
                    //foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                    //    Iphlpapi.EditRoute("0.0.0.0", "0.0.0.0", ip.Address, nic.Index.ToString(), "4000");
                    //foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                    //    Iphlpapi.EditRoute("::", "0", ip.Address, nic.Index.ToString(), "4000");
                }
            }
            splash.Stop();
            Program.Refresh();
        }

        Label CreateLabel(string text, int x, int y, int width, bool showToolTip = false, string name = "", Color? color = null, ContentAlignment align = ContentAlignment.MiddleLeft)
        {
            Label label = new Label();
            label.Name = name;
            label.Text = text == null ? "" : text;
            label.Location = new Point(x, y);
            label.Width = width;
            label.Height = 15;
            label.Anchor = AnchorStyles.Right;
            label.ForeColor = color ?? Color.Black;
            label.TextAlign = align;
            label.Click += CopyToClipboard;
            if (showToolTip)
            {
                toolTip1.SetToolTip(label, "Click to copy to clipboard");
                label.Cursor = Cursors.Hand;
            }
            return label;
        }

        void CopyToClipboard(object sender, EventArgs e)
        {
            Clipboard.SetText(((Control)sender).Text!=""?((Control)sender).Text:"N/A");
        }

        ListView CreateListView(string name)
        {
            ListView listView = new ListView();
            listView.Name = name;
            listView.View = View.Details;
            listView.Location = new Point(10, 40);
            listView.Width = 340;
            listView.Height = 210;
            listView.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
            listView.MinimumSize = new System.Drawing.Size(340, 180);
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView.FullRowSelect = true;
            listView.ShowItemToolTips = true;
            listView.ColumnWidthChanging += listView_ColumnWidthChanging;
            //listView.ItemSelectionChanged += listView_ItemSelectionChanged;
            //listView.MouseMove += listView_MouseMove;
            listView.MouseClick += listView_MouseClick;
            listView.Columns.Add("Property").Width = 150;
            listView.Columns.Add("Value").Width = 150;
            listView.Columns.Add("Value").Width = 150;
            return listView;
        }

        void listView_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            
        }

        void listView_MouseClick(object sender, MouseEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItems[0].SubItems.Count > 0 && e.X < listView.Columns[0].Width)
                Clipboard.SetText(listView.SelectedItems[0].SubItems[0].Text != "" ? listView.SelectedItems[0].SubItems[0].Text : "N/A");
            else if (listView.SelectedItems[0].SubItems.Count > 1 && e.X < (listView.Columns[0].Width + listView.Columns[1].Width))
                Clipboard.SetText(listView.SelectedItems[0].SubItems[1].Text != "" ? listView.SelectedItems[0].SubItems[1].Text : "N/A");
            else if (listView.SelectedItems[0].SubItems.Count > 2 && e.X < (listView.Columns[0].Width + listView.Columns[1].Width + listView.Columns[2].Width))
                Clipboard.SetText(listView.SelectedItems[0].SubItems[2].Text != "" ? listView.SelectedItems[0].SubItems[2].Text : "N/A");
        }

        void listView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = ((ListView)sender).Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        Chart CreateGraph(string name, int x, int y)
        {
            Chart chart = new Chart();
            ChartArea chartArea = new ChartArea();
            Series seriesDown = new Series();
            Series seriesUp = new Series();

            chartArea.BackColor = Color.Gray;
            chartArea.BorderWidth = 0;
            chartArea.Position.Auto = false;
            chartArea.Position.Width = 100;
            chartArea.Position.Height = 100;
            chartArea.Position.X = 0;
            chartArea.Position.Y = 0;

            chartArea.AxisX.ArrowStyle = AxisArrowStyle.None;
            chartArea.AxisX.IsMarginVisible = false;
            chartArea.AxisX.MinorGrid.Enabled = false;
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisX.MajorTickMark.Enabled = false;
            chartArea.AxisX.MinorTickMark.Enabled = false;
            chartArea.AxisX.LabelStyle.Enabled = false;
            chartArea.AxisX.LineWidth = 0;

            chartArea.AxisY.ArrowStyle = AxisArrowStyle.None;
            chartArea.AxisY.IsMarginVisible = false;
            chartArea.AxisY.MinorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorTickMark.Enabled = false;
            chartArea.AxisY.MinorTickMark.Enabled = false;
            chartArea.AxisY.LabelStyle.Enabled = false;
            chartArea.AxisY.LineWidth = 0;

            seriesDown.ChartType = SeriesChartType.Line;
            seriesDown.Color = Color.Yellow;
            seriesDown.IsValueShownAsLabel = false;
            seriesDown.IsVisibleInLegend = false;
            for (int i = 0; i < graphTimeSpan; i++)
                seriesDown.Points.AddY(0);

            seriesUp.ChartType = SeriesChartType.Line;
            seriesUp.Color = Color.Lime;
            seriesUp.IsValueShownAsLabel = false;
            seriesUp.IsVisibleInLegend = false;
            for (int i = 0; i < graphTimeSpan; i++)
                seriesUp.Points.AddY(0);

            chart.Series.Add(seriesDown);
            chart.Series.Add(seriesUp);
            chart.ChartAreas.Add(chartArea);
            chart.Name = name;
            chart.Location = new Point(x, y);
            chart.Size = new Size(140, 30);
            chart.Margin = new Padding(0);
            chart.Anchor = AnchorStyles.Right;
            chart.AntiAliasing = AntiAliasingStyles.None;
            chart.MouseEnter += new EventHandler((s, e) => { hoveredGraph = (Chart)s; });
            chart.MouseLeave += new EventHandler((s, e) => { hoveredGraph = null; toolTip1.Hide((Control)s); });
            chart.MouseMove += new MouseEventHandler(Graph_MouseMove);
            return chart;
        }

        void Graph_MouseMove(object sender, MouseEventArgs e)
        {
            if ((this.lastMouseEvent.X != e.X) && (e.X < ((Chart)sender).Width))
            {
                ChartArea chartArea = ((Chart)sender).ChartAreas[0];
                int xValue = e.X * ((Chart)sender).Series[0].Points.Count / ((Chart)sender).Width;
                Int64 dw = (Int64)((Chart)sender).Series[0].Points[xValue].YValues[0];
                Int64 up = (Int64)((Chart)sender).Series[1].Points[xValue].YValues[0];
                toolTip1.Show("D:" + Unit.AutoScale(dw, "B/s") + "\nU:" + Unit.AutoScale(up, "B/s"), (Control)sender, e.X + 15, e.Y + 15);
            }
            lastMouseEvent = e;
        }

        async void UpdateTraffic(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int i = 0;
                    foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                    {
                        Controls["interface" + nic.Guid].Controls["downByte"].Text = Unit.AutoScale(nic.IPv4InSpeed, "B");
                        Controls["interface" + nic.Guid].Controls["downBit"].Text = Unit.AutoScale(nic.IPv4InSpeed * 8, "b");
                        Controls["interface" + nic.Guid].Controls["upByte"].Text = Unit.AutoScale(nic.IPv4OutSpeed, "B");
                        Controls["interface" + nic.Guid].Controls["upBit"].Text = Unit.AutoScale(nic.IPv4OutSpeed * 8, "b");
                        Controls["interface" + nic.Guid].Controls["totalDown"].Text = Unit.AutoScale(nic.IPv4BytesReceived, "B");
                        Controls["interface" + nic.Guid].Controls["totalUp"].Text = Unit.AutoScale(nic.IPv4BytesSent, "B");
                        Chart chart = ((Chart)Controls["interface" + nic.Guid].Controls["graph"]);
                        chart.Series[0].Points.RemoveAt(0);
                        chart.Series[0].Points.AddY(nic.IPv4InSpeed);
                        chart.Series[1].Points.RemoveAt(0);
                        chart.Series[1].Points.AddY(nic.IPv4OutSpeed);
                        chart.ChartAreas[0].RecalculateAxesScale();

                        toolTip1.SetToolTip(Controls["interface" + nic.Guid].Controls["downByte"], "Avg:" + Unit.AutoScale(nic.IPv4InSpeedAvg20, "B/s"));
                        toolTip1.SetToolTip(Controls["interface" + nic.Guid].Controls["downBit"], "Avg:" + Unit.AutoScale(nic.IPv4InSpeedAvg20 * 8, "b/s"));
                        toolTip1.SetToolTip(Controls["interface" + nic.Guid].Controls["upByte"], "Avg:" + Unit.AutoScale(nic.IPv4OutSpeedAvg20, "B/s"));
                        toolTip1.SetToolTip(Controls["interface" + nic.Guid].Controls["upBit"], "Avg:" + Unit.AutoScale(nic.IPv4OutSpeedAvg20 * 8, "b/s"));

                        if (hoveredGraph == chart)
                        {
                            int xValue = lastMouseEvent.X * chart.Series[0].Points.Count / chart.Width;
                            Int64 dw = (Int64)chart.Series[0].Points[xValue].YValues[0];
                            Int64 up = (Int64)chart.Series[1].Points[xValue].YValues[0];
                            this.toolTip1.Show("D:" + Unit.AutoScale(dw, "B") + "\nU:" + Unit.AutoScale(up, "B"), (Control)chart, lastMouseEvent.X + 15, lastMouseEvent.Y + 15);
                        }
                        i++;
                    }
                    await TaskEx.Delay(1000);
                }
            }
            catch (Exception) { }
        }

        private void ControlPanelForm_Load(object sender, EventArgs e)
        {
            UpdateTraffic(cts.Token);
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}

        private void ControlPanelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (KeyValuePair<NetworkInterface, EventHandler<TextEventArgs>> subscription in publicIPv4Subscriptions)
                subscription.Key.PublicIPv4Changed -= subscription.Value;
            foreach (KeyValuePair<NetworkInterface, EventHandler<TextEventArgs>> subscription in publicIPv6Subscriptions)
                subscription.Key.PublicIPv6Changed -= subscription.Value;
            foreach (EventHandler<TextEventArgs> subscription in internetInterfaceSubscriptions)
                NetworkInterface.InternetInterfaceChanged -= subscription;
            cts.Cancel();
            Instance = null;
            busyForm.Done.SetResult(true);
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                {
                    ListView listView = (ListView)Controls["interface" + nic.Guid].Controls["ipProperties"];
                    listView.Clear();
                    listView.Columns.Add("Property").Width = 150;
                    listView.Columns.Add("Value").Width = 150;
                    listView.Columns.Add("Value").Width = 150;
                    for (int i = 0; i < nic.IPv4Address.Count; i++)
                        listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Address IP & Mask" : "", nic.IPv4Address[i].Address, nic.IPv4Address[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                    for (int i = 0; i < nic.IPv4Gateway.Count; i++)
                        listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Gateway IP & Metric" : "", nic.IPv4Gateway[i].Address, nic.IPv4Gateway[i].GatewayMetric.ToString() })).ToolTipText = "Click to copy to clipboard";
                    for (int i = 0; i < nic.IPv4DnsServer.Count; i++)
                        listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "DNS Server" : "", nic.IPv4DnsServer[i] })).ToolTipText = "Click to copy to clipboard";
                    listView.Items.Add(new ListViewItem(new string[] { "DHCP Enabled", nic.DhcpEnabledString })).ToolTipText = "Click to copy to clipboard";
                    if (nic.DhcpServer != null && nic.DhcpServer != "255.255.255.255")
                        listView.Items.Add(new ListViewItem(new string[] { "DHCP Server", nic.DhcpServer })).ToolTipText = "Click to copy to clipboard";
                    listView.Items.Add(new ListViewItem(new string[] { "NetBIOS over TCP/IP", nic.NetbiosEnabledString })).ToolTipText = "Click to copy to clipboard";
                    listView.Items.Add(new ListViewItem(new string[] { "MTU", nic.IPv4Mtu })).ToolTipText = "Click to copy to clipboard";
                    //listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    foreach (ColumnHeader column in listView.Columns)
                        column.Width = -2;
                }
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
            {
                ListView listView = (ListView)Controls["interface" + nic.Guid].Controls["ipProperties"];
                listView.Clear();
                listView.Columns.Add("Property").Width = 150;
                listView.Columns.Add("Value").Width = 150;
                listView.Columns.Add("Value").Width = 150;
                for (int i = 0; i < nic.IPv6Address.Global.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Global IP & Mask" : "", nic.IPv6Address.Global[i].Address, nic.IPv6Address.Global[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Address.Temporary.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Temporary IP & Mask" : "", nic.IPv6Address.Temporary[i].Address, nic.IPv6Address.Temporary[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Address.LinkLocal.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Link-Local IP & Mask" : "", nic.IPv6Address.LinkLocal[i].Address, nic.IPv6Address.LinkLocal[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Address.SiteLocal.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Site-Local IP & Mask" : "", nic.IPv6Address.SiteLocal[i].Address, nic.IPv6Address.SiteLocal[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Address.UniqueLocal.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Unique-Local IP & Mask" : "", nic.IPv6Address.UniqueLocal[i].Address, nic.IPv6Address.UniqueLocal[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Address.Local.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Local IP & Mask" : "", nic.IPv6Address.Local[i].Address, nic.IPv6Address.Local[i].Subnet })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6Gateway.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "Gateway IP & Metric" : "", nic.IPv6Gateway[i].Address, nic.IPv6Gateway[i].GatewayMetric.ToString() })).ToolTipText = "Click to copy to clipboard";
                for (int i = 0; i < nic.IPv6DnsServer.Count; i++)
                    listView.Items.Add(new ListViewItem(new string[] { i == 0 ? "DNS Server" : "", nic.IPv6DnsServer[i] })).ToolTipText = "Click to copy to clipboard";
                if (nic.Dhcpv6Server != null)
                    listView.Items.Add(new ListViewItem(new string[] { "DHCPv6 Server", nic.Dhcpv6Server })).ToolTipText = "Click to copy to clipboard";
                listView.Items.Add(new ListViewItem(new string[] { "Router Discovery", nic.IPv6RouterDiscoveryEnabled ? "Enabled" : "Disabled" })).ToolTipText = "Click to copy to clipboard";
                listView.Items.Add(new ListViewItem(new string[] { "MTU", nic.IPv6Mtu })).ToolTipText = "Click to copy to clipboard";
                //listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                foreach (ColumnHeader column in listView.Columns)
                    column.Width = -2;
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.VersionInfo.CheckForUpdates(true);
        }

        private void routesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Routes.RoutesForm();
        }

        private void ipSessionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new IPSessions.IPSessionsForm();
        }

        private void gadgetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings.SettingsForm();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About.AboutForm();
        }

        private void loadBalancingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LoadBalancing.LoadBalancingForm();
        }

        private void iPv4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
                "This will remove any existing TCP/IPv4 configurations and restore the default ones for all network interfaces. This can be useful when you have broken TCP/IPv4 configurations that cause you network issues like DHCP failure, routing errors and even BSODs.\nIf you currently have static cofingurations for any of your interfaces that you wish to restore afterwards, you should save them before doing this either through the interface configuration menu or simply by writing them down.\nThis is the same as manually running \"netsh int ip reset\" in command prompt.\n\nAre you sure you want to continue?",
                "Reset TCP/IPv4", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == System.Windows.Forms.DialogResult.No)
                return;
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "netsh.exe";
            process.StartInfo.Arguments = "int ip reset \"" + Environment.GetEnvironmentVariable("temp") + "\"";
            process.Start();
            process.WaitForExit();
            string stdo = process.StandardOutput.ReadToEnd();
            if (Regex.IsMatch(stdo, @"^\s*$"))
                stdo = "Resetting , OK!\nRestart the computer to complete this action.";
            MessageBox.Show(this,
                "The command result is:\n" + stdo,
                "TCP/IPv4 Reset Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void iPv6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
                "This will remove any existing TCP/IPv6 configurations and restore the default ones for all network interfaces. This can be useful when you have broken TCP/IPv6 configurations that cause you network issues like DHCPv6 failure, routing errors and even BSODs.\nIf you currently have static cofingurations for any of your interfaces that you wish to restore afterwards, you should save them before doing this either through the interface configuration menu or simply by writing them down.\nThis is the same as manually running \"netsh int ipv6 reset\" in command prompt.\n\nAre you sure you want to continue?",
                "Reset TCP/IPv6", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == System.Windows.Forms.DialogResult.No)
                return;
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "netsh.exe";
            process.StartInfo.Arguments = "int ip reset \"" + Environment.GetEnvironmentVariable("temp") + "\"";
            process.Start();
            process.WaitForExit();
            string stdo = process.StandardOutput.ReadToEnd();
            if (Regex.IsMatch(stdo, @"^\s*$"))
                stdo = "Resetting , OK!\nRestart the computer to complete this action.";
            MessageBox.Show(this,
                "The command result is:\n" + stdo,
                "TCP/IPv6 Reset Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
