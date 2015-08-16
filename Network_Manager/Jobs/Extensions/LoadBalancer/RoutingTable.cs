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
using System.Collections.Concurrent;
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
using Lib.Network;

namespace Network_Manager.Jobs.Extensions
{
    public partial class LoadBalancer
    {
        // TODO: routing table code cleanup
        public class RoutingTable
        {
            public struct ConnId
            {
                public ConnId(ushort lPort, IpV4Address rIp, ushort rPort, IpV4Protocol protocol)
                {
                    this.lPort = lPort;
                    this.rIp = rIp;
                    this.rPort = rPort;
                    this.protocol = protocol;
                }
                public IpV4Address rIp;
                public ushort lPort;
                public ushort rPort;
                public IpV4Protocol protocol;
            }
            public class ConnStatus
            {
                public DateTime timeStamp = new DateTime();
                public uint expectedSeq;
                public int state = 0; // 0 - Closed; 1 - SYN; 2 - SYN-ACK; 3 - Data
                public bool remoteEPFin = false;
                public bool localEPFin = false;
                public void UpdateExpectedSeq(Packet packet)
                {
                    this.expectedSeq = IpFunctions.AddM32(this.expectedSeq, (uint)packet.Ethernet.IpV4.Tcp.PayloadLength, (uint)((packet.Ethernet.IpV4.Tcp.ControlBits & (TcpControlBits.Fin | TcpControlBits.Synchronize)) > 0 ? 1 : 0));
                }
            }
            public class ConnParams
            {
                public byte[] receivingBuffer = new byte[MSS];
                public Socket socket;
                public ushort localPort;
                public IpV4Address remoteIp;
                public ushort remotePort;
                public uint ack;
                public uint seq;
                public byte windowScale;
                public ushort windowSize;
                public int freeWindow;
                public bool windowFull = false;
                public uint bytesToSend = 0;
                public int bytesReceived = 0;
                public SemaphoreSlim lockSendCallback = new SemaphoreSlim(1);
                public ConnParams()
                {
                    Random random = new Random();
                    this.seq = (uint)(random.Next(0xffff)) << 16 | (uint)(random.Next(0xffff));
                }
                public void UpdateSeq(uint seqInc)
                {
                    this.seq = (uint)(((UInt64)this.seq + (UInt64)seqInc) % 0x100000000);
                }
                public void UpdateSeq(int seqInc)
                {
                    this.seq = (uint)(((UInt64)this.seq + (UInt64)seqInc) % 0x100000000);
                }
                public void UpdateAck(uint ackInc)
                {
                    this.ack = (uint)(((UInt64)this.ack + (UInt64)ackInc) % 0x100000000);
                }
                public void UpdateAck(int ackInc)
                {
                    this.ack = (uint)(((UInt64)this.ack + (UInt64)ackInc) % 0x100000000);
                }
                public ushort GetWindow()
                {
                    return (ushort)(0xfaf0 << this.windowScale > this.bytesToSend ? (0xfaf0 << this.windowScale) - this.bytesToSend : 0);
                }
            }

            private static ConcurrentDictionary<ConnId, Guid> guidList = new ConcurrentDictionary<ConnId, Guid>();
            public static ConcurrentDictionary<Guid, int> routingTable = new ConcurrentDictionary<Guid, int>();
            public static ConcurrentDictionary<Guid, ConnStatus> connStatus = new ConcurrentDictionary<Guid, ConnStatus>();
            public static ConcurrentDictionary<Guid, ConnParams> connParams = new ConcurrentDictionary<Guid, ConnParams>();
            private static List<ConnId> connToRemove = new List<ConnId>();
            private static ManualResetEventSlim unlockedCleanUp = new ManualResetEventSlim(true);
            private static ManualResetEventSlim unlockedRouting = new ManualResetEventSlim(true);
            private static SemaphoreSlim masterAccess = new SemaphoreSlim(1);
            private static bool getListWaiting = false;
            private static bool cleanUpWaiting = false;
            private static object lockEvents = new Object();
            private static object lockNumOfRoutings = new Object();

            public static object lockNumOfCallbacks = new Object();
            public static CountdownEvent numOfRoutings = new CountdownEvent(0);
            public static int numOfCallbacks = 0;
            public static bool clearing = false;


            public static RoutingObject GetInterface(Packet packet)
            {
                RequestAccess();
                RoutingObject routingObject = new RoutingObject();
                if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Tcp)
                {
                    ConnId connId = new ConnId(packet.Ethernet.IpV4.Tcp.SourcePort, packet.Ethernet.IpV4.Destination, packet.Ethernet.IpV4.Tcp.DestinationPort, IpV4Protocol.Tcp);
                    Guid guid;
                    if (guidList.TryGetValue(connId, out guid))
                    {
                        connStatus[guid].timeStamp = DateTime.Now;
                        routingObject.response = 0;
                        routingObject.ifIndex = routingTable[guid];
                        routingObject.guid = guid;
                        if ((packet.Ethernet.IpV4.Tcp.ControlBits & TcpControlBits.Reset) > 0)
                        {
                            connStatus[guid].localEPFin = true;
                            connStatus[guid].remoteEPFin = true;
                            routingObject.response = -1;
                            lock (connParams[guid])
                            {
                                connParams[guid].socket.Close();
                            }
                        }
                        else if ((packet.Ethernet.IpV4.Tcp.ControlBits & TcpControlBits.Fin) > 0)
                        {
                            routingObject.response = -2;
                        }
                        if (!connStatus[guid].localEPFin && packet.Ethernet.IpV4.Tcp.SequenceNumber == connStatus[guid].expectedSeq)
                        {
                            connStatus[guid].UpdateExpectedSeq(packet);
                            if (connStatus[guid].state == 2)
                                connStatus[guid].state = 3;
                        }
                        else if (packet.Ethernet.IpV4.Tcp.SequenceNumber == IpFunctions.SubM32(connStatus[guid].expectedSeq, 1) &&
                            (packet.Ethernet.IpV4.Tcp.ControlBits & (TcpControlBits.Fin | TcpControlBits.Synchronize)) == 0 &&
                            (packet.Ethernet.IpV4.Tcp.PayloadLength == 0 || packet.Ethernet.IpV4.Tcp.PayloadLength == 1) &&
                            !connStatus[guid].remoteEPFin)
                            routingObject.response = -3; // KeepAlive
                        else
                            routingObject.response = -1;
                    }
                    else
                    {
                        if ((packet.Ethernet.IpV4.Tcp.ControlBits & TcpControlBits.Synchronize) == TcpControlBits.Synchronize)
                        {
                            guid = Guid.NewGuid();
                            guidList.TryAdd(connId, guid);
                            int routingInterface = LoadBalancer.routingInterface;
                            routingTable.TryAdd(guid, routingInterface);
                            connParams.TryAdd(guid, new ConnParams());
                            connParams[guid].ack = packet.Ethernet.IpV4.Tcp.SequenceNumber;
                            connParams[guid].UpdateAck(1);
                            connParams[guid].localPort = packet.Ethernet.IpV4.Tcp.SourcePort;
                            connParams[guid].remoteIp = packet.Ethernet.IpV4.Destination;
                            connParams[guid].remotePort = packet.Ethernet.IpV4.Tcp.DestinationPort;
                            connStatus.TryAdd(guid, new ConnStatus());
                            connStatus[guid].timeStamp = DateTime.Now;
                            connStatus[guid].expectedSeq = packet.Ethernet.IpV4.Tcp.SequenceNumber;
                            connStatus[guid].UpdateExpectedSeq(packet);
                            connStatus[guid].state = 1;
                            foreach (TcpOption tcpOption in packet.Ethernet.IpV4.Tcp.Options.OptionsCollection)
                            {
                                if (tcpOption.OptionType == TcpOptionType.WindowScale)
                                    connParams[guid].windowScale = ((TcpOptionWindowScale)tcpOption).ScaleFactorLog;
                            }
                            connParams[guid].windowSize = packet.Ethernet.IpV4.Tcp.Window;
                            connParams[guid].freeWindow = (int)((packet.Ethernet.IpV4.Tcp.Window << RoutingTable.connParams[guid].windowScale) - (RoutingTable.connParams[guid].seq - packet.Ethernet.IpV4.Tcp.AcknowledgmentNumber));
                            connParams[guid].socket = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            try
                            {
                                connParams[guid].socket.Bind(new IPEndPoint(new System.Net.IPAddress(physicalWorkers[routingInterface].ifProtocolAddressByte), 0));
                                connParams[guid].socket.BeginConnect(new IPEndPoint(new IPAddress(packet.Ethernet.IpV4.Destination.ToString().Split('.').Select(byte.Parse).ToArray()), (int)packet.Ethernet.IpV4.Tcp.DestinationPort), new AsyncCallback(physicalWorkers[routingInterface].ConnectCallback), guid);
                            }
                            catch (Exception)
                            {
                                connStatus[guid].localEPFin = true;
                                connStatus[guid].remoteEPFin = true;
                            }

                        }
                        routingObject.response = -1;
                    }
                }
                else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
                {
                    ConnId connId = new ConnId(packet.Ethernet.IpV4.Udp.SourcePort, packet.Ethernet.IpV4.Destination, packet.Ethernet.IpV4.Udp.DestinationPort, IpV4Protocol.Udp);
                    Guid guid;
                    if (guidList.TryGetValue(connId, out guid))
                    {
                        connStatus[guid].timeStamp = DateTime.Now;
                        routingObject.ifIndex = routingTable[guid];
                        routingObject.guid = guid;
                    }
                    else
                    {
                        guid = Guid.NewGuid();
                        guidList.TryAdd(connId, guid);
                        int routingInterface = LoadBalancer.routingInterface;
                        routingTable.TryAdd(guid, routingInterface);
                        connStatus.TryAdd(guid, new ConnStatus());
                        connStatus[guid].timeStamp = DateTime.Now;
                        routingObject.ifIndex = routingInterface;
                        routingObject.guid = guid;
                    }
                }
                else if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.InternetControlMessageProtocol)
                    routingObject = new RoutingObject(0, LoadBalancer.routingInterface);
                else
                    routingObject = new RoutingObject(-1, 0);
                ReleaseAccess();
                return routingObject;
            }
            public static RoutingObject IsUdpConn(Packet packet)
            {
                RequestAccess();
                RoutingObject routingObject = new RoutingObject();
                if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
                {
                    ConnId connId = new ConnId(packet.Ethernet.IpV4.Tcp.DestinationPort, packet.Ethernet.IpV4.Source, packet.Ethernet.IpV4.Tcp.SourcePort, IpV4Protocol.Udp);
                    Guid guid;
                    if (guidList.TryGetValue(connId, out guid))
                    {
                        connStatus[guid].timeStamp = DateTime.Now;
                        routingObject.ifIndex = routingTable[guid];
                    }
                    else
                    {
                        routingObject.ifIndex = -1;
                    }
                }
                ReleaseAccess();
                return routingObject;
            }
            public static void CleanUp()
            {
                lock (lockEvents)
                    unlockedRouting.Reset();
                cleanUpWaiting = true;
                numOfRoutings.Wait();
                masterAccess.Wait();
                connToRemove.Clear();
                if (Global.Config.Gadget.Debug)
                    Global.WriteLog("\r\nList:");
                foreach (KeyValuePair<ConnId, Guid> entry in guidList)
                {
                    if (Global.Config.Gadget.Debug)
                        Global.WriteLog("NAT " + entry.Key.lPort.ToString() + " " + entry.Key.rIp.ToString() + ":" + entry.Key.rPort.ToString() + " " + entry.Key.protocol.ToString() + " " + routingTable[entry.Value].ToString() + " " + connStatus[entry.Value].state.ToString());
                    if (entry.Key.protocol == IpV4Protocol.Tcp)
                    {
                        if (connStatus[entry.Value].localEPFin && connStatus[entry.Value].remoteEPFin)
                        {
                            if (connStatus[entry.Value].timeStamp < DateTime.Now.Subtract(new TimeSpan(0, 1, 0)))
                                connToRemove.Add(entry.Key);
                        }
                        else if (connStatus[entry.Value].timeStamp < DateTime.Now.Subtract(new TimeSpan(0, 10, 0)) && connStatus[entry.Value].state > 2)
                        {
                            connStatus[entry.Value].state = 2;
                            //lock (RoutingTable.connParams[entry.Value]) // callbacks call requestaccess = deadlock
                            tapWorker.SendKeepAlive(entry.Value);
                        }
                        else if (connStatus[entry.Value].state < 3)
                        {
                            if (connStatus[entry.Value].timeStamp < DateTime.Now.Subtract(new TimeSpan(0, 1, 0)))
                            {
                                connParams[entry.Value].socket.Close();
                                connStatus[entry.Value].localEPFin = true;
                                connStatus[entry.Value].remoteEPFin = true;
                            }
                        }
                    }
                    else if (entry.Key.protocol == IpV4Protocol.Udp)
                    {
                        if (connStatus[entry.Value].timeStamp < DateTime.Now.Subtract(new TimeSpan(0, 5, 0)))
                            connToRemove.Add(entry.Key);
                        else if (entry.Key.rPort == 53)
                            if (connStatus[entry.Value].timeStamp < DateTime.Now.Subtract(new TimeSpan(0, 1, 0)))
                                connToRemove.Add(entry.Key);
                    }
                }
                ConnParams value1;
                int value2;
                ConnStatus value3;
                Guid value4;
                foreach (ConnId key in connToRemove)
                {
                    if (Global.Config.Gadget.Debug)
                        Global.WriteLog("Removing " + key.lPort.ToString() + " " + key.rIp.ToString() + ":" + key.rPort.ToString() + " " + key.protocol.ToString());
                    if (key.protocol == IpV4Protocol.Tcp)
                    {
                        connParams[guidList[key]].socket.Close();
                        connParams.TryRemove(guidList[key], out value1);
                    }
                    routingTable.TryRemove(guidList[key], out value2);
                    connStatus.TryRemove(guidList[key], out value3);
                    guidList.TryRemove(key, out value4);
                }
                cleanUpWaiting = false;
                masterAccess.Release();
                if (!getListWaiting)
                    unlockedRouting.Set();
            }
            public static void Clear()
            {
                clearing = true;
                lock (lockEvents)
                    unlockedRouting.Reset();
                numOfRoutings.Wait();

                for (int i = 0; i < 100; i++)
                {
                    lock (lockNumOfCallbacks)
                        if (numOfCallbacks == 0)
                            break;
                    Thread.Sleep(100);
                }
                if (numOfCallbacks != 0)
                {
                    Global.WriteLog("Callbacks deadlock! (" + numOfCallbacks + ")");
                    Global.ShowTrayTip("Load Balancer", "Load Balancer has crashed", ToolTipIcon.Error);
                    Global.Exit();
                }
                connToRemove.Clear();
                foreach (KeyValuePair<ConnId, Guid> entry in guidList)
                    connToRemove.Add(entry.Key);
                ConnParams value1;
                int value2;
                ConnStatus value3;
                Guid value4;
                foreach (ConnId key in connToRemove)
                {
                    if (Global.Config.Gadget.Debug)
                        Global.WriteLog("Removing " + key.lPort.ToString() + " " + key.rIp.ToString() + ":" + key.rPort.ToString() + " " + key.protocol.ToString());
                    if (key.protocol == IpV4Protocol.Tcp)
                    {
                        connParams[guidList[key]].socket.Close();
                        connParams.TryRemove(guidList[key], out value1);
                    }
                    routingTable.TryRemove(guidList[key], out value2);
                    connStatus.TryRemove(guidList[key], out value3);
                    guidList.TryRemove(key, out value4);
                }
                unlockedRouting.Set();
            }
            public static List<RoutingEntry> GetList()
            {
                lock (lockEvents)
                    unlockedRouting.Reset();
                getListWaiting = true;
                numOfRoutings.Wait();
                masterAccess.Wait();
                List<RoutingEntry> routingList = new List<RoutingEntry>();
                foreach (KeyValuePair<ConnId, Guid> entry in guidList)
                    routingList.Add(new RoutingEntry(entry.Key, physicalWorkers[routingTable[entry.Value]].Guid));
                getListWaiting = false;
                masterAccess.Release();
                if (!cleanUpWaiting)
                    unlockedRouting.Set();
                return routingList;
            }
            public static void RequestAccess()
            {
                lock (lockEvents)
                {
                    unlockedRouting.Wait();
                    lock (numOfRoutings)
                    {
                        if (numOfRoutings.IsSet)
                            numOfRoutings.Reset(1);
                        else
                            numOfRoutings.AddCount();
                    }
                }
            }
            public static void ReleaseAccess()
            {
                lock (numOfRoutings)
                    numOfRoutings.Signal();
            }

            public class RoutingEntry
            {
                public IP.SocketID SocketID = new IP.SocketID();
                public string InterfaceGuid;

                public RoutingEntry(ConnId connID, string guid)
                {
                    SocketID.LocalEP = new IPEndPoint(IPAddress.Parse(Global.Config.LoadBalancer.IPv4LocalAddresses[0].Address), connID.lPort);
                    SocketID.RemoteEP = new IPEndPoint(IPAddress.Parse(connID.rIp.ToString()), connID.rPort);
                    SocketID.Protocol = (IP.ProtocolFamily)connID.protocol;
                    InterfaceGuid = guid;
                }
            }
        }

        public struct RoutingObject
        {
            public uint seq;
            public uint ack;
            public int response;
            public int ifIndex;
            public Guid guid;
            public RoutingObject(int response, int ifIndex)
            {
                this.seq = 0;
                this.ack = 0;
                this.ifIndex = ifIndex;
                this.guid = new Guid();
                this.response = response;
            }
        }
    }
}
