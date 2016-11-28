using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinLib.Forms
{
    public partial class LoadingForm : Form
    {
        /// <summary>
        /// Starts a splash loading form on a new thread running a separate message pump
        /// </summary>
        public LoadingForm(string status)
        {
            InitializeComponent();
            label1.Text = status;
            Thread thread = new Thread(new ThreadStart(Start));
            thread.Start();
        }

        public void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(this);
        }

        public void UpdateStatus(string status)
        {
            while (!IsHandleCreated)
                Thread.Sleep(100);
            Invoke(new Action(() => { label1.Text = status; }));
        }

        public void UpdateProgress(int percent)
        {
            while (!IsHandleCreated)
                Thread.Sleep(100);
            if (percent < 0)
                Invoke(new Action(() => {
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.Value = 33;
                }));
            else
                Invoke(new Action(() => {
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = percent; }));
        }

        public void Stop()
        {
            while (!IsHandleCreated)
                Thread.Sleep(100);
            Invoke(new Action(() => { Close(); }));
        }
    }
}
