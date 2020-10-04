﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinLib.WinAPI;
using static WinLib.WinAPI.Kernel32;

namespace Network_Manager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static SemaphoreSlim requestRefresh = new SemaphoreSlim(1);
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //AppDomain.CurrentDomain.FirstChanceException += (s, e) => { Global.WriteLog(e.Exception.ToString()); };
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            //Advapi32.SetDebugPrivilege();
            // Check if Network Manager is already running
            Process[] processes = Process.GetProcesses();
            List<Process> processDuplicates = new List<Process>();
            foreach (Process process in processes)
            {
                try
                {
                    if ((process.ProcessName == "Network_Manager_x86" ||
                        process.ProcessName == "Network_Manager_x64") &&
                        process.Id != Process.GetCurrentProcess().Id)
                        processDuplicates.Add(process);
                }
                catch { }
            }

            if (processDuplicates.Count > 0)
            {
                DialogResult result = MessageBox.Show("Network Manager is already running. Do you want to restart the application ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    foreach (Process process in processDuplicates)
                    {
                        try { process.Kill(); }
                        catch { }
                    }
                else
                    Environment.Exit(0);
            }
            if (!IsUserAdmin())
                MessageBox.Show("The current logged in Windows user is not an administrator. This program can only be run as an administrator.\nThe program won't work properly.", "User Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (Environment.OSVersion.Version.CompareTo(new Version("5.1.2600.196608")) < 0)
                Global.TrayIcon.ShowBalloonTip(5000, "Unsupported OS", "This program is incompatible with versions of Windows older than Windows XP Service Pack 3.\nProgram might not work properly.", ToolTipIcon.Warning);
            if (File.Exists("Downloads\\Network_Manager_x86.exe"))
            {
                string param = "/v:on /c " +
                            "taskkill /F /IM Network_Manager_x86.exe & " +
                            "taskkill /F /IM Network_Manager_x64.exe & " +
                            "for /l %i in (0,1,20) do ( " +
                            "copy /y Downloads\\Network_Manager_x86.exe Network_Manager_x86.exe & " +
                            "if [!errorlevel!]==[0] ( " +
                            "del /f /q Downloads\\Network_Manager_x86.exe & " +
                            "start Launcher.exe & exit ) " +
                            "else ( ping -n 2 127.0.0.1>nul ) )";
                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = "cmd.exe";
                si.Arguments = param;
                si.CreateNoWindow = true;
                si.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(si);
                Environment.Exit(0);
            }
            if (File.Exists("Downloads\\Network_Manager_x64.exe"))
            {
                string param = "/v:on /c " +
                            "taskkill /F /IM Network_Manager_x86.exe & " +
                            "taskkill /F /IM Network_Manager_x64.exe & " +
                            "for /l %i in (0,1,20) do ( " +
                            "copy /y Downloads\\Network_Manager_x64.exe Network_Manager_x64.exe & " +
                            "if [!errorlevel!]==[0] ( " +
                            "del /f /q Downloads\\Network_Manager_x64.exe & " +
                            "start Launcher.exe & exit ) " +
                            "else ( ping -n 2 127.0.0.1>nul ) )";
                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = "cmd.exe";
                si.Arguments = param;
                si.CreateNoWindow = true;
                si.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(si);
                Environment.Exit(0);
            }
            Global.ShowTrayIcon();
            if (File.Exists("Network_Manager.log"))
                File.SetAttributes("Network_Manager.log", (new FileInfo("Network_Manager.log")).Attributes & ~FileAttributes.ReadOnly);
            try { File.Delete("Network_Manager.log"); }
            catch { }
            Trace.Listeners.Add(new TextWriterTraceListener("Network_Manager.log"));
            Trace.AutoFlush = true;
            Trace.Indent();
            Trace.WriteLine(Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductName + " has started");
            Trace.Unindent();
            Refresh();
            List<Config.SavedRouteItem> savedRoutes = new List<Config.SavedRouteItem>();
            foreach (Config.SavedRouteNode node in Global.Config.SavedRoutes.Nodes)
                if (node is Config.SavedRouteGroup)
                    savedRoutes.AddRange(Global.Config.SavedRoutes.GetRoutes(node, true));
            int savedRoutesAutoLoadFailures = 0;
            foreach (Config.SavedRouteItem route in savedRoutes)
                try
                {
                    route.Load();
                }
                catch (Exception ex)
                {
                    savedRoutesAutoLoadFailures++;
                    Global.WriteLog($"Failed to load saved route {route.Name} at startup: {ex.Message}");
                }
            if (savedRoutesAutoLoadFailures > 0)
                Global.TrayIcon.ShowBalloonTip(5000, "Saved routes auto load", $"{savedRoutesAutoLoadFailures} saved route(s) failed to auto load on startup", ToolTipIcon.Error);
            Application.Run();
            //try
            //{
                
            //}
            //catch (Exception e)
            //{
            //    System.Windows.Forms.MessageBox.Show(e.Message);
            //}
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            try
            {
                foreach (Config.InterfaceDataUsage interfaceDataUsage in Global.Config.DataUsage)
                {
                    Global.NetworkInterfaces.Where(i => i.Value.Guid == interfaceDataUsage.InterfaceGuid).ToList().ForEach(i =>
                    {
                        interfaceDataUsage.CurrentSessionReceivedBytes = i.Value.IPv4BytesReceived;
                        interfaceDataUsage.CurrentSessionSentBytes = i.Value.IPv4BytesSent;
                    });
                }
                Global.Config.Save();
                Global.TrayIcon.Visible = false;
                Global.WriteLog(Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductName + " has exited");
            }
            catch { }
        }

        // TODO: fix Refresh() deadlock on closing gadget form when Windows is resuming
        /// <summary>
        /// Needs to be executed on the UI thread
        /// </summary>
        public static void Refresh()
        {
            if (!requestRefresh.Wait(0))
                return;
            //closing
            Global.TrayIcon.Icon = Properties.Resources.logo_nm_blue_ico;
            Global.TrayMenu.MenuItems[0].Enabled = false;
            Global.TrayMenu.MenuItems[1].Enabled = false;
            Global.TrayMenu.MenuItems[2].Enabled = false;
            Global.TrayMenu.MenuItems[3].Enabled = false;
            Global.TrayMenu.MenuItems[4].Enabled = false;
            Splash.StartupForm splashForm = new Splash.StartupForm();
            splashForm.UpdateStatus("Closing forms ...");
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
                if (!Application.OpenForms[i].InvokeRequired &&
                    !Application.OpenForms[i].Disposing &&
                    !Application.OpenForms[i].IsDisposed)
                {
                    string name = Application.OpenForms[i].Text;
                    splashForm.UpdateStatus("Closing " + name + " form ...");
                    Application.OpenForms[i].Close();
                }
            splashForm.UpdateStatus("Stopping jobs ...");
            Jobs.TrafficMonitor.Stop();
            Jobs.CheckUpdates.Stop();

            //starting
            splashForm.UpdateStatus("Loading configuration file ...");
            Global.Load();
            Global.NetworkInterfaces = WinLib.Network.NetworkInterface.GetAll(splashForm.UpdateStatus);
            splashForm.Stop();
            new Gadget.GadgetForm();
            new Thread(new ThreadStart(Jobs.TrafficMonitor.Start)).Start();
            new Thread(new ThreadStart(Jobs.CheckUpdates.Start)).Start();
            Global.TrayIcon.Icon = Properties.Resources.logo_nm_green_ico;
            Global.TrayMenu.MenuItems[0].Enabled = true;
            Global.TrayMenu.MenuItems[1].Enabled = true;
            Global.TrayMenu.MenuItems[2].Enabled = true;
            Global.TrayMenu.MenuItems[3].Enabled = true;
            Global.TrayMenu.MenuItems[4].Enabled = true;
            requestRefresh.Release();
        }

        static bool IsUserAdmin()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
