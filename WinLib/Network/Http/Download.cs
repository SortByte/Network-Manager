using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using WinLib.WinAPI;
using WinLib.Network.Http;

namespace WinLib.Network.Http
{
    /// <summary>
    /// TODO: Implement pause/resume download (C++ too)
    /// </summary>
    public partial class Download : Form
    {
        HttpWebRequest request;
        HttpWebResponse response;
        string fileName;
        int crc32;
        long length;
        long received = 0;
        long lastReceived = 0;
        Action<bool, string> callback;
        System.Timers.Timer timer = new System.Timers.Timer();
        bool downloadEnabled = true;

        public Download(string url, string fileName, int crc32, long length, Action<bool, string> callback)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            int lastCrc32 = 0;
            if (File.Exists(fileName))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open));
                    int size;
                    byte[] buffer = new byte[512];
                    while ((size = reader.Read(buffer, 0, 512)) > 0)
                        lastCrc32 = Ntdll.RtlComputeCrc32(lastCrc32, buffer, (uint)size);
                    reader.Close();
                    if (lastCrc32 == crc32)
                    {
                        Close();
                        callback(true, fileName);
                        return;
                    }
                }
                catch { }
            }
            InitializeComponent();
            textBox2.Text = Path.GetFullPath(fileName);
            label6.Text = "0B out of " + Unit.AutoScale(length, "B");
            request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.UserAgent = Headers.DefaultUserAgent;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Close();
                System.Windows.Forms.MessageBox.Show("Unable to connect to server:\n\n" + ex.Message, "Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                callback(false, fileName);
                return;
            }
            
            textBox1.Text = response.ResponseUri.AbsoluteUri;
            this.fileName = fileName;
            this.crc32 = crc32;
            this.length = length;
            this.callback = callback;
            Show();
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            Receive();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new Action(() => {
                label4.Text = Unit.AutoScale(received - lastReceived, "B/s");
                lastReceived = received;
            }));
            
        }

        async void Receive()
        {
            BinaryWriter writer = null;
            Stream stream = null;
            byte[] buffer = new byte[512];
            Task<int> task;
            try
            {
                writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                stream = response.GetResponseStream();
                while (downloadEnabled)
                {
                    await (task = stream.ReadAsync(buffer, 0, 512));
                    if (task.Result == 0)
                        break;
                    writer.Write(buffer, 0, task.Result);
                    received += task.Result;
                    label6.Text = Unit.AutoScale(received, "B") + " out of " + Unit.AutoScale(length, "B");
                    progressBar1.Value = (int)(received * 100 / length);
                }
            }
            catch { }
            if (stream != null)
                stream.Close();
            if (writer != null)
                writer.Close();
            int lastCrc32 = 0;
            if (File.Exists(fileName))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open));
                    int size;
                    while ((size = reader.Read(buffer, 0, 512)) > 0)
                        lastCrc32 = Ntdll.RtlComputeCrc32(lastCrc32, buffer, (uint)size);
                    reader.Close();
                }
                catch { }
            }
            Close();
            if (lastCrc32 == crc32)
                callback(true, fileName);
            else
            {
                System.Windows.Forms.MessageBox.Show("Download was corrupted, interrupted or already started !", "Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                callback(false, fileName);   
            }
                
        }

        private void Download_FormClosing(object sender, FormClosingEventArgs e)
        {
            downloadEnabled = false;
            timer.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
