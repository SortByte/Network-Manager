using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using WinLib.Network;
using static WinLib.WinAPI.Rpcdce;

namespace WinLib.WinAPI
{
    public static class Iphlpapi
    {
        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR GetAdaptersInfo(IntPtr pAdapterInfo, ref uint outBufLen);
        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetInterfaceInfo(byte[] IP_INTERFACE_INFO, ref int size);
        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ERROR GetAdaptersAddresses(uint Family, uint Flags, IntPtr Reserved, IntPtr pAdapterAddresses, ref uint outBufLen);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetIfEntry(ref byte[] pIfRow);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR SendARP(in_addr DestIP, in_addr SrcIP, ref UInt64 MacAddr, ref uint PhyAddrLen);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr IcmpCreateFile();
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IcmpCloseHandle(IntPtr IcmpHandle);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint IcmpSendEcho(IntPtr IcmpHandle, in_addr DestinationAddress, byte[] RequestData, ushort RequestSize, IntPtr RequestOptions, byte[] ReplyBuffer, uint ReplySize, uint Timeout);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint IcmpSendEcho(IntPtr IcmpHandle, in_addr DestinationAddress, byte[] RequestData, ushort RequestSize, ref IP_OPTION_INFORMATION RequestOptions, byte[] ReplyBuffer, uint ReplySize, uint Timeout);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint IcmpSendEcho(IntPtr IcmpHandle, in_addr DestinationAddress, byte[] RequestData, ushort RequestSize, ref IP_OPTION_INFORMATION RequestOptions, ref ICMP_ECHO_REPLY ReplyBuffer, uint ReplySize, uint Timeout);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint IcmpSendEcho(IntPtr IcmpHandle, in_addr DestinationAddress, byte[] RequestData, ushort RequestSize, IntPtr pRequestOptions, ref ICMP_ECHO_REPLY ReplyBuffer, uint ReplySize, uint Timeout);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint FreeMibTable(IntPtr Memory);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetIpForwardTable2(FAMILY Family, out IntPtr Table);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint CreateIpForwardEntry2(IntPtr pRow);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SetIpForwardEntry2(IntPtr pRow);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint DeleteIpForwardEntry2(IntPtr pRow);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR FlushIpPathTable(FAMILY Family);

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetIpForwardTable(IntPtr pIpForwardTable, ref uint dwSize, bool bOrder);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void InitializeIpForwardEntry(out IntPtr PMIB_IPFORWARD_ROW2);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR CreateIpForwardEntry(IntPtr pRoute);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SetIpForwardEntry(IntPtr pRoute);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint DeleteIpForwardEntry(IntPtr pRoute);

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR GetExtendedTcpTable(IntPtr pTcpTable, ref int dwSize, bool bOrder, FAMILY Family, TCP_TABLE_CLASS TableClass, uint Reserved);
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ERROR GetExtendedUdpTable(IntPtr pUdpTable, ref int dwSize, bool bOrder, FAMILY Family, UDP_TABLE_CLASS TableClass, uint Reserved);

        // Route functions
        // ===============

        /// <summary>
        /// XP - IPv4 Only
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        public static List<Route> GetRoutes(FAMILY family)
        {
            List<Route> routes = new List<Route>();
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                if (family == FAMILY.AF_INET6)
                    return routes;
                uint dwSize = 0;
                IntPtr pNull = IntPtr.Zero;
                GetIpForwardTable(pNull, ref dwSize, true);
                IntPtr pIpForwardTable = Marshal.AllocHGlobal((int)dwSize);
                GetIpForwardTable(pIpForwardTable, ref dwSize, true);
                MIB_IPFORWARDTABLE table = (MIB_IPFORWARDTABLE)Marshal.PtrToStructure(pIpForwardTable, typeof(MIB_IPFORWARDTABLE));
                IntPtr pRow = pIpForwardTable + Marshal.SizeOf(table.dwNumEntries);
                for (int i = 0; i < table.dwNumEntries; i++)
                {
                    MIB_IPFORWARDROW row = (MIB_IPFORWARDROW)Marshal.PtrToStructure(pRow, typeof(MIB_IPFORWARDROW));
                    Route route = new Route();
                    route.Destination = new IPAddress(row.dwForwardDest).ToString();
                    route.Prefix = new IPAddress(row.dwForwardMask).ToString();
                    route.Gateway = new IPAddress(row.dwForwardNextHop).ToString();
                    route.InterfaceIndex = (int)row.dwForwardIfIndex;
                    route.Age = row.dwForwardAge;
                    route.Metric = (ushort)row.dwForwardMetric1;
                    route.Type = row.dwForwardType;
                    route.Protocol = row.dwForwardProto;
                    route.IPVersion = 4;
                    routes.Add(route);
                    pRow += Marshal.SizeOf(typeof(MIB_IPFORWARDROW));
                }
                Marshal.FreeHGlobal(pIpForwardTable);
            }
            else
            {
                IntPtr pTable;
                GetIpForwardTable2(family, out pTable);
                MIB_IPFORWARD_TABLE2 table = (MIB_IPFORWARD_TABLE2)Marshal.PtrToStructure(pTable, typeof(MIB_IPFORWARD_TABLE2));
                IntPtr pRow = pTable + Marshal.SizeOf(table.NumEntries) + 4;
                for (int i = 0; i < table.NumEntries; i++)
                {
                    MIB_IPFORWARD_ROW2 row = (MIB_IPFORWARD_ROW2)Marshal.PtrToStructure(pRow, typeof(MIB_IPFORWARD_ROW2));
                    Route route = new Route();
                    route.Prefix = row.DestinationPrefix.PrefixLength.ToString();
                    route.InterfaceIndex = (int)row.InterfaceIndex;
                    route.Age = row.Age;
                    route.Metric = (ushort)row.Metric;
                    if (row.DestinationPrefix.Prefix.Ipv4.sin_family == FAMILY.AF_INET)
                    {
                        route.Destination = new IPAddress(row.DestinationPrefix.Prefix.Ipv4.sin_addr.S_addr).ToString();
                        route.Gateway = new IPAddress(row.NextHop.Ipv4.sin_addr.S_addr).ToString();
                        IP.ValidateIPv4Mask(ref route.Prefix);
                        route.IPVersion = 4;
                    }
                    else
                    {
                        route.Destination = new IPAddress(row.DestinationPrefix.Prefix.Ipv6.sin6_addr.Byte).ToString();
                        route.Gateway = new IPAddress(row.NextHop.Ipv6.sin6_addr.Byte).ToString();
                        route.IPVersion = 6;
                    }
                    routes.Add(route);
                    pRow += Marshal.SizeOf(typeof(MIB_IPFORWARD_ROW2));
                }
                FreeMibTable(pTable);
            }
            return routes;
        }

        // TODO: replace CreateIpForwardEntry2 with CreateIpForwardEntry; CreateIpForwardEntry2 adds the route but is not active; same for edit
        /// <summary>
        /// XP - IPv4 Only
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="prefix"></param>
        /// <param name="gateway"></param>
        /// <param name="interfaceIndex"></param>
        /// <param name="metric"></param>
        /// <param name="type">must be set correctly for XP</param>
        /// <param name="protocol"></param>
        public static void AddRoute(string destination, string prefix, string gateway, string interfaceIndex, string metric, MIB_IPFORWARD_TYPE type = MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_INDIRECT, NL_ROUTE_PROTOCOL protocol = NL_ROUTE_PROTOCOL.MIB_IPPROTO_NETMGMT)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                MIB_IPFORWARDROW route = new MIB_IPFORWARDROW();
                route.dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                route.dwForwardMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().ToArray(), 0);
                route.dwForwardNextHop = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                route.dwForwardIfIndex = uint.Parse(interfaceIndex);
                route.dwForwardType = type;
                route.dwForwardProto = protocol;
                route.dwForwardMetric1 = uint.Parse(metric);
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route));
                Marshal.StructureToPtr(route, pRoute, false);
                CreateIpForwardEntry(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARDROW));
                Marshal.FreeHGlobal(pRoute);
            }
            else
            {
                MIB_IPFORWARD_ROW2 route2 = new MIB_IPFORWARD_ROW2();
                uint prefixLength = 0;
                if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    SOCKADDR_IN address = new SOCKADDR_IN();
                    address.sin_family = FAMILY.AF_INET;
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                    route2.DestinationPrefix.Prefix.Ipv4 = address;
                    uint ipMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().Reverse().ToArray(), 0);
                    while (ipMask > 0)
                    {
                        prefixLength++;
                        ipMask <<= 1;
                    }
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                    route2.NextHop.Ipv4 = address;
                }
                else
                {
                    SOCKADDR_IN6 address = new SOCKADDR_IN6();
                    address.sin6_family = FAMILY.AF_INET6;
                    address.sin6_addr.Byte = IPAddress.Parse(destination).GetAddressBytes().ToArray();
                    route2.DestinationPrefix.Prefix.Ipv6 = address;
                    prefixLength = uint.Parse(prefix);
                    address.sin6_addr.Byte = IPAddress.Parse(gateway).GetAddressBytes().ToArray();
                    route2.NextHop.Ipv6 = address;
                }
                route2.DestinationPrefix.PrefixLength = prefixLength;
                route2.InterfaceIndex = uint.Parse(interfaceIndex);
                route2.Metric = uint.Parse(metric);
                route2.Protocol = protocol;
                route2.PreferredLifetime = uint.MaxValue;
                route2.ValidLifetime = uint.MaxValue;
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route2));
                Marshal.StructureToPtr(route2, pRoute, false);
                CreateIpForwardEntry2(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARD_ROW2));
                Marshal.FreeHGlobal(pRoute);
            }
        }

        /// <summary>
        /// XP - IPv4 only
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="prefix"></param>
        /// <param name="gateway"></param>
        /// <param name="interfaceIndex"></param>
        /// <param name="metric"></param>
        public static void EditRoute(string destination, string prefix, string gateway, string interfaceIndex, string metric, MIB_IPFORWARD_TYPE type = MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_INDIRECT, NL_ROUTE_PROTOCOL protocol = NL_ROUTE_PROTOCOL.MIB_IPPROTO_NETMGMT)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                MIB_IPFORWARDROW route = new MIB_IPFORWARDROW();
                route.dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                route.dwForwardMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().ToArray(), 0);
                route.dwForwardPolicy = 0;
                route.dwForwardNextHop = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                route.dwForwardIfIndex = uint.Parse(interfaceIndex);
                route.dwForwardType = type;
                route.dwForwardProto = protocol;
                route.dwForwardMetric1 = uint.Parse(metric);
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route));
                Marshal.StructureToPtr(route, pRoute, false);
                SetIpForwardEntry(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARDROW));
                Marshal.FreeHGlobal(pRoute);
            }
            else
            {
                MIB_IPFORWARD_ROW2 route2 = new MIB_IPFORWARD_ROW2();
                uint prefixLength = 0;
                if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    SOCKADDR_IN address = new SOCKADDR_IN();
                    address.sin_family = FAMILY.AF_INET;
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                    route2.DestinationPrefix.Prefix.Ipv4 = address;
                    uint ipMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().Reverse().ToArray(), 0);
                    while (ipMask > 0)
                    {
                        prefixLength++;
                        ipMask <<= 1;
                    }
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                    route2.NextHop.Ipv4 = address;
                }
                else
                {
                    SOCKADDR_IN6 address = new SOCKADDR_IN6();
                    address.sin6_family = FAMILY.AF_INET6;
                    address.sin6_addr.Byte = IPAddress.Parse(destination).GetAddressBytes().ToArray();
                    route2.DestinationPrefix.Prefix.Ipv6 = address;
                    prefixLength = uint.Parse(prefix);
                    address.sin6_addr.Byte = IPAddress.Parse(gateway).GetAddressBytes().ToArray();
                    route2.NextHop.Ipv6 = address;
                }
                route2.DestinationPrefix.PrefixLength = prefixLength;
                route2.InterfaceIndex = uint.Parse(interfaceIndex);
                route2.Metric = uint.Parse(metric);
                route2.Protocol = protocol;
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route2));
                Marshal.StructureToPtr(route2, pRoute, false);
                SetIpForwardEntry2(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARD_ROW2));
                Marshal.FreeHGlobal(pRoute);
            }
        }

        /// <summary>
        /// XP - IPv4 only
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="prefix"></param>
        /// <param name="gateway"></param>
        /// <param name="interfaceIndex"></param>
        public static void DeleteRoute(string destination, string prefix, string gateway, string interfaceIndex)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                MIB_IPFORWARDROW route = new MIB_IPFORWARDROW();
                route.dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                route.dwForwardMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().ToArray(), 0);
                route.dwForwardNextHop = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                route.dwForwardIfIndex = uint.Parse(interfaceIndex);
                route.dwForwardProto = NL_ROUTE_PROTOCOL.MIB_IPPROTO_NETMGMT;
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route));
                Marshal.StructureToPtr(route, pRoute, false);
                DeleteIpForwardEntry(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARDROW));
                Marshal.FreeHGlobal(pRoute);
            }
            else
            {
                MIB_IPFORWARD_ROW2 route2 = new MIB_IPFORWARD_ROW2();
                uint prefixLength = 0;
                if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    SOCKADDR_IN address = new SOCKADDR_IN();
                    address.sin_family = FAMILY.AF_INET;
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes().ToArray(), 0);
                    route2.DestinationPrefix.Prefix.Ipv4 = address;
                    uint ipMask = BitConverter.ToUInt32(IPAddress.Parse(prefix).GetAddressBytes().Reverse().ToArray(), 0);
                    while (ipMask > 0)
                    {
                        prefixLength++;
                        ipMask <<= 1;
                    }
                    address.sin_addr.S_addr = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes().ToArray(), 0);
                    route2.NextHop.Ipv4 = address;
                }
                else
                {
                    SOCKADDR_IN6 address = new SOCKADDR_IN6();
                    address.sin6_family = FAMILY.AF_INET6;
                    address.sin6_addr.Byte = IPAddress.Parse(destination).GetAddressBytes().ToArray();
                    route2.DestinationPrefix.Prefix.Ipv6 = address;
                    prefixLength = uint.Parse(prefix);
                    address.sin6_addr.Byte = IPAddress.Parse(gateway).GetAddressBytes().ToArray();
                    route2.NextHop.Ipv6 = address;
                }
                route2.DestinationPrefix.PrefixLength = prefixLength;
                route2.InterfaceIndex = uint.Parse(interfaceIndex);
                IntPtr pRoute = Marshal.AllocHGlobal(Marshal.SizeOf(route2));
                Marshal.StructureToPtr(route2, pRoute, false);
                DeleteIpForwardEntry2(pRoute);
                Marshal.DestroyStructure(pRoute, typeof(MIB_IPFORWARD_ROW2));
                Marshal.FreeHGlobal(pRoute);
            }
        }

        // Adapters functions
        // ==================

        public static List<Adapter> GetAdapters(FAMILY family)
        {
            List<Adapter> adapters = new List<Adapter>();
            
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
            {
                uint outBufLen = 0;
                ERROR error = GetAdaptersInfo(IntPtr.Zero, ref outBufLen);
                if (error == ERROR.ERROR_BUFFER_OVERFLOW)
                {
                    IntPtr pAdapterInfo = Marshal.AllocHGlobal((int)outBufLen);
                    error = GetAdaptersInfo(pAdapterInfo, ref outBufLen);
                    if (error == ERROR.ERROR_SUCCESS)
                    {
                        IntPtr pCurrAdapterInfo = pAdapterInfo;
                        while(pCurrAdapterInfo != IntPtr.Zero)
                        {
                            IP_ADAPTER_INFO adapterInfo = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pCurrAdapterInfo, typeof(IP_ADAPTER_INFO));
                            Adapter adapter = new Adapter();
                            adapter.Guid = new Guid(adapterInfo.AdapterName);
                            adapter.UnicastAddresses.Add(new Adapter.UnicastAddress(adapterInfo.IpAddressList.IpAddress, adapterInfo.IpAddressList.IpMask));
                            IntPtr pUnicastAddress = adapterInfo.IpAddressList.Next;
                            while (pUnicastAddress != IntPtr.Zero)
                            {
                                IP_ADDR_STRING unicastAddress = (IP_ADDR_STRING)Marshal.PtrToStructure(pUnicastAddress, typeof(IP_ADDR_STRING));
                                adapter.UnicastAddresses.Add(new Adapter.UnicastAddress(unicastAddress.IpAddress, unicastAddress.IpMask));
                                pUnicastAddress = unicastAddress.Next;
                            }
                            adapter.GatewayAddresses.Add(adapterInfo.GatewayList.IpAddress);
                            IntPtr pGatewayAdress = adapterInfo.GatewayList.Next;
                            while (pGatewayAdress != IntPtr.Zero)
                            {
                                IP_ADDR_STRING gatewayAddress = (IP_ADDR_STRING)Marshal.PtrToStructure(pGatewayAdress, typeof(IP_ADDR_STRING));
                                adapter.GatewayAddresses.Add(gatewayAddress.IpAddress);
                                pGatewayAdress = gatewayAddress.Next;
                            }
                            adapter.DhcpEnabled = adapterInfo.DhcpEnabled > 0;
                            adapter.Dhcpv4Server = adapterInfo.DhcpServer.IpAddress;
                            adapter.InterfaceIndex = adapterInfo.Index;
                            adapters.Add(adapter);
                            pCurrAdapterInfo = adapterInfo.Next;
                        }
                    }
                    Marshal.FreeHGlobal(pAdapterInfo);
                }
            }
            else
            {
                uint outBufLen = 0;
                ERROR error = GetAdaptersAddresses((uint)family, 0, IntPtr.Zero, IntPtr.Zero, ref outBufLen);
                if (error == ERROR.ERROR_BUFFER_OVERFLOW)
                {
                    IntPtr pAdapterAddresses = Marshal.AllocHGlobal((int)outBufLen);
                    error = GetAdaptersAddresses((uint)family, 0, IntPtr.Zero, pAdapterAddresses, ref outBufLen);
                    if (error == ERROR.ERROR_SUCCESS)
                    {
                        IntPtr currPtr = pAdapterAddresses;
                        while (currPtr != IntPtr.Zero)
                        {
                            IP_ADAPTER_ADDRESSES adapterAddress = (IP_ADAPTER_ADDRESSES)Marshal.PtrToStructure(currPtr, typeof(IP_ADAPTER_ADDRESSES));
                            Adapter adapter = new Adapter();
                            adapter.Guid = new Guid(Marshal.PtrToStringAnsi(adapterAddress.AdapterName));
                            IntPtr pUnicastAddress = adapterAddress.FirstUnicastAddress;
                            while (pUnicastAddress != IntPtr.Zero)
                            {
                                IP_ADAPTER_UNICAST_ADDRESS unicastAddress = (IP_ADAPTER_UNICAST_ADDRESS)Marshal.PtrToStructure(pUnicastAddress, typeof(IP_ADAPTER_UNICAST_ADDRESS));
                                SOCKADDR address = (SOCKADDR)Marshal.PtrToStructure(unicastAddress.Address.lpSockAddr, typeof(SOCKADDR));
                                if (address.sa_family == FAMILY.AF_INET)
                                {
                                    SOCKADDR_IN ipv4Address = (SOCKADDR_IN)Marshal.PtrToStructure(unicastAddress.Address.lpSockAddr, typeof(SOCKADDR_IN));
                                    adapter.UnicastAddresses.Add(new Adapter.UnicastAddress(new IPAddress(ipv4Address.sin_addr.S_addr).ToString(), unicastAddress.OnLinkPrefixLength));
                                }
                                else if (address.sa_family == FAMILY.AF_INET6)
                                {
                                    SOCKADDR_IN6 ipv6Address = (SOCKADDR_IN6)Marshal.PtrToStructure(unicastAddress.Address.lpSockAddr, typeof(SOCKADDR_IN6));
                                    adapter.UnicastAddresses.Add(new Adapter.UnicastAddress(new IPAddress(ipv6Address.sin6_addr.Byte).ToString(), unicastAddress.OnLinkPrefixLength));
                                }
                                pUnicastAddress = unicastAddress.Next;
                            }
                            IntPtr pGatewayAddress = adapterAddress.FirstGatewayAddress;
                            while (pGatewayAddress != IntPtr.Zero)
                            {
                                IP_ADAPTER_GATEWAY_ADDRESS gatewayAddress = (IP_ADAPTER_GATEWAY_ADDRESS)Marshal.PtrToStructure(pGatewayAddress, typeof(IP_ADAPTER_GATEWAY_ADDRESS));
                                SOCKADDR address = (SOCKADDR)Marshal.PtrToStructure(gatewayAddress.Address.lpSockAddr, typeof(SOCKADDR));
                                if (address.sa_family == FAMILY.AF_INET)
                                {
                                    SOCKADDR_IN ipv4Address = (SOCKADDR_IN)Marshal.PtrToStructure(gatewayAddress.Address.lpSockAddr, typeof(SOCKADDR_IN));
                                    adapter.GatewayAddresses.Add(new IPAddress(ipv4Address.sin_addr.S_addr).ToString());
                                }
                                else if (address.sa_family == FAMILY.AF_INET6)
                                {
                                    SOCKADDR_IN6 ipv6Address = (SOCKADDR_IN6)Marshal.PtrToStructure(gatewayAddress.Address.lpSockAddr, typeof(SOCKADDR_IN6));
                                    adapter.GatewayAddresses.Add(new IPAddress(ipv6Address.sin6_addr.Byte).ToString());
                                }
                                pGatewayAddress = gatewayAddress.Next;
                            }
                            adapter.DhcpEnabled = (adapterAddress.Flags & IP_ADAPTER_DHCP_ENABLED) > 0;
                            if (adapterAddress.Dhcpv4Server.lpSockAddr != IntPtr.Zero)
                            {
                                SOCKADDR_IN dhcpv4Server = (SOCKADDR_IN)Marshal.PtrToStructure(adapterAddress.Dhcpv4Server.lpSockAddr, typeof(SOCKADDR_IN));
                                adapter.Dhcpv4Server = new IPAddress(dhcpv4Server.sin_addr.S_addr).ToString();
                            }
                            if (adapterAddress.Dhcpv6Server.lpSockAddr != IntPtr.Zero)
                            {
                                SOCKADDR_IN6 dhcpv6Server = (SOCKADDR_IN6)Marshal.PtrToStructure(adapterAddress.Dhcpv6Server.lpSockAddr, typeof(SOCKADDR_IN6));
                                adapter.Dhcpv6Server = new IPAddress(dhcpv6Server.sin6_addr.Byte).ToString();
                            }
                            adapter.InterfaceIndex = adapterAddress.Alignment.IfIndex;
                            adapter.IPv4InterfaceMetric = adapterAddress.Ipv4Metric;
                            adapter.IPv6InterfaceMetric = adapterAddress.Ipv6Metric;
                            adapters.Add(adapter);
                            currPtr = adapterAddress.Next;
                        }
                    }
                    Marshal.FreeHGlobal(pAdapterAddresses);
                }
            }
            
            return adapters;
        }

        /// <summary>
        /// Resolves an IPv4 address to a MAC address
        /// </summary>
        /// <param name="sDestIP"></param>
        /// <param name="sSrcIP"></param>
        /// <returns></returns>
        public static string GetMacAddress(string sDestIP, string sSrcIP = "0.0.0.0")
        {
            in_addr destIP = new in_addr(sDestIP);
            in_addr srcIP = new in_addr(sSrcIP);
            UInt64 macAddr = 0;
            uint phyAddrLen = 8;
            ERROR result = SendARP(destIP, srcIP, ref macAddr, ref phyAddrLen);
            string sMac = null;
            if (result == ERROR.ERROR_SUCCESS)
                sMac = BitConverter.ToString(BitConverter.GetBytes(macAddr).Take((int)phyAddrLen).ToArray()).Replace("-", ":");
            return sMac;
        }

        // TCP & UDP functions
        // ===================
        public static List<IPSession> GetIPSessions()
        {
            List<IPSession> sessions = new List<IPSession>();
            // IPv4
            int dwSize = 0;
            ERROR error = GetExtendedTcpTable(IntPtr.Zero, ref dwSize, false, FAMILY.AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (error == ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr pTcpTable = Marshal.AllocHGlobal(dwSize);
                error = GetExtendedTcpTable(pTcpTable, ref dwSize, false, FAMILY.AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                if (error == ERROR.ERROR_SUCCESS)
                {
                    Table table = (Table)Marshal.PtrToStructure(pTcpTable, typeof(Table));
                    IntPtr pRow = pTcpTable + Marshal.SizeOf(table.dwNumEntries);
                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        MIB_TCPROW_OWNER_PID row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(pRow, typeof(MIB_TCPROW_OWNER_PID));
                        IPSession session = new IPSession();
                        session.SocketID = new IP.SocketID();
                        session.SocketID.LocalEP = new IPEndPoint(new IPAddress(row.dwLocalAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwLocalPort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.RemoteEP = new IPEndPoint(new IPAddress(row.dwRemoteAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwRemotePort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.Protocol = IP.ProtocolFamily.TCP;
                        session.State = GetDescription(row.dwState);
                        session.OwningPid = row.dwOwningPid;
                        sessions.Add(session);
                        pRow += Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
                    }
                }
                Marshal.FreeHGlobal(pTcpTable);
            }
            dwSize = 0;
            error = GetExtendedUdpTable(IntPtr.Zero, ref dwSize, false, FAMILY.AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
            if (error == ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr pUdpTable = Marshal.AllocHGlobal(dwSize);
                error = GetExtendedUdpTable(pUdpTable, ref dwSize, false, FAMILY.AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
                if (error == ERROR.ERROR_SUCCESS)
                {
                    Table table = (Table)Marshal.PtrToStructure(pUdpTable, typeof(Table));
                    IntPtr pRow = pUdpTable + Marshal.SizeOf(table.dwNumEntries);
                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        MIB_UDPROW_OWNER_PID row = (MIB_UDPROW_OWNER_PID)Marshal.PtrToStructure(pRow, typeof(MIB_UDPROW_OWNER_PID));
                        IPSession session = new IPSession();
                        session.SocketID = new IP.SocketID();
                        session.SocketID.LocalEP = new IPEndPoint(new IPAddress(row.dwLocalAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwLocalPort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.RemoteEP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
                        session.SocketID.Protocol = IP.ProtocolFamily.UDP;
                        session.State = "";
                        session.OwningPid = row.dwOwningPid;
                        sessions.Add(session);
                        pRow += Marshal.SizeOf(typeof(MIB_UDPROW_OWNER_PID));
                    }
                }
                Marshal.FreeHGlobal(pUdpTable);
            }

            // IPv6
            dwSize = 0;
            error = GetExtendedTcpTable(IntPtr.Zero, ref dwSize, false, FAMILY.AF_INET6, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (error == ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr pTcpTable = Marshal.AllocHGlobal(dwSize);
                error = GetExtendedTcpTable(pTcpTable, ref dwSize, false, FAMILY.AF_INET6, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                if (error == ERROR.ERROR_SUCCESS)
                {
                    Table table = (Table)Marshal.PtrToStructure(pTcpTable, typeof(Table));
                    IntPtr pRow = pTcpTable + Marshal.SizeOf(table.dwNumEntries);
                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        MIB_TCP6ROW_OWNER_PID row = (MIB_TCP6ROW_OWNER_PID)Marshal.PtrToStructure(pRow, typeof(MIB_TCP6ROW_OWNER_PID));
                        IPSession session = new IPSession();
                        session.SocketID = new IP.SocketID();
                        session.SocketID.LocalEP = new IPEndPoint(new IPAddress(row.ucLocalAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwLocalPort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.RemoteEP = new IPEndPoint(new IPAddress(row.ucRemoteAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwRemotePort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.Protocol = IP.ProtocolFamily.TCP;
                        session.State = GetDescription(row.dwState);
                        session.OwningPid = row.dwOwningPid;
                        sessions.Add(session);
                        pRow += Marshal.SizeOf(typeof(MIB_TCP6ROW_OWNER_PID));
                    }
                }
                Marshal.FreeHGlobal(pTcpTable);
            }
            dwSize = 0;
            error = GetExtendedUdpTable(IntPtr.Zero, ref dwSize, false, FAMILY.AF_INET6, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
            if (error == ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr pUdpTable = Marshal.AllocHGlobal(dwSize);
                error = GetExtendedUdpTable(pUdpTable, ref dwSize, false, FAMILY.AF_INET6, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
                if (error == ERROR.ERROR_SUCCESS)
                {
                    Table table = (Table)Marshal.PtrToStructure(pUdpTable, typeof(Table));
                    IntPtr pRow = pUdpTable + Marshal.SizeOf(table.dwNumEntries);
                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        MIB_UDP6ROW_OWNER_PID row = (MIB_UDP6ROW_OWNER_PID)Marshal.PtrToStructure(pRow, typeof(MIB_UDP6ROW_OWNER_PID));
                        IPSession session = new IPSession();
                        session.SocketID = new IP.SocketID();
                        session.SocketID.LocalEP = new IPEndPoint(new IPAddress(row.ucLocalAddr), BitConverter.ToUInt16(BitConverter.GetBytes(row.dwLocalPort).Reverse().Skip(2).Take(2).ToArray(), 0));
                        session.SocketID.RemoteEP = new IPEndPoint(IPAddress.Parse("::"), 0);
                        session.SocketID.Protocol = IP.ProtocolFamily.UDP;
                        session.State = "";
                        session.OwningPid = row.dwOwningPid;
                        sessions.Add(session);
                        pRow += Marshal.SizeOf(typeof(MIB_UDP6ROW_OWNER_PID));
                    }
                }
                Marshal.FreeHGlobal(pUdpTable);
            }
            return sessions;
        }

        // Structures
        // ==========

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARDROW
        {
            public uint dwForwardDest;
            public uint dwForwardMask;
            public uint dwForwardPolicy;
            public uint dwForwardNextHop;
            public uint dwForwardIfIndex;
            public MIB_IPFORWARD_TYPE dwForwardType;
            public NL_ROUTE_PROTOCOL dwForwardProto;
            public uint dwForwardAge;
            public uint dwForwardNextHopAS;
            public uint dwForwardMetric1;
            public uint dwForwardMetric2;
            public uint dwForwardMetric3;
            public uint dwForwardMetric4;
            public uint dwForwardMetric5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARDTABLE
        {
            public uint dwNumEntries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public MIB_IPFORWARDROW[] table;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct S_un_b
        {
            public char s_b1, s_b2, s_b3, s_b4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct S_un_w
        {
            public ushort s_w1, s_w2;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct in_addr
        {
            //[FieldOffset(0)]
            //public S_un_b S_un_b;
            //[FieldOffset(0)]
            //public S_un_w S_un_w;
            [FieldOffset(0)]
            public uint S_addr;

            public in_addr(string sIP)
            {
                S_addr = BitConverter.ToUInt32(IPAddress.Parse(sIP).GetAddressBytes().ToArray(), 0);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SOCKADDR_IN
        {
            [FieldOffset(0)]
            public FAMILY sin_family;
            [FieldOffset(2)]
            public ushort sin_port;
            [FieldOffset(4)]
            public in_addr sin_addr;
            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sin_zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct in6_addr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Byte;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SOCKADDR_IN6
        {
            [FieldOffset(0)]
            public FAMILY sin6_family;
            [FieldOffset(2)]
            public ushort sin6_port;
            [FieldOffset(4)]
            public uint sin6_flowinfo;
            [FieldOffset(8)]
            public in6_addr sin6_addr;
            [FieldOffset(24)]
            public uint sin6_scope_id;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SOCKADDR_INET
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public byte[] data;
            //[FieldOffset(0)]
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            //public SOCKADDR_IN6 Ipv6;
            //[FieldOffset(0)]
            //FAMILY si_family;
            public SOCKADDR_IN Ipv4
            {
                get
                {
                    IntPtr pAddress = Marshal.AllocHGlobal(28);
                    Marshal.StructureToPtr(this, pAddress, false);
                    SOCKADDR_IN address = (SOCKADDR_IN)Marshal.PtrToStructure(pAddress, typeof(SOCKADDR_IN));
                    Marshal.DestroyStructure(pAddress, typeof(SOCKADDR_INET));
                    Marshal.FreeHGlobal(pAddress);
                    return address;
                }
                set
                {
                    IntPtr pAddress = Marshal.AllocHGlobal(28);
                    Marshal.StructureToPtr(value, pAddress, false);
                    data = new byte[28];
                    Marshal.Copy(pAddress, data, 0, 28);
                    Marshal.DestroyStructure(pAddress, typeof(SOCKADDR_IN));
                    Marshal.FreeHGlobal(pAddress);
                }
            }
            public SOCKADDR_IN6 Ipv6
            {
                get
                {
                    IntPtr pAddress = Marshal.AllocHGlobal(28);
                    Marshal.StructureToPtr(this, pAddress, false);
                    SOCKADDR_IN6 address = (SOCKADDR_IN6)Marshal.PtrToStructure(pAddress, typeof(SOCKADDR_IN6));
                    Marshal.DestroyStructure(pAddress, typeof(SOCKADDR_INET));
                    Marshal.FreeHGlobal(pAddress);
                    return address;
                }
                set
                {
                    IntPtr pAddress = Marshal.AllocHGlobal(28);
                    Marshal.StructureToPtr(value, pAddress, false);
                    data = new byte[28];
                    Marshal.Copy(pAddress, data, 0, 28);
                    Marshal.DestroyStructure(pAddress, typeof(SOCKADDR_IN6));
                    Marshal.FreeHGlobal(pAddress);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADDRESS_PREFIX
        {
            public SOCKADDR_INET Prefix;
            public uint PrefixLength;
        }

        /// <summary>
        /// This covers IP_OPTION_INFORMATION and IP_OPTION_INFORMATION32
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IP_OPTION_INFORMATION
        {
            public char Ttl;
            public char Tos;
            public char Flags;
            public char OptionsSize;
            /// <summary>
            /// This is a UCHAR * POINTER_32 type on 64-bit and a PUCHAR on 32-bit
            /// </summary>
            public int OptionsData;
        }

        /// <summary>
        /// This covers ICMP_ECHO_REPLY and ICMP_ECHO_REPLY32
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ICMP_ECHO_REPLY
        {
            public in_addr Address;
            public uint Status;
            public uint RoundTripTime;
            public ushort DataSize;
            public ushort Reserved;
            /// <summary>
            /// This is a VOID * POINTER_32 type on 64-bit and a PVOID on 32-bit
            /// </summary>
            public int Data;
            public IP_OPTION_INFORMATION Options;
        }

        public enum MIB_IPFORWARD_TYPE: uint
        {
            MIB_IPROUTE_TYPE_OTHER = 1,
            MIB_IPROUTE_TYPE_INVALID,
            MIB_IPROUTE_TYPE_DIRECT,
            MIB_IPROUTE_TYPE_INDIRECT
        }

        public enum NL_ROUTE_PROTOCOL : uint
        {
            MIB_IPPROTO_OTHER = 1,
            MIB_IPPROTO_LOCAL = 2,
            MIB_IPPROTO_NETMGMT = 3,
            MIB_IPPROTO_ICMP = 4,
            MIB_IPPROTO_EGP = 5,
            MIB_IPPROTO_GGP = 6,
            MIB_IPPROTO_HELLO = 7,
            MIB_IPPROTO_RIP = 8,
            MIB_IPPROTO_IS_IS = 9,
            MIB_IPPROTO_ES_IS = 10,
            MIB_IPPROTO_CISCO = 11,
            MIB_IPPROTO_BBN = 12,
            MIB_IPPROTO_OSPF = 13,
            MIB_IPPROTO_BGP = 14,
            MIB_IPPROTO_NT_AUTOSTATIC = 10002,
            MIB_IPPROTO_NT_STATIC = 10006,
            MIB_IPPROTO_NT_STATIC_NON_DOD = 10007
        }

        public enum NL_ROUTE_ORIGIN : uint
        {
            NlroManual = 0,
            NlroWellKnown,
            NlroDHCP,
            NlroRouterAdvertisement,
            Nlro6to4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARD_ROW2
        {
            public UInt64 InterfaceLuid;
            public UInt32 InterfaceIndex;
            public IP_ADDRESS_PREFIX DestinationPrefix;
            public SOCKADDR_INET NextHop;
            public byte SitePrefixLength;
            public uint ValidLifetime;
            public uint PreferredLifetime;
            public uint Metric;
            public NL_ROUTE_PROTOCOL Protocol;
            public byte Loopback;
            public byte AutoconfigureAddress;
            public byte Publish;
            public byte Immortal;
            public uint Age;
            public NL_ROUTE_ORIGIN Origin;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARD_TABLE2
        {
            public uint              NumEntries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public MIB_IPFORWARD_ROW2[] Table;
        }

        public enum FAMILY : uint
        {
            /// <summary>
            /// IPv4
            /// </summary>
            AF_INET = 2,
            /// <summary>
            /// IPv6
            /// </summary>
            AF_INET6 = 23,
            /// <summary>
            /// Unpecified. Includes both IPv4 and IPv6
            /// </summary>
            AF_UNSPEC = 0
        }
        public enum FLAGS : uint
        {
            GAA_FLAG_SKIP_UNICAST = 0x0001,
            GAA_FLAG_SKIP_ANYCAST = 0x0002,
            GAA_FLAG_SKIP_MULTICAST = 0x0004,
            GAA_FLAG_SKIP_DNS_SERVER = 0x0008,
            GAA_FLAG_INCLUDE_PREFIX = 0x0010,
            GAA_FLAG_SKIP_FRIENDLY_NAME = 0x0020
        }
        public enum ERROR : uint
        {
            ERROR_SUCCESS = 0,
            ERROR_ACCESS_DENIED = 5,
            ERROR_NOT_SUPPORTED = 50,
            ERROR_NO_DATA = 232,
            ERROR_BUFFER_OVERFLOW = 111,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_BAD_NET_NAME = 67,
            ERROR_GEN_FAILURE = 31,
            ERROR_INVALID_USER_BUFFER = 1784
        }
        public enum IF_OPER_STATUS : uint
        {
            IfOperStatusUp = 1,
            IfOperStatusDown,
            IfOperStatusTesting,
            IfOperStatusUnknown,
            IfOperStatusDormant,
            IfOperStatusNotPresent,
            IfOperStatusLowerLayerDown,
        }
        public enum IF_TYPE : uint
        {
            IF_TYPE_OTHER = 1,   // None of the below
            IF_TYPE_REGULAR_1822 = 2,
            IF_TYPE_HDH_1822 = 3,
            IF_TYPE_DDN_X25 = 4,
            IF_TYPE_RFC877_X25 = 5,
            IF_TYPE_ETHERNET_CSMACD = 6,
            IF_TYPE_IS088023_CSMACD = 7,
            IF_TYPE_ISO88024_TOKENBUS = 8,
            IF_TYPE_ISO88025_TOKENRING = 9,
            IF_TYPE_ISO88026_MAN = 10,
            IF_TYPE_STARLAN = 11,
            IF_TYPE_PROTEON_10MBIT = 12,
            IF_TYPE_PROTEON_80MBIT = 13,
            IF_TYPE_HYPERCHANNEL = 14,
            IF_TYPE_FDDI = 15,
            IF_TYPE_LAP_B = 16,
            IF_TYPE_SDLC = 17,
            IF_TYPE_DS1 = 18,  // DS1-MIB
            IF_TYPE_E1 = 19,  // Obsolete; see DS1-MIB
            IF_TYPE_BASIC_ISDN = 20,
            IF_TYPE_PRIMARY_ISDN = 21,
            IF_TYPE_PROP_POINT2POINT_SERIAL = 22,  // proprietary serial
            IF_TYPE_PPP = 23,
            IF_TYPE_SOFTWARE_LOOPBACK = 24,
            IF_TYPE_EON = 25,  // CLNP over IP
            IF_TYPE_ETHERNET_3MBIT = 26,
            IF_TYPE_NSIP = 27,  // XNS over IP
            IF_TYPE_SLIP = 28,  // Generic Slip
            IF_TYPE_ULTRA = 29,  // ULTRA Technologies
            IF_TYPE_DS3 = 30,  // DS3-MIB
            IF_TYPE_SIP = 31,  // SMDS, coffee
            IF_TYPE_FRAMERELAY = 32,  // DTE only
            IF_TYPE_RS232 = 33,
            IF_TYPE_PARA = 34,  // Parallel port
            IF_TYPE_ARCNET = 35,
            IF_TYPE_ARCNET_PLUS = 36,
            IF_TYPE_ATM = 37,  // ATM cells
            IF_TYPE_MIO_X25 = 38,
            IF_TYPE_SONET = 39,  // SONET or SDH
            IF_TYPE_X25_PLE = 40,
            IF_TYPE_ISO88022_LLC = 41,
            IF_TYPE_LOCALTALK = 42,
            IF_TYPE_SMDS_DXI = 43,
            IF_TYPE_FRAMERELAY_SERVICE = 44,  // FRNETSERV-MIB
            IF_TYPE_V35 = 45,
            IF_TYPE_HSSI = 46,
            IF_TYPE_HIPPI = 47,
            IF_TYPE_MODEM = 48,  // Generic Modem
            IF_TYPE_AAL5 = 49,  // AAL5 over ATM
            IF_TYPE_SONET_PATH = 50,
            IF_TYPE_SONET_VT = 51,
            IF_TYPE_SMDS_ICIP = 52,  // SMDS InterCarrier Interface
            IF_TYPE_PROP_VIRTUAL = 53,  // Proprietary virtual/internal
            IF_TYPE_PROP_MULTIPLEXOR = 54,  // Proprietary multiplexing
            IF_TYPE_IEEE80212 = 55,  // 100BaseVG
            IF_TYPE_FIBRECHANNEL = 56,
            IF_TYPE_HIPPIINTERFACE = 57,
            IF_TYPE_FRAMERELAY_INTERCONNECT = 58,  // Obsolete, use 32 or 44
            IF_TYPE_AFLANE_8023 = 59,  // ATM Emulated LAN for 802.3
            IF_TYPE_AFLANE_8025 = 60,  // ATM Emulated LAN for 802.5
            IF_TYPE_CCTEMUL = 61,  // ATM Emulated circuit
            IF_TYPE_FASTETHER = 62,  // Fast Ethernet (100BaseT)
            IF_TYPE_ISDN = 63,  // ISDN and X.25
            IF_TYPE_V11 = 64,  // CCITT V.11/X.21
            IF_TYPE_V36 = 65,  // CCITT V.36
            IF_TYPE_G703_64K = 66,  // CCITT G703 at 64Kbps
            IF_TYPE_G703_2MB = 67,  // Obsolete; see DS1-MIB
            IF_TYPE_QLLC = 68,  // SNA QLLC
            IF_TYPE_FASTETHER_FX = 69,  // Fast Ethernet (100BaseFX)
            IF_TYPE_CHANNEL = 70,
            IF_TYPE_IEEE80211 = 71,  // Radio spread spectrum
            IF_TYPE_IBM370PARCHAN = 72,  // IBM System 360/370 OEMI Channel
            IF_TYPE_ESCON = 73,  // IBM Enterprise Systems Connection
            IF_TYPE_DLSW = 74,  // Data Link Switching
            IF_TYPE_ISDN_S = 75,  // ISDN S/T interface
            IF_TYPE_ISDN_U = 76,  // ISDN U interface
            IF_TYPE_LAP_D = 77,  // Link Access Protocol D
            IF_TYPE_IPSWITCH = 78,  // IP Switching Objects
            IF_TYPE_RSRB = 79,  // Remote Source Route Bridging
            IF_TYPE_ATM_LOGICAL = 80,  // ATM Logical Port
            IF_TYPE_DS0 = 81,  // Digital Signal Level 0
            IF_TYPE_DS0_BUNDLE = 82,  // Group of ds0s on the same ds1
            IF_TYPE_BSC = 83,  // Bisynchronous Protocol
            IF_TYPE_ASYNC = 84,  // Asynchronous Protocol
            IF_TYPE_CNR = 85,  // Combat Net Radio
            IF_TYPE_ISO88025R_DTR = 86,  // ISO 802.5r DTR
            IF_TYPE_EPLRS = 87,  // Ext Pos Loc Report Sys
            IF_TYPE_ARAP = 88,  // Appletalk Remote Access Protocol
            IF_TYPE_PROP_CNLS = 89,  // Proprietary Connectionless Proto
            IF_TYPE_HOSTPAD = 90,  // CCITT-ITU X.29 PAD Protocol
            IF_TYPE_TERMPAD = 91,  // CCITT-ITU X.3 PAD Facility
            IF_TYPE_FRAMERELAY_MPI = 92,  // Multiproto Interconnect over FR
            IF_TYPE_X213 = 93,  // CCITT-ITU X213
            IF_TYPE_ADSL = 94,  // Asymmetric Digital Subscrbr Loop
            IF_TYPE_RADSL = 95,  // Rate-Adapt Digital Subscrbr Loop
            IF_TYPE_SDSL = 96,  // Symmetric Digital Subscriber Loop
            IF_TYPE_VDSL = 97,  // Very H-Speed Digital Subscrb Loop
            IF_TYPE_ISO88025_CRFPRINT = 98,  // ISO 802.5 CRFP
            IF_TYPE_MYRINET = 99,  // Myricom Myrinet
            IF_TYPE_VOICE_EM = 100,  // Voice recEive and transMit
            IF_TYPE_VOICE_FXO = 101,  // Voice Foreign Exchange Office
            IF_TYPE_VOICE_FXS = 102,  // Voice Foreign Exchange Station
            IF_TYPE_VOICE_ENCAP = 103,  // Voice encapsulation
            IF_TYPE_VOICE_OVERIP = 104,  // Voice over IP encapsulation
            IF_TYPE_ATM_DXI = 105,  // ATM DXI
            IF_TYPE_ATM_FUNI = 106,  // ATM FUNI
            IF_TYPE_ATM_IMA = 107,  // ATM IMA
            IF_TYPE_PPPMULTILINKBUNDLE = 108,  // PPP Multilink Bundle
            IF_TYPE_IPOVER_CDLC = 109,  // IBM ipOverCdlc
            IF_TYPE_IPOVER_CLAW = 110,  // IBM Common Link Access to Workstn
            IF_TYPE_STACKTOSTACK = 111,  // IBM stackToStack
            IF_TYPE_VIRTUALIPADDRESS = 112,  // IBM VIPA
            IF_TYPE_MPC = 113,  // IBM multi-proto channel support
            IF_TYPE_IPOVER_ATM = 114,  // IBM ipOverAtm
            IF_TYPE_ISO88025_FIBER = 115,  // ISO 802.5j Fiber Token Ring
            IF_TYPE_TDLC = 116,  // IBM twinaxial data link control
            IF_TYPE_GIGABITETHERNET = 117,
            IF_TYPE_HDLC = 118,
            IF_TYPE_LAP_F = 119,
            IF_TYPE_V37 = 120,
            IF_TYPE_X25_MLP = 121,  // Multi-Link Protocol
            IF_TYPE_X25_HUNTGROUP = 122,  // X.25 Hunt Group
            IF_TYPE_TRANSPHDLC = 123,
            IF_TYPE_INTERLEAVE = 124,  // Interleave channel
            IF_TYPE_FAST = 125,  // Fast channel
            IF_TYPE_IP = 126,  // IP (for APPN HPR in IP networks)
            IF_TYPE_DOCSCABLE_MACLAYER = 127,  // CATV Mac Layer
            IF_TYPE_DOCSCABLE_DOWNSTREAM = 128,  // CATV Downstream interface
            IF_TYPE_DOCSCABLE_UPSTREAM = 129,  // CATV Upstream interface
            IF_TYPE_A12MPPSWITCH = 130,  // Avalon Parallel Processor
            IF_TYPE_TUNNEL = 131,  // Encapsulation interface
            IF_TYPE_COFFEE = 132,  // Coffee pot
            IF_TYPE_CES = 133,  // Circuit Emulation Service
            IF_TYPE_ATM_SUBINTERFACE = 134,  // ATM Sub Interface
            IF_TYPE_L2_VLAN = 135,  // Layer 2 Virtual LAN using 802.1Q
            IF_TYPE_L3_IPVLAN = 136,  // Layer 3 Virtual LAN using IP
            IF_TYPE_L3_IPXVLAN = 137,  // Layer 3 Virtual LAN using IPX
            IF_TYPE_DIGITALPOWERLINE = 138,  // IP over Power Lines
            IF_TYPE_MEDIAMAILOVERIP = 139,  // Multimedia Mail over IP
            IF_TYPE_DTM = 140,  // Dynamic syncronous Transfer Mode
            IF_TYPE_DCN = 141,  // Data Communications Network
            IF_TYPE_IPFORWARD = 142,  // IP Forwarding Interface
            IF_TYPE_MSDSL = 143,  // Multi-rate Symmetric DSL
            IF_TYPE_IEEE1394 = 144,  // IEEE1394 High Perf Serial Bus
            IF_TYPE_RECEIVE_ONLY = 145 // TV adapter type
        }
        public enum IP_SUFFIX_ORIGIN : uint
        {
            /// IpSuffixOriginOther -> 0
            IpSuffixOriginOther = 0,
            IpSuffixOriginManual,
            IpSuffixOriginWellKnown,
            IpSuffixOriginDhcp,
            IpSuffixOriginLinkLayerAddress,
            IpSuffixOriginRandom,
        }
        public enum IP_PREFIX_ORIGIN : uint
        {
            /// IpPrefixOriginOther -> 0
            IpPrefixOriginOther = 0,
            IpPrefixOriginManual,
            IpPrefixOriginWellKnown,
            IpPrefixOriginDhcp,
            IpPrefixOriginRouterAdvertisement,
        }
        public enum IP_DAD_STATE : uint
        {
            /// IpDadStateInvalid -> 0
            IpDadStateInvalid = 0,
            IpDadStateTentative,
            IpDadStateDuplicate,
            IpDadStateDeprecated,
            IpDadStatePreferred,
        }

        public enum NET_IF_CONNECTION_TYPE : uint
        {
            NET_IF_CONNECTION_DEDICATED = 1,
            NET_IF_CONNECTION_PASSIVE = 2,
            NET_IF_CONNECTION_DEMAND = 3,
            NET_IF_CONNECTION_MAXIMUM = 4
        }

        public enum TUNNEL_TYPE : uint
        {
            TUNNEL_TYPE_NONE = 0,
            TUNNEL_TYPE_OTHER = 1,
            TUNNEL_TYPE_DIRECT = 2,
            TUNNEL_TYPE_6TO4 = 11,
            TUNNEL_TYPE_ISATAP = 13,
            TUNNEL_TYPE_TEREDO = 14,
            TUNNEL_TYPE_IPHTTPS = 15
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKADDR
        {
            /// u_short->unsigned short
            public FAMILY sa_family;

            /// char[14]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] sa_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKET_ADDRESS
        {
            public IntPtr lpSockAddr;
            public int iSockaddrLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alignment
        {
            public uint Length;
            public int IfIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADAPTER_UNICAST_ADDRESS
        {
            public UInt64 Alignment;
            public IntPtr Next;
            public SOCKET_ADDRESS Address;
            public IP_PREFIX_ORIGIN PrefixOrigin;
            public IP_SUFFIX_ORIGIN SuffixOrigin;
            public IP_DAD_STATE DadState;
            public uint ValidLifetime;
            public uint PreferredLifetime;
            public uint LeaseLifetime;
            // Vista and above
            public byte OnLinkPrefixLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADAPTER_GATEWAY_ADDRESS
        {
            public UInt64 Alignment;
            public IntPtr Next;
            public SOCKET_ADDRESS Address;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADAPTER_ADDRESSES
        {
            public Alignment Alignment;
            public IntPtr Next;
            public IntPtr AdapterName;
            public IntPtr FirstUnicastAddress;
            public IntPtr FirstAnycastAddress;
            public IntPtr FirstMulticastAddress;
            public IntPtr FirstDnsServerAddress;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DnsSuffix;
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)]
            public string Description;
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)]
            public string FriendlyName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] PhysicalAddress;
            public uint PhysicalAddressLength;
            public uint Flags;
            public uint Mtu;
            public IF_TYPE IfType;
            public IF_OPER_STATUS OperStatus;
            uint Ipv6IfIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public uint[] ZoneIndices;
            public IntPtr FirstPrefix;

            // Items added for Vista
            // May need to be removed on Windows versions below Vista to work properly (?)
            public UInt64 TrasmitLinkSpeed;
            public UInt64 ReceiveLinkSpeed;
            public IntPtr FirstWinsServerAddress;
            public IntPtr FirstGatewayAddress;
            public uint Ipv4Metric;
            public uint Ipv6Metric;
            public UInt64 Luid;
            public SOCKET_ADDRESS Dhcpv4Server;
            public uint CompartmentId;
            public GUID NetworkGuid;
            public NET_IF_CONNECTION_TYPE ConnectionType;
            public TUNNEL_TYPE TunnelType;
            public SOCKET_ADDRESS Dhcpv6Server;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DHCPV6_DUID_LENGTH)]
            public byte[] Dhcpv6ClientDuid;
            public uint Dhcpv6ClientDuidLength;
            public uint Dhcpv6Iaid;
            public IntPtr FirstDnsSuffix;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IpAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IpMask;
            public uint Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
            public IntPtr Next;
            public uint ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string Description;
            public uint AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public int Index;
            public uint Type;
            public uint DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public Int64 LeaseObtained;
            public Int64 LeaseExpires;
        }

        public enum NL_ROUTER_DISCOVERY_BEHAVIOR : int
        {
            RouterDiscoveryDisabled,
            RouterDiscoveryEnabled,
            RouterDiscoveryDhcp,
            RouterDiscoveryUnchanged = -1
        }

        public enum NL_LINK_LOCAL_ADDRESS_BEHAVIOR : int
        {
            LinkLocalAlwaysOff,
            LinkLocalDelayed,
            LinkLocalAlwaysOn,
            LinkLocalUnchanged = -1
        }

        // unverified; implement bitfields
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NL_INTERFACE_OFFLOAD_ROD
        {
            //bool NlChecksumSupported  :1;
            //bool NlOptionsSupported  :1;
            //bool TlDatagramChecksumSupported  :1;
            //bool TlStreamChecksumSupported  :1;
            //bool TlStreamOptionsSupported  :1;
            //bool TlStreamFastPathCompatible  :1;
            //bool TlDatagramFastPathCompatible  :1;
            //bool TlLargeSendOffloadSupported  :1;
            //bool TlGiantSendOffloadSupported  :1;
        }

        // unverified
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPINTERFACE_ROW
        {
            public FAMILY Family;
            public UInt64 InterfaceLuid;
            public uint InterfaceIndex;
            public uint MaxReassemblySize;
            public UInt64 InterfaceIdentifier;
            public uint MinRouterAdvertisementInterval;
            public uint MaxRouterAdvertisementInterval;
            public bool AdvertisingEnabled;
            public bool ForwardingEnabled;
            public bool WeakHostSend;
            public bool WeakHostReceive;
            public bool UseAutomaticMetric;
            public bool UseNeighborUnreachabilityDetection;
            public bool ManagedAddressConfigurationSupported;
            public bool OtherStatefulConfigurationSupported;
            public bool AdvertiseDefaultRoute;
            public NL_ROUTER_DISCOVERY_BEHAVIOR RouterDiscoveryBehavior;
            public uint DadTransmits;
            public uint BaseReachableTime;
            public uint RetransmitTime;
            public uint PathMtuDiscoveryTimeout;
            public NL_LINK_LOCAL_ADDRESS_BEHAVIOR LinkLocalAddressBehavior;
            public uint LinkLocalAddressTimeout;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ScopeLevelCount)]
            public uint[] ZoneIndices;
            public uint SitePrefixLength;
            public uint Metric;
            public uint NlMtu;
            public bool Connected;
            public bool SupportsWakeUpPatterns;
            public bool SupportsNeighborDiscovery;
            public bool SupportsRouterDiscovery;
            public uint ReachableTime;
            public NL_INTERFACE_OFFLOAD_ROD TransmitOffload;
            public NL_INTERFACE_OFFLOAD_ROD ReceiveOffload;
            public bool DisableDefaultRoutes;
        }

        /// <summary>
        /// Generic table returned by various WinAPI functions
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Table
        {
            public uint dwNumEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public MIB_TCP_STATE dwState;
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwRemoteAddr;
            public uint dwRemotePort;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] ucLocalAddr;
            public uint dwLocalScopeId;
            public uint dwLocalPort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] ucRemoteAddr;
            public uint dwRemoteScopeId;
            public uint dwRemotePort;
            public MIB_TCP_STATE dwState;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPROW_OWNER_PID
        {
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] ucLocalAddr;
            public uint dwLocalScopeId;
            public uint dwLocalPort;
            public uint dwOwningPid;
        }

        public enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        public enum MIB_TCP_STATE
        {
            [Description("Closed")]
            MIB_TCP_STATE_CLOSED = 1,
            [Description("Listen")]
            MIB_TCP_STATE_LISTEN,
            [Description("SYN sent")]
            MIB_TCP_STATE_SYN_SENT,
            [Description("SYN received")]
            MIB_TCP_STATE_SYN_RCVD,
            [Description("Established")]
            MIB_TCP_STATE_ESTAB,
            [Description("FIN wait 1")]
            MIB_TCP_STATE_FIN_WAIT1,
            [Description("FIN wait 2")]
            MIB_TCP_STATE_FIN_WAIT2,
            [Description("Close wait")]
            MIB_TCP_STATE_CLOSE_WAIT,
            [Description("Closing")]
            MIB_TCP_STATE_CLOSING,
            [Description("Last ACK")]
            MIB_TCP_STATE_LAST_ACK,
            [Description("Time wait")]
            MIB_TCP_STATE_TIME_WAIT,
            [Description("Delete TCB")]
            MIB_TCP_STATE_DELETE_TCB
        }

        public enum UDP_TABLE_CLASS
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_TABLE_OWNER_MODULE
        }

        public enum IP_STATUS
        {
            IP_SUCCESS = 0,
            IP_BUF_TOO_SMALL = 11001,
            IP_DEST_NET_UNREACHABLE = 11002,
            IP_DEST_HOST_UNREACHABLE = 11003,
            IP_DEST_PROT_UNREACHABLE = 11004,
            IP_DEST_PORT_UNREACHABLE = 11005,
            IP_NO_RESOURCES = 11006,
            IP_BAD_OPTION = 11007,
            IP_HW_ERROR = 11008,
            IP_PACKET_TOO_BIG = 11009,
            IP_REQ_TIMED_OUT = 11010,
            IP_BAD_REQ = 11011,
            IP_BAD_ROUTE = 11012,
            IP_TTL_EXPIRED_TRANSIT = 11013,
            IP_TTL_EXPIRED_REASSEM = 11014,
            IP_PARAM_PROBLEM = 11015,
            IP_SOURCE_QUENCH = 11016,
            IP_OPTION_TOO_BIG = 11017,
            IP_BAD_DESTINATION = 11018,
            IP_GENERAL_FAILURE = 11050
        }

        // constants
        private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        private const int MAX_ADAPTER_NAME_LENGTH = 256;
        private const int MAX_DHCPV6_DUID_LENGTH = 130;
        private const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        private const uint IP_ADAPTER_DDNS_ENABLED = 0x0001;
        private const uint IP_ADAPTER_REGISTER_ADAPTER_SUFFIX = 0x0002;
        private const uint IP_ADAPTER_DHCP_ENABLED = 0x0004;
        private const uint IP_ADAPTER_RECEIVE_ONLY = 0x0008;
        private const uint IP_ADAPTER_NO_MULTICAST = 0x0010;
        private const uint IP_ADAPTER_IPV6_OTHER_STATEFUL_CONFIG = 0x0020;
        private const uint IP_ADAPTER_NETBIOS_OVER_TCPIP_ENABLED = 0x0040;
        private const uint IP_ADAPTER_IPV4_ENABLED = 0x0080;
        private const uint IP_ADAPTER_IPV6_ENABLED = 0x0100;
        private const uint IP_ADAPTER_IPV6_MANAGE_ADDRESS_CONFIG = 0x0200;
        private const int ScopeLevelCount = 16;

        public static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
        // Return types
        // ============

        public class Route
        {
            public string Destination;
            public string Prefix;
            public string Gateway;
            public int InterfaceIndex;
            public uint Age;
            /// <summary>
            /// The route metric offset value for this IP route entry.
            /// Note the actual route metric used to compute the route preference is the summation of interface metric
            /// </summary>
            public ushort Metric;
            public MIB_IPFORWARD_TYPE Type;
            public NL_ROUTE_PROTOCOL Protocol;
            public int IPVersion;
        }

        public class Adapter
        {
            public Guid Guid;
            public List<UnicastAddress> UnicastAddresses = new List<UnicastAddress>();
            /// <summary>
            /// unreliable; use routing table
            /// </summary>
            public List<string> GatewayAddresses = new List<string>();
            public bool DhcpEnabled;
            public string Dhcpv4Server;
            public string Dhcpv6Server;
            public uint IPv4InterfaceMetric;
            public uint IPv6InterfaceMetric;
            public int InterfaceIndex;

            public class UnicastAddress
            {
                public string Address;
                public byte Prefix;

                public UnicastAddress(string address, byte prefix)
                {
                    Address = address;
                    Prefix = prefix;
                }

                public UnicastAddress(string address, string ipv4Mask)
                {
                    Address = address;
                    uint uMask = BitConverter.ToUInt32(IPAddress.Parse(ipv4Mask).GetAddressBytes().Reverse().ToArray(), 0);
                    Prefix = 0;
                    while(uMask > 0)
                    {
                        Prefix++;
                        uMask <<= 1;
                    }
                }
            }
        }

        public class IPSession
        {
            public IP.SocketID SocketID;
            public string State;
            public uint OwningPid;
        }
    }
}
