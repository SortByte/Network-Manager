using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WinLib.Network
{
    public static class Unit
    {
        //binary multiples - JEDEC
        public static string AutoScale(double amount, string unit)
        {
            if (amount >= 107374182400)
                return Math.Round(amount / 1024 / 1024 / 1024, 0).ToString() + " G" + unit;
            else if (amount >= 10737418240)
                return Math.Round(amount / 1024 / 1024 / 1024, 1).ToString() + " G" + unit;
            else if (amount >= 1073741824)
                return Math.Round(amount / 1024 / 1024 / 1024, 2).ToString() + " G" + unit;
            else if (amount >= 104857600)
                return Math.Round(amount / 1024 / 1024, 0).ToString() + " M" + unit;
            else if (amount >= 10485760)
                return Math.Round(amount / 1024 / 1024, 1).ToString() + " M" + unit;
            else if (amount >= 1048576)
                return Math.Round(amount / 1024 / 1024, 2).ToString() + " M" + unit;
            else if (amount >= 102400)
                return Math.Round(amount / 1024, 0).ToString() + " K" + unit;
            else if (amount >= 10240)
                return Math.Round(amount / 1024, 1).ToString() + " K" + unit;
            else if (amount >= 1024)
                return Math.Round(amount / 1024, 2).ToString() + " K" + unit;
            else //if (amount < 1024)
                return Math.Round(amount, 0).ToString() + " " + unit;
        }

        public static int Compare(string x, string y)
        {
            if (!Regex.IsMatch(x, @"^([\d\.]+)\s(.).*$") &&
                !Regex.IsMatch(y, @"^([\d\.]+)\s(.).*$"))
                return 0;
            if (!Regex.IsMatch(x, @"^([\d\.]+)\s(.).*$"))
                return -1;
            if (!Regex.IsMatch(y, @"^([\d\.]+)\s(.).*$"))
                return 1;
            
            double xValue = double.Parse(Regex.Replace(x, @"^([\d\.]+)\s(.).*$", "$1"));
            string xMultiplier = Regex.Replace(x, @"^([\d\.]+)\s(.).*$", "$2");
            double yValue = double.Parse(Regex.Replace(y, @"^([\d\.]+)\s(.).*$", "$1"));
            string yMultiplier = Regex.Replace(y, @"^([\d\.]+)\s(.).*$", "$2");
            int xMultiplierValue;
            int yMultiplierValue;
            if (xMultiplier == "G")
                xMultiplierValue = 1024 * 1024 * 1024;
            else if (xMultiplier == "M")
                xMultiplierValue = 1024 * 1024;
            else if (xMultiplier == "K")
                xMultiplierValue = 1024;
            else
                xMultiplierValue = 1;
            if (yMultiplier == "G")
                yMultiplierValue = 1024 * 1024 * 1024;
            else if (yMultiplier == "M")
                yMultiplierValue = 1024 * 1024;
            else if (yMultiplier == "K")
                yMultiplierValue = 1024;
            else
                yMultiplierValue = 1;
            double xBaseValue = xValue * xMultiplierValue;
            double yBaseValue = yValue * yMultiplierValue;
            return (int)((xBaseValue - yBaseValue) / Math.Abs(xBaseValue - yBaseValue));
        }
    }
}
