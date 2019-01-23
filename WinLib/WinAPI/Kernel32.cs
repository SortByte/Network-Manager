using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace WinLib.WinAPI
{
    public static class Kernel32
    {
        public static List<string> DeviceNames = new List<string>();
        public static List<string> DriveNames = new List<string>();
        public delegate bool ConsoleCtrlHandlerDelegate(int sig);

        [DllImportAttribute("Kernel32.dll")]
        public static extern uint GetLastError();
        [DllImportAttribute("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);
        [DllImportAttribute("Kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImportAttribute("Kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetLogicalDriveStrings(uint nBufferLength, [Out] char[] Buffer);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint QueryDosDevice(string DeviceName, [Out] char[] TargetPath, uint ucchMax);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string ModuleName);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, ref uint BytesReturned, IntPtr lpOverlapped);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(string FileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);


        public static string GetLastErrorMessage(uint errorCode)
        {
            string message = "(" + errorCode + ") ";
            IntPtr pBuffer = IntPtr.Zero;
            uint size = FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                IntPtr.Zero,
                errorCode,
                0,
                ref pBuffer,
                0,
                IntPtr.Zero);
            message += Marshal.PtrToStringAuto(pBuffer);
            LocalFree(pBuffer);
            return message;
        }

        public static void GetDeviceNameMap()
        {
            DeviceNames.Clear();
            DriveNames.Clear();
            uint nBufferLength = GetLogicalDriveStrings(0, null);
            char[] buffer = new char[nBufferLength];
            GetLogicalDriveStrings(nBufferLength, buffer);
            int offset = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    if (i == offset)
                        continue;
                    string drive = new string(buffer, offset, i - offset);
                    DriveNames.Add(Regex.Replace(drive, @"\\", ""));
                    offset = i + 1;
                }
            }
            buffer = new char[1024];
            foreach (string drive in DriveNames)
            {
                uint length = QueryDosDevice(drive, buffer, 1024);
                for (int i = 0; i < length; i++)
                    if (buffer[i] == 0)
                    {
                        DeviceNames.Add(new string(buffer, 0, i));
                        break;
                    }
            }
        }

        public static string DevicePathToDrivePath(string devicePath)
        {
            for (int i = 0; i < DeviceNames.Count; i++)
                devicePath = Regex.Replace(@devicePath, @Regex.Replace(@DeviceNames[i], @"\\", @"\\"), @DriveNames[i]);
            return devicePath;
        }

        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public static void IniWriteValue(string Section, string Key, string Value, string iniPath)
        {
            iniPath = Path.GetFullPath(iniPath);
            WritePrivateProfileString(Section, Key, Value, iniPath);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public static string IniReadValue(string Section, string Key, string iniPath)
        {
            iniPath = Path.GetFullPath(iniPath);
            StringBuilder temp = new StringBuilder(65535);
            int i = GetPrivateProfileString(Section, Key, "", temp, 65535, iniPath);
            return temp.ToString();
        }

        public const uint FILE_READ_DATA = 1;
        public const uint FILE_WRITE_DATA = 2;
        public const uint FILE_SHARE_READ = 1;
        public const uint FILE_SHARE_WRITE = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_ATTRIBUTE_SYSTEM = 4;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;

        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public enum HRESULT : uint
        {
            [Description("Operation successful")]
            S_OK = 0x00000000,
            [Description("Operation aborted")]
            E_ABORT = 0x80004004,
            [Description("General access denied error")]
            E_ACCESSDENIED = 0x80070005,
            [Description("Unspecified failure")]
            E_FAIL = 0x80004005,
            [Description("Handle that is not valid")]
            E_HANDLE = 0x80070006,
            [Description("One or more arguments are not valid")]
            E_INVALIDARG = 0x80070057,
            [Description("No such interface supported")]
            E_NOINTERFACE = 0x80004002,
            [Description("Not implemented")]
            E_NOTIMPL = 0x80004001,
            [Description("Failed to allocate necessary memory")]
            E_OUTOFMEMORY = 0x8007000E,
            [Description("Pointer that is not valid")]
            E_POINTER = 0x80004003,
            [Description("Unexpected failure")]
            E_UNEXPECTED = 0x8000FFFF
        }

}
}
