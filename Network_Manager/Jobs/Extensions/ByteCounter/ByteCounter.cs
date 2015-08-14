using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ip;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.IpV6;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Transport;
using Lib.Network;
using Lib.Forms;

namespace Network_Manager.Jobs.Extensions
{
    public partial class ByteCounter
    {
        public static event EventHandler<NewBytesEventArgs> NewBytes;
        public static ByteTable Table = new ByteTable();
        private static List<InterfaceWorker> interfaceWorkers = new List<InterfaceWorker>();

        public static bool Start()
        {
            Global.WriteLog("Byte Counter is starting.", true);
            if (!Dependencies.RunWinPcapService(null, true))
            {
                Global.WriteLog("Byte Counter failed to start.", true);
                Global.ShowTrayTip("Byte Counter", "Failed to start", System.Windows.Forms.ToolTipIcon.Error);
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
            Global.WriteLog("Byte Counter: started");
            Global.ShowTrayTip("Byte Counter", "Started", System.Windows.Forms.ToolTipIcon.Info);
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
            Global.WriteLog("Byte Counter has stopped.", true);
            Global.ShowTrayTip("Byte Counter", "Stopped", System.Windows.Forms.ToolTipIcon.Info);
        }

        public class NewBytesEventArgs : EventArgs
        {
            public IP.SocketID SocketID;

            public NewBytesEventArgs(IP.SocketID socketID)
            {
                SocketID = socketID;
            }
        }
    }
}
