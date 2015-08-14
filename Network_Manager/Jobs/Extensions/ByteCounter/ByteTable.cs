using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace Network_Manager.Jobs.Extensions
{
    public partial class ByteCounter
    {
        public class ByteTable
        {
            public class Bytes
            {
                public UInt64 Received = 0;
                public UInt64 Sent = 0;
                public Bytes(int received, int sent)
                {
                    Received = (UInt64)received;
                    Sent = (UInt64)sent;
                }
                public Bytes Update(int received, int sent)
                {
                    Received += (UInt64)received;
                    Sent += (UInt64)sent;
                    return this;
                }
            }
            private ConcurrentDictionary<IP.SocketID, Bytes> byteTable = new ConcurrentDictionary<IP.SocketID, Bytes>();

            public void CheckPacket(Packet packet, MacAddress mac)
            {
                IP.SocketID key = new IP.SocketID();
                if (packet.Ethernet.EtherType == EthernetType.IpV4)
                {
                    // egress key
                    if (packet.Ethernet.Source == mac)
                    {
                        key.LocalEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Source.ToString()), packet.Ethernet.IpV4.Udp.SourcePort);
                        key.RemoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Destination.ToString()), packet.Ethernet.IpV4.Udp.DestinationPort);
                        
                    }
                    // ingress key
                    else //if (packet.Ethernet.Destination == mac)
                    {
                        key.LocalEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Destination.ToString()), packet.Ethernet.IpV4.Udp.DestinationPort);
                        key.RemoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV4.Source.ToString()), packet.Ethernet.IpV4.Udp.SourcePort);
                    }
                    key.Protocol = (IP.ProtocolFamily)packet.Ethernet.IpV4.Protocol;
                }
                else //if (packet.Ethernet.EtherType == EthernetType.IpV6)
                {
                    // egress key
                    if (packet.Ethernet.Source == mac)
                    {
                        key.LocalEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.Source.ToString()), packet.Ethernet.IpV6.Udp.SourcePort);
                        key.RemoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.CurrentDestination.ToString()), packet.Ethernet.IpV6.Udp.DestinationPort);
                    }
                    // ingress key
                    else //if (packet.Ethernet.Destination == mac)
                    {
                        key.LocalEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.CurrentDestination.ToString()), packet.Ethernet.IpV6.Udp.DestinationPort);
                        key.RemoteEP = new IPEndPoint(IPAddress.Parse(packet.Ethernet.IpV6.Source.ToString()), packet.Ethernet.IpV6.Udp.SourcePort);
                    }
                    key.Protocol = (IP.ProtocolFamily)packet.Ethernet.IpV6.NextHeader;
                }
                // egress update
                if (packet.Ethernet.Source == mac)
                    byteTable.AddOrUpdate(key, new Bytes(0, packet.Ethernet.PayloadLength), (k, v) => v.Update(0, packet.Ethernet.PayloadLength));
                // ingress update
                else //if (packet.Ethernet.Destination == mac)
                    byteTable.AddOrUpdate(key, new Bytes(packet.Ethernet.PayloadLength, 0), (k, v) => v.Update(packet.Ethernet.PayloadLength, 0));
                
                if (NewBytes != null)
                    NewBytes(null, new NewBytesEventArgs(key));
                //if (Global.Config.Gadget.Debug)
                //    Global.WriteLog("New bytes: " +
                //        key.LocalEP.Address.ToString() + ":" + key.LocalEP.Port + (packet.Ethernet.Source == mac ? " -> " : " <- ") +
                //        key.RemoteEP.Address.ToString() + ":" + key.RemoteEP.Port + " " + key.Protocol);
            }
            public Bytes GetBytes(IP.SocketID socketID)
            {
                if (socketID.Protocol == IP.ProtocolFamily.TCP)
                {
                    return byteTable.GetOrAdd(socketID, new Bytes(0, 0));
                }
                else //if (socketID.Protocol == IP.ProtocolFamily.UDP)
                {
                    if (socketID.LocalEP.Address.GetAddressBytes().Max() > 0)
                    {
                        Bytes result = byteTable.FirstOrDefault((i) =>
                            i.Key.LocalEP.Address.Equals(socketID.LocalEP.Address) &&
                            i.Key.LocalEP.Port == socketID.LocalEP.Port).Value;
                        if (result == null)
                            return new Bytes(0, 0);
                        else
                            return result;
                    }
                    else
                    {
                        Bytes result = byteTable.FirstOrDefault((i) =>
                            i.Key.LocalEP.AddressFamily ==  socketID.LocalEP.AddressFamily &&
                            i.Key.LocalEP.Port == socketID.LocalEP.Port).Value;
                        if (result == null)
                            return new Bytes(0, 0);
                        else
                            return result;
                    }
                }
            }

            public void Clear()
            {
                byteTable.Clear();
            }
        }
    }
}
