using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WinLib.WinAPI
{
    public static class Shell32
    {
		[DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, [Out] IntPtr[] piLargeVersion, [Out] IntPtr[] piSmallVersion, int amountIcons);

		/// <summary>
		/// Extract icons from binary's resources
        /// [poor quality icons]
		/// </summary>
		/// <param name="file"></param>
		/// <param name="number">0 based index</param>
		/// <param name="largeIcon">large 32x32, small 16x16</param>
		/// <returns></returns>
        public static Icon ExtractIcon(string file, int number, bool largeIcon)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }

        }
    }
}
