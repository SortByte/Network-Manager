using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WinLib.WinAPI
{
    public static class Ntdll
    {
        [DllImportAttribute("ntdll.dll")]
        public static extern int RtlComputeCrc32(int accumCRC32, byte[] buffer, uint buflen);
    }
}
