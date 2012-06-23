using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Overrides normal command parsing to display a menu allowing the user to input a string.
    /// </summary>
    public class InputStringMenu : CommandMenu
    {
        public int Length = 4096;

        public InputStringMenu(object caller, string callbackMethod)
            : base(caller, callbackMethod)
        {
        }

    }
}
