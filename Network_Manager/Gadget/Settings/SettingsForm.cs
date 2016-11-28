using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinLib.Network;

namespace Network_Manager.Gadget.Settings
{
    public partial class SettingsForm : Form
    {
        public static Form Instance = null;
        private Global.BusyForm busyForm = new Global.BusyForm("Settings");

        public SettingsForm()
        {
            if (Instance != null)
            {
                Instance.WindowState = FormWindowState.Normal;
                Instance.Activate();
                return;
            }
            Instance = this;
            Global.BusyForms.Enqueue(busyForm);
            InitializeComponent();
            autoStartup.Checked = Global.AutoStartup;
            checkForUpdates.Checked = Global.Config.Gadget.CheckForUpdates;
            alwaysOnTop.Checked = Global.Config.Gadget.AlwaysOnTop;
            autoDetectInterfaces.Checked = Global.Config.Gadget.AutoDetectInterfaces;
            graphTimeSpan.Text = Global.Config.Gadget.GraphTimeSpan.ToString();
            maxVerticalSlots.SelectedIndex = Global.Config.Gadget.MaxVerticalSlots-1;
            maxHorizontalSlots.SelectedIndex = Global.Config.Gadget.MaxHorizontalSlots-1;
            maxTotalSlots.Text = (Global.Config.Gadget.MaxHorizontalSlots * Global.Config.Gadget.MaxVerticalSlots).ToString();
            CheckBox checkBox;
            foreach (NetworkInterface nic in Global.NetworkInterfaces.Values)
            {
                checkBox = new CheckBox();
                checkBox.Name = nic.Guid;
                checkBox.Text = nic.Name;
                checkBox.Width = flowLayoutPanel1.Width;
                if (!Global.Config.Gadget.HiddenInterfaces.Contains(nic.Guid))
                    checkBox.Checked = true;
                flowLayoutPanel1.Controls.Add(checkBox);
            }
            Show();
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

        private void apply_Click(object sender, EventArgs e)
        {
            //bool refreshRequired = false;
            Global.AutoStartup = autoStartup.Checked;
            Global.Config.Gadget.CheckForUpdates = checkForUpdates.Checked;
            Global.Config.Gadget.AlwaysOnTop = alwaysOnTop.Checked;
            Global.Config.Gadget.AutoDetectInterfaces = autoDetectInterfaces.Checked;
            Global.Config.Gadget.GraphTimeSpan = (graphTimeSpan.SelectedIndex + 1) * 20;
            Global.Config.Gadget.MaxVerticalSlots = maxVerticalSlots.SelectedIndex+1;
            Global.Config.Gadget.MaxHorizontalSlots = maxHorizontalSlots.SelectedIndex+1;
            Global.Config.Gadget.HiddenInterfaces.Clear();
            foreach(CheckBox checkBox in flowLayoutPanel1.Controls)
            {
                if (!checkBox.Checked)
                    Global.Config.Gadget.HiddenInterfaces.Add(checkBox.Name);
            }
            Global.Save();
            GadgetForm.AutoRefreshAllowed = false;
            Program.Refresh();
        }

        private void maxVerticalSlots_SelectedIndexChanged(object sender, EventArgs e)
        {
            maxTotalSlots.Text = ((maxVerticalSlots.SelectedIndex+1) * (maxHorizontalSlots.SelectedIndex+1)).ToString();
        }

        private void maxHorizontalSlots_SelectedIndexChanged(object sender, EventArgs e)
        {
            maxTotalSlots.Text = ((maxVerticalSlots.SelectedIndex+1) * (maxHorizontalSlots.SelectedIndex+1)).ToString();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            busyForm.Done.SetResult(true);
        }
    }
}
