using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static WinLib.WinAPI.Rpcdce;

namespace WinLib.WinAPI
{
    public static class Setupapi
    {
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetINFClass(string InfName, ref GUID ClassGuid, [Out] char[] ClassName, uint ClassNameSize, ref uint RequiredSize);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevsEx(ref GUID ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags, IntPtr DeviceInfoSet, string MachineName, IntPtr Reserved);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevsEx(IntPtr ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags, IntPtr DeviceInfoSet, IntPtr MachineName, IntPtr Reserved);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiCreateDeviceInfo(IntPtr DeviceInfoSet, string ClassName, ref GUID ClassGuid, string DeviceDescription, IntPtr hwndParent, uint CreationFlags, ref SP_DEVINFO_DATA DeviceInfoData);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiCreateDeviceInfoList(ref GUID ClassGuid, IntPtr hwndParent);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInfoListDetail(IntPtr DeviceInfoSet, ref SP_DEVINFO_LIST_DETAIL_DATA DeviceInfoSetDetailData);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, uint Property, IntPtr PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, uint Property, IntPtr PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, ref uint RequiredSize);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiSetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, uint Property, byte[] PropertyBuffer, uint PropertyBufferSize);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiCallClassInstaller(DI_FUNCTION InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_CLASSINSTALL_HEADER ClassInstallParams, uint ClassInstallParamsSize);
        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_REMOVEDEVICE_PARAMS ClassInstallParams, uint ClassInstallParamsSize);

        /// <summary>
        /// Installs a driver using its INF file
        /// </summary>
        /// <param name="filePath">Relative or absolute INF file path</param>
        /// <param name="hardwareID">ComponentID</param>
        /// <returns></returns>
        public static bool InstallInfDriver(string filePath, string hardwareID, string description = "")
        {
            // TODO: Win10 got wrong bitness onetime (???)
            if (IntPtr.Size == 4 && Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show("This process is 32-bit but the OS is 64-bit. Only 64-bit processes can install 64-bit direvers.",
                    "Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2, MessageBoxOptions.ServiceNotification);
                return false;
            }
            filePath = Path.GetFullPath(filePath);
            GUID classGuid = new GUID();
            char[] className = new char[MAX_CLASS_NAME_LEN];
            uint requiredSize = 0;
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);
            // Use the INF File to extract the Class GUID
            if (!SetupDiGetINFClass(filePath, ref classGuid, className, MAX_CLASS_NAME_LEN, ref requiredSize))
                return false;
            // Create the container for the to-be-created Device Information Element
            IntPtr deviceInfoSet = SetupDiCreateDeviceInfoList(ref classGuid, IntPtr.Zero);
            if (deviceInfoSet == Kernel32.INVALID_HANDLE_VALUE)
                return false;
            // Now create the element. Use the Class GUID and Name from the INF file
            if (!SetupDiCreateDeviceInfo(deviceInfoSet, new string(className), ref classGuid, description, IntPtr.Zero, DICD_GENERATE_ID, ref deviceInfoData))
                return false;
            // Add the HardwareID to the Device's HardwareID property
            if (!SetupDiSetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID, Encoding.Unicode.GetBytes(hardwareID), (uint)Encoding.Unicode.GetByteCount(hardwareID)))
                return false;
            // Transform the registry element into an actual devnode in the PnP HW tree
            if (!SetupDiCallClassInstaller(DI_FUNCTION.DIF_REGISTERDEVICE, deviceInfoSet, ref deviceInfoData))
                return false;
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
            // Update the driver for the device we just created
            if (!Newdev.UpdateDriverForPlugAndPlayDevices(IntPtr.Zero, hardwareID, filePath, Newdev.INSTALLFLAG_FORCE, IntPtr.Zero))
                return false;
            return true;
        }

        /// <summary>
        /// Removes all devices with a matching hardware ID
        /// </summary>
        /// <param name="hardwareID">ComponenetID</param>
        /// <returns></returns>
        public static bool UninstallDevice(string hardwareID)
        {
            if (IntPtr.Size == 4 && Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show("This process is 32-bit but the OS is 64-bit. Only 64-bit processes can uninstall 64-bit direvers.",
                    "Driver Setup", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2, MessageBoxOptions.ServiceNotification);
                return false;
            }
            IntPtr deviceInfoSet = SetupDiGetClassDevsEx(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, DIGCF_ALLCLASSES, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (deviceInfoSet == Kernel32.INVALID_HANDLE_VALUE)
                return false;
            //SP_DEVINFO_LIST_DETAIL_DATA deviceInfoListDetailData = new SP_DEVINFO_LIST_DETAIL_DATA();
            //if (Environment.Is64BitProcess)
            //    deviceInfoListDetailData.cbSize = 560;
            //else
            //    deviceInfoListDetailData.cbSize = 550;
            //if (!SetupDiGetDeviceInfoListDetail(deviceInfoSet, ref deviceInfoListDetailData))
            //    return false;
            uint index = 0;
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);
            while(SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData))
            {
                index++;
                uint requiredSize = 0;
                SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID, IntPtr.Zero, IntPtr.Zero, 0, ref requiredSize);
                if (requiredSize == 0)
                    continue;
                IntPtr pPropertyBuffer = Marshal.AllocHGlobal((int)requiredSize);
                if (!SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID, IntPtr.Zero, pPropertyBuffer, requiredSize, IntPtr.Zero))
                    continue;
                string currHardwareID = Marshal.PtrToStringAuto(pPropertyBuffer);
                Marshal.FreeHGlobal(pPropertyBuffer);
                if (currHardwareID == hardwareID)
                {
                    SP_REMOVEDEVICE_PARAMS classInstallParams = new SP_REMOVEDEVICE_PARAMS();
                    classInstallParams.ClassInstallHeader.cbSize = (uint)Marshal.SizeOf(classInstallParams.ClassInstallHeader);
                    classInstallParams.ClassInstallHeader.InstallFunction = DI_FUNCTION.DIF_REMOVE;
                    classInstallParams.Scope = DI_REMOVEDEVICE_GLOBAL;
                    if (!SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref classInstallParams, (uint)Marshal.SizeOf(classInstallParams)))
                        return false;
                    if (!SetupDiCallClassInstaller(DI_FUNCTION.DIF_REMOVE, deviceInfoSet, ref deviceInfoData))
                        return false;
                }
            }
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public GUID ClassGuid;
            public uint DevInst;
            IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SP_DEVINFO_LIST_DETAIL_DATA
        {
            /// <summary>
            /// 550 on x86 and 560 on x64
            /// </summary>
            public uint cbSize;
            public GUID ClassGuid;
            public IntPtr RemoteMachineHandle; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)SP_MAX_MACHINENAME_LENGTH)]
            public string RemoteMachineName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public uint cbSize;
            public DI_FUNCTION InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_REMOVEDEVICE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public uint Scope;
            public uint HwProfile;
        }

        public enum DI_FUNCTION
        {
            DIF_REMOVE = 5,
            DIF_REGISTERDEVICE = 25
        }

        public const uint MAX_PATH = 260;
        public const uint MAX_CLASS_NAME_LEN = 128;
        public const uint DI_REMOVEDEVICE_GLOBAL = 1;
        public const uint DICD_GENERATE_ID = 1;
        public const uint SPDRP_HARDWAREID = 1;
        public const uint DIGCF_ALLCLASSES = 4;
        public const uint SP_MAX_MACHINENAME_LENGTH = MAX_PATH + 3;
    }
}
