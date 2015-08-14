using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_Manager.Splash
{
    public partial class StartupForm : Form
    {
        public StartupForm()
        {
            InitializeComponent();
            Rectangle workingArea = Screen.GetWorkingArea(Global.Config.Gadget.Location);
            Location = new Point(workingArea.Left + workingArea.Width / 2 - Width / 2, workingArea.Top + workingArea.Height / 2 - Height / 2);
            Thread thread = new Thread(new ThreadStart(Start));
            thread.Start();

        }

        public void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(this);
        }

        public void UpdateStatus(string newStatus)
        {
            while (!IsHandleCreated)
                Thread.Sleep(100);
            Invoke(new Action(() => { status.Text = newStatus; }));
        }

        public void Stop()
        {
            while (!IsHandleCreated)
                Thread.Sleep(100);
            Invoke(new Action(() => { Close(); }));
        }
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
        //        return cp;
        //    }
        //} 
    }
}
