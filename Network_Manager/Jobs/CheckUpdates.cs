using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_Manager.Jobs
{
    public class CheckUpdates
    {
        static bool run = true;
        static Thread thread = null;

        public static void Start()
        {
            try
            {
                run = Global.Config.Gadget.CheckForUpdates;
                thread = Thread.CurrentThread;
                uint count = 60 * 60 * 24;
                while (run)
                {
                    // every 24h
                    if (count == 60 * 60 * 24)
                    {
                        Global.VersionInfo.CheckForUpdates(false);
                        count = 0;
                    }
                    count++;
                    Thread.Sleep(1000);
                }
            }
            catch { }
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
