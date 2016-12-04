using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WinLib.WinAPI
{
    public static class Rpcdce
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            uint Data1;
            ushort Data2;
            ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            byte[] Data4;

            public GUID(string guid)
            {
                Data1 = 0;
                Data2 = 0;
                Data3 = 0;
                Data4 = new byte[8];

                if (!Regex.IsMatch(guid, @"^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$", RegexOptions.IgnoreCase)) return;
                string[] data = guid.Split('-');
                
                Data1 = Convert.ToUInt32(data[0], 16);
                Data2 = Convert.ToUInt16(data[1], 16);
                Data3 = Convert.ToUInt16(data[2], 16);
                BitConverter.GetBytes(Convert.ToUInt16(data[3], 16)).CopyTo(Data4, 0);
                BitConverter.GetBytes(Convert.ToUInt64(data[4], 16)).Take(6).ToArray().CopyTo(Data4, 2);
            }

            public override string ToString()
            {
                string guid = "";
                guid += String.Format("{0:X8}", Data1);
                guid += "-" + String.Format("{0:X4}", Data2);
                guid += "-" + String.Format("{0:X4}", Data3);
                guid += "-" + String.Format("{0:X4}", BitConverter.ToUInt16(Data4, 0));
                guid += "-" + String.Format("{0:X12}", BitConverter.ToUInt64(Data4.Skip(2).Take(6).Concat(new byte[2]).ToArray(), 0));
                return guid;
            }
        }
    }
}
