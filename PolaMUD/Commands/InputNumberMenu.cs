using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Overrides normal command parsing to display a menu allowing the user to input a number.
    /// </summary>
    public class InputNumberMenu : CommandMenu
    {
        public double MinValue = -99999999;
        public double MaxValue = 99999999;

        public InputNumberMenu(object caller, string callbackMethod)
            : base(caller, callbackMethod)
        {
        }

    }
}
