using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WinLib.WinAPI;
using static WinLib.WinAPI.Iphlpapi;

namespace WinLib.Network
{
    public static class L2
    {
        public static PhysicalAddress SendARP(IPAddress destIP, IPAddress srcIP = null)
        {
            in_addr srcAddr = new in_addr((srcIP ?? IPAddress.Parse("0.0.0.0")).ToString());
            in_addr destAddr = new in_addr(destIP.ToString());
            UInt64 macAddr = 0;
            uint phyAddrLen = 8;
            ERROR result = Iphlpapi.SendARP(destAddr, srcAddr, ref macAddr, ref phyAddrLen);
            PhysicalAddress mac = new PhysicalAddress(new byte[] { });
            if (result == ERROR.ERROR_SUCCESS)
                mac = new PhysicalAddress(BitConverter.GetBytes(macAddr));
            return mac;
        }
    }
}
