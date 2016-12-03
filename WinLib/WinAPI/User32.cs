using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinLib.WinAPI
{
    public static class User32
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImportAttribute("user32.dll")]
        public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr LPVOIDlpParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);
        [DllImportAttribute("user32.dll")]
        public static extern IntPtr LoadImage(IntPtr hinst, string szName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);
        [DllImport("user32.dll")]
        internal static extern bool GetUpdateRect(IntPtr hWnd, ref RECT rect, bool bErase);

        [StructLayout(LayoutKind.Sequential)]
        public struct EDITBALLOONTIP
        {
            public uint cbStruct;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszTitle;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszText;
            public int ttiIcon;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width { get { return this.Right - this.Left; } }
            public int Height { get { return this.Bottom - this.Top; } }
        }

        /// <summary>
        /// Needs to be implemeted so higher resolution icons can be extracted, unlike ExtractIconEx which can only extract 32x32.
        /// This can be done with LoadImage, but resource ID is required, so we must enumarate ICON resources first, then map them to a 0 based index
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Icon LoadIconFromExe(string file, int nIconIndex, int size)
        {
            throw new NotImplementedException();
            return null;
        }
    }
}
