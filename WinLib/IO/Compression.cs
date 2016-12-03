using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Shell32;

namespace WinLib.IO
{
    public static class Compression
    {
        /// <summary>
        /// All paths must be absolute
        /// </summary>
        /// <param name="sZip"></param>
        /// <param name="dDir"></param>
        /// <returns></returns>
        public static bool UnZip(string sZip, string dDir)
        {
            sZip = Path.GetFullPath(sZip);
            dDir = Path.GetFullPath(dDir);
            if (!File.Exists(sZip))
                return false;
            if (!Directory.Exists(dDir))
                Directory.CreateDirectory(dDir);
            //platform independent compilation
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            var shell = Activator.CreateInstance(shellAppType);
            Folder source = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new[] { sZip });
            Folder destination = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new[] { dDir });
            //Shell shell = new Shell();
            //Folder destinationFolder = shell.NameSpace(destination);
            //Folder sourceZip = shell.NameSpace(source);
            foreach (var item in source.Items())
            {
                destination.CopyHere(item, 16); //4 | 16 | 512 | 1024);
            }
            SizeComparison rSize = CheckZip(source, destination);
            return (rSize.sSize == rSize.dSize);
        }

        /// <summary>
        /// All paths must be absolute
        /// </summary>
        /// <param name="sDir"></param>
        /// <param name="dZip"></param>
        /// <returns></returns>
        public static bool Zip(string sDir, string dZip)
        {
            if (!Directory.Exists(sDir))
                return false;
            FileStream file = File.Create(dZip);
            file.Write(new byte[] { 80, 75, 5, 6 }, 0, 4);
            file.Write(new byte[18], 0, 18);
            file.Close();
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            var shell = Activator.CreateInstance(shellAppType);
            Folder source = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new[] { sDir });
            Folder destination = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new[] { dZip });
            destination.CopyHere(source.Items(), 16);
            ulong lastSize = 0;
            int timeOut = 0;
            SizeComparison rSize = new SizeComparison();
            while ((rSize.sSize != rSize.dSize || rSize.sSize == 0 || rSize.dSize == 0) && timeOut < 10)
            {
                rSize = CheckZip(source, destination);
                if (lastSize != rSize.dSize && rSize.dSize != 0)
                {
                    lastSize = rSize.dSize;
                    timeOut = 0;
                }
                Thread.Sleep(1000);
            }
            return (rSize.sSize == rSize.dSize);
        }

        static SizeComparison CheckZip(Folder zip, Folder folder)
        {
            SizeComparison size = new SizeComparison();
            foreach (FolderItem sItem in zip.Items())
            {
                FolderItem dItem = null;
                foreach (FolderItem item in folder.Items())
                    if (item.Name == sItem.Name)
                    {
                        dItem = item;
                        break;
                    }
                if (dItem == null)
                    continue;
                if (sItem.IsFolder)
                {
                    if (!dItem.IsFolder)
                        continue;
                    Folder sFolder = sItem.GetFolder;
                    Folder dFolder = dItem.GetFolder;
                    SizeComparison fSize = CheckZip(sFolder, dFolder);
                    size.sSize += fSize.sSize;
                    size.dSize += fSize.dSize;
                }
                else
                {
                    if (dItem.IsFolder)
                        continue;
                    size.sSize += (UInt64)sItem.Size;
                    size.dSize += (UInt64)dItem.Size;
                }
            }
            return size;
        }

        class SizeComparison
        {
            public UInt64 sSize = 0;
            public UInt64 dSize = 0;
        }
    }
}
