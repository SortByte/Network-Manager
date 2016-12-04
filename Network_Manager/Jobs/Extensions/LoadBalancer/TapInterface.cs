using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using WinLib.WinAPI;
using WinLib.Network;
using WinLib.Forms;

namespace Network_Manager.Jobs.Extensions
{
    public partial class LoadBalancer
    {
        public static class TapInterface
        {
            public static Guid Guid;
            public static string FriendlyName;
            public static string Mac;
            public static int Index;
            public static bool Connected = false;
            private const string componentID = "tap0901";
            private static IntPtr handle = IntPtr.Zero;
            

            /// <summary>
            /// Checks if there is any TAP interface ready to be used (disconnected or connected with a valid handle)
            /// </summary>
            /// <returns></returns>
            public static bool Check()
            {
                LoadingForm splash = LoadingForm.Create("Checking TAP interface ...");
                if (handle != IntPtr.Zero)
                {
                    IntPtr pTapVersion = Marshal.AllocHGlobal(2000);
                    uint bytesReturned = 0;
                    if (Kernel32.DeviceIoControl(handle, TAP_IOCTL_GET_VERSION, pTapVersion, 2000, pTapVersion, 2000, ref bytesReturned, IntPtr.Zero))
                    {
                        Marshal.FreeHGlobal(pTapVersion);
                        Connected = true;
                        splash.Stop();
                        return true;
                    }
                    else
                    {
                        Marshal.FreeHGlobal(pTapVersion);
                        handle = IntPtr.Zero;
                    }
                }
                // not connected; seeking available interface
                Connected = false;
                Guid = Guid.Empty;
                string cfgKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
                string nameKeyPath = @"SYSTEM\CurrentControlSet\Control\Network\{4d36e972-e325-11ce-bfc1-08002be10318}";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(cfgKeyPath);
                foreach (string name in key.GetSubKeyNames())
                {
                    try
                    {
                        if ((string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("ComponentId") == componentID)
                        {
                            string netCfgInstanceId = (string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("NetCfgInstanceId");
                            string friendlyName = (string)Registry.LocalMachine.OpenSubKey(nameKeyPath + @"\" + netCfgInstanceId + @"\Connection").GetValue("Name");
                            if (NetworkInterface.GetAdapterStatus(friendlyName) == NetworkInterface.Status.MediaDisconnected ||
                                NetworkInterface.GetAdapterStatus(friendlyName) == NetworkInterface.Status.Disconnected)
                            {
                                Guid = new Guid(netCfgInstanceId);
                                FriendlyName = friendlyName;
                                splash.Stop();
                                return true;
                            }
                        }
                    }
                    catch { }
                }
                key.Close();
                splash.Stop();
                return false;
            }

            public static bool IsTap(Guid guid)
            {
                string cfgKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(cfgKeyPath);
                foreach (string name in key.GetSubKeyNames())
                {
                    try
                    {
                        if ((string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("ComponentId") == componentID)
                        {
                            string netCfgInstanceId = (string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("NetCfgInstanceId");
                            if (netCfgInstanceId.ToUpper().Contains(guid.ToString().ToUpper()))
                                return true;
                        }
                    }
                    catch { }
                }
                key.Close();
                return false;
            }

            public static bool PutUp()
            {
                if (!Check())
                    return false;
                if (handle != IntPtr.Zero)
                    return true;
                LoadingForm splash = LoadingForm.Create("Connecting to \"" + FriendlyName + "\" ...");
                string duplicateName;
                if ((duplicateName = NetworkInterface.CheckIfIPv4Used(Global.Config.LoadBalancer.IPv4LocalAddresses[0].Address, Guid)) != null)
                {
                    splash.Stop();
                    Global.WriteLog("TAP Interface: IP address " + Global.Config.LoadBalancer.IPv4LocalAddresses[0].Address + " already used by \"" + duplicateName + "\"");
                    MessageBox.Show("\"" + FriendlyName + "\" can't use the IP address " + Global.Config.LoadBalancer.IPv4LocalAddresses[0].Address + " because it's already used by \"" + duplicateName + "\".\n\n Set a different IPv4 address in Control Panel>Tools>Load balancing>Advanced.", "TAP Interface", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                handle = Kernel32.CreateFile(@"\\.\Global\{" + Guid + "}.tap",
                    Kernel32.FILE_READ_DATA | Kernel32.FILE_WRITE_DATA,
                    Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    Kernel32.OPEN_EXISTING,
                    Kernel32.FILE_ATTRIBUTE_SYSTEM | Kernel32.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);
                if (handle == Kernel32.INVALID_HANDLE_VALUE)
                {
                    uint errorCode = Kernel32.GetLastError();
                    splash.Stop();
                    Global.WriteLog("TAP Interface: failed to connect to " + FriendlyName + ": " + Kernel32.GetLastErrorMessage(errorCode));
                    MessageBox.Show("Failed to connect to " + FriendlyName + ":\n" + Kernel32.GetLastErrorMessage(errorCode), "TAP Interface", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                // set TAP status to connected
                uint bytesReturned = 0;
                IntPtr pStatus = Marshal.AllocHGlobal(4);
                Marshal.Copy(BitConverter.GetBytes(1), 0, pStatus, 4);
                bool deviceStatus = Kernel32.DeviceIoControl(handle, TAP_IOCTL_SET_MEDIA_STATUS, pStatus, 4, pStatus, 4, ref bytesReturned, IntPtr.Zero);
                Marshal.FreeHGlobal(pStatus);
                // get TAP MAC address
                bytesReturned = 0;
                IntPtr pMac = Marshal.AllocHGlobal(8);
                bool macRetrieved = Kernel32.DeviceIoControl(handle, TAP_IOCTL_GET_MAC, pMac, 12, pMac, 12, ref bytesReturned, IntPtr.Zero);
                byte[] mac = new byte[bytesReturned];
                Marshal.Copy(pMac, mac, 0, (int)bytesReturned);
                Mac = BitConverter.ToString(mac).Replace('-', ':');
                Marshal.FreeHGlobal(pMac);
                // configure TAP
                splash.UpdateStatus("Configuring " + FriendlyName + " ...");
                List<Iphlpapi.Adapter> adapters = Iphlpapi.GetAdapters(Iphlpapi.FAMILY.AF_UNSPEC);
                Iphlpapi.Adapter tap = adapters.Find(i => i.Guid == Guid);
                if (tap == null)
                {
                    splash.Stop();
                    Global.WriteLog("TAP Interface: couldn't find " + FriendlyName + " index");
                    MessageBox.Show("Couldn't find " + FriendlyName + " index.", "TAP Interface", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PutDown();
                    return false;
                }
                Index = tap.InterfaceIndex;
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    NetworkInterface.SetInterfaceMetric(Mac, "1");
                else
                    NetworkInterface.SetInterfaceMetric(Index, "1");
                NetworkInterface.SetNetBios(Guid, false);
                NetworkInterface.ClearIPv4Addresses(Mac, Global.Config.LoadBalancer.IPv4LocalAddresses[0].Address, Global.Config.LoadBalancer.IPv4LocalAddresses[0].Subnet);
                NetworkInterface.ClearGateways(Index);
                NetworkInterface.ClearIPv4DnsServers(Mac);
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                    NetworkInterface.AddIPv4Gateway(Mac, Global.Config.LoadBalancer.IPv4GatewayAddresses[0].Address, Global.Config.LoadBalancer.IPv4GatewayAddresses[0].GatewayMetric.ToString());
                else
                    NetworkInterface.AddIPv4Gateway(Index, Global.Config.LoadBalancer.IPv4GatewayAddresses[0].Address, Global.Config.LoadBalancer.IPv4GatewayAddresses[0].GatewayMetric.ToString());
                NetworkInterface.SetIPv4DnsServer(FriendlyName, Global.Config.LoadBalancer.IPv4DnsAddresses[0]);
                splash.Stop();
                if (handle != IntPtr.Zero && handle != Kernel32.INVALID_HANDLE_VALUE && deviceStatus && macRetrieved)
                    return true;
                else
                    return false;
            }

            public static bool PutDown()
            {
                if (handle == IntPtr.Zero)
                    return true;
                LoadingForm splash = LoadingForm.Create("Disconnecting from \"" + FriendlyName + "\" ...");
                uint bytesReturned = 0;
                IntPtr pBuffer = Marshal.AllocHGlobal(4);
                Marshal.Copy(BitConverter.GetBytes(0), 0, pBuffer, 4);
                bool deviceStatus = Kernel32.DeviceIoControl(handle, TAP_IOCTL_SET_MEDIA_STATUS, pBuffer, 4, pBuffer, 4, ref bytesReturned, IntPtr.Zero);
                Marshal.FreeHGlobal(pBuffer);
                Kernel32.CloseHandle(handle);
                handle = IntPtr.Zero;
                splash.Stop();
                return deviceStatus;
            }

            public static bool Install()
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to create a TAP network interface?",
                    "TAP Driver Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No)
                    return false;
                LoadingForm splash = LoadingForm.Create("Decompressing TAP driver...");
                Directory.CreateDirectory("Temp");
                File.WriteAllBytes(@"Temp\TAP Driver.zip", Network_Manager.Properties.Resources.TAP_Driver);
                WinLib.IO.Compression.UnZip(@"Temp\TAP Driver.zip", "Temp");
                // Vista driver version does not install correctly on Vista, so Win2K is used
                string driverPath;
                if (Environment.Is64BitOperatingSystem)
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.1")) > -1)
                        driverPath = @"Temp\TAP Driver\Vista 64-bit\OemVista.inf";
                    else
                        driverPath = @"Temp\TAP Driver\XP 64-bit\OemWin2k.inf";
                else
                    if (Environment.OSVersion.Version.CompareTo(new Version("6.1")) > -1)
                        driverPath = @"Temp\TAP Driver\Vista 32-bit\OemVista.inf";
                    else
                        driverPath = @"Temp\TAP Driver\XP 32-bit\OemWin2k.inf";
                splash.UpdateStatus("Installing TAP driver ...");
                if (Setupapi.InstallInfDriver(driverPath, componentID, "Network Manager TAP Adapter"))
                {
                    File.Delete("TAP Driver.zip");
                    splash.Stop();
                    MessageBox.Show("TAP network interface was successfully created.", "TAP Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    File.Delete("TAP Driver.zip");
                    splash.Stop();
                    MessageBox.Show("TAP network interface failed to create.", "TAP Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            public static bool Uninstall()
            {
                string cfgKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
                string nameKeyPath = @"SYSTEM\CurrentControlSet\Control\Network\{4d36e972-e325-11ce-bfc1-08002be10318}";
                string tapList = "";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(cfgKeyPath);
                foreach (string name in key.GetSubKeyNames())
                {
                    try
                    {
                        if ((string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("ComponentId") == componentID)
                        {
                            string netCfgInstanceId = (string)Registry.LocalMachine.OpenSubKey(cfgKeyPath + @"\" + name).GetValue("NetCfgInstanceId");
                            string friendlyName = (string)Registry.LocalMachine.OpenSubKey(nameKeyPath + @"\" + netCfgInstanceId + @"\Connection").GetValue("Name");
                            tapList += friendlyName + "\n";
                        }
                    }
                    catch { }
                }
                key.Close();
                if (tapList == "")
                {
                    MessageBox.Show("No TAP interface was found.", "TAP Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                DialogResult dialogResult = MessageBox.Show("This will remove the following network interfaces:\n\n" +
                    tapList + "\nDo you want to remove all these TAP network interfaces?",
                    "TAP Driver Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No)
                    return false;
                LoadingForm splash = LoadingForm.Create("Uninstalling TAP driver ...");
                if (Setupapi.UninstallDevice(componentID))
                {
                    splash.Stop();
                    MessageBox.Show("Uninstallation was successful.", "TAP Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    splash.Stop();
                    MessageBox.Show("Uninstallation failed.", "TAP Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            private static uint TAP_CONTROL_CODE(uint request, uint method)
            {
                return FILE_DEVICE_UNKNOWN << 16 | FILE_ANY_ACCESS << 14 | request << 2 | method;
            }

            private const uint FILE_DEVICE_UNKNOWN = 0x000000022;
            private const uint FILE_ANY_ACCESS = 0;
            private const uint METHOD_BUFFERED = 0;
            private static uint TAP_IOCTL_GET_MAC               = TAP_CONTROL_CODE(1, METHOD_BUFFERED);
            private static uint TAP_IOCTL_GET_VERSION           = TAP_CONTROL_CODE(2, METHOD_BUFFERED);
            private static uint TAP_IOCTL_GET_MTU               = TAP_CONTROL_CODE(3, METHOD_BUFFERED);
            private static uint TAP_IOCTL_GET_INFO              = TAP_CONTROL_CODE(4, METHOD_BUFFERED);
            private static uint TAP_IOCTL_CONFIG_POINT_TO_POINT = TAP_CONTROL_CODE(5, METHOD_BUFFERED);
            private static uint TAP_IOCTL_SET_MEDIA_STATUS      = TAP_CONTROL_CODE(6, METHOD_BUFFERED);
            private static uint TAP_IOCTL_CONFIG_DHCP_MASQ      = TAP_CONTROL_CODE(7, METHOD_BUFFERED);
            private static uint TAP_IOCTL_GET_LOG_LINE          = TAP_CONTROL_CODE(8, METHOD_BUFFERED);
            private static uint TAP_IOCTL_CONFIG_DHCP_SET_OPT   = TAP_CONTROL_CODE(9, METHOD_BUFFERED);
            private static uint TAP_IOCTL_CONFIG_TUN            = TAP_CONTROL_CODE(10, METHOD_BUFFERED);
        }
    }
}
