using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinLib.Forms
{
    public partial class LoadingForm : Form
    {
        private bool stop = false;
        
        private LoadingForm(string status, bool topMost = true)
        {
            InitializeComponent();
            label1.Text = status;
            TopMost = topMost;
            Show();
        }

        /// <summary>
        /// Starts a splash loading form on a new thread running a separate message pump
        /// </summary>
        public static LoadingForm Create(string status, bool topMost = true)
        {
            LoadingForm loadingForm = null;
            new Thread(new ThreadStart(() => {
                loadingForm = new LoadingForm(status, topMost);
                while (!loadingForm.stop)
                    Application.DoEvents();
                loadingForm.Close();
            })).Start();
            while (loadingForm == null || !loadingForm.IsHandleCreated)
                Application.DoEvents(); // usually splashForms are triggered by UI threads; if not, no harm done
            return loadingForm;
        }
        
        public void UpdateStatus(string status)
        {
            Invoke(new Action(() => { label1.Text = status; }));
        }

        public void UpdateProgress(int percent)
        {
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
            stop = true;
        }
    }
}
