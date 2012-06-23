using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Overrides normal command parsing to display a menu allowing the user to select a predefined menu option.
    /// </summary>
    public class DynamicMenu : CommandMenu
    {
        public Dictionary<string, Command> List = new Dictionary<string, Command>();

        public DynamicMenu(object caller, string callbackMethod)
            : base(caller, callbackMethod)
        {
        }

        public DynamicMenu(object caller, string callbackMethod, string displayMessage)
            : base(caller, callbackMethod, displayMessage)
        {
        }
    }
}
