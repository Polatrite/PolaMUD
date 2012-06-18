using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public abstract class CommandMenu
    {
        public Dictionary<string, Command> List = new Dictionary<string, Command>();
    }
}
