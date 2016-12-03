using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace WinLib.WinAPI
{
    public static class Newdev
    {
        [DllImport("Newdev.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UpdateDriverForPlugAndPlayDevices(IntPtr hwndParent, string HardwareId, string FullInfPath, uint InstallFlags, IntPtr pRebootRequired);
        [DllImport("Newdev.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UpdateDriverForPlugAndPlayDevices(IntPtr hwndParent, string HardwareId, string FullInfPath, uint InstallFlags, ref bool RebootRequired);

        public const uint INSTALLFLAG_FORCE = 1;
        public const uint INSTALLFLAG_READONLY = 2;
        public const uint INSTALLFLAG_NONINTERACTIVE = 3;
    }
}
