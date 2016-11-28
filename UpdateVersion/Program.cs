using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using WinLib.IO;
using WinLib.WinAPI;
using Network_Manager;

namespace UpdateVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                string path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                Directory.CreateDirectory(path + @"\Network Manager");
                if (!File.Exists(path + @"\Launcher.exe"))
                {
                    Console.WriteLine(path + @"\Launcher.exe is missing.");
                    Console.ReadKey();
                    return;
                }
                if (!File.Exists(path + @"\Network_Manager_x86.exe"))
                {
                    Console.WriteLine(path + @"\Network_Manager_x86.exe is missing.");
                    Console.ReadKey();
                    return;
                }
                if (!File.Exists(path + @"\Network_Manager_x64.exe"))
                {
                    Console.WriteLine(path + @"\Network_Manager_x64.exe is missing.");
                    Console.ReadKey();
                    return;
                }
                if (!File.Exists(path + @"\License.txt"))
                {
                    Console.WriteLine(path + @"\License.txt is missing.");
                    Console.ReadKey();
                    return;
                }
                File.Copy(path + @"\Launcher.exe", path + @"\Network Manager\Launcher.exe", true);
                File.Copy(path + @"\Network_Manager_x86.exe", path + @"\Network Manager\Network_Manager_x86.exe", true);
                File.Copy(path + @"\Network_Manager_x64.exe", path + @"\Network Manager\Network_Manager_x64.exe", true);
                File.Copy(path + @"\License.txt", path + @"\Network Manager\License.txt", true);
                Compression.Zip(path + "\\Network Manager", path + "\\Network_Manager.zip");
                Directory.Delete(path + "\\Network Manager", true);
                int crc32 = 0;
                byte[] buffer = new byte[512];
                long size = 0;
                FileStream file = new FileStream(path + "\\Network_Manager.zip", FileMode.Open);
                BinaryReader reader = new BinaryReader(file);
                while ((size = reader.Read(buffer, 0, 512)) > 0)
                    crc32 = Ntdll.RtlComputeCrc32(crc32, buffer, (uint)size);
                reader.Close();
                file.Close();
                FileInfo fileInfo = new FileInfo(path + "\\Network_Manager.zip");
                VersionInfo versionInfo = new VersionInfo();
                versionInfo.MainModule.LatestVersion = FileVersionInfo.GetVersionInfo(path + @"\Network_Manager.exe").FileVersion.ToString();
                versionInfo.MainModule.UpdatesEnabled = true;
                VersionInfo.UrlInfo urlInfo = new VersionInfo.UrlInfo();
                urlInfo.Url = "http://www.sortbyte.com/software-programs/networking/network-manager/download/Network_Manager.zip";
                urlInfo.FileName = "Network_Manager.zip";
                urlInfo.Crc32 = crc32.ToString("X8");
                urlInfo.Size = fileInfo.Length;
                versionInfo.MainModule.Urls.Add(urlInfo);
                XmlSerializer writer = new XmlSerializer(typeof(VersionInfo));
                StreamWriter xml = new StreamWriter(path + "\\version.xml");
                writer.Serialize(xml, versionInfo);
                xml.Close();
                return;
            }
            if (Regex.IsMatch(args[0], @"AssemblyInfo\.cs$", RegexOptions.IgnoreCase))
            {
                string path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                string version = FileVersionInfo.GetVersionInfo(path + @"\Launcher.exe").ProductVersion;
                string[] lines = File.ReadAllLines(args[0]);
                for (int i = 0; i < lines.Length; i++ )
                {
                    lines[i] = Regex.Replace(lines[i], @"\[assembly: AssemblyVersion\(""[\d]+\.[\d]+\.[\d]+\.[\d]+""\)\]",
                        "[assembly: AssemblyVersion(\"" + version + "\")]");
                    lines[i] = Regex.Replace(lines[i], @"\[assembly: AssemblyFileVersion\(""[\d]+\.[\d]+\.[\d]+\.[\d]+""\)\]",
                        "[assembly: AssemblyFileVersion(\"" + version + "\")]");
                }
                File.WriteAllLines(args[0], lines, Encoding.UTF8);
            }
            else if (Regex.IsMatch(args[0], @"Launcher\.rc$", RegexOptions.IgnoreCase))
            {
                string version = DateTime.Now.Year + "." +
                DateTime.Now.Month + "." +
                DateTime.Now.Day + "." +
                (DateTime.Now.Hour * 60 + DateTime.Now.Minute);
                string[] lines = File.ReadAllLines(args[0]);
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = Regex.Replace(lines[i], @"FILEVERSION [\d]+,[\d]+,[\d]+,[\d]+",
                        "FILEVERSION " + Regex.Replace(version, @"\.", ","));
                    lines[i] = Regex.Replace(lines[i], @"PRODUCTVERSION [\d]+,[\d]+,[\d]+,[\d]+",
                        "PRODUCTVERSION " + Regex.Replace(version, @"\.", ","));
                    lines[i] = Regex.Replace(lines[i], @"VALUE ""ProductVersion"", ""[\d]+\.[\d]+\.[\d]+\.[\d]+""",
                        "VALUE \"ProductVersion\", \"" + version + "\"");
                    lines[i] = Regex.Replace(lines[i], @"VALUE ""FileVersion"", ""[\d]+\.[\d]+\.[\d]+\.[\d]+""",
                        "VALUE \"FileVersion\", \"" + version + "\"");
                }
                File.WriteAllLines(args[0], lines, Encoding.Unicode);
            }
        }
    }
}
