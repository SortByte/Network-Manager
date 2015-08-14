using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
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

namespace Network_Manager.Jobs.Extensions
{
    public partial class LoadBalancer
    {
        public class PhysicalWorker
        {
            public string Guid;
            public string Name;
            public PacketCommunicator communicator = null;
            public FragmentationBuffer fragBuffer = new FragmentationBuffer();
            public IList<Packet> fragments = new List<Packet>();
            public string ifHardwareAddressString;
            public string ifProtocolAddressString;
            public string ifProtocolMaskString;
            public string ownProtocolAddressString = null;
            public string gatewayProtocolAddressString;
            public string gatewayHardwareAddressString;
            public MacAddress ifHardwareAddress;
            public IpV4Address ifProtocolAddress;
            public IpV4Address ifProtocolMask;
            public IpV4Address ownProtocolAddress;
            public MacAddress gatewayHardwareAddress;
            public IpV4Address gatewayProtocolAddress;
            public byte[] ifHardwareAddressByte;
            public byte[] ifProtocolAddressByte;
            public byte[] ifProtocolMaskByte;
            public byte[] ownProtocolAddressByte;
            public byte[] gatewayHardwareAddressByte;
            public byte[] gatewayProtocolAddressByte;
            public RoutingObject phyRoutingObject;
            public ManualResetEventSlim Initialized = new ManualResetEventSlim(false);
            public ManualResetEventSlim ThreadActive = new ManualResetEventSlim(true);
            private object ipIDLock = new object();
            private ushort _ipID = 0;
            private ushort ipID
            {
                get
                {
                    lock (ipIDLock)
                    {
                        _ipID = (ushort)(_ipID++);
                        return _ipID;
                    }
                }
            }

            // not used
            public static int dhcpState = 0; // 0: discover not sent, 1: discover sent, 2: offer received 3: request sent, 4: ack received, 5: success, 6: no dhcp
            public static int arpState = 0; // 0: arp not sent, 1: arp sent, 2: arp received
            public static uint dhcpId = 0;
            public static uint dhcpClientId = 1;
            public static byte[] offeredIp;
            public static byte[] dhcpServer;
            
            public PhysicalWorker(string guid, string name, string ifMac, string ifIp, string ifMask, string gatewayMac, string gatewayIp)
            {
                Guid = guid;
                Name = name;
                ifHardwareAddressString = ifMac;
                ifProtocolAddressString = ifIp;
                ifProtocolMaskString = ifMask;
                gatewayHardwareAddressString = gatewayMac;
                gatewayProtocolAddressString = gatewayIp;
                ifHardwareAddress = new MacAddress(ifHardwareAddressString);
                ifProtocolAddress = new IpV4Address(ifProtocolAddressString);
                ifProtocolMask = new IpV4Address(ifProtocolMaskString);
                gatewayHardwareAddress = new MacAddress(gatewayHardwareAddressString);
                gatewayProtocolAddress = new IpV4Address(gatewayProtocolAddressString);
                ifHardwareAddressByte = ifHardwareAddressString.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
                ifProtocolAddressByte = ifProtocolAddressString.Split('.').Select(byte.Parse).ToArray();
                ifProtocolMaskByte = ifProtocolMaskString.Split('.').Select(byte.Parse).ToArray();
                gatewayHardwareAddressByte = gatewayHardwareAddressString.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
                gatewayProtocolAddressByte = gatewayProtocolAddressString.Split('.').Select(byte.Parse).ToArray();
            }

            public void SendPacket(Packet packet)
            {
                if (ThreadActive.IsSet)
                {
                    try { communicator.SendPacket(packet); }
                    catch (Exception) { }
                }
            }

            //public Packet CreateDhcpPacket(int optionsSize, byte[] options, bool firstPacket, byte[] clientIp = null)
            //{
            //    EthernetLayer ethernetLayer = new EthernetLayer
            //    {
            //        Source = ifHardwareAddress,
            //        Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
            //        EtherType = EthernetType.None,
            //    };

            //    IpV4Layer ipV4Layer = new IpV4Layer
            //    {
            //        Source = new IpV4Address("0.0.0.0"),
            //        CurrentDestination = new IpV4Address("255.255.255.255"),
            //        Fragmentation = IpV4Fragmentation.None,
            //        HeaderChecksum = null, // Will be filled automatically.
            //        Identification = ipID,
            //        Options = IpV4Options.None,
            //        Protocol = null,
            //        Ttl = 128,
            //        TypeOfService = 0,
            //    };

            //    UdpLayer udpLayer = new UdpLayer
            //    {
            //        SourcePort = 68,
            //        DestinationPort = 67,
            //        Checksum = null, // Will be filled automatically.
            //        CalculateChecksumValue = true,
            //    };

            //    Dhcp dhcpLayer = new Dhcp(optionsSize);
            //    if (firstPacket)
            //        dhcpId = dhcpLayer.Id;
            //    else
            //        dhcpLayer.Id = dhcpId;
            //    if (clientIp != null)
            //        dhcpLayer.ClientIp = clientIp;
            //    dhcpLayer.ClientHardwareAddress = ifHardwareAddressByte;
            //    dhcpLayer.Options = options;


            //    PayloadLayer payloadLayer = new PayloadLayer
            //    {
            //        Data = new Datagram(dhcpLayer.toByteArray()),
            //    };

            //    PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);
            //    return builder.Build(DateTime.Now);
            //}
            //public void SendArp()
            //{
            //    EthernetLayer ethernetLayer = new EthernetLayer
            //    {
            //        Source = ifHardwareAddress,
            //        Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
            //        EtherType = EthernetType.None,
            //    };

            //    ArpLayer arpLayer = new ArpLayer
            //    {
            //        ProtocolType = EthernetType.IpV4,
            //        Operation = ArpOperation.Request,
            //        SenderHardwareAddress = ifHardwareAddressByte.AsReadOnly(),
            //        SenderProtocolAddress = ownProtocolAddressByte.AsReadOnly(),
            //        TargetHardwareAddress = new byte[] { 0, 0, 0, 0, 0, 0 }.AsReadOnly(),
            //        TargetProtocolAddress = gatewayProtocolAddressByte.AsReadOnly(),
            //    };

            //    PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
            //    SendPacket(builder.Build(DateTime.Now));
            //}
            //public void ReceiveIpAddress()
            //{
            //    int retries = 0;
            //    int timewait = 0;
            //    dhcpState = 0;
            //    arpState = 0;

            //    byte[] options;
            //    while (true)
            //    {
            //        if (dhcpState == 1 && retries > 1 ||
            //            dhcpState == 3 && retries > 1)
            //        {
            //            dhcpState = 6;
            //            offeredIp = BitConverter.GetBytes(ifProtocolAddress.ToValue() & ifProtocolMask.ToValue() + 1).Reverse().ToArray();
            //        }
            //        if (dhcpState == 0 || dhcpState == 1 && timewait > 10)
            //        {
            //            options = new byte[]
            //        { 
            //            53, 1, 1,
            //            61, 7, 1, 0x0A, 0x03, 0x03, 0x03, 0x03, (byte)dhcpClientId,
            //            12, 4, 78, 77, 76, 66,
            //            60, 8, 0x4d, 0x53, 0x46, 0x54, 0x20, 0x35, 0x2e, 0x30,
            //            255,
            //            0, 0, 0,
            //        };
            //            SendPacket(CreateDhcpPacket(32, options, true));
            //            if (dhcpState == 0)
            //                retries = 0;
            //            else
            //                retries++;
            //            timewait = 0;
            //            dhcpState = 1;
            //        }
            //        else if (dhcpState == 2 || dhcpState == 3 && timewait > 10)
            //        {
            //            options = new byte[]
            //        { 
            //            53, 1, 3,
            //            61, 7, 1, 0x0A, 0x03, 0x03, 0x03, 0x03, (byte)dhcpClientId,
            //            12, 4, 78, 77, 76, 66,
            //            50, 4, offeredIp[0], offeredIp[1], offeredIp[2], offeredIp[3],
            //            54, 4, dhcpServer[0], dhcpServer[1], dhcpServer[2], dhcpServer[3],
            //            60, 8, 0x4d, 0x53, 0x46, 0x54, 0x20, 0x35, 0x2e, 0x30,
            //            255,
            //            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //        };
            //            SendPacket(CreateDhcpPacket(60, options, false));
            //            if (dhcpState == 2)
            //                retries = 0;
            //            else
            //                retries++;
            //            timewait = 0;
            //            dhcpState = 3;
            //        }
            //        else if (dhcpState == 4 && arpState == 1 && retries > 1)
            //        {
            //            if ((BitConverter.ToUInt32(offeredIp.Reverse().ToArray(), 0) + 1 & ifProtocolMask.ToValue()) !=
            //                (ifProtocolAddress.ToValue() & ifProtocolMask.ToValue()))
            //            {
            //                Global.WriteLog("DHCP: IP " + offeredIp.SequenceToString(".") + " is in a different network!");
            //                offeredIp = BitConverter.GetBytes(ifProtocolAddress.ToValue() & ifProtocolMask.ToValue() + 1).Reverse().ToArray();
            //                dhcpState = 6;
            //                arpState = 0;
            //                continue;
            //            }
            //            dhcpState = 5;
            //            arpState = 0;
            //            Global.WriteLog("DHCP: IP " + offeredIp.SequenceToString(".") + " OK!");
            //            dhcpClientId++;
            //            ownProtocolAddressByte = offeredIp;
            //            ownProtocolAddressString = ownProtocolAddressByte.SequenceToString(".");
            //            ownProtocolAddress = new IpV4Address(ownProtocolAddressString);
            //            SendArp();
            //            break;
            //        }
            //        else if (dhcpState == 4 && arpState == 0 || dhcpState == 4 && arpState == 1 && timewait > 10)
            //        {
            //            EthernetLayer ethernetLayer = new EthernetLayer
            //            {
            //                Source = ifHardwareAddress,
            //                Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
            //                EtherType = EthernetType.None,
            //            };

            //            ArpLayer arpLayer = new ArpLayer
            //            {
            //                ProtocolType = EthernetType.IpV4,
            //                Operation = ArpOperation.Request,
            //                SenderHardwareAddress = ifHardwareAddressByte.AsReadOnly(),
            //                SenderProtocolAddress = new byte[] { 0, 0, 0, 0 }.AsReadOnly(),
            //                TargetHardwareAddress = new byte[] { 0, 0, 0, 0, 0, 0 }.AsReadOnly(),
            //                TargetProtocolAddress = offeredIp.AsReadOnly(),
            //            };

            //            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
            //            SendPacket(builder.Build(DateTime.Now));
            //            if (arpState == 0)
            //                retries = 0;
            //            else
            //                retries++;
            //            timewait = 0;
            //            arpState = 1;
            //        }
            //        else if (dhcpState == 4 && arpState == 2)
            //        {
            //            Global.WriteLog("DHCP: IP " + offeredIp.SequenceToString(".") + " is already used!");
            //            options = new byte[]
            //        { 
            //            53, 1, 4,
            //            61, 7, 1, 0x0A, 0x03, 0x03, 0x03, 0x03, (byte)dhcpClientId,
            //            50, 4, offeredIp[0], offeredIp[1], offeredIp[2], offeredIp[3],
            //            54, 4, dhcpServer[0], dhcpServer[1], dhcpServer[2], dhcpServer[3],
            //            255,
            //            //0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            //        };
            //            SendPacket(CreateDhcpPacket(25, options, false, offeredIp));
            //            arpState = 0;
            //            dhcpState = 0;
            //        }
            //        else if (dhcpState == 6 && arpState == 1 && retries > 1)
            //        {
            //            dhcpState = 5;
            //            arpState = 0;
            //            Global.WriteLog("IPAlloc: IP " + offeredIp.SequenceToString(".") + " OK!");
            //            dhcpClientId++;
            //            ownProtocolAddressByte = offeredIp;
            //            ownProtocolAddressString = ownProtocolAddressByte.SequenceToString(".");
            //            ownProtocolAddress = new IpV4Address(ownProtocolAddressString);
            //            SendArp();
            //            break;
            //        }
            //        else if (dhcpState == 6 && arpState == 0 || dhcpState == 6 && arpState == 1 && timewait > 10)
            //        {
            //            EthernetLayer ethernetLayer = new EthernetLayer
            //            {
            //                Source = ifHardwareAddress,
            //                Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
            //                EtherType = EthernetType.None,
            //            };

            //            ArpLayer arpLayer = new ArpLayer
            //            {
            //                ProtocolType = EthernetType.IpV4,
            //                Operation = ArpOperation.Request,
            //                SenderHardwareAddress = ifHardwareAddressByte.AsReadOnly(),
            //                SenderProtocolAddress = new byte[] { 0, 0, 0, 0 }.AsReadOnly(),
            //                TargetHardwareAddress = new byte[] { 0, 0, 0, 0, 0, 0 }.AsReadOnly(),
            //                TargetProtocolAddress = offeredIp.AsReadOnly(),
            //            };

            //            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
            //            SendPacket(builder.Build(DateTime.Now));
            //            if (arpState == 0)
            //                retries = 0;
            //            else
            //                retries++;
            //            timewait = 0;
            //            arpState = 1;
            //        }
            //        else if (dhcpState == 6 && arpState == 2)
            //        {
            //            if ((BitConverter.ToUInt32(offeredIp.Reverse().ToArray(), 0) + 1 & ifProtocolMask.ToValue()) !=
            //                (ifProtocolAddress.ToValue() & ifProtocolMask.ToValue()))
            //            {
            //                Global.WriteLog("IPAlloc: Couldn't allocate IP address!");
            //                break;
            //            }
            //            Global.WriteLog("IPAlloc: IP " + offeredIp.SequenceToString(".") + " is already used!");
            //            offeredIp = BitConverter.GetBytes((BitConverter.ToUInt32(offeredIp.Reverse().ToArray(), 0) + 1)).Reverse().ToArray();
            //            arpState = 0;
            //        }
            //        Thread.Sleep(100);
            //        timewait++;
            //    }
            //}

            public void ReceivePackets()
            {
                // Retrieve the device list from the local machine
                IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

                // Find the NPF device of the interface
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
                        //communicator.SetFilter("(ether dst " + ifHardwareAddressString + " and (ip or arp)) or (ether dst FF:FF:FF:FF:FF:FF and arp)");
                        communicator.SetFilter("(ether dst " + ifHardwareAddressString + " and ip and (udp or icmp))");
                        Initialized.Set();
                        communicator.ReceivePackets(0, (packet) =>
                        {
                            if (!ThreadActive.IsSet)
                            {
                                communicator.Break();
                                return;
                            }
                            //if (ownProtocolAddressString == null)
                            //{
                            //    if (packet.Ethernet.IpV4.Udp.SourcePort == 67 && packet.Ethernet.IpV4.Udp.DestinationPort == 68 && (PublicVars.dhcpState == 1 || PublicVars.dhcpState == 3))
                            //    {
                            //        Dhcp dhcpReply = new Dhcp(packet.Ethernet.IpV4.Udp.Payload.ToArray());
                            //        if (PublicVars.dhcpId == dhcpReply.Id && dhcpReply.MessageType == 2)
                            //        {
                            //            if (dhcpReply.getOption((int)DhcpOptionCode.DhcpMsgType)[0] == 2)
                            //            {
                            //                PublicVars.offeredIp = dhcpReply.YourIp;
                            //                PublicVars.dhcpServer = dhcpReply.getOption((int)DhcpOptionCode.DhcpServerId);
                            //                PublicVars.dhcpState = 2;
                            //            }
                            //            else if (dhcpReply.getOption((int)DhcpOptionCode.DhcpMsgType)[0] == 5)
                            //            {
                            //                PublicVars.dhcpState = 4;
                            //            }
                            //            else if (dhcpReply.getOption((int)DhcpOptionCode.DhcpMsgType)[0] == 6)
                            //            {
                            //                PublicVars.offeredIp = null;
                            //                PublicVars.dhcpState = 0;
                            //                PublicVars.dhcpId = 0;
                            //            }
                            //        }
                            //    }
                            //    else if (packet.Ethernet.EtherType == EthernetType.Arp && PublicVars.arpState == 1)
                            //    {
                            //        if (packet.Ethernet.Arp.SenderProtocolAddress.SequenceEqual(PublicVars.offeredIp))
                            //            PublicVars.arpState = 2;
                            //    }
                            //}
                            //if (packet.Ethernet.EtherType == EthernetType.Arp)
                            //{
                            //    if (packet.Ethernet.Arp.TargetProtocolAddress.SequenceEqual(ownProtocolAddressByte) &&
                            //        packet.Ethernet.Arp.Operation == ArpOperation.Request)
                            //    {
                            //        EthernetLayer ethernetLayer = new EthernetLayer
                            //        {
                            //            Source = ifHardwareAddress,
                            //            Destination = packet.Ethernet.Source,
                            //            EtherType = EthernetType.None,
                            //        };

                            //        ArpLayer arpLayer = new ArpLayer
                            //        {
                            //            ProtocolType = EthernetType.IpV4,
                            //            Operation = ArpOperation.Reply,
                            //            SenderHardwareAddress = ifHardwareAddressByte.AsReadOnly(),
                            //            SenderProtocolAddress = packet.Ethernet.Arp.TargetProtocolAddress,
                            //            TargetHardwareAddress = packet.Ethernet.Arp.SenderHardwareAddress,
                            //            TargetProtocolAddress = packet.Ethernet.Arp.SenderProtocolAddress,
                            //        };

                            //        PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
                            //        communicator.SendPacket(builder.Build(DateTime.Now));
                            //    }
                            //}
                            if (tapWorker.Initialized.IsSet)
                            {
                                if (packet.Ethernet.IpV4.Length > MTU) // only UDP and ICMP
                                {
                                    EthernetLayer ethernetLayer = new EthernetLayer
                                    {
                                        Source = ifHardwareAddress,
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
                                if (packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.DoNotFragment ||
                                packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.None && packet.Ethernet.IpV4.Fragmentation.Offset == 0)
                                {
                                    if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.InternetControlMessageProtocol)
                                    {

                                        if (packet.Ethernet.IpV4.Destination == ifProtocolAddress)
                                            if ((packet.Ethernet.IpV4.Icmp.MessageType == IcmpMessageType.EchoReply || packet.Ethernet.IpV4.Fragmentation.Offset > 0))
                                            {
                                                EthernetLayer ethernetLayer = new EthernetLayer
                                                {
                                                    Source = tapWorker.ownHardwareAddress,
                                                    Destination = tapWorker.ifHardwareAddress,
                                                    EtherType = EthernetType.None,
                                                };
                                                IpV4Layer ipV4Layer = new IpV4Layer
                                                {
                                                    Source = packet.Ethernet.IpV4.Source,
                                                    CurrentDestination = tapWorker.ifProtocolAddress,
                                                    Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                                                    HeaderChecksum = null, // Will be filled automatically.
                                                    Identification = packet.Ethernet.IpV4.Identification,
                                                    Options = packet.Ethernet.IpV4.Options,
                                                    Protocol = packet.Ethernet.IpV4.Protocol,
                                                    Ttl = packet.Ethernet.IpV4.Ttl,
                                                    TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                                                };
                                                PayloadLayer payloadLayer = new PayloadLayer
                                                {
                                                    Data = new Datagram(packet.Ethernet.IpV4.Payload.ToArray()),
                                                };
                                                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, payloadLayer);
                                                tapWorker.SendPacket(builder.Build(DateTime.Now));
                                            }
                                    }
                                    //else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Tcp)
                                    //{
                                    //    if (packet.Ethernet.IpV4.Destination == ifProtocolAddress)
                                    //    {
                                    //        lock (RoutingTable.lockObj)
                                    //            routingObject = RoutingTable.TranslateBack(packet);
                                    //        if (routingObject.ifIndex == -1)
                                    //            return;
                                    //        EthernetLayer ethernetLayer = new EthernetLayer
                                    //        {
                                    //            Source = TapWorker.ownHardwareAddress,
                                    //            Destination = TapWorker.ifHardwareAddress,
                                    //            EtherType = packet.Ethernet.EtherType,
                                    //        };
                                    //        IpV4Layer ipV4Layer = new IpV4Layer
                                    //        {
                                    //            Source = packet.Ethernet.IpV4.Source,
                                    //            CurrentDestination = TapWorker.ifProtocolAddress,
                                    //            Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                                    //            HeaderChecksum = null, // Will be filled automatically.
                                    //            Identification = packet.Ethernet.IpV4.Identification,
                                    //            Options = packet.Ethernet.IpV4.Options,
                                    //            Protocol = packet.Ethernet.IpV4.Protocol,
                                    //            Ttl = packet.Ethernet.IpV4.Ttl,
                                    //            TypeOfService = packet.Ethernet.IpV4.TypeOfService,
                                    //        };
                                    //        TcpLayer tcpLayer = new TcpLayer
                                    //        {
                                    //            SourcePort = packet.Ethernet.IpV4.Tcp.SourcePort,
                                    //            DestinationPort = routingObject.localPort,
                                    //            Checksum = null, // Will be filled automatically.
                                    //            SequenceNumber = packet.Ethernet.IpV4.Tcp.SequenceNumber,
                                    //            AcknowledgmentNumber = routingObject.ack,
                                    //            ControlBits = packet.Ethernet.IpV4.Tcp.ControlBits,
                                    //            Window = packet.Ethernet.IpV4.Tcp.Window,
                                    //            UrgentPointer = packet.Ethernet.IpV4.Tcp.UrgentPointer,
                                    //            Options = packet.Ethernet.IpV4.Tcp.Options,
                                    //        };
                                    //        PayloadLayer payloadLayer = new PayloadLayer
                                    //        {
                                    //            Data = new Datagram(packet.Ethernet.IpV4.Tcp.Payload.ToArray()),
                                    //        };
                                    //        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer, payloadLayer);
                                    //        MainThread.tapWorker.SendPacket(builder.Build(DateTime.Now));
                                    //    }
                                    //}
                                    else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
                                    {
                                        if (packet.Ethernet.IpV4.Destination == ifProtocolAddress)
                                        {
                                            phyRoutingObject = RoutingTable.IsUdpConn(packet);
                                            if (phyRoutingObject.ifIndex == -1)
                                                return;
                                            EthernetLayer ethernetLayer = new EthernetLayer
                                            {
                                                Source = tapWorker.ownHardwareAddress,
                                                Destination = tapWorker.ifHardwareAddress,
                                                EtherType = packet.Ethernet.EtherType,
                                            };
                                            IpV4Layer ipV4Layer = new IpV4Layer
                                            {
                                                Source = packet.Ethernet.IpV4.Source,
                                                CurrentDestination = tapWorker.ifProtocolAddress,
                                                Fragmentation = packet.Ethernet.IpV4.Fragmentation,
                                                HeaderChecksum = null, // Will be filled automatically.
                                                Identification = packet.Ethernet.IpV4.Identification,
                                                Options = packet.Ethernet.IpV4.Options,
                                                Protocol = packet.Ethernet.IpV4.Protocol,
                                                Ttl = packet.Ethernet.IpV4.Ttl,
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
                                            tapWorker.SendPacket(builder.Build(DateTime.Now));
                                        }
                                    }
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

            public void ConnectCallback(IAsyncResult ar)
            {
                if (RoutingTable.clearing)
                    return;
                else
                    lock (RoutingTable.lockNumOfCallbacks)
                        RoutingTable.numOfCallbacks++;
                Guid guid = (Guid)ar.AsyncState;
                lock (RoutingTable.connParams[guid])
                {
                    try
                    {
                        RoutingTable.connParams[guid].socket.EndConnect(ar);
                        tapWorker.SendSynAck(guid);
                        RoutingTable.connParams[guid].UpdateSeq(1);
                        RoutingTable.RequestAccess();
                        RoutingTable.connStatus[guid].state = 2;
                        RoutingTable.ReleaseAccess();
                        RoutingTable.connParams[guid].socket.BeginReceive(RoutingTable.connParams[guid].receivingBuffer, 0, RoutingTable.connParams[guid].receivingBuffer.Count(), 0, new AsyncCallback(ReceiveCallback), guid);
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException || e is ObjectDisposedException)
                        {
                            RoutingTable.RequestAccess();
                            if (!RoutingTable.connStatus[guid].remoteEPFin)
                                tapWorker.SendRst(guid);
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.connStatus[guid].localEPFin = true;
                            RoutingTable.ReleaseAccess();
                        }
                        else
                            throw;
                    }
                }
                lock (RoutingTable.lockNumOfCallbacks)
                    RoutingTable.numOfCallbacks--;
            }
            public void DisconnectCallback(IAsyncResult ar)
            {
                if (RoutingTable.clearing)
                    return;
                else
                    lock (RoutingTable.lockNumOfCallbacks)
                        RoutingTable.numOfCallbacks++;
                Guid guid = (Guid)ar.AsyncState;
                lock (RoutingTable.connParams[guid])
                {
                    try
                    {
                        RoutingTable.connParams[guid].socket.EndDisconnect(ar);
                        RoutingTable.connParams[guid].UpdateAck(1);
                        tapWorker.SendAck(guid);
                        RoutingTable.RequestAccess();
                        RoutingTable.connStatus[guid].localEPFin = true;
                        RoutingTable.ReleaseAccess();
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException || e is ObjectDisposedException)
                        {
                            RoutingTable.RequestAccess();
                            if (!RoutingTable.connStatus[guid].remoteEPFin)
                                tapWorker.SendRst(guid);
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.connStatus[guid].localEPFin = true;
                            RoutingTable.ReleaseAccess();
                        }
                        else
                            throw;
                    }
                }
                lock (RoutingTable.lockNumOfCallbacks)
                    RoutingTable.numOfCallbacks--;
            }
            public void ReceiveCallback(IAsyncResult ar)
            {
                if (RoutingTable.clearing)
                    return;
                else
                    lock (RoutingTable.lockNumOfCallbacks)
                        RoutingTable.numOfCallbacks++;
                Guid guid = (Guid)ar.AsyncState;
                lock (RoutingTable.connParams[guid])
                {
                    try
                    {
                        int bytesRead = RoutingTable.connParams[guid].socket.EndReceive(ar);
                        if (bytesRead == 0)
                        {
                            tapWorker.SendFinAck(guid);
                            RoutingTable.connParams[guid].UpdateSeq(1);
                            RoutingTable.RequestAccess();
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.ReleaseAccess();
                            lock (RoutingTable.lockNumOfCallbacks)
                                RoutingTable.numOfCallbacks--;
                            return;
                        }
                        if (bytesRead <= RoutingTable.connParams[guid].freeWindow)
                        {
                            tapWorker.SendData(guid, bytesRead);
                            RoutingTable.connParams[guid].freeWindow -= bytesRead;
                            RoutingTable.connParams[guid].UpdateSeq(bytesRead);
                            RoutingTable.connParams[guid].socket.BeginReceive(RoutingTable.connParams[guid].receivingBuffer, 0, RoutingTable.connParams[guid].receivingBuffer.Count(), 0, new AsyncCallback(ReceiveCallback), guid);
                        }
                        else
                        {
                            RoutingTable.connParams[guid].windowFull = true;
                            RoutingTable.connParams[guid].bytesReceived = bytesRead;
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException || e is ObjectDisposedException)
                        {
                            RoutingTable.RequestAccess();
                            if (!RoutingTable.connStatus[guid].remoteEPFin)
                                tapWorker.SendRst(guid);
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.connStatus[guid].localEPFin = true;
                            RoutingTable.ReleaseAccess();
                        }
                        else
                            throw;
                    }
                }
                lock (RoutingTable.lockNumOfCallbacks)
                    RoutingTable.numOfCallbacks--;
            }
            public async void SendData(Packet packet, Guid guid)
            {
                try
                {
                    IAsyncResult ar = null;
                    lock (RoutingTable.connParams[guid])
                    {
                        RoutingTable.connParams[guid].bytesToSend += (uint)packet.Ethernet.IpV4.Tcp.PayloadLength + (uint)((packet.Ethernet.IpV4.Tcp.ControlBits & TcpControlBits.Fin) > 0 ? 1 : 0);
                        if (packet.Ethernet.IpV4.Tcp.PayloadLength > 0)
                            ar = RoutingTable.connParams[guid].socket.BeginSend(packet.Ethernet.IpV4.Tcp.Payload.ToArray(), 0, packet.Ethernet.IpV4.Tcp.PayloadLength, 0, null, guid);
                    }
                    if (packet.Ethernet.IpV4.Tcp.PayloadLength > 0)
                        await Task.Factory.FromAsync(ar, (r) => SendCallback(r));
                    lock (RoutingTable.connParams[guid])
                        if ((packet.Ethernet.IpV4.Tcp.ControlBits & TcpControlBits.Fin) > 0)
                            RoutingTable.connParams[guid].socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), guid);
                }
                catch (Exception e)
                {
                    if (e is SocketException || e is ObjectDisposedException)
                    {
                        lock (RoutingTable.connParams[guid])
                        {
                            RoutingTable.RequestAccess();
                            if (!RoutingTable.connStatus[guid].remoteEPFin)
                                tapWorker.SendRst(guid);
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.connStatus[guid].localEPFin = true;
                            RoutingTable.ReleaseAccess();
                        }
                    }
                    else
                        throw;
                }
            }
            public void SendCallback(IAsyncResult ar)
            {
                Guid guid = (Guid)ar.AsyncState;
                if (RoutingTable.clearing)
                    return;
                else
                {
                    lock (RoutingTable.lockNumOfCallbacks)
                        RoutingTable.numOfCallbacks++;
                    RoutingTable.connParams[guid].lockSendCallback.Wait();
                }
                lock (RoutingTable.connParams[guid])
                {
                    try
                    {
                        int bytesSent = RoutingTable.connParams[guid].socket.EndSend(ar);
                        RoutingTable.connParams[guid].UpdateAck(bytesSent);
                        RoutingTable.connParams[guid].bytesToSend -= (uint)bytesSent;
                        tapWorker.SendAck(guid);
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException || e is ObjectDisposedException)
                        {
                            RoutingTable.RequestAccess();
                            if (!RoutingTable.connStatus[guid].remoteEPFin)
                                tapWorker.SendRst(guid);
                            RoutingTable.connStatus[guid].remoteEPFin = true;
                            RoutingTable.connStatus[guid].localEPFin = true;
                            RoutingTable.ReleaseAccess();
                        }
                        else
                            throw;
                    }
                }
                lock (RoutingTable.lockNumOfCallbacks)
                    RoutingTable.numOfCallbacks--;
                RoutingTable.connParams[guid].lockSendCallback.Release();
            }
        }
    }
}
