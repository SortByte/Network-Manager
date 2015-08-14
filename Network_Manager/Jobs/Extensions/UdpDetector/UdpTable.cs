using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace Network_Manager.Jobs.Extensions
{
    public partial class UdpDetector
    {
        public class UdpTable
        {
            private ConcurrentDictionary<IPEndPoint, IPEndPoint> udpTable = new ConcurrentDictionary<IPEndPoint, IPEndPoint>();

            public void CheckPacket(Packet packet, MacAddress mac)
            {
                IPEndPoint localEP;
                IPEndPoint remoteEP;
                if (packet.Ethernet.EtherType == EthernetType.IpV4)
                {
                    // egress
                    if (packet.Ethernet.Source == mac)
                    {
                        localEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Source.ToString()), packet.Ethernet.IpV4.Udp.SourcePort);
                        remoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Destination.ToString()), packet.Ethernet.IpV4.Udp.DestinationPort);
                    }
                    // ingress
                    else //if (packet.Ethernet.Destination == mac)
                    {
                        localEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Destination.ToString()), packet.Ethernet.IpV4.Udp.DestinationPort);
                        remoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Source.ToString()), packet.Ethernet.IpV4.Udp.SourcePort);
                    }
                }
                else //if (packet.Ethernet.EtherType == EthernetType.IpV6)
                {
                    // egress
                    if (packet.Ethernet.Source == mac)
                    {
                        localEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.Source.ToString()), packet.Ethernet.IpV6.Udp.SourcePort);
                        remoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.CurrentDestination.ToString()), packet.Ethernet.IpV6.Udp.DestinationPort);
                    }
                    // ingress
                    else //if (packet.Ethernet.Destination == mac)
                    {
                        localEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.CurrentDestination.ToString()), packet.Ethernet.IpV6.Udp.DestinationPort);
                        remoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.Source.ToString()), packet.Ethernet.IpV6.Udp.SourcePort);
                    }
                }
                udpTable.AddOrUpdate(localEP, remoteEP, (k,v) => remoteEP);
                if (NewEndPoint != null)
                    NewEndPoint(null, new NewEndPointEventArgs(localEP));
                if (Global.Config.Gadget.Debug)
                    Global.WriteLog("New remote UDP end point: " +
                        localEP.Address.ToString() + ":" + localEP.Port + (packet.Ethernet.Source == mac ? " -> " : " <- ") +
                        remoteEP.Address.ToString() + ":" + remoteEP.Port);
                
            }

            public IPEndPoint GetRemoteEP(IPEndPoint localEP)
            {
                IPEndPoint remoteEP = null;
                if (localEP.Address.GetAddressBytes().Max() > 0)
                {
                    if (udpTable.TryGetValue(localEP, out remoteEP))
                        return remoteEP;
                }
                else
                {
                    remoteEP = udpTable.LastOrDefault((i) => i.Key.Port == localEP.Port).Value;
                }
                return remoteEP;
            }

            public void Clear()
            {
                udpTable.Clear();
            }
        }
    }
}