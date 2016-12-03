using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinLib.Sync
{
    public class TextEventArgs : EventArgs
    {
        public string Text;

        public TextEventArgs(string text)
        {
            Text = text;
        }
    }
}
