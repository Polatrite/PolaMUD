using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class DynamicMenu : CommandMenu
    {
        public Dictionary<string, Command> List = new Dictionary<string, Command>();
        public object Caller;
        public string CallbackMethod;

        public DynamicMenu(object caller, string callbackMethod)
        {
            Caller = caller;
            CallbackMethod = callbackMethod;
        }
    }
}
