using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Dns;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Gre;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Igmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Network_Manager.Jobs.Extensions
{
    public partial class LoadBalancer
    {
        public class TapWorker
        {
            public string Guid;
            public string Name;
            public static string ownHardwareAddressString = "0A:03:03:03:03:03";
            public static string ownProtocolAddressString;
            public byte[] ownHardwareAddressByte = ownHardwareAddressString.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
            public byte[] ownProtocolAddressByte;
            public MacAddress ownHardwareAddress = new MacAddress(ownHardwareAddressString);
            public IpV4Address ownProtocolAddress;
            public string ifHardwareAddressString;
            public string ifProtocolAddressString;
            public MacAddress ifHardwareAddress;
            public IpV4Address ifProtocolAddress;
            public byte[] ifHardwareAddressByte;
            public byte[] ifProtocolAddressByte;
            public PacketCommunicator communicator;
            public FragmentationBuffer fragBuffer = new FragmentationBuffer();
            public IList<Packet> fragments = new List<Packet>();
            public RoutingObject tapRoutingObject;
            public ManualResetEventSlim Initialized = new ManualResetEventSlim(false);
            public ManualResetEventSlim ThreadActive = new ManualResetEventSlim(true);
            private object ipIDLock = new object();
            private ushort _ipID = 0;
            private ushort ipID
            {
                get
                {
                    lock(ipIDLock)
                    {
                        _ipID = (ushort)(_ipID++);
                        return _ipID;
                    }
                }
            }


            public TapWorker(string guid, string name, string mac, string ip, string gateway)
            {
                Guid = guid;
                Name = name;
                ifHardwareAddressString = mac;
                ifProtocolAddressString = ip;
                ownProtocolAddressString = gateway;
                ifHardwareAddress = new MacAddress(ifHardwareAddressString);
                ifProtocolAddress = new IpV4Address(ifProtocolAddressString);
                ownProtocolAddress = new IpV4Address(ownProtocolAddressString);
                ifHardwareAddressByte = ifHardwareAddressString.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
                ifProtocolAddressByte = ifProtocolAddressString.Split('.').Select(byte.Parse).ToArray();
                ownProtocolAddressByte = ownProtocolAddressString.Split('.').Select(byte.Parse).ToArray();
            }
            public void SendPacket(Packet packet)
            {
                if (ThreadActive.IsSet)
                {
                    try { communicator.SendPacket(packet); }
                    catch (Exception) { }
                }
            }
            public void ReceivePackets()
            {
                // Retrieve the device list from the local machine
                IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

                // Find the NPF device of the TAP interface
                PacketDevice selectedDevice = null;
                for (int i = 0; i != allDevices.Count; ++i)
                {
                    LivePacketDevice device = allDevices[i];
                    if (device.Name.Contains(Guid))
                    {
                        selectedDevice = device;
                        break;
                    }
                }

                if (selectedDevice == null)
                {
                    Initialized.Set();
                    Global.ShowTrayTip("Load Balancer", "Interface " + Name + " not captured by WinPcap.", System.Windows.Forms.ToolTipIcon.Warning);
                    Global.WriteLog("Load Balancer: Interface " + Name + " not captured by WinPcap.");
                    return;
                }

                try
                {
                    using (communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.MaximumResponsiveness, 1000))
                    {
                        Global.WriteLog("Load Balancer: Listening on " + Name + "...");
                        communicator.SetFilter("(ether dst " + ownHardwareAddressString + " and ((ip and (tcp or udp or icmp)) or arp)) or (ether dst FF:FF:FF:FF:FF:FF and arp)");
                        Initialized.Set();
                        communicator.ReceivePackets(0, (packet) =>
                        {
                            if (!ThreadActive.IsSet)
                            {
                                communicator.Break();
                                return;
                            }
                            if (packet.Ethernet.EtherType == EthernetType.Arp)
                            {
                                if (packet.Ethernet.Arp.TargetProtocolAddress.SequenceEqual(ownProtocolAddressByte) && packet.Ethernet.Arp.Operation == ArpOperation.Request)
                                {
                                    EthernetLayer ethernetLayer = new EthernetLayer
                                    {
                                        Source = ownHardwareAddress,
                                        Destination = packet.Ethernet.Source,
                                        EtherType = EthernetType.None,
                                    };

                                    ArpLayer arpLayer = new ArpLayer
                                    {
                                        ProtocolType = EthernetType.IpV4,
                                        Operation = ArpOperation.Reply,
                                        SenderHardwareAddress = ownHardwareAddressByte.AsReadOnly(),
                                        SenderProtocolAddress = packet.Ethernet.Arp.TargetProtocolAddress,
                                        TargetHardwareAddress = packet.Ethernet.Arp.SenderHardwareAddress,
                                        TargetProtocolAddress = packet.Ethernet.Arp.SenderProtocolAddress,
                                    };

                                    PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
                                    SendPacket(builder.Build(DateTime.Now));
                                }
                            }
                            else if (packet.Ethernet.EtherType.ToString() == "IpV4")
                            {

                                if (packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.DoNotFragment ||
                                    packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.None && packet.Ethernet.IpV4.Fragmentation.Offset == 0)
                                {
                                    IpV4Handler(packet);
                                }
                                else if (packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.MoreFragments ||
                                    packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.None && packet.Ethernet.IpV4.Fragmentation.Offset > 0)
                                {
                                    // TODO: fix warning spam
                                    if (Global.Config.LoadBalancer.ShowTrayTipsWarnings)
                                        Global.ShowTrayTip("Load Balancer", "IP fragmentation detected on " + Name + ".\n\nIP fragmentation is not supported.", ToolTipIcon.Warning);
                                    Global.WriteLog("Load Balancer: IP fragmentation detected on " + Name);
                                    //fragments = fragBuffer.Add(packet);
                                    //if (fragments != null)
                                    //    for (int i = 0; i < fragments.Count; i++)
                                    //    {
                                    //        IpV4Handler(fragments[i], fragments);
                                    //    }
                                }
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    if (ThreadActive.IsSet)
                    {
                        ThreadActive.Reset();
                        Global.WriteLog("Load Balancer: " + Name + " has disconnected");
                        if (Global.Config.Gadget.Debug)
                            Global.WriteLog(e.ToString());
                        Global.ShowTrayTip("Load Balancer", Name + " has disconnected", System.Windows.Forms.ToolTipIcon.Warning);
                    }
                }
            }

            public void Stop()
            {
                ThreadActive.Reset();
                try { communicator.Break(); }
                catch { }
            }

            public void IpV4Handler(Packet packet, IList<Packet> packetList = null)
            {
                if (packet.Ethernet.IpV4.Length > MTU)
                {
                    EthernetLayer ethernetLayer = new EthernetLayer
                    {
                        Source = ownHardwareAddress,
                        Destination = packet.Ethernet.Source,
                        EtherType = EthernetType.None,
                    };
                    IpV4Layer ipV4Layer = new IpV4Layer
                    {
                        Source = packet.Ethernet.IpV4.Destination,
                        CurrentDestination = packet.Ethernet.IpV4.Source,
                        Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                        HeaderChecksum = null, // Will be filled automatically.
                        Identification = 123,
                        Options = packet.Ethernet.IpV4.Options,
                        Protocol = packet.Ethernet.IpV4.Protocol,
                        Ttl = packet.Ethernet.IpV4.Ttl,
                        TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                    };
                    IcmpDestinationUnreachableLayer icmpLayer = new IcmpDestinationUnreachableLayer
                    {
                        Code = IcmpCodeDestinationUnreachable.FragmentationNeededAndDoNotFragmentSet,
                        NextHopMaximumTransmissionUnit = (ushort)MTU,
                    };

                    PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer);
                    SendPacket(builder.Build(DateTime.Now));
                    // TODO: fix warning spam
                    if (Global.Config.LoadBalancer.ShowTrayTipsWarnings)
                        Global.ShowTrayTip("Load Balancer", "IP packet larger than the MTU detected on " + Name + ".\n\nIP fragmentation is not supported.", ToolTipIcon.Warning);
                    Global.WriteLog("Load Balancer: IP packet larger than the MTU detected on " + Name);
                    return;
                }
                if (packet.Ethernet.IpV4.Ttl < 2)
                {
                    EthernetLayer ethernetLayer = new EthernetLayer
                    {
                        Source = ownHardwareAddress,
                        Destination = packet.Ethernet.Source,
                        EtherType = EthernetType.None,
                    };
                    IpV4Layer ipV4Layer = new IpV4Layer
                    {
                        Source = ownProtocolAddress,
                        CurrentDestination = packet.Ethernet.IpV4.Source,
                        Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                        HeaderChecksum = null, // Will be filled automatically.
                        Identification = 123,
                        Options = packet.Ethernet.IpV4.Options,
                        Protocol = packet.Ethernet.IpV4.Protocol,
                        Ttl = packet.Ethernet.IpV4.Ttl,
                        TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                    };
                    IcmpTimeExceededLayer icmpLayer = new IcmpTimeExceededLayer
                    {
                        Code = 0,
                    };

                    PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer, packet.Ethernet.IpV4.ExtractLayer(), packet.Ethernet.IpV4.Payload.ExtractLayer());
                    SendPacket(builder.Build(DateTime.Now));
                    return;
                }
                if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.InternetControlMessageProtocol)
                {
                    if (packet.Ethernet.IpV4.Destination == ownProtocolAddress)
                    {
                        if ((packet.Ethernet.IpV4.Icmp.MessageType == IcmpMessageType.Echo || packet.Ethernet.IpV4.Fragmentation.Offset > 0))
                        {
                            EthernetLayer ethernetLayer = new EthernetLayer
                            {
                                Source = ownHardwareAddress,
                                Destination = packet.Ethernet.Source,
                                EtherType = EthernetType.None,
                            };
                            IpV4Layer ipV4Layer = new IpV4Layer
                            {
                                Source = packet.Ethernet.IpV4.Destination,
                                CurrentDestination = packet.Ethernet.IpV4.Source,
                                Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                                HeaderChecksum = null, // Will be filled automatically.
                                Identification = 123,
                                Options = packet.Ethernet.IpV4.Options,
                                Protocol = packet.Ethernet.IpV4.Protocol,
                                Ttl = packet.Ethernet.IpV4.Ttl,
                                TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                            };
                            if (packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.DoNotFragment ||
                                packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.None && packet.Ethernet.IpV4.Fragmentation.Offset == 0)
                            {
                                byte[] icmpPacket = packet.Ethernet.IpV4.Payload.ToArray();
                                icmpPacket[0] = 0;
                                icmpPacket[2] = 0;
                                icmpPacket[3] = 0;
                                ushort checksum = IpFunctions.ComputeIpChecksum(icmpPacket);
                                icmpPacket[2] = (byte)(checksum >> 8);
                                icmpPacket[3] = (byte)(checksum & 0xff);
                                PayloadLayer payloadLayer = new PayloadLayer
                                {
                                    Data = new Datagram(icmpPacket),
                                };
                                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, payloadLayer);
                                SendPacket(builder.Build(DateTime.Now));
                            }
                            else if (packet.Ethernet.IpV4.Fragmentation.Offset == 0 && packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.MoreFragments)
                            {
                                byte[] icmpHeader = packet.Ethernet.IpV4.Payload.ToArray();
                                icmpHeader[0] = 0;
                                icmpHeader[2] = 0;
                                icmpHeader[3] = 0;
                                int icmpPacketLength = icmpHeader.Length;
                                for (int i = 1; i < packetList.Count; i++)
                                {
                                    icmpPacketLength += packetList[i].Ethernet.IpV4.Payload.ToArray().Length;
                                }
                                byte[] icmpPacket = new byte[icmpPacketLength];
                                Buffer.BlockCopy(icmpHeader, 0, icmpPacket, 0, icmpHeader.Length);
                                icmpPacketLength = icmpHeader.Length;
                                for (int i = 1; i < packetList.Count; i++)
                                {
                                    Buffer.BlockCopy(packetList[i].Ethernet.IpV4.Payload.ToArray(), 0, icmpPacket, icmpPacketLength, packetList[i].Ethernet.IpV4.Payload.ToArray().Length);
                                    icmpPacketLength += packetList[i].Ethernet.IpV4.Payload.ToArray().Length;
                                }
                                ushort checksum = IpFunctions.ComputeIpChecksum(icmpPacket);
                                icmpHeader[2] = (byte)(checksum >> 8);
                                icmpHeader[3] = (byte)(checksum & 0xff);
                                PayloadLayer payloadLayer = new PayloadLayer
                                {
                                    Data = new Datagram(icmpHeader),
                                };
                                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, payloadLayer);
                                SendPacket(builder.Build(DateTime.Now));
                            }
                            else
                            {
                                PayloadLayer payloadLayer = new PayloadLayer
                                {
                                    Data = new Datagram(packet.Ethernet.IpV4.Payload.ToArray()),
                                };
                                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, payloadLayer);
                                SendPacket(builder.Build(DateTime.Now));
                            }
                        }
                    }
                    else if (packet.Ethernet.IpV4.Source == ifProtocolAddress)
                        if ((packet.Ethernet.IpV4.Icmp.MessageType == IcmpMessageType.Echo || packet.Ethernet.IpV4.Fragmentation.Offset > 0))
                        {
                            {
                                tapRoutingObject = RoutingTable.GetInterface(packet);
                                if (tapRoutingObject.response == -1)
                                    return;
                                EthernetLayer ethernetLayer = new EthernetLayer
                                {
                                    Source = physicalWorkers[tapRoutingObject.ifIndex].ifHardwareAddress,
                                    Destination = physicalWorkers[tapRoutingObject.ifIndex].gatewayHardwareAddress,
                                    EtherType = EthernetType.None,
                                };
                                IpV4Layer ipV4Layer = new IpV4Layer
                                {
                                    Source = physicalWorkers[tapRoutingObject.ifIndex].ifProtocolAddress,
                                    CurrentDestination = packet.Ethernet.IpV4.Destination,
                                    Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                                    HeaderChecksum = null, // Will be filled automatically.
                                    Identification = packet.Ethernet.IpV4.Identification,
                                    Options = packet.Ethernet.IpV4.Options,
                                    Protocol = packet.Ethernet.IpV4.Protocol,
                                    Ttl = (byte)(packet.Ethernet.IpV4.Ttl - 1),
                                    TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                                };
                                PayloadLayer payloadLayer = new PayloadLayer
                                {
                                    Data = new Datagram(packet.Ethernet.IpV4.Payload.ToArray()),
                                };
                                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, payloadLayer);
                                physicalWorkers[tapRoutingObject.ifIndex].SendPacket(builder.Build(DateTime.Now));
                            }
                        }
                }
                else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Tcp)
                {
                    if (packet.Ethernet.IpV4.Source == ifProtocolAddress)
                    {
                        tapRoutingObject = RoutingTable.GetInterface(packet);
                        if (tapRoutingObject.response == -1)
                            return;
                        if (tapRoutingObject.response == -3)
                        {
                            lock (RoutingTable.connParams[tapRoutingObject.guid])
                                SendAck(tapRoutingObject.guid);
                            return;
                        }
                        lock (RoutingTable.connParams[tapRoutingObject.guid])
                        {
                            RoutingTable.connParams[tapRoutingObject.guid].freeWindow = (int)((packet.Ethernet.IpV4.Tcp.Window << RoutingTable.connParams[tapRoutingObject.guid].windowScale) - (RoutingTable.connParams[tapRoutingObject.guid].seq - packet.Ethernet.IpV4.Tcp.AcknowledgmentNumber));
                            if (RoutingTable.connParams[tapRoutingObject.guid].windowFull)
                            {
                                if (RoutingTable.connParams[tapRoutingObject.guid].freeWindow > RoutingTable.connParams[tapRoutingObject.guid].bytesReceived)
                                {
                                    SendData(tapRoutingObject.guid, RoutingTable.connParams[tapRoutingObject.guid].bytesReceived);
                                    RoutingTable.connParams[tapRoutingObject.guid].freeWindow -= RoutingTable.connParams[tapRoutingObject.guid].bytesReceived;
                                    RoutingTable.connParams[tapRoutingObject.guid].windowFull = false;
                                    RoutingTable.connParams[tapRoutingObject.guid].UpdateSeq(RoutingTable.connParams[tapRoutingObject.guid].bytesReceived);
                                    try
                                    {
                                        RoutingTable.connParams[tapRoutingObject.guid].socket.BeginReceive(RoutingTable.connParams[tapRoutingObject.guid].receivingBuffer, 0, RoutingTable.connParams[tapRoutingObject.guid].receivingBuffer.Count(), 0, new AsyncCallback(physicalWorkers[tapRoutingObject.ifIndex].ReceiveCallback), tapRoutingObject.guid);
                                    }
                                    catch (Exception)
                                    {
                                        RoutingTable.RequestAccess();
                                        if (!RoutingTable.connStatus[tapRoutingObject.guid].remoteEPFin)
                                            tapWorker.SendRst(tapRoutingObject.guid);
                                        RoutingTable.connStatus[tapRoutingObject.guid].remoteEPFin = true;
                                        RoutingTable.connStatus[tapRoutingObject.guid].localEPFin = true;
                                        RoutingTable.ReleaseAccess();
                                    }
                                }
                            }
                        }
                        if (packet.Ethernet.IpV4.Tcp.PayloadLength > 0 || tapRoutingObject.response == -2)
                            physicalWorkers[tapRoutingObject.ifIndex].SendData(packet, tapRoutingObject.guid);
                    }
                }
                else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
                {
                    if (packet.Ethernet.IpV4.Source == ifProtocolAddress)
                    {
                        tapRoutingObject = RoutingTable.GetInterface(packet);
                        if (tapRoutingObject.response == -1)
                            return;
                        EthernetLayer ethernetLayer = new EthernetLayer
                        {
                            Source = physicalWorkers[tapRoutingObject.ifIndex].ifHardwareAddress,
                            Destination = physicalWorkers[tapRoutingObject.ifIndex].gatewayHardwareAddress,
                            EtherType = packet.Ethernet.EtherType,
                        };

                        IpV4Layer ipV4Layer = new IpV4Layer
                        {
                            Source = physicalWorkers[tapRoutingObject.ifIndex].ifProtocolAddress,
                            CurrentDestination = packet.Ethernet.IpV4.Destination,
                            Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                            HeaderChecksum = null, // Will be filled automatically.
                            Identification = packet.Ethernet.IpV4.Identification,
                            Options = packet.Ethernet.IpV4.Options,
                            Protocol = packet.Ethernet.IpV4.Protocol,
                            Ttl = (byte)(packet.Ethernet.IpV4.Ttl - 1),
                            TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                        };
                        UdpLayer udpLayer = new UdpLayer
                        {
                            SourcePort = packet.Ethernet.IpV4.Udp.SourcePort,
                            DestinationPort = packet.Ethernet.IpV4.Udp.DestinationPort,
                            Checksum = null, // Will be filled automatically.
                            CalculateChecksumValue = packet.Ethernet.IpV4.Udp.IsChecksumOptional,
                        };
                        PayloadLayer payloadLayer = new PayloadLayer
                        {
                            Data = new Datagram(packet.Ethernet.IpV4.Udp.Payload.ToArray()),
                        };
                        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);
                        physicalWorkers[tapRoutingObject.ifIndex].SendPacket(builder.Build(DateTime.Now));
                    }

                }
            }

            public void SendData(Guid guid, int payloadLength)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = RoutingTable.connParams[guid].seq,
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Acknowledgment,
                    Window = RoutingTable.connParams[guid].GetWindow(),
                    UrgentPointer = 0,
                    Options = TcpOptions.None,
                };
                PayloadLayer payloadLayer = new PayloadLayer
                {
                    Data = new Datagram(RoutingTable.connParams[guid].receivingBuffer.Take(payloadLength).ToArray()),
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer, payloadLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
            public void SendAck(Guid guid)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = RoutingTable.connParams[guid].seq,
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Acknowledgment,
                    Window = RoutingTable.connParams[guid].GetWindow(),
                    UrgentPointer = 0,
                    Options = TcpOptions.None,
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
            public void SendSynAck(Guid guid)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = RoutingTable.connParams[guid].seq,
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Synchronize | TcpControlBits.Acknowledgment,
                    Window = RoutingTable.connParams[guid].GetWindow(),
                    UrgentPointer = 0,
                    Options = new TcpOptions(new TcpOptionMaximumSegmentSize((ushort)MSS), new TcpOptionWindowScale(RoutingTable.connParams[guid].windowScale)),
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
            public void SendFinAck(Guid guid)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = RoutingTable.connParams[guid].seq,
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Fin | TcpControlBits.Acknowledgment,
                    Window = 0xffff,
                    UrgentPointer = 0,
                    Options = TcpOptions.None,
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
            public void SendRst(Guid guid)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = RoutingTable.connParams[guid].seq,
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Reset | TcpControlBits.Acknowledgment,
                    Window = 0,
                    UrgentPointer = 0,
                    Options = TcpOptions.None,
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
            public void SendKeepAlive(Guid guid)
            {
                EthernetLayer ethernetLayer = new EthernetLayer
                {
                    Source = ownHardwareAddress,
                    Destination = ifHardwareAddress,
                    EtherType = EthernetType.None,
                };
                IpV4Layer ipV4Layer = new IpV4Layer
                {
                    Source = RoutingTable.connParams[guid].remoteIp,
                    CurrentDestination = ifProtocolAddress,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = ipID,
                    Options = IpV4Options.None,
                    Protocol = IpV4Protocol.Tcp,
                    Ttl = 128,
                    TypeOfService = 0,
                };
                TcpLayer tcpLayer = new TcpLayer
                {
                    SourcePort = RoutingTable.connParams[guid].remotePort,
                    DestinationPort = RoutingTable.connParams[guid].localPort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = IpFunctions.SubM32(RoutingTable.connParams[guid].seq, 1),
                    AcknowledgmentNumber = RoutingTable.connParams[guid].ack,
                    ControlBits = TcpControlBits.Acknowledgment,
                    Window = RoutingTable.connParams[guid].GetWindow(),
                    UrgentPointer = 0,
                    Options = TcpOptions.None,
                };
                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer);
                SendPacket(builder.Build(DateTime.Now));
            }
        }
    }
}
