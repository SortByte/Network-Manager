using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using WinLib.Network;

namespace Network_Manager.Jobs
{
    static class TrafficMonitor
    {
        static bool run = true;
        static Thread thread = null;

        public static void Start()
        {
            run = true;
            thread = Thread.CurrentThread;
            while(run)
            {
                foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                {
                    nic.GetStatistics();
                    Global.Config.DataUsage.Where(i => i.InterfaceGuid == nic.Guid).ToList().ForEach(i => i.Update(nic));
                }
                if (Global.Config.DataUsage.Any(i => i.SignificantFlow))
                {
                    foreach (Config.InterfaceDataUsage interfaceDataUsage in Global.Config.DataUsage)
                        interfaceDataUsage.SignificantFlow = false;
                    Global.Config.Save();
                }
                Thread.Sleep(1000);
            }
        }
        public static void Stop(bool async = false)
        {
            run = false;
            if (!async && (thread != null))
                thread.Join();
            thread = null;
        }
    }
}
