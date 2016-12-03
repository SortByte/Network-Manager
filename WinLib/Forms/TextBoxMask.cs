using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinLib.Forms
{
    public class TextBoxMask
    {
        public enum Mask
	    {
	        Numeric,
            IPv4Address,
            IPv6Address
	    }
        private string oldText = "";
        private int oldPosition = 0;
        
        public TextBoxMask(TextBox control, Mask mask)
        {
            switch(mask)
            {
                case Mask.Numeric:
                    control.TextChanged += Numeric;
                    break;
                case Mask.IPv4Address:
                    control.TextChanged += IPv4Address;
                    break;
                case Mask.IPv6Address:
                    control.TextChanged += IPv6Address;
                    break;
            }
            control.KeyPress += new KeyPressEventHandler((s, e) => { oldPosition = control.SelectionStart; });
            control.MouseClick += new MouseEventHandler((s, e) => { oldPosition = control.SelectionStart; });
            UpdatePosition(control);
        }

        void Reverse(TextBox control)
        {
            int oldPos = oldPosition;
            control.Text = oldText;
            control.SelectionStart = oldPos;
            oldPosition = oldPos;
        }

        void UpdatePosition(TextBox control)
        {
            oldText = control.Text;
            oldPosition = control.SelectionStart;
        }

        public void Numeric(object sender, EventArgs e)
        {
            TextBox control = (TextBox)sender;
            if (!Regex.IsMatch(control.Text, @"^[\d]*$"))
                Reverse(control);
            else
                UpdatePosition(control);
        }

        public void IPv4Address(object sender, EventArgs e)
        {
            TextBox control = (TextBox)sender;
            int b1, b2, b3, b4;
            Int32.TryParse(Regex.Replace(control.Text, @"^([0-9]{0,3}\.){0}([0-9]{0,3}).*|.*", "$2"), out b1);
            Int32.TryParse(Regex.Replace(control.Text, @"^([0-9]{0,3}\.){1}([0-9]{0,3}).*|.*", "$2"), out b2);
            Int32.TryParse(Regex.Replace(control.Text, @"^([0-9]{0,3}\.){2}([0-9]{0,3}).*|.*", "$2"), out b3);
            Int32.TryParse(Regex.Replace(control.Text, @"^([0-9]{0,3}\.){3}([0-9]{0,3}).*|.*", "$2"), out b4);
            if (Regex.IsMatch(control.Text, @"^[0-9]{0,3}$|^[0-9]{0,3}\.[0-9]{0,3}$|^[0-9]{0,3}\.[0-9]{0,3}\.[0-9]{0,3}$|^[0-9]{0,3}\.[0-9]{0,3}\.[0-9]{0,3}\.[0-9]{0,3}$") &&
                b1 <= 255 &&
                b2 <= 255 &&
                b3 <= 255 &&
                b4 <= 255)
                UpdatePosition(control);
            else
                Reverse(control);
        }

        public void IPv6Address(object sender, EventArgs e)
        {
            TextBox control = (TextBox)sender;

            if (Regex.IsMatch(control.Text, @""))
                UpdatePosition(control);
            else
                Reverse(control);
        }
    }
}
