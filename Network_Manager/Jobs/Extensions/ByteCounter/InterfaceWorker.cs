using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
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
    public partial class ByteCounter
    {
        class InterfaceWorker
        {
            public string Guid;
            public string Name;
            public ManualResetEventSlim Initialized = new ManualResetEventSlim(false);
            private ManualResetEventSlim threadActive = new ManualResetEventSlim(true);
            private string ifHardwareAddressString;
            private MacAddress ifHardwareAddress;
            private byte[] ifHardwareAddressByte;
            private PacketCommunicator communicator;

            public InterfaceWorker(string guid)
            {
                Guid = guid;
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                // hopefully all captured device types have a MAC address
                foreach (NetworkInterface adapter in nics)
                    if (adapter.Id == Guid)
                    {
                        ifHardwareAddressByte = adapter.GetPhysicalAddress().GetAddressBytes();
                        ifHardwareAddressString = ifHardwareAddressByte.BytesSequenceToHexadecimalString(":");
                        ifHardwareAddress = new MacAddress(ifHardwareAddressString);
                        Name = adapter.Name;
                        break;
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
                    Global.ShowTrayTip("Byte Counter", "Interface " + Name + " not captured by WinPcap.", System.Windows.Forms.ToolTipIcon.Warning);
                    Global.WriteLog("Byte Counter: Interface " + Name + " not captured by WinPcap.");
                    return;
                }

                try
                {
                    using (communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.MaximumResponsiveness, 1000))
                    {
                        Global.WriteLog("Byte Counter: Listening on " + Name + "...");
                        communicator.SetFilter("((ether dst " + ifHardwareAddressString + " or ether src " + ifHardwareAddressString + ") and ip and (tcp or udp))");
                        Initialized.Set();
                        communicator.ReceivePackets(0, (packet) =>
                        {
                            if (!threadActive.IsSet)
                            {
                                communicator.Break();
                                return;
                            }
                            Table.CheckPacket(packet, ifHardwareAddress);
                        });
                    }
                }
                catch (Exception e)
                {
                    if (threadActive.IsSet)
                    {
                        threadActive.Reset();
                        Global.WriteLog("Byte Counter: Could not capture traffic from " + Name);
                        if (Global.Config.Gadget.Debug)
                            Global.WriteLog(e.ToString());
                        Global.ShowTrayTip("Byte Counter", "Could not capture traffic from " + Name, System.Windows.Forms.ToolTipIcon.Warning);
                    }
                }
            }

            public void Stop()
            {
                threadActive.Reset();
                try { communicator.Break(); }
                catch { }
            }
        }
    }
}
