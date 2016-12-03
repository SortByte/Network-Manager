using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WinLib.Network.Http
{
    public class Headers
    {
        /// <summary>
        /// Default User-Agent heeader containing OS version, CLR version and platform bitness
        /// </summary>
        public static string DefaultUserAgent
        {
            get
            {
                return "SB " + Process.GetCurrentProcess().Modules[0].FileVersionInfo.ProductName + " " + Process.GetCurrentProcess().Modules[0].FileVersionInfo.FileVersion + " (" +
                    Environment.OSVersion.VersionString + (Environment.Is64BitOperatingSystem ? " 64-bit" : " 32-bit") +
                    "; .NET CLR " + Environment.Version + (Environment.Is64BitProcess ? " 64-bit" : " 32-bit") + ")";
            }
        }
    }
}
