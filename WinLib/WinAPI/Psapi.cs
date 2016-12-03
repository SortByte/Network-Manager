using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WinLib.WinAPI
{
    public static class Psapi
    {
        [DllImport("Psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] char[] ImageFileName, uint nSize);

        public static string GetProcessFileName(uint pid)
        {
            IntPtr hProcess;
            if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                hProcess = Kernel32.OpenProcess(Advapi32.PROCESS_QUERY_INFORMATION, false, pid);
            else
                hProcess = Kernel32.OpenProcess(Advapi32.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            char[] buffer = new char[2000];
            uint length = GetProcessImageFileName(hProcess, buffer, 2000);
            Kernel32.CloseHandle(hProcess);
            return Kernel32.DevicePathToDrivePath(new string (buffer, 0, (int)length));
        }
    }
}
