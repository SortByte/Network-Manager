using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.Win32;
using WinLib.Network;
using WinLib.Network.Http;
using WinLib.Forms;


namespace Network_Manager.Jobs.Extensions
{
    public partial class Dependencies : Form
    {
        public static bool VC2010Installed;
        public static string VC2010InstalledVersion;
        public static bool WinPcapInstalled;
        public static string WinPcapInstalledVersion;
        public static List<string> WinPcapDevices = new List<string>();
        public static CountdownEvent WinPcapInUse = new CountdownEvent(0);
        [DllImport("wpcap.dll")]
        public static extern IntPtr pcap_lib_version();
        [DllImport("wpcap.dll", CharSet = CharSet.Ansi)]
        public static extern int pcap_findalldevs(ref IntPtr pFirstDev, IntPtr pErrBuff);
        [DllImport("wpcap.dll")]
        public static extern void pcap_freealldevs(IntPtr pFirstDev);

        public Dependencies()
        {
            InitializeComponent();
            if (IntPtr.Size == 8)
                label2.Text = "Microsoft Visual C++ 2010 x64 Redistributable";
            else
                label2.Text = "Microsoft Visual C++ 2010 x86 Redistributable";
            UpdateGui();
        }

        private void UpdateGui()
        {
            if (VC2010Installed)
            {
                labelVC2010Installed.Text = VC2010InstalledVersion + " installed";
                labelVC2010Installed.ForeColor = Color.Green;
                button1.Hide();
            }
            else
            {
                labelVC2010Installed.Text = "Not installed";
                labelVC2010Installed.ForeColor = Color.Red;
                button1.Show();
            }
            if (WinPcapInstalled)
            {
                labelWinPcapInstalled.Text = WinPcapInstalledVersion + " installed";
                labelWinPcapInstalled.ForeColor = Color.Green;
                button2.Hide();
            }
            else
            {
                if (WinPcapInstalledVersion == null)
                    labelWinPcapInstalled.Text = "Not installed";
                else
                    labelWinPcapInstalled.Text = WinPcapInstalledVersion + " is outdated";
                labelWinPcapInstalled.ForeColor = Color.Red;
                button2.Show();
            }
        }

        public static bool Check()
        {
            CheckVC2010();
            CheckWinPcap();
            if (!VC2010Installed || !WinPcapInstalled)
            {
                Dependencies form = new Dependencies();
                DialogResult result = form.ShowDialog();
                return result == DialogResult.OK;
            }
            return true;
        }

        private void Download(string link, string filePath)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(link);
            request.AllowAutoRedirect = false;
            request.UserAgent = Headers.DefaultUserAgent;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                string result = "";
                byte[] buffer = new byte[512];
                int size;
                while ((size = stream.Read(buffer, 0, 512)) > 0)
                    result += System.Text.Encoding.ASCII.GetString(buffer, 0, size);
                if (Regex.IsMatch(result, @"^[a-f\d]+\n\d+$", RegexOptions.IgnoreCase))
                {
                    int crc32 = int.Parse(Regex.Replace(result, @"^([a-f\d]+)\n.+$", "$1", RegexOptions.IgnoreCase), System.Globalization.NumberStyles.HexNumber);
                    long length = long.Parse(Regex.Replace(result, @"^[a-f\d]+\n(\d+)$", "$1", RegexOptions.IgnoreCase));
                    new Download(link, @"Downloads\" + filePath, crc32, length, DownloadCompleted);
                }
                else
                    MessageBox.Show("Failed to download the update.\nBroken link or no internet access !", "Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Unable to connect to server:\n\n" + ex.Message, "Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void DownloadCompleted(bool success, string filePath)
        {
            if (success)
            {
                if (Regex.IsMatch(filePath, "winpcap", RegexOptions.IgnoreCase))
                    if (Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(i => i.ModuleName == "wpcap.dll" || i.ModuleName == "packet.dll"))
                    {
                        MessageBox.Show("'wpcap.dll' or 'packet.dll' is in use.\nThe application will close now, so you can install WinPcap.\nIf that doesn't work, try rebooting the system.\n\nYou'll have to manually start Network Manager after WinPcap installation is finished.", "Download", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Process.Start(new ProcessStartInfo(filePath));
                        Global.Exit();
                        return;
                    }
                Process.Start(new ProcessStartInfo(filePath));
            }       
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string link = null;
            if (IntPtr.Size == 8)
            {
                link = "http://download.sortbyte.com/nm/vcredist2010_64";
                Download(link, "vcredist_x64.exe");
            }
            else
            {
                link = "http://download.sortbyte.com/nm/vcredist2010_32";
                Download(link, "vcredist_x86.exe");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Download("http://download.sortbyte.com/nm/winpcap", "WinPcap_4_1_3.exe");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckVC2010();
            CheckWinPcap();
            if (VC2010Installed && WinPcapInstalled)
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            UpdateGui();
        }

        public static bool CheckVC2010()
        {
            RegistryKey key;
            // get VC redist version
            if (IntPtr.Size == 8)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\VC\VCRedist\x64");
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x86");
            if (key != null)
            {
                uint installed = Convert.ToUInt32(key.GetValue("Installed"));
                string version = (string)key.GetValue("Version");
                VC2010Installed = installed > 0;
                VC2010InstalledVersion = version;
                key.Close();
            }
            else
            {
                VC2010Installed = false;
                VC2010InstalledVersion = null;
            }
            return VC2010Installed;
        }

        public static bool CheckWinPcap()
        {
            // get WinPcap version
            if (File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\wpcap.dll") &&
                File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\Packet.dll") &&
                ServiceController.GetDevices().Where(i => i.ServiceName == "NPF").Count() > 0)
            {
                string version = Marshal.PtrToStringAnsi(pcap_lib_version());
                version = Regex.Replace(version, @"^.*WinPcap version ([0-9\.]+).*$", "$1", RegexOptions.IgnoreCase);
                WinPcapInstalled = Version.Parse(version).CompareTo(Version.Parse("4.1.2")) > -1;
                WinPcapInstalledVersion = version;
            }
            else
            {
                WinPcapInstalled = false;
                WinPcapInstalledVersion = null;
            }
            return WinPcapInstalled;
        }

        public static List<string> GetWinPcapDevs()
        {
            List<string> devices = new List<string>();
            if (File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\wpcap.dll") &&
                File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\Packet.dll"))
            {
                IntPtr pFirstDev = IntPtr.Zero;
                IntPtr pErrBuff = Marshal.AllocHGlobal(PCAP_ERRBUF_SIZE);
                if (pcap_findalldevs(ref pFirstDev, pErrBuff) == 0)
                {
                    if (pFirstDev != IntPtr.Zero)
                    {
                        IntPtr pDev = pFirstDev;
                        while (pDev != IntPtr.Zero)
                        {
                            pcap_if pcap_if = (pcap_if)Marshal.PtrToStructure(pDev, typeof(pcap_if));
                            devices.Add(Regex.Replace(pcap_if.Name, @"^.*({.*})$", "$1"));
                            pDev = pcap_if.Next;
                        }
                        pcap_freealldevs(pFirstDev);
                    }
                }
                else
                {
                    if (pErrBuff != IntPtr.Zero)
                        MessageBox.Show(Marshal.PtrToStringAnsi(pErrBuff), "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Marshal.FreeHGlobal(pErrBuff);
            }
            else
            {
                if (File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\wpcap.dll"))
                    MessageBox.Show(@"""" + Environment.GetEnvironmentVariable("windir") + @"\System32\wpcap.dll"" is missing", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (File.Exists(Environment.GetEnvironmentVariable("windir") + @"\System32\Packet.dll"))
                    MessageBox.Show(@"""" + Environment.GetEnvironmentVariable("windir") + @"\System32\Packet.dll"" is missing", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return devices;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct pcap_if
        {
            public IntPtr Next;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Name;
        }

        public const int PCAP_ERRBUF_SIZE = 256;

        /// <summary>
        /// Makes sure that the "NetGroup Packet Filter" service status is "Running" and that all specified interfaces are captured.
        /// </summary>
        /// <param name="requiredNics">If this is null it only alerts the user about any non-captured interfaces and returns true</param>
        /// <returns></returns>
        public static bool RunWinPcapService(IEnumerable<NetworkInterface> requiredNics = null, bool verbose = false)
        {
            LoadingForm splash = null;
            if (verbose)
                splash = new LoadingForm("Searching \"NetGroup Packet Filter\" service ...");
            if (ServiceController.GetDevices().Where(i => i.ServiceName == "NPF").Count() == 0)
            {
                if (verbose)
                    splash.Stop();
                MessageBox.Show("\"NetGroup Packet Filter\" service was not installed by WinPcap.", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            using (ServiceController service = new ServiceController("NPF"))
            {
                bool isCapturingOptional = false;
                if (requiredNics == null)
                {
                    requiredNics = Global.NetworkInterfaces.Values.ToList();
                    isCapturingOptional = true;
                }
                if (service.Status != ServiceControllerStatus.Running)
                {
                    if (verbose)
                        splash.UpdateStatus("Starting \"NetGroup Packet Filter\" service ...");
                    if (!RestartNpfService())
                    {
                        if (verbose)
                            splash.Stop();
                        return false;
                    }
                }
                try { service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(0)); } // update status
                catch { }
                if (service.Status == ServiceControllerStatus.Running)
                {
                    // get captured devices
                    if (verbose)
                        splash.UpdateStatus("Getting devices captured by WinPcap ...");
                    WinPcapDevices = GetWinPcapDevs();
                    List<NetworkInterface> nonCapturedNics = requiredNics.Where(i => WinPcapDevices.Find(j => j.Contains(i.Guid)) == null).ToList();
                    List<NetworkInterface> capturedNics = requiredNics.Where(i => WinPcapDevices.Find(j => j.Contains(i.Guid)) != null).ToList();
                    List<NetworkInterface> supportedNonCapturedNics = nonCapturedNics.Where(i =>
                            i.Type != NetworkInterface.AdapterType.Ppp &&
                            i.Type != NetworkInterface.AdapterType.Wwanpp &&
                            i.Type != NetworkInterface.AdapterType.Wwanpp2).ToList();
                    // if there are NICs not captured by WinPcap that are supported
                    if (supportedNonCapturedNics.Count > 0)
                    {
                        if (verbose)
                            splash.UpdateStatus("Restarting \"NetGroup Packet Filter\" service ...");
                        // if WinPcap is not in use
                        if (Dependencies.WinPcapInUse.CurrentCount == 0)
                            if (!RestartNpfService())
                            {
                                if (verbose)
                                    splash.Stop();
                                return false;
                            }
                        // get captured devices again
                        if (verbose)
                            splash.UpdateStatus("Recheck devices captured by WinPcap ...");
                        WinPcapDevices = GetWinPcapDevs();
                        nonCapturedNics = requiredNics.Where(i => WinPcapDevices.Find(j => j.Contains(i.Guid)) == null).ToList();
                        capturedNics = requiredNics.Where(i => WinPcapDevices.Find(j => j.Contains(i.Guid)) != null).ToList();
                        supportedNonCapturedNics = nonCapturedNics.Where(i =>
                                i.Type != NetworkInterface.AdapterType.Ppp &&
                                i.Type != NetworkInterface.AdapterType.Wwanpp &&
                                i.Type != NetworkInterface.AdapterType.Wwanpp2).ToList();
                    }
                    // gat captured devices names list
                    string nonCapturedNicsNames = null;
                    string capturedNicsNames = null;
                    string supportedNonCapturedNicsNames = null;
                    foreach (NetworkInterface nic in nonCapturedNics)
                        nonCapturedNicsNames += nic.Name + "\n";
                    foreach (NetworkInterface nic in capturedNics)
                        capturedNicsNames += nic.Name + "\n";
                    foreach (NetworkInterface nic in supportedNonCapturedNics)
                        supportedNonCapturedNicsNames += nic.Name + "\n";
                    if (verbose)
                        splash.Stop();

                    // if there are NICs still not captured by WinPcap
                    if (nonCapturedNics.Count > 0)
                    {
                        // if some non-captured NICs are still supported by WinPcap
                        if (supportedNonCapturedNics.Count > 0)
                        {
                            if (isCapturingOptional)
                            {
                                DialogResult result = MessageBox.Show("The following interfaces are not captured by WinPcap:\n\n" +
                                nonCapturedNicsNames + "\n\n" +
                                "Only the following interfaces are captured by WinPcap:\n\n" +
                                capturedNicsNames + "\n\n" +
                                "Do you want to continue?", "WinPcap Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result == DialogResult.Yes)
                                    return true;
                                else
                                    return false;
                            }
                            else
                            {
                                MessageBox.Show("The following interfaces are not captured by WinPcap and can not continue:\n\n" +
                                nonCapturedNicsNames + "\n\n" +
                                "Only the following interfaces are captured by WinPcap:\n\n" +
                                capturedNicsNames, "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else
                        {
                            if (isCapturingOptional)
                            {
                                DialogResult result = MessageBox.Show("The following interfaces are not captured by WinPcap because WinPcap does not support PPP related interfaces:\n\n" +
                                nonCapturedNicsNames + "\n\n" +
                                "Only the following interfaces are captured by WinPcap:\n\n" +
                                capturedNicsNames + "\n\n" +
                                "Do you want to continue?", "WinPcap Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result == DialogResult.Yes)
                                    return true;
                                else
                                    return false;
                            }
                            else
                            {
                                MessageBox.Show("The following interfaces are not captured by WinPcap because WinPcap does not support PPP related interfaces, and can not continue:\n\n" +
                                nonCapturedNicsNames + "\n\n" +
                                "Only the following interfaces are captured by WinPcap:\n\n" +
                                capturedNicsNames, "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                    }
                    // all NICs are captured
                    return true;
                }
                // should never be reached
                MessageBox.Show("\"NetGroup Packet Filter\" service is not running.", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool RestartNpfService()
        {
            using (ServiceController service = new ServiceController("NPF"))
            {
                try { 
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }
                catch { }
                
                try { 
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
                catch { }
                if (service.Status == ServiceControllerStatus.StopPending ||
                    service.Status == ServiceControllerStatus.PausePending)
                {
                    MessageBox.Show("Could not restart \"NetGroup Packet Filter\" service because it is used by another program.", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
                if (service.Status != ServiceControllerStatus.Running)
                {
                    MessageBox.Show("Failed to restart \"NetGroup Packet Filter\" service.", "WinPcap Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }
    }
}
