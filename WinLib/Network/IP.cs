using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using static WinLib.WinAPI.Iphlpapi;
using System.Runtime.InteropServices;
using static WinLib.WinAPI.Kernel32;

namespace WinLib.Network
{
    public static class IP
    {
        public static bool CheckIfSameNetwork(string sIPA, string sIPB, string sMask)
        {
            IPAddress ipAddress;
            if (sIPA == "" || sIPB == "" || sMask == "" ||
                !IPAddress.TryParse(sIPA, out ipAddress) ||
                !IPAddress.TryParse(sIPB, out ipAddress) ||
                !IPAddress.TryParse(sMask, out ipAddress))
                return false;
            IPAddress ipA = IPAddress.Parse(sIPA);
            IPAddress ipB = IPAddress.Parse(sIPB);
            int mask = 0;
            if (ipA.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bMask = IPAddress.Parse(sMask).GetAddressBytes();
                if (!ValidateIPv4Mask(ref sMask))
                    return false;
                byte[] bIPA = ipA.GetAddressBytes().Zip(bMask, (b1, b2) => (byte)(b1 & b2)).ToArray();
                byte[] bIPB = ipB.GetAddressBytes().Zip(bMask, (b1, b2) => (byte)(b1 & b2)).ToArray();
                return bIPA.SequenceEqual(bIPB);
            }
            else if (ipA.AddressFamily == AddressFamily.InterNetworkV6)
            {
                mask = Int32.Parse(sMask);
                byte[] bMask = new byte[16];
                for (int i = 0; i < mask / 8; i++ )
                    bMask[i] = 0xff;
                //mask = 7
                bMask[mask / 8] = (byte)(0xff << (8 - mask % 8));
                byte[] bIPA = ipA.GetAddressBytes().Zip(bMask, (b1, b2) => (byte)(b1 & b2)).ToArray();
                byte[] bIPB = ipB.GetAddressBytes().Zip(bMask, (b1, b2) => (byte)(b1 & b2)).ToArray();
                return bIPA.SequenceEqual(bIPB);
            }
            return false;
        }

        public static bool ValidateIPv4Mask(ref string sMask)
        {
            uint mask;
            if (Regex.IsMatch(sMask, @"^[\d]+$") &&
                Convert.ToInt32(sMask) >= 0 &&
                Convert.ToInt32(sMask) <= 32)
            {
                mask = 0xffffffff << (32 - Convert.ToInt32(sMask));
                if (Convert.ToInt32(sMask) == 0) mask = 0; // good job Anders Hejlsberg :|
                sMask = new IPAddress(BitConverter.GetBytes(mask).Reverse().ToArray()).ToString();
            }
            IPAddress ipAddress;
            if (sMask == "" || !IPAddress.TryParse(sMask, out ipAddress))
                return false;
            sMask = ipAddress.ToString();
            mask = BitConverter.ToUInt32(IPAddress.Parse(sMask).GetAddressBytes().Reverse().ToArray(), 0);
            uint oldMask;
            for (int i = 0; i < 31; i++ )
            {
                oldMask = mask;
                mask = (mask << 1) & 0xFFFFFFFF;
                if (oldMask < mask)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Converts an IPv4 CIDR prefix number (0 - 32) to dot-decimal notation
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string PrefixToMask(int prefix)
        {
            uint uMask = 0xffffffff << (32 - prefix);
            if (prefix == 0) uMask = 0;
            return new IPAddress(BitConverter.GetBytes(uMask).Reverse().ToArray()).ToString();
        }

        public static bool ValidateIPv4(ref string sIP, ref string sMask)
        {
            IPAddress ipAddress;
            if (sIP == "" || !ValidateIPv4Mask(ref sMask) || !IPAddress.TryParse(sIP, out ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetwork)
                return false;
            sIP = ipAddress.ToString();
            uint ip = BitConverter.ToUInt32(IPAddress.Parse(sIP).GetAddressBytes().Reverse().ToArray(), 0);
            uint mask = BitConverter.ToUInt32(IPAddress.Parse(sMask).GetAddressBytes().Reverse().ToArray(), 0);
            if ((ip & 0xff000000) == 0x7f000000 || // loopback
                ip < 0x01000000 || ip > 0xe0ffffff || // leading zero or multicast
                (ip & ~mask) == ~mask) // broadcast and reserved
                return false;
            return true;
        }

        public static bool ValidateIPv4(ref string sIP, string sMask = "0.0.0.0")
        {
            return ValidateIPv4(ref sIP, ref sMask);
        }

        public static bool ValidateIPv4(string sIP, string sMask = "0.0.0.0")
        {
            return ValidateIPv4(ref sIP, ref sMask);
        }

        // TODO: overload IP validation functions with reason string parameter
        public static bool ValidateIPv4(ref string reason, string sIP, string sMask = "0.0.0.0")
        {
            throw new NotImplementedException();
            //return true;
        }

        public static bool ValidateIPv6(ref string sIP, string sPrefix = "0")
        {
            if (sPrefix == "")
                sPrefix = "0";
            IPAddress ipAddress;
            if (sIP == "" || 
                !IPAddress.TryParse(sIP, out ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetworkV6 ||
                !Regex.IsMatch(sPrefix, @"^[\d]+$") || Convert.ToInt32(sPrefix) > 128)
                return false;
            sIP = ipAddress.ToString();
            return true;
        }

        public static bool ValidateIPv6(string sIP, string sPrefix = "0")
        {
            return ValidateIPv6(ref sIP, sPrefix);
        }

        public static bool ValidateIPv6(string sIP, string sPrefix, out string reason)
        {
            throw new NotImplementedException();
            if (sPrefix == "")
                sPrefix = "0";
            IPAddress ipAddress;
            if (sIP == "" ||
                !IPAddress.TryParse(sIP, out ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetworkV6 ||
                !Regex.IsMatch(sPrefix, @"^[\d]+$") || Convert.ToInt32(sPrefix) > 128)
                return false;
            return true;
        }

        public static bool ValidateIPv6Prefix(ref string sPrefix)
        {
            if (sPrefix == "")
                return false;
            int prefix;
            if (!int.TryParse(sPrefix, out prefix) ||
                prefix < 0 || prefix > 128)
                return false;
            sPrefix = prefix.ToString();
            return true;
        }

        public static bool ValidateIPv6Prefix(string sPrefix)
        {
            return ValidateIPv6Prefix(ref sPrefix);
        }

        public static string GetNetwork(string sIP, string sPrefix)
        {
            byte[] bPrefix;
            if (IPAddress.Parse(sIP).AddressFamily == AddressFamily.InterNetwork)
                bPrefix = IPAddress.Parse(sPrefix).GetAddressBytes().ToArray();
            else
            {
                int prefix = int.Parse(sPrefix);
                bPrefix = new byte[16];
                for (int i = 0; i < prefix / 8; i++)
                    bPrefix[i] = 0xff;
                if (prefix % 8 > 0)
                    bPrefix[prefix / 8 - 1] = (byte)(0xff << (8 - prefix % 8));
            }
            byte[] bNetwork = IPAddress.Parse(sIP).GetAddressBytes().Zip(bPrefix, (b1,b2) => (byte)(b1 & b2)).ToArray();
            return new IPAddress(bNetwork).ToString();
        }

        public static int Ping(IPAddress ip)
        {
            in_addr destinationAddress = new in_addr((ip ?? IPAddress.Parse("0.0.0.0")).ToString());
            IntPtr icmpHandle = IcmpCreateFile();
            byte[] requestData = new byte[] { 0 };
            byte[] replyByte = new byte[1000];
            uint result = IcmpSendEcho(icmpHandle, destinationAddress, requestData, (ushort)requestData.Length, IntPtr.Zero, replyByte, (uint)replyByte.Length, 1000);
            if (result > 0)
            {
                GCHandle handle = GCHandle.Alloc(replyByte, GCHandleType.Pinned);
                ICMP_ECHO_REPLY reply = (ICMP_ECHO_REPLY)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ICMP_ECHO_REPLY));
                return (int)reply.RoundTripTime;
            }
            ERROR error = (ERROR)GetLastError();
            return -1;
        }

        public struct SocketID
        {
            public IPEndPoint LocalEP;
            public IPEndPoint RemoteEP;
            public ProtocolFamily Protocol;
        }

        public enum ProtocolFamily
        {
            TCP = 6,
            UDP = 17
        }
    }
}
