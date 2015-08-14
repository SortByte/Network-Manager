using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Network_Manager.Gadget.About
{
    public partial class LicenseForm : Form
    {
        public LicenseForm()
        {
            InitializeComponent();
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
        }
    }
}
