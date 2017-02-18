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
using WinLib.WinAPI;
using WinLib.Network;
using WinLib.Forms;

namespace Network_Manager.Gadget.ControlPanel.ConfigureInterface
{
    // TODO: add advanced IPv6 interface settings (RA - client/server)
    public partial class ConfigureInterfaceForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("Configure Interface");
        private NetworkInterface nic;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private LoadingForm splash;
        private List<NetworkInterface.IPHostAddress> ipv4Address = new List<NetworkInterface.IPHostAddress>();
        private List<NetworkInterface.IPGatewayAddress> ipv4Gateway = new List<NetworkInterface.IPGatewayAddress>();
        private List<string> ipv4Dns = new List<string>();
        private List<NetworkInterface.IPHostAddress> ipv6Address = new List<NetworkInterface.IPHostAddress>();
        private List<NetworkInterface.IPGatewayAddress> ipv6Gateway = new List<NetworkInterface.IPGatewayAddress>();
        private List<string> ipv6Dns = new List<string>();

        public ConfigureInterfaceForm(Guid guid)
        {
            if (Instance != null)
            {
                // TODO: flash existing config form
                Instance.WindowState = FormWindowState.Normal;
                Instance.Activate();
                return;
            }
            Instance = this;
            Global.BusyForms.Enqueue(busyForm);
            InitializeComponent();
            // load profiles
            LoadProfiles();
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
            // IPv4
            nic = Global.NetworkInterfaces[guid];
            for (int i = 0; i < nic.IPv4Address.Count; i++)
                dataGridView1.Rows.Add(nic.IPv4Address[i].Address, nic.IPv4Address[i].Subnet);
            for (int i = 0; i < nic.IPv4Gateway.Count; i++)
                dataGridView2.Rows.Add(nic.IPv4Gateway[i].Address, nic.IPv4Gateway[i].GatewayMetric);
            for (int i = 0; i < nic.IPv4DnsServer.Count; i++)
                dataGridView3.Rows.Add(nic.IPv4DnsServer[i]);
            dhcpIPEnabled.Checked = nic.Dhcpv4Enabled > NetworkInterface.Dhcp.Disabled;
            dataGridView1.Enabled = nic.Dhcpv4Enabled == 0;
            dataGridView2.Enabled = nic.Dhcpv4Enabled == 0;
            dhcpDnsEnabled.Checked = nic.Dhcpv4Enabled == NetworkInterface.Dhcp.IPnDns;
            button13.Enabled = nic.Dhcpv4Enabled < NetworkInterface.Dhcp.IPnDns;
            button14.Enabled = nic.Dhcpv4Enabled < NetworkInterface.Dhcp.IPnDns;
            dataGridView3.Enabled = nic.Dhcpv4Enabled < NetworkInterface.Dhcp.IPnDns;
            interfaceMetric.Text = nic.InterfaceMetric.ToString();
            ipv4Mtu.Text = nic.IPv4Mtu.ToString();
            new WinLib.Forms.TextBoxMask(interfaceMetric, WinLib.Forms.TextBoxMask.Mask.Numeric);
            new WinLib.Forms.TextBoxMask(ipv4Mtu, WinLib.Forms.TextBoxMask.Mask.Numeric);
            netbiosEnabled.Checked = nic.NetbiosEnabled < NetworkInterface.Netbios.Disabled;
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                toolTip.SetToolTip(ipv4Mtu, "This has no effect on versions of Windows older that Windows Vista");
            }
            // IPv6
            for (int i = 0; i < nic.IPv6Address.Global.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.Global[i].Address, nic.IPv6Address.Global[i].Subnet);
            for (int i = 0; i < nic.IPv6Address.Temporary.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.Temporary[i].Address, nic.IPv6Address.Temporary[i].Subnet);
            for (int i = 0; i < nic.IPv6Address.LinkLocal.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.LinkLocal[i].Address, nic.IPv6Address.LinkLocal[i].Subnet);
            for (int i = 0; i < nic.IPv6Address.SiteLocal.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.SiteLocal[i].Address, nic.IPv6Address.SiteLocal[i].Subnet);
            for (int i = 0; i < nic.IPv6Address.UniqueLocal.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.UniqueLocal[i].Address, nic.IPv6Address.UniqueLocal[i].Subnet);
            for (int i = 0; i < nic.IPv6Address.Local.Count; i++)
                dataGridView4.Rows.Add(nic.IPv6Address.Local[i].Address, nic.IPv6Address.Local[i].Subnet);

            for (int i = 0; i < nic.IPv6Gateway.Count; i++)
                dataGridView5.Rows.Add(nic.IPv6Gateway[i].Address, nic.IPv6Gateway[i].GatewayMetric);
            for (int i = 0; i < nic.IPv6DnsServer.Count; i++)
                dataGridView6.Rows.Add(nic.IPv6DnsServer[i]);
            routerDiscoveryEnabled.Checked = nic.IPv6RouterDiscoveryEnabled;
            ipv6Mtu.Text = nic.IPv6Mtu.ToString();
            new WinLib.Forms.TextBoxMask(ipv6Mtu, WinLib.Forms.TextBoxMask.Mask.Numeric);
            if (!nic.IPv4Enabled)
                tabControl1.TabPages[0].Enabled = false;
            if (!nic.IPv6Enabled)
                tabControl1.TabPages[1].Enabled = false;
            Text = "Configure " + Global.NetworkInterfaces[guid].Name;
            Show();
        }

        private void ConfigureInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            busyForm.Done.SetResult(true);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                dataGridView1.Enabled = false;
                dataGridView2.Enabled = false;
            }
            else
            {
                dataGridView1.Enabled = true;
                dataGridView2.Enabled = true;
                dhcpDnsEnabled.Checked = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                button13.Enabled = false;
                button14.Enabled = false;
                dataGridView3.Enabled = false;
                dhcpIPEnabled.Checked = true;
            }
            else
            {
                button13.Enabled = true;
                button14.Enabled = true;
                dataGridView3.Enabled = true;
            }
        }

        private void balloonTip_Popup(object sender, PopupEventArgs e)
        {
            if (balloonTip.Tag == null)
            {
                balloonTip.Tag = "shown";
                return;
            }
            else
            {
                e.Cancel = true;
            }
        }

        //private const int TTS_BALLOON = 0x80;
        //private const int TTS_CLOSE = 0x40;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.Style = TTS_BALLOON | TTS_CLOSE;
        //        return cp;
        //    }
        //}

        private void ShowDGVBallon(string message, Control control, Rectangle cellRect)
        {
            short x = (short)(cellRect.X + cellRect.Width / 2);
            short y = (short)(cellRect.Y + cellRect.Height / 2);
            new BalloonTip("Warning", message, control, BalloonTip.ICON.WARNING, x: x, y: y);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv4(ref cell))
                    ShowDGVBallon("Invalid IPv4 address", control, cellRect);
                else if (dataGridView1.Rows[e.RowIndex].Cells[1].Value == null)
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = "255.255.255.0";
            }
            else
            {
                if (!IP.ValidateIPv4Mask(ref cell))
                    ShowDGVBallon("Invalid IPv4 subnet mask", control, cellRect);
            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv4(ref cell))
                    ShowDGVBallon("Invalid IPv4 address", control, cellRect);
                else if (dataGridView2.Rows[e.RowIndex].Cells[1].Value == null)
                    dataGridView2.Rows[e.RowIndex].Cells[1].Value = "1";
            }
            else
            {
                if (!Regex.IsMatch(cell, @"^[\d]{1,8}$") ||
                    Convert.ToInt32(cell) > 9999)
                    ShowDGVBallon("Metric must be a numeric value between 0 and 9999", control, cellRect);

            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv4(ref cell))
                    ShowDGVBallon("Invalid IPv4 address", control, cellRect);
            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv6(ref cell))
                    ShowDGVBallon("Invalid IPv6 address", control, cellRect);
                else if (dataGridView4.Rows[e.RowIndex].Cells[1].Value == null)
                    dataGridView4.Rows[e.RowIndex].Cells[1].Value = "64";
            }
            else
            {
                if (!Regex.IsMatch(cell, @"^[\d]{1,8}$") ||
                    Convert.ToInt32(cell) > 128)
                    ShowDGVBallon("IPv6 subnet prefix length must be a numeric value beween 0 and 128", control, cellRect);
            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView5_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv6(ref cell))
                    ShowDGVBallon("Invalid IPv6 address", control, cellRect);
                else if (dataGridView5.Rows[e.RowIndex].Cells[1].Value == null)
                    dataGridView5.Rows[e.RowIndex].Cells[1].Value = "1";
            }
            else
            {
                if (!Regex.IsMatch(cell, @"^[\d]{1,8}$") ||
                    Convert.ToInt32(cell) > 9999)
                    ShowDGVBallon("Metric must be a numeric value between 0 and 9999", control, cellRect);
            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView6_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (e.RowIndex < 0 || control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;
            string cell = control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            Rectangle cellRect = control.RectangleToScreen(control.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true));
            if (e.ColumnIndex == 0)
            {
                if (!IP.ValidateIPv6(ref cell))
                    ShowDGVBallon("Invalid IPv6 address", control, cellRect);
            }
            control.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cell;
        }

        private void dataGridView1_EnabledChanged(object sender, EventArgs e)
        {
            DataGridView control = (DataGridView)sender;
            if (control.Enabled)
            {
                control.DefaultCellStyle.BackColor = SystemColors.Window;
                control.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                control.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Window;
                control.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                control.ReadOnly = false;
                control.EnableHeadersVisualStyles = true;
            }
            else
            {
                control.DefaultCellStyle.BackColor = SystemColors.Control;
                control.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                control.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                control.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
                control.CurrentCell = null;
                control.ReadOnly = true;
                control.EnableHeadersVisualStyles = false;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell == null ||
                dataGridView3.CurrentCell.RowIndex == 0 ||
                dataGridView3.CurrentCell.RowIndex == dataGridView3.Rows.Count - 1)
                return;
            object tempCell = dataGridView3.CurrentCell.Value;
            dataGridView3.CurrentCell.Value = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex - 1].Cells[0].Value;
            dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex - 1].Cells[0].Value = tempCell;
            dataGridView3.CurrentCell = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex - 1].Cells[0];
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell == null ||
                dataGridView3.CurrentCell.RowIndex + 1 >= dataGridView3.Rows.Count - 1 )
                return;
            object tempCell = dataGridView3.CurrentCell.Value;
            dataGridView3.CurrentCell.Value = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex + 1].Cells[0].Value;
            dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex + 1].Cells[0].Value = tempCell;
            dataGridView3.CurrentCell = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex + 1].Cells[0];
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (dataGridView6.CurrentCell == null ||
                dataGridView6.CurrentCell.RowIndex == 0 ||
                dataGridView6.CurrentCell.RowIndex == dataGridView6.Rows.Count - 1)
                return;
            object tempCell = dataGridView6.CurrentCell.Value;
            dataGridView6.CurrentCell.Value = dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex - 1].Cells[0].Value;
            dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex - 1].Cells[0].Value = tempCell;
            dataGridView6.CurrentCell = dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex - 1].Cells[0];
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (dataGridView6.CurrentCell == null ||
                dataGridView6.CurrentCell.RowIndex + 1 >= dataGridView6.Rows.Count - 1)
                return;
            object tempCell = dataGridView6.CurrentCell.Value;
            dataGridView6.CurrentCell.Value = dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex + 1].Cells[0].Value;
            dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex + 1].Cells[0].Value = tempCell;
            dataGridView6.CurrentCell = dataGridView6.Rows[dataGridView6.CurrentCell.RowIndex + 1].Cells[0];
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigs())
                return;
            // validation passed; starting configuring
            Hide();
            GadgetForm.AutoRefreshAllowed = false;
            splash = LoadingForm.Create("Configuring interface \"" + nic.Name + "\" ...");
            // XP sets gateway metric along with interface metric, if gwmetric is auto, so we do this first
            if (interfaceMetric.Text != nic.InterfaceMetric.ToString())
                nic.SetInterfaceMetric(interfaceMetric.Text);
            if (nic.IPv4Enabled)
            {
                if (dhcpIPEnabled.Checked || ipv4Address.Count == 0)
                    nic.SetDhcp(1 + Convert.ToInt32(dhcpDnsEnabled.Checked || ipv4Dns.Count == 0));
                else
                {
                    if (nic.Dhcpv4Enabled > 0)
                        nic.SetIPv4Address(ipv4Address[0].Address, ipv4Address[0].Subnet);
                    if (ipv4Address.Count > 0)
                    {
                        foreach (NetworkInterface.IPHostAddress ip in ipv4Address)
                        {
                            if (nic.IPv4Address.Find((i) => i.Address == ip.Address) != null &&
                                nic.IPv4Address.Find((i) => i.Address == ip.Address).Subnet == ip.Subnet &&
                                nic.Dhcpv4Enabled == 0)
                                continue;
                            if (nic.IPv4Address.Find((i) => i.Address == ip.Address) != null ||
                                nic.Dhcpv4Enabled > 0)
                                nic.DeleteIPv4Address(ip.Address, ip.Subnet);
                            nic.AddIPv4Address(ip.Address, ip.Subnet);
                        }
                        foreach (NetworkInterface.IPHostAddress ip in nic.IPv4Address)
                            if (ipv4Address.Find((i) => i.Address == ip.Address) == null)
                                nic.DeleteIPv4Address(ip.Address, ip.Subnet);
                        foreach (NetworkInterface.IPGatewayAddress ip in ipv4Gateway)
                        {
                            if (nic.IPv4Gateway.Find((i) => i.Address == ip.Address) != null &&
                                nic.IPv4Gateway.Find((i) => i.Address == ip.Address).GatewayMetric == ip.GatewayMetric &&
                                nic.Dhcpv4Enabled == 0)
                                continue;
                            if (nic.IPv4Gateway.Find((i) => i.Address == ip.Address) != null ||
                                nic.Dhcpv4Enabled > 0)
                                nic.DeleteIPv4Gateway(ip.Address);
                            nic.AddIPv4Gateway(ip.Address, ip.GatewayMetric.ToString());
                        }
                        foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                            if (ipv4Gateway.Find((i) => i.Address == ip.Address) == null)
                                nic.DeleteIPv4Gateway(ip.Address);
                    }
                }
                foreach (string dns in ipv4Dns)
                {
                    if (nic.IPv4DnsServer.Contains(dns) && nic.Dhcpv4Enabled < NetworkInterface.Dhcp.IPnDns)
                        continue;
                    nic.AddIPv4DnsServer(dns);
                }
                foreach (string dns in nic.IPv4DnsServer)
                    if (!ipv4Dns.Contains(dns))
                        nic.DeleteIPv4DnsServer(dns);
                if (netbiosEnabled.Checked == false && nic.NetbiosEnabled != NetworkInterface.Netbios.Disabled)
                    nic.SetNetBios(false);
                if (netbiosEnabled.Checked && nic.NetbiosEnabled == NetworkInterface.Netbios.Disabled)
                    nic.SetNetBios(true);
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                    if (int.Parse(ipv4Mtu.Text) != nic.IPv4Mtu && int.Parse(ipv4Mtu.Text) > 0)
                        nic.SetIPv4Mtu(ipv4Mtu.Text);
            }
            if (nic.IPv6Enabled)
            {
                foreach (NetworkInterface.IPHostAddress ip in ipv6Address)
                {
                    if (nic.IPv6Address.Contains(ip.Address, ip.Subnet))
                        continue;
                    nic.AddIPv6Address(ip.Address, ip.Subnet);
                }
                foreach (NetworkInterface.IPHostAddress ip in nic.IPv6Address.All)
                    if (ipv6Address.Find((i) => i.Address == ip.Address) == null)
                        nic.DeleteIPv6Address(ip.Address);
                foreach (NetworkInterface.IPGatewayAddress ip in ipv6Gateway)
                {
                    if (nic.IPv6Gateway.Contains(new NetworkInterface.IPGatewayAddress(ip.Address, ip.GatewayMetric)))
                        continue;
                    nic.AddIPv6Gateway(ip.Address, ip.GatewayMetric.ToString());
                }
                foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                    if (ipv6Gateway.Find((i) => i.Address == ip.Address) == null)
                        nic.DeleteIPv6Gateway(ip.Address);
                foreach (string ip in ipv6Dns)
                {
                    if (nic.IPv6DnsServer.Contains(ip))
                        continue;
                    nic.AddIPv6DnsServer(ip);
                }
                foreach (string ip in nic.IPv6DnsServer)
                    if (!ipv6Dns.Contains(ip))
                        nic.DeleteIPv6DnsServer(ip);
                if (routerDiscoveryEnabled.Checked != nic.IPv6RouterDiscoveryEnabled)
                    nic.SetRouterDiscovery(routerDiscoveryEnabled.Checked);
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                    if (int.Parse(ipv6Mtu.Text) != nic.IPv6Mtu && int.Parse(ipv6Mtu.Text) > 0)
                        nic.SetIPv6Mtu(ipv6Mtu.Text);
            }
            splash.UpdateStatus("Waiting for changes to be applied ...");
            timer.AutoReset = false;
            timer.Interval = 10000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            timer.Interval = 5000;
            timer.Stop();
            timer.Start();
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            timer.Interval = 5000;
            timer.Stop();
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            splash.Stop();
            Invoke(new Action(() => { Program.Refresh(); }));
        }

        void LoadProfiles()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("<New profile>");
            foreach (Config.InterfaceProfile profile in Global.Config.InterfaceProfiles)
                comboBox1.Items.Add(profile.Name);
            comboBox1.SelectedIndex = 0;
        }

        private void LoadProfile_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                new BalloonTip("No selection", "Select a configuration first", comboBox1, BalloonTip.ICON.INFO, 5000);
                return;
            }
            using (LoadProfileForm form = new LoadProfileForm(comboBox1.Text))
            {
                DialogResult result = form.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;
                if (form.LoadMode != Config.InterfaceProfile.LoadingMode.Append)
                {
                    if (form.IPv4Address.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView1.Rows.Clear();
                    if (form.IPv4Gateway.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView2.Rows.Clear();
                    if (form.IPv4DnsServer.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView3.Rows.Clear();
                    if (form.IPv6Address.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView4.Rows.Clear();
                    if (form.IPv6Gateway.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView5.Rows.Clear();
                    if (form.IPv6DnsServer.Count > 0 || form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll)
                        dataGridView6.Rows.Clear();
                }
                int addressCount = 0;
                int dnsCount = 0;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                        addressCount++;
                foreach (DataGridViewRow row in dataGridView3.Rows)
                    if (row.Cells[0].Value != null)
                        dnsCount++;
                if (form.IPv4Address.Count > 0 || form.IPv4Gateway.Count > 0)
                    dhcpIPEnabled.Checked = false;
                else if (form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll && (form.IPv4Address.Count == 0 || form.IPv4Gateway.Count == 0))
                    dhcpIPEnabled.Checked = true;
                if (form.IPv4DnsServer.Count > 0)
                    dhcpDnsEnabled.Checked = false;
                else if (form.LoadMode == Config.InterfaceProfile.LoadingMode.ReplaceAll && form.IPv4DnsServer.Count == 0 && dhcpIPEnabled.Checked)
                    dhcpDnsEnabled.Checked = true;

                // IPv4
                if (nic.IPv4Enabled)
                {
                    foreach (NetworkInterface.IPHostAddress ip in form.IPv4Address)
                        dataGridView1.Rows.Add(ip.Address, ip.Subnet);
                    foreach (NetworkInterface.IPGatewayAddress ip in form.IPv4Gateway)
                        dataGridView2.Rows.Add(ip.Address, ip.GatewayMetric);
                    foreach (string ip in form.IPv4DnsServer)
                        dataGridView3.Rows.Add(ip);
                    if (form.DhcpEnabled == NetworkInterface.Dhcp.Disabled)
                    {
                        dhcpIPEnabled.Checked = false;
                        dhcpDnsEnabled.Checked = false;
                    }
                    else if (form.DhcpEnabled == NetworkInterface.Dhcp.IPOnly)
                    {
                        dhcpIPEnabled.Checked = true;
                        dhcpDnsEnabled.Checked = false;
                    }
                    else if (form.DhcpEnabled == NetworkInterface.Dhcp.IPnDns)
                    {
                        dhcpIPEnabled.Checked = true;
                        dhcpDnsEnabled.Checked = true;
                    }
                    if (form.NetbiosEnabled == NetworkInterface.Netbios.Disabled)
                        netbiosEnabled.Checked = false;
                    else if (form.NetbiosEnabled == NetworkInterface.Netbios.Enabled)
                        netbiosEnabled.Checked = true;
                    if (form.IPv4Mtu > 0)
                        ipv4Mtu.Text = Convert.ToString(form.IPv4Mtu);
                }
                // IPv6
                if (nic.IPv6Enabled)
                {
                    foreach (NetworkInterface.IPHostAddress ip in form.IPv6Address)
                        dataGridView4.Rows.Add(ip.Address, ip.Subnet);
                    foreach (NetworkInterface.IPGatewayAddress ip in form.IPv6Gateway)
                        dataGridView5.Rows.Add(ip.Address, ip.GatewayMetric);
                    foreach (string ip in form.IPv6DnsServer)
                        dataGridView6.Rows.Add(ip);
                    if (form.RouterDiscovery == NetworkInterface.RouterDiscovery.Disabled)
                        routerDiscoveryEnabled.Checked = false;
                    else if (form.RouterDiscovery == NetworkInterface.RouterDiscovery.Enabled)
                        routerDiscoveryEnabled.Checked = true;
                    if (form.IPv6Mtu > 0)
                        ipv6Mtu.Text = Convert.ToString(form.IPv6Mtu);
                }

                if (form.InterfaceMetric > -1)
                    interfaceMetric.Text = Convert.ToString(form.InterfaceMetric);
            }
        }

        private void SaveProfile_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigs(false))
                return;
            using (SaveProfileForm form = new SaveProfileForm(comboBox1.SelectedIndex > 0 ? comboBox1.Text : "",
                nic.IPv4Enabled,
                ipv4Address,
                ipv4Gateway,
                ipv4Dns,
                dhcpIPEnabled.Checked,
                dhcpDnsEnabled.Checked,
                netbiosEnabled.Checked,
                int.Parse(ipv4Mtu.Text),
                nic.IPv6Enabled,
                ipv6Address,
                ipv6Gateway,
                ipv6Dns,
                routerDiscoveryEnabled.Checked,
                int.Parse(ipv6Mtu.Text),
                int.Parse(interfaceMetric.Text)))
            {
                DialogResult result = form.ShowDialog();
                if (result.CompareTo(DialogResult.OK) == 0)
                {
                    Config.InterfaceProfile config;
                    if ((config = Global.Config.InterfaceProfiles.Find((i) => i.Name == form.ProfileName)) != null)
                    {
                        int index = Global.Config.InterfaceProfiles.IndexOf(config);
                        Global.Config.InterfaceProfiles[index] = new Config.InterfaceProfile();
                        config = Global.Config.InterfaceProfiles[index];
                    }
                    else
                    {
                        Global.Config.InterfaceProfiles.Add(new Config.InterfaceProfile());
                        config = Global.Config.InterfaceProfiles.Last();
                    }
                    config.Name = form.ProfileName;
                    config.IPv4LocalAddresses = form.IPv4Address;
                    config.IPv4GatewayAddresses = form.IPv4Gateway;
                    config.IPv4DnsAddresses = form.IPv4DnsServer;
                    config.DhcpEnabled = form.DhcpEnabled;
                    config.NetbiosEnabled = form.NetbiosEnabled;
                    config.IPv4Mtu = form.IPv4Mtu;
                    config.IPv6LocalAddresses = form.IPv6Address;
                    config.IPv6GatewayAddresses = form.IPv6Gateway;
                    config.IPv6DnsAddresses = form.IPv6DnsServer;
                    config.IPv6RouterDiscoveryEnabled = form.RouterDiscovery;
                    config.IPv6Mtu = form.IPv6Mtu;
                    config.InterfaceMetric = form.InterfaceMetric;
                    config.LoadMode = form.LoadMode;
                    Global.Config.Save();
                    LoadProfiles();
                    comboBox1.SelectedItem = form.ProfileName;
                }
            }
        }

        private void DeleteProfile_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                new BalloonTip("No selection", "Select a configuration first", comboBox1, BalloonTip.ICON.INFO, 5000);
                return;
            }
            DialogResult result = MessageBox.Show(this, "Do you want to delete \"" + comboBox1.Text + "\" saved interface configuration profile ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.No)
                return;
            if (Global.Config.InterfaceProfiles.Find((i) => i.Name == comboBox1.Text) != null)
            {
                int index = Global.Config.InterfaceProfiles.IndexOf(Global.Config.InterfaceProfiles.Find((i) => i.Name == comboBox1.Text));
                Global.Config.InterfaceProfiles.RemoveAt(index);
                Global.Config.Save();
                LoadProfiles();
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                return;
            int mtu;
            if (Int32.TryParse(ipv4Mtu.Text, out mtu) && mtu < 576)
            {
                tabControl1.SelectTab(0);
                new BalloonTip("Warning", "MTU for IPv4 can't be smaller than 576 bytes", ipv4Mtu, BalloonTip.ICON.WARNING);
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                return;
            int mtu;
            if (Int32.TryParse(ipv6Mtu.Text, out mtu) &&  mtu < 1280)
            {
                tabControl1.SelectTab(1);
                new BalloonTip("Warning", "MTU for IPv6 can't be smaller than 1280 bytes", ipv6Mtu, BalloonTip.ICON.WARNING);
            }
        }

        bool ValidateConfigs(bool checkIfIPUsed = true)
        {
            ipv4Address.Clear();
            ipv4Gateway.Clear();
            ipv4Dns.Clear();
            ipv6Address.Clear();
            ipv6Gateway.Clear();
            ipv6Dns.Clear();
            // IPv4 validation
            if (nic.IPv4Enabled && dhcpIPEnabled.Checked == false)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                    {
                        if (row.Cells[1].Value == null)
                            row.Cells[1].Value = "255.255.255.0";
                        if (row.Cells[0].Value == null ||
                        !IP.ValidateIPv4(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString()))
                        {
                            tabControl1.SelectTab(0);
                            dataGridView1.CurrentCell = null;
                            dataGridView1.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView1.RectangleToScreen(dataGridView1.GetCellDisplayRectangle(1, row.Index, true));
                            ShowDGVBallon("Invalid IPv4 address or subnet mask", dataGridView1, cellRect);
                            return false;
                        }
                        if (ipv4Address.Find((s) => { return s.Address == row.Cells[0].Value.ToString(); }) != null)
                        {
                            tabControl1.SelectTab(0);
                            dataGridView1.CurrentCell = null;
                            dataGridView1.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView1.RectangleToScreen(dataGridView1.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView1, cellRect);
                            return false;
                        }
                        string name;
                        if ((name = NetworkInterface.CheckIfIPv4Used(row.Cells[0].Value.ToString(), nic.Guid)) != null & checkIfIPUsed)
                        {
                            tabControl1.SelectTab(0);
                            dataGridView1.CurrentCell = null;
                            dataGridView1.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView1.RectangleToScreen(dataGridView1.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("IPv4 address already used by \"" + name + "\"", dataGridView1, cellRect);
                            return false;
                        }
                        ipv4Address.Add(new NetworkInterface.IPHostAddress(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString()));
                    }
                }
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                    {
                        if (row.Cells[0].Value == null ||
                        !IP.ValidateIPv4(row.Cells[0].Value.ToString()))
                        {
                            tabControl1.SelectTab(0);
                            dataGridView2.CurrentCell = null;
                            dataGridView2.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView2.RectangleToScreen(dataGridView2.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Invalid IPv4 address", dataGridView2, cellRect);
                            return false;
                        }
                        if (ipv4Gateway.Find((s) => { return s.Address == row.Cells[0].Value.ToString(); }) != null)
                        {
                            tabControl1.SelectTab(0);
                            dataGridView2.CurrentCell = null;
                            dataGridView2.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView2.RectangleToScreen(dataGridView2.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView2, cellRect);
                            return false;
                        }
                        if (row.Cells[1].Value == null)
                        {
                            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                                row.Cells[1].Value = "1";
                            else
                                row.Cells[1].Value = "0";
                        }
                        if (!Regex.IsMatch(row.Cells[1].Value.ToString(), @"^[\d]+$") ||
                            Convert.ToInt32(row.Cells[1].Value.ToString()) > 9999 ||
                            Convert.ToInt32(row.Cells[1].Value.ToString()) == 0 && Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                        {
                            tabControl1.SelectTab(0);
                            dataGridView2.CurrentCell = null;
                            dataGridView2.CurrentCell = row.Cells[1];
                            Rectangle cellRect = dataGridView2.RectangleToScreen(dataGridView2.GetCellDisplayRectangle(1, row.Index, true));
                            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                                ShowDGVBallon("Metric must be a numeric value between 1 and 9999", dataGridView2, cellRect);
                            else
                                ShowDGVBallon("Metric must be a numeric value between 0 and 9999", dataGridView2, cellRect);
                            return false;
                        }
                        ipv4Gateway.Add(new NetworkInterface.IPGatewayAddress(row.Cells[0].Value.ToString(), ushort.Parse(row.Cells[1].Value.ToString())));
                    }
                }
            }
            if (nic.IPv4Enabled && dhcpDnsEnabled.Checked == false)
            {
                foreach (DataGridViewRow row in dataGridView3.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        if (!IP.ValidateIPv4(row.Cells[0].Value.ToString()))
                        {
                            tabControl1.SelectTab(0);
                            dataGridView3.CurrentCell = null;
                            dataGridView3.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView3.RectangleToScreen(dataGridView3.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Invalid IPv4 address", dataGridView3, cellRect);
                            return false;
                        }
                        if (ipv4Dns.Find((s) => { return s == row.Cells[0].Value.ToString(); }) != null)
                        {
                            tabControl1.SelectTab(0);
                            dataGridView3.CurrentCell = null;
                            dataGridView3.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView3.RectangleToScreen(dataGridView3.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView3, cellRect);
                            return false;
                        }
                        ipv4Dns.Add(row.Cells[0].Value.ToString());
                    }
                }
            }
            if (ipv4Mtu.Text == "")
                ipv4Mtu.Text = "1500";
            if (nic.IPv4Enabled && Regex.IsMatch(ipv4Mtu.Text, @"^[\d]*$"))
                if (Convert.ToInt32(ipv4Mtu.Text) < 576 && Convert.ToInt32(ipv4Mtu.Text) > 0)
                {
                    tabControl1.SelectTab(0);
                    new BalloonTip("Warning", "MTU for IPv4 can't be smaller than 576 bytes", ipv4Mtu, BalloonTip.ICON.WARNING);
                    return false;
                }
            // IPv6 validation
            if (nic.IPv6Enabled)
            {
                foreach (DataGridViewRow row in dataGridView4.Rows)
                {
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                    {
                        if (row.Cells[0].Value == null ||
                        !IP.ValidateIPv6(row.Cells[0].Value.ToString()))
                        {
                            tabControl1.SelectTab(1);
                            dataGridView4.CurrentCell = null;
                            dataGridView4.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView4.RectangleToScreen(dataGridView4.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Invalid IPv6 address", dataGridView4, cellRect);
                            return false;
                        }
                        if (ipv6Address.Find((s) =>
                        {
                            return IPAddress.Parse(s.Address).Equals(IPAddress.Parse(row.Cells[0].Value.ToString()));
                        }) != null)
                        {
                            tabControl1.SelectTab(1);
                            dataGridView4.CurrentCell = null;
                            dataGridView4.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView4.RectangleToScreen(dataGridView4.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView4, cellRect);
                            return false;
                        }
                        foreach (NetworkInterface n in Global.NetworkInterfaces.Values)
                        {
                            if (n.Guid != nic.Guid &&
                                n.IPv6Address.Contains(row.Cells[0].Value.ToString()) &&
                                checkIfIPUsed)
                            {
                                tabControl1.SelectTab(1);
                                dataGridView4.CurrentCell = null;
                                dataGridView4.CurrentCell = row.Cells[0];
                                Rectangle cellRect = dataGridView4.RectangleToScreen(dataGridView4.GetCellDisplayRectangle(0, row.Index, true));
                                ShowDGVBallon("IPv6 address already used by \"" + n.Name + "\"", dataGridView4, cellRect);
                                return false;
                            }
                        }
                        if (row.Cells[1].Value == null)
                            row.Cells[1].Value = "64";
                        if (!Regex.IsMatch(row.Cells[1].Value.ToString(), @"^[\d]+$") ||
                            Convert.ToInt32(row.Cells[1].Value.ToString()) > 128)
                        {
                            tabControl1.SelectTab(1);
                            dataGridView4.CurrentCell = null;
                            dataGridView4.CurrentCell = row.Cells[1];
                            Rectangle cellRect = dataGridView4.RectangleToScreen(dataGridView4.GetCellDisplayRectangle(1, row.Index, true));
                            ShowDGVBallon("IPv6 subnet prefix length must be a numeric value beween 0 and 128", dataGridView4, cellRect);
                            return false;
                        }
                        ipv6Address.Add(new NetworkInterface.IPHostAddress(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString()));
                    }
                }
                foreach (DataGridViewRow row in dataGridView5.Rows)
                {
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                    {
                        if (row.Cells[0].Value == null ||
                        !IP.ValidateIPv6(row.Cells[0].Value.ToString()))
                        {
                            tabControl1.SelectTab(1);
                            dataGridView5.CurrentCell = null;
                            dataGridView5.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView5.RectangleToScreen(dataGridView5.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Invalid IPv6 address", dataGridView5, cellRect);
                            return false;
                        }
                        if (ipv6Gateway.Find((s) =>
                        {
                            return IPAddress.Parse(s.Address).Equals(IPAddress.Parse(row.Cells[0].Value.ToString()));
                        }) != null)
                        {
                            tabControl1.SelectTab(1);
                            dataGridView5.CurrentCell = null;
                            dataGridView5.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView5.RectangleToScreen(dataGridView5.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView5, cellRect);
                            return false;
                        }
                        if (row.Cells[1].Value == null)
                            row.Cells[1].Value = "0";
                        if (!Regex.IsMatch(row.Cells[1].Value.ToString(), @"^[\d]+$") ||
                            Convert.ToInt32(row.Cells[1].Value.ToString()) > 9999)
                        {
                            tabControl1.SelectTab(1);
                            dataGridView5.CurrentCell = null;
                            dataGridView5.CurrentCell = row.Cells[1];
                            Rectangle cellRect = dataGridView5.RectangleToScreen(dataGridView5.GetCellDisplayRectangle(1, row.Index, true));
                            ShowDGVBallon("Metric must be a numeric value between 0 and 9999", dataGridView5, cellRect);
                            return false;
                        }
                        ipv6Gateway.Add(new NetworkInterface.IPGatewayAddress(row.Cells[0].Value.ToString(), ushort.Parse(row.Cells[1].Value.ToString())));
                    }
                }
                foreach (DataGridViewRow row in dataGridView6.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        if (!IP.ValidateIPv6(row.Cells[0].Value.ToString()))
                        {
                            tabControl1.SelectTab(1);
                            dataGridView6.CurrentCell = null;
                            dataGridView6.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView6.RectangleToScreen(dataGridView6.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Invalid IPv6 address", dataGridView6, cellRect);
                            return false;
                        }
                        if (ipv6Dns.Find((s) =>
                        {
                            return IPAddress.Parse(s).Equals(IPAddress.Parse(row.Cells[0].Value.ToString()));
                        }) != null)
                        {
                            tabControl1.SelectTab(1);
                            dataGridView6.CurrentCell = null;
                            dataGridView6.CurrentCell = row.Cells[0];
                            Rectangle cellRect = dataGridView6.RectangleToScreen(dataGridView6.GetCellDisplayRectangle(0, row.Index, true));
                            ShowDGVBallon("Duplicate entry", dataGridView6, cellRect);
                            return false;
                        }
                        ipv6Dns.Add(row.Cells[0].Value.ToString());
                    }
                }
                if (ipv6Mtu.Text == "")
                    ipv6Mtu.Text = "1280";
                if (Regex.IsMatch(ipv6Mtu.Text, @"^[\d]*$"))
                    if (Convert.ToInt32(ipv6Mtu.Text) < 1280 && Convert.ToInt32(ipv6Mtu.Text) > 0)
                    {
                        tabControl1.SelectTab(1);
                        new BalloonTip("Warning", "MTU for IPv6 can't be smaller than 1280 bytes", ipv6Mtu, BalloonTip.ICON.WARNING);
                        return false;
                    }
            }
            if (interfaceMetric.Text == "")
                interfaceMetric.Text = "1";
            return true;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            DataGridViewCell cell = dgv.CurrentCell;
            if (e.KeyCode == Keys.Delete &&
                cell != null &&
                dgv.CurrentRow != dgv.Rows[dgv.Rows.Count - 1])
            {
                ((DataGridView)sender).Rows.Remove(cell.OwningRow);
                e.SuppressKeyPress = false;
            }
        }
    }
}
