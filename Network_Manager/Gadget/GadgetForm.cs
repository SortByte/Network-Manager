using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Lib.Network;
using Lib.WinAPI;
using Lib.Sync;

namespace Network_Manager.Gadget
{
    public partial class GadgetForm : Form
    {
        public static Form Instance = null;
        public ContextMenuStrip MainContextMenu;
        public static volatile bool AutoRefreshAllowed = true;
        public static volatile bool RefreshInProgress = false;
        private int requiredSlots = Global.NetworkInterfaces.Count;
        private MouseEventArgs lastMouseEvent = new MouseEventArgs(MouseButtons.None, 0, -1, 0, 0);
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();
        private int maxVerticalSlots = Global.Config.Gadget.MaxVerticalSlots;
        private int maxHorizontalSlots = Global.Config.Gadget.MaxHorizontalSlots;
        private int graphTimeSpan = Global.Config.Gadget.GraphTimeSpan;
        private List<string> hiddenInterfaces = Global.Config.Gadget.HiddenInterfaces;
        private Chart hoveredGraph = null;
        private Point startMovePoint;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private Dictionary<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>> publicIPv4Subscriptions = new Dictionary<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>>();
        private Dictionary<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>> publicIPv6Subscriptions = new Dictionary<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>>();
        private List<EventHandler<TextEventArgs>> internetInterfaceSubscriptions = new List<EventHandler<TextEventArgs>>();

        public GadgetForm()
        {
            if (GadgetForm.Instance != null)
            {
                Instance.WindowState = FormWindowState.Normal;
                Instance.Activate();
                return;
            }
            GadgetForm.Instance = this;
            InitializeComponent();
            SuspendLayout();
            CheckLocation(Global.Config.Gadget.Location);
            TopMost = Global.Config.Gadget.AlwaysOnTop;
            int interfaceHeight = 180;
            Image midBkgImage;
            Image botBkgImage;
            MainContextMenu = new ContextMenuStrip();
            ((ToolStripMenuItem)MainContextMenu.Items.Add("Always on top", null, Global.AlwaysOnTop)).Checked = Global.Config.Gadget.AlwaysOnTop;
            MainContextMenu.Items.Add("Check for updates", null, new EventHandler((s, e) => { Global.VersionInfo.CheckForUpdates(true); }));
            MainContextMenu.Items.Add("Settings", null, new EventHandler((s, e) => { new Settings.SettingsForm(); }));
            MainContextMenu.Items.Add("About", null, new EventHandler((s, e) => { new About.AboutForm(); }));

            foreach (string ifGuid in hiddenInterfaces)
                if (Global.NetworkInterfaces.ContainsKey(ifGuid))
                    requiredSlots--;
            if ((requiredSlots > maxVerticalSlots) && (maxHorizontalSlots > 1))
            {
                this.Width = 372;
                AddImage(global::Network_Manager.Properties.Resources.button_close_on, "closeButtonHover", 351, 2);
                AddImage(global::Network_Manager.Properties.Resources.titlebar_wide, "titlebar", 0, 0);
                AddImage(global::Network_Manager.Properties.Resources.background_top_wide, "topBkg", 0, 25);
                midBkgImage = global::Network_Manager.Properties.Resources.background_middle_wide;
                botBkgImage = global::Network_Manager.Properties.Resources.background_bottom_wide;
            }
            else
            {
                this.Width = 186;
                AddImage(global::Network_Manager.Properties.Resources.button_close_on, "closeButtonHover", 165, 2);
                AddImage(global::Network_Manager.Properties.Resources.titlebar, "titlebar", 0, 0);
                AddImage(global::Network_Manager.Properties.Resources.background_top, "topBkg", 0, 25);
                midBkgImage = global::Network_Manager.Properties.Resources.background_middle;
                botBkgImage = global::Network_Manager.Properties.Resources.background_bottom;
            }
            Controls["closeButtonHover"].Visible = false;
            Controls["closeButtonHover"].Click += new EventHandler((s, e) => { Global.Exit(); });
            Controls["titlebar"].MouseMove += Titlebar_MouseMove;
            Controls["titlebar"].MouseDown += GadgetForm_MouseDown;
            Controls["titlebar"].MouseUp += Titlebar_MouseUp;
            if (requiredSlots == 0)
            {
                if (Global.NetworkInterfaces.Count == 0)
                {
                    Height = 45+30+40;
                    AddImage(midBkgImage, "midBkg", 0, 25+30);
                    AddImage(botBkgImage, "botBkg", 0, 25+60);
                    AddLabel("noInterfaceMsg", "All interfaces are disconnected!", 23, 45, 145);
                    ((Label)Controls["noInterfaceMsg"]).Height = 30;
                    Controls["midBkg"].ContextMenuStrip = MainContextMenu;
                }
                else
                {
                    Height = 45+60+40;
                    AddImage(midBkgImage, "midBkg0", 0, 25+30);
                    AddImage(midBkgImage, "midBkg1", 0, 25+60);
                    AddImage(botBkgImage, "botBkg", 0, 25+90);
                    AddLabel("noInterfaceMsg", "All interfaces are hidden or disconnected.\nCheck them in the settings menu or refresh.", 23, 45, 145);
                    ((Label)Controls["noInterfaceMsg"]).Height = 60;
                    Controls["midBkg0"].ContextMenuStrip = MainContextMenu;
                    Controls["midBkg1"].ContextMenuStrip = MainContextMenu;
                }
            }
            else
            {
                Height = 45 + (requiredSlots < maxVerticalSlots ? requiredSlots : maxVerticalSlots) * interfaceHeight + 40;
                int xOffset = 0;
                int i = 0;
                int y = 0;
                foreach(Lib.Network.NetworkInterface nic in Global.NetworkInterfaces.Values)
                {
                    if (hiddenInterfaces.Contains(nic.Guid))
                        continue;
                    if (i >= (maxVerticalSlots*maxHorizontalSlots))
                        break;
                    if (i == maxVerticalSlots)
                    {
                        xOffset = 186;
                        y = 0;
                    }
                    else
                        for (int j=0; j<6; j++)
                        {
                            AddImage(midBkgImage, "midBkg" + i.ToString() + j.ToString(), 0, 25 + 30 * (6 * i + j + 1));
                            Controls["midBkg" + i.ToString() + j.ToString()].ContextMenuStrip = MainContextMenu;
                        }
                    AddGroupBox("interface" + nic.Guid, xOffset+13, 38 + y * interfaceHeight);
                    AddLabel("interfaceName" + nic.Guid, nic.Name, xOffset + 23, 38 + 15 + y * interfaceHeight, 145);
                    AddLabel("localLabel" + nic.Guid, "Local IP:", xOffset + 23, 38 + 30 + y * interfaceHeight, 55);
                    AddLabel("localIP" + nic.Guid, nic.IPv4Address.Count > 0 ? (nic.LocalIPv4Exit ?? nic.IPv4Address[0].Address) : "N/A", xOffset + 23 + 55, 38 + 30 + y * interfaceHeight, 90);
                    AddLabel("publicLabel" + nic.Guid, "Public IP:", xOffset + 23, 38 + 45 + y * interfaceHeight, 55);
                    AddLabel("publicIP" + nic.Guid, "Detecting ...", xOffset + 23 + 55, 38 + 45 + y * interfaceHeight, 90);
                    AddLabel("downLabel" + nic.Guid, "Download:", xOffset + 23, 38 + 60 + y * interfaceHeight, 70, Color.Yellow);
                    AddLabel("downByte" + nic.Guid, "0 B/s", xOffset + 23, 38 + 75 + y * interfaceHeight, 70, Color.Yellow);
                    AddLabel("downBit" + nic.Guid, "0 b/s", xOffset + 23, 38 + 90 + y * interfaceHeight, 70, Color.Yellow);
                    AddLabel("upLabel" + nic.Guid, "Upload:", xOffset + 23 + 75, 38 + 60 + y * interfaceHeight, 70, Color.Lime, ContentAlignment.MiddleRight);
                    AddLabel("upByte" + nic.Guid, "0 B/s", xOffset + 23 + 75, 38 + 75 + y * interfaceHeight, 70, Color.Lime, ContentAlignment.MiddleRight);
                    AddLabel("upBit" + nic.Guid, "0 b/s", xOffset + 23 + 75, 38 + 90 + y * interfaceHeight, 70, Color.Lime, ContentAlignment.MiddleRight);
                    AddGraph("graph" + nic.Guid, xOffset+23, 38 + 110 + y * interfaceHeight);
                    AddLabel("totalLabel" + nic.Guid, "Total:", xOffset + 23 + 55, 38 + 150 + y * interfaceHeight, 50);
                    AddLabel("totalDown" + nic.Guid, Unit.AutoScale(nic.IPv4BytesReceived, "B"), xOffset + 23, 38 + 160 + y * interfaceHeight, 70, Color.Yellow);
                    AddLabel("totalUp" + nic.Guid, Unit.AutoScale(nic.IPv4BytesSent, "B"), xOffset + 23 + 75, 38 + 160 + y * interfaceHeight, 70, Color.Lime, ContentAlignment.MiddleRight);
                    toolTip.SetToolTip(Controls["localIP" + nic.Guid], "Click to copy to clipboard");
                    toolTip.SetToolTip(Controls["publicIP" + nic.Guid], "Click to copy to clipboard");
                    toolTip.SetToolTip(Controls["downByte" + nic.Guid], "Avg:" + Unit.AutoScale(nic.IPv4InSpeedAvg20, "B/s"));
                    toolTip.SetToolTip(Controls["downBit" + nic.Guid], "Avg:" + Unit.AutoScale(nic.IPv4InSpeedAvg20 * 8, "b/s"));
                    toolTip.SetToolTip(Controls["upByte" + nic.Guid], "Avg:" + Unit.AutoScale(nic.IPv4OutSpeedAvg20, "B/s"));
                    toolTip.SetToolTip(Controls["upBit" + nic.Guid], "Avg:" + Unit.AutoScale(nic.IPv4OutSpeedAvg20 * 8, "b/s"));
                    Controls["localIP" + nic.Guid].Cursor = Cursors.Hand;
                    Controls["localIP" + nic.Guid].Click += new EventHandler((s, e) => { Clipboard.SetText(((Control)s).Text != "" ? ((Control)s).Text : "N/A"); });
                    Controls["publicIP" + nic.Guid].Cursor = Cursors.Hand;
                    Controls["publicIP" + nic.Guid].Click += new EventHandler((s, e) => { Clipboard.SetText(((Control)s).Text != "" ? ((Control)s).Text : "N/A"); });
                    Controls["interface" + nic.Guid].ContextMenuStrip = MainContextMenu;
                    i++;
                    y++;
                    // interface event subscriptions
                    EventHandler<TextEventArgs> handler = new EventHandler<TextEventArgs>((s, e) =>
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                if (nic.Guid == ((Lib.Network.NetworkInterface)s).Guid)
                                    Controls["publicIP" + nic.Guid].Text = e.Text;
                            }));
                        }
                        catch { }
                    });
                    nic.PublicIPv4Changed += handler;
                    publicIPv4Subscriptions.Add(nic, handler);

                    handler = (s, e) =>
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                if (nic.IPv4Address.Any(a => a.Address == e.Text))
                                {
                                    Controls["interfaceName" + nic.Guid].ForeColor = Color.Aqua;
                                    toolTip.SetToolTip(Controls["interfaceName" + nic.Guid], "Internet (remote network connections) goes through this interface");
                                    Global.InternetInterface = nic.Guid;
                                }
                                else
                                {
                                    Controls["interfaceName" + nic.Guid].ForeColor = Color.White;
                                    toolTip.SetToolTip(Controls["interfaceName" + nic.Guid], "");
                                }
                            }));
                        }
                        catch { }
                    };
                    Lib.Network.NetworkInterface.InternetInterfaceChanged += handler;
                    internetInterfaceSubscriptions.Add(handler);
                }
                AddImage(midBkgImage, "midBkgLast", 0, this.Height-60);
                AddImage(botBkgImage, "botBkg", 0, this.Height-30);
                Controls["midBkgLast"].ContextMenuStrip = MainContextMenu;
            }
            Controls["titleBar"].ContextMenuStrip = MainContextMenu;
            Controls["topBkg"].ContextMenuStrip = MainContextMenu;
            Controls["botBkg"].ContextMenuStrip = MainContextMenu;
            AddImage(global::Network_Manager.Properties.Resources.button_control_panel, "controlPanel", 15, this.Height - 40);
            AddImage(global::Network_Manager.Properties.Resources.button_refresh, "refresh", 45, this.Height - 40);
            AddImage(global::Network_Manager.Properties.Resources.button_settings, "settings", 75, this.Height - 40);
            AddImage(global::Network_Manager.Properties.Resources.button_update, "update", 145, this.Height - 40);
            Controls["controlPanel"].BringToFront();
            Controls["controlPanel"].MouseEnter += new EventHandler((s, e) => { this.Cursor = Cursors.Hand; });
            Controls["controlPanel"].MouseLeave += new EventHandler((s, e) => { this.Cursor = Cursors.Default; });
            Controls["controlPanel"].Cursor = Cursors.Hand;
            Controls["controlPanel"].Click += new EventHandler((s, e) => { new ControlPanel.ControlPanelForm(); });
            if (Global.NetworkInterfaces.Count == 0)
                this.Controls["controlPanel"].Visible = false;
            Controls["refresh"].BringToFront();
            Controls["refresh"].MouseEnter += new EventHandler((s, e) => { this.Cursor = Cursors.Hand; });
            Controls["refresh"].MouseLeave += new EventHandler((s, e) => { this.Cursor = Cursors.Default; });
            Controls["refresh"].Cursor = Cursors.Hand;
            Controls["refresh"].Click += new EventHandler((s, e) => { Refresh(); });
            Controls["settings"].BringToFront();
            Controls["settings"].MouseEnter += new EventHandler((s, e) => { this.Cursor = Cursors.Hand; });
            Controls["settings"].MouseLeave += new EventHandler((s, e) => { this.Cursor = Cursors.Default; });
            Controls["settings"].Cursor = Cursors.Hand;
            Controls["settings"].Click += new EventHandler((s, e) => { new Settings.SettingsForm(); });
            Controls["update"].BringToFront();
            Controls["update"].MouseEnter += new EventHandler((s, e) => { this.Cursor = Cursors.Hand; });
            Controls["update"].MouseLeave += new EventHandler((s, e) => { this.Cursor = Cursors.Default; });
            Controls["update"].Cursor = Cursors.Hand;
            Controls["update"].Click += Update_Click;
            Controls["update"].Visible = false;
            toolTip.SetToolTip(Controls["controlPanel"], "Control Panel");
            toolTip.SetToolTip(Controls["refresh"], "Refresh");
            toolTip.SetToolTip(Controls["settings"], "Settings");
            toolTip.SetToolTip(Controls["update"], "Update available");
            ResumeLayout(false);
            Show();
            Activate();
            Global.VersionInfo.UpdateAvailableEvent += VersionInfo_UpdateAvailableEvent;
            if (Global.Config.Gadget.AutoDetectInterfaces)
            {
                timer.AutoReset = false;
                timer.Elapsed += timer_Elapsed;
                timer.Interval = 5000;
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            }
        }

        void VersionInfo_UpdateAvailableEvent(object sender, VersionInfo.UpdateEventArgs e)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    Controls["update"].Visible = true;
                    IntPtr handle = User32.WindowFromPoint(new User32.POINT(Location.X + Width - 1, Location.Y + Height - 1));
                if (handle == Controls["botBkg"].Handle)
                    new BalloonTip("Update available", "New version " + Global.VersionInfo.MainModule.LatestVersion, Controls["update"], BalloonTip.ICON.INFO, 5000);
                }));
            }
            catch { }
        }

        public new void Refresh()
        {
            if (RefreshInProgress == false)
            {
                RefreshInProgress = true;
                if (InvokeRequired)
                    Invoke(new Action(() => { Program.Refresh(); }));
                else
                    Program.Refresh();
            }
        }

        void Update_Click(object sender, EventArgs e)
        {
            Global.VersionInfo.CheckForUpdates(true);
        }

        async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (AutoRefreshAllowed)
                {
                    AutoRefreshAllowed = false;
                    Global.BusyForm busyForm;
                    while (!Global.BusyForms.IsEmpty)
                    {
                        if (Global.BusyForms.TryDequeue(out busyForm))
                        {
                            try
                            {
                                if (!busyForm.Done.Task.IsCompleted)
                                    Global.TrayIcon.ShowBalloonTip(5000, "Interface changed", "One interface changed its configuration and a refresh is required, but you need to close the following window before that:\n\n" + busyForm.Name, ToolTipIcon.Info);
                                await busyForm.Done.Task;
                            }
                            catch (Exception) { }
                        }
                    }
                    Refresh();
                }
            }
            catch (Exception) { }
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Start();
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            timer.Stop();
            timer.Start();
        }

        void AddImage(Image image, string name, int x, int y)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.Location = new Point(x, y);
            pictureBox.Name = name;
            if (image != null)
                pictureBox.Size = new Size(image.Width, image.Height);
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            this.Controls.Add(pictureBox);
        }

        void AddGroupBox(string name, int x, int y)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Name = name;
            groupBox.Location = new Point(x, y);
            groupBox.Size = new System.Drawing.Size(160, 180);
            groupBox.BackColor = Color.Transparent;
            this.Controls.Add(groupBox);
            this.Controls[name].BringToFront();
        }

        void AddLabel(string name, string text, int x, int y, int width, Color? color = null, ContentAlignment align = ContentAlignment.MiddleLeft)
        {
            Label label = new Label();
            label.Name = name;
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new System.Drawing.Size(width, 13);
            label.BackColor = Color.Transparent;
            label.ForeColor = color ?? Color.White;
            label.Font = new Font("Arial", 7);
            label.TextAlign = align;
            this.Controls.Add(label);
            this.Controls[name].BringToFront();
        }

        void AddGraph(string name, int x, int y)
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
            chart.AntiAliasing = AntiAliasingStyles.None;
            chart.MouseMove += new MouseEventHandler(Graph_MouseMove);
            this.Controls.Add(chart);
            this.Controls[name].BringToFront();
            this.Controls[name].MouseEnter += new EventHandler((s, e) => { hoveredGraph = (Chart)s; });
            this.Controls[name].MouseLeave += new EventHandler((s, e) => { hoveredGraph = null; this.toolTip.Hide((Control)s); });
        }

        void Graph_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if ((this.lastMouseEvent.X != e.X) && (e.X < ((Chart)sender).Width))
                {
                    ChartArea chartArea = ((Chart)sender).ChartAreas[0];
                    int xValue = e.X * ((Chart)sender).Series[0].Points.Count / ((Chart)sender).Width;
                    Int64 dw = (Int64)((Chart)sender).Series[0].Points[xValue].YValues[0];
                    Int64 up = (Int64)((Chart)sender).Series[1].Points[xValue].YValues[0];
                    this.toolTip.Show("D:" + Lib.Network.Unit.AutoScale(dw, "B") + "\nU:" + Lib.Network.Unit.AutoScale(up, "B"), (Control)sender, e.X + 15, e.Y + 15);
                }
                lastMouseEvent = e;
            }
            catch (Exception) { }
        }

        void Titlebar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Location == lastMouseEvent.Location)
                return;
            if ((e.X > (this.Width - 21)) && (e.X < (this.Width - 5)) && (e.Y > (2)) && (e.Y < (16)))
            {
                if (this.Controls["closeButtonHover"].Visible == false)
                {
                    this.Controls["closeButtonHover"].Visible = true;
                    this.Cursor = Cursors.Hand;
                }
            }
            else
            {
                if (this.Controls["closeButtonHover"].Visible == true)
                {
                    this.Controls["closeButtonHover"].Visible = false;
                    this.Cursor = Cursors.Default;
                }
            }
            if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Left) && (e.Location != lastMouseEvent.Location))
            {
                //Lib.Windows.User32.ReleaseCapture();
                //Lib.Windows.User32.SendMessage(this.Handle, Lib.Windows.User32.WM_NCLBUTTONDOWN, Lib.Windows.User32.HT_CAPTION, 0);
                if (lastMouseEvent.Button.HasFlag(MouseButtons.Left))
                {
                    Point p = PointToScreen(e.Location);
                    Location = new Point(p.X - startMovePoint.X, p.Y - startMovePoint.Y);
                }
            }
            lastMouseEvent = e;
        }


        void Titlebar_MouseUp(object sender, MouseEventArgs e)
        {
            Global.Config.Gadget.Location = CheckLocation(Location);
            Global.Config.Save();
        }

        void GadgetForm_MouseDown(object sender, MouseEventArgs e)
        {
            startMovePoint = e.Location;
            lastMouseEvent = e;
        }

        async void UpdateTraffic(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int i = 0;
                    foreach (Lib.Network.NetworkInterface nic in Global.NetworkInterfaces.Values)
                    {
                        if (hiddenInterfaces.Contains(nic.Guid))
                            continue;
                        if (i >= (maxVerticalSlots * maxHorizontalSlots))
                            break;
                        this.Controls["downByte" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4InSpeed, "B");
                        this.Controls["downBit" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4InSpeed * 8, "b");
                        this.Controls["upByte" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4OutSpeed, "B");
                        this.Controls["upBit" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4OutSpeed * 8, "b");
                        this.Controls["totalDown" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4BytesReceived, "B");
                        this.Controls["totalUp" + nic.Guid].Text = Lib.Network.Unit.AutoScale(nic.IPv4BytesSent, "B");
                        Chart chart = ((Chart)this.Controls["graph" + nic.Guid]);
                        chart.Series[0].Points.RemoveAt(0);
                        chart.Series[0].Points.AddY(nic.IPv4InSpeed);
                        chart.Series[1].Points.RemoveAt(0);
                        chart.Series[1].Points.AddY(nic.IPv4OutSpeed);
                        chart.ChartAreas[0].RecalculateAxesScale();

                        this.toolTip.SetToolTip(this.Controls["downByte" + nic.Guid], "Avg:" + Lib.Network.Unit.AutoScale(nic.IPv4InSpeedAvg20, "B/s"));
                        this.toolTip.SetToolTip(this.Controls["downBit" + nic.Guid], "Avg:" + Lib.Network.Unit.AutoScale(nic.IPv4InSpeedAvg20 * 8, "b/s"));
                        this.toolTip.SetToolTip(this.Controls["upByte" + nic.Guid], "Avg:" + Lib.Network.Unit.AutoScale(nic.IPv4OutSpeedAvg20, "B/s"));
                        this.toolTip.SetToolTip(this.Controls["upBit" + nic.Guid], "Avg:" + Lib.Network.Unit.AutoScale(nic.IPv4OutSpeedAvg20 * 8, "b/s"));

                        if (hoveredGraph == chart)
                        {
                            int xValue = lastMouseEvent.X * chart.Series[0].Points.Count / chart.Width;
                            Int64 dw = (Int64)chart.Series[0].Points[xValue].YValues[0];
                            Int64 up = (Int64)chart.Series[1].Points[xValue].YValues[0];
                            this.toolTip.Show("D:" + Lib.Network.Unit.AutoScale(dw, "B") + "\nU:" + Lib.Network.Unit.AutoScale(up, "B"), (Control)chart, lastMouseEvent.X + 15, lastMouseEvent.Y + 15);
                        }
                        i++;
                    }
                    await TaskEx.Delay(1000);
                }
            }
            catch (Exception) { }
        }

        Point CheckLocation(Point location)
        {
            Rectangle screen = Screen.GetWorkingArea(location);
            if (location.X + Width / 2 < screen.Left)
                location.X = screen.Left - Width / 2;
            if (location.X + Width / 2 > screen.Right)
                location.X = screen.Right - Width / 2;
            if (location.Y < screen.Top)
                location.Y = screen.Top;
            if (location.Y + 25 > screen.Bottom)
                location.Y = screen.Bottom - 25;
            Location = location;
            return location;
        }

        private void GadgetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // null reference exception in NET 4.0 if not subscribed to network events
            try { NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged; }
            catch (Exception) { }
            try { NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged; }
            catch (Exception) { }
            Global.VersionInfo.UpdateAvailableEvent -= VersionInfo_UpdateAvailableEvent;
            foreach (KeyValuePair<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>> subscription in publicIPv4Subscriptions)
                subscription.Key.PublicIPv4Changed -= subscription.Value;
            foreach (KeyValuePair<Lib.Network.NetworkInterface, EventHandler<TextEventArgs>> subscription in publicIPv6Subscriptions)
                subscription.Key.PublicIPv6Changed -= subscription.Value;
            foreach (EventHandler<TextEventArgs> subscription in internetInterfaceSubscriptions)
                Lib.Network.NetworkInterface.InternetInterfaceChanged -= subscription;
            cts.Cancel();
            GadgetForm.Instance = null;
            //Task task;
            //while (!tasks.IsEmpty)
            //{
            //    if (tasks.TryDequeue(out task))
            //    {
            //        try { await task; }
            //        catch (Exception) { }
            //    }
            //}
            
            //if (!updater.IsCompleted)
            //{
            //    //e.Cancel = true;
            //    //this.Hide();
            //    //updater.ContinueWith((t) => { Invoke(new Action(() => Close())); });
            //    updater.Wait();
            //}
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

        private void GadgetForm_Shown(object sender, EventArgs e)
        {
            UpdateTraffic(cts.Token);
            Global.GetInternetInterface();
            Global.GetPublicIPs();
            AutoRefreshAllowed = true;
            RefreshInProgress = false;
        }
    }
}
