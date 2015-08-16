using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Drawing;
using Lib.Network;

namespace Network_Manager
{
    static class Global
    {
        public static ConcurrentDictionary<string, NetworkInterface> NetworkInterfaces;
        public static ConcurrentQueue<BusyForm> BusyForms = new ConcurrentQueue<BusyForm>();
        public static Config Config = new Config();
        public static bool AutoStartup = false;
        public static NotifyIcon TrayIcon = new NotifyIcon();
        public static ContextMenu TrayMenu = new ContextMenu();
        public static VersionInfo VersionInfo = new VersionInfo();
        public static string InternetInterface = null;

        public static void ShowTrayIcon()
        {
            TrayMenu.MenuItems.Add(new MenuItem("Show", ShowGadget));
            TrayMenu.MenuItems.Add(new MenuItem("Center Gadget", CenterGadget));
            TrayMenu.MenuItems.Add(new MenuItem("Always on top", AlwaysOnTop));
            TrayMenu.MenuItems.Add(new MenuItem("Check for updates", CheckForUpdates));
            TrayMenu.MenuItems.Add(new MenuItem("Settings", new EventHandler((s, e) => { new Gadget.Settings.SettingsForm(); })));
            TrayMenu.MenuItems.Add(new MenuItem("About", new EventHandler((s, e) => { new Gadget.About.AboutForm(); })));
            TrayMenu.MenuItems.Add(new MenuItem("Exit", new EventHandler(Exit)));
            TrayMenu.MenuItems[0].DefaultItem = true;
            TrayMenu.MenuItems[0].Enabled = false;
            TrayMenu.MenuItems[1].Enabled = false;
            TrayMenu.MenuItems[2].Checked = Config.Gadget.AlwaysOnTop;
            TrayIcon.Text = Process.GetCurrentProcess().Modules[0].FileVersionInfo.ProductName;
            TrayIcon.ContextMenu = TrayMenu;
            TrayIcon.Icon = global::Network_Manager.Properties.Resources.logo_nm_blue_ico;
            TrayIcon.Click += new EventHandler(ShowGadget);
            TrayIcon.Visible = true;
        }

        public static void ShowTrayTip(string title, string message, ToolTipIcon icon)
        {
            TrayIcon.ShowBalloonTip(5000, title, message, icon);
        }

        public static void WriteLog(string message, bool isRoot = false)
        {
            Trace.WriteLine(DateTime.Now.ToString() + (isRoot ? " " : "\t") + message);
        }

        public static void Load()
        {
            InternetInterface = null;
            BusyForm busyForm;
            while (!BusyForms.IsEmpty)
                BusyForms.TryDequeue(out busyForm);
            VersionInfo.MainModule.CurrentVersion = new Version(Process.GetCurrentProcess().Modules[0].FileVersionInfo.FileVersion);
            Config.Load();
            string startupKey;
            if (Environment.Is64BitOperatingSystem)
                startupKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run";
            else
                startupKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(startupKey, true);
            string value = (string)key.GetValue("Network Manager", null);
            if (value == null)
                AutoStartup = false;
            else
                AutoStartup = true;
            if (AutoStartup)
                key.SetValue("Network Manager", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Launcher.exe", RegistryValueKind.String);
            key.Close();
        }

        public static void Save()
        {
            Config.Save();
            string startupKey;
            if (Environment.Is64BitOperatingSystem)
                startupKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run";
            else
                startupKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(startupKey, true);
            if (AutoStartup)
                key.SetValue("Network Manager", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Launcher.exe", RegistryValueKind.String);
            else
                key.DeleteValue("Network Manager", false);
            key.Close();
        }

        public static void ShowGadget(object sender = null, EventArgs e = null)
        {
            if (Gadget.GadgetForm.Instance != null)
                Gadget.GadgetForm.Instance.Activate();
        }

        public static void CenterGadget(object sender = null, EventArgs e = null)
        {
            if (Gadget.GadgetForm.Instance != null)
                Gadget.GadgetForm.Instance.Location = new Point(Screen.PrimaryScreen.WorkingArea.Left + Screen.PrimaryScreen.WorkingArea.Width / 2 - 100
                , Screen.PrimaryScreen.WorkingArea.Top + Screen.PrimaryScreen.WorkingArea.Height / 2 - 200);
        }

        public static void AlwaysOnTop(object sender = null, EventArgs e = null)
        {
            Config.Gadget.AlwaysOnTop ^= true;
            Config.Save();
            TrayMenu.MenuItems[2].Checked = Config.Gadget.AlwaysOnTop;
            if (Gadget.GadgetForm.Instance != null)
            {
                Gadget.GadgetForm.Instance.TopMost = Config.Gadget.AlwaysOnTop;
                ((ToolStripMenuItem)((Gadget.GadgetForm)Gadget.GadgetForm.Instance).MainContextMenu.Items[0]).Checked = Config.Gadget.AlwaysOnTop;
            }
        }

        // TODO: make public IP detection, internet interface detection and possibly other network health indicators, a job and periodic
        public static void GetPublicIPs()
        {
            try
            {
                foreach (Lib.Network.NetworkInterface nic in NetworkInterfaces.Values)
                    Task.Factory.StartNew(() => { nic.GetPublicIP(); });
            }
            catch (Exception) { }
        }

        public static async void GetInternetInterface()
        {
            try
            {
                await Task.Factory.StartNew(() => { return Lib.Network.NetworkInterface.GetInternetInterface(); });
            }
            catch (Exception) { }
        }

        public static void CheckForUpdates(object sender = null, EventArgs e = null)
        {
            VersionInfo.CheckForUpdates(true);
        }

        public static void Exit(object sender = null, EventArgs e = null)
        {
            TrayIcon.Visible = false;
            Environment.Exit(0);
        }

        public class BusyForm
        {
            public string Name;
            public TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();


            public BusyForm(string name)
            {
                this.Name = name;
            }
        }
    }
}
