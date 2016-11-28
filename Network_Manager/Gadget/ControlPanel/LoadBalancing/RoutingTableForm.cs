using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinLib.Network;

namespace Network_Manager.Gadget.ControlPanel.LoadBalancing
{
    public partial class RoutingTableForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("Load Balancer - Routing Table");
        private CancellationTokenSource cts = new CancellationTokenSource();

        public RoutingTableForm()
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
            // enable double buffering to stop flickering
            var doubleBufferPropertyInfo = listView1.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(listView1, true, null);
            Show();
        }

        private async void UpdateRoutes(CancellationTokenSource cts)
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    List<Jobs.Extensions.LoadBalancer.RoutingTable.RoutingEntry> routingTable = Jobs.Extensions.LoadBalancer.RoutingTable.GetList();
                    listView1.BeginUpdate();
                    // add/update items
                    foreach (Jobs.Extensions.LoadBalancer.RoutingTable.RoutingEntry route in routingTable)
                    {
                        // update existing items
                        bool found = false;
                        foreach (ListViewItem item in listView1.Items)
                            if (route.SocketID.Equals(item.Tag))
                            {
                                item.SubItems[5].Text = Global.NetworkInterfaces[route.InterfaceGuid].Name;
                                found = true;
                                break;
                            }
                        if (found)
                            continue;
                        listView1.Items.Add(new ListViewItem(new string[] {
                            route.SocketID.LocalEP.Address.ToString(),
                            route.SocketID.LocalEP.Port.ToString(),
                            route.SocketID.RemoteEP.Address.ToString(),
                            route.SocketID.RemoteEP.Port.ToString(),
                            route.SocketID.Protocol.ToString(),
                            Global.NetworkInterfaces[route.InterfaceGuid].Name
                        })).Tag = route.SocketID;
                    }
                    // delete items
                    foreach (ListViewItem item in listView1.Items)
                        if (!routingTable.Any(i => i.SocketID.Equals(item.Tag)))
                            item.Remove();
                    foreach (ColumnHeader column in listView1.Columns)
                        column.Width = -2;
                    listView1.EndUpdate();
                    await TaskEx.Delay(1000);
                }
            }
            catch { }
        }

        private void RoutingTableForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
            Instance = null;
            busyForm.Done.SetResult(true);
        }

        private void RoutingTableForm_Shown(object sender, EventArgs e)
        {
            UpdateRoutes(cts);
        }
    }
}
