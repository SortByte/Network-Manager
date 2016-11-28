using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Net;
using System.Xml;
using WinLib.Network.Http;
using WinLib.IO;


namespace Network_Manager
{
    public class VersionInfo
    {
        [field: NonSerialized]
        public event EventHandler<UpdateEventArgs> UpdateAvailableEvent;

        public UpdateInfo MainModule = new UpdateInfo();
        // add modules here
        private bool showMsg = false;

        public class UpdateInfo
        {
            [XmlIgnore]
            public Version CurrentVersion;
            public string LatestVersion = "0.0.0.0";
            public bool UpdatesEnabled = false;
            public List<UrlInfo> Urls = new List<UrlInfo>();
        }

        public class UrlInfo
        {
            public string Url = "http://";
            public string FileName = "filename.ext";
            public string Crc32 = "ffffffff";
            /// <summary>
            /// Size in bytes
            /// </summary>
            public long Size = 0;
        }

        /// <summary>
        /// Parameterless constructor for XML serialization
        /// </summary>
        public VersionInfo()
        {

        }

        /// <summary>
        /// Updates VersionInfo fields by checking online
        /// </summary>
        /// <param name="showMsg">Indicates wheter the check up is done verbose or quiet</param>
        public void CheckForUpdates(bool showMsg)
        {
            this.showMsg = showMsg;
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", Headers.DefaultUserAgent);
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.OpenReadAsync(new Uri("http://www.sortbyte.com/software-programs/networking/network-manager/download/version.xml"));
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                Stream data = e.Result;
                StreamReader httpReader = new StreamReader(data);
                string xml = httpReader.ReadToEnd();
                data.Close();
                httpReader.Close();
                XmlSerializer xmlReader = new XmlSerializer(typeof(VersionInfo));
                StringReader stringReader = new StringReader(xml);
                VersionInfo versionInfo = (VersionInfo)xmlReader.Deserialize(stringReader);
                MainModule = (UpdateInfo)Xml.CheckIfNull(typeof(UpdateInfo), versionInfo.MainModule, MainModule);
                stringReader.Close();
            }
            catch (Exception ex)
            {
                if (showMsg)
                    System.Windows.Forms.MessageBox.Show("Unable to connect to server:\n\n" + ex.Message, "Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ((WebClient)sender).Dispose();
            }
            // Check modules for new versions
            List<Modules> newUpdates = new List<Modules>();
            // Check MainModule version
            if (new Version(MainModule.LatestVersion).CompareTo(MainModule.CurrentVersion) > 0 &&
                MainModule.Urls.Count > 0)
                newUpdates.Add(Modules.MainModule);
            // If any module has an update
            if (newUpdates.Count > 0)
            {
                if (showMsg)
                {
                    UpdateForm form = new UpdateForm(this);
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.Yes)
                        Update();
                }
                if (UpdateAvailableEvent != null)
                    UpdateAvailableEvent(this, new UpdateEventArgs(newUpdates));
            }
            else
            {
                if (showMsg)
                    MessageBox.Show("No update available. You already have the latest version (" + MainModule.CurrentVersion.ToString() + ").", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void Update()
        {
            int crc32 = Convert.ToInt32(MainModule.Urls[0].Crc32, 16);
            new Download(MainModule.Urls[0].Url, @"Downloads\" + MainModule.Urls[0].FileName, crc32, MainModule.Urls[0].Size, UpdateMainModule_Callback);
        }

        private void UpdateMainModule_Callback(bool success, string fileName)
        {
            if (success)
            {
                if (WinLib.IO.Compression.UnZip(Path.GetFullPath(fileName), Path.GetDirectoryName(Path.GetFullPath(fileName))))
                {
                    File.Delete(fileName);
                    if (File.Exists("License.txt"))
                        File.Delete("License.txt");
                    File.Move(@"Downloads\License.txt", "License.txt");
                    System.Diagnostics.Process.Start("Launcher.exe");
                    Global.Exit();
                }
                else
                    MessageBox.Show("Extraction was corrupted or interrupted !", "Extraction", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class UpdateEventArgs : EventArgs
        {
            public List<Modules> NewUpdates;

            public UpdateEventArgs(List<Modules> newUpdates)
            {
                NewUpdates = newUpdates;
            }
        }

        public enum Modules
        {
            MainModule
        }

        // debug only
        public void Save()
        {
            XmlSerializer writer = new XmlSerializer(typeof(VersionInfo));
            StreamWriter file = new StreamWriter("updates.xml");
            writer.Serialize(file, this);
            file.Close();
        }
    }
}
