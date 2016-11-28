using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinLib.Network;
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
using WinLib.Forms;

namespace Network_Manager.Jobs.Extensions
{
    // TODO: LoadBalancer code cleanup
    public partial class LoadBalancer
    {
        public static LBStatus Status = new LBStatus();
        public static IEnumerable<NetworkInterface> Interfaces;
        private static List<PhysicalWorker> physicalWorkers = new List<PhysicalWorker>();
        private static TapWorker tapWorker = null;
        private static int MTU = 1500;
        private static int MSS = 1460;
        private static object routingInterfaceLock = new object();
        private static int _routingInterface = 0;
        private static int routingInterface
        {
            get
            {
                lock(routingInterfaceLock)
                {
                    return _routingInterface;
                }
            }
            set
            {
                lock(routingInterfaceLock)
                {
                    _routingInterface = value;
                }
            }
        }

        public static bool Start(IEnumerable<NetworkInterface> requiredNics)
        {
            Global.WriteLog("Load Balancer is starting.", true);
            Status.Update(State.Starting);
            if (requiredNics.Count() == 0)
            {
                Global.WriteLog("Load Balancer can't start without any physical interface.", true);
                Global.ShowTrayTip("Load Balancer", "Can't start whitout any physical interface", System.Windows.Forms.ToolTipIcon.Error);
                Status.Update(State.Failed);
                return false;
            }
            if (!Jobs.Extensions.Dependencies.Check())
            {
                Status.Update(State.Failed);
                return false;
            }
            if (!TapInterface.PutUp())
            {
                Global.WriteLog("Load Balancer failed to connect to " + TapInterface.FriendlyName, true);
                Global.ShowTrayTip("Load Balancer", "Failed to connect to " + TapInterface.FriendlyName, System.Windows.Forms.ToolTipIcon.Error);
                Status.Update(State.Failed);
                return false;
            }
            if (!Dependencies.RunWinPcapService(requiredNics, true))
            {
                Global.WriteLog("Load Balancer failed to start because some interfaces were not captured by WinPcap.", true);
                Global.ShowTrayTip("Load Balancer", "Failed to start", System.Windows.Forms.ToolTipIcon.Error);
                TapInterface.PutDown();
                Status.Update(State.Failed);
                return false;
            }
            Interfaces = requiredNics;
            // start LB threads
            LoadingForm splash = new LoadingForm("Initializing ...");
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
                if (nic.Guid != TapInterface.Guid &&
                    (nic.IPv4Gateway.Count > 0 || nic.IPv6Gateway.Count > 0))
                {
                    splash.UpdateStatus("Configuring " + nic.Name + " ...");
                    nic.SetInterfaceMetric("4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv4Gateway)
                        nic.EditIPv4Gateway(ip.Address, "4000");
                    foreach (NetworkInterface.IPGatewayAddress ip in nic.IPv6Gateway)
                        nic.EditIPv6Gateway(ip.Address, "4000");
                }
            splash.UpdateStatus("Initializing " + TapInterface.FriendlyName + " ...");
            physicalWorkers.Clear();
            tapWorker = new TapWorker(TapInterface.Guid, TapInterface.FriendlyName, TapInterface.Mac,
                Global.Config.LoadBalancer.IPv4LocalAddresses.First().Address, Global.Config.LoadBalancer.IPv4GatewayAddresses.First().Address);
            new Thread(new ThreadStart(tapWorker.ReceivePackets)).Start();
            tapWorker.Initialized.Wait(1000);
            foreach (NetworkInterface nic in requiredNics)
            {
                splash.UpdateStatus("Initializing " + nic.Name + " ...");
                physicalWorkers.Add(new PhysicalWorker(nic.Guid, nic.Name, nic.Mac, nic.IPv4Address.First().Address,
                    nic.IPv4Address.First().Subnet, nic.DefaultIPv4GatewayMac, nic.DefaultIPv4Gateway));
                new Thread(new ThreadStart(physicalWorkers.Last().ReceivePackets)).Start();
                physicalWorkers.Last().Initialized.Wait(10000);
            }
            int mtu;
            MTU = requiredNics.Min((i) => int.TryParse(i.IPv4Mtu, out mtu) ? mtu : 1500);
            Global.WriteLog("Load Balancer: Negociated MTU = " + MTU);
            MSS = (ushort)(MTU - 40);
            splash.Stop();
            Dependencies.WinPcapInUse.Reset(Dependencies.WinPcapInUse.CurrentCount + 1);
            Global.WriteLog("Load Balancer: started");
            Global.ShowTrayTip("Load Balancer", "Started", System.Windows.Forms.ToolTipIcon.Info);
            Status.Update(State.Running);
            new Thread(new ThreadStart(CheckUp)).Start();
            return true;
        }

        private static void CheckUp()
        {
            int natCleanUpDelay = 60;
            while (Status.State == State.Running)
            {
                if (natCleanUpDelay < 1)
                {
                    RoutingTable.CleanUp();
                    natCleanUpDelay = 60;
                }
                if (physicalWorkers.Any(i => !i.ThreadActive.IsSet) ||
                    !tapWorker.ThreadActive.IsSet)
                {
                    Stop();
                    break;
                }
                double lowestTrafficRate = -1;
                int currentRoutingInterface = 0;
                for (int i = 0; i < physicalWorkers.Count; i++)
                {
                    double currentTrafficRate = Global.NetworkInterfaces[physicalWorkers[i].Guid].IPv4InSpeedAvg10 +
                        Global.NetworkInterfaces[physicalWorkers[i].Guid].IPv4OutSpeedAvg10;
                    if (currentTrafficRate < lowestTrafficRate || lowestTrafficRate == -1)
                    {
                        lowestTrafficRate = currentTrafficRate;
                        currentRoutingInterface = i;
                    }
                }
                routingInterface = currentRoutingInterface;
                natCleanUpDelay--;
                Thread.Sleep(1000);
            }
        }

        public static bool Stop()
        {
            Status.Update(State.Stopping);
            TapInterface.PutDown();
            LoadingForm splash = new LoadingForm("Stopping Load Balancer ...");
            if (physicalWorkers.Count() > 0)
            {
                for (int i = physicalWorkers.Count() - 1; i >= 0; i--)
                {
                    splash.UpdateStatus("Stop capturing from " + physicalWorkers[i].Name + " ...");
                    physicalWorkers[i].Stop();
                    physicalWorkers.RemoveAt(i);
                }
            }
            splash.UpdateStatus("Stop capturing from " + tapWorker.Name + " ...");
            tapWorker.Stop();
            splash.UpdateStatus("Clearing routing table ...");
            RoutingTable.Clear();
            Dependencies.WinPcapInUse.Signal();
            splash.Stop();
            Global.WriteLog("Load Balancer stopped.", true);
            Status.Update(State.Stopped);
            return true;
        }

        // TODO: enhance LB status information
        public class LBStatus
        {
            public string Message = "Stoped";
            public Color Color = Color.Red;
            public State State = State.Stopped;
            public event EventHandler Changed;

            public void Update(State state)
            {
                State = state;
                Message = state.ToString();
                if (state == LoadBalancer.State.Stopped)
                    Color = Color.Red;
                if (state == LoadBalancer.State.Starting)
                    Color = Color.Orange;
                if (state == LoadBalancer.State.Failed)
                    Color = Color.Red;
                if (state == LoadBalancer.State.Running)
                    Color = Color.Green;
                if (state == LoadBalancer.State.Stopping)
                    Color = Color.Orange;
                if (Changed != null)
                    Changed(null, null);
            }
        }

        public enum State
        {
            Stopped,
            Starting,
            Failed,
            Running,
            Stopping
        }
    }

    // STRUCTURES

    public class FragmentationBuffer
    {
        struct DictKey
        {
            public DictKey(IpV4Address ip, ushort id)
            {
                this.Ip = ip;
                this.Identification = id;
            }
            private IpV4Address Ip;
            private ushort Identification;
        }
        class DictValue
        {
            public IList<Packet> fragments = new List<Packet>();
            private int offset = 0;
            private ushort lastOffset = 0;

            public bool AddPacket(Packet packet)
            {
                fragments.Add(packet);
                if (packet.Ethernet.IpV4.Fragmentation.Options == IpV4FragmentationOptions.None && packet.Ethernet.IpV4.Fragmentation.Offset > 0)
                    lastOffset = packet.Ethernet.IpV4.Fragmentation.Offset;
                else
                    offset += packet.Ethernet.IpV4.Length - packet.Ethernet.IpV4.HeaderLength;
                if (offset == lastOffset)
                    return true;
                else
                    return false;
            }
        }
        private Dictionary<DictKey, DictValue> ElementList = new Dictionary<DictKey, DictValue>();
        private DictKey dictKey;
        private DictValue dictValue;
        private IList<Packet> fragments;

        public IList<Packet> Add(Packet packet)
        {
            dictKey = new DictKey(packet.Ethernet.IpV4.Source, packet.Ethernet.IpV4.Identification);
            if (ElementList.TryGetValue(dictKey, out dictValue))
            {
                if (dictValue.AddPacket(packet))
                {
                    fragments = dictValue.fragments;
                    ElementList.Remove(dictKey);
                    return fragments;
                }
                else
                {
                    fragments = null;
                    return fragments;
                }
            }
            else
            {
                dictValue = new DictValue();
                dictValue.AddPacket(packet);
                ElementList.Add(dictKey, dictValue);
                fragments = null;
                return fragments;
            }
        }
    }

    static class IpFunctions
    {
        // Calculate ICMP checksum (same as IP, TCP, UDP, etc) - rfc1071
        public static ushort ComputeIpChecksum(byte[] data)
        {
            long sum = 0;
            int count = 0;
            int remaining = data.Length;

            while (remaining > 1)
            {
                sum += (long)((ushort)(data[count] << 8) | data[count + 1]);
                count += 2;
                remaining -= 2;
            }

            if (remaining > 0)
                sum += (long)(data[count] << 8);

            while (sum >> 16 != 0)
                sum = (sum & 0xffff) + (sum >> 16);

            sum = ~sum;
            return (ushort)sum;
        }
        // Modulo 2**32 addition
        public static uint AddM32(uint a, uint b, uint c = 0)
        {
            return (uint)(((UInt64)a + (UInt64)b + (UInt64)c) % 0x100000000);
        }
        // Modulo 2**32 subtraction
        public static uint SubM32(uint a, uint b, uint c = 0)
        {
            return (uint)((((UInt64)a - (UInt64)b - (UInt64)c) + 0x100000000) % 0x100000000);
        }
    }

    struct Dhcp
    {
        public byte MessageType;
        public byte HardwareType;
        public byte HardwareAddressLength;
        public byte Hops;
        public uint Id;
        public ushort SecondsElapsed;
        public ushort Flags;
        public byte[] ClientIp;
        public byte[] YourIp;
        public byte[] ServerIp;
        public byte[] GatewayIp;
        public byte[] ClientHardwareAddress;
        public byte[] HardwareAddressPadding;
        public byte[] ServerHostName;
        public byte[] BootFileName;
        public uint MagicCookie;
        public byte[] Options;
        public Dhcp(int optionsLength)
        {
            Random random = new Random();
            MessageType = 1;
            HardwareType = 1;
            HardwareAddressLength = 6;
            Hops = 0;
            Id = (uint)(random.Next(0xffff)) << 16 | (uint)(random.Next(0xffff));
            SecondsElapsed = 0;
            Flags = 0;
            ClientIp = new byte[4];
            YourIp = new byte[4];
            ServerIp = new byte[4];
            GatewayIp = new byte[4];
            ClientHardwareAddress = new byte[6];
            HardwareAddressPadding = new byte[10];
            ServerHostName = new byte[64];
            BootFileName = new byte[128];
            MagicCookie = 0x63825363;
            Options = new byte[optionsLength];
        }
        public Dhcp(byte[] buffer)
        {
            MessageType = buffer[0];
            HardwareType = buffer[1];
            HardwareAddressLength = buffer[2];
            Hops = buffer[3];
            Id = (uint)(buffer[4] << 24 | buffer[5] << 16 | buffer[6] << 8 | buffer[7]);
            SecondsElapsed = (ushort)(buffer[8] << 8 | buffer[9]);
            Flags = (ushort)(buffer[10] << 8 | buffer[11]);
            ClientIp = buffer.Skip(12).Take(4).ToArray();
            YourIp = buffer.Skip(16).Take(4).ToArray();
            ServerIp = buffer.Skip(20).Take(4).ToArray();
            GatewayIp = buffer.Skip(24).Take(4).ToArray();
            ClientHardwareAddress = buffer.Skip(28).Take(6).ToArray();
            HardwareAddressPadding = buffer.Skip(34).Take(10).ToArray();
            ServerHostName = buffer.Skip(44).Take(64).ToArray();
            BootFileName = buffer.Skip(108).Take(128).ToArray();
            MagicCookie = (uint)(buffer[236] << 24 | buffer[237] << 16 | buffer[238] << 8 | buffer[239]);
            Options = buffer.Skip(240).Take(buffer.Length - 240).ToArray();
        }
        public byte[] getOption(int code)
        {
            byte[] option = null;
            int length;
            for (int i = 0; i < Options.Length; i++)
            {
                switch (Options[i])
                {
                    case 0:
                        break;
                    case 255:
                        break;
                    default:
                        length = Options[i + 1];
                        if (Options[i] == code)
                            Buffer.BlockCopy(Options, i + 2, option = new byte[length], 0, length);
                        i = i + length + 1;
                        break;
                }
            }
            return option;
        }
        public byte[] toByteArray()
        {
            int size = 240 + Options.Length;
            byte[] array = new byte[size];
            Buffer.BlockCopy(new byte[] { MessageType }, 0, array, 0, 1);
            Buffer.BlockCopy(new byte[] { HardwareType }, 0, array, 1, 1);
            Buffer.BlockCopy(new byte[] { HardwareAddressLength }, 0, array, 2, 1);
            Buffer.BlockCopy(new byte[] { Hops }, 0, array, 3, 1);
            Buffer.BlockCopy(new byte[] { (byte)(Id >> 24), (byte)(Id >> 16), (byte)(Id >> 8), (byte)(Id) }, 0, array, 4, 4);
            Buffer.BlockCopy(new byte[] { (byte)(SecondsElapsed >> 8), (byte)(SecondsElapsed) }, 0, array, 8, 2);
            Buffer.BlockCopy(new byte[] { (byte)(Flags >> 8), (byte)(Flags) }, 0, array, 10, 2);
            Buffer.BlockCopy(ClientIp, 0, array, 12, 4);
            Buffer.BlockCopy(YourIp, 0, array, 16, 4);
            Buffer.BlockCopy(ServerIp, 0, array, 20, 4);
            Buffer.BlockCopy(GatewayIp, 0, array, 24, 4);
            Buffer.BlockCopy(ClientHardwareAddress, 0, array, 28, 6);
            Buffer.BlockCopy(HardwareAddressPadding, 0, array, 34, 10);
            Buffer.BlockCopy(ServerHostName, 0, array, 44, 64);
            Buffer.BlockCopy(BootFileName, 0, array, 108, 128);
            Buffer.BlockCopy(new byte[] { (byte)(MagicCookie >> 24), (byte)(MagicCookie >> 16), (byte)(MagicCookie >> 8), (byte)(MagicCookie) }, 0, array, 236, 4);
            Buffer.BlockCopy(Options, 0, array, 240, Options.Length);
            return array;
        }
    };
    enum DhcpOptionCode
    {
        Pad = 0,
        SubnetMask = 1,
        Hostname = 12,
        AddressTime = 51,
        DhcpMsgType = 53,
        DhcpServerId = 54,
        ParameterList = 55,
        ClassId = 60,
        ClientId = 61,
        End = 255
    };
}
