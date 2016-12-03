using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using WinLib.WinAPI;
using WinLib.Network;

namespace WinLib.Network
{
    //class Route
    //{
    //    public string Destination;
    //    public string Prefix;
    //    public string Gateway;
    //    public int InterfaceIndex;
    //    public uint Age;
    //    // The route metric offset value for this IP route entry.
    //    // Note the actual route metric used to compute the route preference is the summation of interface metric
    //    public ushort Metric;
    //    public int IPVersion;


    //    public static void AddRoute(string destination, string mask, string gateway, string interfaceIndex, string metric)
    //    {
    //        if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > 0)
    //        {
    //            //if (metric == "0")
    //            //    metric = "1";
    //            //Iphlpapi.AddRouteIPv4(destination, mask, gateway, interfaceIndex, metric);
    //        }
    //        else
    //        {
    //            Process process = CreateProcess();
    //            if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    //                process.StartInfo.Arguments = "add " + destination + " mask " + mask + " " + gateway + " metric " + metric + " if " + interfaceIndex;
    //            else
    //                process.StartInfo.Arguments = "add " + destination + "/" + mask + " " + gateway + " metric " + metric + " if " + interfaceIndex;
    //            process.Start();
    //            process.WaitForExit();
    //            process.Close();
    //        }
    //    }

    //    public static void EditRoute(string destination, string prefix, string gateway, string interfaceIndex, string metric)
    //    {
    //        Process process = CreateProcess();
    //        if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    //            process.StartInfo.Arguments = "change " + destination + " mask " + prefix + " " + gateway + " metric " + metric + " if " + interfaceIndex;
    //        else
    //            process.StartInfo.Arguments = "change " + destination + "/" + prefix + " " + gateway + " metric " + metric + " if " + interfaceIndex;
    //        process.Start();
    //        process.WaitForExit();
    //        process.Close();
    //    }

    //    public static void DeleteRoute(string destination, string prefix, string gateway, string interfaceIndex)
    //    {
    //        Process process = CreateProcess();
    //        if (IPAddress.Parse(destination).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    //            process.StartInfo.Arguments = "delete " + destination + " mask " + prefix + " " + gateway + " if " + interfaceIndex;
    //        else
    //            process.StartInfo.Arguments = "delete " + destination + "/" + prefix + " " + gateway + " if " + interfaceIndex;
    //        process.Start();
    //        process.WaitForExit();
    //        process.Close();
    //    }

    //    private static Process CreateProcess()
    //    {
    //        Process process = new Process();
    //        process.StartInfo.UseShellExecute = false;
    //        process.StartInfo.CreateNoWindow = true;
    //        process.StartInfo.RedirectStandardOutput = true;
    //        process.StartInfo.FileName = "route.exe";
    //        return process;
    //    }
    //}
}
