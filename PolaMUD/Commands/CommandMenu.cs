using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public abstract class CommandMenu
    {
        public object Caller;
        public string CallbackMethod;
        public string DisplayMessage = "";

        public CommandMenu(object caller, string callbackMethod)
        {
            Caller = caller;
            CallbackMethod = callbackMethod;
        }

        public CommandMenu(object caller, string callbackMethod, string displayMessage)
        {
            Caller = caller;
            CallbackMethod = callbackMethod;
            DisplayMessage = displayMessage;

            if (caller is Player)
            {
                ((Player)caller).SendMessage(displayMessage);
            }
        }
    }
}
