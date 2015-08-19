using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.NetworkInformation;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using Lib.WinAPI;
using Lib.Sync;
using Lib.Extensions;
using Lib.Network.Http;

namespace Lib.Network
{
    /// <summary>
    /// Describe your class quickly here.
    /// </summary>
    /// <remarks>
    /// Add more details here.
    /// </remarks>
    public class NetworkInterface
    {
        public static event EventHandler<ExceptionEventArgs> ExceptionThrown;
        /// <summary>
        /// On change the IPv4 address of the corresponding interface is passed to subscribers
        /// </summary>
        public static event EventHandler<TextEventArgs> InternetInterfaceChanged;
        /// <summary>
        /// On change the public IPv4 is passed
        /// </summary>
        public event EventHandler<TextEventArgs> PublicIPv4Changed;
        /// <summary>
        /// On change the public IPv6 is passed
        /// </summary>
        public event EventHandler<TextEventArgs> PublicIPv6Changed;
        public System.Net.NetworkInformation.NetworkInterface networkInterface;
        public string Guid;
        public int Index;
        public string Name;
        public string Description;
        public AdapterType Type;
        public string Mac = "N/A";
        
        public ushort InterfaceMetric;
        
        // IPv4
        public bool IPv4Enabled;
        // TODO: accomodate naming conventions
        public List<IPHostAddress> IPv4Address = new List<IPHostAddress>();
        public List<IPGatewayAddress> IPv4Gateway = new List<IPGatewayAddress>();
        public List<string> IPv4DnsServer = new List<string>();
        public Dhcp Dhcpv4Enabled;
        public string DhcpServer;
        /// <summary>
        /// Local address in the same network as the default gateway with the smallest metric
        /// </summary>
        public string LocalIPv4Exit;
        /// <summary>
        /// The default gateway with the smallest metric that is reachable
        /// </summary>
        public string DefaultIPv4Gateway;
        public event EventHandler DefaultIPv4GatewayChecked;
        public string DefaultIPv4GatewayMac;
        public string PublicIPv4 = "Detecting ...";
        public string IPv4Mtu = "N/A";
        public Netbios NetbiosEnabled;
        public Int64 IPv4BytesReceived;
        public Int64 IPv4BytesSent;
        public double IPv4InSpeed = 0;
        public double IPv4InSpeedAvg10 = 0;
        public double IPv4InSpeedAvg20 = 0;
        public double IPv4InSpeedAvg40 = 0;
        public double IPv4OutSpeed = 0;
        public double IPv4OutSpeedAvg10 = 0;
        public double IPv4OutSpeedAvg20 = 0;
        public double IPv4OutSpeedAvg40 = 0;
        // IPv6
        public bool IPv6Enabled;
        public IPv6HostAddress IPv6Address = new IPv6HostAddress();
        public List<IPGatewayAddress> IPv6Gateway = new List<IPGatewayAddress>();
        public List<string> IPv6DnsServer = new List<string>();
        /// <summary>
        /// Indicates whether or not Router Advertisments are processed<para/>
        /// Regardless, DHCPv6 is always used if there is an DHCPv6 server on the network
        /// </summary>
        public bool IPv6RouterDiscoveryEnabled = false;
        public string Dhcpv6Server;
        public string PublicIPv6 = "Detecting ...";
        public string IPv6Mtu = "N/A";
        //public Int64 IPv6BytesReceived; // ???
        //public Int64 IPv6BytesSent; // ???
        //public Int64 IPv6InSpeed; // ???
        //public Int64 IPv6OutSpeed; // ???

        public static NetworkInterface Loopback = new NetworkInterface();

        public static Process Netsh()
        {
            Process netsh = new Process();
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.CreateNoWindow = true;
            netsh.StartInfo.RedirectStandardOutput = true;
            netsh.StartInfo.FileName = "netsh.exe";
            return netsh;
        }

        // Acquiring functions
        // ===================

        public static ConcurrentDictionary<string, NetworkInterface> GetAll(Action<string>UpdateStatus = null)
        {
            ConcurrentDictionary<string, NetworkInterface> NICs = new ConcurrentDictionary<string, NetworkInterface>();
            try
            {
                System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                ManagementClass Win32_NetworkAdapterConfiguration = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection Win32_NetworkAdapterConfiguration_Items;
                try { Win32_NetworkAdapterConfiguration_Items = Win32_NetworkAdapterConfiguration.GetInstances(); }
                catch (COMException) {
                    System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return NICs;
                }
                IPAddress IPAddress;

                List<Iphlpapi.Route> routes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_UNSPEC);
                List<Iphlpapi.Adapter> adapters = Iphlpapi.GetAdapters(Iphlpapi.FAMILY.AF_UNSPEC);

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "netsh.exe";
                process.StartInfo.Arguments = "int ipv4 show subinterfaces";
                process.Start();
                process.WaitForExit();
                StreamReader stdo = process.StandardOutput;
                string[] ipv4Mtus = stdo.ReadToEnd().Split("\r'\n".ToCharArray());

                process.StartInfo.Arguments = "int ipv6 show subinterfaces";
                process.Start();
                process.WaitForExit();
                stdo = process.StandardOutput;
                string[] ipv6Mtus = stdo.ReadToEnd().Split("\r'\n".ToCharArray());

                string[] netsh;

                // Loopback
                NetworkInterface.Loopback.Name = "Loopback Interface";
                NetworkInterface.Loopback.Index = 1;
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                {
                    System.Net.NetworkInformation.NetworkInterface loopbackNic = nics.Where((i) =>
                        (i.Supports(NetworkInterfaceComponent.IPv4) &&  i.Supports(NetworkInterfaceComponent.IPv6) &&
                        i.GetIPProperties().GetIPv6Properties().Index == 1)).FirstOrDefault();
                    NetworkInterface.Loopback.networkInterface = loopbackNic;
                    NetworkInterface.Loopback.Guid = loopbackNic.Id;
                    NetworkInterface.Loopback.Name = loopbackNic.Name;
                    NetworkInterface.Loopback.Description = loopbackNic.Description;
                    NetworkInterface.Loopback.Type = (AdapterType)loopbackNic.NetworkInterfaceType;
                    NetworkInterface.Loopback.Mac = BitConverter.ToString(loopbackNic.GetPhysicalAddress().GetAddressBytes().ToArray()).Replace('-', ':');
                    NetworkInterface.Loopback.IPv4BytesReceived = loopbackNic.GetIPv4Statistics().BytesReceived;
                    NetworkInterface.Loopback.IPv4BytesSent = loopbackNic.GetIPv4Statistics().BytesSent;
                    NetworkInterface.Loopback.IPv4Enabled = loopbackNic.Supports(NetworkInterfaceComponent.IPv4);
                    NetworkInterface.Loopback.IPv6Enabled = loopbackNic.Supports(NetworkInterfaceComponent.IPv6);
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                        NetworkInterface.Loopback.IPv6Enabled = false;
                    if (loopbackNic.GetIPProperties().GetIPv4Properties() != null)
                        NetworkInterface.Loopback.Index = loopbackNic.GetIPProperties().GetIPv4Properties().Index;
                    else if (loopbackNic.GetIPProperties().GetIPv6Properties() != null)
                        NetworkInterface.Loopback.Index = loopbackNic.GetIPProperties().GetIPv6Properties().Index;
                    foreach (string line in ipv4Mtus)
                        if (Regex.IsMatch(line, @"\s*" + loopbackNic.Name + @"\s*$"))
                            NetworkInterface.Loopback.IPv4Mtu = Regex.Replace(line, @"^\s*(\d+)\s+.+$", "$1");
                    foreach (string line in ipv6Mtus)
                        if (Regex.IsMatch(line, @"\s*" + loopbackNic.Name + @"\s*$"))
                            NetworkInterface.Loopback.IPv6Mtu = Regex.Replace(line, @"^\s*(\d+)\s+.+$", "$1");
                    foreach (IPAddress ip in loopbackNic.GetIPProperties().DnsAddresses)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            NetworkInterface.Loopback.IPv4DnsServer.Add(ip.ToString());
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                            NetworkInterface.Loopback.IPv6DnsServer.Add(ip.ToString());
                    }
                    foreach (IPAddress ip in loopbackNic.GetIPProperties().DhcpServerAddresses)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            Loopback.DhcpServer = ip.ToString();
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                            Loopback.Dhcpv6Server = ip.ToString();
                    }
                    process.StartInfo.Arguments = "int ip show address \"" + Loopback.Name + "\"";
                    process.Start();
                    process.WaitForExit();
                    stdo = process.StandardOutput;
                    netsh = stdo.ReadToEnd().Split("\r'\n".ToCharArray());
                    foreach (string line in netsh)
                        if (Regex.IsMatch(line, @"InterfaceMetric", RegexOptions.IgnoreCase))
                        {
                            NetworkInterface.Loopback.InterfaceMetric = ushort.Parse(Regex.Replace(line, @"^\s*InterfaceMetric:\s*(\d*)\s*$", "$1"));
                        }
                }
                // Connected interfaces
                foreach (System.Net.NetworkInformation.NetworkInterface nic in nics)
                    if (nic.Supports(NetworkInterfaceComponent.IPv4) ||
                        nic.Supports(NetworkInterfaceComponent.IPv6) && Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1)
                        if (nic.OperationalStatus == OperationalStatus.Up)
                            if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                            //if (nic.GetPhysicalAddress().GetAddressBytes().Count() >= 6) // not all interfaces are ethernet emulated
                            {
                                if (UpdateStatus != null)
                                    UpdateStatus("Detecting " + nic.Name + " ...");

                                NICs.TryAdd(nic.Id, new NetworkInterface());
                                NICs[nic.Id].networkInterface = nic;
                                NICs[nic.Id].Guid = nic.Id;
                                NICs[nic.Id].Name = nic.Name;
                                NICs[nic.Id].Description = nic.Description;
                                NICs[nic.Id].Type = (AdapterType)nic.NetworkInterfaceType;
                                NICs[nic.Id].Mac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes().ToArray()).Replace("-", ":");
                                NICs[nic.Id].IPv4BytesReceived = nic.GetIPv4Statistics().BytesReceived;
                                NICs[nic.Id].IPv4BytesSent = nic.GetIPv4Statistics().BytesSent;
                                NICs[nic.Id].IPv4Enabled = nic.Supports(NetworkInterfaceComponent.IPv4);
                                NICs[nic.Id].IPv6Enabled = nic.Supports(NetworkInterfaceComponent.IPv6);

                                // forcefully no IPv6 support on XP
                                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                                    NICs[nic.Id].IPv6Enabled = false;

                                if (nic.Supports(NetworkInterfaceComponent.IPv4))
                                {
                                    if (nic.GetIPProperties().GetIPv4Properties() != null)
                                        NICs[nic.Id].Index = nic.GetIPProperties().GetIPv4Properties().Index;
                                }
                                else if (nic.Supports(NetworkInterfaceComponent.IPv6))
                                    if (nic.GetIPProperties().GetIPv6Properties() != null)
                                        NICs[nic.Id].Index = nic.GetIPProperties().GetIPv6Properties().Index;

                                // unicasts
                                if (adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index) != null)
                                    foreach (Iphlpapi.Adapter.UnicastAddress ip in adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).UnicastAddresses)
                                        if (IPAddress.Parse(ip.Address).AddressFamily == AddressFamily.InterNetwork)
                                            NICs[nic.Id].IPv4Address.Add(new IPHostAddress(ip.Address, IP.PrefixToMask(ip.Prefix)));
                                        else if (IPAddress.Parse(ip.Address).AddressFamily == AddressFamily.InterNetworkV6)
                                            NICs[nic.Id].IPv6Address.Add(ip.Address, ip.Prefix.ToString());

                                // interface metric
                                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                                    NICs[nic.Id].GetInterfaceMetric();
                                else if (adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index) != null)
                                    NICs[nic.Id].InterfaceMetric = (ushort)adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).IPv4InterfaceMetric;

                                // default gateways
                                foreach (Iphlpapi.Route route in routes.Where(i =>
                                    IPAddress.Parse(i.Destination).GetAddressBytes().Max() == 0 &&
                                    (i.IPVersion == 4 ? IPAddress.Parse(i.Prefix).GetAddressBytes().Max() == 0 : int.Parse(i.Prefix) == 0) &&
                                    i.InterfaceIndex == NICs[nic.Id].Index))
                                    if (route.IPVersion == 4)
                                        NICs[nic.Id].IPv4Gateway.Add(new IPGatewayAddress(route.Gateway, route.Metric));
                                    else
                                        NICs[nic.Id].IPv6Gateway.Add(new IPGatewayAddress(route.Gateway, route.Metric));
                                
                                List<IPGatewayAddress> gateways = NICs[nic.Id].IPv4Gateway;
                                gateways.Sort((x, y) => x.GatewayMetric.CompareTo(y.GatewayMetric));
                                if (gateways.Any((i) => !IPAddress.Parse(i.Address).Equals(IPAddress.Any)))
                                {
                                    if (gateways.Any(i => NICs[nic.Id].IPv4Address.Any(j => IP.CheckIfSameNetwork(i.Address, j.Address, j.Subnet))))
                                    {
                                        NICs[nic.Id].DefaultIPv4Gateway = gateways.Find(i => NICs[nic.Id].IPv4Address.Any(j => IP.CheckIfSameNetwork(i.Address, j.Address, j.Subnet))).Address;
                                        NICs[nic.Id].LocalIPv4Exit = NICs[nic.Id].IPv4Address.Find(i => IP.CheckIfSameNetwork(NICs[nic.Id].DefaultIPv4Gateway, i.Address, i.Subnet)).Address;
                                    }
                                }
                                else
                                    if (NICs[nic.Id].IPv4Address.Count > 0)
                                        NICs[nic.Id].LocalIPv4Exit = NICs[nic.Id].IPv4Address[0].Address;
                                
                                
                                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\NetBT\Parameters\Interfaces\Tcpip_" + nic.Id, true);
                                if (key != null)
                                {
                                    NICs[nic.Id].NetbiosEnabled = (Netbios)Convert.ToInt32(key.GetValue("NetbiosOptions"));
                                    key.Close();
                                }

                                key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + nic.Id, true);
                                if (key != null && adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index) != null)
                                {
                                    NICs[nic.Id].Dhcpv4Enabled = (Dhcp)(adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).DhcpEnabled ? 1 : 0) + (((string)key.GetValue("NameServer", "") == "") & adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).DhcpEnabled ? 1 : 0);
                                    key.Close();
                                }

                                if (adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index) != null)
                                {
                                    NICs[nic.Id].DhcpServer = adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).Dhcpv4Server;
                                    NICs[nic.Id].Dhcpv6Server = adapters.Find(i => i.InterfaceIndex == NICs[nic.Id].Index).Dhcpv6Server;
                                }

                                foreach (string line in ipv4Mtus)
                                    if (Regex.IsMatch(line, @"\s*" + nic.Name + @"\s*$"))
                                        NICs[nic.Id].IPv4Mtu = Regex.Replace(line, @"^\s*(\d+)\s+.+$", "$1");
                                foreach (string line in ipv6Mtus)
                                    if (Regex.IsMatch(line, @"\s*" + nic.Name + @"\s*$"))
                                        NICs[nic.Id].IPv6Mtu = Regex.Replace(line, @"^\s*(\d+)\s+.+$", "$1");

                                //heavily unreliable
                                //foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                                //{
                                //    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                //        NICs[nic.Id].IPv4Address.Add(new IPHostAddress(ip.Address.ToString(), ip.IPv4Mask.ToString()));
                                //    else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                                //        NICs[nic.Id].IPv4Address.Add(new IPHostAddress(ip.Address.ToString(), ""));
                                //}

                                //no IPv6
                                //foreach (GatewayIPAddressInformation ip in nic.GetIPProperties().GatewayAddresses)
                                //{
                                //    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                //    {
                                //        ushort gwMetric = 0;
                                //        Iphlpapi.Route defaultRoute = routes.Find(i => 
                                //            IPAddress.Parse(i.Destination).GetAddressBytes().Max() == 0 &&
                                //            IPAddress.Parse(i.Prefix).GetAddressBytes().Max() == 0 &&
                                //            IPAddress.Parse(i.Gateway).Equals(ip.Address) &&
                                //            i.InterfaceIndex == nic.GetIPProperties().GetIPv4Properties().Index);
                                //        if (defaultRoute != null)
                                //            gwMetric = defaultRoute.Metric;
                                //        NICs[nic.Id].IPv4Gateway.Add(new IPGatewayAddress(ip.Address.ToString(), gwMetric, 0));
                                //    }   
                                //    else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                                //    {
                                //        ushort gwMetric = 0;
                                //        Iphlpapi.Route defaultRoute = routes.Find(i =>
                                //            IPAddress.Parse(i.Destination).GetAddressBytes().Max() == 0 &&
                                //            int.Parse(i.Prefix) == 0 &&
                                //            IPAddress.Parse(i.Gateway).Equals(ip.Address) &&
                                //            i.InterfaceIndex == nic.GetIPProperties().GetIPv6Properties().Index);
                                //        if (defaultRoute != null)
                                //            gwMetric = defaultRoute.Metric;
                                //        NICs[nic.Id].IPv6Gateway.Add(new IPGatewayAddress(ip.Address.ToString(), gwMetric, 0));
                                //    }
                                //}

                                foreach (IPAddress ip in nic.GetIPProperties().DnsAddresses)
                                {
                                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                                        NICs[nic.Id].IPv4DnsServer.Add(ip.ToString());
                                    else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                                        NICs[nic.Id].IPv6DnsServer.Add(ip.ToString());
                                }
                                foreach (IPAddress ip in nic.GetIPProperties().DhcpServerAddresses)
                                {
                                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                                        NICs[nic.Id].DhcpServer = ip.ToString();
                                    else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                                        NICs[nic.Id].Dhcpv6Server = ip.ToString();
                                }

                                // PPP missing on Vista and above
                                //foreach (ManagementObject obj in Win32_NetworkAdapterConfiguration_Items)
                                //    if (((string)obj["MACAddress"] == NICs[nic.Id].Mac) && ((bool)obj["IPEnabled"] == true))
                                //    {
                                //        NICs[nic.Id].InterfaceMetric = (ushort)((uint)obj["IPConnectionMetric"]);
                                //        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\NetBT\Parameters\Interfaces\Tcpip_" + nic.Id, true);
                                //        //NICs[nic.Id].NetbiosEnabled = Convert.ToBoolean((uint)obj["TcpipNetbiosOptions"]);
                                //        NICs[nic.Id].NetbiosEnabled = Convert.ToInt32(key.GetValue("NetbiosOptions"));
                                //        key.Close();
                                //        key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\"+nic.Id, true);
                                //        NICs[nic.Id].DhcpEnabled = ((bool)obj["DHCPEnabled"] ? 1 : 0) + (((string)key.GetValue("NameServer", "") == "") & (bool)obj["DHCPEnabled"] ? 1 : 0);
                                //        key.Close();
                                //        NICs[nic.Id].IPv4DhcpServer = (string)obj["DHCPServer"];
                                //        string[] IPAddresses = (string[])obj["IPAddress"];
                                //        string[] IPSubnets = (string[])obj["IPSubnet"];
                                //        string[] DefaultIPGateways = (string[])obj["DefaultIPGateway"];
                                //        UInt16[] GatewayCostMetrics = (UInt16[])obj["GatewayCostMetric"];
                                //        string[] DNSServerSearchOrders = (string[])obj["DNSServerSearchOrder"];
                                //        if (IPAddresses != null)
                                //        {
                                //            for (int i = 0; i < IPAddresses.Length; i++)
                                //            {
                                //                IPAddress.TryParse(IPAddresses[i], out IPAddress);
                                //                if (IPAddress.AddressFamily == AddressFamily.InterNetwork)
                                //                {
                                //                    if (IPAddresses[i] != IPSubnets[i])
                                //                        NICs[nic.Id].IPv4Address.Add(new IPHostAddress(IPAddresses[i], IPSubnets[i]));
                                //                    else
                                //                        NICs[nic.Id].IPv4Address.Add(new IPHostAddress(IPAddresses[i], "0.0.0.0")); // WMI IPv4 mask is bugged when it is 0.0.0.0 and is equal to the equivalent IPAddress entry
                                //                }
                                //                else if (IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                //                {
                                //                    NICs[nic.Id].IPv6Address.Add(IPAddresses[i], IPSubnets[i]);
                                //                }
                                //            }
                                //        }
                                //        if (DefaultIPGateways != null)
                                //        {
                                //            for (int i = 0; i < DefaultIPGateways.Length; i++)
                                //            {
                                //                IPAddress.TryParse(DefaultIPGateways[i], out IPAddress);
                                //                if (IPAddress.AddressFamily == AddressFamily.InterNetwork)
                                //                {
                                //                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0) // Vista
                                //                        NICs[nic.Id].IPv4Gateway.Add(new IPGatewayAddress(DefaultIPGateways[i], GatewayCostMetrics[i], GatewayCostMetrics[i]));
                                //                    else
                                //                        NICs[nic.Id].IPv4Gateway.Add(new IPGatewayAddress(DefaultIPGateways[i], GatewayCostMetrics[i], (ushort)(NICs[nic.Id].InterfaceMetric + GatewayCostMetrics[i])));
                                //                }   
                                //                else if (IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                //                {
                                //                    if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                                //                        NICs[nic.Id].IPv6Gateway.Add(new IPGatewayAddress(DefaultIPGateways[i], GatewayCostMetrics[i], GatewayCostMetrics[i]));
                                //                    else
                                //                        NICs[nic.Id].IPv6Gateway.Add(new IPGatewayAddress(DefaultIPGateways[i], GatewayCostMetrics[i], (ushort)(NICs[nic.Id].InterfaceMetric + GatewayCostMetrics[i])));
                                //                }
                                //            }
                                //        }
                                //        // only IPv4
                                //        //if (DNSServerSearchOrders != null)
                                //        //{
                                //        //    for (int i = 0; i < DNSServerSearchOrders.Length; i++)
                                //        //    {
                                //        //        IPAddress.TryParse(DNSServerSearchOrders[i], out IPAddress);
                                //        //        if (IPAddress.AddressFamily == AddressFamily.InterNetwork)
                                //        //            NICs[nic.Id].IPv4DnsServer.Add(DNSServerSearchOrders[i]);
                                //        //        else if (IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                //        //            NICs[nic.Id].IPv6DnsServer.Add(DNSServerSearchOrders[i]);
                                //        //    }
                                //        //}
                                //    }
                                process.StartInfo.Arguments = "int ipv6 show interface " + NICs[nic.Id].Index;
                                process.Start();
                                process.WaitForExit();
                                stdo = process.StandardOutput;
                                netsh = stdo.ReadToEnd().Split("\r'\n".ToCharArray());
                                foreach (string line in netsh)
                                {
                                    if (Regex.IsMatch(line, @"Router Discovery", RegexOptions.IgnoreCase))
                                        if (Regex.IsMatch(line, @"enabled|dhcp", RegexOptions.IgnoreCase))
                                            NICs[nic.Id].IPv6RouterDiscoveryEnabled = true;
                                }

                            }
            }
            catch (Exception e) 
            { 
                if (ExceptionThrown != null)
                    ExceptionThrown(null, new ExceptionEventArgs(e));
            }
            return NICs;
        }

        public void GetStatistics()
        {
            try
            {
                IPv4InterfaceStatistics stats = networkInterface.GetIPv4Statistics(); // accumulates both IPv4 and IPv6 traffic
                IPv4InSpeed = stats.BytesReceived - IPv4BytesReceived;
                IPv4InSpeedAvg10 = IPv4InSpeedAvg10 * 9 / 10 + IPv4InSpeed * 1 / 10;
                IPv4InSpeedAvg20 = IPv4InSpeedAvg20 * 19 / 20 + IPv4InSpeed * 1 / 20;
                IPv4InSpeedAvg40 = IPv4InSpeedAvg40 * 39 / 40 + IPv4InSpeed * 1 / 40;
                IPv4OutSpeed = stats.BytesSent - IPv4BytesSent;
                IPv4OutSpeedAvg10 = IPv4OutSpeedAvg10 * 9 / 10 + IPv4OutSpeed * 1 / 10;
                IPv4OutSpeedAvg20 = IPv4OutSpeedAvg20 * 19 / 20 + IPv4OutSpeed * 1 / 20;
                IPv4OutSpeedAvg40 = IPv4OutSpeedAvg40 * 39 / 40 + IPv4OutSpeed * 1 / 40;
                IPv4BytesReceived = stats.BytesReceived;
                IPv4BytesSent = stats.BytesSent;
            }
            catch 
            {
                IPv4InSpeed = 0;
                IPv4InSpeedAvg10 = IPv4InSpeedAvg10 * 9 / 10 + IPv4InSpeed * 1 / 10;
                IPv4InSpeedAvg20 = IPv4InSpeedAvg20 * 19 / 20 + IPv4InSpeed * 1 / 20;
                IPv4InSpeedAvg40 = IPv4InSpeedAvg40 * 39 / 40 + IPv4InSpeed * 1 / 40;
                IPv4OutSpeed = 0;
                IPv4OutSpeedAvg10 = IPv4OutSpeedAvg10 * 9 / 10 + IPv4OutSpeed * 1 / 10;
                IPv4OutSpeedAvg20 = IPv4OutSpeedAvg20 * 19 / 20 + IPv4OutSpeed * 1 / 20;
                IPv4OutSpeedAvg40 = IPv4OutSpeedAvg40 * 39 / 40 + IPv4OutSpeed * 1 / 40;
            }
        }

        public void GetPublicIP()
        {
            string host = "network-manager-whatsmyip.appspot.com";
            IPAddress hostIP;
            // IPv4
            try
            {
                if (IPv4Gateway.Count == 0)
                    throw new Exception("No gateway");
                if (LocalIPv4Exit == null)
                    throw new Exception("Unreachable gateway");
                // we manually send DNS packet to make sure it goes through the tested interface in case the lowest metric default gateway is dead
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(IPAddress.Parse(LocalIPv4Exit), 0));
                socket.Connect(IPAddress.Parse("8.8.8.8"), 53);
                byte[] buffer = new byte[host.Length + 18];
                // network byte order
                BitConverter.GetBytes((ushort)0x1234).CopyTo(buffer, 0);
                BitConverter.GetBytes((ushort)0x0001).CopyTo(buffer, 2);
                BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, 4);
                int pRR = 12;
                string[] nodes = host.Split('.');
                for (int i = 0; i < nodes.Count(); i++)
                {
                    buffer[pRR] = (byte)nodes[i].Length;
                    Encoding.ASCII.GetBytes(nodes[i]).CopyTo(buffer, pRR + 1);
                    pRR += nodes[i].Length + 1;
                }
                pRR++;
                BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, pRR);
                BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, pRR + 2);
                socket.Send(buffer);
                buffer = new byte[1000];
                socket.ReceiveTimeout = 5000;
                int size = socket.Receive(buffer);
                socket.Close();
                ushort dnsFlags = (ushort)((buffer[2] << 8) | (buffer[3]));
                if (((dnsFlags & 0x8000) != 0x8000) || ((dnsFlags & 0xf) != 0))
                    throw new Exception("DNS flag error:" + dnsFlags.ToString());
                hostIP = new IPAddress(buffer.Skip(size - 4).Take(4).ToArray());
                // we manually send HTTP Get packet to ensure IPv4 reply and to make sure it goes through the tested interface
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // HACK: sometimes socket bind is ignored (???)
                socket.Bind(new IPEndPoint(IPAddress.Parse(LocalIPv4Exit), 0));
                socket.Connect(hostIP, 80);
                string HTTPGet = "GET / HTTP/1.1\r\n";
                HTTPGet += "User-Agent: " + Headers.DefaultUserAgent + "\r\n";
                HTTPGet += "Host: " + host + "\r\n";
                HTTPGet += "Cache-Control: no-cache\r\n\r\n";
                socket.SendTimeout = 5000;
                socket.Send(Encoding.ASCII.GetBytes(HTTPGet));
                buffer = new byte[1000];
                socket.ReceiveTimeout = 5000;
                size = socket.Receive(buffer);
                socket.Close();
                string HTTPResponse = Encoding.ASCII.GetString(buffer, 0, size);
                if (Regex.IsMatch(HTTPResponse, @"^HTTP/1\.1 200 OK"))
                    PublicIPv4 = Regex.Replace(HTTPResponse, @"^.*\s([^\s]*)$", "$1", RegexOptions.Singleline);
                else
                    throw new Exception(Regex.Replace(HTTPResponse, @"^([^\r\n]*)\r\n.*$", "$1", RegexOptions.Singleline));
            }
            catch (SocketException e)
            {
                if (ExceptionThrown != null)
                    ExceptionThrown(null, new ExceptionEventArgs(e));
                if (e.SocketErrorCode == SocketError.TimedOut)
                    PublicIPv4 = "Timeout";
                else
                    PublicIPv4 = "None found";
            }
            catch (Exception e)
            {
                if (ExceptionThrown != null)
                    ExceptionThrown(null, new ExceptionEventArgs(e));
                PublicIPv4 = e.Message;
            }
            if (PublicIPv4Changed != null)
                PublicIPv4Changed(this, new TextEventArgs(PublicIPv4));

            // IPv6
            //if (IPv6Gateway.Count == 0)
            //{
            //    PublicIPv6 = "No gateway";
            //    return;
            //}
            if (IPv6Address.Global.Count == 0)
            {
                PublicIPv6 = "No global IP";
                return;
            }
            foreach (IPHostAddress localAddress in IPv6Address.Global)
            {
                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(new IPEndPoint(IPAddress.Parse(localAddress.Address), 0));
                    socket.Connect(IPAddress.Parse("2001:4860:4860::8888"), 53);
                    byte[] buffer = new byte[host.Length + 18];
                    //network byte order
                    BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, 0);
                    BitConverter.GetBytes((ushort)0x0001).CopyTo(buffer, 2);
                    BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, 4);
                    int pRR = 12;
                    string[] nodes = host.Split('.');
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        buffer[pRR] = (byte)nodes[i].Length;
                        Encoding.ASCII.GetBytes(nodes[i]).CopyTo(buffer, pRR + 1);
                        pRR += nodes[i].Length + 1;
                    }
                    pRR++;
                    BitConverter.GetBytes((ushort)0x1c00).CopyTo(buffer, pRR);
                    BitConverter.GetBytes((ushort)0x0100).CopyTo(buffer, pRR + 2);
                    socket.Send(buffer);
                    buffer = new byte[1000];
                    socket.ReceiveTimeout = 5000;
                    int size = socket.Receive(buffer);
                    socket.Close();
                    ushort dnsFlags = (ushort)((buffer[2] << 8) | (buffer[3]));
                    if (((dnsFlags & 0x8000) != 0x8000) || ((dnsFlags & 0xf) != 0))
                        throw new Exception("DNS flag error:" + dnsFlags.ToString());
                    hostIP = new IPAddress(buffer.Skip(size - 16).Take(16).ToArray());
                    //we manually send HTTP Get packet to ensure IPv6 reply and to make sure it goes through the tested interface
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(IPAddress.Parse(localAddress.Address), 0));
                    socket.Connect(hostIP, 80);
                    string HTTPGet = "GET / HTTP/1.1\r\n";
                    HTTPGet += "User-Agent: " + Headers.DefaultUserAgent + "\r\n";
                    HTTPGet += "Host: " + host + "\r\n";
                    HTTPGet += "Cache-Control: no-cache\r\n\r\n";
                    socket.SendTimeout = 5000;
                    socket.Send(Encoding.ASCII.GetBytes(HTTPGet));
                    buffer = new byte[1000];
                    socket.ReceiveTimeout = 5000;
                    size = socket.Receive(buffer);
                    socket.Close();
                    string HTTPResponse = Encoding.ASCII.GetString(buffer, 0, size);
                    if (Regex.IsMatch(HTTPResponse, @"^HTTP/1\.1 200 OK"))
                        PublicIPv6 = Regex.Replace(HTTPResponse, @"^.*\s([^\s]*)$", "$1", RegexOptions.Singleline);
                    else
                        throw new Exception(Regex.Replace(HTTPResponse, @"^([^\r\n]*)\r\n.*$", "$1", RegexOptions.Singleline));
                    break;
                }
                catch (SocketException e)
                {
                    if (ExceptionThrown != null)
                        ExceptionThrown(null, new ExceptionEventArgs(e));
                    if (e.SocketErrorCode == SocketError.TimedOut)
                        PublicIPv6 = "Timeout";
                    else
                        PublicIPv6 = "None found";
                }
                catch (Exception e)
                {
                    if (ExceptionThrown != null)
                        ExceptionThrown(null, new ExceptionEventArgs(e));
                    PublicIPv6 = e.Message;
                }
                if (PublicIPv6Changed != null)
                    PublicIPv6Changed(this, new TextEventArgs(PublicIPv6));
            }
        }

        public static string GetInternetInterface()
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(IPAddress.Parse("8.8.8.8"), 53);
                if (InternetInterfaceChanged != null)
                    InternetInterfaceChanged(null, new TextEventArgs(((IPEndPoint)socket.LocalEndPoint).Address.ToString()));
                return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
            }
            catch (Exception e)
            {
                if (ExceptionThrown != null)
                    ExceptionThrown(null, new ExceptionEventArgs(e));
                if (InternetInterfaceChanged != null)
                    InternetInterfaceChanged(null, new TextEventArgs("0.0.0.0"));
                return "0.0.0.0";
            }
        }

        public static string CheckIfIPv4Used(string ip, string excludedGuid = "")
        {
            IPAddress ipVersion;
            IPAddress.TryParse(ip, out ipVersion);
            if (ipVersion.AddressFamily == AddressFamily.InterNetwork)
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces", true);
                foreach (string guid in key.GetSubKeyNames())
                {
                    if (guid == excludedGuid)
                        continue;
                    RegistryKey ifKey = key.OpenSubKey(guid, true);
                    string[] ipAddress = (string[])ifKey.GetValue("IPAddress", new[] { "" });
                    string dhcpIPAddress = (string)ifKey.GetValue("DhcpIPAddress", "");
                    int enableDhcp = (int)ifKey.GetValue("EnableDHCP", 0);
                    if (ipAddress.Contains(ip) && enableDhcp == 0 ||
                        Regex.IsMatch(dhcpIPAddress, @"(^|\s)" + ip + @"(\s|$)") && enableDhcp == 1) // duplicate found
                    {
                        RegistryKey nameKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Network\{4D36E972-E325-11CE-BFC1-08002BE10318}\" + guid + @"\Connection");
                        string name = (string)nameKey.GetValue("Name", "");
                        nameKey.Close();
                        ushort status = (ushort)GetAdapterStatus(name);
                        if (status == 1 || status == 2 || status == 3 || status == 8 || status == 9) // online
                            return name;
                        else if (enableDhcp == 0) // offline, only if static config
                        {
                            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                            "The IP address " + ip + " is used on another disconnected network adapter (" + name + ") !\n\n" +
                            "Do you want to remove the IP configuration from the disconnected network adapter and continue ?",
                            "IP Duplication", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2, System.Windows.Forms.MessageBoxOptions.ServiceNotification);
                            if (result == System.Windows.Forms.DialogResult.No)
                                return name;
                            ipAddress = ipAddress.Where((v, i) => { return v != ip && v != null; }).ToArray();
                            if (ipAddress.Length == 0)
                                ipAddress = ipAddress.Concat(new[] { "0.0.0.0" }).ToArray();
                            ifKey.SetValue("IPAddress", ipAddress, RegistryValueKind.MultiString);
                            if (ipAddress.Length == 1 &&
                                ipAddress[0] == "0.0.0.0")
                                ifKey.SetValue("EnableDHCP", 1, RegistryValueKind.DWord);
                            ifKey.Close();
                        }
                    }
                }
                key.Close();
            }
            return null;
        }

        public static Status GetAdapterStatus(string friendlyName)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); return 0; }
            foreach (ManagementObject obj in items)
                if ((string)obj["NetConnectionID"] == friendlyName)
                    return (Status)(ushort)obj["NetConnectionStatus"];
            return 0;
        }

        /// <summary>
        /// Checks if the default gateway replies to an ARP request
        /// </summary>
        /// <returns></returns>
        public bool CheckDefaultIPv4Gateway()
        {
            // TODO: default gateway check needs testing on multi default gateway interfaces
            new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(1000);
                    if (DefaultIPv4Gateway != null)
                        DefaultIPv4GatewayMac = Iphlpapi.GetMacAddress(DefaultIPv4Gateway, LocalIPv4Exit);
                    if (DefaultIPv4GatewayChecked != null)
                        DefaultIPv4GatewayChecked(this, null);
                })).Start();
            return true;
        }
        // Configuring fuctions
        // ======================

        /// <summary>
        /// Enables static TCP/IPv4 addressing for the target network adapter clearing any existing IPv4 address configurations. As a result, DHCP for this network adapter is disabled. 
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="ip">The IPv4 address that will replace an exisiting configurations</param>
        /// <param name="mask">The IPv4 mask that will replace an exisiting configurations</param>
        public static void ClearIPv4Addresses(string mac, string ip, string mask)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                    obj.InvokeMethod("EnableStatic", new object[] { new string[] { ip }, new string[] { mask } });
        }

        /// <summary>
        /// IPv4 and IPv6
        /// </summary>
        public void ClearGateways()
        {
            ClearGateways(Index);
        }

        public static void ClearGateways(int index)
        {
            List<Iphlpapi.Route> routes = Iphlpapi.GetRoutes(Iphlpapi.FAMILY.AF_UNSPEC);
            foreach (Iphlpapi.Route route in routes)
                if (route.InterfaceIndex == index &&
                    IPAddress.Parse(route.Destination).GetAddressBytes().Max() == 0 &&
                    (IPAddress.Parse(route.Destination).AddressFamily == AddressFamily.InterNetwork && IPAddress.Parse(route.Prefix).GetAddressBytes().Max() == 0 ||
                    IPAddress.Parse(route.Destination).AddressFamily == AddressFamily.InterNetworkV6 && int.Parse(route.Prefix) == 0))
                    Iphlpapi.DeleteRoute(route.Destination, route.Prefix, route.Gateway, index.ToString());
        }

        /// <summary>
        /// IPv4 and IPv6
        /// IT APPEARS THAT IS NOT WORKING
        /// </summary>
        /// <param name="mac"></param>
        public static void ClearGateways(string mac)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                    obj.InvokeMethod("SetGateways", new object[] { new string[0], new short[0] });
        }

        /// <summary>
        /// IPv4 only
        /// </summary>
        public void ClearIPv4DnsServers()
        {
            ClearIPv4DnsServers(Mac);
        }

        /// <summary>
        /// IPv4 only
        /// </summary>
        /// <param name="mac"></param>
        public static void ClearIPv4DnsServers(string mac)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                    obj.InvokeMethod("SetDNSServerSearchOrder", new object[] { new string[0] });
        }

        // IPv4
        // ====
        public void SetDhcp(int enabled)
        {
            SetDhcp(Name, enabled);
        }

        public static void SetDhcp(string name, int enabled)
        {
            Process netsh = Netsh();
            if (enabled > 0)
            {
                netsh.StartInfo.Arguments = "int ip set address dhcp name=\"" + name + "\"";
                netsh.Start();
                netsh.WaitForExit();
            }
            if (enabled > 1)
            {
                netsh.StartInfo.Arguments = "int ip set dns dhcp name=\"" + name + "\"";
                netsh.Start();
                netsh.WaitForExit();
            }
        }

        public void SetIPv4Address(string ip, string mask)
        {
            SetIPv4Address(Name, ip, mask);
        }

        public static void SetIPv4Address(string name, string ip, string mask)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip set address static name=\"" + name + "\" " + ip + " " + mask;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void AddIPv4Address(string ip, string mask)
        {
            AddIPv4Address(Name, ip, mask);
        }

        public static void AddIPv4Address(string name, string ip, string mask)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip add address name=\"" + name + "\" " + ip + " " + mask;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void DeleteIPv4Address(string ip, string mask)
        {
            DeleteIPv4Address(Name, ip, mask);
        }

        public static void DeleteIPv4Address(string name, string ip, string mask)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip delete address name=\"" + name + "\" " + ip + " " + mask;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void AddIPv4Gateway(string gateway, string metric)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                AddIPv4Gateway(Mac, gateway, metric);
            else
                AddIPv4Gateway(Index, gateway, metric);
        }

        /// <summary>
        /// XP only
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void AddIPv4Gateway(string mac, string gateway, string metric)
        {
            if (int.Parse(metric) == 0)
                metric = "1";
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                {
                    string[] gateways = (string[])obj["DefaultIPGateway"];
                    ushort[] metrics = (ushort[])obj["GatewayCostMetric"];
                    if (gateways != null)
                        gateways = gateways.Concat(new[] { gateway }).ToArray();
                    else
                        gateways = new string[] { gateway };
                    if (metrics != null)
                        metrics = metrics.Concat(new[] { UInt16.Parse(metric) }).ToArray();
                    else
                        metrics = new ushort[] { UInt16.Parse(metric) };
                    obj.InvokeMethod("SetGateways", new object[] { gateways, metrics });
                }
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void AddIPv4Gateway(int index, string gateway, string metric)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip add route interface=" + index + " 0.0.0.0/0 " + gateway + " metric=" + metric;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void EditIPv4Gateway(string gateway, string metric)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                EditIPv4Gateway(Mac, gateway, metric);
            else
                EditIPv4Gateway(Index, gateway, metric);
        }

        /// <summary>
        /// XP only
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void EditIPv4Gateway(string mac, string gateway, string metric)
        {
            if (int.Parse(metric) == 0)
                metric = "1";
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                {
                    string[] gateways = (string[])obj["DefaultIPGateway"];
                    ushort[] metrics = (ushort[])obj["GatewayCostMetric"];
                    if (gateways != null &&
                        !gateways.Contains(gateway))
                        gateways = gateways.Concat(new[] { gateway }).ToArray();
                    else
                        gateways = new string[] { gateway };
                    if (metrics != null)
                        metrics[gateways.ToList().IndexOf(gateway)] = UInt16.Parse(metric);
                    else
                        metrics = new ushort[] { UInt16.Parse(metric) };
                    obj.InvokeMethod("SetGateways", new object[] { gateways, metrics });
                }
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void EditIPv4Gateway(int index, string gateway, string metric)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip set route interface=" + index + " 0.0.0.0/0 " + gateway + " metric=" + metric;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void DeleteIPv4Gateway(string gateway)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                DeleteIPv4Gateway(Mac, gateway);
            else
                DeleteIPv4Gateway(Index, gateway);
        }

        /// <summary>
        /// XP only
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void DeleteIPv4Gateway(string mac, string gateway)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                {
                    string[] gateways = (string[])obj["DefaultIPGateway"];
                    ushort[] metrics = (ushort[])obj["GatewayCostMetric"];
                    if (gateways != null &&
                        gateways.Contains(gateway) &&
                        metrics != null)
                    {
                        int index = gateways.ToList().IndexOf(gateway);
                        metrics = metrics.Take(index).Concat(metrics.Skip(index + 1)).ToArray();
                        gateways = gateways.Where((i) => i != gateway).ToArray();
                    }
                    obj.InvokeMethod("SetGateways", new object[] { gateways, metrics });
                }
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void DeleteIPv4Gateway(int index, string gateway)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip delete route interface=" + index + " 0.0.0.0/0 " + gateway;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void SetIPv4DnsServer(string dns)
        {
            SetIPv4DnsServer(Name, dns);
        }

        public static void SetIPv4DnsServer(string name, string dns)
        {
            Process netsh = Netsh();
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                netsh.StartInfo.Arguments = "int ip set dns static name=\"" + name + "\" " + dns;
            else
                netsh.StartInfo.Arguments = "int ip set dns static validate=no name=\"" + name + "\" " + dns;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void AddIPv4DnsServer(string dns)
        {
            AddIPv4DnsServer(Name, dns);
        }

        public static void AddIPv4DnsServer(string name, string dns)
        {
            Process netsh = Netsh();
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                netsh.StartInfo.Arguments = "int ip add dns name=\"" + name + "\" " + dns;
            else
                netsh.StartInfo.Arguments = "int ip add dns validate=no name=\"" + name + "\" " + dns;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void DeleteIPv4DnsServer(string dns)
        {
            DeleteIPv4DnsServer(Name, dns);
        }

        public static void DeleteIPv4DnsServer(string name, string dns)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip delete dns name=\"" + name + "\" " + dns;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void SetNetBios(bool enabled)
        {
            SetNetBios(Guid, enabled);
        }

        public static void SetNetBios(string guid, bool enabled)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\NetBT\Parameters\Interfaces\Tcpip_" + guid, true);
            key.SetValue("NetbiosOptions", enabled ? 1 : 2, RegistryValueKind.DWord);
            key.Close();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="mtu"></param>
        public void SetIPv4Mtu(string mtu)
        {
            SetIPv4Mtu(Index, mtu);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="mtu"></param>
        public static void SetIPv4Mtu(int index, string mtu)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip set int interface=" + index + " mtu=" + mtu;
            netsh.Start();
            netsh.WaitForExit();
        }

        // IPv6
        // ====

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="prefix"></param>
        public void AddIPv6Address(string ip, string prefix)
        {
            AddIPv6Address(Index, ip, prefix);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ip"></param>
        /// <param name="prefix"></param>
        public static void AddIPv6Address(int index, string ip, string prefix)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 add address interface=" + index + " " + ip + "/" + prefix;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="ip"></param>
        public void DeleteIPv6Address(string ip)
        {
            DeleteIPv6Address(Index, ip);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ip"></param>
        public static void DeleteIPv6Address(int index, string ip)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 delete address interface=" + index + " " + ip;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public void AddIPv6Gateway(string gateway, string metric)
        {
            AddIPv6Gateway(Index, gateway, metric);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void AddIPv6Gateway(int index, string gateway, string metric)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 add route interface=" + index + " ::/0 " + gateway + " metric=" + metric;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public void EditIPv6Gateway(string gateway, string metric)
        {
            EditIPv6Gateway(Index, gateway, metric);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        /// <param name="metric"></param>
        public static void EditIPv6Gateway(int index, string gateway, string metric)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 edit route interface=" + index + " ::/0 " + gateway + " metric=" + metric;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="gateway"></param>
        public void DeleteIPv6Gateway(string gateway)
        {
            DeleteIPv6Gateway(Index, gateway);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gateway"></param>
        public static void DeleteIPv6Gateway(int index, string gateway)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 delete route interface=" + index + " ::/0 " + gateway;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="dns"></param>
        public void AddIPv6DnsServer(string dns)
        {
            AddIPv6DnsServer(Index, dns);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dns"></param>
        public static void AddIPv6DnsServer(int index, string dns)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 add dns validate=no name=" + index + " " + dns;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="dns"></param>
        public void DeleteIPv6DnsServer(string dns)
        {
            DeleteIPv6DnsServer(Index, dns);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dns"></param>
        public static void DeleteIPv6DnsServer(int index, string dns)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 delete dns validate=no name=" + index + " " + dns;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRouterDiscovery(bool enabled)
        {
            SetRouterDiscovery(Index, enabled);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="enabled"></param>
        public static void SetRouterDiscovery(int index, bool enabled)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 set int interface=" + index + " routerdiscovery=" + (enabled ? "enabled" : "disabled");
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="mtu"></param>
        public void SetIPv6Mtu(string mtu)
        {
            SetIPv6Mtu(Index, mtu);
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="mtu"></param>
        public static void SetIPv6Mtu(int index, string mtu)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ipv6 set int interface=" + index + " mtu=" + mtu;
            netsh.Start();
            netsh.WaitForExit();
        }

        public void SetInterfaceMetric(string metric)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                SetInterfaceMetric(Mac, metric);
            else
                SetInterfaceMetric(Index, metric);
        }

        /// <summary>
        /// XP only
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="metric"></param>
        public static void SetInterfaceMetric(string mac, string metric)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((string)obj["MACAddress"] == mac && (bool)obj["IPEnabled"] == true)
                    obj.InvokeMethod("SetIPConnectionMetric", new[] { metric });
        }

        /// <summary>
        /// Vista and above only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="metric"></param>
        public static void SetInterfaceMetric(int index, string metric)
        {
            Process netsh = Netsh();
            netsh.StartInfo.Arguments = "int ip set int interface=" + index + " metric=" + metric;
            netsh.Start();
            netsh.WaitForExit();
        }

        /// <summary>
        /// Win32_NetworkAdapterConfiguration<para/>
        /// XP OK<para/>
        /// Vista/7 without PPP
        /// </summary>
        /// <returns></returns>
        public void GetInterfaceMetric()
        {
            if (IPv4Address.Count > 0 && !IPAddress.Parse(IPv4Address[0].Address).Equals(IPAddress.Parse("0.0.0.0")))
                InterfaceMetric = GetInterfaceMetric(IPv4Address[0].Address, IPv4Address[0].Subnet);
            else if (IPv6Address.All.Count > 0 && !IPAddress.Parse(IPv6Address.All[0].Address).Equals(IPAddress.Parse("::")))
                InterfaceMetric = GetInterfaceMetric(IPv6Address.All[0].Address, IPv6Address.All[0].Subnet);
        }

        /// <summary>
        /// Win32_NetworkAdapterConfiguration<para/>
        /// XP OK<para/>
        /// Vista/7 without PPP
        /// </summary>
        /// <param name="ipAddress">IPv4 or IPv6</param>
        /// <param name="subnet">Dot-decimal-notation for IPv4 (i.e. "255.255.255.0") and CIDR for IPv6 (i.e. "64")</param>
        /// <returns></returns>
        public static ushort GetInterfaceMetric(string ipAddress, string subnet)
        {
            ManagementClass wmiClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection items = null;
            try { items = wmiClass.GetInstances(); }
            catch (COMException) { System.Windows.Forms.MessageBox.Show("\"" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\" function failed this time becose WMI did not respond in time. Try again.", "WMI error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            foreach (ManagementObject obj in items)
                if ((bool)obj["IPEnabled"] == true)
                {
                    string[] IPAddresses = (string[])obj["IPAddress"];
                    string[] IPSubnets = (string[])obj["IPSubnet"];
                    if (IPAddresses.Contains(ipAddress))
                        if (IPSubnets.ElementAt(Array.IndexOf(IPAddresses, ipAddress)) == subnet)
                            return (ushort)(uint)obj["IPConnectionMetric"];
                }
            return 0;
        }

        // Data types, constants and properties
        // ====================================

        // http://www.iana.org/assignments/ipv6-address-space/ipv6-address-space.xhtml
        public class IPv6HostAddress
        {
            public List<IPHostAddress> Global = new List<IPHostAddress>();
            public List<IPHostAddress> Temporary = new List<IPHostAddress>();
            public List<IPHostAddress> LinkLocal = new List<IPHostAddress>();
            public List<IPHostAddress> SiteLocal = new List<IPHostAddress>();
            public List<IPHostAddress> UniqueLocal = new List<IPHostAddress>();
            public List<IPHostAddress> Local = new List<IPHostAddress>();

            public List<IPHostAddress> All
            {
                get
                {
                    return Global.Concat(Temporary).Concat(LinkLocal).Concat(SiteLocal).Concat(UniqueLocal).Concat(Local).ToList();
                }
            }

            public void Add(string ip, string subnet)
            {
                if (IP.CheckIfSameNetwork(ip, "fe80::", "10"))
                    LinkLocal.Add(new IPHostAddress(ip, subnet));
                else if (IP.CheckIfSameNetwork(ip, "fec0::", "10"))
                    SiteLocal.Add(new IPHostAddress(ip, subnet));
                else if (IP.CheckIfSameNetwork(ip, "fc00::", "7"))
                    UniqueLocal.Add(new IPHostAddress(ip, subnet));
                else if (IP.CheckIfSameNetwork(ip, "2000::", "3"))
                    Global.Add(new IPHostAddress(ip, subnet));
                else
                    Local.Add(new IPHostAddress(ip, subnet));
            }

            public bool Contains(string ip, string subnet = null)
            {
                if (Global.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || Global.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                if (Temporary.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || Temporary.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                if (LinkLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || LinkLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                if (SiteLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || SiteLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                if (UniqueLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || UniqueLocal.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                if (Local.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }) != null &&
                    (subnet != null || Local.Find((i) => { return IPAddress.Parse(i.Address).Equals(IPAddress.Parse(ip)); }).Subnet == subnet))
                    return true;
                return false;
            }
        }

        // IPv4 or IPv6 address and its subnet
        public class IPHostAddress
        {
            public string Address;
            public string Subnet;

            /// <summary>
            /// to make the class serializable
            /// </summary>
            public IPHostAddress()
            { }

            public IPHostAddress(string ip, string subnet)
            {
                Address = ip;
                Subnet = subnet;
            }
        }

        // IPv4 or IPv6 gateway address and its metric
        public class IPGatewayAddress
        {
            public string Address;
            public ushort GatewayMetric;

            /// <summary>
            /// to make the class serializable
            /// </summary>
            public IPGatewayAddress()
            { }

            public IPGatewayAddress(string ip, ushort gwMetric)
            {
                Address = ip;
                GatewayMetric = gwMetric;
            }
        }

        public string NetbiosEnabledString
        {
            get
            {
                return new string[] { "DHCP Enabled", "Enabled", "Disabled" }[(int)NetbiosEnabled];
            }
        }

        public string DhcpEnabledString
        {
            get
            {
                return new string[] { "Disabled", "IP Only", "IP & DNS" }[(int)Dhcpv4Enabled];
            }
        }
        
        public enum AdapterType
        {
            [Description("ASDL")]
            AsymmetricDsl = NetworkInterfaceType.AsymmetricDsl,
            [Description("ATM")]
	        Atm = NetworkInterfaceType.Atm,
            [Description("Basic ISDN")]
            BasicIsdn = NetworkInterfaceType.BasicIsdn,
            [Description("Ethernet")]
	        Ethernet = NetworkInterfaceType.Ethernet,
            [Description("Ethernet 3Mb/s")]
	        Ethernet3Megabit = NetworkInterfaceType.Ethernet3Megabit,
            [Description("Fast Ethernet 100Base-FX")]
	        FastEthernetFx = NetworkInterfaceType.FastEthernetFx,
            [Description("Fast Ethernet 100Base-T")]
	        FastEthernetT = NetworkInterfaceType.FastEthernetT,
            [Description("FDDI")]
	        Fddi = NetworkInterfaceType.Fddi,
            [Description("Modem")]
	        GenericModem = NetworkInterfaceType.GenericModem,
            [Description("Gigabit Ethernet")]
	        GigabitEthernet = NetworkInterfaceType.GigabitEthernet,
            [Description("High Performance Serial Bus")]
	        HighPerformanceSerialBus = NetworkInterfaceType.HighPerformanceSerialBus,
            [Description("IP over ATM")]
	        IPOverAtm = NetworkInterfaceType.IPOverAtm,
            [Description("ISDN")]
	        Isdn = NetworkInterfaceType.Isdn,
            [Description("Loopback")]
	        Loopback = NetworkInterfaceType.Loopback,
            [Description("MDSL")]
	        MultiRateSymmetricDsl = NetworkInterfaceType.MultiRateSymmetricDsl,
            [Description("PPP")]
	        Ppp = NetworkInterfaceType.Ppp,
            [Description("Primary rate ISDN")]
	        PrimaryIsdn = NetworkInterfaceType.PrimaryIsdn,
            [Description("RADSL")]
	        RateAdaptDsl = NetworkInterfaceType.RateAdaptDsl,
            [Description("SLIP")]
	        Slip = NetworkInterfaceType.Slip,
            [Description("SDSL")]
	        SymmetricDsl = NetworkInterfaceType.SymmetricDsl,
            [Description("Token-Ring")]
	        TokenRing = NetworkInterfaceType.TokenRing,
            [Description("Tunnel")]
	        Tunnel = NetworkInterfaceType.Tunnel,
            [Description("Unknown")]
	        Unknown = NetworkInterfaceType.Unknown,
            [Description("VDSL")]
	        VeryHighSpeedDsl = NetworkInterfaceType.VeryHighSpeedDsl,
            [Description("Wireless IEEE 802.11")]
	        Wireless80211 = NetworkInterfaceType.Wireless80211,
            [Description("WiMax")]
	        Wman = 237,
            /// <summary>
            /// GSM-based
            /// </summary>
            [Description("GSM-based")]
	        Wwanpp = 243,
            /// <summary>
            /// CDMA-based
            /// </summary>
            [Description("CDMA-based")]
	        Wwanpp2 = 244
        }

        public enum Status : ushort
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting,
            HardwareNotPresent,
            HardwareDisabled,
            HardwareMalfunction,
            MediaDisconnected,
            Authenticating,
            AuthenticationSucceeded,
            AuthenticationFailed,
            InvalidAddress,
            CredentialsRequired
        }

        public enum Dhcp
        {
            /// <summary>
            /// This value is used when setting the properties for an IP interface when the value should be unchanged.
            /// </summary>
            [Description("Unchanged")]
            Unchanged = -1,
            [Description("Disabled")]
            Disabled,
            [Description("IP only")]
            IPOnly,
            [Description("IP & DNS")]
            IPnDns
        }

        public enum Netbios
        {
            /// <summary>
            /// This value is used when setting the properties for an IP interface when the value should be unchanged.
            /// </summary>
            [Description("Unchanged")]
            Unchanged = -1,
            [Description("DHCP Enabled")]
            DhcpEnabled,
            [Description("Enabled")]
            Enabled,
            [Description("Disabled")]
            Disabled
        }

        public enum RouterDiscovery
        {
            /// <summary>
            /// This value is used when setting the properties for an IP interface when the value should be unchanged.
            /// </summary>
            [Description("Unchanged")]
            Unchanged = -1,
            [Description("Disabled")]
            Disabled,
            [Description("Enabled")]
            Enabled,
            /// <summary>
            /// Is configured based on DHCP
            /// </summary>
            [Description("DHCP Enabled")]
            Dhcp
        }
    }
}
