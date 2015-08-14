using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ip;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.IpV6;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Transport;
using Lib.Forms;

namespace Network_Manager.Jobs.Extensions
{
    public partial class UdpDetector
    {
        public static event EventHandler<NewEndPointEventArgs> NewEndPoint;
        public static UdpTable Table = new UdpTable();
        private static List<InterfaceWorker> interfaceWorkers = new List<InterfaceWorker>();

        public static bool Start()
        {
            Global.WriteLog("UDP Detector is starting.", true);
            if (!Dependencies.RunWinPcapService(null, true))
            {
                Global.WriteLog("UDP Detector failed to start.", true);
                Global.ShowTrayTip("UDP Detector", "Failed to start", System.Windows.Forms.ToolTipIcon.Error);
                return false;
            }
            LoadingForm splash = new LoadingForm("Initializing ...");
            interfaceWorkers.Clear();
            foreach (string guid in Dependencies.WinPcapDevices)
            {
                interfaceWorkers.Add(new InterfaceWorker(guid));
                splash.UpdateStatus("Initializing " + interfaceWorkers.Last().Name + " ...");
                new Thread(new ThreadStart(interfaceWorkers.Last().ReceivePackets)).Start();
                interfaceWorkers.Last().Initialized.Wait(10000);
            }
            splash.Stop();
            Dependencies.WinPcapInUse.Reset(Dependencies.WinPcapInUse.CurrentCount + 1);
            Global.WriteLog("UDP Detector: started");
            Global.ShowTrayTip("UDP Detector", "Started", System.Windows.Forms.ToolTipIcon.Info);
            return true;
        }
        public static void Stop(bool async = false)
        {
            if (interfaceWorkers.Count() == 0)
                return;
            for (int i = interfaceWorkers.Count() - 1; i >= 0; i--)
            {
                interfaceWorkers[i].Stop();
                interfaceWorkers.RemoveAt(i);
            }
            Table.Clear();
            Dependencies.WinPcapInUse.Signal();
            Global.WriteLog("Udp Detector has stopped.", true);
            Global.ShowTrayTip("UDP Detector", "Stopped", System.Windows.Forms.ToolTipIcon.Info);
        }

        public class NewEndPointEventArgs : EventArgs
        {
            public IPEndPoint LocalEP;

            public NewEndPointEventArgs(IPEndPoint localEP)
            {
                LocalEP = localEP;
            }
        }
    }

    
}
